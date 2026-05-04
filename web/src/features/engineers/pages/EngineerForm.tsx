import { Save } from '@mui/icons-material'
import { Box, Button, Stack, TextField } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { useEngineer } from '../hooks/useEngineers'

export function EngineerForm({ mode }: { mode: 'new' | 'edit' }) {
  const { data: sampleEngineer } = useEngineer(1)

  return (
    <Stack spacing={2}>
      <PageHeader title={mode === 'new' ? '技術者作成' : '技術者編集'} subtitle="プロフィール、スキル、キャリア志向を登録" backTo="/engineers" />
      <Section title="基本情報">
        <Box className="form-grid" key={sampleEngineer?.id ?? mode}>
          <TextField label="氏名" defaultValue={mode === 'edit' ? sampleEngineer?.name ?? '' : ''} />
          <TextField label="メールアドレス" defaultValue="tanaka@example.com" />
          <TextField label="単価(万円)" defaultValue="85" />
          <TextField label="空き予定日" type="date" defaultValue="2025-06-01" slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="保有スキル" className="wide-field" defaultValue="React 5年, TypeScript 4年, C# 3年" />
          <TextField label="避けたい業務" className="wide-field" defaultValue="パッケージ導入, 運用保守" />
        </Box>
      </Section>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button component={RouterLink} to="/engineers">キャンセル</Button>
        <Button variant="contained" startIcon={<Save />}>保存</Button>
      </Stack>
    </Stack>
  )
}
