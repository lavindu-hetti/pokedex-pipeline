using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PokemonIngestion.Data;
using PokemonIngestion.Models;

namespace PokemonIngestion.Services;

public class PokemonService
{
    private readonly HttpClient _client = new HttpClient();

   public async Task RunAsync()
{
    var batch = new List<Pokemon>();

    string url = "https://pokeapi.co/api/v2/pokemon?offset=0&limit=20";

    int totalFetched = 0;
    int maxLimit = 100;

    while (!string.IsNullOrWhiteSpace(url) && totalFetched < maxLimit)
    {
        var response = await _client.GetStringAsync(url);

        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;

        var results = root.GetProperty("results");

        foreach (var item in results.EnumerateArray())
        {
            if (totalFetched >= maxLimit)
                break;

            var pokemonUrl = item.GetProperty("url").GetString();
            if (string.IsNullOrWhiteSpace(pokemonUrl))
            {
                continue;
            }

            var pokemon = await FetchPokemonDetails(pokemonUrl);

            batch.Add(pokemon);
            totalFetched++;

            Console.WriteLine($"Fetched: {pokemon.Name}");

            // 💾 SAVE EVERY 20
            if (batch.Count == 20)
            {
                await SaveToDatabase(batch);
                batch.Clear(); // reset batch
            }
        }

        url = root.GetProperty("next").GetString() ?? string.Empty;
    }

    //  SAVE REMAINING (if not exactly 20)
    if (batch.Count > 0)
    {
        await SaveToDatabase(batch);
    }

    Console.WriteLine(" Finished processing!");
}

    private async Task<Pokemon> FetchPokemonDetails(string url)
    {
        var json = await _client.GetStringAsync(url);

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

    private async Task SaveToDatabase(List<Pokemon> pokemonList)
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();

        var incomingIds = pokemonList.Select(p => p.Id).ToList();
        var existingIds = await db.Pokemons
            .Where(p => incomingIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        var newPokemon = pokemonList
            .Where(p => !existingIds.Contains(p.Id))
            .ToList();

        if (newPokemon.Count == 0)
        {
            Console.WriteLine(" No new Pokemon to save in this batch.");
            return;
        }

        await db.Database.OpenConnectionAsync();
        try
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Pokemons ON");
            try
            {
                db.Pokemons.AddRange(newPokemon);
                await db.SaveChangesAsync();
            }
            finally
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Pokemons OFF");
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        Console.WriteLine(" Data saved to database!");
    }
}   
