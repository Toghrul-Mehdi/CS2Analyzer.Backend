using System.Text.RegularExpressions;
using CS2Analyzer.Application.Interfaces;

namespace CS2Analyzer.Infrastructure.Services;

public sealed partial class SteamOpenIdVerifier(HttpClient httpClient) : ISteamAuthService
{
    private const string SteamOpenIdEndpoint = "https://steamcommunity.com/openid/login";
    private const string OpenIdPrefix = "openid.";

    [GeneratedRegex(@"^https://steamcommunity\.com/openid/id/(\d{17})$")]
    private static partial Regex ClaimedIdPattern();

    public async Task<string?> VerifyLoginAsync(
        IReadOnlyDictionary<string, string> callbackParameters,
        string expectedReturnTo,
        CancellationToken cancellationToken)
    {
        if (!HasValue(callbackParameters, "openid.mode", "id_res") ||
            !HasValue(callbackParameters, "openid.op_endpoint", SteamOpenIdEndpoint) ||
            !HasValue(callbackParameters, "openid.return_to", expectedReturnTo))
        {
            return null;
        }

        if (!callbackParameters.TryGetValue("openid.claimed_id", out string? claimedId))
        {
            return null;
        }

        Match match = ClaimedIdPattern().Match(claimedId);
        if (!match.Success)
        {
            return null;
        }

        // Cavabı Steam-ə geri göndərib həqiqətən onun yaratdığını təsdiqləyirik.
        Dictionary<string, string> form = callbackParameters
            .Where(pair => pair.Key.StartsWith(OpenIdPrefix, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        form["openid.mode"] = "check_authentication";

        using HttpResponseMessage response = await httpClient.PostAsync(
            "openid/login", new FormUrlEncodedContent(form), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        bool isValid = body
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains("is_valid:true");

        return isValid ? match.Groups[1].Value : null;
    }

    private static bool HasValue(IReadOnlyDictionary<string, string> parameters, string key, string expected) =>
        parameters.TryGetValue(key, out string? actual) && string.Equals(actual, expected, StringComparison.Ordinal);
}