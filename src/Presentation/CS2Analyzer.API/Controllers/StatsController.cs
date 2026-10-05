using System.Net;
using CS2Analyzer.Application.DTOs;
using CS2Analyzer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CS2Analyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly ISteamService _steamService;

    public StatsController(ISteamService steamService)
    {
        _steamService = steamService;
    }

    [HttpGet("current-players")]
    public async Task<IActionResult> GetCurrentPlayers(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _steamService.GetCurrentPlayersAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>Bölmələrə ayrılmış statistika: overview, combat, weapons, maps, objectives, economy, lastMatch.</summary>
    [HttpGet("user/{steamId}")]
    [ProducesResponseType<PlayerStatsDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserStats(string steamId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _steamService.GetUserStatsAsync(steamId, cancellationToken);
            return Ok(result);
        }
        // Gizli profildə Steam 401/403 qaytarır; frontend bunu ayrıca izah edir
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Profile or game details are private." });
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}