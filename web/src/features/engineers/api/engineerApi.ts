import { apiRequest } from '../../../shared/api/http'
import type { Engineer } from '../../../shared/types/domain'
import type { PageRequest, PageResponse } from '../../projects/api/projectApi'

type ApiEngineerSummary = {
  id: number
  name: string
  primarySales?: {
    name: string
  } | null
  isAvailable: boolean
  skills: {
    skillName: string
    years: number
  }[]
}

type ApiEngineerDetail = ApiEngineerSummary & {
  avoidedWorkNote: string
  preferredCategories: string[]
  currentContract?: {
    projectTitle: string
    periodTo: string
    unitPrice: number
  } | null
}

type ApiEngineerListResponse = {
  items: ApiEngineerSummary[]
  pagination: {
    total: number
  }
}

function formatUnitPrice(unitPrice?: number) {
  return unitPrice ? `${Math.round(unitPrice / 10000)}万` : '-'
}

function toEngineer(response: ApiEngineerSummary | ApiEngineerDetail): Engineer {
  const detail = response as Partial<ApiEngineerDetail>
  return {
    id: response.id,
    name: response.name,
    status: response.isAvailable ? '空き' : '稼働中',
    project: detail.currentContract?.projectTitle ?? '-',
    availableFrom: detail.currentContract?.periodTo ?? '-',
    sales: response.primarySales?.name ?? '未設定',
    unitPrice: formatUnitPrice(detail.currentContract?.unitPrice),
    skills: response.skills.map((skill) => `${skill.skillName} ${skill.years}年`),
    categories: detail.preferredCategories ?? [],
    avoid: detail.avoidedWorkNote ? [detail.avoidedWorkNote] : [],
  }
}

export const engineerApi = {
  list: async ({ page, pageSize }: PageRequest): Promise<PageResponse<Engineer>> => {
    const response = await apiRequest<ApiEngineerListResponse>(`/engineers?page=${page + 1}&limit=${pageSize}`)
    return {
      items: response.items.map(toEngineer),
      total: response.pagination.total,
    }
  },
  get: async (id: number): Promise<Engineer | undefined> => {
    const response = await apiRequest<ApiEngineerDetail>(`/engineers/${id}`)
    return toEngineer(response)
  },
}
