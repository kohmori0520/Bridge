import { useQuery } from '@tanstack/react-query'
import { projectApi } from '../api/projectApi'

export function useProjects(page: number, pageSize: number, status?: 'open' | 'draft' | 'closed') {
  return useQuery({
    queryKey: ['projects', page, pageSize, status],
    queryFn: () => projectApi.list({ page, pageSize, status }),
  })
}

export function useProject(projectId: number, enabled = true) {
  return useQuery({
    queryKey: ['projects', projectId],
    queryFn: () => projectApi.get(projectId),
    enabled,
  })
}

export function useMyProjectMatches() {
  return useQuery({
    queryKey: ['me', 'matches'],
    queryFn: () => projectApi.listMyMatches(),
  })
}

export function useEngineerProjectList(page: number, pageSize: number) {
  return useQuery({
    queryKey: ['projects', 'engineer-matches', page, pageSize],
    queryFn: () => projectApi.listMyMatches({ page, pageSize }),
  })
}

export function useSkills() {
  return useQuery({
    queryKey: ['skills'],
    queryFn: projectApi.listSkills,
  })
}
