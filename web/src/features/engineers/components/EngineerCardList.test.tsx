import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { users } from '../../../test/fixtures'
import type { Engineer } from '../../../shared/types/domain'
import { EngineerCardList } from './EngineerCardList'

const engineer: Engineer = {
  id: 7,
  name: '田中 太郎',
  email: 'tanaka@example.com',
  status: '稼働中',
  project: '金融系Webアプリ開発',
  availableFrom: '2026-08-31',
  sales: '佐藤 営業',
  primarySalesId: 1,
  unitPrice: '70万',
  skills: [
    { name: 'Java', years: 5 },
    { name: 'AWS', years: 2 },
  ],
  categories: [],
  desiredTechnologies: [],
  desiredAreas: [],
  desiredRoles: [],
  avoid: [],
}

describe('EngineerCardList', () => {
  it('技術者の要点と連絡先リンクを表示する', () => {
    renderWithProviders(<EngineerCardList engineers={[engineer]} />, { user: users.Sales })

    expect(screen.getByText('田中 太郎')).toBeInTheDocument()
    expect(screen.getByText('稼働中')).toBeInTheDocument()
    expect(screen.getByText('金融系Webアプリ開発')).toBeInTheDocument()
    expect(screen.getByText('佐藤 営業')).toBeInTheDocument()
    expect(screen.getByText('Java 5年')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'メール' })).toHaveAttribute('href', 'mailto:tanaka@example.com')
    expect(screen.getByRole('link', { name: '詳細' })).toHaveAttribute('href', '/engineers/7')
  })

  it('0件のときは空状態メッセージを表示する', () => {
    renderWithProviders(<EngineerCardList engineers={[]} />, { user: users.Sales })

    expect(screen.getByText('条件に合う技術者がありません')).toBeInTheDocument()
  })
})
