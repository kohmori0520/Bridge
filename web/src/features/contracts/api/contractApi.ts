import { contracts } from '../../../shared/mocks/mockData'
import type { Contract } from '../../../shared/types/domain'

export const contractApi = {
  listMine: async (): Promise<Contract[]> => contracts,
}
