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

/** 自分の担当エンジニアの更新間近契約 (Sales ロールのときのみ enabled にする) */
export function useMyExpiringContracts(days = 30, enabled = true) {
  return useQuery({
    queryKey: ['sales', 'expiring-contracts', days],
    queryFn: () => contractApi.listMyExpiring(days),
    enabled,
  })
}
