import { Groups, History, ManageAccounts } from '@mui/icons-material'
import { Alert, Box, Button, Stack } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { useExpiringContracts } from '../../contracts/hooks/useContracts'

export function AdminDashboard() {
  const { data: expiringContracts = [] } = useExpiringContracts(30)

  return (
    <Stack spacing={2}>
      <PageHeader title="Admin ダッシュボード" subtitle="アカウント管理と全社契約アラート" />
      {expiringContracts.length > 0 && (
        <Alert
          severity="warning"
          action={<Button component={RouterLink} to="/admin/contracts/expiring">確認</Button>}
        >
          30日以内に終了する契約が {expiringContracts.length} 件あります。
        </Alert>
      )}
      <Box className="two-column">
        <Section title="ユーザー管理">
          <Stack spacing={1}>
            <Button component={RouterLink} to="/admin/users/new" variant="contained" startIcon={<ManageAccounts />}>
              ユーザーを作成
            </Button>
            <Button component={RouterLink} to="/engineers" startIcon={<Groups />}>
              技術者一覧を見る
            </Button>
          </Stack>
        </Section>
        <Section title="契約管理">
          <Stack spacing={1}>
            <Button component={RouterLink} to="/admin/contracts/expiring" variant="contained" startIcon={<History />}>
              更新間近契約を見る
            </Button>
          </Stack>
        </Section>
      </Box>
    </Stack>
  )
}
