using CS2Analyzer.Application.DTOs;

namespace CS2Analyzer.Application.Stats;

public sealed record WeaponInfo(string Name, WeaponCategory Category);

/// <summary>Steam stat açarındakı silah adı (məs. "ak47") -> göstərilən ad və kateqoriya.</summary>
public static class WeaponCatalog
{
    private static readonly Dictionary<string, WeaponInfo> Weapons = new(StringComparer.OrdinalIgnoreCase)
    {
        ["glock"] = new("Glock-18", WeaponCategory.Pistol),
        ["hkp2000"] = new("P2000 / USP-S", WeaponCategory.Pistol),
        ["p250"] = new("P250", WeaponCategory.Pistol),
        ["elite"] = new("Dual Berettas", WeaponCategory.Pistol),
        ["fiveseven"] = new("Five-SeveN", WeaponCategory.Pistol),
        ["tec9"] = new("Tec-9", WeaponCategory.Pistol),
        ["deagle"] = new("Desert Eagle", WeaponCategory.Pistol),

        ["mac10"] = new("MAC-10", WeaponCategory.Smg),
        ["mp9"] = new("MP9", WeaponCategory.Smg),
        ["mp7"] = new("MP7", WeaponCategory.Smg),
        ["ump45"] = new("UMP-45", WeaponCategory.Smg),
        ["p90"] = new("P90", WeaponCategory.Smg),
        ["bizon"] = new("PP-Bizon", WeaponCategory.Smg),

        ["ak47"] = new("AK-47", WeaponCategory.Rifle),
        ["m4a1"] = new("M4A4 / M4A1-S", WeaponCategory.Rifle),
        ["galilar"] = new("Galil AR", WeaponCategory.Rifle),
        ["famas"] = new("FAMAS", WeaponCategory.Rifle),
        ["sg556"] = new("SG 553", WeaponCategory.Rifle),
        ["aug"] = new("AUG", WeaponCategory.Rifle),

        ["awp"] = new("AWP", WeaponCategory.Sniper),
        ["ssg08"] = new("SSG 08", WeaponCategory.Sniper),
        ["scar20"] = new("SCAR-20", WeaponCategory.Sniper),
        ["g3sg1"] = new("G3SG1", WeaponCategory.Sniper),

        ["nova"] = new("Nova", WeaponCategory.Shotgun),
        ["xm1014"] = new("XM1014", WeaponCategory.Shotgun),
        ["mag7"] = new("MAG-7", WeaponCategory.Shotgun),
        ["sawedoff"] = new("Sawed-Off", WeaponCategory.Shotgun),

        ["m249"] = new("M249", WeaponCategory.Heavy),
        ["negev"] = new("Negev", WeaponCategory.Heavy),

        ["knife"] = new("Knife", WeaponCategory.Melee),
        ["hegrenade"] = new("HE Grenade", WeaponCategory.Grenade),
        ["molotov"] = new("Molotov / Incendiary", WeaponCategory.Grenade),
        ["decoy"] = new("Decoy Grenade", WeaponCategory.Grenade),
        ["taser"] = new("Zeus x27", WeaponCategory.Equipment),
    };

    /// <summary>"total_kills_*" ilə başlayan, amma silah olmayan açarlar.</summary>
    private static readonly HashSet<string> NonWeaponSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "headshot",
        "enemy_weapon",
        "enemy_blinded",
        "knife_fight",
        "against_zoomed_sniper",
    };

    /// <summary>
    /// "last_match_favweapon_id" oyunun item definition index-idir. Bəzi indekslərin öz stat açarı yoxdur
    /// (M4A1-S, USP-S stat-larda m4a1 / hkp2000 altında yazılır), ona görə ad ayrıca saxlanılır.
    /// </summary>
    private static readonly Dictionary<int, (string Key, string Name)> DefinitionIndexes = new()
    {
        [1] = ("deagle", "Desert Eagle"),
        [2] = ("elite", "Dual Berettas"),
        [3] = ("fiveseven", "Five-SeveN"),
        [4] = ("glock", "Glock-18"),
        [7] = ("ak47", "AK-47"),
        [8] = ("aug", "AUG"),
        [9] = ("awp", "AWP"),
        [10] = ("famas", "FAMAS"),
        [11] = ("g3sg1", "G3SG1"),
        [13] = ("galilar", "Galil AR"),
        [14] = ("m249", "M249"),
        [16] = ("m4a1", "M4A4"),
        [17] = ("mac10", "MAC-10"),
        [19] = ("p90", "P90"),
        [23] = ("mp5sd", "MP5-SD"),
        [24] = ("ump45", "UMP-45"),
        [25] = ("xm1014", "XM1014"),
        [26] = ("bizon", "PP-Bizon"),
        [27] = ("mag7", "MAG-7"),
        [28] = ("negev", "Negev"),
        [29] = ("sawedoff", "Sawed-Off"),
        [30] = ("tec9", "Tec-9"),
        [31] = ("taser", "Zeus x27"),
        [32] = ("hkp2000", "P2000"),
        [33] = ("mp7", "MP7"),
        [34] = ("mp9", "MP9"),
        [35] = ("nova", "Nova"),
        [36] = ("p250", "P250"),
        [38] = ("scar20", "SCAR-20"),
        [39] = ("sg556", "SG 553"),
        [40] = ("ssg08", "SSG 08"),
        [42] = ("knife", "Knife"),
        [44] = ("hegrenade", "HE Grenade"),
        [46] = ("molotov", "Molotov"),
        [47] = ("decoy", "Decoy Grenade"),
        [48] = ("molotov", "Incendiary Grenade"),
        [59] = ("knife", "Knife"),
        [60] = ("m4a1", "M4A1-S"),
        [61] = ("hkp2000", "USP-S"),
        [63] = ("cz75a", "CZ75-Auto"),
        [64] = ("revolver", "R8 Revolver"),
    };

    public static bool IsWeapon(string suffix) => !NonWeaponSuffixes.Contains(suffix);

    public static WeaponInfo Describe(string key) =>
        Weapons.TryGetValue(key, out WeaponInfo? info)
            ? info
            : new WeaponInfo(key.ToUpperInvariant(), WeaponCategory.Other);

    public static (string Key, string Name)? FromDefinitionIndex(int definitionIndex) =>
        DefinitionIndexes.TryGetValue(definitionIndex, out var weapon) ? weapon : null;
}
