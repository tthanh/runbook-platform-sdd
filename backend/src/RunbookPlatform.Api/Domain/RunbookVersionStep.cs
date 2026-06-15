namespace RunbookPlatform.Api.Domain;

// The Step as frozen inside a published Runbook Version (data-model.md): the
// immutable copy. Carries the same content as the working Step — title, optional
// detail, and Step Type — frozen at publish (FR-007). Constructed only by
// RunbookVersion.Freeze (ADR-0003).
public class RunbookVersionStep
{
    private RunbookVersionStep() { } // EF Core

    internal RunbookVersionStep(Guid id, Guid runbookVersionId, int position, string text,
        string? instructions, string? command, string? expectedResult, StepType type)
    {
        Id = id;
        RunbookVersionId = runbookVersionId;
        Position = position;
        Text = text;
        Instructions = instructions;
        Command = command;
        ExpectedResult = expectedResult;
        Type = type;
    }

    public Guid Id { get; private set; }
    public Guid RunbookVersionId { get; private set; }
    public int Position { get; private set; }
    public string Text { get; private set; } = null!;
    public string? Instructions { get; private set; }
    public string? Command { get; private set; }
    public string? ExpectedResult { get; private set; }
    public StepType Type { get; private set; }
}
