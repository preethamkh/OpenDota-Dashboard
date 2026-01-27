namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for player data within a match
/// </summary>
public class OpenDotaPlayerDto
{
    public long AccountId { get; set; }
    public int HeroId { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int PlayerSlot { get; set; }
}
