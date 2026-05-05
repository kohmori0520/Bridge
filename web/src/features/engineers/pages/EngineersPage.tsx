import { Add, Visibility } from '@mui/icons-material'
import { Button, Chip, FormControlLabel, IconButton, Stack, Switch, Tooltip } from '@mui/material'
import { DataGrid, type GridColDef } from '@mui/x-data-grid'
import { useMemo, useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import { useAuth } from '../../../auth/useAuth'
import { ApiErrorAlert } from '../../../shared/components/ApiErrorAlert'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { SummaryStats } from '../../../shared/components/SummaryStats'
import { ToolbarPanel } from '../../../shared/components/ToolbarPanel'
import { useEngineers } from '../hooks/useEngineers'
import type { Engineer } from '../../../shared/types/domain'

export function EngineersPage() {
  const { user } = useAuth()
  const isSales = user?.role === 'Sales'
  const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 })
  const [keyword, setKeyword] = useState('')
  const [status, setStatus] = useState('all')
  const [mineOnly, setMineOnly] = useState(isSales)
  const { data, error, isLoading } = useEngineers(paginationModel.page, paginationModel.pageSize, {
    primarySalesId: mineOnly && isSales ? 'me' : undefined,
  })
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
  const rows = useMemo(() => {
    const normalizedKeyword = keyword.trim().toLowerCase()
    return (data?.items ?? []).filter((engineer) => {
      const matchesKeyword =
        normalizedKeyword.length === 0 ||
        [engineer.name, engineer.project, engineer.sales, ...engineer.skills.map((skill) => skill.name), ...engineer.categories, ...engineer.avoid]
          .join(' ')
          .toLowerCase()
          .includes(normalizedKeyword)
      const matchesStatus = status === 'all' || (status === 'available' ? engineer.status === '空き' : engineer.status === '稼働中')
      return matchesKeyword && matchesStatus
    })
  }, [data?.items, keyword, status])
  const pageItems = data?.items ?? []
  const availableCount = pageItems.filter((engineer) => engineer.status === '空き').length
  const activeCount = pageItems.filter((engineer) => engineer.status === '稼働中').length

  return (
    <Stack spacing={2}>
      <PageHeader
        title="技術者一覧"
        subtitle="担当エンジニアの稼働状況とスキル"
        actions={
          <Button component={RouterLink} to="/engineers/new" variant="contained" startIcon={<Add />}>
            技術者作成
          </Button>
        }
      />
      <SummaryStats
        items={[
          { label: '表示中', value: `${rows.length}名` },
          { label: '空き', value: `${availableCount}名`, tone: 'success' },
          { label: '稼働中', value: `${activeCount}名` },
        ]}
      />
      <ToolbarPanel
        keyword={keyword}
        status={status}
        statusOptions={[
          { value: 'all', label: 'すべて' },
          { value: 'active', label: '稼働中' },
          { value: 'available', label: '空き' },
        ]}
        keywordPlaceholder="氏名・案件名・スキル"
        onKeywordChange={setKeyword}
        onStatusChange={setStatus}
        resultLabel={`${rows.length} / ${data?.total ?? 0} 名`}
        extra={
          isSales ? (
            <FormControlLabel
              control={<Switch checked={mineOnly} onChange={(event) => setMineOnly(event.target.checked)} />}
              label="自分担当のみ"
            />
          ) : undefined
        }
      />
      {error ? <ApiErrorAlert error={error} fallbackMessage="技術者一覧の取得に失敗しました。" /> : null}
      <Section title="技術者">
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
          localeText={{ noRowsLabel: '条件に合う技術者がありません' }}
        />
      </Section>
    </Stack>
  )
}
