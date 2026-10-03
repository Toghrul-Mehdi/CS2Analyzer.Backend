using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using CS2Analyzer.Application.Interfaces;
using CS2Analyzer.Application.DTOs;
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

    public async Task<PlayerSummaryDto> GetUserStatsAsync(string steamId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<SteamUserStatsDto>(
            $"ISteamUserStats/GetUserStatsForGame/v2/?key={_apiKey}&steamid={steamId}&appid={AppId}",
            cancellationToken);

        if (response?.PlayerStats == null)
            throw new Exception("Stats not found or profile is private.");

        var statsList = response.PlayerStats.Stats;
        var statDict = statsList.ToDictionary(x => x.Name, x => x.Value);
        int GetStat(string key) => statDict.GetValueOrDefault(key, 0);

        var totalKills = GetStat("total_kills");
        var totalDeaths = GetStat("total_deaths");
        var headshotKills = GetStat("total_kills_headshot");
        var matchesPlayed = GetStat("total_matches_played");
        var matchesWon = GetStat("total_matches_won");

        var weaponKills = statsList
            .Where(s => s.Name.StartsWith("total_kills_") &&
                        !s.Name.Contains("headshot") &&
                        !s.Name.Contains("enemy"))
            .OrderByDescending(s => s.Value)
            .Take(3)
            .ToDictionary(
                s => s.Name.Replace("total_kills_", "").ToUpper(),
                s => s.Value
            );

        return new PlayerSummaryDto
        {
            SteamId = response.PlayerStats.SteamId,
            PlayTimeHours = Math.Round(GetStat("total_time_played") / 3600.0, 1),
            TotalKills = totalKills,
            TotalDeaths = totalDeaths,
            KillDeathRatio = totalDeaths > 0 ? Math.Round((double)totalKills / totalDeaths, 2) : totalKills,
            HeadshotPercentage = totalKills > 0 ? Math.Round(((double)headshotKills / totalKills) * 100, 1) : 0,
            TotalMatchesPlayed = matchesPlayed,
            TotalMatchesWon = matchesWon,
            WinRatePercentage = matchesPlayed > 0 ? Math.Round(((double)matchesWon / matchesPlayed) * 100, 1) : 0,
            TopWeaponsByKills = weaponKills,
            LastMatch = new LastMatchSummary
            {
                Kills = GetStat("last_match_kills"),
                Deaths = GetStat("last_match_deaths"),
                Mvps = GetStat("last_match_mvps"),
                Damage = GetStat("last_match_damage")
            }
        };
    }
}