import { apiRequest } from '../../../shared/api/http'
import type { Engineer, MatchCandidate, PreferenceMatchType } from '../../../shared/types/domain'

type ApiProjectMatchesResponse = {
  matches: {
    engineer: {
      id: number
      name: string
      primarySalesName: string
    }
    score: number
    scoreBreakdown: {
      skillMatch: number
      yearsAdequacy: number
      categoryMatch: number
    }
    skillEvaluations: {
      skillName: string
      requirement: string
      requiredYears: number
      actualYears: number
      status: 'matched' | 'insufficient' | 'unmet'
    }[]
    preferenceMatches: {
      skillId?: number
      skillName: string
      matchType: PreferenceMatchType
    }[]
  }[]
}

function toEngineer(engineer: ApiProjectMatchesResponse['matches'][number]['engineer']): Engineer {
  return {
    id: engineer.id,
    name: engineer.name,
    email: '',
    status: '空き',
    project: '-',
    availableFrom: '-',
    sales: engineer.primarySalesName || '未設定',
    primarySalesId: null,
    unitPrice: '-',
    skills: [],
    categories: [],
    desiredTechnologies: [],
    desiredAreas: [],
    desiredRoles: [],
    avoid: [],
  }
}

export const matchingApi = {
  listProjectMatches: async (projectId: number): Promise<MatchCandidate[]> => {
    const response = await apiRequest<ApiProjectMatchesResponse>(`/projects/${projectId}/matches?page=1&limit=20`)
    return response.matches.map((match, index) => ({
      rank: index + 1,
      engineer: toEngineer(match.engineer),
      score: match.score,
      breakdown: {
        skill: match.scoreBreakdown.skillMatch,
        years: match.scoreBreakdown.yearsAdequacy,
        preference: match.scoreBreakdown.categoryMatch,
      },
      evaluations: match.skillEvaluations.map((evaluation) => ({
        skill: evaluation.skillName,
        type: evaluation.requirement === 'required' ? '必須' : '歓迎',
        requiredYears: evaluation.requiredYears,
        actualYears: evaluation.actualYears === 0 ? null : evaluation.actualYears,
        status: evaluation.status,
      })),
      preferenceMatches: match.preferenceMatches.map((preference) => ({
        skillId: preference.skillId,
        skillName: preference.skillName,
        matchType: preference.matchType,
      })),
    }))
  },
}
