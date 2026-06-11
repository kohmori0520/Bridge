import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { NotificationProvider } from './NotificationProvider'
import { useNotify } from './useNotify'

function NotifyButton({ message, severity }: { message: string; severity?: 'success' | 'error' }) {
  const notify = useNotify()
  return (
    <button type="button" onClick={() => notify(message, severity)}>
      通知する
    </button>
  )
}

describe('NotificationProvider', () => {
  it('notify でメッセージをトースト表示する', async () => {
    const user = userEvent.setup()
    render(
      <NotificationProvider>
        <NotifyButton message="保存しました" />
      </NotificationProvider>,
    )

    expect(screen.queryByText('保存しました')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: '通知する' }))

    expect(await screen.findByText('保存しました')).toBeInTheDocument()
  })

  it('閉じるボタンで通知を消せる', async () => {
    const user = userEvent.setup()
    render(
      <NotificationProvider>
        <NotifyButton message="保存しました" />
      </NotificationProvider>,
    )

    await user.click(screen.getByRole('button', { name: '通知する' }))
    await user.click(await screen.findByTitle('Close'))

    expect(screen.queryByText('保存しました')).not.toBeInTheDocument()
  })

  it('NotificationProvider の外で useNotify を使うと例外を投げる', () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {})

    expect(() => render(<NotifyButton message="x" />)).toThrowError(
      'useNotify must be used within NotificationProvider',
    )

    consoleError.mockRestore()
  })
})
