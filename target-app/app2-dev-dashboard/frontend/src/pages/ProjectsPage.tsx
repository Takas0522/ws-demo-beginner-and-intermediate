import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getProjects } from '../api/client'
import { useState } from 'react'

export default function ProjectsPage() {
  const [status, setStatus] = useState<string>('')
  const { data, isLoading } = useQuery({
    queryKey: ['projects', status],
    queryFn: () => getProjects({ status: status || undefined }),
  })

  const statusColor = (s: string) =>
    s === 'active' ? 'bg-blue-100 text-blue-700'
    : s === 'completed' ? 'bg-green-100 text-green-700'
    : 'bg-gray-100 text-gray-500'

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-gray-800">プロジェクト一覧</h2>
        <select
          value={status}
          onChange={e => setStatus(e.target.value)}
          className="text-sm border rounded px-3 py-1.5"
        >
          <option value="">全ステータス</option>
          <option value="active">進行中</option>
          <option value="completed">完了</option>
          <option value="planned">計画中</option>
        </select>
      </div>

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              {['プロジェクト名', '事業部', '進捗', 'CPI', 'SPI', 'ステータス', '終了予定'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y">
            {data?.map((p: {
              id: string; name: string; status: string; progressPercent: number
              cpi?: number; spi?: number; plannedEndDate: string
              businessUnit: { name: string }
            }) => (
              <tr key={p.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <Link to={`/projects/${p.id}`} className="text-slate-700 hover:underline font-medium">
                    {p.name}
                  </Link>
                </td>
                <td className="px-4 py-3 text-gray-600">{p.businessUnit?.name}</td>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2">
                    <div className="w-20 bg-gray-100 rounded-full h-1.5">
                      <div className="bg-slate-500 h-1.5 rounded-full" style={{ width: `${p.progressPercent}%` }} />
                    </div>
                    <span className="text-xs">{p.progressPercent}%</span>
                  </div>
                </td>
                <td className={`px-4 py-3 font-medium ${p.cpi != null && p.cpi < 1 ? 'text-red-500' : 'text-emerald-600'}`}>
                  {p.cpi?.toFixed(2) ?? '-'}
                </td>
                <td className={`px-4 py-3 font-medium ${p.spi != null && p.spi < 1 ? 'text-amber-600' : 'text-emerald-600'}`}>
                  {p.spi?.toFixed(2) ?? '-'}
                </td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${statusColor(p.status)}`}>
                    {p.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-500">{p.plannedEndDate}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
