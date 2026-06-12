import type { VersionSummary } from '../api/client'

// US2-4 / FR-008: latest published Runbook Version is current by default;
// earlier ones stay reachable (FR-007).
export function VersionHistory({
  runbookId,
  versions,
  currentVersionNumber,
}: {
  runbookId: string
  versions: VersionSummary[]
  currentVersionNumber: number | null
}) {
  if (versions.length === 0) {
    return <p className="muted">Nothing published yet.</p>
  }

  return (
    <ul className="version-history">
      {[...versions]
        .sort((a, b) => b.number - a.number)
        .map((v) => (
          <li key={v.number}>
            <a href={`#/runbooks/${runbookId}/versions/${v.number}`}>Version {v.number}</a>{' '}
            {v.number === currentVersionNumber && <span className="badge">current</span>}{' '}
            <span className="muted">{new Date(v.publishedAt).toLocaleString()}</span>
          </li>
        ))}
    </ul>
  )
}
