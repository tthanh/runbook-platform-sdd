import { useEffect, useState } from 'react'
import { RunbookList } from './pages/RunbookList'
import { RunbookDetail } from './pages/RunbookDetail'
import { VersionView } from './pages/VersionView'
import { ExecutionStartPage, ExecutionRunPage, ExecutionReviewPage } from './pages/ExecutionRun'

// Hash routing keeps the slice dependency-free (no router package):
//   #/                             → Runbook list
//   #/runbooks/:id                 → Runbook detail (edit + publish + history)
//   #/runbooks/:id/versions/:n     → read-only Runbook Version view
//   #/executions/start?runbookId=  → Start/resume an Execution
//   #/executions/:id               → Active run view (mark steps, close)
//   #/executions/:id/review        → Computed Review (closed Execution)
function useHashRoute(): string {
  const [hash, setHash] = useState(window.location.hash)
  useEffect(() => {
    const onChange = () => setHash(window.location.hash)
    window.addEventListener('hashchange', onChange)
    return () => window.removeEventListener('hashchange', onChange)
  }, [])
  return hash.replace(/^#/, '') || '/'
}

function App() {
  const route = useHashRoute()

  const reviewMatch = route.match(/^\/executions\/([^/]+)\/review$/)
  if (reviewMatch) {
    return <ExecutionReviewPage executionId={reviewMatch[1]} />
  }

  const execMatch = route.match(/^\/executions\/([^/?]+)$/)
  if (execMatch) {
    return <ExecutionRunPage executionId={execMatch[1]} />
  }

  if (route.startsWith('/executions/start')) {
    const params = new URLSearchParams(route.replace('/executions/start', '').replace(/^\?/, ''))
    const runbookId = params.get('runbookId') ?? ''
    return <ExecutionStartPage runbookId={runbookId} />
  }

  const versionMatch = route.match(/^\/runbooks\/([^/]+)\/versions\/(\d+)$/)
  if (versionMatch) {
    return <VersionView runbookId={versionMatch[1]} number={Number(versionMatch[2])} />
  }

  const detailMatch = route.match(/^\/runbooks\/([^/]+)$/)
  if (detailMatch) {
    return <RunbookDetail runbookId={detailMatch[1]} />
  }

  return <RunbookList />
}

export default App
