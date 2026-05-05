import { lazy, Suspense } from 'react'
import { useAuth } from '../auth/useAuth'
import { LoadingState } from '../shared/components/LoadingState'

const EngineerDashboard = lazy(() =>
  import('../features/me/pages/EngineerDashboard').then((module) => ({ default: module.EngineerDashboard })),
)
const SalesDashboard = lazy(() =>
  import('../features/sales/pages/SalesDashboard').then((module) => ({ default: module.SalesDashboard })),
)
const AdminDashboard = lazy(() =>
  import('../features/admin/pages/AdminDashboard').then((module) => ({ default: module.AdminDashboard })),
)

export function HomeByRole() {
  const { user } = useAuth()
  return (
    <Suspense fallback={<LoadingState message="ホームを読み込み中です。" />}>
      {user?.role === 'Admin' ? <AdminDashboard /> : user?.role === 'Sales' ? <SalesDashboard /> : <EngineerDashboard />}
    </Suspense>
  )
}
