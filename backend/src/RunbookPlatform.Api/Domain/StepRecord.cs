namespace RunbookPlatform.Api.Domain;

// Append-only captured outcome of one Step action during an Execution.
// Constructed only by Execution.RecordStep — no public mutation (FR-005, FR-008).
public class StepRecord
{
    private StepRecord() { } // EF Core

    internal StepRecord(Guid id, Guid executionId, int stepPosition, StepOutcome outcome, string? note, DateTimeOffset recordedAt, int sequence)
    {
        Id = id;
        ExecutionId = executionId;
        StepPosition = stepPosition;
        Outcome = outcome;
        Note = note;
        RecordedAt = recordedAt;
        Sequence = sequence;
    }

    public Guid Id { get; private set; }
    public Guid ExecutionId { get; private set; }
    public int StepPosition { get; private set; }
    public StepOutcome Outcome { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    // Per-Execution monotonic tiebreaker for in-memory ordering (R6).
    public int Sequence { get; private set; }
}
