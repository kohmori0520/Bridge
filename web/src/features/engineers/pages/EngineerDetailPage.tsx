import { Edit } from '@mui/icons-material'
import { Box, Button, Stack } from '@mui/material'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { ChipList } from '../../../shared/components/ChipList'
import { EmptyState } from '../../../shared/components/EmptyState'
import { Info } from '../../../shared/components/Info'
import { LoadingState } from '../../../shared/components/LoadingState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { AssignSalesPanel } from '../components/AssignSalesPanel'
import { ContractList } from '../../contracts/components/ContractList'
import { useEngineer } from '../hooks/useEngineers'

export function EngineerDetailPage() {
  const { user } = useAuth()
  const { id } = useParams()
  const engineerId = Number(id)
  const isValidId = Number.isFinite(engineerId) && engineerId > 0
  const { data: engineer, isLoading } = useEngineer(isValidId ? engineerId : 0, {
    enabled: isValidId,
  })

  if (!isValidId) {
    return <EmptyState message="技術者が見つかりませんでした。" />
  }

  if (isLoading) {
    return <LoadingState message="技術者情報を読み込み中です。" />
  }

  if (!engineer) {
    return <EmptyState message="技術者が見つかりませんでした。" />
  }

  return (
    <Stack spacing={2}>
      <PageHeader
        title={engineer.name}
        subtitle={`${engineer.status} ・ 担当:${engineer.sales}`}
        backTo="/engineers"
        actions={<Button component={RouterLink} to={`/engineers/${engineer.id}/edit`} startIcon={<Edit />}>編集</Button>}
      />
      <Box className="two-column">
        <Section title="プロフィール">
          <div className="detail-grid single">
            <Info label="現在の案件" value={engineer.project} />
            <Info label="空き予定" value={engineer.availableFrom} />
            <Info label="単価" value={engineer.unitPrice} />
            <Info label="担当営業" value={engineer.sales} />
          </div>
        </Section>
        <Section title="キャリア志向">
          <Stack spacing={1}>
            <ChipList label="保有スキル" values={engineer.skills.map((skill) => `${skill.name} ${skill.years}年`)} />
            <ChipList label="やりたい技術" values={engineer.desiredTechnologies} color="success" />
            <ChipList label="関わりたい領域" values={engineer.desiredAreas} color="success" />
            <ChipList label="希望役割" values={engineer.desiredRoles} color="success" />
            <ChipList label="避けたい業務" values={engineer.avoid} color="warning" />
          </Stack>
        </Section>
      </Box>
      {user?.role === 'Admin' && (
        <Section title="担当営業の変更(管理者)">
          <AssignSalesPanel engineerId={engineer.id} currentSalesId={engineer.primarySalesId} />
        </Section>
      )}
      <Section title="契約履歴">
        <ContractList />
      </Section>
    </Stack>
  )
}
