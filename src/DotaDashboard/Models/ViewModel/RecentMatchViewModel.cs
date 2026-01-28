namespace DotaDashboard.Models.ViewModel;

public class RecentMatchViewModel
{
    public long MatchId { get; set; }
    public DateTime StartTime { get; set; }
    public int Duration { get; set; }
    public bool RadiantWin { get; set; }

    // List of heroes in this match (up to 10)
    public List<HeroInMatch> Heroes { get; set; } = new();

    // Aggregate stats for the match
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }
}