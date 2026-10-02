namespace Sirius.Protocol.InternalApi;

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

/// <summary>
/// Where an import job obtains the official <c>api/data/user</c> response.
/// </summary>
public static class OfficialUserImportSources
{
    /// <summary>The game server downloads the response from the official API.</summary>
    public const string OfficialApi = "official_api";

    /// <summary>The caller uploaded a captured response body, or reused a cached one.</summary>
    public const string UploadedFile = "uploaded_file";
}
