using System.Text.Json;
using Sirius.Protocol.InternalApi;
using Xunit;

namespace Sirius.Protocol.Tests;

public sealed class OfficialImportContractTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData(null, "incremental")]
    [InlineData("", "incremental")]
    [InlineData("unknown", "incremental")]
    [InlineData("incremental", "incremental")]
    [InlineData("new_account", "new_account")]
    [InlineData(" NEW_ACCOUNT ", "new_account")]
    public void Policy_matches_the_server_documented_fallback(string? input, string expected) =>
        Assert.Equal(expected, OfficialUserImportConflictPolicies.Normalize(input));

    [Fact]
    public void Legacy_request_keeps_unspecified_policy_and_no_target()
    {
        var request = JsonSerializer.Deserialize<PlatformCreateOfficialUserImportRequest>("""{"linkageId":"123","password":"fake"}""", Json)!;
        Assert.Equal("", request.ConflictPolicy);
        Assert.Equal(0, request.TargetAccountId);
        Assert.Equal("incremental", OfficialUserImportConflictPolicies.Normalize(request.ConflictPolicy));
    }

    [Fact]
    public void Incremental_request_and_reusable_job_status_round_trip()
    {
        var request = JsonSerializer.Deserialize<PlatformCreateOfficialUserImportRequest>("""{"linkageId":"123","password":"fake","conflictPolicy":"incremental","targetAccountId":81}""", Json)!;
        Assert.Equal(81, request.TargetAccountId);
        Assert.Equal("incremental", request.ConflictPolicy);
        var status = new PlatformOfficialUserImportStatusReply { ConflictPolicy = "new_account", Source = "uploaded_file", HasReusableRawData = true };
        var encoded = JsonSerializer.Serialize(status, Json);
        Assert.Contains("\"hasReusableRawData\":true", encoded);
        var decoded = JsonSerializer.Deserialize<PlatformOfficialUserImportStatusReply>(encoded, Json)!;
        Assert.Equal(status.ConflictPolicy, decoded.ConflictPolicy);
        Assert.Equal(status.Source, decoded.Source);
        Assert.True(decoded.HasReusableRawData);
    }
}
