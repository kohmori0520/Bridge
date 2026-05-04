import { Stack } from '@mui/material'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { MatchProjectRow } from '../../matching/components/MatchProjectRow'
import { useMyProjectMatches } from '../../projects/hooks/useProjects'

export function MyMatchesPage() {
  const { data: sortedProjects = [] } = useMyProjectMatches()

  return (
    <Stack spacing={2}>
      <PageHeader title="自分へのマッチング一覧" subtitle="希望スキル・カテゴリとの一致度が高い順" />
      <Section title="マッチング案件">
        <Stack spacing={1.5}>
          {sortedProjects.map((project, index) => (
            <MatchProjectRow key={project.id} rank={index + 1} project={project} expanded />
          ))}
        </Stack>
      </Section>
    </Stack>
  )
}
