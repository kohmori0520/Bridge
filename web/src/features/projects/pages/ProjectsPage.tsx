import { Add, Visibility } from '@mui/icons-material'
import { Button, Chip, IconButton, Stack, Tooltip } from '@mui/material'
import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { ToolbarPanel } from '../../../shared/components/ToolbarPanel'
import { useProjects } from '../hooks/useProjects'
import type { Project, Role } from '../../../shared/types/domain'

export function ProjectsPage() {
  const { user } = useAuth()
  const role: Role = user?.role ?? 'Engineer'
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 })
  const { data, isLoading } = useProjects(paginationModel.page, paginationModel.pageSize)
  const columns: GridColDef<Project>[] = [
    { field: 'title', headerName: role === 'Sales' ? 'タイトル' : '案件名', flex: 1.2, minWidth: 210 },
    { field: 'client', headerName: 'クライアント', flex: 0.8, minWidth: 140 },
    {
      field: 'status',
      headerName: 'ステータス',
      width: 120,
      renderCell: ({ value }) => <Chip size="small" color={value === '募集中' ? 'success' : 'default'} label={value} />,
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
  const rows = role === 'Engineer' ? (data?.items ?? []).slice().sort((a, b) => b.matchScore - a.matchScore) : (data?.items ?? [])

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
      <ToolbarPanel />
      <Section title={role === 'Sales' ? '案件' : '公開案件'}>
        <DataGrid
          autoHeight
          disableRowSelectionOnClick
          density="compact"
          rows={rows}
          columns={columns}
          loading={isLoading}
          paginationMode="server"
          rowCount={data?.total ?? 0}
          paginationModel={paginationModel}
          onPaginationModelChange={setPaginationModel}
          pageSizeOptions={[10, 20]}
        />
      </Section>
    </Stack>
  )
}
