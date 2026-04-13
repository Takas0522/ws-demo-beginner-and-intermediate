import { Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import SummaryPage from './pages/SummaryPage'
import BusinessUnitsPage from './pages/BusinessUnitsPage'
import BusinessUnitDetailPage from './pages/BusinessUnitDetailPage'
import ProjectsPage from './pages/ProjectsPage'
import ProjectDetailPage from './pages/ProjectDetailPage'
import TicketsPage from './pages/TicketsPage'
import TicketDetailPage from './pages/TicketDetailPage'
import PullRequestsPage from './pages/PullRequestsPage'
import PullRequestDetailPage from './pages/PullRequestDetailPage'

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/" element={<Layout />}>
            <Route index element={<SummaryPage />} />
            <Route path="business-units" element={<BusinessUnitsPage />} />
            <Route path="business-units/:id" element={<BusinessUnitDetailPage />} />
            <Route path="projects" element={<ProjectsPage />} />
            <Route path="projects/:id" element={<ProjectDetailPage />} />
            <Route path="tickets" element={<TicketsPage />} />
            <Route path="tickets/:id" element={<TicketDetailPage />} />
            <Route path="pull-requests" element={<PullRequestsPage />} />
            <Route path="pull-requests/:id" element={<PullRequestDetailPage />} />
          </Route>
        </Route>
      </Routes>
    </AuthProvider>
  )
}
