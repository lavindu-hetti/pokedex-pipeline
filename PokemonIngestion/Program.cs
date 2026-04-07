using PokemonIngestion.Services;

class Program
{
    static async Task Main()
    {
        var service = new PokemonService();
        await service.RunAsync();
    }
}