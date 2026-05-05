import { Visibility } from '@mui/icons-material'
import { Box, Divider, IconButton, Stack, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import type { Engineer } from '../../../shared/types/domain'

type CompactEngineerListProps = {
  engineers: Engineer[]
}

export function CompactEngineerList({ engineers }: CompactEngineerListProps) {
  if (engineers.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        表示できる技術者はまだいません。
      </Typography>
    )
  }

  return (
    <Stack divider={<Divider />} spacing={1}>
      {engineers.map((engineer) => (
        <Box className="compact-row" key={engineer.id}>
          <Box>
            <Typography sx={{ fontWeight: 700 }}>{engineer.name}</Typography>
            <Typography variant="body2" color="text.secondary">
              {engineer.status} ・ {engineer.project} ・ {engineer.availableFrom}〜
            </Typography>
          </Box>
          <IconButton component={RouterLink} to={`/engineers/${engineer.id}`} size="small">
            <Visibility fontSize="small" />
          </IconButton>
        </Box>
      ))}
    </Stack>
  )
}
