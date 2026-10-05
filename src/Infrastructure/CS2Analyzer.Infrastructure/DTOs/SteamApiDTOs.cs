using System.Text.Json.Serialization;

namespace CS2Analyzer.Infrastructure.DTOs;

internal class SteamPlayerCountDto
{
    [JsonPropertyName("response")]
    public PlayerCountData? Response { get; set; }
}

internal class PlayerCountData
{
    [JsonPropertyName("player_count")]
    public int PlayerCount { get; set; }
}

internal class SteamUserStatsDto
{
    [JsonPropertyName("playerstats")]
    public PlayerStatsData? PlayerStats { get; set; }
}

internal class PlayerStatsData
{
    [JsonPropertyName("steamID")]
    public string SteamId { get; set; } = string.Empty;

    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = string.Empty;

    [JsonPropertyName("stats")]
    public List<SteamStatItem> Stats { get; set; } = new();

    [JsonPropertyName("achievements")]
    public List<SteamAchievementItem> Achievements { get; set; } = new();
}

internal class SteamStatItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public long Value { get; set; }
}

internal class SteamAchievementItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("achieved")]
    public int Achieved { get; set; }
}