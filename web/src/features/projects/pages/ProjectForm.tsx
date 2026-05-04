import { Add, Save } from '@mui/icons-material'
import { Box, Button, Chip, Stack, TextField } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { useProject } from '../hooks/useProjects'

export function ProjectForm({ mode }: { mode: 'new' | 'edit' }) {
  const { data: sampleProject } = useProject(1)
  const editableSkills = sampleProject
    ? sampleProject.requiredSkills.concat(sampleProject.welcomeSkills)
    : []

  return (
    <Stack spacing={2}>
      <PageHeader title={mode === 'new' ? '案件作成' : '案件編集'} subtitle="API接続前のため保存操作は画面確認用です" backTo="/projects" />
      <Section title="基本情報">
        <Box className="form-grid" key={sampleProject?.id ?? mode}>
          <TextField label="タイトル" defaultValue={mode === 'edit' ? sampleProject?.title ?? '' : ''} />
          <TextField label="クライアント" defaultValue={mode === 'edit' ? sampleProject?.client ?? '' : ''} />
          <TextField label="開始日" type="date" defaultValue="2025-06-01" slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="終了日" type="date" defaultValue="2025-12-31" slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="単価下限(万円)" defaultValue="70" />
          <TextField label="単価上限(万円)" defaultValue="90" />
          <TextField label="概要" multiline minRows={4} className="wide-field" defaultValue={sampleProject?.summary ?? ''} />
        </Box>
      </Section>
      <Section title="要求スキル">
        <Stack spacing={1}>
          {editableSkills.map((skill) => (
            <Box className="skill-edit-row" key={skill.name}>
              <TextField label="スキル" defaultValue={skill.name} />
              <TextField label="必要年数" defaultValue={skill.years} />
              <Chip color={skill.type === '必須' ? 'primary' : 'default'} label={skill.type} />
            </Box>
          ))}
          <Box>
            <Button startIcon={<Add />}>スキルを追加</Button>
          </Box>
        </Stack>
      </Section>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button component={RouterLink} to="/projects">キャンセル</Button>
        <Button variant="contained" startIcon={<Save />}>保存</Button>
      </Stack>
    </Stack>
  )
}
