using System.ComponentModel.DataAnnotations;

namespace PokemonAPI.Models;

public class Pokemon
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int Height { get; set; }
    public int Weight { get; set; }

    public string Types { get; set; } = string.Empty;
    public string Abilities { get; set; } = string.Empty;
}
