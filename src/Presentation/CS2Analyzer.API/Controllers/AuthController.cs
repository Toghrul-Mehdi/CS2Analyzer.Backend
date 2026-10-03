using CS2Analyzer.Application.DTOs;
using CS2Analyzer.Application.Interfaces;
using CS2Analyzer.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace CS2Analyzer.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IConfiguration configuration,
    ISteamAuthService steamAuthService,
    ISteamProfileService steamProfileService,
    ITokenService tokenService) : ControllerBase
{
    private const string SteamOpenIdEndpoint = "https://steamcommunity.com/openid/login";
    private const string OpenIdNamespace = "http://specs.openid.net/auth/2.0";
    private const string IdentifierSelect = "http://specs.openid.net/auth/2.0/identifier_select";

    private string FrontendUrl =>
        configuration["Frontend:BaseUrl"]?.TrimEnd('/')
        ?? throw new InvalidOperationException("Frontend:BaseUrl konfiqurasiyada təyin olunmayıb.");

    // Steam istifadəçini bu backend-in callback ünvanına qaytarır.
    private string CallbackUrl => Url.Action(nameof(SteamCallback), "Auth", values: null, protocol: Request.Scheme)!;

    private string BackendOrigin => new Uri(CallbackUrl).GetLeftPart(UriPartial.Authority);

    /// <summary>İstifadəçini Steam giriş səhifəsinə yönləndirir.</summary>
    [HttpGet("steam/login")]
    public IActionResult SteamLogin()
    {
        var parameters = new Dictionary<string, string?>
        {
            ["openid.ns"] = OpenIdNamespace,
            ["openid.mode"] = "checkid_setup",
            ["openid.return_to"] = CallbackUrl,
            ["openid.realm"] = BackendOrigin,
            ["openid.identity"] = IdentifierSelect,
            ["openid.claimed_id"] = IdentifierSelect,
        };

        return Redirect(QueryHelpers.AddQueryString(SteamOpenIdEndpoint, parameters));
    }

    /// <summary>
    /// Steam-dən qayıdışı yoxlayır, JWT yaradır və frontend-ə qaytarır.
    /// Token URL-in # hissəsində göndərilir (server loglarına və Referer-ə düşmür).
    /// </summary>
    [HttpGet("steam/callback")]
    public async Task<IActionResult> SteamCallback(CancellationToken cancellationToken)
    {
        Dictionary<string, string> parameters = Request.Query.ToDictionary(
            pair => pair.Key, pair => pair.Value.ToString());

        string? steamId = await steamAuthService.VerifyLoginAsync(parameters, CallbackUrl, cancellationToken);
        if (steamId is null)
        {
            return Redirect($"{FrontendUrl}/?login=failed");
        }

        SteamProfileDto profile =
            await steamProfileService.GetProfileAsync(steamId, cancellationToken)
            ?? new SteamProfileDto(steamId, steamId, AvatarUrl: null);

        string token = tokenService.CreateAccessToken(profile);

        return Redirect($"{FrontendUrl}/#token={Uri.EscapeDataString(token)}");
    }

    /// <summary>Token etibarlıdırsa istifadəçi məlumatını qaytarır, əks halda 401.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() =>
        Ok(new CurrentUserResponse(
            User.FindFirst(JwtTokenService.SteamIdClaim)!.Value,
            User.FindFirst(JwtTokenService.NameClaim)!.Value,
            User.FindFirst(JwtTokenService.AvatarClaim)?.Value));

    public sealed record CurrentUserResponse(string SteamId, string Name, string? AvatarUrl);
}