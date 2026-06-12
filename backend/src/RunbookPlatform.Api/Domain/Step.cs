namespace RunbookPlatform.Api.Domain;

// One ordered instruction in a Runbook's editable working set (glossary).
// Constructed only via Runbook.ReplaceSteps (ADR-0003).
public class Step
{
    private Step() { } // EF Core

    internal Step(Guid id, Guid runbookId, int position, string text)
    {
        Id = id;
        RunbookId = runbookId;
        Position = position;
        Text = text;
    }

    public Guid Id { get; private set; }
    public Guid RunbookId { get; private set; }
    public int Position { get; private set; }
    public string Text { get; private set; } = null!;
}
