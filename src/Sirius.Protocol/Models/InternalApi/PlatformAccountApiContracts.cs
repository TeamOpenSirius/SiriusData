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

/// <summary>
/// Accepted values for <see cref="PlatformCreateOfficialUserImportRequest.ConflictPolicy"/>.
/// An unrecognized or empty value is treated as <see cref="Incremental"/>, so existing callers
/// keep the previous behaviour.
/// </summary>
public static class OfficialUserImportConflictPolicies
{
    /// <summary>
    /// Reuse the private account already bound to the official account and replace its state.
    /// A public user id that belongs to an account which was not created by an official import
    /// remains a hard conflict.
    /// </summary>
    public const string Incremental = "incremental";

    /// <summary>
    /// Never reuse an existing private account. One more private account is created, and it
    /// receives a fresh public user id when the official public user id is already taken.
    /// </summary>
    public const string NewAccount = "new_account";

    /// <summary>
    /// Normalizes a caller supplied policy name. Unknown values fall back to
    /// <see cref="Incremental"/>.
    /// </summary>
    public static string Normalize(string? value) =>
        value?.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant() switch
        {
            NewAccount or "newaccount" or "new" or "allocate_new_user_id" => NewAccount,
            _ => Incremental
        };
}

public sealed class PlatformCreateOfficialUserImportRequest
{
    public string LinkageId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// How an official account that is already bound to a private account is resolved.
    /// <see cref="OfficialUserImportConflictPolicies.Incremental"/> (the default when the value is
    /// empty) refreshes the private account that is already bound to the official account.
    /// <see cref="OfficialUserImportConflictPolicies.NewAccount"/> creates one more private
    /// account instead, and allocates a new public user id when the official public user id is
    /// already taken by another account.
    /// </summary>
    public string ConflictPolicy { get; set; } = string.Empty;

    /// <summary>
    /// Optional private account that an incremental import must refresh. It removes the ambiguity
    /// once several private accounts were imported from the same official account.
    /// </summary>
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
    /// <summary>See <see cref="OfficialUserImportConflictPolicies"/>.</summary>
    public string ConflictPolicy { get; set; } = string.Empty;
    /// <summary>See <see cref="OfficialUserImportSources"/>.</summary>
    public string Source { get; set; } = string.Empty;
    /// <summary>Whether the raw response is cached and reusable by a retry.</summary>
    public bool HasReusableRawData { get; set; }
    public long CreatedAt { get; set; }
    public long? StartedAt { get; set; }
    public long? CompletedAt { get; set; }
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
