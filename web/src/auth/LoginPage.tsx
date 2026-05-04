import { Login } from '@mui/icons-material'
import { Alert, Box, Button, FormControl, InputLabel, MenuItem, Paper, Select, Stack, TextField, Typography } from '@mui/material'
import type { SelectChangeEvent } from '@mui/material'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from './useAuth'
import type { Role } from '../shared/types/domain'

export function LoginPage() {
  const { demoUsers, loginAs } = useAuth()
  const [role, setRole] = useState<Role>('Engineer')
  const navigate = useNavigate()

  const handleRoleChange = (event: SelectChangeEvent) => {
    setRole(event.target.value as Role)
  }

  return (
    <Box className="login-page">
      <Paper className="login-panel">
        <Typography variant="h4">Bridge</Typography>
        <Typography color="text.secondary">営業とエンジニアの契約・案件・マッチング情報をひとつに集約します。</Typography>
        <Stack spacing={2}>
          <TextField key={role} label="メールアドレス" defaultValue={demoUsers[role].email} />
          <TextField label="パスワード" type="password" defaultValue="password" />
          <FormControl>
            <InputLabel id="role-label">ログイン種別</InputLabel>
            <Select labelId="role-label" value={role} label="ログイン種別" onChange={handleRoleChange}>
              <MenuItem value="Engineer">エンジニア</MenuItem>
              <MenuItem value="Sales">営業</MenuItem>
            </Select>
          </FormControl>
          <Button
            size="large"
            variant="contained"
            startIcon={<Login />}
            onClick={() => {
              loginAs(role)
              navigate('/')
            }}
          >
            ログイン
          </Button>
        </Stack>
        <Alert severity="info">バックエンド認証が接続されるまでは、デモユーザーで画面遷移を確認できます。</Alert>
      </Paper>
    </Box>
  )
}
