import { useQuery } from '@tanstack/react-query'
import { matchingApi } from '../api/matchingApi'

export function useProjectMatches(projectId: number) {
  return useQuery({
    queryKey: ['projects', projectId, 'matches'],
    queryFn: () => matchingApi.listProjectMatches(projectId),
  })
}
