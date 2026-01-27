using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotaDashboard.Models.Entities;

public class Match
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long MatchId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public int Duration { get; set; }

    [Required]
    public bool WinnerRadiant { get; set; }

    public DateTime ProcessedAt { get; set; }

    // Navigation property
    public virtual ICollection<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
}
