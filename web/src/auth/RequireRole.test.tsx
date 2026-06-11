import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { users } from '../test/fixtures'
import { renderWithProviders } from '../test/renderWithProviders'
import { RequireRole } from './RequireRole'

function renderProtected(options: Parameters<typeof renderWithProviders>[1]) {
  return renderWithProviders(
    <Routes>
      <Route path="/login" element={<div>ログイン画面</div>} />
      <Route
        path="/protected"
        element={
          <RequireRole allowed={['Sales']}>
            <div>営業専用コンテンツ</div>
          </RequireRole>
        }
      />
    </Routes>,
    { route: '/protected', ...options },
  )
}

describe('RequireRole', () => {
  it('未ログイン時は /login にリダイレクトする', () => {
    renderProtected({ user: null })

    expect(screen.getByText('ログイン画面')).toBeInTheDocument()
    expect(screen.queryByText('営業専用コンテンツ')).not.toBeInTheDocument()
  })

  it('許可されたロールはコンテンツを表示する', () => {
    renderProtected({ user: users.Sales })

    expect(screen.getByText('営業専用コンテンツ')).toBeInTheDocument()
  })

  it('許可されていないロールには権限エラーを表示する', () => {
    renderProtected({ user: users.Engineer })

    expect(screen.getByText('権限エラー')).toBeInTheDocument()
    expect(screen.getByText('現在のロール: Engineer')).toBeInTheDocument()
    expect(screen.queryByText('営業専用コンテンツ')).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'ホームへ戻る' })).toHaveAttribute('href', '/')
  })
})
