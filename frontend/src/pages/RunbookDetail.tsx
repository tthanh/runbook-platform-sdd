import { useCallback, useEffect, useState } from 'react'
import { api, ApiError, type RunbookDetail as RunbookDetailDto } from '../api/client'
import { StepEditor } from '../components/StepEditor'
import { VersionHistory } from '../components/VersionHistory'

// US1: edit working Steps and publish; US2: republish + history.
export function RunbookDetail({ runbookId }: { runbookId: string }) {
  const [runbook, setRunbook] = useState<RunbookDetailDto | null>(null)
  const [stepTexts, setStepTexts] = useState<string[]>([])
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<string | null>(null)

  const load = useCallback(async () => {
    const detail = await api.getRunbook(runbookId)
    setRunbook(detail)
    setStepTexts(detail.steps.map((s) => s.text))
  }, [runbookId])

  useEffect(() => {
    load().catch((e: Error) => setError(e.message))
  }, [load])

  async function saveSteps() {
    setError(null)
    setNotice(null)
    try {
      await api.saveSteps(runbookId, stepTexts)
      await load()
      setNotice('Steps saved.')
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
    }
  }

  async function publish() {
    setError(null)
    setNotice(null)
    try {
      // Save first so publish freezes what the author sees on screen.
      await api.saveSteps(runbookId, stepTexts)
      const version = await api.publish(runbookId)
      await load()
      setNotice(`Published Version ${version.number}.`)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
    }
  }

  if (error && !runbook) return <main><p className="error">{error}</p></main>
  if (!runbook) return <main><p>Loading…</p></main>

  return (
    <main>
      <p>
        <a href="#/">← All Runbooks</a>
      </p>
      <h1>{runbook.name}</h1>

      <h2>Steps</h2>
      <StepEditor steps={stepTexts} onChange={setStepTexts} />
      <p>
        <button type="button" onClick={() => void saveSteps()}>
          Save Steps
        </button>{' '}
        <button type="button" className="primary" onClick={() => void publish()}>
          Publish
        </button>
      </p>
      {error && <p className="error">{error}</p>}
      {notice && <p className="notice">{notice}</p>}

      {runbook.currentVersionNumber !== null && (
        <p>
          <a
            href={`#/executions/start?runbookId=${runbook.id}`}
            className="primary"
          >
            Run against Incident →
          </a>
        </p>
      )}

      <h2>Published versions</h2>
      <VersionHistory
        runbookId={runbook.id}
        versions={runbook.versions}
        currentVersionNumber={runbook.currentVersionNumber}
      />
    </main>
  )
}
