import { useQuery } from '@tanstack/react-query'
import { engineerApi } from '../api/engineerApi'

type UseEngineersOptions = {
  primarySalesId?: 'me' | number
}

export function useEngineers(page: number, pageSize: number, options: UseEngineersOptions = {}) {
  const { primarySalesId } = options
  return useQuery({
    queryKey: ['engineers', page, pageSize, primarySalesId ?? null],
    queryFn: () => engineerApi.list({ page, pageSize, primarySalesId }),
  })
}

export function useEngineer(engineerId: number, options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: ['engineers', engineerId],
    queryFn: () => engineerApi.get(engineerId),
    enabled: options.enabled ?? true,
  })
}

export function useMyEngineer() {
  return useQuery({
    queryKey: ['engineers', 'me'],
    queryFn: () => engineerApi.getMe(),
  })
}

export function useEngineerProfile(engineerId: number, enabled = true) {
  return useQuery({
    queryKey: ['engineers', engineerId, 'profile'],
    queryFn: () => engineerApi.getProfile(engineerId),
    enabled,
  })
}
