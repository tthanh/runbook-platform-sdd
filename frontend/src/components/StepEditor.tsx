import type { StepInput, StepType } from '../api/client'

// 004 FR-001/002: edit each Step's title, optional detail (instructions/command/
// expected result) and Step Type. Edits are local until "Save Steps" sends the
// full ordered replacement. Title is the only required field (FR-003).
export function StepEditor({
  steps,
  onChange,
}: {
  steps: StepInput[]
  onChange: (steps: StepInput[]) => void
}) {
  function patch(index: number, fields: Partial<StepInput>) {
    onChange(steps.map((s, i) => (i === index ? { ...s, ...fields } : s)))
  }

  function remove(index: number) {
    onChange(steps.filter((_, i) => i !== index))
  }

  function move(index: number, delta: -1 | 1) {
    const target = index + delta
    if (target < 0 || target >= steps.length) return
    const next = [...steps]
    ;[next[index], next[target]] = [next[target], next[index]]
    onChange(next)
  }

  function add() {
    onChange([
      ...steps,
      { text: '', instructions: '', command: '', expectedResult: '', type: 'Action' },
    ])
  }

  return (
    <ol className="step-editor">
      {steps.map((step, i) => (
        // Position is the identity of a working Step; index key is correct here.
        <li key={i} className="step-edit-card">
          <div className="step-edit-head">
            <input
              className="step-title"
              value={step.text}
              onChange={(e) => patch(i, { text: e.target.value })}
              placeholder="Step title (required)"
              aria-label={`Step ${i + 1} title`}
            />
            <select
              value={step.type}
              onChange={(e) => patch(i, { type: e.target.value as StepType })}
              aria-label={`Step ${i + 1} type`}
            >
              <option value="Action">Action</option>
              <option value="Check">Check</option>
            </select>
            <button type="button" onClick={() => move(i, -1)} disabled={i === 0} aria-label="Move up">↑</button>
            <button type="button" onClick={() => move(i, 1)} disabled={i === steps.length - 1} aria-label="Move down">↓</button>
            <button type="button" onClick={() => remove(i)} aria-label="Remove step">✕</button>
          </div>
          <textarea
            className="step-instructions"
            value={step.instructions ?? ''}
            onChange={(e) => patch(i, { instructions: e.target.value })}
            placeholder="Instructions (markdown: **bold**, lists, `code`, links)"
            aria-label={`Step ${i + 1} instructions`}
            rows={2}
          />
          <input
            className="step-command"
            value={step.command ?? ''}
            onChange={(e) => patch(i, { command: e.target.value })}
            placeholder="Command (optional, shown verbatim)"
            aria-label={`Step ${i + 1} command`}
          />
          <input
            className="step-expected"
            value={step.expectedResult ?? ''}
            onChange={(e) => patch(i, { expectedResult: e.target.value })}
            placeholder="Expected result (optional)"
            aria-label={`Step ${i + 1} expected result`}
          />
        </li>
      ))}
      <li className="add-row">
        <button type="button" onClick={add}>+ Add Step</button>
      </li>
    </ol>
  )
}
