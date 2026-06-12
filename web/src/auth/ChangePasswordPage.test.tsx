import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { HttpResponse, http } from 'msw'
import { describe, expect, it } from 'vitest'
import { users } from '../test/fixtures'
import { renderWithProviders } from '../test/renderWithProviders'
import { api, server } from '../test/server'
import { ChangePasswordPage } from './ChangePasswordPage'

async function fillForm(current: string, next: string, confirm: string) {
  await userEvent.type(screen.getByLabelText('現在のパスワード'), current)
  await userEvent.type(screen.getByLabelText('新しいパスワード'), next)
  await userEvent.type(screen.getByLabelText('新しいパスワード(確認)'), confirm)
  await userEvent.click(screen.getByRole('button', { name: '変更する' }))
}

describe('ChangePasswordPage', () => {
  it('変更に成功すると完了通知を表示する', async () => {
    server.use(
      http.put(api('/auth/me/password'), () => new HttpResponse(null, { status: 204 })),
    )
    renderWithProviders(<ChangePasswordPage />, { user: users.Engineer })

    await fillForm('OldPassword1!', 'NewPassword456!', 'NewPassword456!')

    expect(await screen.findByText('パスワードを変更しました')).toBeInTheDocument()
  })

  it('確認パスワードが一致しない場合は送信せずエラーを表示する', async () => {
    renderWithProviders(<ChangePasswordPage />, { user: users.Engineer })

    await fillForm('OldPassword1!', 'NewPassword456!', 'Different789!')

    expect(screen.getByText('新しいパスワード(確認)が一致しません。')).toBeInTheDocument()
  })

  it('現在のパスワードが誤っている場合は API のエラーメッセージを表示する', async () => {
    server.use(
      http.put(api('/auth/me/password'), () =>
        HttpResponse.json(
          { error: { code: 'INVALID_CURRENT_PASSWORD', message: '現在のパスワードが正しくありません' } },
          { status: 400 },
        ),
      ),
    )
    renderWithProviders(<ChangePasswordPage />, { user: users.Engineer })

    await fillForm('WrongPassword!', 'NewPassword456!', 'NewPassword456!')

    expect(await screen.findByText('現在のパスワードが正しくありません')).toBeInTheDocument()
  })
})
