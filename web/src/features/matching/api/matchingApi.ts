import { matchCandidates } from '../../../shared/mocks/mockData'
import type { MatchCandidate } from '../../../shared/types/domain'

export const matchingApi = {
  listProjectMatches: async (): Promise<MatchCandidate[]> => matchCandidates,
}
