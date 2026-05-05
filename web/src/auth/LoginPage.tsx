import { Login } from '@mui/icons-material'
import { Alert, Box, Button, FormControl, InputLabel, MenuItem, Paper, Select, Stack, TextField, Typography } from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from './useAuth'
import type { Role } from '../shared/types/domain'

export function LoginPage() {
  const { demoUsers, login } = useAuth()
  const [role, setRole] = useState<Role>('Engineer')
  const [email, setEmail] = useState(demoUsers.Engineer.email)
  const [password, setPassword] = useState('Engineer1234!')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const navigate = useNavigate()

  const handleRoleChange = (event: SelectChangeEvent) => {
    const nextRole = event.target.value as Role
    setRole(nextRole)
    setEmail(demoUsers[nextRole].email)
    setPassword(nextRole === 'Admin' ? 'Admin1234!' : nextRole === 'Sales' ? 'Sales1234!' : 'Engineer1234!')
  }

  const handleSubmit = async () => {
    setError(null)
    setIsSubmitting(true)
    try {
      await login({ email, password })
      navigate('/')
    } catch (e) {
      setError(e instanceof Error ? e.message : 'ログインに失敗しました')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Box className="login-page">
      <Paper className="login-panel">
        <Typography variant="h4">Bridge</Typography>
        <Typography color="text.secondary">営業とエンジニアの契約・案件・マッチング情報をひとつに集約します。</Typography>
        <Stack spacing={2}>
          <TextField label="メールアドレス" value={email} onChange={(event) => setEmail(event.target.value)} />
          <TextField label="パスワード" type="password" value={password} onChange={(event) => setPassword(event.target.value)} />
          <FormControl>
            <InputLabel id="role-label">ログイン種別</InputLabel>
            <Select labelId="role-label" value={role} label="ログイン種別" onChange={handleRoleChange}>
              <MenuItem value="Admin">管理者</MenuItem>
              <MenuItem value="Engineer">エンジニア</MenuItem>
              <MenuItem value="Sales">営業</MenuItem>
            </Select>
          </FormControl>
          <Button
            size="large"
            variant="contained"
            startIcon={<Login />}
            disabled={isSubmitting}
            onClick={handleSubmit}
          >
            {isSubmitting ? 'ログイン中...' : 'ログイン'}
          </Button>
        </Stack>
        {error && <Alert severity="error">{error}</Alert>}
        <Alert severity="info">開発用ユーザーのメールアドレスとパスワードを初期入力しています。</Alert>
      </Paper>
    </Box>
  )
}
