namespace DotaDashboard.Models.DTOs;

public class TopPlayerDto
{
    public long PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }
    public int TotalMatches { get; set; }
    public double Kda { get; set; }
}
