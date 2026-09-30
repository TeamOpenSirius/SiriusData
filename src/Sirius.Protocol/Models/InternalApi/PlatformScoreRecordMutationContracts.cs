using System.Text.Json.Serialization;

namespace Sirius.Protocol.InternalApi;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformClearScoresRequest
{
    public required string OperationId { get; set; }
    public required string Reason { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformAddScoreRequest
{
    public required string OperationId { get; set; }
    public required string Reason { get; set; }
    public required long PartyId { get; set; }
    public required long LiveSettingMasterId { get; set; }
    public bool UseStamina { get; set; } = true;
    public int StaminaConsumptionRatio { get; set; } = 1;
    public bool IsStoryEventChallenge { get; set; }
    public required long Score { get; set; }
    public long SenseScore { get; set; }
    public long StarActScore { get; set; }
    public int StarActCount { get; set; }
    public required int MaxCombo { get; set; }
    public required int FinalLife { get; set; }
    public required PlatformScoreJudgements Judgements { get; set; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformScoreJudgements
{
    public required int PerfectStar { get; set; }
    public required int Perfect { get; set; }
    public required int Great { get; set; }
    public required int Good { get; set; }
    public required int Bad { get; set; }
    public required int Miss { get; set; }
}

public sealed record PlatformScoreReward(int ThingType, long ThingId, int Quantity);

public sealed record PlatformScoreMutationReply
{
    public long AccountId { get; init; }
    public long MusicId { get; init; }
    public int Difficulty { get; init; }
    public long? ResultId { get; init; }
    public long ClearedRecords { get; init; }
    public double PlayerRate { get; init; }
    public double MaxPlayerRate { get; init; }
    public int TotalSpCount { get; init; }
    public int PlayerRank { get; init; }
    public int CurrentStamina { get; init; }
    public PlatformScoreReward[] Rewards { get; init; } = [];
    public bool AlreadyProcessed { get; init; }
}
