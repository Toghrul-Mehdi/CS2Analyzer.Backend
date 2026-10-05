using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CS2Analyzer.Application.Interfaces;
using CS2Analyzer.Infrastructure.Options;
using CS2Analyzer.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string FrontendCorsPolicy = "Frontend";
const string SteamProfileClientName = "SteamProfile";

var builder = WebApplication.CreateBuilder(args);

// Enum-lar (silah kateqoriyası, xəritə rejimi) JSON-da "rifle", "armsRace" kimi yazılsın
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: yalnız appsettings-də göstərilən frontend ünvanlarına icazə verilir.
string[] allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// JWT: token yaratma və yoxlama eyni konfiqurasiyadan istifadə edir.
JwtOptions jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("'Jwt' bölməsi konfiqurasiyada tapılmadı.");

if (jwtOptions.Secret.Length < JwtOptions.MinimumSecretLength)
{
    throw new InvalidOperationException(
        $"Jwt:Secret ən azı {JwtOptions.MinimumSecretLength} simvol olmalıdır (user-secrets ilə təyin et).");
}

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // "sub", "name", "picture" adları dəyişməsin
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

// Steam API Service qeydiyyatı
builder.Services.AddHttpClient<ISteamService, SteamApiService>(client =>
{
    var baseUrl = builder.Configuration["SteamAPI:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

// Steam OpenID yoxlaması (steamcommunity.com-a sorğu göndərir)
builder.Services.AddHttpClient<ISteamAuthService, SteamOpenIdVerifier>(client =>
{
    client.BaseAddress = new Uri("https://steamcommunity.com/");
});

// Steam profil məlumatı (ad, avatar). Açar: SteamAPI:ApiKey (user-secrets ilə saxla)
string steamApiKey = builder.Configuration["SteamAPI:Key"] ?? string.Empty;

builder.Services.AddHttpClient(SteamProfileClientName, client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SteamAPI:BaseUrl"]!);
});
builder.Services.AddTransient<ISteamProfileService>(serviceProvider =>
    new SteamProfileService(
        serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(SteamProfileClientName),
        steamApiKey));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();   // UseAuthorization-dan əvvəl olmalıdır
app.UseAuthorization();
app.MapControllers();

app.Run();