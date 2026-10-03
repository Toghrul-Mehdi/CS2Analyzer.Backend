
using CS2Analyzer.Application.DTOs;

namespace CS2Analyzer.Application.Interfaces;

public interface ISteamProfileService
{
    /// <summary>Profil alına bilmirsə (açar yoxdur, Steam əlçatmazdır) null qaytarır.</summary>
    Task<SteamProfileDto?> GetProfileAsync(string steamId, CancellationToken cancellationToken);
}