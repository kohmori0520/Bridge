import { useMemo, useState, type ReactNode } from 'react'
import { users } from '../shared/mocks/mockData'
import type { User } from '../shared/types/domain'
import { AuthContext, type AuthContextValue } from './authContext'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User>(users.Engineer)
  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      demoUsers: users,
      loginAs: (role) => setUser(users[role]),
      logout: () => setUser(users.Engineer),
    }),
    [user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
