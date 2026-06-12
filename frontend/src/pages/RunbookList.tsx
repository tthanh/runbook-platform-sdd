import { useEffect, useState } from 'react'
import { api, ApiError, type RunbookListItem } from '../api/client'

// US3-1 / FR-009: every Runbook listed, published or not.
export function RunbookList() {
  const [runbooks, setRunbooks] = useState<RunbookListItem[] | null>(null)
  const [name, setName] = useState('')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api.listRunbooks().then(setRunbooks).catch((e: Error) => setError(e.message))
  }, [])

  async function create() {
    setError(null)
    try {
      const created = await api.createRunbook(name)
      window.location.hash = `#/runbooks/${created.id}`
    } catch (e) {
      setError(e instanceof ApiError ? e.message : 'Something went wrong.')
    }
  }

  return (
    <main>
      <h1>Runbooks</h1>

      <form
        onSubmit={(e) => {
          e.preventDefault()
          void create()
        }}
      >
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="New Runbook name"
          aria-label="New Runbook name"
        />
        <button type="submit">Create</button>
      </form>
      {error && <p className="error">{error}</p>}

      {runbooks === null ? (
        <p>Loading…</p>
      ) : runbooks.length === 0 ? (
        <p>No Runbooks yet — create the first one above.</p>
      ) : (
        <ul className="runbook-list">
          {runbooks.map((r) => (
            <li key={r.id}>
              <a href={`#/runbooks/${r.id}`}>{r.name}</a>{' '}
              {r.currentVersionNumber !== null ? (
                <span className="badge">v{r.currentVersionNumber}</span>
              ) : (
                <span className="muted">nothing published yet</span>
              )}
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}
