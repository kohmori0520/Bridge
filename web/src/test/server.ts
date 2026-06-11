import { setupServer } from 'msw/node'

/** テスト時の API ベース URL (vite.config.ts の test.env で固定) */
export const API_BASE_URL = 'http://localhost:5130'

export function api(path: string) {
  return `${API_BASE_URL}${path}`
}

/**
 * MSW モックサーバー。
 * デフォルトハンドラーは持たず、各テストが `server.use(...)` で必要な分を登録する。
 * 未登録のリクエストはエラーになる (setup.ts の onUnhandledRequest: 'error')。
 */
export const server = setupServer()
