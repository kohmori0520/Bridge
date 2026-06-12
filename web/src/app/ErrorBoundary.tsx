import { Refresh, WarningAmber } from '@mui/icons-material'
import { Box, Button, Stack, Typography } from '@mui/material'
import { Component, type ErrorInfo, type ReactNode } from 'react'

type ErrorBoundaryProps = {
  children: ReactNode
}

type ErrorBoundaryState = {
  error: Error | null
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unhandled render error:', error, info.componentStack)
  }

  private handleReload = () => {
    window.location.reload()
  }

  render() {
    if (this.state.error) {
      return (
        <Box className="error-boundary" sx={{ alignItems: 'center', display: 'flex', justifyContent: 'center', minHeight: '100svh', p: 3 }}>
          <Stack spacing={2} sx={{ alignItems: 'center', maxWidth: 420, textAlign: 'center' }}>
            <WarningAmber color="warning" sx={{ fontSize: 56 }} />
            <Typography variant="h5">予期しないエラーが発生しました</Typography>
            <Typography color="text.secondary" variant="body2">
              ページを再読み込みしてください。問題が続く場合は管理者にお問い合わせください。
            </Typography>
            {import.meta.env.DEV && (
              <Typography
                component="pre"
                sx={{
                  bgcolor: 'grey.100',
                  borderRadius: 1,
                  fontSize: '0.75rem',
                  maxWidth: '100%',
                  overflow: 'auto',
                  p: 1.5,
                  textAlign: 'left',
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word',
                }}
              >
                {this.state.error.message}
              </Typography>
            )}
            <Button variant="contained" startIcon={<Refresh />} onClick={this.handleReload}>
              再読み込み
            </Button>
          </Stack>
        </Box>
      )
    }

    return this.props.children
  }
}
