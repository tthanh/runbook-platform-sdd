namespace RunbookPlatform.Api.Domain;

// The Step as captured inside a published Runbook Version (data-model.md:
// not a fourth domain concept — the frozen copy immutability requires).
// Constructed only by RunbookVersion.Freeze (ADR-0003).
public class RunbookVersionStep
{
    private RunbookVersionStep() { } // EF Core

    internal RunbookVersionStep(Guid id, Guid runbookVersionId, int position, string text)
    {
        Id = id;
        RunbookVersionId = runbookVersionId;
        Position = position;
        Text = text;
    }

    public Guid Id { get; private set; }
    public Guid RunbookVersionId { get; private set; }
    public int Position { get; private set; }
    public string Text { get; private set; } = null!;
}
