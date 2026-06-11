import { describe, expect, it } from 'vitest'
import type { User } from '../shared/types/domain'
import { clearAuthSession, loadAuthSession, saveAuthSession } from './authStorage'

const SESSION_KEY = 'bridge.auth.session'

const user: User = { role: 'Engineer', name: '田中 太郎', email: 'tanaka@bridge.local' }

describe('authStorage', () => {
  it('保存したセッションを復元できる', () => {
    saveAuthSession({ token: 'jwt-token', user })

    expect(loadAuthSession()).toEqual({ token: 'jwt-token', user })
  })

  it('セッションがない場合は null を返す', () => {
    expect(loadAuthSession()).toBeNull()
  })

  it('壊れた JSON は破棄して null を返す', () => {
    window.sessionStorage.setItem(SESSION_KEY, '{invalid json')

    expect(loadAuthSession()).toBeNull()
    expect(window.sessionStorage.getItem(SESSION_KEY)).toBeNull()
  })

  it('clearAuthSession でセッションを削除する', () => {
    saveAuthSession({ token: 'jwt-token', user })

    clearAuthSession()

    expect(loadAuthSession()).toBeNull()
  })
})
