import { Alert, Button, Stack } from '@mui/material'
import type { ReactNode } from 'react'
import { Link as RouterLink, Navigate } from 'react-router-dom'
import { useAuth } from './useAuth'
import { PageHeader } from '../shared/components/PageHeader'
import type { Role } from '../shared/types/domain'

type RequireRoleProps = {
  allowed: Role[]
  children: ReactNode
}

export function RequireRole({ allowed, children }: RequireRoleProps) {
  const { user } = useAuth()

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (allowed.includes(user.role)) {
    return children
  }

  return (
    <Stack spacing={2}>
      <PageHeader title="権限エラー" subtitle="この画面を表示する権限がありません" />
      <Alert severity="warning">現在のロール: {user.role}</Alert>
      <Button component={RouterLink} to="/" variant="contained">
        ホームへ戻る
      </Button>
    </Stack>
  )
}
