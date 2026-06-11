import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { api, server } from '../../../test/server'
import { contractApi } from './contractApi'

describe('contractApi.listMine', () => {
  it('アサインごとの契約をフラットな契約一覧に変換する', async () => {
    server.use(
      http.get(api('/engineers/me/assignments'), () =>
        HttpResponse.json({
          assignments: [
            {
              projectTitle: '金融系Webアプリ開発',
              clientName: 'A銀行',
              contracts: [
                { periodFrom: '2025-03-01', periodTo: '2025-05-31', unitPrice: 850000, contractType: 'renewal', isCurrent: true },
                { periodFrom: '2024-12-01', periodTo: '2025-02-28', unitPrice: 850000, contractType: 'initial', isCurrent: false },
              ],
            },
            {
              projectTitle: 'パッケージ導入支援',
              clientName: 'Z社',
              contracts: [
                { periodFrom: '2024-03-01', periodTo: '2024-05-31', unitPrice: 700000, contractType: 'initial', isCurrent: false },
              ],
            },
          ],
        }),
      ),
    )

    const contracts = await contractApi.listMine()

    expect(contracts).toEqual([
      { title: '金融系Webアプリ開発(A銀行)', period: '2025-03-01 〜 2025-05-31', unitPrice: '85万', type: '更新', current: true },
      { title: '金融系Webアプリ開発(A銀行)', period: '2024-12-01 〜 2025-02-28', unitPrice: '85万', type: '初回', current: false },
      { title: 'パッケージ導入支援(Z社)', period: '2024-03-01 〜 2024-05-31', unitPrice: '70万', type: '初回', current: false },
    ])
  })
})

describe('contractApi.listExpiring', () => {
  const expiringItem = {
    contractId: 100,
    assignmentId: 200,
    engineer: { id: 1, name: '田中 太郎' },
    project: { id: 5, title: '金融系Webアプリ開発', clientName: 'A銀行' },
    periodTo: '2025-06-30',
    daysRemaining: 14,
    renewalStatus: 'scheduled',
  }

  it('days パラメータを送信し、表示用に変換する', async () => {
    let searchParams: URLSearchParams | null = null
    server.use(
      http.get(api('/contracts/expiring'), ({ request }) => {
        searchParams = new URL(request.url).searchParams
        return HttpResponse.json({ items: [expiringItem] })
      }),
    )

    const contracts = await contractApi.listExpiring(60)

    expect(searchParams!.get('days')).toBe('60')
    expect(contracts[0]).toEqual({
      id: 100,
      assignmentId: 200,
      engineerId: 1,
      engineerName: '田中 太郎',
      projectId: 5,
      projectTitle: '金融系Webアプリ開発',
      clientName: 'A銀行',
      periodTo: '2025-06-30',
      daysRemaining: 14,
      renewalStatus: '更新予定あり',
    })
  })

  it.each([
    ['scheduled', '更新予定あり'],
    ['not_planned', '更新未定'],
    ['unknown', '対象外'],
  ])('更新ステータス %s を「%s」に変換する', async (apiStatus, expected) => {
    server.use(
      http.get(api('/contracts/expiring'), () =>
        HttpResponse.json({ items: [{ ...expiringItem, renewalStatus: apiStatus }] }),
      ),
    )

    const contracts = await contractApi.listExpiring()

    expect(contracts[0].renewalStatus).toBe(expected)
  })

  it('days のデフォルトは 30 日', async () => {
    let searchParams: URLSearchParams | null = null
    server.use(
      http.get(api('/contracts/expiring'), ({ request }) => {
        searchParams = new URL(request.url).searchParams
        return HttpResponse.json({ items: [] })
      }),
    )

    await contractApi.listExpiring()

    expect(searchParams!.get('days')).toBe('30')
  })
})
