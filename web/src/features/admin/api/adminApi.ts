import { apiRequest } from '../../../shared/api/http'
import type { Role } from '../../../shared/types/domain'

export type CreateUserInput = {
  email: string
  password: string
  role: Role
  name: string
  department: string
  primarySalesId: number | null
}

export type CreatedUser = {
  userId: number
  email: string
  role: Role
  salesId: number | null
  engineerId: number | null
}

export type SalesOption = {
  id: number
  name: string
  department: string
  email: string
}

export const adminApi = {
  createUser: async (input: CreateUserInput): Promise<CreatedUser> => {
    return apiRequest<CreatedUser>('/users', {
      method: 'POST',
      body: JSON.stringify({
        email: input.email,
        password: input.password,
        role: input.role,
        name: input.name,
        department: input.role === 'Sales' ? input.department : '',
        primarySalesId: input.role === 'Engineer' ? input.primarySalesId : null,
      }),
    })
  },
  listSales: async (): Promise<SalesOption[]> => {
    return apiRequest<SalesOption[]>('/sales')
  },
}
