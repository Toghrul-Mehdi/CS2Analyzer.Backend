using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CS2Analyzer.Application.DTOs;
using CS2Analyzer.Application.Interfaces;

namespace CS2Analyzer.Infrastructure.Services;

public sealed class SteamProfileService(HttpClient httpClient, string apiKey) : ISteamProfileService
{
    public async Task<SteamProfileDto?> GetProfileAsync(string steamId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        string url = $"ISteamUser/GetPlayerSummaries/v2/?key={Uri.EscapeDataString(apiKey)}&steamids={Uri.EscapeDataString(steamId)}";

        try
        {
            PlayerSummariesResponse? payload =
                await httpClient.GetFromJsonAsync<PlayerSummariesResponse>(url, cancellationToken);

            PlayerSummary? player = payload?.Response?.Players?.FirstOrDefault();
            return player is null
                ? null
                : new SteamProfileDto(steamId, player.PersonaName ?? steamId, player.AvatarMedium);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            return null;
        }
    }

    private sealed record PlayerSummariesResponse(
        [property: JsonPropertyName("response")] PlayerSummariesBody? Response);

    private sealed record PlayerSummariesBody(
        [property: JsonPropertyName("players")] List<PlayerSummary>? Players);

    private sealed record PlayerSummary(
        [property: JsonPropertyName("personaname")] string? PersonaName,
        [property: JsonPropertyName("avatarmedium")] string? AvatarMedium);
}