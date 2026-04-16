import React, { createContext, useContext, ReactNode } from 'react'
import { useAuth, UseAuth } from '../hooks/useAuth'

const AuthContext = createContext<UseAuth | null>(null)

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const auth = useAuth()
  return <AuthContext.Provider value={auth}>{children}</AuthContext.Provider>
}

export const useAuthContext = (): UseAuth => {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuthContext must be used inside AuthProvider')
  return ctx
}
