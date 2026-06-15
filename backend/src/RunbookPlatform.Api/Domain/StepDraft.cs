namespace RunbookPlatform.Api.Domain;

// One authored Step's content, handed to Runbook.ReplaceSteps. Title (Text) is
// required; the detail is optional (FR-001/003). Not persisted — it is the input
// shape the aggregate turns into Step / RunbookVersionStep rows.
public record StepDraft(
    string Text,
    string? Instructions = null,
    string? Command = null,
    string? ExpectedResult = null,
    StepType Type = StepType.Action);
