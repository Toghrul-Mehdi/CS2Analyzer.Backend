using CS2Analyzer.Application.DTOs;

namespace CS2Analyzer.Application.Stats;

/// <summary>
/// Steam-in düz "ad -> dəyər" siyahısını bölmələrə ayrılmış <see cref="PlayerStatsDto"/>-ya çevirir.
/// Silah və xəritə açarları siyahıdan avtomatik tapılır, ona görə Steam yeni silah/xəritə əlavə etsə də görünəcək.
/// </summary>
public static class PlayerStatsAggregator
{
    private const string KillsPrefix = "total_kills_";
    private const string ShotsPrefix = "total_shots_";
    private const string HitsPrefix = "total_hits_";
    private const string MapRoundsPrefix = "total_rounds_map_";
    private const string MapWinsPrefix = "total_wins_map_";
    private const string MapMatchesWonPrefix = "total_matches_won_";

    public static PlayerStatsDto Build(
        string steamId,
        string gameName,
        IReadOnlyDictionary<string, long> stats,
        int achievementsUnlocked)
    {
        long Get(string key) => stats.GetValueOrDefault(key);

        long kills = Get("total_kills");
        long deaths = Get("total_deaths");
        long roundsPlayed = Get("total_rounds_played");
        long matchesPlayed = Get("total_matches_played");
        long headshotKills = Get("total_kills_headshot");
        long damage = Get("total_damage_done");
        long timePlayed = Get("total_time_played");

        List<WeaponStatsDto> weapons = BuildWeapons(stats, kills);
        long trackedShots = weapons.Sum(w => w.Shots ?? 0);
        long trackedHits = weapons.Sum(w => w.Hits ?? 0);

        long KillsWith(string weaponKey) => Get(KillsPrefix + weaponKey);

        var overview = new OverviewStatsDto(
            PlayTimeSeconds: timePlayed,
            PlayTimeHours: Math.Round(timePlayed / 3600.0, 1),
            Kills: kills,
            Deaths: deaths,
            KillDeathRatio: KillDeath(kills, deaths),
            HeadshotKills: headshotKills,
            HeadshotPercentage: Percent(headshotKills, kills),
            Mvps: Get("total_mvps"),
            MatchesPlayed: matchesPlayed,
            MatchesWon: Get("total_matches_won"),
            MatchWinRate: Percent(Get("total_matches_won"), matchesPlayed),
            RoundsPlayed: roundsPlayed,
            RoundsWon: Get("total_wins"),
            RoundWinRate: Percent(Get("total_wins"), roundsPlayed),
            DamageDone: damage,
            AverageDamagePerRound: Ratio(damage, roundsPlayed, 1),
            KillsPerRound: Ratio(kills, roundsPlayed, 2),
            ContributionScore: Get("total_contribution_score"));

        var combat = new CombatStatsDto(
            ShotsFired: Get("total_shots_fired"),
            TrackedShots: trackedShots,
            TrackedHits: trackedHits,
            Accuracy: Percent(trackedHits, trackedShots),
            KnifeKills: KillsWith("knife"),
            GrenadeKills: KillsWith("hegrenade") + KillsWith("molotov") + KillsWith("decoy"),
            ZeusKills: KillsWith("taser"),
            EnemyWeaponKills: Get("total_kills_enemy_weapon"),
            BlindedEnemyKills: Get("total_kills_enemy_blinded"),
            EnemyWeaponKillShare: Percent(Get("total_kills_enemy_weapon"), kills),
            BlindedEnemyKillShare: Percent(Get("total_kills_enemy_blinded"), kills));

        var objectives = new ObjectiveStatsDto(
            BombsPlanted: Get("total_planted_bombs"),
            BombsDefused: Get("total_defused_bombs"),
            HostagesRescued: Get("total_rescued_hostages"),
            PistolRoundsWon: Get("total_wins_pistolround"),
            PlantRate: Percent(Get("total_planted_bombs"), roundsPlayed),
            DefuseRate: Percent(Get("total_defused_bombs"), roundsPlayed));

        long moneyEarned = Get("total_money_earned");
        var economy = new EconomyStatsDto(
            MoneyEarned: moneyEarned,
            MoneyPerRound: Ratio(moneyEarned, roundsPlayed, 0),
            MoneyPerMatch: Ratio(moneyEarned, matchesPlayed, 0),
            WeaponsDonated: Get("total_weapons_donated"));

        long gunGameRounds = Get("total_gun_game_rounds_played");
        var gunGame = new GunGameStatsDto(
            RoundsPlayed: gunGameRounds,
            RoundsWon: Get("total_gun_game_rounds_won"),
            RoundWinRate: Percent(Get("total_gun_game_rounds_won"), gunGameRounds),
            MatchesPlayed: Get("total_gg_matches_played"));

        return new PlayerStatsDto(
            SteamId: steamId,
            GameName: gameName,
            Overview: overview,
            Combat: combat,
            Weapons: weapons,
            WeaponCategories: BuildCategories(weapons, kills),
            Maps: BuildMaps(stats),
            Objectives: objectives,
            Economy: economy,
            LastMatch: BuildLastMatch(stats),
            GunGame: gunGame,
            Achievements: new AchievementStatsDto(achievementsUnlocked));
    }

