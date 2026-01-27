using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotaDashboard.Models.Entities;

public class MatchPlayer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long MatchId { get; set; }

    [Required]
    public long PlayerId { get; set; }

    [Required]
    public int HeroId { get; set; }

    public int Kills { get; set; }

    public int Deaths { get; set; }

    public int Assists { get; set; }

    public bool IsRadiant { get; set; }

    public bool Won { get; set; }

    // Navigation properties
    [ForeignKey(nameof(MatchId))]
    public virtual Match Match { get; set; } = null!;

    [ForeignKey(nameof(PlayerId))]
    public virtual Player Player { get; set; } = null!;

    [ForeignKey(nameof(HeroId))]
    public virtual Hero Hero { get; set; } = null!;
}
