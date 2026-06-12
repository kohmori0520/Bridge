import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { users } from '../../../test/fixtures'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { ProjectDetailPage } from './ProjectDetailPage'

function renderProjectDetail(route: string) {
  return renderWithProviders(
    <Routes>
      <Route path="/projects/:id" element={<ProjectDetailPage />} />
    </Routes>,
    { user: users.Sales, route },
  )
}

describe('ProjectDetailPage', () => {
  it('不正な ID の場合は案件が見つからない旨を表示する', () => {
    renderProjectDetail('/projects/abc')

    expect(screen.getByText('案件が見つかりませんでした。')).toBeInTheDocument()
  })

  it('ID が 0 の場合も案件が見つからない旨を表示する', () => {
    renderProjectDetail('/projects/0')

    expect(screen.getByText('案件が見つかりませんでした。')).toBeInTheDocument()
  })
})
