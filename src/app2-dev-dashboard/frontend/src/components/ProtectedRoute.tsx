import React from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import { useAuthContext } from '../context/AuthContext'

export default function ProtectedRoute() {
  const { isAuthed } = useAuthContext()
  return isAuthed ? <Outlet /> : <Navigate to="/login" replace />
}
