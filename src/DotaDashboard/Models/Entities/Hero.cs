using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotaDashboard.Models.Entities;

public class Hero
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int HeroId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int TotalPicks { get; set; }

    public int TotalWins { get; set; }

    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public virtual ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();

    // Computed property for win rate
    [NotMapped]
    public double WinRate => TotalPicks > 0
        ? (TotalWins / (double)TotalPicks) * 100
        : 0;
}