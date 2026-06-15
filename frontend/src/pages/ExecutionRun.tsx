import { useEffect, useState } from 'react'
import { api, ApiError, type ExecutionView, type ComputedReview } from '../api/client'

// US1 run view + US2 close/review: hash-routed, no router package (C-003 / R7).
// Routes:
//   #/executions/:id         → active run (mark steps, close)
//   #/executions/:id/review  → computed review (closed Execution only)

interface RunViewProps {
  execution: ExecutionView
  onRecordStep: (pos: number, outcome: string, note: string) => Promise<void>
  onClose: () => Promise<void>
  error: string | null
}

function RunView({ execution, onRecordStep, onClose, error }: RunViewProps) {
  const [noteFor, setNoteFor] = useState<number | null>(null)
  const [notes, setNotes] = useState<Record<number, string>>({})
  const [marking, setMarking] = useState(false)

  const lastRecordByPosition: Record<number, string> = {}
  for (const r of execution.records) {
    lastRecordByPosition[r.stepPosition] = r.outcome
  }

  async function mark(pos: number, outcome: string) {
    setMarking(true)
    await onRecordStep(pos, outcome, notes[pos] ?? '')
    setNotes((prev) => ({ ...prev, [pos]: '' }))
    setNoteFor(null)
    setMarking(false)
  }

  return (
    <main>
      <h1>{execution.runbookName} — v{execution.pinnedVersionNumber}</h1>
      <p className="muted">
        Incident: <strong>{execution.incidentTitle ?? execution.incidentId}</strong>
        {execution.incidentTitle && <span className="muted"> ({execution.incidentId})</span>}
      </p>
      {error && <p className="error">{error}</p>}

      <ul className="step-list">
        {execution.steps.map((step) => {
          const last = lastRecordByPosition[step.position]
          return (
            <li key={step.position} className={last ? `step-${last.toLowerCase()}` : ''}>
              <span className="step-pos">{step.position}.</span>
              <span className="step-text">{step.text}</span>
              {last && <span className="badge">{last}</span>}

              {execution.status === 'Open' && (
                <span className="step-actions">
                  <button disabled={marking} onClick={() => void mark(step.position, 'Done')}>Done</button>
                  <button disabled={marking} onClick={() => void mark(step.position, 'Skipped')}>Skipped</button>
                  <button disabled={marking} onClick={() => void mark(step.position, 'Failed')}>Failed</button>
                  <button
                    className="note-toggle"
                    onClick={() => setNoteFor(noteFor === step.position ? null : step.position)}
                  >
                    Note
                  </button>
                  {noteFor === step.position && (
                    <input
                      autoFocus
                      value={notes[step.position] ?? ''}
                      onChange={(e) => setNotes((prev) => ({ ...prev, [step.position]: e.target.value }))}
                      placeholder="Optional note…"
                    />
                  )}
                </span>
              )}
            </li>
          )
        })}
      </ul>

      {execution.status === 'Open' && (
        <button className="close-btn" onClick={() => void onClose()}>
          Close Execution
        </button>
      )}

      {execution.status === 'Closed' && (
        <p>
          Execution closed.{' '}
          <a href={`#/executions/${execution.id}/review`}>View review →</a>
        </p>
      )}
    </main>
  )
}

interface ReviewViewProps {
  review: ComputedReview
  executionId: string
}

