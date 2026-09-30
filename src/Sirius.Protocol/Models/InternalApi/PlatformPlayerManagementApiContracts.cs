namespace Sirius.Protocol.InternalApi;

public sealed class PlatformPlayerInventoryReply
{
    public long AccountId { get; set; }
    public PlatformPlayerInventoryItem[] Items { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformPlayerInventoryItem
{
    public int ThingType { get; set; }
    public string ThingTypeName { get; set; } = string.Empty;
    public long ThingId { get; set; }
    public long? InstanceId { get; set; }
    public long Quantity { get; set; }
    public long PaidQuantity { get; set; }
    public long FreeQuantity { get; set; }
    public int? Level { get; set; }
    public bool? Locked { get; set; }
    public long? AcquiredAt { get; set; }
    public string Source { get; set; } = string.Empty;
}

public sealed class PlatformRemoveInventoryItemRequest
{
    public long AccountId { get; set; }
    public string OperationId { get; set; } = string.Empty;
    public int ThingType { get; set; }
    public long ThingId { get; set; }
    public long Quantity { get; set; }
}

public sealed class PlatformRemoveInventoryItemReply
{
    public bool Succeeded { get; set; }
    public bool AlreadyProcessed { get; set; }
    public long AccountId { get; set; }
    public int ThingType { get; set; }
    public long ThingId { get; set; }
    public long RemovedQuantity { get; set; }
    public long RemovedPaidQuantity { get; set; }
    public long RemovedFreeQuantity { get; set; }
    public long RemainingQuantity { get; set; }
    public long RemainingPaidQuantity { get; set; }
    public long RemainingFreeQuantity { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformCharacterStarRankReply
{
    public long AccountId { get; set; }
    public PlatformCharacterStarRank[] Characters { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformCharacterStarRank
{
    public long CharacterBaseMasterId { get; set; }
    public long CharacterBaseId { get; set; }
    public int StarRank { get; set; }
    public int TotalStarPoint { get; set; }
    public int CharacterCount { get; set; }
    public int HighestLevel { get; set; }
    public int KeyMissionLevel { get; set; }
}

public sealed class PlatformPlayerProfileReply
{
    public bool Found { get; set; }
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public int Status { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public int PlayerRank { get; set; }
    public long PlayerExperience { get; set; }
    public int PlayerRankLimit { get; set; }
    public double PlayerRate { get; set; }
    public double MaxPlayerRate { get; set; }
    public bool IsPublicPlayerRate { get; set; }
    public int CurrentStamina { get; set; }
    public long Coin { get; set; }
    public long FreeJewel { get; set; }
    public long PaidJewel { get; set; }
    public int CharacterCount { get; set; }
    public int PosterCount { get; set; }
    public int AccessoryCount { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
    public long? LastLoginAt { get; set; }
    public int ContentLanguage { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformPlayerRankingReply
{
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public PlatformPlayerRankingEntry[] Entries { get; set; } = [];
}

public sealed class PlatformPlayerRankingEntry
{
    public int Position { get; set; }
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int PlayerRank { get; set; }
    public double PlayerRate { get; set; }
    public double MaxPlayerRate { get; set; }
    public bool IsPublicPlayerRate { get; set; }
    public long? LastLoginAt { get; set; }
}

public sealed class PlatformScorePageReply
{
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public PlatformScoreEntry[] Entries { get; set; } = [];
}

public sealed class PlatformScoreEntry
{
    public long ResultId { get; set; }
    public long LiveId { get; set; }
    public long MusicId { get; set; }
    public string MusicTitle { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public long Score { get; set; }
    public int MaxCombo { get; set; }
    public bool IsCleared { get; set; }
    public int ClearLamp { get; set; }
    public int RateGrade { get; set; }
    public double AchievementRate { get; set; }
    public double LiveRate { get; set; }
    public double ContributedRating { get; set; }
    public bool IsBest30 { get; set; }
    public long AcceptedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    // Per-attempt counts supplied by /scores; null when unavailable or a best snapshot.
    public int? PerfectStar { get; set; }
    public int? Perfect { get; set; }
    public int? Great { get; set; }
    public int? Good { get; set; }
    public int? Bad { get; set; }
    public int? Miss { get; set; }
}

public sealed class PlatformBest30Reply
{
    public double TotalRating { get; set; }
    public PlatformScoreEntry[] Entries { get; set; } = [];
}
