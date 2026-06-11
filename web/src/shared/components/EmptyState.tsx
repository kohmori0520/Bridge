import { InboxOutlined } from '@mui/icons-material'
import { Stack, Typography } from '@mui/material'
import type { ReactNode } from 'react'

type EmptyStateProps = {
  message: string
  /** 空状態から次の行動につなげるボタンなど */
  action?: ReactNode
}

export function EmptyState({ message, action }: EmptyStateProps) {
  return (
    <Stack className="empty-state" spacing={1.25} sx={{ alignItems: 'center' }}>
      <InboxOutlined className="empty-state-icon" />
      <Typography variant="body2" color="text.secondary">
        {message}
      </Typography>
      {action}
    </Stack>
  )
}
