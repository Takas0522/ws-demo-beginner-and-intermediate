import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import SummaryPage from './pages/SummaryPage'
import BusinessUnitsPage from './pages/BusinessUnitsPage'
import BusinessUnitDetailPage from './pages/BusinessUnitDetailPage'
import ServicesPage from './pages/ServicesPage'
import ServiceDetailPage from './pages/ServiceDetailPage'
import AbTestsPage from './pages/AbTestsPage'
import AbTestDetailPage from './pages/AbTestDetailPage'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<SummaryPage />} />
        <Route path="business-units" element={<BusinessUnitsPage />} />
        <Route path="business-units/:id" element={<BusinessUnitDetailPage />} />
        <Route path="services" element={<ServicesPage />} />
        <Route path="services/:id" element={<ServiceDetailPage />} />
        <Route path="ab-tests" element={<AbTestsPage />} />
        <Route path="ab-tests/:id" element={<AbTestDetailPage />} />
      </Route>
    </Routes>
  )
}
