import { createContext } from 'react'

export type NotificationSeverity = 'success' | 'info' | 'warning' | 'error'

export type NotificationContextValue = {
  notify: (message: string, severity?: NotificationSeverity) => void
}

export const NotificationContext = createContext<NotificationContextValue | null>(null)
