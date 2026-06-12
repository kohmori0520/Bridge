import { useMediaQuery, useTheme } from '@mui/material'

/**
 * md 未満 (900px) をモバイル相当として扱う。
 * 一覧はテーブルの代わりにカード表示へ切り替える(Docs/screen-design.md のモバイル方針参照)。
 */
export function useIsMobile() {
  const theme = useTheme()
  return useMediaQuery(theme.breakpoints.down('md'))
}
