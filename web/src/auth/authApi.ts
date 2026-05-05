import { apiRequest } from '../shared/api/http'
import type { Role, User } from '../shared/types/domain'

type AuthUserResponse = {
  id: number
  email: string
  role: string
  salesId: number | null
  engineerId: number | null
  name?: string | null
}

type LoginResponse = {
  token: string
  user: AuthUserResponse
}

function toRole(role: string): Role {
  if (role === 'Sales' || role === 'Engineer') return role
  throw new Error(`Unsupported role: ${role}`)
}

export function toUser(response: AuthUserResponse): User {
  return {
    role: toRole(response.role),
    name: response.name ?? response.email,
    email: response.email,
  }
}

export const authApi = {
  login: async (email: string, password: string) => {
    const response = await apiRequest<LoginResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    })

    return {
      token: response.token,
      user: toUser(response.user),
    }
  },
  me: async () => {
    const response = await apiRequest<AuthUserResponse>('/auth/me')
    return toUser(response)
  },
  logout: async () => {
    await apiRequest<void>('/auth/logout', { method: 'POST' })
  },
}
