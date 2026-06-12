namespace RunbookPlatform.Api.Domain;

// An aggregate invariant was violated (ADR-0003). Mapped to the
// { "error": "…" } 400 shape in exactly one place: the endpoint-group filter.
public class DomainException(string message) : Exception(message);
