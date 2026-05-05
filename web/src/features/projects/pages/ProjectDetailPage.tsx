import { Close, Edit } from '@mui/icons-material'
import { Alert, Button, Stack } from '@mui/material'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { Info } from '../../../shared/components/Info'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { MatchCandidateCard } from '../../matching/components/MatchCandidateCard'
import { MatchProjectRow } from '../../matching/components/MatchProjectRow'
import { useProjectMatches } from '../../matching/hooks/useMatching'
import { SkillBlock } from '../components/SkillBlock'
import { useProject } from '../hooks/useProjects'

export function ProjectDetailPage() {
  const { user } = useAuth()
  const { id } = useParams()
  const projectId = Number(id) || 1
  const { data: project, isLoading } = useProject(projectId)
  const { data: matchCandidates = [] } = useProjectMatches(projectId)

  if (isLoading) {
    return <Alert severity="info">案件情報を読み込み中です。</Alert>
  }

  if (!project) {
    return <Alert severity="error">案件が見つかりませんでした。</Alert>
  }

  return (
    <Stack spacing={2}>
      <PageHeader
        title={project.title}
        subtitle={`${project.client} ・ ${project.status} ・ 担当:${project.owner}`}
        backTo="/projects"
        actions={
          user?.role === 'Sales' ? (
            <>
              <Button component={RouterLink} to={`/projects/${project.id}/edit`} startIcon={<Edit />}>編集</Button>
              <Button color="inherit" startIcon={<Close />}>閉じる</Button>
            </>
          ) : undefined
        }
      />
      <Section title="案件情報">
        <div className="detail-grid">
          <Info label="期間" value={project.period} />
          <Info label="単価" value={project.price} />
          <Info label="概要" value={project.summary} wide />
          <SkillBlock title="必須スキル" skills={project.requiredSkills} />
          <SkillBlock title="歓迎スキル" skills={project.welcomeSkills} />
        </div>
      </Section>
      {user?.role === 'Sales' ? (
        <Section title="マッチング候補(スコア順)">
          <Stack spacing={2}>
            {matchCandidates.map((candidate) => (
              <MatchCandidateCard key={candidate.rank} candidate={candidate} />
            ))}
          </Stack>
        </Section>
      ) : (
        <Section title="あなたとのマッチング">
          <MatchProjectRow rank={1} project={project} expanded />
        </Section>
      )}
    </Stack>
  )
}
