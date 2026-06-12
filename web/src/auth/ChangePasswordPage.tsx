import { Save } from '@mui/icons-material'
import { Alert, Button, Stack, TextField } from '@mui/material'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { PageHeader } from '../shared/components/PageHeader'
import { Section } from '../shared/components/Section'
import { useNotify } from '../shared/notifications/useNotify'
import { authApi } from './authApi'

const emptyForm = {
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
}

export function ChangePasswordPage() {
  const notify = useNotify()
  const navigate = useNavigate()
  const [form, setForm] = useState(emptyForm)
  const [error, setError] = useState<string | null>(null)

  const mutation = useMutation({
    mutationFn: () => authApi.changePassword(form.currentPassword, form.newPassword),
    onSuccess: () => {
      notify('パスワードを変更しました')
      navigate('/')
    },
    onError: (err) => setError(err.message),
  })

  const updateForm = (field: keyof typeof emptyForm, value: string) => {
    setError(null)
    setForm((current) => ({ ...current, [field]: value }))
  }

  const handleSubmit = () => {
    if (!form.currentPassword || !form.newPassword || !form.confirmPassword) {
      setError('すべての項目を入力してください。')
      return
    }
    if (form.newPassword.length < 8) {
      setError('新しいパスワードは8文字以上で入力してください。')
      return
    }
    if (form.newPassword !== form.confirmPassword) {
      setError('新しいパスワード(確認)が一致しません。')
      return
    }
    mutation.mutate()
  }

  return (
    <Stack spacing={3}>
      <PageHeader title="パスワード変更" subtitle="ログインに使うパスワードを変更します" />
      <Section title="新しいパスワードの設定">
        <Stack spacing={2} sx={{ maxWidth: 420 }}>
          {error && <Alert severity="error">{error}</Alert>}
          <TextField
            label="現在のパスワード"
            type="password"
            autoComplete="current-password"
            value={form.currentPassword}
            onChange={(event) => updateForm('currentPassword', event.target.value)}
          />
          <TextField
            label="新しいパスワード"
            type="password"
            autoComplete="new-password"
            helperText="8文字以上"
            value={form.newPassword}
            onChange={(event) => updateForm('newPassword', event.target.value)}
          />
          <TextField
            label="新しいパスワード(確認)"
            type="password"
            autoComplete="new-password"
            value={form.confirmPassword}
            onChange={(event) => updateForm('confirmPassword', event.target.value)}
          />
          <Button
            variant="contained"
            startIcon={<Save />}
            onClick={handleSubmit}
            disabled={mutation.isPending}
            sx={{ alignSelf: 'flex-start' }}
          >
            変更する
          </Button>
        </Stack>
      </Section>
    </Stack>
  )
}
