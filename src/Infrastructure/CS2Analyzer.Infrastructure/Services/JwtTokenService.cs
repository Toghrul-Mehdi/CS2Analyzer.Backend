using System.Security.Claims;
using System.Text;
using CS2Analyzer.Application.DTOs;
using CS2Analyzer.Application.Interfaces;
using CS2Analyzer.Infrastructure.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CS2Analyzer.Infrastructure.Services;

public sealed class JwtTokenService(JwtOptions options) : ITokenService
{
    public const string SteamIdClaim = "sub";
    public const string NameClaim = "name";
    public const string AvatarClaim = "picture";

    private readonly SigningCredentials _signingCredentials = new(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)),
        SecurityAlgorithms.HmacSha256);

    public string CreateAccessToken(SteamProfileDto profile)
    {
        var claims = new List<Claim>
        {
            new(SteamIdClaim, profile.SteamId),
            new(NameClaim, profile.PersonaName),
        };
        if (profile.AvatarUrl is not null)
        {
            claims.Add(new Claim(AvatarClaim, profile.AvatarUrl));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = options.Issuer,
            Audience = options.Audience,
            Expires = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes),
            SigningCredentials = _signingCredentials,
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}