import { Box, Card, CardContent, Typography } from '@mui/material'
import type { ReactNode } from 'react'

type SectionProps = {
  title: string
  action?: ReactNode
  children: ReactNode
}

export function Section({ title, action, children }: SectionProps) {
  return (
    <Card className="section-card">
      <CardContent>
        <Box className="section-header">
          <Typography variant="h5">{title}</Typography>
          {action}
        </Box>
        {children}
      </CardContent>
    </Card>
  )
}
