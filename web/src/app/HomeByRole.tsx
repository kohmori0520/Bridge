import { useAuth } from '../auth/useAuth'
import { EngineerDashboard } from '../features/me/pages/EngineerDashboard'
import { SalesDashboard } from '../features/sales/pages/SalesDashboard'

export function HomeByRole() {
  const { user } = useAuth()
  return user.role === 'Sales' ? <SalesDashboard /> : <EngineerDashboard />
}
