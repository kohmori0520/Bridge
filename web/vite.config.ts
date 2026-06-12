/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const isCi = !!process.env.CI

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    watch: {
      usePolling: true,
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setupMocks.ts', './src/test/setup.ts'],
    include: ['src/**/*.test.{ts,tsx}'],
    // テストでは常にローカル API URL を使い、.env.local の影響を受けないようにする
    env: {
      VITE_API_BASE_URL: 'http://localhost:5130',
    },
    // CI (2 vCPU) では worker 数を絞り、依存 transform をキャッシュする
    maxWorkers: isCi ? 2 : undefined,
    deps: {
      optimizer: {
        web: {
          enabled: true,
        },
      },
    },
    coverage: {
      provider: 'v8',
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/**/*.test.{ts,tsx}', 'src/test/**', 'src/main.tsx', 'src/**/styles/**'],
    },
  },
})
