namespace Sirius.Protocol.InternalApi;

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
