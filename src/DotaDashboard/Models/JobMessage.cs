namespace DotaDashboard.Models;

/// <summary>
/// Message format for job queue
/// </summary>
public class JobMessage
{
    public int JobId { get; set; }
    public string Type { get;  set; } = string.Empty;
    public string? Target { get; set; }
}
