namespace Sirius.Protocol.InternalApi;

public sealed class PlatformRegisterAccountRequest
{
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class PlatformRegisterPlayableAccountRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformRegisterPlayableAccountReply
{
    public bool Succeeded { get; set; }
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string LoginToken { get; set; } = string.Empty;
    public string LinkageCode { get; set; } = string.Empty;
    public string TakeOverPassword { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformRegisterAccountReply
{
    public bool Succeeded { get; set; }
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string LoginToken { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformIssueTakeOverRequest
{
    public long AccountId { get; set; }
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformIssueTakeOverReply
{
    public bool Succeeded { get; set; }
    public string LinkageCode { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformIssueTemporaryTakeOverReply
{
    public bool Succeeded { get; set; }
    public string LinkageCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
}

public static class OfficialUserImportConflictPolicies
{
    public const string Incremental = "incremental";
    public const string NewAccount = "new_account";

    public static string Normalize(string? value) =>
        string.Equals(value?.Trim(), NewAccount, StringComparison.OrdinalIgnoreCase) ? NewAccount : Incremental;
}

public sealed class PlatformCreateOfficialUserImportRequest
{
    public string LinkageId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // Empty preserves legacy requests; the server normalizes it to incremental.
    public string ConflictPolicy { get; set; } = string.Empty;
    public long TargetAccountId { get; set; }
}

public sealed class PlatformCreateOfficialUserImportReply
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Reused { get; set; }
}

public sealed class PlatformOfficialUserImportStatusReply
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public int Progress { get; set; }
    public long? AccountId { get; set; }
    public long? PublicUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int? PlayerRank { get; set; }
    public string LinkageId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorDetail { get; set; } = string.Empty;
    public string RawSha256 { get; set; } = string.Empty;
    public int? ObjectCount { get; set; }
    public int UnsupportedObjectCount { get; set; }
    public string WarningCode { get; set; } = string.Empty;
    public string WarningDetail { get; set; } = string.Empty;
    public bool? BinaryMatched { get; set; }
    public long CreatedAt { get; set; }
    public long? StartedAt { get; set; }
    public long? CompletedAt { get; set; }
    public string ConflictPolicy { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool HasReusableRawData { get; set; }
}

public sealed class PlatformOfficialUserImportJobPage
{
    public PlatformOfficialUserImportStatusReply[] Items { get; set; } = [];
    public int Offset { get; set; }
    public int Limit { get; set; }
    public long TotalCount { get; set; }
}

public sealed class PlatformBanAccountRequest
{
    public long AccountId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class PlatformBanAccountReply
{
    public bool Succeeded { get; set; }
    public bool AlreadyBanned { get; set; }
    public int RevokedSessionCount { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformDeleteAccountRequest
{
    public long AccountId { get; set; }
}

public sealed class PlatformDeleteAccountReply
{
    public bool Succeeded { get; set; }
    public bool AlreadyDeleted { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
}

public sealed class PlatformManagedGameAccount
{
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public int PlayerRank { get; set; }
    public long PlayerExperience { get; set; }
    public int CurrentStamina { get; set; }
    public int Status { get; set; }
    public bool HasTakeOver { get; set; }
    public string LinkageCode { get; set; } = string.Empty;
    public bool IsOfficialImported { get; set; }
    public long? OfficialPublicUserId { get; set; }
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }
    public long? LastLoginAt { get; set; }
    public int ContentLanguage { get; set; }
}

public sealed class PlatformManagedGameAccountPage
{
    public PlatformManagedGameAccount[] Items { get; set; } = [];
    public int Offset { get; set; }
    public int Limit { get; set; }
    public long TotalCount { get; set; }
}

public sealed class PlatformUpdateGameAccountRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
}

public sealed class PlatformUpdateContentLanguageRequest
{
    public int ContentLanguage { get; set; }
}
