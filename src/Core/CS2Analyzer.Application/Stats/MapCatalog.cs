using CS2Analyzer.Application.DTOs;

namespace CS2Analyzer.Application.Stats;

/// <summary>Steam stat açarındakı xəritə adı (məs. "de_dust2") -> göstərilən ad və rejim.</summary>
public static class MapCatalog
{
    private static readonly Dictionary<string, string> Names = new(StringComparer.OrdinalIgnoreCase)
    {
        ["de_dust2"] = "Dust II",
        ["de_inferno"] = "Inferno",
        ["de_mirage"] = "Mirage",
        ["de_nuke"] = "Nuke",
        ["de_overpass"] = "Overpass",
        ["de_vertigo"] = "Vertigo",
        ["de_ancient"] = "Ancient",
        ["de_anubis"] = "Anubis",
        ["de_train"] = "Train",
        ["de_cache"] = "Cache",
        ["de_cbble"] = "Cobblestone",
        ["de_aztec"] = "Aztec",
        ["de_dust"] = "Dust",
        ["de_vertigo_old"] = "Vertigo (köhnə)",
        ["de_lake"] = "Lake",
        ["de_safehouse"] = "Safehouse",
        ["de_shortdust"] = "Shortdust",
        ["de_stmarc"] = "St. Marc",
        ["de_bank"] = "Bank",
        ["de_sugarcane"] = "Sugarcane",
        ["cs_office"] = "Office",
        ["cs_italy"] = "Italy",
        ["cs_assault"] = "Assault",
        ["cs_militia"] = "Militia",
        ["ar_shoots"] = "Shoots",
        ["ar_baggage"] = "Baggage",
        ["ar_monastery"] = "Monastery",
        ["ar_pool_day"] = "Pool Day",
    };

    public static string NameOf(string key)
    {
        if (Names.TryGetValue(key, out string? name))
        {
            return name;
        }

        // Tanınmayan xəritə: "de_some_map" -> "Some Map"
        string bare = key.Contains('_') ? key[(key.IndexOf('_') + 1)..] : key;
        return string.Join(' ', bare.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    public static MapMode ModeOf(string key) => key.Split('_')[0].ToLowerInvariant() switch
    {
        "de" => MapMode.Defusal,
        "cs" => MapMode.Hostage,
        "ar" => MapMode.ArmsRace,
        _ => MapMode.Other,
    };
}
