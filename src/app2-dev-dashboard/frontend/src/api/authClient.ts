import axios from 'axios'

const AUTH_BASE = import.meta.env.VITE_AUTH_API_URL ?? 'http://localhost:5000'
const TOKEN_KEY = 'auth_token'
const USER_KEY  = 'auth_user'

export interface AuthUser {
  id:             string
  username:       string
  email:          string
  displayName:    string
  role:           string
  departmentId:   string | null
  departmentName: string | null
}

export interface LoginResponse {
  token: string
  user:  AuthUser
}

const authApi = axios.create({ baseURL: AUTH_BASE })

// ─── トークン管理 ──────────────────────────────────────────────────────────────

export const getToken    = (): string | null  => localStorage.getItem(TOKEN_KEY)
export const getAuthUser = (): AuthUser | null => {
  const raw = localStorage.getItem(USER_KEY)
  return raw ? JSON.parse(raw) : null
}

export const saveAuth = (res: LoginResponse) => {
  localStorage.setItem(TOKEN_KEY, res.token)
  localStorage.setItem(USER_KEY, JSON.stringify(res.user))
}

export const clearAuth = () => {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}

export const isAuthenticated = (): boolean => !!getToken()

// ─── API ──────────────────────────────────────────────────────────────────────

export const login = async (username: string, password: string): Promise<LoginResponse> => {
  const res = await authApi.post<LoginResponse>('/api/auth/login', { username, password })
  return res.data
}

export const fetchMe = async (): Promise<AuthUser> => {
  const token = getToken()
  const res = await authApi.get<AuthUser>('/api/auth/me', {
    headers: { Authorization: `Bearer ${token}` },
  })
  return res.data
}
