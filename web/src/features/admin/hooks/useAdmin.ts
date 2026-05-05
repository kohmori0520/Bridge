import { useQuery } from '@tanstack/react-query'
import { adminApi } from '../api/adminApi'

export function useSalesOptions() {
  return useQuery({
    queryKey: ['admin', 'sales-options'],
    queryFn: adminApi.listSales,
  })
}
