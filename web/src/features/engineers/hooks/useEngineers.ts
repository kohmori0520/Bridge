import { useQuery } from '@tanstack/react-query'
import { engineerApi } from '../api/engineerApi'

export function useEngineers(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['engineers', page, pageSize],
    queryFn: () => engineerApi.list({ page, pageSize }),
  })
}

export function useEngineer(engineerId: number) {
  return useQuery({
    queryKey: ['engineers', engineerId],
    queryFn: () => engineerApi.get(engineerId),
  })
}
