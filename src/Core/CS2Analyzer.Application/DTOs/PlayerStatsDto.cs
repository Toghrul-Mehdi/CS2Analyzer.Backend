namespace CS2Analyzer.Application.DTOs;

/// <summary>Steam "GetUserStatsForGame" cavabından hesablanmış, bölmələrə ayrılmış statistika.</summary>
public sealed record PlayerStatsDto(
    string SteamId,
    string GameName,
    OverviewStatsDto Overview,
    CombatStatsDto Combat,
    IReadOnlyList<WeaponStatsDto> Weapons,
    IReadOnlyList<WeaponCategoryStatsDto> WeaponCategories,
    IReadOnlyList<MapStatsDto> Maps,
    ObjectiveStatsDto Objectives,
    EconomyStatsDto Economy,
    LastMatchStatsDto? LastMatch,
    GunGameStatsDto GunGame,
    AchievementStatsDto Achievements);

public sealed record OverviewStatsDto(
    long PlayTimeSeconds,
    double PlayTimeHours,
    long Kills,
    long Deaths,
    double KillDeathRatio,
    long HeadshotKills,
    double HeadshotPercentage,
    long Mvps,
    long MatchesPlayed,
    long MatchesWon,
    double MatchWinRate,
    long RoundsPlayed,
    long RoundsWon,
    double RoundWinRate,
    long DamageDone,
    double AverageDamagePerRound,
    double KillsPerRound,
    long ContributionScore);

/// <summary>
/// Steam-in ümumi "total_shots_hit" sayğacı CS2-də düzgün işləmir (atəş sayından min dəfələrlə azdır),
/// ona görə dəqiqlik silahlar üzrə isabət/atəş cəmindən hesablanır.
/// </summary>
public sealed record CombatStatsDto(
    long ShotsFired,
    long TrackedShots,
    long TrackedHits,
    double Accuracy,
    long KnifeKills,
    long GrenadeKills,
    long ZeusKills,
    long EnemyWeaponKills,
    long BlindedEnemyKills,
    double EnemyWeaponKillShare,
    double BlindedEnemyKillShare);

public enum WeaponCategory
{
    Pistol,
    Smg,
    Rifle,
    Sniper,
    Shotgun,
    Heavy,
    Melee,
    Grenade,
    Equipment,
    Other,
}

/// <summary>Shots/Hits/Accuracy bıçaq və qumbara kimi atəş sayılmayan silahlarda null olur.</summary>
public sealed record WeaponStatsDto(
    string Key,
    string Name,
    WeaponCategory Category,
    long Kills,
    long? Shots,
    long? Hits,
    double? Accuracy,
    double KillShare,
    double? ShotsPerKill);

public sealed record WeaponCategoryStatsDto(
    WeaponCategory Category,
    long Kills,
    long Shots,
    long Hits,
    double? Accuracy,
    double KillShare);

public enum MapMode
{
    Defusal,
    Hostage,
    ArmsRace,
    Other,
}

/// <summary>Steam xəritələr üzrə yalnız raund sayı verir, ona görə qələbə faizi raund əsasındadır.</summary>
public sealed record MapStatsDto(
    string Key,
    string Name,
    MapMode Mode,
    long RoundsPlayed,
    long RoundsWon,
    long RoundsLost,
    double RoundWinRate,
    double RoundShare,
    long? MatchesWon);

public sealed record ObjectiveStatsDto(
    long BombsPlanted,
    long BombsDefused,
    long HostagesRescued,
    long PistolRoundsWon,
    double PlantRate,
    double DefuseRate);

public sealed record EconomyStatsDto(
    long MoneyEarned,
    double MoneyPerRound,
    double MoneyPerMatch,
    long WeaponsDonated);

public sealed record LastMatchStatsDto(
    long Rounds,
    long RoundsWon,
    long RoundsLost,
    long TRoundWins,
    long CtRoundWins,
    long Kills,
    long Deaths,
    double KillDeathRatio,
    long Mvps,
    long Damage,
    double AverageDamagePerRound,
    long ContributionScore,
    long MoneySpent,
    long MaxPlayers,
    FavoriteWeaponDto? FavoriteWeapon);

public sealed record FavoriteWeaponDto(
    int DefinitionIndex,
    string Key,
    string Name,
    long Shots,
    long Hits,
    long Kills,
    double Accuracy);

public sealed record GunGameStatsDto(
    long RoundsPlayed,
    long RoundsWon,
    double RoundWinRate,
    long MatchesPlayed);

public sealed record AchievementStatsDto(int Unlocked);
