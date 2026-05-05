import { Add, DeleteOutlined, Save } from '@mui/icons-material'
import { Alert, Autocomplete, Box, Button, Chip, IconButton, ListSubheader, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { Link as RouterLink, useNavigate, useParams } from 'react-router-dom'
import { PageHeader } from '../../../shared/components/PageHeader'
import { Section } from '../../../shared/components/Section'
import { ApiErrorAlert } from '../../../shared/components/ApiErrorAlert'
import { LoadingState } from '../../../shared/components/LoadingState'
import { getSkillCategoryLabel, groupBySkillCategory, sortSkillCategories } from '../../../shared/utils/skillCategories'
import type { SkillOption } from '../../projects/api/projectApi'
import { useSkills } from '../../projects/hooks/useProjects'
import { engineerApi, type SaveEngineerProfileInput } from '../api/engineerApi'
import { useEngineerProfile } from '../hooks/useEngineers'

type SkillRow = {
  skillId: string
  years: string
}

type EngineerFormState = {
  name: string
  bio: string
  avoidedWorkNote: string
  preferredSkillIds: number[]
  preferredCategories: string[]
}

const emptyForm: EngineerFormState = {
  name: '',
  bio: '',
  avoidedWorkNote: '',
  preferredSkillIds: [],
  preferredCategories: [],
}

const preferredSkillCategories = new Set(['Language', 'Framework', 'Infrastructure', 'Database', 'Tool'])
const preferredAreaCategories = new Set(['Domain', 'ProductType'])
const preferredRoleCategories = new Set(['Role'])

export function EngineerForm({ mode }: { mode: 'new' | 'edit' }) {
  const { id } = useParams()
  const engineerId = Number(id)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { data: skills = [] } = useSkills()
  const { data: profile, isLoading } = useEngineerProfile(engineerId, mode === 'edit' && Number.isFinite(engineerId))
  const [form, setForm] = useState<EngineerFormState>(emptyForm)
  const [skillRows, setSkillRows] = useState<SkillRow[]>([])
  const [error, setError] = useState<string | null>(null)
  const groupedSkills = useMemo(() => groupBySkillCategory(skills), [skills])
  const skillGroups = useMemo(() => sortSkillCategories(Object.keys(groupedSkills)), [groupedSkills])
  const preferredSkillChoices = useMemo(() => skills.filter((skill) => preferredSkillCategories.has(skill.category)), [skills])
  const preferredAreaChoices = useMemo(() => skills.filter((skill) => preferredAreaCategories.has(skill.category)), [skills])
  const preferredRoleChoices = useMemo(() => skills.filter((skill) => preferredRoleCategories.has(skill.category)), [skills])
  const selectedSkillIds = useMemo(() => skillRows.map((row) => row.skillId).filter(Boolean), [skillRows])
  const preferredTechnologyOptions = useMemo(
    () => preferredSkillChoices.filter((skill) => form.preferredSkillIds.includes(skill.id)),
    [form.preferredSkillIds, preferredSkillChoices],
  )
  const preferredAreaOptions = useMemo(
    () => preferredAreaChoices.filter((skill) => form.preferredSkillIds.includes(skill.id)),
    [form.preferredSkillIds, preferredAreaChoices],
  )
  const preferredRoleOptions = useMemo(
    () => preferredRoleChoices.filter((skill) => form.preferredSkillIds.includes(skill.id)),
    [form.preferredSkillIds, preferredRoleChoices],
  )

  useEffect(() => {
    if (!profile) return

    queueMicrotask(() => {
      setForm({
        name: profile.name,
        bio: profile.bio,
        avoidedWorkNote: profile.avoidedWorkNote,
        preferredSkillIds: profile.preferredSkillIds,
        preferredCategories: profile.preferredCategories,
      })
      setSkillRows(profile.skills.map((skill) => ({ skillId: String(skill.skillId), years: String(skill.years) })))
    })
  }, [profile])

  const mutation = useMutation({
    mutationFn: (input: SaveEngineerProfileInput) => engineerApi.saveProfile(engineerId, input),
    onSuccess: async (savedProfile) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['engineers'] }),
        queryClient.invalidateQueries({ queryKey: ['engineers', engineerId] }),
        queryClient.invalidateQueries({ queryKey: ['engineers', engineerId, 'profile'] }),
      ])
      navigate(`/engineers/${savedProfile.id}`)
    },
  })

  const updateForm = <K extends keyof EngineerFormState>(field: K, value: EngineerFormState[K]) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const updatePreferredSkillGroup = (targetGroup: Set<string>, values: SkillOption[]) => {
    setForm((current) => {
      const preservedSkillIds = current.preferredSkillIds.filter((skillId) => {
        const skill = skills.find((option) => option.id === skillId)
        return skill ? !targetGroup.has(skill.category) : false
      })
      const preferredSkillIds = [...preservedSkillIds, ...values.map((skill) => skill.id)]
      const preferredCategories = sortSkillCategories(
        Array.from(new Set(skills.filter((skill) => preferredSkillIds.includes(skill.id)).map((skill) => skill.category))),
      )

      return { ...current, preferredSkillIds, preferredCategories }
    })
  }

  const updateSkill = (index: number, patch: Partial<SkillRow>) => {
    setSkillRows((current) => current.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  const addSkill = () => {
    const selectedIds = new Set(selectedSkillIds)
    const firstSkill = skills.find((skill) => !selectedIds.has(String(skill.id))) ?? skills[0]
    if (!firstSkill) return
    setSkillRows((current) => [...current, { skillId: String(firstSkill.id), years: '1' }])
  }

  const removeSkill = (index: number) => {
    setSkillRows((current) => current.filter((_, i) => i !== index))
  }

  const handleSave = () => {
    setError(null)

    if (mode === 'new') {
      setError('技術者の新規作成はユーザー作成フローから行ってください。')
      return
    }

    if (!form.name.trim()) {
      setError('氏名を入力してください。')
      return
    }

    const duplicateSkillIds = selectedSkillIds.filter((skillId, index) => selectedSkillIds.indexOf(skillId) !== index)
    if (duplicateSkillIds.length > 0) {
      setError('同じ保有スキルが複数選択されています。重複を削除してください。')
      return
    }

    const structuredSkills = skillRows
      .map((row) => ({
        skillId: Number(row.skillId),
        years: Math.max(0, Number(row.years || 0)),
      }))
      .filter((row) => Number.isFinite(row.skillId) && row.skillId > 0)

    if (structuredSkills.length === 0) {
      setError('保有スキルを1つ以上選択してください。')
      return
    }

    mutation.mutate({
      name: form.name,
      bio: form.bio,
      avoidedWorkNote: form.avoidedWorkNote,
      skills: structuredSkills,
      preferredSkillIds: form.preferredSkillIds,
      preferredCategories: sortSkillCategories(
        Array.from(new Set(skills.filter((skill) => form.preferredSkillIds.includes(skill.id)).map((skill) => skill.category))),
      ),
    })
  }

  if (isLoading) {
    return <LoadingState message="技術者情報を読み込み中です。" />
  }

  return (
    <Stack spacing={2}>
      <PageHeader title={mode === 'new' ? '技術者作成' : '技術者編集'} subtitle="保有スキルと希望条件を構造化して登録します" backTo="/engineers" />
      {mode === 'new' && <Alert severity="info">技術者の新規作成は、管理者のユーザー作成フローで Engineer ユーザーを作成してから編集してください。</Alert>}
      {error && <Alert severity="error">{error}</Alert>}
      {mutation.error && <ApiErrorAlert error={mutation.error} fallbackMessage="保存に失敗しました" />}
      <Section title="基本情報">
        <Box className="form-grid">
          <TextField label="氏名" value={form.name} disabled={mode === 'new'} onChange={(event) => updateForm('name', event.target.value)} />
          <TextField
            label="自己紹介"
            className="wide-field"
            multiline
            minRows={3}
            value={form.bio}
            disabled={mode === 'new'}
            helperText="文章は営業向けの補足として扱い、マッチング条件には保有スキル・希望条件を使います。"
            onChange={(event) => updateForm('bio', event.target.value)}
          />
        </Box>
      </Section>
      <Section title="保有スキル" action={<Chip size="small" color="primary" variant="outlined" label={`${skillRows.length}件`} />}>
        <Stack spacing={1.5}>
          <Typography variant="body2" color="text.secondary">
            本人プロフィールと同じく、スキル名と経験年数を分けて登録します。
          </Typography>
          {skillRows.map((row, index) => (
            <Box className="skill-edit-row" key={`${row.skillId}-${index}`}>
              <TextField
                select
                label="スキル"
                value={row.skillId}
                disabled={mode === 'new'}
                helperText={skills.find((skill) => String(skill.id) === row.skillId)?.category ? getSkillCategoryLabel(skills.find((skill) => String(skill.id) === row.skillId)!.category) : ' '}
                onChange={(event) => updateSkill(index, { skillId: event.target.value })}
              >
                {skillGroups.flatMap((category) => [
                  <ListSubheader key={`${category}-header`} disableSticky>
                    {getSkillCategoryLabel(category)}
                  </ListSubheader>,
                  ...groupedSkills[category].map((skill) => {
                    const selectedInAnotherRow = selectedSkillIds.includes(String(skill.id)) && row.skillId !== String(skill.id)
                    return (
                      <MenuItem key={skill.id} value={String(skill.id)} disabled={selectedInAnotherRow}>
                        {skill.name}
                      </MenuItem>
                    )
                  }),
                ])}
              </TextField>
              <TextField label="経験年数" value={row.years} disabled={mode === 'new'} helperText="年" onChange={(event) => updateSkill(index, { years: event.target.value })} />
              <Box />
              <IconButton aria-label="スキルを削除" color="inherit" disabled={mode === 'new' || skillRows.length === 1} onClick={() => removeSkill(index)}>
                <DeleteOutlined />
              </IconButton>
            </Box>
          ))}
          <Box>
            <Button startIcon={<Add />} onClick={addSkill} disabled={mode === 'new' || (skills.length > 0 && selectedSkillIds.length >= skills.length)}>
              スキルを追加
            </Button>
          </Box>
        </Stack>
      </Section>
      <Section title="希望条件">
        <Stack spacing={2}>
          <Autocomplete
            multiple
            options={preferredSkillChoices}
            value={preferredTechnologyOptions}
            disabled={mode === 'new'}
            groupBy={(option) => getSkillCategoryLabel(option.category)}
            getOptionLabel={(option: SkillOption) => option.name}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_, value) => updatePreferredSkillGroup(preferredSkillCategories, value)}
            renderInput={(params) => <TextField {...params} label="やりたい技術" helperText="言語・フレームワーク・DB・インフラ・ツールから選びます。" />}
          />
          <Autocomplete
            multiple
            options={preferredAreaChoices}
            value={preferredAreaOptions}
            disabled={mode === 'new'}
            groupBy={(option) => getSkillCategoryLabel(option.category)}
            getOptionLabel={(option: SkillOption) => option.name}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_, value) => updatePreferredSkillGroup(preferredAreaCategories, value)}
            renderInput={(params) => <TextField {...params} label="関わりたい領域" helperText="金融・医療・Web開発・データ分析など、具体的な領域を選びます。" />}
          />
          <Autocomplete
            multiple
            options={preferredRoleChoices}
            value={preferredRoleOptions}
            disabled={mode === 'new'}
            groupBy={(option) => getSkillCategoryLabel(option.category)}
            getOptionLabel={(option: SkillOption) => option.name}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            onChange={(_, value) => updatePreferredSkillGroup(preferredRoleCategories, value)}
            renderInput={(params) => <TextField {...params} label="希望役割" helperText="PM・テックリード・要件定義など、担当したい役割や工程を選びます。" />}
          />
          <TextField
            label="避けたい業務メモ"
            multiline
            minRows={3}
            value={form.avoidedWorkNote}
            disabled={mode === 'new'}
            helperText="ここだけは自由入力です。営業が案件提案時に確認する注意事項として扱います。"
            onChange={(event) => updateForm('avoidedWorkNote', event.target.value)}
          />
        </Stack>
      </Section>
      <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
        <Button component={RouterLink} to="/engineers">キャンセル</Button>
        <Button variant="contained" startIcon={<Save />} disabled={mode === 'new' || mutation.isPending} onClick={handleSave}>
          {mutation.isPending ? '保存中...' : '保存'}
        </Button>
      </Stack>
    </Stack>
  )
}
