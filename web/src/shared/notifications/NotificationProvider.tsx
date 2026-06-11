import { Alert, Snackbar } from '@mui/material'
import { useCallback, useMemo, useState, type ReactNode } from 'react'
import {
  NotificationContext,
  type NotificationContextValue,
  type NotificationSeverity,
} from './notificationContext'

type Notification = {
  id: number
  message: string
  severity: NotificationSeverity
}

export function NotificationProvider({ children }: { children: ReactNode }) {
  const [notification, setNotification] = useState<Notification | null>(null)

  const notify = useCallback((message: string, severity: NotificationSeverity = 'success') => {
    setNotification({ id: Date.now(), message, severity })
  }, [])

  const value = useMemo<NotificationContextValue>(() => ({ notify }), [notify])

  return (
    <NotificationContext.Provider value={value}>
      {children}
      {notification && (
        <Snackbar
          key={notification.id}
          open
          autoHideDuration={4000}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
          onClose={(_, reason) => {
            if (reason === 'clickaway') return
            setNotification(null)
          }}
        >
          <Alert
            severity={notification.severity}
            variant="filled"
            elevation={3}
            onClose={() => setNotification(null)}
            sx={{ minWidth: 280 }}
          >
            {notification.message}
          </Alert>
        </Snackbar>
      )}
    </NotificationContext.Provider>
  )
}
