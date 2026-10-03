namespace CS2Analyzer.Application.DTOs;

public class PlayerSummaryDto
{
    public string SteamId { get; set; } = string.Empty;
    public double PlayTimeHours { get; set; }

    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public double KillDeathRatio { get; set; }
    public double HeadshotPercentage { get; set; }

    public int TotalMatchesPlayed { get; set; }
    public int TotalMatchesWon { get; set; }
    public double WinRatePercentage { get; set; }

    public Dictionary<string, int> TopWeaponsByKills { get; set; } = new();

    public LastMatchSummary? LastMatch { get; set; }
}

public class LastMatchSummary
{
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Mvps { get; set; }
    public int Damage { get; set; }
}