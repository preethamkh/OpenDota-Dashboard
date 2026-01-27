using Newtonsoft.Json;

namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for player profile from OpenDota API
/// </summary>
public class OpenDotaPlayerProfileDto
{
    [JsonProperty("account_id")]
    public long AccountId { get; set; }

    [JsonProperty("personaname")]
    public string? PersonaName { get; set; }

    [JsonProperty("avatarfull")]
    public string? AvatarFull { get; set; }

    [JsonProperty("profileurl")]
    public string? ProfileUrl { get; set; }
}