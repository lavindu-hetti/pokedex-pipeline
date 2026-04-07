using Microsoft.EntityFrameworkCore;
using PokemonIngestion.Models;

namespace PokemonIngestion.Data;

public class AppDbContext : DbContext
{
    public DbSet<Pokemon> Pokemons => Set<Pokemon>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=localhost;Database=PokemonDB;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pokemon>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
    }
}
