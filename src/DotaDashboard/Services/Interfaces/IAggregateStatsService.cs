using DotaDashboard.Models.DTOs;

namespace DotaDashboard.Services.Interfaces;

/// <summary>
/// Service for calculating aggregate statistics and metrics.
/// </summary>
public interface IAggregateStatsService
{
    /// <summary>
    /// Get top N heroes by win rate
    /// </summary>
    /// <param name="count">Number of heroes to return</param>
    /// <param name="minimumPicks">Minimum number of picks to qualify</param>
    /// <returns></returns>
    Task<List<TopHeroDto>> GetTopHeroesByWinRateAsync(int count = 5, int minimumPicks = 10);

    /// <summary>
    /// Get top N players by KDA ratio
    /// </summary>
    /// <param name="count">Number of players to return</param>
    /// <param name="minimumMatches">Minimum number of matches to qualify</param>
    /// <returns>List of top players with KDA and stats</returns>
    Task<List<TopPlayerDto>> GetTopPlayersByKdaAsync(int count = 5, int minimumMatches = 5);

    /// <summary>
    /// Get match volume grouped by time period
    /// Identifies when players are most active
    /// </summary>
    /// <param name="hours">Number of hours to look back</param>
    /// <returns>Match counts by hour</returns>
    Task<List<MatchVolumeDto>> GetMatchVolumeByHourAsync(int hours = 24);

    /// <summary>
    /// Get total match count
    /// </summary>
    Task<int> GetTotalMatchesAsync();

    /// <summary>
    /// Get total player count
    /// </summary>
    Task<int> GetTotalPlayersAsync();

    /// <summary>
    /// Get active job count
    /// </summary>
    // todo: consider moving to a different service (job related - rabbitmq)
    Task<int> GetActiveJobCountAsync();
}
