import { Add, Visibility } from '@mui/icons-material'
import { Button, Chip, IconButton, Stack, Tooltip } from '@mui/material'
import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { ToolbarPanel } from '../../../shared/components/ToolbarPanel'
import { useEngineers } from '../hooks/useEngineers'
import type { Engineer } from '../../../shared/types/domain'

export function EngineersPage() {
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 })
  const { data, isLoading } = useEngineers(paginationModel.page, paginationModel.pageSize)
  const columns: GridColDef<Engineer>[] = [
    { field: 'name', headerName: '氏名', flex: 1, minWidth: 150 },
    {
      field: 'status',
      headerName: '稼働状況',
      width: 120,
      renderCell: ({ value }) => <Chip size="small" color={value === '空き' ? 'success' : 'primary'} label={value} />,
    },
    { field: 'project', headerName: '現在の案件', flex: 1.2, minWidth: 200 },
    { field: 'availableFrom', headerName: '空き予定', width: 130 },
    { field: 'sales', headerName: '担当営業', width: 130 },
    { field: 'unitPrice', headerName: '単価', width: 100 },
    {
      field: 'action',
      headerName: '',
      width: 90,
      sortable: false,
      renderCell: ({ row }) => (
        <Tooltip title="詳細">
          <IconButton component={RouterLink} to={`/engineers/${row.id}`} size="small">
            <Visibility fontSize="small" />
          </IconButton>
        </Tooltip>
      ),
    },
  ]

  return (
    <Stack spacing={2}>
      <PageHeader
        title="技術者一覧"
        subtitle="担当エンジニアの稼働状況とスキル"
        actions={<Button component={RouterLink} to="/engineers/new" variant="contained" startIcon={<Add />}>技術者作成</Button>}
      />
      <ToolbarPanel />
      <Section title="技術者">
        <DataGrid
          autoHeight
          disableRowSelectionOnClick
          density="compact"
          rows={data?.items ?? []}
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
