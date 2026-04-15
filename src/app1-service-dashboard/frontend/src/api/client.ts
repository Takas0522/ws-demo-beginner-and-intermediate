import axios from 'axios'
import { getToken, clearAuth } from './authClient'

const api = axios.create({ baseURL: '/api' })

// JWT を自動付与
api.interceptors.request.use(config => {
  const token = getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// 401 → ログアウト
api.interceptors.response.use(
  r => r,
  err => {
    if (err.response?.status === 401) {
      clearAuth()
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

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

// ─── Stakeholders ─────────────────────────────────────────────────────────────
export const getStakeholders = (serviceId: string) =>
  api.get(`/services/${serviceId}/stakeholders`).then(r => r.data)

export const addStakeholder = (serviceId: string, body: {
  authUserId: string
  displayName: string
  role: string
  hourlyRate: number
  allocatedHoursMonthly: number
}) => api.post(`/services/${serviceId}/stakeholders`, body).then(r => r.data)

export const updateStakeholder = (serviceId: string, id: string, body: {
  authUserId: string
  displayName: string
  role: string
  hourlyRate: number
  allocatedHoursMonthly: number
}) => api.put(`/services/${serviceId}/stakeholders/${id}`, body).then(r => r.data)

export const deleteStakeholder = (serviceId: string, id: string) =>
  api.delete(`/services/${serviceId}/stakeholders/${id}`).then(r => r.data)

// ─── AB Tests ─────────────────────────────────────────────────────────────────
export const getAbTests = (serviceId: string, status?: string) =>
  api.get(`/services/${serviceId}/ab-tests`, { params: { status } }).then(r => r.data)

export const getAbTestDetail = (id: string) =>
  api.get(`/ab-tests/${id}`).then(r => r.data)

// ─── Export ───────────────────────────────────────────────────────────────────
export const downloadCsv = async (path: string, filename: string, params?: Record<string, unknown>) => {
  const res = await api.get(`/export/${path}`, {
    params,
    responseType: 'blob',
  })
  const url = URL.createObjectURL(new Blob([res.data], { type: 'text/csv' }))
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

/** @deprecated JWT認証追加後は downloadCsv を使用してください */
export const exportUrl = (path: string, params?: Record<string, unknown>) => {
  const url = new URL(`/api/export/${path}`, window.location.origin)
  if (params) {
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null) url.searchParams.append(k, String(v))
    })
  }
  return url.toString()
}
