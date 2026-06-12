import type { ReactNode } from 'react'

/** テスト用の GridColDef スタブ (本番型と互換の最小定義) */
export type GridColDef<T = unknown> = {
  field: string
  headerName?: string
  flex?: number
  minWidth?: number
  width?: number
  sortable?: boolean
  renderCell?: (params: { value?: unknown; row: T }) => ReactNode
}

type DataGridProps<T extends { id: number | string }> = {
  rows: T[]
  columns: GridColDef<T>[]
  loading?: boolean
  localeText?: { noRowsLabel?: string }
}

/**
 * @mui/x-data-grid の軽量モック。
 * 本番 DataGrid (~430KB) の transform/import を避け、行データの表示検証に十分な table を返す。
 */
export function DataGrid<T extends { id: number | string }>({ rows, columns, loading, localeText }: DataGridProps<T>) {
  if (loading) {
    return <div role="progressbar">Loading...</div>
  }

  if (rows.length === 0) {
    return <div>{localeText?.noRowsLabel ?? 'No rows'}</div>
  }

  return (
    <table data-testid="mock-data-grid">
      <tbody>
        {rows.map((row) => (
          <tr key={row.id}>
            {columns.map((column) => {
              const value = (row as Record<string, unknown>)[column.field]
              const content = column.renderCell ? column.renderCell({ value, row }) : String(value ?? '')
              return <td key={column.field}>{content}</td>
            })}
          </tr>
        ))}
      </tbody>
    </table>
  )
}
