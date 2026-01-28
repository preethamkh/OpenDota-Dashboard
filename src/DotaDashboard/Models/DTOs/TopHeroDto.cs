namespace DotaDashboard.Models.DTOs;

public class TopHeroDto
{
    public int HeroId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalPicks { get; set; }
    public int TotalWins { get; set; }
    public double WinRate { get; set; }
}