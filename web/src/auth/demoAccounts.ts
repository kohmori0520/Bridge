import type { Role } from '../shared/types/domain'

/** 開発環境のみ。本番ビルドでは Vite が除去し null になる */
export const demoAccounts: Record<Role, { email: string; password: string }> | null = import.meta.env.DEV
  ? {
      Admin: { email: 'admin@bridge.local', password: 'Admin1234!' },
      Sales: { email: 'sato@bridge.local', password: 'Sales1234!' },
      Engineer: { email: 'tanaka@bridge.local', password: 'Engineer1234!' },
    }
  : null
