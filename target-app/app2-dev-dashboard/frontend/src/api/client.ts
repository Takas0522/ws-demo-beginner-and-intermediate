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
export const getSummary = () => api.get('/summary').then(r => r.data)

// ─── Business Units ───────────────────────────────────────────────────────────
export const getBusinessUnits = () => api.get('/business-units').then(r => r.data)
export const getBusinessUnitSummary = (id: string) =>
  api.get(`/business-units/${id}/summary`).then(r => r.data)

// ─── Departments ──────────────────────────────────────────────────────────────
export const getDepartments = () => api.get('/departments').then(r => r.data)

// ─── Members ──────────────────────────────────────────────────────────────────
export const getMembers = (departmentId?: string) =>
  api.get('/members', { params: { departmentId } }).then(r => r.data)

// ─── Projects ─────────────────────────────────────────────────────────────────
export const getProjects = (params?: { businessUnitIds?: string[]; status?: string }) =>
  api.get('/projects', { params }).then(r => r.data)

export const getProjectDetail = (id: string) =>
  api.get(`/projects/${id}`).then(r => r.data)

export const getProjectSprints = (id: string) =>
  api.get(`/projects/${id}/sprints`).then(r => r.data)

export const getProjectTickets = (id: string, params?: {
  status?: string; ticketType?: string; priority?: string
}) => api.get(`/projects/${id}/tickets`, { params }).then(r => r.data)

export const getProjectPullRequests = (id: string, status?: string) =>
  api.get(`/projects/${id}/pull-requests`, { params: { status } }).then(r => r.data)

// ─── Tickets / PRs ────────────────────────────────────────────────────────────
export const getTicket = (id: string) => api.get(`/tickets/${id}`).then(r => r.data)
export const getPullRequest = (id: string) => api.get(`/pull-requests/${id}`).then(r => r.data)

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
