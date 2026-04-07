using System.Text.Json;
using PokemonAPI.Models;

namespace PokemonAPI.Services;

public class PokemonLiveService
{
    private readonly HttpClient _client;

    public PokemonLiveService(HttpClient client)
    {
        _client = client;
    }

    public async Task<Pokemon?> GetPokemon(int id)
    {
        return await GetPokemonFromResource(id.ToString());
    }

    public async Task<Pokemon?> GetPokemon(string name)
    {
        return await GetPokemonFromResource(name.Trim().ToLowerInvariant());
    }

    private async Task<Pokemon?> GetPokemonFromResource(string resource)
    {
        using var response = await _client.GetAsync($"https://pokeapi.co/api/v2/pokemon/{resource}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var typesList = root.GetProperty("types")
            .EnumerateArray()
            .Select(t => t.GetProperty("type").GetProperty("name").GetString())
            .ToList();

        var abilitiesList = root.GetProperty("abilities")
            .EnumerateArray()
            .Select(a => a.GetProperty("ability").GetProperty("name").GetString())
            .ToList();

        return new Pokemon
        {
            Id = root.GetProperty("id").GetInt32(),
            Name = root.GetProperty("name").GetString() ?? string.Empty,
            Height = root.GetProperty("height").GetInt32(),
            Weight = root.GetProperty("weight").GetInt32(),
            Types = string.Join(",", typesList),
            Abilities = string.Join(",", abilitiesList)
        };
    }
}
