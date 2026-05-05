import { apiRequest } from '../../../shared/api/http'
import type { Contract } from '../../../shared/types/domain'

type ApiEngineerAssignmentsResponse = {
  assignments: {
    projectTitle: string
    clientName: string
    contracts: {
      periodFrom: string
      periodTo: string
      unitPrice: number
      contractType: string
      isCurrent: boolean
    }[]
  }[]
}

function toContract(
  assignment: ApiEngineerAssignmentsResponse['assignments'][number],
  contract: ApiEngineerAssignmentsResponse['assignments'][number]['contracts'][number],
): Contract {
  return {
    title: `${assignment.projectTitle}(${assignment.clientName})`,
    period: `${contract.periodFrom} 〜 ${contract.periodTo}`,
    unitPrice: `${Math.round(contract.unitPrice / 10000)}万`,
    type: contract.contractType === 'initial' ? '初回' : '更新',
    current: contract.isCurrent,
  }
}

export const contractApi = {
  listMine: async (): Promise<Contract[]> => {
    const response = await apiRequest<ApiEngineerAssignmentsResponse>('/engineers/me/assignments')
    return response.assignments.flatMap((assignment) =>
      assignment.contracts.map((contract) => toContract(assignment, contract)),
    )
  },
}
