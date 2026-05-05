import { createContext } from 'react'
import type { LoginCredentials, Role, User } from '../shared/types/domain'

export type AuthContextValue = {
  user: User | null
  demoUsers: Record<Role, User>
  login: (credentials: LoginCredentials) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
