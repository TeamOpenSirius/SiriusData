namespace Sirius.Protocol.InternalApi;

public sealed class PlatformAuthenticateTakeOverRequest
{
    public string LinkageCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformAuthenticateTakeOverReply
{
    public bool Succeeded { get; set; }
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformMailItemRequest
{
    public int ThingType { get; set; }
    public long ThingId { get; set; }
    public int Quantity { get; set; }
}

public sealed class PlatformSendMailItemsRequest
{
    public string TargetType { get; set; } = "Player";
    public long? TargetPlayerId { get; set; }
    public string OperationId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string SenderName { get; set; } = "GameMaster";
    public int ExpiresInDays { get; set; } = 30;
    public PlatformMailItemRequest[] Items { get; set; } = [];
}

public sealed class PlatformSendMailItemsReply
{
    public bool Succeeded { get; set; }
    public bool AlreadyProcessed { get; set; }
    public long AccountId { get; set; }
    public long[] AccountIds { get; set; } = [];
    public long[] MessageIds { get; set; } = [];
    public string ErrorCode { get; set; } = string.Empty;
}
