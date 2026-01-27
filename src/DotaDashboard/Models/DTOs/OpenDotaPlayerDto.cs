using Newtonsoft.Json;

namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for player data within a match
/// </summary>
public class OpenDotaPlayerDto
{
    [JsonProperty("account_id")]
    public long AccountId { get; set; }

    [JsonProperty("hero_id")]
    public int HeroId { get; set; }

    [JsonProperty("kills")]
    public int Kills { get; set; }

    [JsonProperty("deaths")]
    public int Deaths { get; set; }

    [JsonProperty("assists")]
    public int Assists { get; set; }

    [JsonProperty("player_slot")]
    public int PlayerSlot { get; set; }
}