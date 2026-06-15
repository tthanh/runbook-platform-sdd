namespace RunbookPlatform.Api.Tests;

// 004 response shapes: the shared per-Step shape carries title + optional detail
// + Step Type (contracts/http-api.md). Separate from the slice-01/03 DTOs so
// those tests stay unmodified.
public record RichStepDto(
    int Position,
    string Text,
    string? Instructions,
    string? Command,
    string? ExpectedResult,
    string Type);

public record RichRunbookDto(
    Guid Id,
    string Name,
    List<RichStepDto> Steps,
    int? CurrentVersionNumber,
    List<VersionSummaryDto> Versions);

public record RichVersionDto(
    int Number,
    string NameAtPublish,
    DateTimeOffset PublishedAt,
    List<RichStepDto> Steps);

public record RichExecutionDto(
    Guid Id,
    string IncidentId,
    string? IncidentTitle,
    string Status,
    string RunbookName,
    int? PinnedVersionNumber,
    List<RichStepDto> Steps,
    List<StepRecordDto> Records);

public record RichReviewDto(
    string Incident,
    string RunbookName,
    int PinnedVersionNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset? ClosedAt,
    List<RichTimelineEntryDto> Timeline,
    List<CoverageEntryDto> Coverage);

public record RichTimelineEntryDto(
    int StepPosition,
    string StepText,
    string? Instructions,
    string? Command,
    string? ExpectedResult,
    string? Type,
    string Outcome,
    string? Note,
    DateTimeOffset RecordedAt);
