import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { renderWithProviders } from '../../../test/renderWithProviders'
import { users } from '../../../test/fixtures'
import type { ExpiringContract } from '../api/contractApi'
import { ExpiringContractCardList } from './ExpiringContractCardList'

const contract: ExpiringContract = {
  id: 100,
  assignmentId: 200,
  engineerId: 7,
  engineerName: '田中 太郎',
  projectId: 42,
  projectTitle: '金融系Webアプリ開発',
  clientName: 'A銀行',
  periodTo: '2026-07-10',
  daysRemaining: 12,
  renewalStatus: '更新未定',
}

describe('ExpiringContractCardList', () => {
  it('契約の要点とエンジニア詳細リンクを表示する', () => {
    renderWithProviders(<ExpiringContractCardList contracts={[contract]} />, { user: users.Sales })

    expect(screen.getByText('田中 太郎')).toBeInTheDocument()
    expect(screen.getByText('金融系Webアプリ開発(A銀行)')).toBeInTheDocument()
    expect(screen.getByText('残り12日')).toBeInTheDocument()
    expect(screen.getByText('更新未定')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'エンジニア詳細' })).toHaveAttribute('href', '/engineers/7')
  })

  it('0件のときは空状態メッセージを表示する', () => {
    renderWithProviders(<ExpiringContractCardList contracts={[]} />, { user: users.Sales })

    expect(screen.getByText('更新間近の契約はありません。')).toBeInTheDocument()
  })
})
