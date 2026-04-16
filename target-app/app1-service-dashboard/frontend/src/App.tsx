import { Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import SummaryPage from './pages/SummaryPage'
import BusinessUnitsPage from './pages/BusinessUnitsPage'
import BusinessUnitDetailPage from './pages/BusinessUnitDetailPage'
import ServicesPage from './pages/ServicesPage'
import ServiceDetailPage from './pages/ServiceDetailPage'
import AbTestsPage from './pages/AbTestsPage'
import AbTestDetailPage from './pages/AbTestDetailPage'

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
            <Route path="services" element={<ServicesPage />} />
            <Route path="services/:id" element={<ServiceDetailPage />} />
            <Route path="ab-tests" element={<AbTestsPage />} />
            <Route path="ab-tests/:id" element={<AbTestDetailPage />} />
          </Route>
        </Route>
      </Routes>
    </AuthProvider>
  )
}
