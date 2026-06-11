import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { api, server } from '../../../test/server'
import { matchingApi } from './matchingApi'

const apiMatch = {
  engineer: { id: 1, name: '田中 太郎', primarySalesName: '佐藤 営業' },
  score: 85,
  scoreBreakdown: { skillMatch: 55, yearsAdequacy: 20, categoryMatch: 10 },
  skillEvaluations: [
    { skillName: 'React', requirement: 'required', requiredYears: 3, actualYears: 5, status: 'matched' as const },
    { skillName: '金融ドメイン経験', requirement: 'required', requiredYears: 2, actualYears: 0, status: 'unmet' as const },
    { skillName: 'AWS', requirement: 'preferred', requiredYears: 1, actualYears: 2, status: 'matched' as const },
  ],
  preferenceMatches: [{ skillId: 1, skillName: 'React', matchType: 'preferred_skill' as const }],
}

describe('matchingApi.listProjectMatches', () => {
  it('マッチ候補を順位付きで変換する', async () => {
    server.use(
      http.get(api('/projects/1/matches'), () =>
        HttpResponse.json({
          matches: [apiMatch, { ...apiMatch, engineer: { id: 2, name: '鈴木 花子', primarySalesName: '' }, score: 72 }],
        }),
      ),
    )

    const candidates = await matchingApi.listProjectMatches(1)

    expect(candidates).toHaveLength(2)
    expect(candidates[0].rank).toBe(1)
    expect(candidates[1].rank).toBe(2)
    expect(candidates[0].score).toBe(85)
    expect(candidates[0].engineer).toMatchObject({ id: 1, name: '田中 太郎', sales: '佐藤 営業' })
  })

  it('スコア内訳をアプリ内の項目名に変換する', async () => {
    server.use(http.get(api('/projects/1/matches'), () => HttpResponse.json({ matches: [apiMatch] })))

    const [candidate] = await matchingApi.listProjectMatches(1)

    expect(candidate.breakdown).toEqual({ skill: 55, years: 20, preference: 10 })
  })

  it('スキル評価の requirement を必須/歓迎に変換し、経験 0 年は null にする', async () => {
    server.use(http.get(api('/projects/1/matches'), () => HttpResponse.json({ matches: [apiMatch] })))

    const [candidate] = await matchingApi.listProjectMatches(1)

    expect(candidate.evaluations).toEqual([
      { skill: 'React', type: '必須', requiredYears: 3, actualYears: 5, status: 'matched' },
      { skill: '金融ドメイン経験', type: '必須', requiredYears: 2, actualYears: null, status: 'unmet' },
      { skill: 'AWS', type: '歓迎', requiredYears: 1, actualYears: 2, status: 'matched' },
    ])
  })

  it('営業名が空の場合は「未設定」と表示する', async () => {
    server.use(
      http.get(api('/projects/1/matches'), () =>
        HttpResponse.json({ matches: [{ ...apiMatch, engineer: { id: 1, name: '田中 太郎', primarySalesName: '' } }] }),
      ),
    )

    const [candidate] = await matchingApi.listProjectMatches(1)

    expect(candidate.engineer.sales).toBe('未設定')
  })
})