    private static List<WeaponStatsDto> BuildWeapons(IReadOnlyDictionary<string, long> stats, long totalKills)
    {
        IEnumerable<string> keys = SuffixesOf(stats, KillsPrefix)
            .Concat(SuffixesOf(stats, ShotsPrefix))
            .Concat(SuffixesOf(stats, HitsPrefix))
            .Where(WeaponCatalog.IsWeapon)
            // "total_shots_fired" / "total_shots_hit" ümumi sayğaclardır, silah deyil
            .Where(key => key is not ("fired" or "hit"))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var weapons = new List<WeaponStatsDto>();
        foreach (string key in keys)
        {
            long kills = stats.GetValueOrDefault(KillsPrefix + key);
            long? shots = stats.TryGetValue(ShotsPrefix + key, out long s) ? s : null;
            long? hits = stats.TryGetValue(HitsPrefix + key, out long h) ? h : null;

            if (kills == 0 && (shots ?? 0) == 0)
            {
                continue;
            }

            WeaponInfo info = WeaponCatalog.Describe(key);
            bool hasShots = shots > 0;

            weapons.Add(new WeaponStatsDto(
                Key: key.ToLowerInvariant(),
                Name: info.Name,
                Category: info.Category,
                Kills: kills,
                Shots: shots,
                Hits: hits,
                Accuracy: hasShots ? Percent(hits ?? 0, shots!.Value) : null,
                KillShare: Percent(kills, totalKills),
                ShotsPerKill: hasShots && kills > 0 ? Ratio(shots!.Value, kills, 1) : null));
        }

        return weapons
            .OrderByDescending(w => w.Kills)
            .ThenByDescending(w => w.Shots ?? 0)
            .ToList();
    }

    private static List<WeaponCategoryStatsDto> BuildCategories(List<WeaponStatsDto> weapons, long totalKills) =>
        weapons
            .GroupBy(w => w.Category)
            .Select(group =>
            {
                long shots = group.Sum(w => w.Shots ?? 0);
                long hits = group.Sum(w => w.Hits ?? 0);
                long kills = group.Sum(w => w.Kills);
                return new WeaponCategoryStatsDto(
                    Category: group.Key,
                    Kills: kills,
                    Shots: shots,
                    Hits: hits,
                    Accuracy: shots > 0 ? Percent(hits, shots) : null,
                    KillShare: Percent(kills, totalKills));
            })
            .OrderBy(c => c.Category)
            .ToList();

