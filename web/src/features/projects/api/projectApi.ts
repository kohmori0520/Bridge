import { apiRequest } from '../../../shared/api/http'
import type { Project, RequiredSkill } from '../../../shared/types/domain'

export type PageRequest = {
  page: number
  pageSize: number
}

export type PageResponse<T> = {
  items: T[]
  total: number
}

type ApiProjectSummary = {
  id: number
  title: string
  clientName: string
  startDate: string
  endDate: string
  unitPriceMin: number
  unitPriceMax: number
  status: string
  ownerSales: {
    name: string
  }
  assignedCount: number
}

type ApiRequiredSkill = {
  skillName: string
  requirement: string
  requiredYears: number
}

type ApiProjectDetail = ApiProjectSummary & {
  description: string
  requiredSkills: ApiRequiredSkill[]
}

type ApiProjectListResponse = {
  items: ApiProjectSummary[]
  pagination: {
    total: number
  }
}

type ApiEngineerMatchesResponse = {
  matches: {
    project: ApiProjectSummary & {
      ownerSalesName: string
    }
    score: number
  }[]
}

function formatDateRange(startDate: string, endDate?: string) {
  return endDate ? `${startDate} 〜 ${endDate}` : `${startDate}〜`
}

function formatPrice(min: number, max: number) {
  return `${Math.round(min / 10000)}万〜${Math.round(max / 10000)}万 / 月`
}

function toStatus(status: string): Project['status'] {
  return status.toLowerCase() === 'open' ? '募集中' : 'クローズ'
}

function toRequiredSkill(skill: ApiRequiredSkill): RequiredSkill {
  return {
    name: skill.skillName,
    years: skill.requiredYears,
    type: skill.requirement === 'required' ? '必須' : '歓迎',
  }
}

function toProject(summary: ApiProjectSummary, detail?: Partial<ApiProjectDetail>, matchScore = 0): Project {
  const requiredSkills = detail?.requiredSkills?.map(toRequiredSkill) ?? []

  return {
    id: summary.id,
    title: summary.title,
    client: summary.clientName,
    status: toStatus(summary.status),
    owner: summary.ownerSales?.name ?? '未設定',
    period: formatDateRange(summary.startDate, summary.endDate),
    startDate: summary.startDate,
    price: formatPrice(summary.unitPriceMin, summary.unitPriceMax),
    summary: detail?.description ?? '',
    assigned: `${summary.assignedCount ?? 0}名`,
    requiredSkills: requiredSkills.filter((skill) => skill.type === '必須'),
    welcomeSkills: requiredSkills.filter((skill) => skill.type === '歓迎'),
    matchScore,
  }
}

export const projectApi = {
  list: async ({ page, pageSize }: PageRequest): Promise<PageResponse<Project>> => {
    const response = await apiRequest<ApiProjectListResponse>(`/projects?page=${page + 1}&limit=${pageSize}`)
    return {
      items: response.items.map((project) => toProject(project)),
      total: response.pagination.total,
    }
  },
  get: async (id: number): Promise<Project | undefined> => {
    const response = await apiRequest<ApiProjectDetail>(`/projects/${id}`)
    return toProject(response, response)
  },
  listMyMatches: async (): Promise<Project[]> => {
    const response = await apiRequest<ApiEngineerMatchesResponse>('/engineers/me/matches?page=1&limit=20')
    return response.matches.map((match) =>
      toProject(
        {
          ...match.project,
          endDate: '',
          status: 'open',
          ownerSales: { name: match.project.ownerSalesName },
          assignedCount: 0,
        },
        undefined,
        match.score,
      ),
    )
  },
}
