import { CheckCircle, ErrorOutlined, WarningAmber, Work } from '@mui/icons-material'
import { Box, Button, Chip, Paper, Stack, Typography } from '@mui/material'
import type { MatchCandidate } from '../../../shared/types/domain'

type MatchCandidateCardProps = {
  candidate: MatchCandidate
}

function EvaluationIcon({ status }: { status: 'matched' | 'insufficient' | 'unmet' }) {
  if (status === 'matched') {
    return <CheckCircle color="success" fontSize="small" />
  }
  if (status === 'insufficient') {
    return <WarningAmber color="warning" fontSize="small" />
  }
  return <ErrorOutlined color="error" fontSize="small" />
}

export function MatchCandidateCard({ candidate }: MatchCandidateCardProps) {
  return (
    <Paper variant="outlined" className="candidate-card">
      <Box className="candidate-header">
        <Box>
          <Typography variant="h6">
            #{candidate.rank} {candidate.engineer.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            担当営業:{candidate.engineer.sales}
          </Typography>
        </Box>
        <Chip color="primary" label={`スコア: ${candidate.score}`} />
      </Box>
      <Typography variant="body2">
        スコア内訳: スキル一致:{candidate.breakdown.skill} / 年数充足:{candidate.breakdown.years} /
        希望合致:{candidate.breakdown.preference}
      </Typography>
      <Stack spacing={0.5}>
        {candidate.evaluations.map((evaluation) => (
          <Box className="evaluation-row" key={evaluation.skill}>
            <EvaluationIcon status={evaluation.status} />
            <Typography sx={{ fontWeight: 700 }}>{evaluation.skill}</Typography>
            <Chip size="small" label={evaluation.type} />
            <Typography variant="body2" color="text.secondary">
              {evaluation.requiredYears}年以上
            </Typography>
            <Typography variant="body2">
              実績 {evaluation.actualYears === null ? 'なし' : `${evaluation.actualYears}年`}
            </Typography>
            {evaluation.status === 'insufficient' && <Chip size="small" color="warning" label="不足" />}
          </Box>
        ))}
      </Stack>
      <Box className="candidate-footer">
        <Chip size="small" color="success" label="希望カテゴリ一致: Web開発" />
        <Button variant="contained" endIcon={<Work />}>
          この案件にアサイン
        </Button>
      </Box>
    </Paper>
  )
}
