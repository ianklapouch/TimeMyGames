using Microsoft.AspNetCore.Mvc;
using TimeMyGames.Services;

namespace TimeMyGames.Controllers;

[ApiController]
[Route("api/games/")]
public class TestController : ControllerBase
{
    
    private readonly SteamService _steamService;

    public TestController(SteamService steamService)
    {
        _steamService = steamService;
    }

    [HttpGet]
    [Route("/{name}")]
    
    public async Task<IActionResult> Get([FromRoute] string name)
    {
        var steamGames = await _steamService.GetSteamGamesAsync(name);
        return Ok(new { steamGames });
    }
}
