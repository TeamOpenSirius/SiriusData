using System.Text.Json;
using Sirius.Protocol.InternalApi;
using Xunit;

namespace Sirius.Protocol.Tests;

public sealed class SongExplorerContractTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string LegacySong = """
        {"id":"1","title":"Song","pronunciation":"song","composer":"Composer",
         "lyricist":"","arranger":"","singer":"Singer","coverUrl":"https://cdn.example/cover.webp",
         "coverType":"original","isLongVersion":false,"durationSeconds":120,"releasedAt":null,
         "knownTimesCompletedSum":0,"countCoverage":"local","charts":[],"matchedChartKeys":[]}
        """;

    [Theory]
    [InlineData("")]
    [InlineData(",\"coverFallbackUrl\":null")]
    public void Legacy_or_null_fallback_keeps_the_primary_cover(string additionalField)
    {
        var payload = LegacySong.TrimEnd().TrimEnd('}') + additionalField + "}";
        var song = JsonSerializer.Deserialize<PlatformSongSummary>(payload, Json)!;

        Assert.Equal("https://cdn.example/cover.webp", song.CoverUrl);
        Assert.Null(song.CoverFallbackUrl);
        Assert.Null(song.ReleasedAt);
        Assert.Equal(0, song.KnownTimesCompletedSum);
    }

    [Fact]
    public void Pinned_fallback_round_trips_in_page_and_detail_web_json()
    {
        const string fallback = "https://raw.githubusercontent.com/example/covers/0123456789abcdef0123456789abcdef01234567/cover.webp";
        var coverage = new PlatformSongSourceCoverage("available", "unavailable", "local", []);
        var chart = new PlatformSongChartSummary("normal:1", "normal", "1", "1", 1, "Normal",
            "1", null, "achievement", null, new("none", null, null, 0, null, coverage));
        var song = new PlatformSongSummary("1", "Song", "song", "Composer", "", "", "Singer",
            "https://cdn.example/cover.webp", "original", false, 120, null, 0, "local", [chart], [])
        {
            CoverFallbackUrl = fallback
        };
        var page = new PlatformSongExplorerPage([song], null, 1, "1", "catalog-1", "score-1", coverage);
        var detail = new PlatformSongChartDetail(song, chart, "unavailable", null, null, null,
            "1", "catalog-1", "score-1");
        var pageJson = JsonSerializer.Serialize(page, Json);
        var detailJson = JsonSerializer.Serialize(detail, Json);

        using var document = JsonDocument.Parse(pageJson);
        var encodedSong = document.RootElement.GetProperty("items")[0];
        Assert.Equal(fallback, encodedSong.GetProperty("coverFallbackUrl").GetString());
        Assert.False(encodedSong.TryGetProperty("CoverFallbackUrl", out _));
        Assert.Equal(fallback, JsonSerializer.Deserialize<PlatformSongExplorerPage>(pageJson, Json)!.Items[0].CoverFallbackUrl);
        Assert.Equal(fallback, JsonSerializer.Deserialize<PlatformSongChartDetail>(detailJson, Json)!.Song.CoverFallbackUrl);
    }
}
