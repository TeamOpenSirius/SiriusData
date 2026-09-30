using System.Text.Json.Serialization;

namespace Sirius.Protocol.InternalApi;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformSetBestScoreRequest
{
    public required string OperationId { get; set; }
    public required string Reason { get; set; }
    public required long ExpectedRevision { get; set; }
    public required long BestScore { get; set; }
    public required int BestCombo { get; set; }
    public required double AchievementRate { get; set; }
    public required int ClearLamp { get; set; }
    // Olivier's accuracy component cannot be inferred from aggregate achievement.
    public int? AccuracyStar { get; set; }
}

public sealed record PlatformBestScoreReply
{
    public long AccountId { get; init; }
    public long MusicId { get; init; }
    public long LiveId { get; init; }
    public int Difficulty { get; init; }
    public bool HasRecord { get; init; }
    public long Revision { get; init; }
    public long BestScore { get; init; }
    public int BestCombo { get; init; }
    public double AchievementRate { get; init; }
    public int ClearLamp { get; init; }
    public int RateGrade { get; init; }
    public double NotationRate { get; init; }
    public int? AccuracyStar { get; init; }
    public int? SpPoint { get; init; }
    public double PlayerRate { get; init; }
    public double MaxPlayerRate { get; init; }
    public int TotalSpCount { get; init; }
    public long UpdatedAt { get; init; }
    public bool AlreadyProcessed { get; init; }
}
