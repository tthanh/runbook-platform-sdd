namespace RunbookPlatform.Api.Domain;

// One ordered instruction in a Runbook's editable working set (glossary).
public class Step
{
    public Guid Id { get; set; }
    public Guid RunbookId { get; set; }
    public int Position { get; set; }
    public required string Text { get; set; }
}
