import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { LoginPage } from '../auth/LoginPage'
import { RequireRole } from '../auth/RequireRole'
import { EngineerDetailPage } from '../features/engineers/pages/EngineerDetailPage'
import { EngineerForm } from '../features/engineers/pages/EngineerForm'
import { EngineersPage } from '../features/engineers/pages/EngineersPage'
import { MyContractsPage } from '../features/me/pages/MyContractsPage'
import { MyMatchesPage } from '../features/me/pages/MyMatchesPage'
import { MyProfilePage } from '../features/me/pages/MyProfilePage'
import { ProjectDetailPage } from '../features/projects/pages/ProjectDetailPage'
import { ProjectForm } from '../features/projects/pages/ProjectForm'
import { ProjectsPage } from '../features/projects/pages/ProjectsPage'
import { AppLayout } from './AppLayout'
import { HomeByRole } from './HomeByRole'
import { NotFoundPage } from './NotFoundPage'

export function AppRoutes() {
  return (
    <BrowserRouter>
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
            element={<RequireRole allowed={['Sales']}><EngineersPage /></RequireRole>}
          />
          <Route
            path="/engineers/new"
            element={<RequireRole allowed={['Sales']}><EngineerForm mode="new" /></RequireRole>}
          />
          <Route
            path="/engineers/:id"
            element={<RequireRole allowed={['Sales']}><EngineerDetailPage /></RequireRole>}
          />
          <Route
            path="/engineers/:id/edit"
            element={<RequireRole allowed={['Sales']}><EngineerForm mode="edit" /></RequireRole>}
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
    </BrowserRouter>
  )
}
