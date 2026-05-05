import { useQuery } from '@tanstack/react-query'
import { contractApi } from '../api/contractApi'

export function useContracts() {
  return useQuery({
    queryKey: ['me', 'contracts'],
    queryFn: contractApi.listMine,
  })
}

export function useExpiringContracts(days = 30) {
  return useQuery({
    queryKey: ['contracts', 'expiring', days],
    queryFn: () => contractApi.listExpiring(days),
  })
}
