using DotaDashboard.Models.DTOs;

namespace DotaDashboard.Models.ViewModel;

public class DashboardViewModel
{
    // KPI Cards
    public int TotalMatches { get; set; }
    public int TotalPlayers { get; set; }
    public int ActiveJobs { get; set; }
    public TopHeroDto? TopHero { get; set; }
    public TopPlayerDto? TopPlayer { get; set; }

    // Charts Data
    public List<TopHeroDto> TopHeroes { get; set; } = new();
    public List<TopPlayerDto> TopPlayers { get; set; } = new();
    public List<MatchVolumeDto> MatchVolume { get; set; } = new();

    // Recent Matches
    public List<RecentMatchViewModel> RecentMatches { get; set; } = new();
}