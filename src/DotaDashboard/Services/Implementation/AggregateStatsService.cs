using DotaDashboard.Data;
using DotaDashboard.Models.DTOs;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DotaDashboard.Services.Implementation;

public class AggregateStatsService(ApplicationDbContext context, ILogger<AggregateStatsService> logger)
    : IAggregateStatsService
{
    public async Task<List<TopHeroDto>> GetTopHeroesByWinRateAsync(int count = 5, int minimumPicks = 10)
    {
        try
        {
            logger.LogInformation("Calculating top {Count} heroes by win rate (minimum picks: {MinimumPicks})", count, minimumPicks);

            // ✅ FIX: Fetch to memory FIRST, then calculate WinRate
            var heroes = await context.Heroes
                .Where(h => h.TotalPicks >= minimumPicks)
                .ToListAsync();  // ← Bring to memory

            // Now calculate WinRate in C# (not SQL)
            var topHeroes = heroes
                .Select(h => new TopHeroDto
                {
                    HeroId = h.HeroId,
                    Name = h.Name,
                    ImageUrl = h.ImageUrl,
                    TotalPicks = h.TotalPicks,
                    TotalWins = h.TotalWins,
                    WinRate = h.TotalPicks > 0 ? (h.TotalWins / (double)h.TotalPicks) * 100 : 0  // ← Calculate here
                })
                .OrderByDescending(h => h.WinRate)
                .Take(count)
                .ToList();

            logger.LogInformation("Found {Count} top heroes with win rates", topHeroes.Count);
            return topHeroes;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating top heroes by win rate");
            return new List<TopHeroDto>();
        }
    }

    public async Task<List<TopPlayerDto>> GetTopPlayersByKdaAsync(int count = 5, int minimumMatches = 5)
    {
        try
        {
            logger.LogInformation("Calculating top {Count} players by KDA (min matches: {MinimumMatches})",
                count, minimumMatches);

            // FIX: Fetch to memory FIRST
            var players = await context.Players
                .Where(p => p.TotalMatches >= minimumMatches)
                .ToListAsync();  // ← Bring to memory

            // Calculate KDA in C# (not SQL)
            var topPlayers = players
                .Select(p => new TopPlayerDto
                {
                    PlayerId = p.PlayerId,
                    Name = p.Name ?? $"Player_{p.PlayerId}",
                    AvatarUrl = p.AvatarUrl,
                    TotalKills = p.TotalKills,
                    TotalDeaths = p.TotalDeaths,
                    TotalAssists = p.TotalAssists,
                    TotalMatches = p.TotalMatches,
                    Kda = p.TotalDeaths > 0
                        ? (double)(p.TotalKills + p.TotalAssists) / p.TotalDeaths
                        : p.TotalKills + p.TotalAssists  // ← Calculate here
                })
                .OrderByDescending(p => p.Kda)
                .Take(count)
                .ToList();

            logger.LogInformation("Found {Count} top players", topPlayers.Count);
            return topPlayers;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating top players by KDA");
            return new List<TopPlayerDto>();
        }
    }

    public async Task<List<MatchVolumeDto>> GetMatchVolumeByHourAsync(int hours = 24)
    {
        try
        {
            logger.LogInformation("Calculating match volume for last {Hours} hours", hours);

            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            var matchVolume = await context.Matches
                .Where(m => m.StartTime >= cutoffTime)
                .GroupBy(m => new
                {
                    m.StartTime.Year,
                    m.StartTime.Month,
                    m.StartTime.Day,
                    m.StartTime.Hour
                })
                .Select(g => new MatchVolumeDto
                {
                    Hour = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                    MatchCount = g.Count()
                })
                .OrderBy(mv => mv.Hour)
                .ToListAsync();

            logger.LogInformation("Found match volume data for {Count} hours", matchVolume.Count);
            return matchVolume;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating match volume");
            return new List<MatchVolumeDto>();
        }
    }

    public async Task<int> GetTotalMatchesAsync()
    {
        try
        {
            return await context.Matches.CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total match count");
            return 0;
        }
    }

    public async Task<int> GetTotalPlayersAsync()
    {
        try
        {
            return await context.Players.CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting total player count");
            return 0;
        }
    }

    public async Task<int> GetActiveJobCountAsync()
    {
        try
        {
            return await context.Jobs
                .Where(j => j.Status == JobStatus.Pending || j.Status == JobStatus.Running)
                .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active job count");
            return 0;
        }
    }
}