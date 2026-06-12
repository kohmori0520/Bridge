import { Visibility } from '@mui/icons-material'
import { Button, Card, CardActions, CardContent, Chip, Stack, Typography } from '@mui/material'
import { Link as RouterLink } from 'react-router-dom'
import { EmptyState } from '../../../shared/components/EmptyState'
import type { ExpiringContract } from '../api/contractApi'

type ExpiringContractCardListProps = {
  contracts: ExpiringContract[]
}

/** モバイル向けの更新間近契約カード一覧(外出先での確認を想定) */
export function ExpiringContractCardList({ contracts }: ExpiringContractCardListProps) {
  if (contracts.length === 0) {
    return <EmptyState message="更新間近の契約はありません。" />
  }

  return (
    <Stack spacing={1.5}>
      {contracts.map((contract) => (
        <Card key={contract.id} variant="outlined">
          <CardContent sx={{ pb: 0.5 }}>
            <Stack direction="row" spacing={1} sx={{ alignItems: 'center', justifyContent: 'space-between' }}>
              <Typography variant="h6">{contract.engineerName}</Typography>
              <Chip
                size="small"
                color={contract.renewalStatus === '更新未定' ? 'warning' : 'success'}
                label={contract.renewalStatus}
              />
            </Stack>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {contract.projectTitle}({contract.clientName})
            </Typography>
            <Stack direction="row" spacing={1.5} sx={{ mt: 1, alignItems: 'center' }}>
              <Chip
                size="small"
                color={contract.daysRemaining <= 14 ? 'error' : 'warning'}
                label={`残り${contract.daysRemaining}日`}
              />
              <Typography variant="body2" color="text.secondary">
                終了日 {contract.periodTo}
              </Typography>
            </Stack>
          </CardContent>
          <CardActions>
            <Button
              size="small"
              startIcon={<Visibility />}
              component={RouterLink}
              to={`/engineers/${contract.engineerId}`}
            >
              エンジニア詳細
            </Button>
          </CardActions>
        </Card>
      ))}
    </Stack>
  )
}
