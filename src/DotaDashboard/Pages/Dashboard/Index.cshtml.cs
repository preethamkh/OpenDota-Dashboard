using DotaDashboard.Data;
using DotaDashboard.Models.ViewModel;
using DotaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DotaDashboard.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IAggregateStatsService _aggregateStatsService;
    private readonly IJobService _jobService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IAggregateStatsService aggregateStatsService,
        IJobService jobService,
        ApplicationDbContext context,
        ILogger<IndexModel> logger)
    {
        _aggregateStatsService = aggregateStatsService;
        _jobService = jobService;
        _context = context;
        _logger = logger;
    }

    public DashboardViewModel ViewModel { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading dashboard data");

            // Load KPIs
            ViewModel.TotalMatches = await _aggregateStatsService.GetTotalMatchesAsync();
            ViewModel.TotalPlayers = await _aggregateStatsService.GetTotalPlayersAsync();
            ViewModel.ActiveJobs = await _aggregateStatsService.GetActiveJobCountAsync();

            // Load Top Heroes
            ViewModel.TopHeroes = await _aggregateStatsService.GetTopHeroesByWinRateAsync(5, 1);
            ViewModel.TopHero = ViewModel.TopHeroes.FirstOrDefault();

            // Load Top Players
            ViewModel.TopPlayers = await _aggregateStatsService.GetTopPlayersByKdaAsync(5, 1);
            ViewModel.TopPlayer = ViewModel.TopPlayers.FirstOrDefault();

            // Load Match Volume
            ViewModel.MatchVolume = await _aggregateStatsService.GetMatchVolumeByHourAsync(24);

            // Load Recent Matches
            ViewModel.RecentMatches = await GetRecentMatchesAsync(10);

            _logger.LogInformation("Dashboard data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
        }
    }

    public async Task<IActionResult> OnPostCreateJobAsync(string jobType)
    {
        try
        {
            _logger.LogInformation("Creating job of type {JobType} from dashboard", jobType);

            await _jobService.CreateJobAsync(jobType);

            TempData["Success"] = $"Job '{jobType}' created successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            TempData["Error"] = $"Failed to create job: {ex.Message}";
        }

        return RedirectToPage();
    }

    private async Task<List<RecentMatchViewModel>> GetRecentMatchesAsync(int count)
    {
        var matches = await _context.Matches
            .OrderByDescending(m => m.StartTime)
            .Take(count)
            .Include(m => m.MatchPlayers)
                .ThenInclude(mp => mp.Hero)
            .Include(m => m.MatchPlayers)
                .ThenInclude(mp => mp.Player)
            .ToListAsync();

        var recentMatches = matches.Select(m => new RecentMatchViewModel
        {
            MatchId = m.MatchId,
            StartTime = m.StartTime,
            Duration = m.Duration,
            RadiantWin = m.WinnerRadiant,
            Heroes = m.MatchPlayers.Select(mp => new HeroInMatch
            {
                HeroId = mp.HeroId,
                HeroName = mp.Hero.Name,
                HeroImageUrl = mp.Hero.ImageUrl,
                PlayerName = mp.Player.Name ?? $"Player_{mp.PlayerId}",
                Kills = mp.Kills,
                Deaths = mp.Deaths,
                Assists = mp.Assists,
                IsRadiant = mp.IsRadiant,
                Won = mp.Won
            }).ToList(),
            TotalKills = m.MatchPlayers.Sum(mp => mp.Kills),
            TotalDeaths = m.MatchPlayers.Sum(mp => mp.Deaths),
            TotalAssists = m.MatchPlayers.Sum(mp => mp.Assists)
        }).ToList();

        return recentMatches;
    }
}