import { Add, DeleteOutlined, Save } from '@mui/icons-material'
import { Alert, Box, Button, Chip, IconButton, ListSubheader, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { projectApi, type ProjectRequiredSkillInput, type SaveProjectInput, type SkillOption } from '../api/projectApi'
import { useProject, useSkills } from '../hooks/useProjects'

type ProjectFormState = {
  title: string
  clientName: string
  startDate: string
  endDate: string
  unitPriceMin: string
  unitPriceMax: string
  description: string
  status: 'open' | 'closed'
}

type SkillFormState = {
  id: string
  requirement: ProjectRequiredSkillInput['requirement']
  years: string
}

const emptyProjectForm: ProjectFormState = {
  title: '',
  clientName: '',
  startDate: '2026-06-01',
  endDate: '2026-12-31',
  unitPriceMin: '70',
  unitPriceMax: '90',
  description: '',
  status: 'open',
}

function toYen(value: string) {
  return Math.max(0, Number(value || 0)) * 10000
}

const skillCategoryOrder = ['Language', 'Framework', 'Infrastructure', 'Database', 'Domain', 'Role', 'ProductType', 'Tool', 'Other']

const skillCategoryLabels: Record<string, string> = {
  Language: '言語',
  Framework: 'フレームワーク',
  Infrastructure: 'インフラ',
  Database: 'データベース',
  Domain: '業務ドメイン',
  Role: '役割',
  ProductType: 'プロダクト種別',
  Tool: 'ツール',
  Other: 'その他',
}

function getSkillCategoryLabel(category: string) {
  return skillCategoryLabels[category] ?? category
}

function groupSkillsByCategory(skills: SkillOption[]) {
  return skills.reduce<Record<string, SkillOption[]>>((groups, skill) => {
    groups[skill.category] = [...(groups[skill.category] ?? []), skill]
    return groups
  }, {})
}

export function ProjectForm({ mode }: { mode: 'new' | 'edit' }) {
  const { id } = useParams()
  const projectId = Number(id)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: project, isLoading: isProjectLoading } = useProject(projectId, mode === 'edit' && Number.isFinite(projectId))
  const { data: skills = [] } = useSkills()
  const [form, setForm] = useState<ProjectFormState>(emptyProjectForm)
  const [skillRows, setSkillRows] = useState<SkillFormState[]>([])
  const [error, setError] = useState<string | null>(null)
  const groupedSkills = useMemo(() => groupSkillsByCategory(skills), [skills])
  const skillGroups = useMemo(
    () =>
      Object.keys(groupedSkills).sort((a, b) => {
        const aIndex = skillCategoryOrder.indexOf(a)
        const bIndex = skillCategoryOrder.indexOf(b)
        return (aIndex === -1 ? Number.MAX_SAFE_INTEGER : aIndex) - (bIndex === -1 ? Number.MAX_SAFE_INTEGER : bIndex)
      }),
    [groupedSkills],
  )
  const selectedSkillIds = useMemo(() => skillRows.map((row) => row.id).filter(Boolean), [skillRows])
  const selectedSkills = useMemo(
    () =>
      skillRows
        .map((row) => ({
          row,
          skill: skills.find((option) => String(option.id) === row.id),
        }))
        .filter((item): item is { row: SkillFormState; skill: SkillOption } => Boolean(item.skill)),
    [skillRows, skills],
  )

  useEffect(() => {
    if (mode === 'new') return
    if (!project) return

    queueMicrotask(() => {
      setForm({
        title: project.title,
        clientName: project.client,
        startDate: project.startDate,
        endDate: project.period.split(' 〜 ')[1] ?? project.startDate,
        unitPriceMin: project.price.match(/^(\d+)万/)?.[1] ?? '70',
        unitPriceMax: project.price.match(/〜(\d+)万/)?.[1] ?? '90',
        description: project.summary,
        status: project.status === '募集中' ? 'open' : 'closed',
      })
      setSkillRows(
        project.requiredSkills.concat(project.welcomeSkills).map((skill) => ({
          id: String(skill.skillId ?? ''),
          requirement: skill.type === '必須' ? 'required' : 'preferred',
          years: String(skill.years),
        })),
      )
    })
  }, [mode, project])

  useEffect(() => {
    if (skillRows.length > 0) return
    const firstSkill = skills[0]
    if (!firstSkill) return
    queueMicrotask(() => {
      setSkillRows([{ id: String(firstSkill.id), requirement: 'required', years: '1' }])
    })
  }, [skillRows.length, skills])

  const mutation = useMutation({
    mutationFn: (input: SaveProjectInput) =>
      mode === 'new' ? projectApi.create(input) : projectApi.update(projectId, input),
    onSuccess: async (savedProject) => {
      await queryClient.invalidateQueries({ queryKey: ['projects'] })
      navigate(`/projects/${savedProject.id}`)
    },
  })

  const updateForm = (field: keyof ProjectFormState, value: string) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const updateSkill = (index: number, patch: Partial<SkillFormState>) => {
    setSkillRows((current) => current.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  const addSkill = () => {
    const selectedIds = new Set(selectedSkillIds)
    const firstSkill = skills.find((skill) => !selectedIds.has(String(skill.id))) ?? skills[0]
    if (!firstSkill) return
    setSkillRows((current) => [...current, { id: String(firstSkill.id), requirement: 'required', years: '1' }])
  }

  const removeSkill = (index: number) => {
    setSkillRows((current) => current.filter((_, i) => i !== index))
  }

  const handleSave = () => {
    setError(null)

    const duplicateSkillIds = selectedSkillIds.filter((skillId, index) => selectedSkillIds.indexOf(skillId) !== index)
    if (duplicateSkillIds.length > 0) {
      setError('同じスキルが複数選択されています。重複を削除してください。')
      return
    }

    const requiredSkills = skillRows
      .map((row) => ({
        skillId: Number(row.id),
        requirement: row.requirement,
        requiredYears: Math.max(0, Number(row.years || 0)),
      }))
      .filter((row) => Number.isFinite(row.skillId) && row.skillId > 0)

    if (!form.title.trim() || !form.clientName.trim()) {
      setError('タイトルとクライアントは必須です。')
      return
    }

    if (requiredSkills.length === 0) {
      setError('要求スキルを1つ以上選択してください。')
      return
    }

    mutation.mutate({
      title: form.title,
      clientName: form.clientName,
      description: form.description,
      startDate: form.startDate,
      endDate: form.endDate,
      unitPriceMin: toYen(form.unitPriceMin),
      unitPriceMax: toYen(form.unitPriceMax),
      requiredSkills,
      status: form.status,
    })
  }

  return (
    <Stack spacing={2}>
      <PageHeader title={mode === 'new' ? '案件作成' : '案件編集'} subtitle="案件情報と要求スキルを登録します" backTo="/projects" />
      {error && <Alert severity="error">{error}</Alert>}
      {mutation.error && <Alert severity="error">{mutation.error instanceof Error ? mutation.error.message : '保存に失敗しました'}</Alert>}
      <Section title="基本情報">
        <Box className="form-grid">
          <TextField label="タイトル" value={form.title} onChange={(event) => updateForm('title', event.target.value)} />
          <TextField label="クライアント" value={form.clientName} onChange={(event) => updateForm('clientName', event.target.value)} />
          <TextField label="開始日" type="date" value={form.startDate} onChange={(event) => updateForm('startDate', event.target.value)} slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="終了日" type="date" value={form.endDate} onChange={(event) => updateForm('endDate', event.target.value)} slotProps={{ inputLabel: { shrink: true } }} />
          <TextField label="単価下限(万円)" value={form.unitPriceMin} onChange={(event) => updateForm('unitPriceMin', event.target.value)} />
          <TextField label="単価上限(万円)" value={form.unitPriceMax} onChange={(event) => updateForm('unitPriceMax', event.target.value)} />
          {mode === 'edit' && (
            <TextField select label="ステータス" value={form.status} onChange={(event) => updateForm('status', event.target.value)}>
              <MenuItem value="open">募集中</MenuItem>
              <MenuItem value="closed">クローズ</MenuItem>
            </TextField>
          )}
          <TextField label="概要" multiline minRows={4} className="wide-field" value={form.description} onChange={(event) => updateForm('description', event.target.value)} />
        </Box>
      </Section>
      <Section
        title="要求スキル"
        action={<Chip size="small" color="primary" variant="outlined" label={`${selectedSkills.length}件選択中`} />}
      >
        <Stack spacing={1.5}>
          <Typography variant="body2" color="text.secondary">
            スキルはカテゴリ別に並んでいます。必須スキルと歓迎スキルを分けると、マッチング候補の理由が見やすくなります。
          </Typography>
          {skillRows.map((skill, index) => (
            <Box className="skill-edit-row" key={`${skill.id}-${index}`}>
              <TextField
                select
                label="スキル"
                value={skill.id}
                helperText={skills.find((option) => String(option.id) === skill.id)?.category ? getSkillCategoryLabel(skills.find((option) => String(option.id) === skill.id)!.category) : ' '}
                onChange={(event) => updateSkill(index, { id: event.target.value })}
              >
                {skillGroups.flatMap((category) => [
                  <ListSubheader key={`${category}-header`} disableSticky>
                    {getSkillCategoryLabel(category)}
                  </ListSubheader>,
                  ...groupedSkills[category].map((option) => {
                    const isSelectedInAnotherRow = selectedSkillIds.includes(String(option.id)) && skill.id !== String(option.id)
                    return (
                      <MenuItem key={option.id} value={String(option.id)} disabled={isSelectedInAnotherRow}>
                        {option.name}
                      </MenuItem>
                    )
                  }),
                ])}
              </TextField>
              <TextField label="必要年数" value={skill.years} helperText="年" onChange={(event) => updateSkill(index, { years: event.target.value })} />
              <TextField select label="種別" value={skill.requirement} onChange={(event) => updateSkill(index, { requirement: event.target.value as ProjectRequiredSkillInput['requirement'] })}>
                <MenuItem value="required">必須</MenuItem>
                <MenuItem value="preferred">歓迎</MenuItem>
              </TextField>
              <IconButton aria-label="スキルを削除" color="inherit" disabled={skillRows.length === 1} onClick={() => removeSkill(index)}>
                <DeleteOutlined />
              </IconButton>
            </Box>
          ))}
          {selectedSkills.length > 0 && (
            <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}>
              {selectedSkills.map(({ row, skill }) => (
                <Chip
                  key={`${skill.id}-${row.requirement}`}
                  label={`${skill.name} / ${row.requirement === 'required' ? '必須' : '歓迎'} / ${row.years || 0}年`}
                  color={row.requirement === 'required' ? 'primary' : 'default'}
                  variant={row.requirement === 'required' ? 'filled' : 'outlined'}
                />
              ))}
            </Stack>
          )}
          <Box>
            <Button startIcon={<Add />} onClick={addSkill} disabled={skills.length > 0 && selectedSkillIds.length >= skills.length}>
              スキルを追加
            </Button>
          </Box>
        </Stack>
      </Section>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button component={RouterLink} to="/projects">キャンセル</Button>
        <Button variant="contained" startIcon={<Save />} disabled={mutation.isPending || isProjectLoading} onClick={handleSave}>
          {mutation.isPending ? '保存中...' : '保存'}
        </Button>
      </Stack>
    </Stack>
  )
}
