import { Alert } from '@mui/material'

export function LoadingState({ message }: { message: string }) {
  return <Alert severity="info">{message}</Alert>
}
