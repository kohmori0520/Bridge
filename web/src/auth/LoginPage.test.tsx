import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { api, server } from '../test/server'
import { AuthProvider } from './AuthProvider'
import { LoginPage } from './LoginPage'

function renderLoginPage() {
  return render(
    <AuthProvider>
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<div>ホーム画面</div>} />
        </Routes>
      </MemoryRouter>
    </AuthProvider>,
  )
}

function mockLoginSuccess() {
  server.use(
    http.post(api('/auth/login'), () =>
      HttpResponse.json({
        token: 'jwt-token',
        user: { id: 1, email: 'tanaka@bridge.local', role: 'Engineer', salesId: null, engineerId: 5, name: '田中 太郎' },
      }),
    ),
  )
}

describe('LoginPage', () => {
  it('初期表示ではエンジニアの開発用ユーザーが入力されている', () => {
    renderLoginPage()

    expect(screen.getByLabelText('メールアドレス')).toHaveValue('tanaka@bridge.local')
    expect(screen.getByLabelText('パスワード')).toHaveValue('Engineer1234!')
  })

  it('ログイン種別を切り替えると対応する開発用ユーザーが入力される', async () => {
    const user = userEvent.setup()
    renderLoginPage()

    await user.click(screen.getByRole('combobox', { name: 'ログイン種別' }))
    await user.click(within(screen.getByRole('listbox')).getByText('営業'))

    expect(screen.getByLabelText('メールアドレス')).toHaveValue('sato@bridge.local')
    expect(screen.getByLabelText('パスワード')).toHaveValue('Sales1234!')
  })

  it('パスワードの表示/非表示を切り替えられる', async () => {
    const user = userEvent.setup()
    renderLoginPage()

    expect(screen.getByLabelText('パスワード')).toHaveAttribute('type', 'password')

    await user.click(screen.getByRole('button', { name: 'パスワードを表示' }))
    expect(screen.getByLabelText('パスワード')).toHaveAttribute('type', 'text')

    await user.click(screen.getByRole('button', { name: 'パスワードを隠す' }))
    expect(screen.getByLabelText('パスワード')).toHaveAttribute('type', 'password')
  })

  it('Enter キーでログインを送信できる', async () => {
    mockLoginSuccess()
    const user = userEvent.setup()
    renderLoginPage()

    await user.click(screen.getByLabelText('パスワード'))
    await user.keyboard('{Enter}')

    expect(await screen.findByText('ホーム画面')).toBeInTheDocument()
  })

  it('ログイン成功時はセッションを保存しホームへ遷移する', async () => {
    mockLoginSuccess()
    const user = userEvent.setup()
    renderLoginPage()

    await user.click(screen.getByRole('button', { name: 'ログイン' }))

    expect(await screen.findByText('ホーム画面')).toBeInTheDocument()
    const session = JSON.parse(window.sessionStorage.getItem('bridge.auth.session') ?? 'null')
    expect(session).toEqual({
      token: 'jwt-token',
      user: { role: 'Engineer', name: '田中 太郎', email: 'tanaka@bridge.local' },
    })
  })

  it('ログイン失敗時は API のエラーメッセージを表示して画面に留まる', async () => {
    server.use(
      http.post(api('/auth/login'), () =>
        HttpResponse.json(
          { error: { code: 'UNAUTHORIZED', message: 'メールアドレスまたはパスワードが違います' } },
          { status: 401 },
        ),
      ),
    )
    const user = userEvent.setup()
    renderLoginPage()

    await user.click(screen.getByRole('button', { name: 'ログイン' }))

    expect(await screen.findByText('メールアドレスまたはパスワードが違います')).toBeInTheDocument()
    expect(screen.queryByText('ホーム画面')).not.toBeInTheDocument()
    expect(window.sessionStorage.getItem('bridge.auth.session')).toBeNull()
  })
})
