import type { User } from '../shared/types/domain'

const AUTH_SESSION_KEY = 'bridge.auth.session'

type AuthSession = {
  token: string
  user: User
}

function getSessionStorage() {
  if (typeof window === 'undefined') {
    return null
  }

  return window.sessionStorage
}

export function loadAuthSession(): AuthSession | null {
  const storage = getSessionStorage()
  if (!storage) {
    return null
  }

  try {
    const raw = storage.getItem(AUTH_SESSION_KEY)
    return raw ? (JSON.parse(raw) as AuthSession) : null
  } catch {
    storage.removeItem(AUTH_SESSION_KEY)
    return null
  }
}

export function saveAuthSession(session: AuthSession) {
  getSessionStorage()?.setItem(AUTH_SESSION_KEY, JSON.stringify(session))
}

export function clearAuthSession() {
  getSessionStorage()?.removeItem(AUTH_SESSION_KEY)
}
