import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuthContext } from '../context/AuthContext'

const navItems = [
  { to: '/',              label: '全社サマリー' },
  { to: '/business-units', label: '事業部別' },
  { to: '/services',      label: 'サービス一覧' },
  { to: '/ab-tests',      label: 'ABテスト' },
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
      {/* サイドナビ */}
      <aside className="w-56 bg-indigo-800 text-white flex flex-col">
        <div className="px-4 py-5 border-b border-indigo-700">
          <h1 className="text-sm font-bold leading-tight">サービス<br />ダッシュボード</h1>
        </div>
        <nav className="flex-1 px-2 py-4 space-y-1">
          {navItems.map(item => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) =>
                `block px-3 py-2 rounded text-sm ${
                  isActive ? 'bg-indigo-600 font-semibold' : 'hover:bg-indigo-700'
                }`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="px-4 py-4 border-t border-indigo-700">
          <p className="text-xs text-indigo-300 truncate mb-2">{user?.displayName ?? user?.username}</p>
          <button
            onClick={handleLogout}
            className="w-full text-xs bg-indigo-600 hover:bg-indigo-500 text-white py-1.5 rounded transition"
          >
            ログアウト
          </button>
        </div>
      </aside>

      {/* メインコンテンツ */}
      <main className="flex-1 overflow-auto p-6">
        <Outlet />
      </main>
    </div>
  )
}
