import { useEffect, useState } from 'react'
import { api, type RunbookVersion } from '../api/client'

// US3-2 / FR-007: a published Runbook Version, exactly as published. Read-only.
export function VersionView({ runbookId, number }: { runbookId: string; number: number }) {
  const [version, setVersion] = useState<RunbookVersion | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .getVersion(runbookId, number)
      .then(setVersion)
      .catch((e: Error) => setError(e.message))
  }, [runbookId, number])

  if (error) return <main><p className="error">{error}</p></main>
  if (!version) return <main><p>Loading…</p></main>

  return (
    <main>
      <p>
        <a href={`#/runbooks/${runbookId}`}>← Back to Runbook</a>
      </p>
      <h1>
        {version.nameAtPublish} <span className="badge">Version {version.number}</span>
      </h1>
      <p className="muted">Published {new Date(version.publishedAt).toLocaleString()}</p>

      <ol className="version-steps">
        {version.steps.map((s) => (
          <li key={s.position}>{s.text}</li>
        ))}
      </ol>
    </main>
  )
}
