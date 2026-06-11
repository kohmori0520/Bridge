import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { users } from '../../../test/fixtures'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { api, server } from '../../../test/server'
import { ProjectsPage } from './ProjectsPage'

const apiProjects = [
  {
    id: 1,
    title: '金融系Webアプリ開発',
    clientName: 'A銀行',
    startDate: '2025-06-01',
    endDate: '2025-12-31',
    unitPriceMin: 700000,
    unitPriceMax: 900000,
    status: 'open',
    ownerSales: { name: '佐藤 営業' },
    assignedCount: 1,
  },
  {
    id: 2,
    title: 'SaaSフロントエンド開発',
    clientName: 'B社',
    startDate: '2025-06-01',
    endDate: '2026-03-31',
    unitPriceMin: 800000,
    unitPriceMax: 1000000,
    status: 'draft',
    ownerSales: { name: '佐藤 営業' },
    assignedCount: 0,
  },
]

function mockProjectsApi() {
  server.use(
    http.get(api('/projects'), () => HttpResponse.json({ items: apiProjects, pagination: { total: 2 } })),
    http.get(api('/engineers/me/matches'), () =>
      HttpResponse.json({
        matches: [
          { project: { ...apiProjects[0], ownerSalesName: '佐藤 営業' }, score: 85 },
        ],
        pagination: { total: 1 },
      }),
    ),
  )
}

describe('ProjectsPage (営業)', () => {
  it('案件一覧と件数サマリーを表示する', async () => {
    mockProjectsApi()
    renderWithProviders(<ProjectsPage />, { user: users.Sales })

    expect(screen.getByText('案件一覧')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: '案件作成' })).toHaveAttribute('href', '/projects/new')

    expect(await screen.findByText('金融系Webアプリ開発')).toBeInTheDocument()
    expect(screen.getByText('SaaSフロントエンド開発')).toBeInTheDocument()
    expect(screen.getByText('2 / 2 件')).toBeInTheDocument()
  })

  it('キーワードで案件を絞り込める', async () => {
    mockProjectsApi()
    const user = userEvent.setup()
    renderWithProviders(<ProjectsPage />, { user: users.Sales })

    await screen.findByText('金融系Webアプリ開発')
    await user.type(screen.getByPlaceholderText('案件名・クライアント・スキル'), 'SaaS')

    await waitFor(() => {
      expect(screen.queryByText('金融系Webアプリ開発')).not.toBeInTheDocument()
    })
    expect(screen.getByText('SaaSフロントエンド開発')).toBeInTheDocument()
    expect(screen.getByText('1 / 2 件')).toBeInTheDocument()
  })

  it('API エラー時はエラーメッセージを表示する', async () => {
    server.use(
      http.get(api('/projects'), () =>
        HttpResponse.json({ error: { code: 'INTERNAL', message: 'サーバーエラーが発生しました' } }, { status: 500 }),
      ),
      http.get(api('/engineers/me/matches'), () =>
        HttpResponse.json({ matches: [], pagination: { total: 0 } }),
      ),
    )
    renderWithProviders(<ProjectsPage />, { user: users.Sales })

    expect(await screen.findByText('サーバーエラーが発生しました')).toBeInTheDocument()
  })
})

describe('ProjectsPage (エンジニア)', () => {
  it('公開案件一覧をマッチ度付きで表示し、案件作成ボタンは表示しない', async () => {
    mockProjectsApi()
    renderWithProviders(<ProjectsPage />, { user: users.Engineer })

    expect(screen.getByText('公開案件一覧')).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: '案件作成' })).not.toBeInTheDocument()

    expect(await screen.findByText('金融系Webアプリ開発')).toBeInTheDocument()
    expect(screen.getByText('85点')).toBeInTheDocument()
  })
})
