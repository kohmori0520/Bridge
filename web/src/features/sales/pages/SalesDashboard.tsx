import { Groups, Visibility, WarningAmber, Work } from '@mui/icons-material'
import { Alert, Box, Button, Chip, IconButton, Stack, Tooltip } from '@mui/material'
import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { Link as RouterLink } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { EmptyState } from '../../../shared/components/EmptyState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { SummaryStats } from '../../../shared/components/SummaryStats'
import { useIsMobile } from '../../../shared/hooks/useIsMobile'
import type { ExpiringContract } from '../../contracts/api/contractApi'
import { ExpiringContractCardList } from '../../contracts/components/ExpiringContractCardList'
import { useMyExpiringContracts } from '../../contracts/hooks/useContracts'
import { CompactEngineerList } from '../../engineers/components/CompactEngineerList'
import { useEngineers } from '../../engineers/hooks/useEngineers'
import { CompactProjectList } from '../../projects/components/CompactProjectList'
import { useProjects } from '../../projects/hooks/useProjects'

const expiringColumns: GridColDef<ExpiringContract>[] = [
  { field: 'engineerName', headerName: '技術者', flex: 1, minWidth: 140 },
  { field: 'projectTitle', headerName: '案件', flex: 1, minWidth: 140 },
  { field: 'days', headerName: '残日数', width: 110, renderCell: ({ row }) => `残り${row.daysRemaining}日` },
  {
    field: 'renewalStatus',
    headerName: '更新状況',
    width: 130,
    renderCell: ({ value }) => <Chip size="small" color={value === '更新未定' ? 'warning' : 'success'} label={value} />,
  },
  {
    field: 'action',
    headerName: '',
    width: 90,
    sortable: false,
    renderCell: ({ row }) => (
      <Tooltip title="詳細">
        <IconButton component={RouterLink} to={`/engineers/${row.engineerId}`} size="small">
          <Visibility fontSize="small" />
        </IconButton>
      </Tooltip>
    ),
  },
]

export function SalesDashboard() {
  const { user } = useAuth()
  const isSales = user?.role === 'Sales'
  const isMobile = useIsMobile()
  const myFilter = isSales ? { primarySalesId: 'me' as const } : {}
  const { data: projectPage } = useProjects(0, 3)
  const { data: engineerPage } = useEngineers(0, 3, myFilter)
  const { data: expiringContracts = [] } = useMyExpiringContracts(30, isSales)
  const myEngineers = engineerPage?.items ?? []
  const myEngineersTotal = engineerPage?.total ?? 0
  const myAvailableCount = myEngineers.filter((engineer) => engineer.status === '空き').length
  const mostUrgent = expiringContracts.reduce<ExpiringContract | null>(
    (urgent, contract) => (urgent === null || contract.daysRemaining < urgent.daysRemaining ? contract : urgent),
    null,
  )

  return (
    <Stack spacing={2}>
      <PageHeader title="営業ダッシュボード" subtitle="担当案件と契約更新アラート" />
      {isSales && (
        <SummaryStats
          items={[
            { label: '自分の担当エンジニア', value: `${myEngineersTotal}名` },
            { label: '空き', value: `${myAvailableCount}名`, tone: 'success' },
            { label: '稼働中', value: `${Math.max(0, myEngineersTotal - myAvailableCount)}名` },
          ]}
        />
      )}
      {mostUrgent && (
        <Alert
          severity="warning"
          icon={<WarningAmber />}
          action={<Button component={RouterLink} to={`/engineers/${mostUrgent.engineerId}`}>詳細</Button>}
        >
          契約更新アラート: {mostUrgent.engineerName}さんの契約終了まで残り{mostUrgent.daysRemaining}日・
          {mostUrgent.renewalStatus}
        </Alert>
      )}
      <Section title="契約更新アラート(担当エンジニア)">
        {expiringContracts.length === 0 ? (
          <EmptyState message="30日以内に終了する担当エンジニアの契約はありません。" />
        ) : isMobile ? (
          <ExpiringContractCardList contracts={expiringContracts} />
        ) : (
          <DataGrid
            autoHeight
            disableRowSelectionOnClick
            density="compact"
            rows={expiringContracts}
            columns={expiringColumns}
            pageSizeOptions={[5]}
            initialState={{ pagination: { paginationModel: { pageSize: 5 } } }}
          />
        )}
      </Section>
      <Box className="two-column">
        <Section title="あなたの担当案件" action={<Button component={RouterLink} to="/projects" endIcon={<Work />}>すべて</Button>}>
          <CompactProjectList projects={projectPage?.items ?? []} />
        </Section>
        <Section title="あなたの担当エンジニア(稼働状況)" action={<Button component={RouterLink} to="/engineers" endIcon={<Groups />}>すべて</Button>}>
          <CompactEngineerList engineers={myEngineers} />
        </Section>
      </Box>
    </Stack>
  )
}
