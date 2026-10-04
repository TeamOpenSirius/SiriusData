using System.ComponentModel.DataAnnotations;

namespace Sirius.Protocol.InternalApi;

// JSON-only platform contracts. These types deliberately do not implement IDataObject.
public sealed record PlatformSongExplorerQuery
{
    // Empty search is valid; MVC must not turn it into null before validation.
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Search { get; init; } = "";
    public int Difficulty { get; init; } = 1;
    public string ChartKind { get; init; } = "all";
    public string CoverType { get; init; } = "all";
    public string Length { get; init; } = "all";
    public string Sort { get; init; } = "title-asc";
    public string? Cursor { get; init; }
    public int Limit { get; init; } = 18;
}

public sealed record PlatformSongSourceCoverage(
    string Local, string Official, string Counts, string[] Warnings);
public sealed record PlatformSongChartPersonal(
    string RecordStatus, double? Achievement, double? MetricValue,
    long? TimesCompleted, int? HighestClearLamp, PlatformSongSourceCoverage SourceCoverage);
public sealed record PlatformSongNoteCount(string Kind, string Label, long Count);
public sealed record PlatformSongNoteStatistics(long ComboCount, PlatformSongNoteCount[] NoteCounts);
public sealed record PlatformSongChartSummary(
    string Key, string Kind, string Id, string MusicId, int Difficulty, string Label,
    string Level, string? SpecialType, string MetricKind,
    PlatformSongNoteStatistics? NoteStatistics, PlatformSongChartPersonal Personal)
{
    // Additive public-artifact identity. Older API responses may omit it.
    public string? ResourceKey { get; init; }
}
public sealed record PlatformSongSummary(
    string Id, string Title, string Pronunciation, string Composer, string Lyricist,
    string Arranger, string Singer, string? CoverUrl, string CoverType, bool IsLongVersion,
    int DurationSeconds, string? ReleasedAt, long KnownTimesCompletedSum, string CountCoverage,
    PlatformSongChartSummary[] Charts, string[] MatchedChartKeys)
{
    // Optional pinned GitHub fallback for the primary CDN CoverUrl.
    public string? CoverFallbackUrl { get; init; }
}
public sealed record PlatformSongExplorerPage(
    PlatformSongSummary[] Items, string? NextCursor, int TotalCount,
    string SchemaVersion, string CatalogVersion, string ScoreVersion,
    PlatformSongSourceCoverage SourceCoverage)
{
    public string StatisticsVersion { get; init; } = "0";
    public string ResourceVersion { get; init; } = "";
    public string CounterRulesVersion { get; init; } = "";
}
public sealed record PlatformSongChartDetail(
    PlatformSongSummary Song, PlatformSongChartSummary Chart,
    string StatisticsStatus, string? StatisticsSourceUrl, string? StatisticsContentHash,
    string? StatisticsErrorCode, string SchemaVersion, string CatalogVersion, string ScoreVersion)
{
    public string StatisticsVersion { get; init; } = "0";
    public string ResourceVersion { get; init; } = "";
    public string CounterRulesVersion { get; init; } = "";
}

public sealed record PlatformSongExplorerPublicSong(
    string Id, string Title, PlatformSongChartSummary[] Charts);
public sealed record PlatformSongExplorerPublicCatalog(
    string CatalogVersion, string ResourceVersion, string CounterRulesVersion,
    string SchemaVersion, PlatformSongExplorerPublicSong[] Songs, string ChartBaseUrl);
public sealed record PlatformSongPopulationValue(double Value, long Count);
public sealed record PlatformSongPopulationMetric(
    string Metric, bool Applicable, long SampleCount, long VisibleSampleCount,
    double? Mean, PlatformSongPopulationValue[] Values)
{
    public long NoRecordCount { get; init; }
    public long NoEligibleRecordCount { get; init; }
    public long UnavailableCount { get; init; }
}
public sealed record PlatformSongExplorerPopulationArtifact(
    string ChartKey, string SnapshotId, string CatalogVersion, string ScoreVersion,
    string SchemaVersion, string AsOf, string ExpiresAt,
    PlatformSongSourceCoverage SourceCoverage, PlatformSongPopulationMetric[] Metrics);
public sealed record PlatformSongExplorerPersonalChart(
    string ChartKey, PlatformSongChartPersonal Personal);
public sealed record PlatformSongExplorerPersonalProjection(
    string CatalogVersion, string ScoreVersion, string SchemaVersion,
    PlatformSongSourceCoverage SourceCoverage, PlatformSongExplorerPersonalChart[] Charts);
public sealed record PlatformSongDistributionRange(double Min, double Max);
public sealed record PlatformSongDistributionBin(double Lower, double Upper, long Count);
public sealed record PlatformSongDistributionQuery
{
    public string Metric { get; init; } = "achievement";
    public double? Min { get; init; }
    public double? Max { get; init; }
    public int Bins { get; init; } = 24;
    public string? SnapshotId { get; init; }
}
public sealed record PlatformSongDistribution(
    string ChartKey, string Metric, string Unit, bool Applicable, string SnapshotId,
    string AsOf, string ExpiresAt, PlatformSongDistributionRange Domain,
    PlatformSongDistributionRange Range, PlatformSongDistributionBin[] Bins,
    long SampleCount, long VisibleSampleCount, long NoRecordCount, long NoEligibleRecordCount,
    long UnavailableCount, long UnderflowCount, long OverflowCount, double? Mean, double? SelfValue,
    string SchemaVersion, string CatalogVersion, string ScoreVersion,
    PlatformSongSourceCoverage SourceCoverage);
public sealed record PlatformSongExplorerError(string Error, string? Detail = null);

public sealed record PlatformSongExplorerRebuildResult(
    bool DryRun, int Scanned, int Eligible, int Ineligible,
    IReadOnlyDictionary<string, int> Skipped, string NextAfterResultId, bool HasMore, string[] Warnings);
