namespace DotaDashboard.Services.Interfaces;

/// <summary>
/// Service for ingesting data from OpenDota API into the database
/// </summary>
public interface IDataIngestionService
{
    /// <summary>
    /// Ingest all Dota 2 heroes into the database
    /// </summary>
    /// <returns>Number of heroes ingested</returns>
    Task<int> IngestHeroesAsync();

    /// <summary>
    /// Ingest recent professional matches
    /// </summary>
    /// <param name="limit"></param>
    /// <returns></returns>
    Task<int> IngestProMatchesAsync(int limit = 10);

    ///// <summary>
    ///// Ingest recent professional matches
    ///// </summary>
    ///// <param name="limit">Number of matches to fetch</param>
    ///// <returns>Number of matches ingested</returns>
    //Task<int> IngestPublicMatchesAsync(int limit = 50);

    /// <summary>
    /// Ingest details for a specific match
    /// </summary>
    /// <param name="matchId">The match ID</param>
    /// <returns>1 if successful, 0 if failed or already exists</returns>
    Task<int> IngestMatchDetailsAsync(long matchId);
}
