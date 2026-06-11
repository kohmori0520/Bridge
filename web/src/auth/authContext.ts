import { createContext } from 'react'
import type { LoginCredentials, User } from '../shared/types/domain'

export type AuthContextValue = {
  user: User | null
  login: (credentials: LoginCredentials) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
