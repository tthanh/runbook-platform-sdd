namespace RunbookPlatform.Api.Domain;

// The authored, evolving thing — not a single run of it (glossary).
// Aggregate root: every state change is a behavior method enforcing its own
// invariants (ADR-0003); endpoints hold no domain rules.
public class Runbook
{
    private readonly List<Step> _steps = [];
    private readonly List<RunbookVersion> _versions = [];

    private Runbook() { } // EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<Step> Steps => _steps;
    public IReadOnlyList<RunbookVersion> Versions => _versions;

    // FR-008: "current" is the highest published number — derived, not stored.
    public int? CurrentVersionNumber =>
        _versions.Count == 0 ? null : _versions.Max(v => v.Number);

    // FR-001: a Runbook exists only with a non-empty name.
    public static Runbook Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("A name is required.");

        return new Runbook
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    // FR-002 (research R5): full ordered replacement of the working Steps.
    // Never touches published Runbook Versions (FR-006).
    public void ReplaceSteps(IEnumerable<string> texts)
    {
        var list = texts.ToList();
        if (list.Any(string.IsNullOrWhiteSpace))
            throw new DomainException("A Step needs content.");

        _steps.Clear();
        _steps.AddRange(list.Select((text, i) =>
            new Step(Guid.NewGuid(), Id, i + 1, text.Trim())));
    }

    // FR-003 publish gate; FR-004 freeze; FR-005 sequential number (ADR-0001).
    public RunbookVersion Publish()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("A name is required.");
        if (_steps.Count == 0)
            throw new DomainException("At least one Step is required.");

        var version = RunbookVersion.Freeze(this, (CurrentVersionNumber ?? 0) + 1);
        _versions.Add(version);
        return version;
    }
}
