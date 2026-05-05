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

type ApiExpiringContractsResponse = {
  items: {
    contractId: number
    assignmentId: number
    engineer: {
      id: number
      name: string
    }
    project: {
      id: number
      title: string
      clientName: string
    }
    periodTo: string
    daysRemaining: number
    renewalStatus: string
  }[]
}

export type ExpiringContract = {
  id: number
  assignmentId: number
  engineerId: number
  engineerName: string
  projectId: number
  projectTitle: string
  clientName: string
  periodTo: string
  daysRemaining: number
  renewalStatus: '更新予定あり' | '更新未定' | '対象外'
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
  listExpiring: async (days = 30): Promise<ExpiringContract[]> => {
    const response = await apiRequest<ApiExpiringContractsResponse>(`/contracts/expiring?days=${days}`)
    return response.items.map((item) => ({
      id: item.contractId,
      assignmentId: item.assignmentId,
      engineerId: item.engineer.id,
      engineerName: item.engineer.name,
      projectId: item.project.id,
      projectTitle: item.project.title,
      clientName: item.project.clientName,
      periodTo: item.periodTo,
      daysRemaining: item.daysRemaining,
      renewalStatus: item.renewalStatus === 'scheduled' ? '更新予定あり' : item.renewalStatus === 'not_planned' ? '更新未定' : '対象外',
    }))
  },
}
