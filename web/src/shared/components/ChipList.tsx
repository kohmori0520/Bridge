import { Box, Chip, Stack, Typography } from '@mui/material'

type ChipListProps = {
  label: string
  values: string[]
  color?: 'primary' | 'success' | 'warning'
}

export function ChipList({ label, values, color = 'primary' }: ChipListProps) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
        {values.map((value) => (
          <Chip key={value} size="small" color={color} label={value} />
        ))}
      </Stack>
    </Box>
  )
}
