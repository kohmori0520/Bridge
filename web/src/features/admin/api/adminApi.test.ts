import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { api, server } from '../../../test/server'
import { adminApi } from './adminApi'

const createdUser = {
  userId: 1,
  email: 'new@bridge.local',
  role: 'Sales',
  salesId: 10,
  engineerId: null,
}

describe('adminApi.createUser', () => {
  it('Sales 作成時は department を送信し primarySalesId は null にする', async () => {
    let requestBody: Record<string, unknown> | null = null
    server.use(
      http.post(api('/users'), async ({ request }) => {
        requestBody = (await request.json()) as Record<string, unknown>
        return HttpResponse.json(createdUser, { status: 201 })
      }),
    )

    await adminApi.createUser({
      email: 'new@bridge.local',
      password: 'Sales1234!',
      role: 'Sales',
      name: '新規 営業',
      department: '第一営業部',
      primarySalesId: 5,
    })

    expect(requestBody).toEqual({
      email: 'new@bridge.local',
      password: 'Sales1234!',
      role: 'Sales',
      name: '新規 営業',
      department: '第一営業部',
      primarySalesId: null,
    })
  })

  it('Engineer 作成時は primarySalesId を送信し department は空にする', async () => {
    let requestBody: Record<string, unknown> | null = null
    server.use(
      http.post(api('/users'), async ({ request }) => {
        requestBody = (await request.json()) as Record<string, unknown>
        return HttpResponse.json({ ...createdUser, role: 'Engineer', salesId: null, engineerId: 20 }, { status: 201 })
      }),
    )

    await adminApi.createUser({
      email: 'engineer@bridge.local',
      password: 'Engineer1234!',
      role: 'Engineer',
      name: '新規 技術者',
      department: '無視される部署',
      primarySalesId: 5,
    })

    expect(requestBody).toMatchObject({ role: 'Engineer', department: '', primarySalesId: 5 })
  })
})

describe('adminApi.listSales', () => {
  it('営業一覧を取得する', async () => {
    const sales = [{ id: 1, name: '佐藤 営業', department: '第一営業部', email: 'sato@bridge.local' }]
    server.use(http.get(api('/sales'), () => HttpResponse.json(sales)))

    await expect(adminApi.listSales()).resolves.toEqual(sales)
  })
})
