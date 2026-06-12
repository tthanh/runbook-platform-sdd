import { useEffect, useState } from 'react'
import { RunbookList } from './pages/RunbookList'
import { RunbookDetail } from './pages/RunbookDetail'
import { VersionView } from './pages/VersionView'

// Hash routing keeps the slice dependency-free (no router package):
//   #/                      → Runbook list
//   #/runbooks/:id          → Runbook detail (edit + publish + history)
//   #/runbooks/:id/versions/:n → read-only Runbook Version view
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
