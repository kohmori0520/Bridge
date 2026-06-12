/** URL パスパラメータから正の整数 ID を取り出す。不正な値は null */
export function parseRouteId(raw: string | undefined): number | null {
  if (raw === undefined || raw.trim() === '') {
    return null
  }

  const id = Number(raw)
  if (!Number.isInteger(id) || id <= 0) {
    return null
  }

  return id
}
