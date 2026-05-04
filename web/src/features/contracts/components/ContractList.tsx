import { Box, Chip, Divider, Stack, Typography } from '@mui/material'
import { useContracts } from '../hooks/useContracts'

type ContractListProps = {
  pastOnly?: boolean
}

export function ContractList({ pastOnly }: ContractListProps) {
  const { data: contracts = [] } = useContracts()
  const rows = pastOnly ? contracts.filter((contract) => !contract.current) : contracts

  return (
    <Stack divider={<Divider />} spacing={1}>
      {rows.map((contract) => (
        <Box className="contract-row" key={`${contract.title}-${contract.period}`}>
          <Box>
            <Typography sx={{ fontWeight: 700 }}>{contract.title}</Typography>
            <Typography variant="body2" color="text.secondary">
              {contract.period} 単価:{contract.unitPrice}
            </Typography>
          </Box>
          <Chip
            color={contract.current ? 'warning' : contract.type === '更新' ? 'primary' : 'default'}
            label={contract.current ? '現在' : contract.type}
          />
        </Box>
      ))}
    </Stack>
  )
}
