import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { api, server } from '../../../test/server'
import { engineerApi } from './engineerApi'

const apiEngineerSummary = {
  id: 1,
  name: '田中 太郎',
  primarySales: { id: 10, name: '佐藤 営業' },
  isAvailable: false,
  currentContract: {
    projectTitle: '金融系Webアプリ開発',
    periodTo: '2025-12-31',
    unitPrice: 850000,
  },
  skills: [{ skillId: 1, skillName: 'React', category: 'Framework', years: 5 }],
}

const apiEngineerDetail = {
  ...apiEngineerSummary,
  bio: 'フロントエンドが得意です',
  avoidedWorkNote: '運用保守',
  preferredSkills: [
    { skillId: 1, skillName: 'React', category: 'Framework' },
    { skillId: 2, skillName: 'AWS', category: 'Infrastructure' },
    { skillId: 3, skillName: '金融', category: 'Domain' },
    { skillId: 4, skillName: 'テックリード', category: 'Role' },
  ],
  preferredCategories: ['Web開発'],
}

describe('engineerApi.list', () => {
  it('稼働状況・単価・営業担当を表示用に変換する', async () => {
    server.use(
      http.get(api('/engineers'), () =>
        HttpResponse.json({ items: [apiEngineerSummary], pagination: { total: 1 } }),
      ),
    )

    const result = await engineerApi.list({ page: 0, pageSize: 10 })

    expect(result.items[0]).toMatchObject({
      id: 1,
      name: '田中 太郎',
      status: '稼働中',
      project: '金融系Webアプリ開発',
      availableFrom: '2025-12-31',
      sales: '佐藤 営業',
      primarySalesId: 10,
      unitPrice: '85万',
    })
    expect(result.items[0].skills).toEqual([{ skillId: 1, name: 'React', years: 5, category: 'Framework' }])
  })

  it('空き状態・営業未設定・契約なしを正しく変換する', async () => {
    server.use(
      http.get(api('/engineers'), () =>
        HttpResponse.json({
          items: [{ ...apiEngineerSummary, primarySales: null, isAvailable: true, currentContract: null }],
          pagination: { total: 1 },
        }),
      ),
    )

    const result = await engineerApi.list({ page: 0, pageSize: 10 })

    expect(result.items[0]).toMatchObject({
      status: '空き',
      project: '-',
      availableFrom: '-',
      sales: '未設定',
      primarySalesId: null,
      unitPrice: '-',
    })
  })

  it('primarySalesId フィルターをクエリに含める', async () => {
    let searchParams: URLSearchParams | null = null
    server.use(
      http.get(api('/engineers'), ({ request }) => {
        searchParams = new URL(request.url).searchParams
        return HttpResponse.json({ items: [], pagination: { total: 0 } })
      }),
    )

    await engineerApi.list({ page: 0, pageSize: 10, primarySalesId: 'me' })

    expect(searchParams!.get('primarySalesId')).toBe('me')
  })
})

describe('engineerApi.get', () => {
  it('希望スキルをカテゴリ別 (技術・領域・役割) に振り分ける', async () => {
    server.use(http.get(api('/engineers/1'), () => HttpResponse.json(apiEngineerDetail)))

    const engineer = await engineerApi.get(1)

    expect(engineer?.desiredTechnologies).toEqual(['React', 'AWS'])
    expect(engineer?.desiredAreas).toEqual(['金融'])
    expect(engineer?.desiredRoles).toEqual(['テックリード'])
    expect(engineer?.avoid).toEqual(['運用保守'])
    expect(engineer?.categories).toEqual(['Web開発'])
  })
})

describe('engineerApi.getProfile', () => {
  it('skillId のないスキルは編集用プロフィールから除外する', async () => {
    server.use(
      http.get(api('/engineers/1'), () =>
        HttpResponse.json({
          ...apiEngineerDetail,
          skills: [
            { skillId: 1, skillName: 'React', category: 'Framework', years: 5 },
            { skillName: '独自スキル', years: 2 },
          ],
        }),
      ),
    )

    const profile = await engineerApi.getProfile(1)

    expect(profile.skills).toEqual([{ skillId: 1, skillName: 'React', category: 'Framework', years: 5 }])
    expect(profile.preferredSkillIds).toEqual([1, 2, 3, 4])
  })
})

describe('engineerApi.saveMyProfile', () => {
  it('プロフィール・スキル・希望条件を順に保存し、最新プロフィールを返す', async () => {
    const calls: { method: string; path: string; body: unknown }[] = []
    const record = async (request: Request) => {
      calls.push({
        method: request.method,
        path: new URL(request.url).pathname,
        body: await request.clone().json().catch(() => null),
      })
    }

    server.use(
      http.patch(api('/engineers/me'), async ({ request }) => {
        await record(request)
        return HttpResponse.json(apiEngineerDetail)
      }),
      http.put(api('/engineers/me/skills'), async ({ request }) => {
        await record(request)
        return new HttpResponse(null, { status: 204 })
      }),
      http.put(api('/engineers/me/preferences'), async ({ request }) => {
        await record(request)
        return new HttpResponse(null, { status: 204 })
      }),
      http.get(api('/engineers/me'), () => HttpResponse.json(apiEngineerDetail)),
    )

    const profile = await engineerApi.saveMyProfile({
      name: '田中 太郎',
      bio: '更新後の自己紹介',
      avoidedWorkNote: '夜間対応',
      skills: [{ skillId: 1, years: 6 }],
      preferredSkillIds: [1, 2],
      preferredCategories: ['Web開発'],
    })

    expect(calls.map((call) => `${call.method} ${call.path}`)).toEqual([
      'PATCH /engineers/me',
      'PUT /engineers/me/skills',
      'PUT /engineers/me/preferences',
    ])
    expect(calls[0].body).toEqual({ name: '田中 太郎', bio: '更新後の自己紹介', avoidedWorkNote: '夜間対応' })
    expect(calls[1].body).toEqual({ skills: [{ skillId: 1, years: 6 }] })
    expect(calls[2].body).toEqual({
      preferredSkillIds: [1, 2],
      preferredCategories: ['Web開発'],
      avoidedWorkNote: '夜間対応',
    })
    expect(profile.id).toBe(1)
  })
})

describe('engineerApi.assignSales', () => {
  it('担当営業を PUT で更新する', async () => {
    let requestBody: unknown = null
    server.use(
      http.put(api('/engineers/1/sales'), async ({ request }) => {
        requestBody = await request.json()
        return new HttpResponse(null, { status: 204 })
      }),
    )

    await engineerApi.assignSales(1, 10)

    expect(requestBody).toEqual({ primarySalesId: 10 })
  })
})
