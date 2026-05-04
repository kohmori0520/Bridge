import { projects } from '../../../shared/mocks/mockData'
import type { Project } from '../../../shared/types/domain'

export type PageRequest = {
  page: number
  pageSize: number
}

export type PageResponse<T> = {
  items: T[]
  total: number
}

function paginate<T>(items: T[], { page, pageSize }: PageRequest): PageResponse<T> {
  const start = page * pageSize
  return {
    items: items.slice(start, start + pageSize),
    total: items.length,
  }
}

export const projectApi = {
  list: async (request: PageRequest): Promise<PageResponse<Project>> => paginate(projects, request),
  get: async (id: number): Promise<Project | undefined> => projects.find((project) => project.id === id),
  listMyMatches: async (): Promise<Project[]> => projects.slice().sort((a, b) => b.matchScore - a.matchScore),
}
