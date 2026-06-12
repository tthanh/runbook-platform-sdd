namespace RunbookPlatform.Api.Domain;

// The authored, evolving thing — not a single run of it (glossary).
public class Runbook
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<Step> Steps { get; set; } = [];
    public List<RunbookVersion> Versions { get; set; } = [];
}
