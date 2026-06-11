import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import type { ReactElement, ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../auth/authContext'
import { NotificationProvider } from '../shared/notifications/NotificationProvider'
import type { User } from '../shared/types/domain'

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })
}

export function createAuthValue(user: User | null, overrides: Partial<AuthContextValue> = {}): AuthContextValue {
  return {
    user,
    login: vi.fn(async () => {}),
    logout: vi.fn(async () => {}),
    ...overrides,
  }
}

type RenderWithProvidersOptions = {
  /** ログイン中のユーザー。省略時は未ログイン */
  user?: User | null
  /** AuthContext の値を細かく制御したい場合に指定 (user より優先) */
  auth?: AuthContextValue
  /** MemoryRouter の初期 URL */
  route?: string
  queryClient?: QueryClient
}

export function renderWithProviders(ui: ReactElement, options: RenderWithProvidersOptions = {}) {
  const { user = null, route = '/', queryClient = createTestQueryClient() } = options
  const auth = options.auth ?? createAuthValue(user)

  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <NotificationProvider>
        <QueryClientProvider client={queryClient}>
          <AuthContext.Provider value={auth}>
            <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>
          </AuthContext.Provider>
        </QueryClientProvider>
      </NotificationProvider>
    )
  }

  return { ...render(ui, { wrapper: Wrapper }), auth, queryClient }
}
