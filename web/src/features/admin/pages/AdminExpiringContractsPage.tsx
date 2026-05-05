import { Visibility } from '@mui/icons-material'
import { Chip, IconButton, Stack, Tooltip } from '@mui/material'
import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { Link as RouterLink } from 'react-router-dom'
import { EmptyState } from '../../../shared/components/EmptyState'
import { LoadingState } from '../../../shared/components/LoadingState'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import type { ExpiringContract } from '../../contracts/api/contractApi'
import { useExpiringContracts } from '../../contracts/hooks/useContracts'

export function AdminExpiringContractsPage() {
  const { data: contracts = [], isLoading } = useExpiringContracts(30)

  const columns: GridColDef<ExpiringContract>[] = [
    { field: 'engineerName', headerName: 'エンジニア', flex: 0.9, minWidth: 150 },
    { field: 'projectTitle', headerName: '案件', flex: 1.3, minWidth: 220 },
    { field: 'clientName', headerName: 'クライアント', flex: 0.8, minWidth: 140 },
    { field: 'periodTo', headerName: '契約終了日', width: 130 },
    {
      field: 'daysRemaining',
      headerName: '残日数',
      width: 110,
      renderCell: ({ value }) => `残り${value}日`,
    },
    {
      field: 'renewalStatus',
      headerName: '更新状況',
      width: 140,
      renderCell: ({ value }) => <Chip size="small" color={value === '更新未定' ? 'warning' : 'success'} label={value} />,
    },
    {
      field: 'action',
      headerName: '',
      width: 90,
      sortable: false,
      renderCell: ({ row }) => (
        <Tooltip title="エンジニア詳細">
          <IconButton component={RouterLink} to={`/engineers/${row.engineerId}`} size="small">
            <Visibility fontSize="small" />
          </IconButton>
        </Tooltip>
      ),
    },
  ]

  if (isLoading) {
    return <LoadingState message="更新間近契約を読み込み中です。" />
  }

  return (
    <Stack spacing={2}>
      <PageHeader title="更新間近契約" subtitle="全社の30日以内に終了する契約を確認します" backTo="/" />
      <Section title="全社アラート">
        {contracts.length === 0 ? (
          <EmptyState message="更新間近の契約はありません。" />
        ) : (
          <DataGrid
            autoHeight
            disableRowSelectionOnClick
            density="compact"
            rows={contracts}
            columns={columns}
            pageSizeOptions={[10, 20]}
            initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
          />
        )}
      </Section>
    </Stack>
  )
}
