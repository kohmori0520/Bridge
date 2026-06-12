import { describe, expect, it } from 'vitest'
import { parseRouteId } from './routeParams'

describe('parseRouteId', () => {
  it('正の整数文字列を ID に変換する', () => {
    expect(parseRouteId('1')).toBe(1)
    expect(parseRouteId('42')).toBe(42)
  })

  it.each(['abc', '0', '-1', '1.5', '', undefined])('不正な値 "%s" は null を返す', (raw) => {
    expect(parseRouteId(raw)).toBeNull()
  })
})
