import { Box, Paper, Typography } from '@mui/material'
import type { ReactNode } from 'react'

type SummaryStat = {
  label: string
  value: ReactNode
  tone?: 'default' | 'success' | 'warning'
}

type SummaryStatsProps = {
  items: SummaryStat[]
}

export function SummaryStats({ items }: SummaryStatsProps) {
  return (
    <Box className="summary-stats">
      {items.map((item) => (
        <Paper className={`summary-stat summary-stat-${item.tone ?? 'default'}`} variant="outlined" key={item.label}>
          <Typography variant="caption" color="text.secondary">
            {item.label}
          </Typography>
          <Typography variant="h5">{item.value}</Typography>
        </Paper>
      ))}
    </Box>
  )
}
