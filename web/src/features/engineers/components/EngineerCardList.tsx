import { Email, Visibility } from '@mui/icons-material'
import { Box, Button, Card, CardActions, CardContent, Chip, Stack, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { EmptyState } from '../../../shared/components/EmptyState'
import type { Engineer } from '../../../shared/types/domain'

const MAX_SKILL_CHIPS = 4

type EngineerCardListProps = {
  engineers: Engineer[]
}

/** モバイル向けの技術者カード一覧(連絡先・稼働状況を一目で確認する用途) */
export function EngineerCardList({ engineers }: EngineerCardListProps) {
  if (engineers.length === 0) {
    return <EmptyState message="条件に合う技術者がありません" />
  }

  return (
    <Stack spacing={1.5}>
      {engineers.map((engineer) => (
        <Card key={engineer.id} variant="outlined">
          <CardContent sx={{ pb: 0.5 }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography variant="h6">{engineer.name}</Typography>
              <Chip
                size="small"
                color={engineer.status === '空き' ? 'success' : 'primary'}
                label={engineer.status}
              />
            </Stack>
            <Stack spacing={0.5} sx={{ mt: 1 }}>
              <InfoRow label="現在の案件" value={engineer.project} />
              <InfoRow label="空き予定" value={engineer.availableFrom} />
              <InfoRow label="担当営業" value={engineer.sales} />
              <InfoRow label="単価" value={engineer.unitPrice} />
            </Stack>
            {engineer.skills.length > 0 && (
              <Box sx={{ mt: 1, display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {engineer.skills.slice(0, MAX_SKILL_CHIPS).map((skill) => (
                  <Chip key={skill.name} size="small" variant="outlined" label={`${skill.name} ${skill.years}年`} />
                ))}
                {engineer.skills.length > MAX_SKILL_CHIPS && (
                  <Chip size="small" variant="outlined" label={`+${engineer.skills.length - MAX_SKILL_CHIPS}`} />
                )}
              </Box>
            )}
          </CardContent>
          <CardActions>
            {engineer.email && (
              <Button size="small" startIcon={<Email />} href={`mailto:${engineer.email}`}>
                メール
              </Button>
            )}
            <Button size="small" startIcon={<Visibility />} component={RouterLink} to={`/engineers/${engineer.id}`}>
              詳細
            </Button>
          </CardActions>
        </Card>
      ))}
    </Stack>
  )
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <Stack direction="row" spacing={1}>
      <Typography variant="body2" color="text.secondary" sx={{ width: 88, flexShrink: 0 }}>
        {label}
      </Typography>
      <Typography variant="body2">{value}</Typography>
    </Stack>
  )
}
