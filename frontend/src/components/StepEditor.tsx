// FR-002: add, edit, remove, and reorder the working Steps.
// Edits are local until "Save Steps" sends the full ordered replacement.
export function StepEditor({
  steps,
  onChange,
}: {
  steps: string[]
  onChange: (steps: string[]) => void
}) {
  function update(index: number, text: string) {
    onChange(steps.map((s, i) => (i === index ? text : s)))
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

  return (
    <ol className="step-editor">
      {steps.map((text, i) => (
        // Position is the identity of a working Step; index key is correct here.
        // eslint-disable-next-line react-x/no-array-index-key
        <li key={i}>
          <input
            value={text}
            onChange={(e) => update(i, e.target.value)}
            aria-label={`Step ${i + 1}`}
          />
          <button type="button" onClick={() => move(i, -1)} disabled={i === 0} aria-label="Move up">
            ↑
          </button>
          <button
            type="button"
            onClick={() => move(i, 1)}
            disabled={i === steps.length - 1}
            aria-label="Move down"
          >
            ↓
          </button>
          <button type="button" onClick={() => remove(i)} aria-label="Remove step">
            ✕
          </button>
        </li>
      ))}
      <li className="add-row">
        <button type="button" onClick={() => onChange([...steps, ''])}>
          + Add Step
        </button>
      </li>
    </ol>
  )
}
