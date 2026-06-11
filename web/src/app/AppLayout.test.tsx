import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import { users } from '../test/fixtures'
import { createAuthValue, renderWithProviders } from '../test/renderWithProviders'
import { AppLayout } from './AppLayout'

function renderLayout(options: Parameters<typeof renderWithProviders>[1] = {}) {
  return renderWithProviders(
    <Routes>
      <Route path="/login" element={<div>ログイン画面</div>} />
      <Route element={<AppLayout />}>
        <Route path="/" element={<div>ホームコンテンツ</div>} />
      </Route>
    </Routes>,
    { route: '/', ...options },
  )
}

describe('AppLayout', () => {
  it('未ログイン時は /login にリダイレクトする', () => {
    renderLayout({ user: null })

    expect(screen.getByText('ログイン画面')).toBeInTheDocument()
  })

  it('ログイン中はユーザー名とロールを表示する', () => {
    renderLayout({ user: users.Engineer })

    expect(screen.getByText('田中 太郎 さん')).toBeInTheDocument()
    expect(screen.getByText('Engineer')).toBeInTheDocument()
    expect(screen.getByText('ホームコンテンツ')).toBeInTheDocument()
  })

  it('エンジニアにはエンジニア向けのタブを表示する', () => {
    renderLayout({ user: users.Engineer })

    for (const label of ['ホーム', 'プロフィール', '契約履歴', '公開案件', 'マッチ']) {
      expect(screen.getByRole('tab', { name: label })).toBeInTheDocument()
    }
    expect(screen.queryByRole('tab', { name: 'ユーザー作成' })).not.toBeInTheDocument()
  })

  it('営業には営業向けのタブを表示する', () => {
    renderLayout({ user: users.Sales })

    for (const label of ['ホーム', '案件', '技術者']) {
      expect(screen.getByRole('tab', { name: label })).toBeInTheDocument()
    }
    expect(screen.queryByRole('tab', { name: 'マッチ' })).not.toBeInTheDocument()
  })

  it('管理者には管理者向けのタブを表示する', () => {
    renderLayout({ user: users.Admin })

    for (const label of ['ホーム', 'ユーザー作成', '更新間近契約', '技術者']) {
      expect(screen.getByRole('tab', { name: label })).toBeInTheDocument()
    }
  })

  it('ログアウトすると logout を呼び /login へ遷移する', async () => {
    const logout = vi.fn(async () => {})
    const auth = createAuthValue(users.Engineer, { logout })
    const user = userEvent.setup()
    renderLayout({ auth })

    await user.click(screen.getByRole('button', { name: 'ログアウト' }))

    expect(logout).toHaveBeenCalledTimes(1)
    expect(await screen.findByText('ログイン画面')).toBeInTheDocument()
  })
})
