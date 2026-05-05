import { Add, Visibility } from '@mui/icons-material'
import { Button, Chip, IconButton, Stack, Tooltip } from '@mui/material'
import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { useMemo, useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { ApiErrorAlert } from '../../../shared/components/ApiErrorAlert'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { SummaryStats } from '../../../shared/components/SummaryStats'
import { ToolbarPanel } from '../../../shared/components/ToolbarPanel'
import { useEngineerProjectList, useProjects } from '../hooks/useProjects'
import type { Project, Role } from '../../../shared/types/domain'

export function ProjectsPage() {
  const { user } = useAuth()
  const role: Role = user?.role ?? 'Engineer'
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 })
  const [keyword, setKeyword] = useState('')
  const [status, setStatus] = useState('all')
  const salesProjectsQuery = useProjects(
    paginationModel.page,
    paginationModel.pageSize,
    role === 'Sales' || role === 'Admin' ? undefined : 'open',
  )
  const engineerProjectsQuery = useEngineerProjectList(paginationModel.page, paginationModel.pageSize)
  const { data, error, isLoading } = role === 'Engineer' ? engineerProjectsQuery : salesProjectsQuery
  const columns: GridColDef<Project>[] = [
    { field: 'title', headerName: role === 'Sales' ? 'タイトル' : '案件名', flex: 1.2, minWidth: 210 },
    { field: 'client', headerName: 'クライアント', flex: 0.8, minWidth: 140 },
    {
      field: 'status',
      headerName: 'ステータス',
      width: 120,
      renderCell: ({ value }) => (
        <Chip
          size="small"
          color={value === '募集中' ? 'success' : value === '下書き' ? 'warning' : 'default'}
          label={value}
        />
      ),
    },
    ...(role === 'Engineer'
      ? [
          {
            field: 'matchScore',
            headerName: 'マッチ度',
            width: 130,
            renderCell: ({ value }: { value?: number }) => `${value ?? 0}点`,
          } satisfies GridColDef<Project>,
        ]
      : [{ field: 'assigned', headerName: 'アサイン', width: 120 } satisfies GridColDef<Project>]),
    { field: 'price', headerName: '単価', flex: 0.8, minWidth: 150 },
    { field: 'startDate', headerName: '開始日', width: 130 },
    {
      field: 'action',
      headerName: '',
      width: 90,
      sortable: false,
      renderCell: ({ row }) => (
        <Tooltip title="詳細">
          <IconButton component={RouterLink} to={`/projects/${row.id}`} size="small">
            <Visibility fontSize="small" />
          </IconButton>
        </Tooltip>
      ),
    },
  ]
  const rows = useMemo(() => {
    const normalizedKeyword = keyword.trim().toLowerCase()
    const filtered = (data?.items ?? []).filter((project) => {
      const matchesKeyword =
        normalizedKeyword.length === 0 ||
        [project.title, project.client, project.owner, project.summary, ...project.requiredSkills.map((skill) => skill.name), ...project.welcomeSkills.map((skill) => skill.name)]
          .join(' ')
          .toLowerCase()
          .includes(normalizedKeyword)
      const matchesStatus =
        status === 'all' ||
        (status === 'open' && project.status === '募集中') ||
        (status === 'draft' && project.status === '下書き') ||
        (status === 'closed' && project.status === 'クローズ')
      return matchesKeyword && matchesStatus
    })

    return role === 'Engineer' ? filtered.slice().sort((a, b) => b.matchScore - a.matchScore) : filtered
  }, [data?.items, keyword, role, status])
  const pageItems = data?.items ?? []
  const openCount = pageItems.filter((project) => project.status === '募集中').length
  const draftCount = pageItems.filter((project) => project.status === '下書き').length
  const closedCount = pageItems.filter((project) => project.status === 'クローズ').length

  return (
    <Stack spacing={2}>
      <PageHeader
        title={role === 'Sales' ? '案件一覧' : '公開案件一覧'}
        subtitle={role === 'Sales' ? '担当案件の募集状況' : 'マッチ度順に公開案件を表示'}
        actions={
          role === 'Sales' ? (
            <Button component={RouterLink} to="/projects/new" variant="contained" startIcon={<Add />}>
              案件作成
            </Button>
          ) : undefined
        }
      />
      <SummaryStats
        items={[
          { label: '表示中', value: `${rows.length}件` },
          { label: '募集中', value: `${openCount}件`, tone: 'success' },
          ...(role === 'Sales' ? [{ label: '下書き', value: `${draftCount}件`, tone: 'warning' as const }] : []),
          { label: 'クローズ', value: `${closedCount}件` },
        ]}
      />
      <ToolbarPanel
        keyword={keyword}
        status={status}
        statusOptions={[
          { value: 'all', label: 'すべて' },
          { value: 'draft', label: '下書き' },
          { value: 'open', label: '募集中' },
          { value: 'closed', label: 'クローズ' },
        ]}
        keywordPlaceholder="案件名・クライアント・スキル"
        onKeywordChange={setKeyword}
        onStatusChange={setStatus}
        resultLabel={`${rows.length} / ${data?.total ?? 0} 件`}
      />
      {error ? <ApiErrorAlert error={error} fallbackMessage="案件一覧の取得に失敗しました。" /> : null}
      <Section title={role === 'Sales' ? '案件' : '公開案件'}>
        <DataGrid
          autoHeight
          disableRowSelectionOnClick
          density="compact"
          rows={rows}
          columns={columns}
          loading={isLoading}
          paginationMode="server"
          rowCount={keyword || status !== 'all' ? rows.length : data?.total ?? 0}
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 20]}
          localeText={{ noRowsLabel: '条件に合う案件がありません' }}
        />
      </Section>
    </Stack>
  )
}