    private static List<MapStatsDto> BuildMaps(IReadOnlyDictionary<string, long> stats)
    {
        List<string> keys = SuffixesOf(stats, MapRoundsPrefix)
            .Concat(SuffixesOf(stats, MapWinsPrefix))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        long totalMapRounds = keys.Sum(key => stats.GetValueOrDefault(MapRoundsPrefix + key));

        return keys
            .Select(key =>
            {
                long rounds = stats.GetValueOrDefault(MapRoundsPrefix + key);
                // Bəzi köhnə hesablarda qələbə raund sayından çox yazılıb, faiz 100-ü keçməsin
                long wins = Math.Min(stats.GetValueOrDefault(MapWinsPrefix + key), rounds);

                // Matç qələbəsi açarında prefiks yoxdur: "de_train" -> "total_matches_won_train"
                string shortName = key.Contains('_') ? key[(key.IndexOf('_') + 1)..] : key;
                long? matchesWon = stats.TryGetValue(MapMatchesWonPrefix + shortName, out long m) ? m : null;

                return new MapStatsDto(
                    Key: key.ToLowerInvariant(),
                    Name: MapCatalog.NameOf(key),
                    Mode: MapCatalog.ModeOf(key),
                    RoundsPlayed: rounds,
                    RoundsWon: wins,
                    RoundsLost: rounds - wins,
                    RoundWinRate: Percent(wins, rounds),
                    RoundShare: Percent(rounds, totalMapRounds),
                    MatchesWon: matchesWon);
            })
            .Where(map => map.RoundsPlayed > 0)
            .OrderByDescending(map => map.RoundsPlayed)
            .ToList();
    }

    private static LastMatchStatsDto? BuildLastMatch(IReadOnlyDictionary<string, long> stats)
    {
        long Get(string key) => stats.GetValueOrDefault("last_match_" + key);

        long rounds = Get("rounds");
        if (rounds == 0)
        {
            return null;
        }

        long kills = Get("kills");
        long deaths = Get("deaths");
        long damage = Get("damage");
        long roundsWon = Get("wins");

        return new LastMatchStatsDto(
            Rounds: rounds,
            RoundsWon: roundsWon,
            RoundsLost: Math.Max(rounds - roundsWon, 0),
            TRoundWins: Get("t_wins"),
            CtRoundWins: Get("ct_wins"),
            Kills: kills,
            Deaths: deaths,
            KillDeathRatio: KillDeath(kills, deaths),
            Mvps: Get("mvps"),
            Damage: damage,
            AverageDamagePerRound: Ratio(damage, rounds, 1),
            ContributionScore: Get("contribution_score"),
            MoneySpent: Get("money_spent"),
            MaxPlayers: Get("max_players"),
            FavoriteWeapon: BuildFavoriteWeapon(stats));
    }

    private static FavoriteWeaponDto? BuildFavoriteWeapon(IReadOnlyDictionary<string, long> stats)
    {
        if (!stats.TryGetValue("last_match_favweapon_id", out long id) || id <= 0)
        {
            return null;
        }

        var weapon = WeaponCatalog.FromDefinitionIndex((int)id) ?? ($"weapon_{id}", $"#{id}");
        long shots = stats.GetValueOrDefault("last_match_favweapon_shots");
        long hits = stats.GetValueOrDefault("last_match_favweapon_hits");

        return new FavoriteWeaponDto(
            DefinitionIndex: (int)id,
            Key: weapon.Key,
            Name: weapon.Name,
            Shots: shots,
            Hits: hits,
            Kills: stats.GetValueOrDefault("last_match_favweapon_kills"),
            Accuracy: Percent(hits, shots));
    }

    private static IEnumerable<string> SuffixesOf(IReadOnlyDictionary<string, long> stats, string prefix) =>
        stats.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && key.Length > prefix.Length)
            .Select(key => key[prefix.Length..]);

    /// <summary>0–100 arası faiz, bir onluq rəqəmlə. Məxrəc 0-dırsa 0.</summary>
    private static double Percent(long part, long whole) =>
        whole > 0 ? Math.Round(part * 100.0 / whole, 1) : 0;

    private static double Ratio(long numerator, long denominator, int digits) =>
        denominator > 0 ? Math.Round((double)numerator / denominator, digits) : 0;

    /// <summary>Ölüm yoxdursa K/D öldürmə sayına bərabər götürülür.</summary>
    private static double KillDeath(long kills, long deaths) =>
        deaths > 0 ? Math.Round((double)kills / deaths, 2) : kills;
}
