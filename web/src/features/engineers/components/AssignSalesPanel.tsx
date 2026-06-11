import { Save } from '@mui/icons-material'
import { Autocomplete, Button, Stack, TextField } from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { adminApi, type SalesOption } from '../../admin/api/adminApi'
import { ApiErrorAlert } from '../../../shared/components/ApiErrorAlert'
import { useNotify } from '../../../shared/notifications/useNotify'
import { engineerApi } from '../api/engineerApi'

type AssignSalesPanelProps = {
  engineerId: number
  currentSalesId: number | null
}

export function AssignSalesPanel({ engineerId, currentSalesId }: AssignSalesPanelProps) {
  const queryClient = useQueryClient()
  const notify = useNotify()
  const { data: salesOptions = [], isLoading } = useQuery({
    queryKey: ['admin', 'sales'],
    queryFn: () => adminApi.listSales(),
  })
  const [selected, setSelected] = useState<SalesOption | null>(null)
  const initialSelected = salesOptions.find((option) => option.id === currentSalesId) ?? null
  const value = selected ?? initialSelected
  const isDirty = (value?.id ?? null) !== currentSalesId

  const mutation = useMutation({
    mutationFn: (salesId: number | null) => engineerApi.assignSales(engineerId, salesId),
    onSuccess: async () => {
      notify('担当営業を更新しました')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['engineers'] }),
        queryClient.invalidateQueries({ queryKey: ['engineers', engineerId] }),
      ])
    },
  })

  return (
    <Stack spacing={1}>
      <Autocomplete
        size="small"
        options={salesOptions}
        loading={isLoading}
        value={value}
        getOptionLabel={(option) => `${option.name}(${option.department})`}
        isOptionEqualToValue={(option, candidate) => option.id === candidate.id}
        onChange={(_, next) => {
          setSelected(next ?? null)
        }}
        renderInput={(params) => <TextField {...params} label="担当営業" placeholder="未選択で担当を外す" />}
      />
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button
          variant="contained"
          size="small"
          startIcon={<Save />}
          disabled={!isDirty || mutation.isPending}
          onClick={() => mutation.mutate(value?.id ?? null)}
        >
          {mutation.isPending ? '保存中...' : '保存'}
        </Button>
      </Stack>
      {mutation.error && <ApiErrorAlert error={mutation.error} fallbackMessage="担当営業の更新に失敗しました" />}
    </Stack>
  )
}
