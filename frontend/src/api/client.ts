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

export class ApiError extends Error {}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { error?: string } | null
    throw new ApiError(body?.error ?? `Request failed (${response.status})`)
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
}
