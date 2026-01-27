using Newtonsoft.Json;

namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for hero data from OpenDota API
/// </summary>
public class OpenDotaHeroDto
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("localized_name")]
    public string LocalizedName { get; set; } = string.Empty;

    [JsonProperty("primary_attr")]
    public string? PrimaryAttr { get; set; }

    [JsonProperty("attack_type")]
    public string? AttackType { get; set; }

    [JsonProperty("roles")]
    public List<string>? Roles { get; set; }

    /// <summary>
    /// Constructs the image URL from the hero name.
    /// Example: "npc_dota_hero_antimage" -> "/apps/dota2/images/dota_react/heroes/antimage.png"
    /// </summary>
    public string GetImagePath()
    {
        if (string.IsNullOrEmpty(Name))
            return string.Empty;

        // Remove "npc_dota_hero_" prefix
        var heroName = Name.Replace("npc_dota_hero_", "");
        return $"/apps/dota2/images/dota_react/heroes/{heroName}.png";
    }
}