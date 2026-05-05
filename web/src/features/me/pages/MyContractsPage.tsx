import { MailOutlined } from '@mui/icons-material'
import { Alert, Button, Card, CardContent, Stack, Typography } from '@mui/material'
import { ContractList } from '../../contracts/components/ContractList'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { useMyEngineer } from '../../engineers/hooks/useEngineers'

export function MyContractsPage() {
  const { data: me } = useMyEngineer()
  const projectName = me?.project && me.project !== '-' ? me.project : '案件未割当'
  const period = me?.availableFrom && me.availableFrom !== '-' ? `〜 ${me.availableFrom}` : '-'
  const unitPrice = me?.unitPrice ?? '-'
  const salesName = me?.sales ?? '未設定'

  return (
    <Stack spacing={2}>
      <PageHeader title="契約履歴" backTo="/" />
      <Section title="現在の契約">
        <Card variant="outlined" className="contract-current">
          <CardContent>
            <Stack spacing={1}>
              <Typography variant="h6">{projectName}</Typography>
              <Typography>{period} 単価:{unitPrice}</Typography>
              <Alert severity="warning">残り23日 ・ 更新未定</Alert>
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                <Typography color="text.secondary">担当営業:{salesName}</Typography>
                <Button startIcon={<MailOutlined />}>連絡する</Button>
              </Stack>
            </Stack>
          </CardContent>
        </Card>
      </Section>
      <Section title="過去の契約">
        <ContractList pastOnly />
      </Section>
    </Stack>
  )
}
