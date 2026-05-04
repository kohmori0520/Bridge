import { createContext } from 'react'
import type { Role, User } from '../shared/types/domain'

export type AuthContextValue = {
  user: User
  demoUsers: Record<Role, User>
  loginAs: (role: Role) => void
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)
