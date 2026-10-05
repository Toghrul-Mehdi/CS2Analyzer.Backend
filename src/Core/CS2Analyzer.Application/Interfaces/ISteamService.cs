using CS2Analyzer.Application.DTOs;

namespace CS2Analyzer.Application.Interfaces;

public interface ISteamService
{
    Task<CurrentPlayerStatDto> GetCurrentPlayersAsync(CancellationToken cancellationToken = default);
    Task<PlayerStatsDto> GetUserStatsAsync(string steamId, CancellationToken cancellationToken = default);
}