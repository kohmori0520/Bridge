import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { users } from '../../../test/fixtures'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { api, server } from '../../../test/server'
import type { ImportResult } from '../api/importApi'
import { ImportPage } from './ImportPage'

const validPreview: ImportResult = {
  dryRun: true,
  valid: true,
  sheets: [
    { sheet: '技術者', newCount: 2, updateCount: 1, errorCount: 0 },
    { sheet: '案件', newCount: 1, updateCount: 0, errorCount: 0 },
    { sheet: '契約', newCount: 3, updateCount: 0, errorCount: 0 },
  ],
  errors: [],
  createdAccounts: [],
}

const invalidPreview: ImportResult = {
  ...validPreview,
  valid: false,
  sheets: [
    { sheet: '技術者', newCount: 0, updateCount: 0, errorCount: 1 },
    { sheet: '案件', newCount: 1, updateCount: 0, errorCount: 0 },
    { sheet: '契約', newCount: 0, updateCount: 0, errorCount: 0 },
  ],
  errors: [{ sheet: '技術者', row: 5, message: 'スキル『React.js』はスキルマスタに存在しません' }],
}

const commitResult: ImportResult = {
  ...validPreview,
  dryRun: false,
  createdAccounts: [{ email: 'tanaka@example.com', name: '田中太郎', initialPassword: 'abcDEF234kmnp' }],
}

function registerImportHandler(previewResult: ImportResult, commitResponse: ImportResult = commitResult) {
  server.use(
    http.post(api('/imports/excel'), ({ request }) => {
      const dryRun = new URL(request.url).searchParams.get('dryRun')
      return HttpResponse.json(dryRun === 'true' ? previewResult : commitResponse)
    }),
  )
}

function selectFile(container: HTMLElement) {
  const input = container.querySelector<HTMLInputElement>('input[type="file"]')
  expect(input).not.toBeNull()
  const file = new File(['dummy'], 'import.xlsx', {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  })
  return userEvent.upload(input!, file)
}

describe('ImportPage', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('ファイル選択でプレビューを実行し、エラーなしなら確定ボタンを表示する', async () => {
    registerImportHandler(validPreview)
    const { container } = renderWithProviders(<ImportPage />, { user: users.Sales })

    await selectFile(container)

    expect(await screen.findByText('エラーはありません。取り込みを確定できます。')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: '取り込みを確定' })).toBeEnabled()
    expect(screen.getByText('技術者')).toBeInTheDocument()
  })

  it('検証エラーがある場合はエラー一覧を表示し、確定ボタンを出さない', async () => {
    registerImportHandler(invalidPreview)
    const { container } = renderWithProviders(<ImportPage />, { user: users.Sales })

    await selectFile(container)

    expect(await screen.findByText('スキル『React.js』はスキルマスタに存在しません')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: '取り込みを確定' })).not.toBeInTheDocument()
  })

  it('確定後に作成されたアカウントの初期パスワードを一度だけ表示する', async () => {
    registerImportHandler(validPreview)
    const { container } = renderWithProviders(<ImportPage />, { user: users.Sales })

    await selectFile(container)
    await userEvent.click(await screen.findByRole('button', { name: '取り込みを確定' }))

    expect(await screen.findByText('新規アカウントの初期パスワード')).toBeInTheDocument()
    expect(screen.getByText('tanaka@example.com')).toBeInTheDocument()
    expect(screen.getByText('abcDEF234kmnp')).toBeInTheDocument()
  })

  it('テンプレートダウンロードボタンで xlsx を取得する', async () => {
    server.use(
      http.get(api('/imports/template'), () =>
        HttpResponse.arrayBuffer(new ArrayBuffer(8), {
          headers: { 'Content-Type': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' },
        }),
      ),
    )
    const createObjectURL = vi.fn(() => 'blob:mock')
    const revokeObjectURL = vi.fn()
    Object.assign(URL, { createObjectURL, revokeObjectURL })
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {})

    renderWithProviders(<ImportPage />, { user: users.Sales })
    await userEvent.click(screen.getByRole('button', { name: 'テンプレートをダウンロード' }))

    await waitFor(() => expect(click).toHaveBeenCalled())
    expect(createObjectURL).toHaveBeenCalled()
  })
})
