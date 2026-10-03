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

    [HttpGet("user/{steamId}")]
    public async Task<IActionResult> GetUserStats(string steamId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _steamService.GetUserStatsAsync(steamId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }
}