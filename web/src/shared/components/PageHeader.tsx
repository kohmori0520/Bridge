import { ArrowBack } from '@mui/icons-material'
import { Box, Button, Stack, Typography } from '@mui/material'
import { useEffect, type ReactNode } from 'react'
import { Link as RouterLink } from 'react-router-dom'

type PageHeaderProps = {
  title: string
  subtitle?: string
  actions?: ReactNode
  backTo?: string
}

export function PageHeader({ title, subtitle, actions, backTo }: PageHeaderProps) {
  useEffect(() => {
    document.title = `${title} | Bridge`
  }, [title])

  return (
    <Stack className="page-header" spacing={1}>
      {backTo && (
        <Button component={RouterLink} to={backTo} startIcon={<ArrowBack />} className="back-button">
          戻る
        </Button>
      )}
      <Box className="header-row">
        <Box>
          <Typography variant="h4">{title}</Typography>
          {subtitle && <Typography color="text.secondary">{subtitle}</Typography>}
        </Box>
        {actions && <Stack direction="row" spacing={1}>{actions}</Stack>}
      </Box>
    </Stack>
  )
}
