namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for player profile from OpenDota API
/// </summary>
public class OpenDotaPlayerProfileDto
{
    public long AccountId { get; set; }
    public string? PersonaName { get; set; }
    public string? AvatarFull { get; set; }
    public string? ProfileUrl { get; set; }
}
