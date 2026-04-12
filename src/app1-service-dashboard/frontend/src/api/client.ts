import axios from 'axios'

const api = axios.create({ baseURL: '/api' })

export type DateRange = { from?: string; to?: string }

// ─── Summary ──────────────────────────────────────────────────────────────────
export const getSummary = (range?: DateRange) =>
  api.get('/summary', { params: range }).then(r => r.data)

// ─── Business Units ───────────────────────────────────────────────────────────
export const getBusinessUnits = () =>
  api.get('/business-units').then(r => r.data)

export const getBusinessUnitSummary = (id: string, range?: DateRange) =>
  api.get(`/business-units/${id}/summary`, { params: range }).then(r => r.data)

// ─── Services ─────────────────────────────────────────────────────────────────
export const getServices = (params?: {
  businessUnitIds?: string[]
  categoryIds?: string[]
  status?: string
}) => api.get('/services', { params }).then(r => r.data)

export const getServiceDetail = (id: string, range?: DateRange) =>
  api.get(`/services/${id}`, { params: range }).then(r => r.data)

// ─── AB Tests ─────────────────────────────────────────────────────────────────
export const getAbTests = (serviceId: string, status?: string) =>
  api.get(`/services/${serviceId}/ab-tests`, { params: { status } }).then(r => r.data)

export const getAbTestDetail = (id: string) =>
  api.get(`/ab-tests/${id}`).then(r => r.data)

// ─── Export ───────────────────────────────────────────────────────────────────
export const exportUrl = (path: string, params?: Record<string, unknown>) => {
  const url = new URL(`/api/export/${path}`, window.location.origin)
  if (params) {
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null) url.searchParams.append(k, String(v))
    })
  }
  return url.toString()
}
