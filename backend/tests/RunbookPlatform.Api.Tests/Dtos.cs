namespace RunbookPlatform.Api.Tests;

// Response shapes per contracts/http-api.md.
public record RunbookDto(
    Guid Id,
    string Name,
    List<StepDto> Steps,
    int? CurrentVersionNumber,
    List<VersionSummaryDto> Versions);

public record StepDto(int Position, string Text);

public record VersionSummaryDto(int Number, DateTimeOffset PublishedAt);

public record VersionDto(
    int Number,
    string NameAtPublish,
    DateTimeOffset PublishedAt,
    List<StepDto> Steps);

public record RunbookListItemDto(Guid Id, string Name, int? CurrentVersionNumber);

public record ErrorDto(string Error);
