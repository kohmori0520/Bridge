import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { api, server } from '../test/server'
import { authApi, toUser } from './authApi'

describe('toUser', () => {
  it('API レスポンスをアプリ内の User に変換する', () => {
    const user = toUser({
      id: 1,
      email: 'sato@bridge.local',
      role: 'Sales',
      salesId: 10,
      engineerId: null,
      name: '佐藤 営業',
    })

    expect(user).toEqual({ role: 'Sales', name: '佐藤 営業', email: 'sato@bridge.local' })
  })

  it('name がない場合は email をフォールバックする', () => {
    const user = toUser({
      id: 2,
      email: 'tanaka@bridge.local',
      role: 'Engineer',
      salesId: null,
      engineerId: 5,
    })

    expect(user.name).toBe('tanaka@bridge.local')
  })

  it('未知のロールは例外を投げる', () => {
    expect(() =>
      toUser({ id: 3, email: 'x@bridge.local', role: 'SuperUser', salesId: null, engineerId: null }),
    ).toThrowError('Unsupported role: SuperUser')
  })
})

describe('authApi.login', () => {
  it('資格情報を送信しトークンと変換済みユーザーを返す', async () => {
    let requestBody: unknown = null
    server.use(
      http.post(api('/auth/login'), async ({ request }) => {
        requestBody = await request.json()
        return HttpResponse.json({
          token: 'jwt-token',
          user: { id: 1, email: 'tanaka@bridge.local', role: 'Engineer', salesId: null, engineerId: 5, name: '田中 太郎' },
        })
      }),
    )

    const result = await authApi.login('tanaka@bridge.local', 'Engineer1234!')

    expect(requestBody).toEqual({ email: 'tanaka@bridge.local', password: 'Engineer1234!' })
    expect(result).toEqual({
      token: 'jwt-token',
      user: { role: 'Engineer', name: '田中 太郎', email: 'tanaka@bridge.local' },
    })
  })

  it('認証失敗時は API のエラーメッセージで reject する', async () => {
    server.use(
      http.post(api('/auth/login'), () =>
        HttpResponse.json({ error: { code: 'UNAUTHORIZED', message: 'メールアドレスまたはパスワードが違います' } }, { status: 401 }),
      ),
    )

    await expect(authApi.login('tanaka@bridge.local', 'wrong')).rejects.toMatchObject({
      status: 401,
      message: 'メールアドレスまたはパスワードが違います',
    })
  })
})
