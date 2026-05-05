import { useMemo, useState, type ReactNode } from 'react'
import { setAccessToken } from '../shared/api/http'
import { users } from '../shared/mocks/mockData'
import type { User } from '../shared/types/domain'
import { authApi } from './authApi'
import { clearAuthSession, loadAuthSession, saveAuthSession } from './authStorage'
import { AuthContext, type AuthContextValue } from './authContext'

const initialSession = loadAuthSession()
if (initialSession) {
  setAccessToken(initialSession.token)
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(initialSession?.user ?? null)
  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      demoUsers: users,
      login: async ({ email, password }) => {
        const result = await authApi.login(email, password)
        setAccessToken(result.token)
        saveAuthSession(result)
        setUser(result.user)
      },
      logout: async () => {
        try {
          await authApi.logout()
        } finally {
          setAccessToken(null)
          clearAuthSession()
          setUser(null)
        }
      },
    }),
    [user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
