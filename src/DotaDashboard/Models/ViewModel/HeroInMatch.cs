namespace DotaDashboard.Models.ViewModel;

public class HeroInMatch
{
    public int HeroId { get; set; }
    public string HeroName { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public bool IsRadiant { get; set; }
    public bool Won { get; set; }
}
