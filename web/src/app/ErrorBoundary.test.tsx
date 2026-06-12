import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ErrorBoundary } from './ErrorBoundary'

function ThrowOnRender({ shouldThrow }: { shouldThrow: boolean }) {
  if (shouldThrow) {
    throw new Error('テスト用のレンダリングエラー')
  }
  return <div>正常表示</div>
}

describe('ErrorBoundary', () => {
  it('子コンポーネントをそのまま表示する', () => {
    render(
      <ErrorBoundary>
        <ThrowOnRender shouldThrow={false} />
      </ErrorBoundary>,
    )

    expect(screen.getByText('正常表示')).toBeInTheDocument()
  })

  it('レンダリングエラー時にフォールバック UI を表示する', () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})

    render(
      <ErrorBoundary>
        <ThrowOnRender shouldThrow />
      </ErrorBoundary>,
    )

    expect(screen.getByText('予期しないエラーが発生しました')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: '再読み込み' })).toBeInTheDocument()

    consoleError.mockRestore()
  })

  it('再読み込みボタンで location.reload を呼ぶ', async () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})
    const reload = vi.fn()
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: { ...window.location, reload },
    })

    const user = userEvent.setup()
    render(
      <ErrorBoundary>
        <ThrowOnRender shouldThrow />
      </ErrorBoundary>,
    )

    await user.click(screen.getByRole('button', { name: '再読み込み' }))

    expect(reload).toHaveBeenCalledTimes(1)

    consoleError.mockRestore()
  })
})
