import { Save } from '@mui/icons-material'
import { Alert, Box, Button, MenuItem, Stack, TextField } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Link as RouterLink } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { useNotify } from '../../../shared/notifications/useNotify'
import type { Role } from '../../../shared/types/domain'
import { adminApi, type CreateUserInput } from '../api/adminApi'
import { useSalesOptions } from '../hooks/useAdmin'

const emptyForm: CreateUserInput = {
  email: '',
  password: '',
  role: 'Sales',
  name: '',
  department: '',
  primarySalesId: null,
}

export function AdminUserCreatePage() {
  const queryClient = useQueryClient()
  const notify = useNotify()
  const { data: salesOptions = [] } = useSalesOptions()
  const [form, setForm] = useState<CreateUserInput>(emptyForm)
  const [error, setError] = useState<string | null>(null)

  const mutation = useMutation({
    mutationFn: adminApi.createUser,
    onSuccess: async (createdUser) => {
      notify(`${createdUser.email} を作成しました`)
      setForm({ ...emptyForm, role: form.role })
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['engineers'] }),
        queryClient.invalidateQueries({ queryKey: ['admin', 'sales-options'] }),
      ])
    },
  })

  const updateForm = <K extends keyof CreateUserInput>(field: K, value: CreateUserInput[K]) => {
    setError(null)
    setForm((current) => ({ ...current, [field]: value }))
  }

  const handleRoleChange = (role: Role) => {
    updateForm('role', role)
    setForm((current) => ({
      ...current,
      role,
      department: role === 'Sales' ? current.department : '',
      primarySalesId: role === 'Engineer' ? current.primarySalesId : null,
    }))
  }

  const handleSubmit = () => {
    setError(null)

    if (!form.email.trim() || !form.password.trim() || !form.name.trim()) {
      setError('メールアドレス、パスワード、氏名は必須です。')
      return
    }

    if (form.password.length < 8) {
      setError('パスワードは8文字以上で入力してください。')
      return
    }

    if (form.role === 'Engineer' && !form.primarySalesId) {
      setError('エンジニア作成時は担当営業を選択してください。')
      return
    }

    mutation.mutate(form)
  }

  return (
    <Stack spacing={2}>
      <PageHeader title="ユーザー作成" subtitle="Admin / 営業 / エンジニアのアカウントを作成します" backTo="/" />
      {error && <Alert severity="error">{error}</Alert>}
      {mutation.error && <Alert severity="error">{mutation.error instanceof Error ? mutation.error.message : 'ユーザー作成に失敗しました'}</Alert>}
      <Section title="アカウント情報">
        <Box className="form-grid">
          <TextField select label="ロール" value={form.role} onChange={(event) => handleRoleChange(event.target.value as Role)}>
            <MenuItem value="Admin">Admin</MenuItem>
            <MenuItem value="Sales">営業</MenuItem>
            <MenuItem value="Engineer">エンジニア</MenuItem>
          </TextField>
          <TextField label="氏名" value={form.name} onChange={(event) => updateForm('name', event.target.value)} />
          <TextField label="メールアドレス" type="email" value={form.email} onChange={(event) => updateForm('email', event.target.value)} />
          <TextField label="初期パスワード" type="password" value={form.password} helperText="8文字以上" onChange={(event) => updateForm('password', event.target.value)} />
          {form.role === 'Sales' && (
            <TextField label="部署" value={form.department} onChange={(event) => updateForm('department', event.target.value)} />
          )}
          {form.role === 'Engineer' && (
            <TextField
              select
              label="担当営業"
              value={form.primarySalesId ?? ''}
              onChange={(event) => updateForm('primarySalesId', Number(event.target.value))}
            >
              {salesOptions.map((sales) => (
                <MenuItem key={sales.id} value={sales.id}>
                  {sales.name} ({sales.department || '部署未設定'})
                </MenuItem>
              ))}
            </TextField>
          )}
        </Box>
      </Section>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button component={RouterLink} to="/">
          キャンセル
        </Button>
        <Button variant="contained" startIcon={<Save />} disabled={mutation.isPending} onClick={handleSubmit}>
          {mutation.isPending ? '作成中...' : '作成'}
        </Button>
      </Stack>
    </Stack>
  )
}
