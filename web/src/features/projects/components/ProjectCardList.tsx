import { Visibility } from '@mui/icons-material'
import { Button, Card, CardActions, CardContent, Chip, Stack, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { EmptyState } from '../../../shared/components/EmptyState'
import type { Project } from '../../../shared/types/domain'

type ProjectCardListProps = {
  projects: Project[]
  /** Engineer ロールではマッチ度を表示する */
  showMatchScore?: boolean
}

/** モバイル向けの案件カード一覧 */
export function ProjectCardList({ projects, showMatchScore = false }: ProjectCardListProps) {
  if (projects.length === 0) {
    return <EmptyState message="条件に合う案件がありません" />
  }

  return (
    <Stack spacing={1.5}>
      {projects.map((project) => (
        <Card key={project.id} variant="outlined">
          <CardContent sx={{ pb: 0.5 }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
              <Typography variant="h6" sx={{ lineHeight: 1.3 }}>
                {project.title}
              </Typography>
              <Chip
                size="small"
                color={project.status === '募集中' ? 'success' : project.status === '下書き' ? 'warning' : 'default'}
                label={project.status}
                sx={{ flexShrink: 0 }}
              />
            </Stack>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {project.client}
            </Typography>
            <Stack direction="row" spacing={2} sx={{ mt: 1, flexWrap: 'wrap' }}>
              <Typography variant="body2">{project.price}</Typography>
              <Typography variant="body2" color="text.secondary">
                開始 {project.startDate}
              </Typography>
              {showMatchScore ? (
                <Chip size="small" color="info" label={`マッチ度 ${project.matchScore}点`} />
              ) : (
                <Typography variant="body2" color="text.secondary">
                  アサイン {project.assigned}
                </Typography>
              )}
            </Stack>
          </CardContent>
          <CardActions>
            <Button size="small" startIcon={<Visibility />} component={RouterLink} to={`/projects/${project.id}`}>
              詳細
            </Button>
          </CardActions>
        </Card>
      ))}
    </Stack>
  )
}
