namespace RunbookPlatform.Api.Domain;

// One ordered instruction in a Runbook's editable working set (glossary).
// Text is the required title; the detail (Instructions/Command/ExpectedResult)
// and StepType are optional/defaulted (FR-001/003). Constructed only via
// Runbook.ReplaceSteps (ADR-0003).
public class Step
{
    private Step() { } // EF Core

    internal Step(Guid id, Guid runbookId, int position, string text,
        string? instructions, string? command, string? expectedResult, StepType type)
    {
        Id = id;
        RunbookId = runbookId;
        Position = position;
        Text = text;
        Instructions = instructions;
        Command = command;
        ExpectedResult = expectedResult;
        Type = type;
    }

    public Guid Id { get; private set; }
    public Guid RunbookId { get; private set; }
    public int Position { get; private set; }
    public string Text { get; private set; } = null!;
    public string? Instructions { get; private set; }
    public string? Command { get; private set; }
    public string? ExpectedResult { get; private set; }
    public StepType Type { get; private set; }
}
