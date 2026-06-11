import { CircularProgress, Stack, Typography } from '@mui/material'

export function LoadingState({ message }: { message: string }) {
  return (
    <Stack className="loading-state" spacing={1.5} role="status" sx={{ alignItems: 'center' }}>
      <CircularProgress size={30} thickness={4} />
      <Typography variant="body2" color="text.secondary">
        {message}
      </Typography>
    </Stack>
  )
}
