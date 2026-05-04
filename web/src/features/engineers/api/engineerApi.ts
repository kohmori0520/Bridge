import { engineers } from '../../../shared/mocks/mockData'
import type { Engineer } from '../../../shared/types/domain'
import type { PageRequest, PageResponse } from '../../projects/api/projectApi'

function paginate<T>(items: T[], { page, pageSize }: PageRequest): PageResponse<T> {
  const start = page * pageSize
  return {
    items: items.slice(start, start + pageSize),
    total: items.length,
  }
}

export const engineerApi = {
  list: async (request: PageRequest): Promise<PageResponse<Engineer>> => paginate(engineers, request),
  get: async (id: number): Promise<Engineer | undefined> => engineers.find((engineer) => engineer.id === id),
}
