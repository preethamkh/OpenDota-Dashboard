using DotaDashboard.Models.DTOs;

namespace DotaDashboard.Services.Interfaces;

/// <summary>
/// Service for interacting with the OpenDota API
/// </summary>
public interface IOpenDotaService
{
    /// <summary>
    /// Fetch recent professional matches
    /// </summary>
    /// <param name="limit">Maximum number of matches to fetch</param>
    /// <returns>List of professional match DTOs</returns>
    Task<List<OpenDotaPublicMatchDto>> GetPublicMatchesAsync(int limit = 50);

    /// <summary>
    /// Get recent professional matches
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<List<OpenDotaPublicMatchDto>> GetProMatchesAsync(int limit = 50);

    /// <summary>
    /// Get detailed information about a specific match
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <returns>Match details including player information</returns>
    Task<OpenDotaMatchDto?> GetMatchDetailsAsync(long matchId);

    /// <summary>
    /// Get all Dota 2 heroes
    /// </summary>
    /// <returns>List of hero DTOs</returns>
    Task<List<OpenDotaHeroDto>> GetHeroesAsync();

    /// <summary>
    /// Get player profile information
    /// </summary>
    /// <param name="accountId">The player's account ID</param>
    /// <returns>Player profile DTO</returns>
    Task<OpenDotaPlayerProfileDto?> GetPlayerProfileAsync(long accountId);
}
