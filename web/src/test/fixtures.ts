import type { Role, User } from '../shared/types/domain'

// テスト専用のユーザー fixture(本番バンドルには含めない)
export const users: Record<Role, User> = {
  Admin: { role: 'Admin', name: '管理者', email: 'admin@bridge.local' },
  Sales: { role: 'Sales', name: '佐藤 営業', email: 'sato@bridge.local' },
  Engineer: { role: 'Engineer', name: '田中 太郎', email: 'tanaka@bridge.local' },
}
