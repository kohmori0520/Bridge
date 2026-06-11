import { CssBaseline, ThemeProvider } from '@mui/material'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { AuthProvider } from '../auth/AuthProvider'
import { NotificationProvider } from '../shared/notifications/NotificationProvider'
import { bridgeTheme } from '../shared/theme/bridgeTheme'

const queryClient = new QueryClient()

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider theme={bridgeTheme}>
      <CssBaseline />
      <NotificationProvider>
        <QueryClientProvider client={queryClient}>
          <AuthProvider>{children}</AuthProvider>
        </QueryClientProvider>
      </NotificationProvider>
    </ThemeProvider>
  )
}
