import { useQuery } from '@tanstack/react-query'
import { projectApi } from '../api/projectApi'

export function useProjects(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['projects', page, pageSize],
    queryFn: () => projectApi.list({ page, pageSize }),
  })
}

export function useProject(projectId: number) {
  return useQuery({
    queryKey: ['projects', projectId],
    queryFn: () => projectApi.get(projectId),
  })
}

export function useMyProjectMatches() {
  return useQuery({
    queryKey: ['me', 'matches'],
    queryFn: projectApi.listMyMatches,
  })
}
