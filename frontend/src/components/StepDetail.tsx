import { renderMarkdown } from '../lib/markdown'

// 004 FR-004/005: render a Step's frozen detail on every read surface (version
// view, run view, Computed Review). Instructions go through the escape-first
// markdown renderer (ADR-0006); command and expected result are verbatim.
export function StepDetail({
  instructions,
  command,
  expectedResult,
}: {
  instructions: string | null
  command: string | null
  expectedResult: string | null
}) {
  if (!instructions && !command && !expectedResult) return null
  return (
    <div className="step-detail">
      {instructions && (
        <div
          className="step-detail-instructions"
          // Safe: renderMarkdown escapes all HTML first (ADR-0006).
          dangerouslySetInnerHTML={{ __html: renderMarkdown(instructions) }}
        />
      )}
      {command && (
        <pre className="step-detail-command">
          <code>{command}</code>
        </pre>
      )}
      {expectedResult && (
        <p className="step-detail-expected">
          <span className="step-detail-label">Expected:</span> {expectedResult}
        </p>
      )}
    </div>
  )
}
