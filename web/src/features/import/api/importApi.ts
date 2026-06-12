import { apiRequest, apiRequestBlob } from '../../../shared/api/http'

export type ImportSheetSummary = {
  sheet: string
  newCount: number
  updateCount: number
  errorCount: number
}

export type ImportRowError = {
  sheet: string
  row: number
  message: string
}

export type CreatedAccount = {
  email: string
  name: string
  initialPassword: string
}

export type ImportResult = {
  dryRun: boolean
  valid: boolean
  sheets: ImportSheetSummary[]
  errors: ImportRowError[]
  createdAccounts: CreatedAccount[]
}

export const importApi = {
  downloadTemplate: (): Promise<Blob> => {
    return apiRequestBlob('/imports/template')
  },
  uploadExcel: (file: File, dryRun: boolean): Promise<ImportResult> => {
    const form = new FormData()
    form.append('file', file)
    return apiRequest<ImportResult>(`/imports/excel?dryRun=${dryRun}`, {
      method: 'POST',
      body: form,
    })
  },
}
