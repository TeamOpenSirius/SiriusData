namespace Sirius.Protocol.InternalApi;

public sealed class PlatformPhotoPageReply
{
    public bool Found { get; set; }
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
    public int TotalCount { get; set; }
    public PlatformPhotoSummary[] Photos { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformPhotoSummary
{
    public long PhotoId { get; set; }
    public long? PhotoSpotMasterId { get; set; }
    public long? PhotoEffectMasterId { get; set; }
    public bool Locked { get; set; }
    public int? UseAlbumPage { get; set; }
    public int Level { get; set; }
    public int Rarity { get; set; }
    public long? SignMasterId { get; set; }
    public long GeneratedAt { get; set; }
    public long[] AppearedCharacterBaseMasterIds { get; set; } = [];
    public long[] TaggedCharacterBaseMasterIds { get; set; } = [];
    public int UseDecoPage { get; set; }
    public bool ThumbnailAvailable { get; set; }
    public bool OriginalAvailable { get; set; }
}

public sealed class PlatformPhotoThumbnailBatchRequest
{
    public long[] PhotoIds { get; set; } = [];
}

public sealed class PlatformPhotoThumbnailBatchReply
{
    public bool Found { get; set; }
    public long AccountId { get; set; }
    public PlatformPhotoThumbnail[] Thumbnails { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformPhotoThumbnail
{
    public long PhotoId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
}
