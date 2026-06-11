import { http, HttpResponse } from 'msw'
import { describe, expect, it, vi } from 'vitest'
import { api, server } from '../../test/server'
import { ApiError, apiRequest, setAccessToken, setUnauthorizedHandler } from './http'

describe('apiRequest', () => {
  it('GET リクエストで JSON レスポンスを返す', async () => {
    server.use(http.get(api('/items'), () => HttpResponse.json({ value: 42 })))

    await expect(apiRequest<{ value: number }>('/items')).resolves.toEqual({ value: 42 })
  })

  it('body がある場合は Content-Type: application/json を自動付与する', async () => {
    let contentType: string | null = null
    server.use(
      http.post(api('/items'), ({ request }) => {
        contentType = request.headers.get('Content-Type')
        return HttpResponse.json({})
      }),
    )

    await apiRequest('/items', { method: 'POST', body: JSON.stringify({ a: 1 }) })

    expect(contentType).toBe('application/json')
  })

  it('body がない場合は Content-Type を付与しない', async () => {
    let contentType: string | null = 'unset'
    server.use(
      http.get(api('/items'), ({ request }) => {
        contentType = request.headers.get('Content-Type')
        return HttpResponse.json({})
      }),
    )

    await apiRequest('/items')

    expect(contentType).toBeNull()
  })

  it('アクセストークン設定時は Authorization ヘッダーを付与する', async () => {
    let authorization: string | null = null
    server.use(
      http.get(api('/items'), ({ request }) => {
        authorization = request.headers.get('Authorization')
        return HttpResponse.json({})
      }),
    )

    setAccessToken('test-token')
    await apiRequest('/items')

    expect(authorization).toBe('Bearer test-token')
  })

  it('トークン未設定時は Authorization ヘッダーを付与しない', async () => {
    let authorization: string | null = 'unset'
    server.use(
      http.get(api('/items'), ({ request }) => {
        authorization = request.headers.get('Authorization')
        return HttpResponse.json({})
      }),
    )

    await apiRequest('/items')

    expect(authorization).toBeNull()
  })

  it('204 No Content は undefined を返す', async () => {
    server.use(http.delete(api('/items/1'), () => new HttpResponse(null, { status: 204 })))

    await expect(apiRequest<void>('/items/1', { method: 'DELETE' })).resolves.toBeUndefined()
  })

  it('エラー時は構造化エラー (error.message) からメッセージを抽出する', async () => {
    server.use(
      http.get(api('/items'), () =>
        HttpResponse.json({ error: { code: 'NOT_FOUND', message: '対象が見つかりません' } }, { status: 404 }),
      ),
    )

    const promise = apiRequest('/items')
    await expect(promise).rejects.toBeInstanceOf(ApiError)
    await expect(promise).rejects.toMatchObject({ status: 404, message: '対象が見つかりません' })
  })

  it('トップレベル message のみのエラーレスポンスにも対応する', async () => {
    server.use(http.get(api('/items'), () => HttpResponse.json({ message: '認証エラー' }, { status: 401 })))

    await expect(apiRequest('/items')).rejects.toMatchObject({ status: 401, message: '認証エラー' })
  })

  it('JSON でないエラーレスポンスは statusText にフォールバックする', async () => {
    server.use(
      http.get(api('/items'), () => new HttpResponse('oops', { status: 500, statusText: 'Internal Server Error' })),
    )

    await expect(apiRequest('/items')).rejects.toMatchObject({ status: 500, message: 'Internal Server Error' })
  })

  it('トークン付きリクエストが 401 の場合はトークンを破棄し unauthorized ハンドラーを呼ぶ', async () => {
    const requestAuthHeaders: (string | null)[] = []
    server.use(
      http.get(api('/items'), ({ request }) => {
        requestAuthHeaders.push(request.headers.get('Authorization'))
        return HttpResponse.json({ message: 'expired' }, { status: 401 })
      }),
    )
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)
    setAccessToken('expired-token')

    await expect(apiRequest('/items')).rejects.toMatchObject({ status: 401 })
    expect(onUnauthorized).toHaveBeenCalledTimes(1)

    // トークンは破棄されており、次のリクエストには付与されない
    await expect(apiRequest('/items')).rejects.toMatchObject({ status: 401 })
    expect(requestAuthHeaders).toEqual(['Bearer expired-token', null])
  })

  it('トークンなしの 401 (ログイン失敗) では unauthorized ハンドラーを呼ばない', async () => {
    server.use(http.post(api('/auth/login'), () => HttpResponse.json({ message: '認証失敗' }, { status: 401 })))
    const onUnauthorized = vi.fn()
    setUnauthorizedHandler(onUnauthorized)

    await expect(apiRequest('/auth/login', { method: 'POST', body: '{}' })).rejects.toMatchObject({ status: 401 })

    expect(onUnauthorized).not.toHaveBeenCalled()
  })
})
