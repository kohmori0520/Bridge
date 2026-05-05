import { Groups, Visibility, WarningAmber, Work } from '@mui/icons-material'
import { Alert, Box, Button, Chip, IconButton, Stack, Tooltip } from '@mui/material'
import { DataGrid } from '@mui/x-data-grid'
import { Link as RouterLink } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { SummaryStats } from '../../../shared/components/SummaryStats'
import { CompactEngineerList } from '../../engineers/components/CompactEngineerList'
import { useEngineers } from '../../engineers/hooks/useEngineers'
import { CompactProjectList } from '../../projects/components/CompactProjectList'
import { useProjects } from '../../projects/hooks/useProjects'

export function SalesDashboard() {
  const { user } = useAuth()
  const isSales = user?.role === 'Sales'
  const myFilter = isSales ? { primarySalesId: 'me' as const } : {}
  const { data: projectPage } = useProjects(0, 3)
  const { data: engineerPage } = useEngineers(0, 3, myFilter)
  const myEngineers = engineerPage?.items ?? []
  const myEngineersTotal = engineerPage?.total ?? 0
  const myAvailableCount = myEngineers.filter((engineer) => engineer.status === '空き').length

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
      <Alert severity="warning" icon={<WarningAmber />} action={<Button component={RouterLink} to="/engineers/1">詳細</Button>}>
        契約更新アラート: 田中 太郎さんの契約終了まで残り23日・更新未定
      </Alert>
      <Section title="契約更新アラート(担当エンジニア)">
        <DataGrid
          autoHeight
          disableRowSelectionOnClick
          density="compact"
          rows={[
            { id: 1, engineer: '田中 太郎', project: 'A銀行案件', days: 23, status: '更新未定' },
            { id: 2, engineer: '鈴木 花子', project: 'C社案件', days: 15, status: '更新未定' },
            { id: 3, engineer: '山田 次郎', project: 'D社案件', days: 28, status: '調整済' },
          ]}
          columns={[
            { field: 'engineer', headerName: '技術者', flex: 1, minWidth: 140 },
            { field: 'project', headerName: '案件', flex: 1, minWidth: 140 },
            { field: 'days', headerName: '残日数', width: 110, renderCell: ({ value }) => `残り${value}日` },
            {
              field: 'status',
              headerName: '更新状況',
              width: 130,
              renderCell: ({ value }) => <Chip size="small" color={value === '更新未定' ? 'warning' : 'success'} label={value} />,
            },
            {
              field: 'action',
              headerName: '',
              width: 90,
              sortable: false,
              renderCell: () => (
                <Tooltip title="詳細">
                  <IconButton component={RouterLink} to="/engineers/1" size="small">
                    <Visibility fontSize="small" />
                  </IconButton>
                </Tooltip>
              ),
            },
          ]}
          pageSizeOptions={[5]}
          initialState={{ pagination: { paginationModel: { pageSize: 5 } } }}
        />
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
