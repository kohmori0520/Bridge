import { Box, Typography } from '@mui/material'

type InfoProps = {
  label: string
  value: string
  wide?: boolean
}

export function Info({ label, value, wide }: InfoProps) {
  return (
    <Box className={wide ? 'info-item wide' : 'info-item'}>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography>{value}</Typography>
    </Box>
  )
}
