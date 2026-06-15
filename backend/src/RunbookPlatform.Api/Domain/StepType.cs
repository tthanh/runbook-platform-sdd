namespace RunbookPlatform.Api.Domain;

// The classification of a Step by the kind of work it represents (glossary;
// ADR-0007). Descriptive only — it does not change a Step's required fields or
// how it is executed (FR-012). Decision and Gate are deferred with branching.
public enum StepType
{
    Action,
    Check,
}
