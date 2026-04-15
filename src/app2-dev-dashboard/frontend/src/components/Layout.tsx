import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthContext } from '../context/AuthContext'

const navItems = [
  { to: '/',               label: '全社サマリー' },
  { to: '/business-units', label: '事業部別' },
  { to: '/projects',       label: 'プロジェクト' },
  { to: '/tickets',        label: 'チケット' },
  { to: '/pull-requests',  label: 'プルリクエスト' },
]

export default function Layout() {
  const { user, logout } = useAuthContext()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <aside className="w-60 bg-slate-800 text-white flex flex-col">
        <div className="px-4 py-5 border-b border-slate-700">
          <h1 className="text-sm font-bold leading-tight">開発状況確認<br />ダッシュボード</h1>
        </div>
        <nav className="flex-1 px-2 py-4 space-y-1">
          {navItems.map(item => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) =>
                `block px-3 py-2 rounded text-sm ${
                  isActive ? 'bg-slate-600 font-semibold' : 'hover:bg-slate-700'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="px-4 py-4 border-t border-slate-700">
          <p className="text-xs text-slate-400 truncate mb-2">{user?.displayName ?? user?.username}</p>
          <button
            onClick={handleLogout}
            className="w-full text-xs bg-slate-600 hover:bg-slate-500 text-white py-1.5 rounded transition"
          >
            ログアウト
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-auto p-6">
        <Outlet />
      </main>
    </div>
  )
}
