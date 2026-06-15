// Typed client for the HTTP API (contracts/http-api.md).

export interface RunbookListItem {
  id: string
  name: string
  currentVersionNumber: number | null
}

export interface StepItem {
  position: number
  text: string
}

export interface VersionSummary {
  number: number
  publishedAt: string
}

export interface RunbookDetail {
  id: string
  name: string
  steps: StepItem[]
  currentVersionNumber: number | null
  versions: VersionSummary[]
}

export interface RunbookVersion {
  number: number
  nameAtPublish: string
  publishedAt: string
  steps: StepItem[]
}

// Execution types (003-runbook-execution)
export interface StepRecord {
  stepPosition: number
  outcome: string
  note: string | null
  recordedAt: string
}

export interface ExecutionView {
  id: string
  incidentId: string
  incidentTitle: string | null
  status: string
  runbookName: string
  pinnedVersionNumber: number | null
  steps: StepItem[]
  records: StepRecord[]
}

export interface ReviewTimeline {
  stepPosition: number
  stepText: string
  outcome: string
  note: string | null
  recordedAt: string
}

export interface ReviewCoverage {
  stepPosition: number
  endState: string
}

export interface ComputedReview {
  incident: string
  runbookName: string
  pinnedVersionNumber: number
  startedAt: string
  closedAt: string | null
  timeline: ReviewTimeline[]
  coverage: ReviewCoverage[]
}

export class ApiError extends Error {
  constructor(message: string, public readonly status?: number) {
    super(message)
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { error?: string } | null
    throw new ApiError(body?.error ?? `Request failed (${response.status})`, response.status)
  }
  return (await response.json()) as T
}

export const api = {
  listRunbooks: () => request<RunbookListItem[]>('/api/runbooks'),

  createRunbook: (name: string) =>
    request<RunbookDetail>('/api/runbooks', {
      method: 'POST',
      body: JSON.stringify({ name }),
    }),

  getRunbook: (id: string) => request<RunbookDetail>(`/api/runbooks/${id}`),

  saveSteps: (id: string, texts: string[]) =>
    request<{ steps: StepItem[] }>(`/api/runbooks/${id}/steps`, {
      method: 'PUT',
      body: JSON.stringify({ steps: texts.map((text) => ({ text })) }),
    }),

  publish: (id: string) =>
    request<RunbookVersion>(`/api/runbooks/${id}/publish`, { method: 'POST' }),

  getVersion: (id: string, number: number) =>
    request<RunbookVersion>(`/api/runbooks/${id}/versions/${number}`),

  // Execution endpoints (003-runbook-execution)
  startExecution: (runbookId: string, incidentId: string, incidentTitle: string) =>
    request<ExecutionView>('/api/executions', {
      method: 'POST',
      body: JSON.stringify({ runbookId, incidentId, incidentTitle: incidentTitle || null }),
    }),

  getExecution: (id: string) => request<ExecutionView>(`/api/executions/${id}`),

  recordStep: (executionId: string, stepPosition: number, outcome: string, note: string) =>
    request<StepRecord>(`/api/executions/${executionId}/records`, {
      method: 'POST',
      body: JSON.stringify({ stepPosition, outcome, note: note || null }),
    }),

  closeExecution: (id: string) =>
    request<ExecutionView>(`/api/executions/${id}/close`, { method: 'POST' }),

  getReview: (id: string) => request<ComputedReview>(`/api/executions/${id}/review`),
}
