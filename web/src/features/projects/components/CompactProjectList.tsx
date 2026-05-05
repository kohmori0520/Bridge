import { Visibility } from '@mui/icons-material'
import { Box, Divider, IconButton, Stack, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import type { Project } from '../../../shared/types/domain'

type CompactProjectListProps = {
  projects: Project[]
}

export function CompactProjectList({ projects }: CompactProjectListProps) {
  if (projects.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        表示できる案件はまだありません。
      </Typography>
    )
  }

  return (
    <Stack divider={<Divider />} spacing={1}>
      {projects.map((project) => (
        <Box className="compact-row" key={project.id}>
          <Box>
            <Typography sx={{ fontWeight: 700 }}>{project.title}</Typography>
            <Typography variant="body2" color="text.secondary">
              {project.client} ・ {project.status} ・ {project.assigned}
            </Typography>
          </Box>
          <IconButton component={RouterLink} to={`/projects/${project.id}`} size="small">
            <Visibility fontSize="small" />
          </IconButton>
        </Box>
      ))}
    </Stack>
  )
}
