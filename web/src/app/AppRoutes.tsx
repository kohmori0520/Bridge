import { lazy, Suspense } from 'react'
import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { RequireRole } from '../auth/RequireRole'
import { LoadingState } from '../shared/components/LoadingState'
import { AppLayout } from './AppLayout'
import { ScrollToTop } from './ScrollToTop'

const LoginPage = lazy(() => import('../auth/LoginPage').then((module) => ({ default: module.LoginPage })))
const AdminExpiringContractsPage = lazy(() =>
  import('../features/admin/pages/AdminExpiringContractsPage').then((module) => ({ default: module.AdminExpiringContractsPage })),
)
const AdminUserCreatePage = lazy(() =>
  import('../features/admin/pages/AdminUserCreatePage').then((module) => ({ default: module.AdminUserCreatePage })),
)
const EngineerDetailPage = lazy(() =>
  import('../features/engineers/pages/EngineerDetailPage').then((module) => ({ default: module.EngineerDetailPage })),
)
const EngineerForm = lazy(() =>
  import('../features/engineers/pages/EngineerForm').then((module) => ({ default: module.EngineerForm })),
)
const EngineersPage = lazy(() =>
  import('../features/engineers/pages/EngineersPage').then((module) => ({ default: module.EngineersPage })),
)
const MyContractsPage = lazy(() =>
  import('../features/me/pages/MyContractsPage').then((module) => ({ default: module.MyContractsPage })),
)
const MyMatchesPage = lazy(() =>
  import('../features/me/pages/MyMatchesPage').then((module) => ({ default: module.MyMatchesPage })),
)
const MyProfilePage = lazy(() =>
  import('../features/me/pages/MyProfilePage').then((module) => ({ default: module.MyProfilePage })),
)
const ProjectDetailPage = lazy(() =>
  import('../features/projects/pages/ProjectDetailPage').then((module) => ({ default: module.ProjectDetailPage })),
)
const ProjectForm = lazy(() =>
  import('../features/projects/pages/ProjectForm').then((module) => ({ default: module.ProjectForm })),
)
const ProjectsPage = lazy(() =>
  import('../features/projects/pages/ProjectsPage').then((module) => ({ default: module.ProjectsPage })),
)
const HomeByRole = lazy(() => import('./HomeByRole').then((module) => ({ default: module.HomeByRole })))
const NotFoundPage = lazy(() => import('./NotFoundPage').then((module) => ({ default: module.NotFoundPage })))

export function AppRoutes() {
  return (
    <BrowserRouter>
      <ScrollToTop />
      <Suspense fallback={<LoadingState message="画面を読み込み中です。" />}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<AppLayout />}>
            <Route path="/" element={<HomeByRole />} />
            <Route path="/projects" element={<ProjectsPage />} />
            <Route
              path="/projects/new"
              element={<RequireRole allowed={['Sales']}><ProjectForm mode="new" /></RequireRole>}
            />
            <Route path="/projects/:id" element={<ProjectDetailPage />} />
            <Route
              path="/projects/:id/edit"
              element={<RequireRole allowed={['Sales']}><ProjectForm mode="edit" /></RequireRole>}
            />
            <Route
              path="/engineers"
              element={<RequireRole allowed={['Admin', 'Sales']}><EngineersPage /></RequireRole>}
            />
            <Route
              path="/engineers/new"
              element={<RequireRole allowed={['Admin', 'Sales']}><EngineerForm mode="new" /></RequireRole>}
            />
            <Route
              path="/engineers/:id"
              element={<RequireRole allowed={['Admin', 'Sales']}><EngineerDetailPage /></RequireRole>}
            />
            <Route
              path="/engineers/:id/edit"
              element={<RequireRole allowed={['Admin', 'Sales']}><EngineerForm mode="edit" /></RequireRole>}
            />
            <Route
              path="/admin/users/new"
              element={<RequireRole allowed={['Admin']}><AdminUserCreatePage /></RequireRole>}
            />
            <Route
              path="/admin/contracts/expiring"
              element={<RequireRole allowed={['Admin']}><AdminExpiringContractsPage /></RequireRole>}
            />
            <Route
              path="/me/profile"
              element={<RequireRole allowed={['Engineer']}><MyProfilePage /></RequireRole>}
            />
            <Route
              path="/me/contracts"
              element={<RequireRole allowed={['Engineer']}><MyContractsPage /></RequireRole>}
            />
            <Route
              path="/me/matches"
              element={<RequireRole allowed={['Engineer']}><MyMatchesPage /></RequireRole>}
            />
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </Suspense>
    </BrowserRouter>
  )
}
