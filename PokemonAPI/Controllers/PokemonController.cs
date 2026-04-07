using Microsoft.AspNetCore.Mvc;
using PokemonAPI.Services;

namespace PokemonAPI.Controllers;

[ApiController]
[Route("api/pokemon")]
public class PokemonController : ControllerBase
{
    private readonly PokemonDbService _service;
    private readonly PokemonLiveService _liveService;

    public PokemonController(PokemonDbService service, PokemonLiveService liveService)
    {
        _service = service;
        _liveService = liveService;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAll();
        return Ok(data);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pokemon = await _service.GetById(id);

        if (pokemon == null)
            return NotFound();

        return Ok(pokemon);
    }

    // GET BY NAME
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _service.GetByName(name);
        return Ok(result);
    }

    // GET LIVE BY ID
    [HttpGet("live/{id}")]
    public async Task<IActionResult> GetLive(int id)
    {
        var data = await _liveService.GetPokemon(id);
        if (data == null)
        {
            return NotFound();
        }

        return Ok(data);
    }

    // GET LIVE BY NAME
    [HttpGet("live/name/{name}")]
    public async Task<IActionResult> GetLiveByName(string name)
    {
        var data = await _liveService.GetPokemon(name);
        if (data == null)
        {
            return NotFound();
        }

        return Ok(data);
    }

    // GET BY TYPE
    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(string type)
    {
        var result = await _service.GetByType(type);
        return Ok(result);
    }
}
