namespace RunbookPlatform.Api.Domain;

// An immutable snapshot frozen at publish (glossary). Never updated or
// deleted after insert — no code path exists (FR-006, FR-011).
public class RunbookVersion
{
    public Guid Id { get; set; }
    public Guid RunbookId { get; set; }
    // Per-Runbook sequential, starts at 1 (ADR-0001); unique (RunbookId, Number).
    public int Number { get; set; }
    public required string NameAtPublish { get; set; }
    public DateTimeOffset PublishedAt { get; set; }

    public List<RunbookVersionStep> Steps { get; set; } = [];
}
