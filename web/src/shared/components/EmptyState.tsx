import { Alert } from '@mui/material'

export function EmptyState({ message }: { message: string }) {
  return <Alert severity="warning">{message}</Alert>
}
