const DEFAULT_API_BASE_URL = 'http://localhost:5130'
const PRODUCTION_API_BASE_URL = 'https://bridge-api-mk.fly.dev'

let accessToken: string | null = null

export function setAccessToken(token: string | null) {
  accessToken = token
}

let onUnauthorized: (() => void) | null = null

export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler
}

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

function getApiBaseUrl() {
  const configuredUrl = import.meta.env.VITE_API_BASE_URL
  const fallbackUrl = import.meta.env.PROD ? PRODUCTION_API_BASE_URL : DEFAULT_API_BASE_URL

  return (configuredUrl ?? fallbackUrl).replace(/\/$/, '')
}

async function readErrorMessage(response: Response) {
  try {
    const body = await response.json()
    return body?.error?.message ?? body?.message ?? response.statusText
  } catch {
    return response.statusText
  }
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (!headers.has('Content-Type') && init.body) {
    headers.set('Content-Type', 'application/json')
  }
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`)
  }

  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    ...init,
    headers,
  })

  if (!response.ok) {
    // トークン付きリクエストの 401 はセッション失効(期限切れ等)とみなし、
    // ログイン画面へ戻す(ログイン試行自体の 401 はトークンがないので対象外)
    if (response.status === 401 && accessToken) {
      setAccessToken(null)
      onUnauthorized?.()
    }
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}
