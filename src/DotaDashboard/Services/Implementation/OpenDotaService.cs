using DotaDashboard.Models.DTOs;
using DotaDashboard.Services.Interfaces;
using Newtonsoft.Json;

namespace DotaDashboard.Services.Implementation;

public class OpenDotaService : IOpenDotaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenDotaService> _logger;
    private readonly RateLimiterService _rateLimiter;

    public OpenDotaService(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenDotaService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        var rateLimit = configuration.GetValue("OpenDotaApi:RateLimitPerMinute", 60);
        _rateLimiter = new RateLimiterService(rateLimit);

        _logger.LogInformation("OpenDota API configured (Free Tier: {RateLimit} calls/min, no API key required)",
            rateLimit);
    }

    public async Task<List<OpenDotaPublicMatchDto>> GetPublicMatchesAsync(int limit = 50)
    {
        try
        {
            _logger.LogInformation("Fetching {Limit} public matches from OpenDota API", limit);

            var client = _httpClientFactory.CreateClient("OpenDotaApi");

            // Direct call, no API key required for the free tier
            var response = await client.GetAsync("api/publicMatches");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch public matches. Status: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var matches = JsonConvert.DeserializeObject<List<OpenDotaPublicMatchDto>>(content)
                          ?? [];

            _logger.LogInformation("Successfully fetched {Count} public matches", matches.Count);
            return matches.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching public matches from OpenDota API");
            return [];
        }
    }

    public async Task<List<OpenDotaPublicMatchDto>> GetProMatchesAsync(int limit = 50)
    {
        try
        {
            _logger.LogInformation("Fetching {Limit} pro matches from OpenDota API", limit);

            var client = _httpClientFactory.CreateClient("OpenDotaApi");
            var response = await client.GetAsync("api/proMatches");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch pro matches. Status: {StatusCode}", response.StatusCode);
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var matches = JsonConvert.DeserializeObject<List<OpenDotaPublicMatchDto>>(content)
                          ?? [];

            _logger.LogInformation("Successfully fetched {Count} pro matches", matches.Count);
            return matches.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pro matches from OpenDota API");
            return [];
        }
    }

    public async Task<List<OpenDotaHeroDto>> GetHeroesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching heroes from OpenDota API");

            var client = _httpClientFactory.CreateClient("OpenDotaApi");
            var response = await client.GetAsync("api/heroes");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch heroes. Status: {StatusCode}", response.StatusCode);
                return new List<OpenDotaHeroDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var heroes = JsonConvert.DeserializeObject<List<OpenDotaHeroDto>>(content)
                         ?? new List<OpenDotaHeroDto>();

            _logger.LogInformation("Successfully fetched {Count} heroes", heroes.Count);
            return heroes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching heroes from OpenDota API");
            return new List<OpenDotaHeroDto>();
        }
    }

    public async Task<OpenDotaMatchDto?> GetMatchDetailsAsync(long matchId)
    {
        try
        {
            // Wait for rate limit slot
            await _rateLimiter.WaitForSlotAsync();

            var callsInMinute = _rateLimiter.GetCallsInLastMinute();
            _logger.LogInformation("Fetching match details for {MatchId}. Rate limit: {CallsInMinute}/60",
                matchId, callsInMinute);

            var client = _httpClientFactory.CreateClient("OpenDotaApi");
            var response = await client.GetAsync($"api/matches/{matchId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch match details for {MatchId}. Status: {StatusCode}",
                    matchId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var match = JsonConvert.DeserializeObject<OpenDotaMatchDto>(content);

            _logger.LogInformation("Successfully fetched match details for {MatchId}", matchId);
            return match;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching match details for {MatchId}", matchId);
            return null;
        }
    }

    // todo: for later if we want to expand functionality
    public async Task<OpenDotaPlayerProfileDto?> GetPlayerProfileAsync(long accountId)
    {
        try
        {
            await _rateLimiter.WaitForSlotAsync();

            _logger.LogInformation("Fetching player profile for account {AccountId}", accountId);

            var client = _httpClientFactory.CreateClient("OpenDotaApi");
            var response = await client.GetAsync($"api/players/{accountId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch player profile for {AccountId}. Status: {StatusCode}",
                    accountId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var profile = JsonConvert.DeserializeObject<OpenDotaPlayerProfileDto>(content);

            _logger.LogInformation("Successfully fetched player profile for {AccountId}", accountId);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching player profile for {AccountId}", accountId);
            return null;
        }
    }
}