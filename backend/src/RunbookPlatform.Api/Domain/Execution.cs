namespace RunbookPlatform.Api.Domain;

// Aggregate root: invariants live here, endpoints stay thin (ADR-0003 / C-004).
public class Execution
{
    private readonly List<StepRecord> _stepRecords = [];

    private Execution() { } // EF Core

    public Guid Id { get; private set; }
    public string IncidentId { get; private set; } = null!;
    public string? IncidentTitle { get; private set; }
    public Guid PinnedRunbookVersionId { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    public IReadOnlyList<StepRecord> StepRecords => _stepRecords;

    // FR-001/002: pins current published Version; requires a published version (else DomainException).
    // IncidentId non-empty after trim (ADR-0005). Pin is set once and never reassigned (ADR-0004).
    public static Execution Start(Runbook runbook, string incidentId, string? incidentTitle)
    {
        if (runbook.CurrentVersionNumber is null)
            throw new DomainException("The Runbook must be published before it can be run.");

        var id = incidentId?.Trim();
        if (string.IsNullOrEmpty(id))
            throw new DomainException("An incident identifier is required.");

        var pinnedVersion = runbook.Versions
            .First(v => v.Number == runbook.CurrentVersionNumber.Value);

        return new Execution
        {
            Id = Guid.NewGuid(),
            IncidentId = id,
            IncidentTitle = string.IsNullOrWhiteSpace(incidentTitle) ? null : incidentTitle.Trim(),
            PinnedRunbookVersionId = pinnedVersion.Id,
            Status = ExecutionStatus.Open,
            StartedAt = DateTimeOffset.UtcNow,
        };
    }

    // FR-004/005/006/007: open-only, any order, append-only, re-marking allowed.
    // stepPosition must be 1..n of the pinned Version's Steps (validated at the endpoint seam).
    public StepRecord RecordStep(int stepPosition, StepOutcome outcome, string? note, int maxValidPosition)
    {
        if (Status == ExecutionStatus.Closed)
            throw new DomainException("This Execution is closed.");

        if (stepPosition < 1 || stepPosition > maxValidPosition)
            throw new DomainException($"Step position {stepPosition} does not exist in the pinned Version.");

        var record = new StepRecord(
            Guid.NewGuid(),
            Id,
            stepPosition,
            outcome,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            DateTimeOffset.UtcNow,
            _stepRecords.Count + 1);

        _stepRecords.Add(record);
        return record;
    }

    // FR-009/010: Open → Closed, one-way; already-closed throws DomainException.
    public void Close()
    {
        if (Status == ExecutionStatus.Closed)
            throw new DomainException("This Execution is already closed.");

        Status = ExecutionStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;
    }
}

public enum ExecutionStatus
{
    Open,
    Closed,
}
