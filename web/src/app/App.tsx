import './styles/forms.css'
import './styles/layout.css'
import './styles/shared-ui.css'
import '../auth/styles/auth.css'
import '../features/contracts/styles/contracts.css'
import '../features/engineers/styles/engineers.css'
import '../features/matching/styles/matching.css'
import '../features/projects/styles/projects.css'
import { AppProviders } from './AppProviders'
import { AppRoutes } from './AppRoutes'

function App() {
  return (
    <AppProviders>
      <AppRoutes />
    </AppProviders>
  )
}

export default App
