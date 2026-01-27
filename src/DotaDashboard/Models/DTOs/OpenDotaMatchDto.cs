namespace DotaDashboard.Models.DTOs;

/// <summary>
/// DTO representing a match from the OpenDota API.
/// </summary>
public class OpenDotaMatchDto
{
    public long MatchId { get; set; }
    public long StartTime { get; set; }
    public int Duration { get; set; }
    public bool RadiantWin { get; set; }
    public List<OpenDotaPlayerDto>? Players { get; set; }
}
