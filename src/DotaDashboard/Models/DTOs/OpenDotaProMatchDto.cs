namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO for professional matches from OpenDota API
/// </summary>
//todo: might not have access to this anymore
public class OpenDotaProMatchDto
{
    public long MatchId { get; set; }
    public long StartTime { get; set; }
    public int Duration { get; set; }
    public bool RadiantWin { get; set; }
}
