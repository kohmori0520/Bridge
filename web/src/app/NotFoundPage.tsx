import { Dashboard, SearchOff } from '@mui/icons-material'
import { Button, Stack, Typography } from '@mui/material'
import { useEffect } from 'react'
import { Link as RouterLink } from 'react-router-dom'

export function NotFoundPage() {
  useEffect(() => {
    document.title = 'ページが見つかりません | Bridge'
  }, [])

  return (
    <Stack className="not-found-page" spacing={2} sx={{ alignItems: 'center', textAlign: 'center' }}>
      <SearchOff className="not-found-icon" />
      <Typography variant="h4">ページが見つかりません</Typography>
      <Typography color="text.secondary">
        URL が間違っているか、ページが移動・削除された可能性があります。
      </Typography>
      <Button component={RouterLink} to="/" variant="contained" startIcon={<Dashboard />}>
        ホームへ戻る
      </Button>
    </Stack>
  )
}
