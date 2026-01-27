namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for hero data from OpenDota API
/// </summary>
public class OpenDotaHeroDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LocalizedName { get; set; } = string.Empty;
    public string Img { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
