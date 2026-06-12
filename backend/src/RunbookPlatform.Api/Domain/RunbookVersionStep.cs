namespace RunbookPlatform.Api.Domain;

// The Step as captured inside a published Runbook Version (data-model.md:
// not a fourth domain concept — the frozen copy immutability requires).
public class RunbookVersionStep
{
    public Guid Id { get; set; }
    public Guid RunbookVersionId { get; set; }
    public int Position { get; set; }
    public required string Text { get; set; }
}
