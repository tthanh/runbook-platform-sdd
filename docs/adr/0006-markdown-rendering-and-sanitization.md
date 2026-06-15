# 0006 — Markdown for the instructions body: hand-rolled, escape-first renderer

## Context

Workshop hotspot **H2** (initiative 04). A Step's **instructions** body is authored
in lightweight markdown and rendered when read (PRD-04 decision, 2026-06-15;
spec FR-004/006). Markdown must be displayed as formatted text, which means turning
author-supplied source into markup — the classic injection surface. The spec
requires safe rendering (FR-006): displaying a Step must never execute or inject
active content.

Two facts shape the choice. First, the frontend has **zero runtime dependencies**
beyond react/react-dom, and an earlier decision kept even routing dependency-free
(C-003, no router package) — a deliberate minimal-deps stance. Second, the platform
is **single-user, local, and demonstrative**: the author and the responder are the
same trusted person, so the threat model is far lower than a multi-tenant SaaS.

Appetite: a couple of evenings. The instructions are short operational text — a few
emphasised words, a command, a link, a short list — not documents.

## Options considered

1. **Hand-rolled, escape-first minimal renderer (in-repo).** A small frontend module
   escapes all HTML special characters first, then applies a fixed whitelist of
   markdown rules (bold, italic, inline code, fenced code block, links with safe
   schemes, unordered/ordered lists, paragraphs/line breaks), emitting a safe HTML
   string. No dependency. Safe by construction — because the source is escaped before
   any rule runs, no raw HTML or `<script>` can survive; links are restricted to
   http/https/mailto so `javascript:` cannot. Limited feature set; we own its
   correctness.
2. **Vetted libraries (markdown-it + DOMPurify).** A battle-tested parser plus an
   HTML sanitizer. Most robust, handles every edge case. Rejected: introduces the
   first runtime dependencies beyond React, a bundle and supply-chain surface, and
   ongoing version maintenance — against the established minimal-deps grain, for a
   short-text use case a tiny renderer covers.
3. **Server-side render at publish.** Convert markdown → sanitized HTML once and
   freeze the HTML into the Version. Rejected: it stores *rendered* HTML rather than
   the source markup, muddying the "freeze the author's source immutably" model;
   adds a backend markdown + sanitizer dependency; and diverges from the thin-
   frontend-render pattern the SPA already uses.

## Decision

Adopt **option 1**. Render the instructions body with a small in-repo frontend module
(`frontend/src/lib/markdown.ts`) that **escapes all HTML first**, then applies a fixed
whitelist of lightweight markdown rules, then returns a safe HTML string for display.
No markdown or sanitizer dependency is added. Links are limited to http/https/mailto;
raw HTML, images, and tables are not supported. The **command** and **expected
result** are not markdown — they render verbatim (ADR scope note; FR-005).

(Human decision, 2026-06-15: hand-rolled minimal renderer chosen over a vetted-library
or server-side approach.)

## Trade-offs accepted

- **We own the renderer's correctness, including escaping.** Accepted: escape-first
  ordering makes injection structurally impossible for the supported subset, and the
  subset is small enough to test exhaustively. The security-critical rules — escape
  before transform, and a link-scheme allowlist — are explicitly covered by tests.
- **A limited markdown subset.** No images, tables, raw HTML, or headings. Accepted:
  operational instructions don't need them, and excluding images/raw-HTML removes the
  main injection vectors.
- **If instructions later grow into rich documents, the hand-rolled renderer will
  strain.** Accepted for this appetite; the flip condition covers it.

## Consequences

- A new file `frontend/src/lib/markdown.ts` is the single render path, reused by the
  version view, the Execution run view, and the Computed Review (FR-009/010/011).
- The frontend keeps its react/react-dom-only runtime footprint; C-003's no-extra-
  dependency posture extends to markdown.
- Tests must cover the safety invariants: escaped raw HTML, stripped/neutralised
  `<script>`, blocked `javascript:` links, and verbatim command rendering (quickstart
  Scenario D).
- The supported subset (R5) bounds what authors can rely on; documented for authors.

## Flip condition

If instructions outgrow the subset (authors need images, tables, or richer
formatting), or if maintaining the hand-rolled renderer's safety becomes a recurring
cost, supersede this ADR to adopt a vetted markdown + sanitizer dependency (option 2).
The escape-first safety principle does not flip; only the build-vs-buy choice does.

## Status + date

Proposed — 2026-06-15
