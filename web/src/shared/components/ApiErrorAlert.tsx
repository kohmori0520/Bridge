import { Alert } from '@mui/material'
import { ApiError } from '../api/http'

type ApiErrorAlertProps = {
  error: unknown
  fallbackMessage: string
}

function getErrorMessage(error: unknown, fallbackMessage: string) {
  if (error instanceof ApiError) {
    return error.status === 401
      ? 'ログインの有効期限が切れたか、認証に失敗しました。再ログインしてください。'
      : error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return fallbackMessage
}

export function ApiErrorAlert({ error, fallbackMessage }: ApiErrorAlertProps) {
  return <Alert severity="error">{getErrorMessage(error, fallbackMessage)}</Alert>
}
