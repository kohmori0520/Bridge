import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { api, server } from '../../../test/server'
import { projectApi } from './projectApi'

const apiProject = {
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
}

describe('projectApi.list', () => {
  it('0始まりのページを API の1始まりに変換して送信する', async () => {
    let searchParams: URLSearchParams | null = null
    server.use(
      http.get(api('/projects'), ({ request }) => {
        searchParams = new URL(request.url).searchParams
        return HttpResponse.json({ items: [], pagination: { total: 0 } })
      }),
    )

    await projectApi.list({ page: 2, pageSize: 10, status: 'open' })

    expect(searchParams!.get('page')).toBe('3')
    expect(searchParams!.get('limit')).toBe('10')
    expect(searchParams!.get('status')).toBe('open')
  })

  it('status 未指定時は status パラメータを送信しない', async () => {
    let searchParams: URLSearchParams | null = null
    server.use(
      http.get(api('/projects'), ({ request }) => {
        searchParams = new URL(request.url).searchParams
        return HttpResponse.json({ items: [], pagination: { total: 0 } })
      }),
    )

    await projectApi.list({ page: 0, pageSize: 10 })

    expect(searchParams!.has('status')).toBe(false)
  })

  it('API レスポンスを表示用の Project に変換する', async () => {
    server.use(
      http.get(api('/projects'), () => HttpResponse.json({ items: [apiProject], pagination: { total: 25 } })),
    )

    const result = await projectApi.list({ page: 0, pageSize: 10 })

    expect(result.total).toBe(25)
    expect(result.items).toHaveLength(1)
    expect(result.items[0]).toMatchObject({
      id: 1,
      title: '金融系Webアプリ開発',
      client: 'A銀行',
      status: '募集中',
      owner: '佐藤 営業',
      period: '2025-06-01 〜 2025-12-31',
      price: '70万〜90万 / 月',
      assigned: '1名',
    })
  })

  it.each([
    ['open', '募集中'],
    ['draft', '下書き'],
    ['closed', 'クローズ'],
    ['cancelled', 'クローズ'],
  ])('ステータス %s を「%s」に変換する', async (apiStatus, expected) => {
    server.use(
      http.get(api('/projects'), () =>
        HttpResponse.json({ items: [{ ...apiProject, status: apiStatus }], pagination: { total: 1 } }),
      ),
    )

    const result = await projectApi.list({ page: 0, pageSize: 10 })

    expect(result.items[0].status).toBe(expected)
  })

  it('担当営業が未設定の場合は「未設定」と表示する', async () => {
    server.use(
      http.get(api('/projects'), () =>
        HttpResponse.json({ items: [{ ...apiProject, ownerSales: null }], pagination: { total: 1 } }),
      ),
    )

    const result = await projectApi.list({ page: 0, pageSize: 10 })

    expect(result.items[0].owner).toBe('未設定')
  })
})

describe('projectApi.get', () => {
  it('詳細レスポンスから必須・歓迎スキルを振り分ける', async () => {
    server.use(
      http.get(api('/projects/1'), () =>
        HttpResponse.json({
          ...apiProject,
          description: '投信管理システム刷新',
          requiredSkills: [
            { skillId: 1, skillName: 'React', requirement: 'required', requiredYears: 3 },
            { skillId: 2, skillName: 'TypeScript', requirement: 'required', requiredYears: 2 },
            { skillId: 3, skillName: 'AWS', requirement: 'preferred', requiredYears: 1 },
          ],
        }),
      ),
    )

    const project = await projectApi.get(1)

    expect(project?.summary).toBe('投信管理システム刷新')
    expect(project?.requiredSkills).toEqual([
      { skillId: 1, name: 'React', years: 3, type: '必須' },
      { skillId: 2, name: 'TypeScript', years: 2, type: '必須' },
    ])
    expect(project?.welcomeSkills).toEqual([{ skillId: 3, name: 'AWS', years: 1, type: '歓迎' }])
  })
})

describe('projectApi.update', () => {
  it('status 未指定時は draft を補完して PATCH する', async () => {
    let requestBody: Record<string, unknown> | null = null
    server.use(
      http.patch(api('/projects/1'), async ({ request }) => {
        requestBody = (await request.json()) as Record<string, unknown>
        return HttpResponse.json({ ...apiProject, description: '', requiredSkills: [] })
      }),
    )

    await projectApi.update(1, {
      title: 'タイトル',
      clientName: 'A銀行',
      description: '',
      startDate: '2025-06-01',
      endDate: '2025-12-31',
      unitPriceMin: 700000,
      unitPriceMax: 900000,
      requiredSkills: [],
    })

    expect(requestBody!.status).toBe('draft')
  })
})

describe('projectApi.listMyMatches', () => {
  it('マッチ一覧を matchScore 付きの Project に変換する', async () => {
    server.use(
      http.get(api('/engineers/me/matches'), () =>
        HttpResponse.json({
          matches: [
            {
              project: { ...apiProject, ownerSalesName: '山田 営業' },
              score: 85,
            },
          ],
          pagination: { total: 1 },
        }),
      ),
    )

    const result = await projectApi.listMyMatches()

    expect(result.items[0]).toMatchObject({
      id: 1,
      matchScore: 85,
      owner: '山田 営業',
      status: '募集中',
    })
  })
})