function ReviewView({ review, executionId }: ReviewViewProps) {
  return (
    <main>
      <h1>Review: {review.incident}</h1>
      <p className="muted">
        {review.runbookName} v{review.pinnedVersionNumber} ·{' '}
        {new Date(review.startedAt).toLocaleString()} –{' '}
        {review.closedAt ? new Date(review.closedAt).toLocaleString() : ''}
      </p>

      <h2>Timeline</h2>
      {review.timeline.length === 0 ? (
        <p className="muted">No steps were recorded.</p>
      ) : (
        <ol className="timeline">
          {review.timeline.map((entry, i) => (
            <li key={i} className={`step-${entry.outcome.toLowerCase()}`}>
              <span className="badge">{entry.outcome}</span>{' '}
              Step {entry.stepPosition}: {entry.stepText}
              {entry.note && <span className="muted"> — {entry.note}</span>}
              <span className="muted time"> {new Date(entry.recordedAt).toLocaleTimeString()}</span>
            </li>
          ))}
        </ol>
      )}

      <h2>Coverage</h2>
      <ul className="coverage">
        {review.coverage.map((c) => (
          <li key={c.stepPosition} className={`step-${c.endState.toLowerCase()}`}>
            <span className="badge">{c.endState}</span> Step {c.stepPosition}
          </li>
        ))}
      </ul>

      <p><a href={`#/executions/${executionId}`}>← Back to run</a></p>
    </main>
  )
}

// StartForm: collects incidentId + optional title for a given Runbook.
interface StartFormProps {
  runbookId: string
  runbookName?: string
}

function StartForm({ runbookId, runbookName }: StartFormProps) {
  const [incidentId, setIncidentId] = useState('')
  const [incidentTitle, setIncidentTitle] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function start() {
    if (!incidentId.trim()) { setError('Incident identifier is required.'); return }
    setLoading(true)
    setError(null)
    try {
      const exec = await api.startExecution(runbookId, incidentId.trim(), incidentTitle)
      window.location.hash = `#/executions/${exec.id}`
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
      setLoading(false)
    }
  }

  return (
    <main>
      <h1>Start Execution{runbookName ? ` — ${runbookName}` : ''}</h1>
      <form onSubmit={(e) => { e.preventDefault(); void start() }}>
        <label>
          Incident ID <span className="required">*</span>
          <input
            required
            value={incidentId}
            onChange={(e) => setIncidentId(e.target.value)}
            placeholder="INC-1234 or URL"
          />
        </label>
        <label>
          Incident Title <span className="muted">(optional)</span>
          <input
            value={incidentTitle}
            onChange={(e) => setIncidentTitle(e.target.value)}
            placeholder="Short description"
          />
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" disabled={loading}>
          {loading ? 'Starting…' : 'Start'}
        </button>
      </form>
    </main>
  )
}

// Router entry points exported from this module.

export function ExecutionStartPage({ runbookId }: { runbookId: string }) {
  const [name, setName] = useState<string | undefined>(undefined)

  useEffect(() => {
    api.getRunbook(runbookId).then((r) => setName(r.name)).catch(() => undefined)
  }, [runbookId])

  return <StartForm runbookId={runbookId} runbookName={name} />
}

export function ExecutionRunPage({ executionId }: { executionId: string }) {
  const [execution, setExecution] = useState<ExecutionView | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api.getExecution(executionId).then(setExecution).catch((e: Error) => setError(e.message))
  }, [executionId])

  async function recordStep(pos: number, outcome: string, note: string) {
    setError(null)
    try {
      await api.recordStep(executionId, pos, outcome, note)
      const updated = await api.getExecution(executionId)
      setExecution(updated)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
    }
  }

  async function closeExecution() {
    setError(null)
    try {
      const updated = await api.closeExecution(executionId)
      setExecution(updated)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
    }
  }

  if (error && !execution) return <main><p className="error">{error}</p></main>
  if (!execution) return <main><p>Loading…</p></main>

  return (
    <RunView
      execution={execution}
      onRecordStep={recordStep}
      onClose={closeExecution}
      error={error}
    />
  )
}

export function ExecutionReviewPage({ executionId }: { executionId: string }) {
  const [review, setReview] = useState<ComputedReview | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api.getReview(executionId).then(setReview).catch((e: Error) => setError(e.message))
  }, [executionId])

  if (error) return <main><p className="error">{error}</p></main>
  if (!review) return <main><p>Loading…</p></main>

  return <ReviewView review={review} executionId={executionId} />
}
