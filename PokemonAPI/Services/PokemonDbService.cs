using Microsoft.EntityFrameworkCore;
using PokemonAPI.Data;
using PokemonAPI.Models;

namespace PokemonAPI.Services;

public class PokemonDbService
{
    private readonly AppDbContext _db;

    public PokemonDbService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Pokemon>> GetAll()
    {
        return await _db.Pokemons.ToListAsync();
    }

    public async Task<Pokemon?> GetById(int id)
    {
        return await _db.Pokemons.FindAsync(id);
    }

    public async Task<List<Pokemon>> GetByName(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();

        return await _db.Pokemons
            .Where(p => p.Name.ToLower().Contains(normalizedName))
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<List<Pokemon>> GetByType(string type)
    {
        var normalizedType = type.Trim().ToLowerInvariant();

        return await _db.Pokemons
            .Where(p =>
                p.Types.ToLower() == normalizedType ||
                p.Types.ToLower().StartsWith(normalizedType + ",") ||
                p.Types.ToLower().EndsWith("," + normalizedType) ||
                p.Types.ToLower().Contains("," + normalizedType + ","))
            .ToListAsync();
    }
}
