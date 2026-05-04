import { Save } from '@mui/icons-material'
import { Box, Button, Stack, TextField } from '@mui/material'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'

export function MyProfilePage() {
  return (
    <Stack spacing={2}>
      <PageHeader title="自分のプロフィール編集" subtitle="スキルと希望条件を営業に共有" />
      <Section title="基本情報">
        <Box className="form-grid">
          <TextField label="氏名" defaultValue="田中 太郎" />
          <TextField label="メールアドレス" defaultValue="tanaka@example.com" />
          <TextField label="保有スキル" className="wide-field" defaultValue="React 5年, TypeScript 4年, C# 3年, AWS 2年" />
          <TextField label="希望スキル" className="wide-field" defaultValue="React, TypeScript, AWS" />
          <TextField label="希望カテゴリ" className="wide-field" defaultValue="Web開発, フロントエンド" />
          <TextField label="避けたい業務" className="wide-field" defaultValue="パッケージ導入, 運用保守" />
        </Box>
      </Section>
      <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
        <Button variant="contained" startIcon={<Save />}>保存</Button>
      </Stack>
    </Stack>
  )
}
