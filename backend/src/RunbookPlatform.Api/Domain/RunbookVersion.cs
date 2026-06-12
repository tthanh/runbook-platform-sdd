namespace RunbookPlatform.Api.Domain;

// An immutable snapshot frozen at publish (glossary). Constructed only by
// Runbook.Publish(); no mutation exists after construction (FR-006, FR-011,
// ADR-0003). Per-Runbook sequential Number starts at 1 (ADR-0001); the
// database enforces unique (RunbookId, Number).
public class RunbookVersion
{
    private readonly List<RunbookVersionStep> _steps = [];

    private RunbookVersion() { } // EF Core

    public Guid Id { get; private set; }
    public Guid RunbookId { get; private set; }
    public int Number { get; private set; }
    public string NameAtPublish { get; private set; } = null!;
    public DateTimeOffset PublishedAt { get; private set; }

    public IReadOnlyList<RunbookVersionStep> Steps => _steps;

    internal static RunbookVersion Freeze(Runbook runbook, int number)
    {
        var version = new RunbookVersion
        {
            Id = Guid.NewGuid(),
            RunbookId = runbook.Id,
            Number = number,
            NameAtPublish = runbook.Name,
            PublishedAt = DateTimeOffset.UtcNow,
        };
        version._steps.AddRange(runbook.Steps
            .OrderBy(s => s.Position)
            .Select(s => new RunbookVersionStep(Guid.NewGuid(), version.Id, s.Position, s.Text)));
        return version;
    }
}
