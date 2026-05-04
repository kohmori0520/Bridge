import { Dashboard } from '@mui/icons-material'
import { Button, Stack } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { PageHeader } from '../shared/components/PageHeader'

export function NotFoundPage() {
  return (
    <Stack spacing={2} sx={{ alignItems: 'flex-start' }}>
      <PageHeader title="404 / エラー" subtitle="指定された画面は見つかりませんでした" />
      <Button component={RouterLink} to="/" variant="contained" startIcon={<Dashboard />}>
        ホームへ戻る
      </Button>
    </Stack>
  )
}
