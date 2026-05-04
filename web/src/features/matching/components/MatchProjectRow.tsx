import { Visibility } from '@mui/icons-material'
import { Box, Button, LinearProgress, Paper, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import type { Project } from '../../../shared/types/domain'

type MatchProjectRowProps = {
  rank: number
  project: Project
  expanded?: boolean
}

export function MatchProjectRow({ rank, project, expanded }: MatchProjectRowProps) {
  return (
    <Paper variant="outlined" className="match-project-row">
      <Box className="score-badge">
        <Typography variant="caption">#{rank}</Typography>
        <Typography variant="h6">{project.matchScore}</Typography>
        <Typography variant="caption">点</Typography>
      </Box>
      <Box className="match-project-body">
        <Typography sx={{ fontWeight: 700 }}>
          {project.title}({project.client})
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {project.requiredSkills.map((skill) => skill.name).join('/')}、{project.price}、{project.startDate}〜
        </Typography>
        {expanded && (
          <>
            <LinearProgress variant="determinate" value={project.matchScore} className="score-progress" />
            <Typography variant="body2">{project.summary}</Typography>
          </>
        )}
      </Box>
      <Button component={RouterLink} to={`/projects/${project.id}`} endIcon={<Visibility />}>
        詳細
      </Button>
    </Paper>
  )
}
