using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using CS2Analyzer.Application.Interfaces;
using CS2Analyzer.Application.DTOs;
using CS2Analyzer.Application.Stats;
using CS2Analyzer.Infrastructure.DTOs;

namespace CS2Analyzer.Infrastructure.Services;

public class SteamApiService : ISteamService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string AppId = "730"; // CS2

    public SteamApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["SteamAPI:Key"]
                  ?? throw new ArgumentNullException("Steam API Key not found");
    }

    public async Task<CurrentPlayerStatDto> GetCurrentPlayersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<SteamPlayerCountDto>(
            $"ISteamUserStats/GetNumberOfCurrentPlayers/v1/?appid={AppId}",
            cancellationToken);

        return new CurrentPlayerStatDto
        {
            PlayerCount = response?.Response?.PlayerCount ?? 0
        };
    }

    public async Task<PlayerStatsDto> GetUserStatsAsync(string steamId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<SteamUserStatsDto>(
            $"ISteamUserStats/GetUserStatsForGame/v2/?key={_apiKey}&steamid={Uri.EscapeDataString(steamId)}&appid={AppId}",
            cancellationToken);

        if (response?.PlayerStats == null)
            throw new Exception("Stats not found or profile is private.");

        // Eyni ad iki dəfə gəlsə ToDictionary partlamasın, sonuncu dəyər götürülür
        var statDict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var stat in response.PlayerStats.Stats)
            statDict[stat.Name] = stat.Value;

        var achievementsUnlocked = response.PlayerStats.Achievements.Count(a => a.Achieved == 1);

        return PlayerStatsAggregator.Build(
            response.PlayerStats.SteamId,
            response.PlayerStats.GameName,
            statDict,
            achievementsUnlocked);
    }
}