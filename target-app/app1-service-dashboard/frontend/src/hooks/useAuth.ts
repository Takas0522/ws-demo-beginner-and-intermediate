import { useState, useCallback } from 'react'
import { login as apiLogin, saveAuth, clearAuth, getToken, getAuthUser, AuthUser } from '../api/authClient'

export interface UseAuth {
  user:     AuthUser | null
  token:    string | null
  login:    (username: string, password: string) => Promise<void>
  logout:   () => void
  isAuthed: boolean
}

export function useAuth(): UseAuth {
  const [token, setToken] = useState<string | null>(getToken)
  const [user,  setUser]  = useState<AuthUser | null>(getAuthUser)

  const login = useCallback(async (username: string, password: string) => {
    const res = await apiLogin(username, password)
    saveAuth(res)
    setToken(res.token)
    setUser(res.user)
  }, [])

  const logout = useCallback(() => {
    clearAuth()
    setToken(null)
    setUser(null)
  }, [])

  return { user, token, login, logout, isAuthed: !!token }
}
