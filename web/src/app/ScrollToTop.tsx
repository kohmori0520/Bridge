import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'

/** 画面遷移時にスクロール位置を先頭へ戻す */
export function ScrollToTop() {
  const { pathname } = useLocation()

  useEffect(() => {
    window.scrollTo({ top: 0, left: 0 })
  }, [pathname])

  return null
}
