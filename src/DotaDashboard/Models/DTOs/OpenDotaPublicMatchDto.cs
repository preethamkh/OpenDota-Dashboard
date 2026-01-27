using Newtonsoft.Json;

namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for public matches from OpenDota API (/publicMatches endpoint)
/// </summary>
public class OpenDotaPublicMatchDto
{
    [JsonProperty("match_id")]
    public long MatchId { get; set; }

    [JsonProperty("start_time")]
    public long StartTime { get; set; }

    [JsonProperty("duration")]
    public int Duration { get; set; }

    [JsonProperty("radiant_win")]
    public bool RadiantWin { get; set; }

    [JsonProperty("avg_mmr")]
    public int? AvgMmr { get; set; }

    [JsonProperty("num_mmr")]
    public int? NumMmr { get; set; }
}