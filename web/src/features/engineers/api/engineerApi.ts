import { apiRequest } from '../../../shared/api/http'
import type { Engineer } from '../../../shared/types/domain'
import type { PageRequest, PageResponse } from '../../projects/api/projectApi'

const technologyCategories = new Set(['Language', 'Framework', 'Infrastructure', 'Database', 'Tool'])
const areaCategories = new Set(['Domain', 'ProductType'])
const roleCategories = new Set(['Role'])

type ApiEngineerSummary = {
  id: number
  name: string
  primarySales?: {
    name: string
  } | null
  isAvailable: boolean
  skills: {
    skillId?: number
    skillName: string
    category?: string
    years: number
  }[]
}

type ApiEngineerDetail = ApiEngineerSummary & {
  bio: string
  avoidedWorkNote: string
  preferredSkills: {
    skillId: number
    skillName: string
    category: string
  }[]
  preferredCategories: string[]
  currentContract?: {
    projectTitle: string
    periodTo: string
    unitPrice: number
  } | null
}

export type EngineerProfile = {
  id: number
  name: string
  bio: string
  avoidedWorkNote: string
  skills: {
    skillId: number
    skillName: string
    category: string
    years: number
  }[]
  preferredSkillIds: number[]
  preferredCategories: string[]
}

export type SaveEngineerProfileInput = {
  name: string
  bio: string
  avoidedWorkNote: string
  skills: {
    skillId: number
    years: number
  }[]
  preferredSkillIds: number[]
  preferredCategories: string[]
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
  const preferredSkills = detail.preferredSkills ?? []
  return {
    id: response.id,
    name: response.name,
    status: response.isAvailable ? '空き' : '稼働中',
    project: detail.currentContract?.projectTitle ?? '-',
    availableFrom: detail.currentContract?.periodTo ?? '-',
    sales: response.primarySales?.name ?? '未設定',
    unitPrice: formatUnitPrice(detail.currentContract?.unitPrice),
    skills: response.skills.map((skill) => ({
      skillId: skill.skillId,
      name: skill.skillName,
      years: skill.years,
      category: skill.category,
    })),
    categories: detail.preferredCategories ?? [],
    desiredTechnologies: preferredSkills.filter((skill) => technologyCategories.has(skill.category)).map((skill) => skill.skillName),
    desiredAreas: preferredSkills.filter((skill) => areaCategories.has(skill.category)).map((skill) => skill.skillName),
    desiredRoles: preferredSkills.filter((skill) => roleCategories.has(skill.category)).map((skill) => skill.skillName),
    avoid: detail.avoidedWorkNote ? [detail.avoidedWorkNote] : [],
  }
}

function toEngineerProfile(response: ApiEngineerDetail): EngineerProfile {
  return {
    id: response.id,
    name: response.name,
    bio: response.bio ?? '',
    avoidedWorkNote: response.avoidedWorkNote ?? '',
    skills: response.skills
      .filter((skill) => typeof skill.skillId === 'number')
      .map((skill) => ({
        skillId: skill.skillId!,
        skillName: skill.skillName,
        category: skill.category ?? '',
        years: skill.years,
      })),
    preferredSkillIds: response.preferredSkills.map((skill) => skill.skillId),
    preferredCategories: response.preferredCategories,
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
  getProfile: async (id: number): Promise<EngineerProfile> => {
    const response = await apiRequest<ApiEngineerDetail>(`/engineers/${id}`)
    return toEngineerProfile(response)
  },
  getMyProfile: async (): Promise<EngineerProfile> => {
    const response = await apiRequest<ApiEngineerDetail>('/engineers/me')
    return toEngineerProfile(response)
  },
  saveProfile: async (id: number, input: SaveEngineerProfileInput): Promise<EngineerProfile> => {
    await apiRequest<ApiEngineerDetail>(`/engineers/${id}`, {
      method: 'PATCH',
      body: JSON.stringify({
        name: input.name,
        bio: input.bio,
        avoidedWorkNote: input.avoidedWorkNote,
      }),
    })
    await apiRequest(`/engineers/${id}/skills`, {
      method: 'PUT',
      body: JSON.stringify({ skills: input.skills }),
    })
    await apiRequest(`/engineers/${id}/preferences`, {
      method: 'PUT',
      body: JSON.stringify({
        preferredSkillIds: input.preferredSkillIds,
        preferredCategories: input.preferredCategories,
        avoidedWorkNote: input.avoidedWorkNote,
      }),
    })

    return engineerApi.getProfile(id)
  },
  saveMyProfile: async (input: SaveEngineerProfileInput): Promise<EngineerProfile> => {
    await apiRequest<ApiEngineerDetail>('/engineers/me', {
      method: 'PATCH',
      body: JSON.stringify({
        name: input.name,
        bio: input.bio,
        avoidedWorkNote: input.avoidedWorkNote,
      }),
    })
    await apiRequest('/engineers/me/skills', {
      method: 'PUT',
      body: JSON.stringify({ skills: input.skills }),
    })
    await apiRequest('/engineers/me/preferences', {
      method: 'PUT',
      body: JSON.stringify({
        preferredSkillIds: input.preferredSkillIds,
        preferredCategories: input.preferredCategories,
        avoidedWorkNote: input.avoidedWorkNote,
      }),
    })

    return engineerApi.getMyProfile()
  },
}
