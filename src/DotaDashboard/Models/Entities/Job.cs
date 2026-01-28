using System.ComponentModel.DataAnnotations;

namespace DotaDashboard.Models.Entities;

// To track background tasks or processing jobs related to Dota matches.
public class Job
{
    // Reason for using int:
    // Sequential background jobs - created in order and auto-incrementing ID
    // Better performance due to smaller index size
    // Human readable
    // DB generated
    // URL friendly
    [Key]
    public int JobId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = JobStatus.Pending;

    [MaxLength(255)]
    public string? Target { get; set; }

    public int MatchesProcessed { get; set; }

    public int Retries { get; set; }

    [MaxLength(1000)]
    public string? Error { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
