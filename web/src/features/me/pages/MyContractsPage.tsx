import { MailOutlined } from '@mui/icons-material'
import { Alert, Button, Card, CardContent, Stack, Typography } from '@mui/material'
import { ContractList } from '../../contracts/components/ContractList'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'

export function MyContractsPage() {
  return (
    <Stack spacing={2}>
      <PageHeader title="契約履歴" backTo="/" />
      <Section title="現在の契約">
        <Card variant="outlined" className="contract-current">
          <CardContent>
            <Stack spacing={1}>
              <Typography variant="h6">金融系Webアプリ開発(A銀行)</Typography>
              <Typography>2025-03-01 〜 2025-05-31 単価:85万</Typography>
              <Alert severity="warning">残り23日 ・ 更新未定</Alert>
              <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
                <Typography color="text.secondary">担当営業:佐藤 営業</Typography>
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
