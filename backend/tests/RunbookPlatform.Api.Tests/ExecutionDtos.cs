namespace RunbookPlatform.Api.Tests;

public record ExecutionDto(
    Guid Id,
    string IncidentId,
    string? IncidentTitle,
    string Status,
    string RunbookName,
    int? PinnedVersionNumber,
    List<StepDto> Steps,
    List<StepRecordDto> Records);

public record StepRecordDto(int StepPosition, string Outcome, string? Note, DateTimeOffset RecordedAt);

public record ReviewDto(
    string Incident,
    string RunbookName,
    int PinnedVersionNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset? ClosedAt,
    List<TimelineEntryDto> Timeline,
    List<CoverageEntryDto> Coverage);

public record TimelineEntryDto(int StepPosition, string StepText, string Outcome, string? Note, DateTimeOffset RecordedAt);
public record CoverageEntryDto(int StepPosition, string EndState);
