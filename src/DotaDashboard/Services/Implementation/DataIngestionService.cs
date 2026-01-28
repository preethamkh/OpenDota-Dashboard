using DotaDashboard.Data;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DotaDashboard.Services.Implementation;

public class DataIngestionService(
    ApplicationDbContext context,
    IOpenDotaService openDotaService,
    ILogger<DataIngestionService> logger)
    : IDataIngestionService
{
    public async Task<int> IngestHeroesAsync()
    {
        try
        {
            logger.LogInformation("Starting hero ingestion...");

            var heroes = await openDotaService.GetHeroesAsync();

            if (!heroes.Any())
            {
                logger.LogWarning("No heroes returned from API");
                return 0;
            }

            int count = 0;
            foreach (var heroDto in heroes)
            {
                var existingHero = await context.Heroes.FindAsync(heroDto.Id);

                if (existingHero == null)
                {
                    var hero = new Hero
                    {
                        HeroId = heroDto.Id,
                        Name = heroDto.LocalizedName,
                        ImageUrl = $"https://cdn.cloudflare.steamstatic.com{heroDto.GetImagePath()}",
                        TotalPicks = 0,
                        TotalWins = 0,
                        LastUpdated = DateTime.UtcNow
                    };

                    context.Heroes.Add(hero);
                    count++;
                }
                else
                {
                    // Update existing hero
                    existingHero.Name = heroDto.LocalizedName;
                    existingHero.ImageUrl = $"https://cdn.cloudflare.steamstatic.com{heroDto.GetImagePath()}";
                    existingHero.LastUpdated = DateTime.UtcNow;
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully ingested {Count} new heroes", count);

            return count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during hero ingestion");
            throw;
        }
    }

    public async Task<int> IngestProMatchesAsync(int limit = 10)
    {
        try
        {
            logger.LogInformation("Starting ingestion of {Limit} professional matches...", limit);

            var proMatches = await openDotaService.GetProMatchesAsync(limit);

            if (!proMatches.Any())
            {
                logger.LogWarning("No pro matches returned from API");
                return 0;
            }

            int count = 0;
            int skipped = 0;

            foreach (var match in proMatches)
            {
                if (match.MatchId == 0 || match.Duration < 600)
                {
                    logger.LogWarning("Skipping invalid/short match {MatchId} (duration: {Duration}s)",
                        match.MatchId, match.Duration);
                    skipped++;
                    continue;
                }

                var existing = await context.Matches
                    .AsNoTracking()
                    .AnyAsync(m => m.MatchId == match.MatchId);

                if (!existing)
                {
                    var ingested = await IngestMatchDetailsAsync(match.MatchId);
                    count += ingested;

                    logger.LogInformation("Ingested pro match {MatchId}, total: {Count}/{Total}",
                        match.MatchId, count, proMatches.Count);
                }
                else
                {
                    logger.LogDebug("Pro match {MatchId} already exists, skipping", match.MatchId);
                    skipped++;
                }
            }

            logger.LogInformation("Successfully ingested {Count} pro matches ({Skipped} skipped)", count, skipped);
            return count;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ingesting pro matches");
            throw;
        }
    }

    //public async Task<int> IngestPublicMatchesAsync(int limit = 50)
    //{
    //    try
    //    {
    //        logger.LogInformation("Starting ingestion of {Limit} public matches...", limit);

    //        // Warn if limit might exceed daily quota (each match = 1 API call)
    //        if (limit > 100)
    //        {
    //            logger.LogWarning("Requesting {Limit} matches. This will use {Limit} API calls. Daily limit is 3000.", limit);
    //        }

    //        var publicMatches = await openDotaService.GetPublicMatchesAsync(limit);

    //        if (!publicMatches.Any())
    //        {
    //            logger.LogWarning("No public matches returned from API");
    //            return 0;
    //        }

    //        int count = 0;
    //        int processed = 0;
    //        int skipped = 0;

    //        foreach (var matchDto in publicMatches)
    //        {
    //            processed++;

    //            // Skip invalid match IDs
    //            if (matchDto.MatchId == 0)
    //            {
    //                logger.LogWarning("Skipping invalid match with MatchId = 0");
    //                continue;
    //            }

    //            // Filter out suspicious matches based on public match data
    //            if (matchDto.Duration < 600) // Less than 10 minutes
    //            {
    //                logger.LogWarning("Skipping match {MatchId} - too short (duration: {Duration}s)",
    //                    matchDto.MatchId, matchDto.Duration);
    //                skipped++;
    //                continue;
    //            }

    //            // Skip very low MMR matches (likely bots)
    //            if (matchDto.AvgMmr is < 1000)
    //            {
    //                logger.LogWarning("Skipping match {MatchId} - MMR too low (avg_mmr: {AvgMmr})",
    //                    matchDto.MatchId, matchDto.AvgMmr);
    //                skipped++;
    //                continue;
    //            }

    //            // Use AsNoTracking to avoid tracking conflicts
    //            var existingMatch = await context.Matches
    //                .AsNoTracking()
    //                .FirstOrDefaultAsync(m => m.MatchId == matchDto.MatchId);

    //            if (existingMatch == null)
    //            {
    //                try
    //                {
    //                    // Fetch full match details to get player information
    //                    var ingestedCount = await IngestMatchDetailsAsync(matchDto.MatchId);
    //                    count += ingestedCount;

    //                    logger.LogInformation("Progress: {Processed}/{Total} matches processed, {Ingested} ingested, {Skipped} skipped",
    //                        processed, publicMatches.Count, count, skipped);
    //                }
    //                catch (InvalidOperationException ex) when (ex.Message.Contains("Daily API call limit"))
    //                {
    //                    logger.LogError("Daily API limit reached after ingesting {Count} matches. Stopping.", count);
    //                    break; // Stop processing if we hit daily limit
    //                }
    //            }
    //            else
    //            {
    //                logger.LogDebug("Match {MatchId} already exists, skipping", matchDto.MatchId);
    //                skipped++;
    //            }
    //        }

    //        logger.LogInformation("Successfully ingested {Count} new matches out of {Total} processed ({Skipped} skipped)",
    //            count, processed, skipped);
    //        return count;
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error during public match ingestion");
    //        throw;
    //    }
    //}

    public async Task<int> IngestMatchDetailsAsync(long matchId)
    {
        try
        {
            // Use AsNoTracking to check existence without tracking (issue fix to avoid adding existing match while ingesting match data)
            var matchExists = await context.Matches
                .AsNoTracking()
                .AnyAsync(m => m.MatchId == matchId);

            if (matchExists)
            {
                logger.LogDebug("Match {MatchId} already exists, skipping", matchId);
                return 0;
            }

            var matchDetails = await openDotaService.GetMatchDetailsAsync(matchId);

            if (matchDetails == null)
            {
                logger.LogWarning("Failed to fetch details for match {MatchId}", matchId);
                return 0;
            }

            // Create match entity
            var match = new Match
            {
                MatchId = matchDetails.MatchId,
                StartTime = DateTimeOffset.FromUnixTimeSeconds(matchDetails.StartTime).UtcDateTime,
                Duration = matchDetails.Duration,
                WinnerRadiant = matchDetails.RadiantWin,
                ProcessedAt = DateTime.UtcNow
            };

            context.Matches.Add(match);

            // Process players
            if (matchDetails.Players != null && matchDetails.Players.Any())
            {
                foreach (var playerDto in matchDetails.Players)
                {
                    // Check if hero exists, create if missing
                    var hero = await context.Heroes.FindAsync(playerDto.HeroId);
                    if (hero == null)
                    {
                        logger.LogWarning("Hero {HeroId} not found for match {MatchId}, creating placeholder hero",
                            playerDto.HeroId, matchId);

                        // Create a placeholder hero
                        hero = new Hero
                        {
                            HeroId = playerDto.HeroId,
                            Name = $"Hero_{playerDto.HeroId}", // Placeholder name
                            ImageUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{playerDto.HeroId}.png",
                            TotalPicks = 0,
                            TotalWins = 0,
                            LastUpdated = DateTime.UtcNow
                        };
                        context.Heroes.Add(hero);
                    }

                    // Ensure player exists
                    var player = await context.Players.FindAsync(playerDto.AccountId);
                    if (player == null)
                    {
                        // Optionally fetch profile for avatar
                        //var profile = await openDotaService.GetPlayerProfileAsync(playerDto.AccountId);

                        player = new Player
                        {
                            PlayerId = playerDto.AccountId,
                            Name = $"Player_{playerDto.AccountId}",
                            // avoiding another fetch for avatar URL, set to null for now
                            AvatarUrl = null,
                            TotalKills = 0,
                            TotalDeaths = 0,
                            TotalAssists = 0,
                            TotalMatches = 0,
                            TotalWins = 0,
                            LastUpdated = DateTime.UtcNow
                        };
                        context.Players.Add(player);
                    }

                    // Determine team and win status
                    // player_slot < 128 means Radiant side
                    bool isRadiant = playerDto.PlayerSlot < 128;
                    bool won = (isRadiant && matchDetails.RadiantWin) ||
                              (!isRadiant && !matchDetails.RadiantWin);

                    // Create MatchPlayer record
                    var matchPlayer = new MatchPlayer
                    {
                        MatchId = matchId,
                        PlayerId = playerDto.AccountId,
                        HeroId = playerDto.HeroId,
                        Kills = playerDto.Kills,
                        Deaths = playerDto.Deaths,
                        Assists = playerDto.Assists,
                        IsRadiant = isRadiant,
                        Won = won
                    };

                    context.MatchPlayers.Add(matchPlayer);

                    // Update player aggregates
                    player.TotalKills += playerDto.Kills;
                    player.TotalDeaths += playerDto.Deaths;
                    player.TotalAssists += playerDto.Assists;
                    player.TotalMatches++;
                    if (won) player.TotalWins++;
                    player.LastUpdated = DateTime.UtcNow;

                    // Update hero aggregates
                    hero.TotalPicks++;
                    if (won) hero.TotalWins++;
                    hero.LastUpdated = DateTime.UtcNow;
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully ingested match {MatchId} with {PlayerCount} players",
                matchId, matchDetails.Players?.Count ?? 0);

            return 1;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException {SqlState: "23505"})
        {
            // Handle duplicate key error - match was already inserted (race condition or previous partial success)
            logger.LogWarning("Match {MatchId} already exists in database (duplicate key), skipping", matchId);

            // Clear change tracker to prevent tracking conflicts
            context.ChangeTracker.Clear();

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ingesting match details for {MatchId}", matchId);

            // Clear change tracker to prevent conflicts with next match
            context.ChangeTracker.Clear();

            return 0;
        }
    }
}
