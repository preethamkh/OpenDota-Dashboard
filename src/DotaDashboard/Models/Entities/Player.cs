using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotaDashboard.Models.Entities;

public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long PlayerId { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public int TotalKills { get; set; }

    public int TotalDeaths { get; set; }

    public int TotalAssists { get; set; }

    public int TotalMatches { get; set; }

    public int TotalWins { get; set; }

    public DateTime LastUpdated { get; set; }

    // Navigation properties
    public virtual ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();

    // Computed property for K/D/A
    [NotMapped]
    public double Kda => TotalDeaths > 0
        ? (TotalKills + TotalAssists) / (double)TotalDeaths
        : TotalKills + TotalAssists;
}