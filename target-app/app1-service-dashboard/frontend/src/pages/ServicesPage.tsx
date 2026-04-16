import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getServices } from '../api/client'

export default function ServicesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['services'],
    queryFn: () => getServices({ status: 'active' }),
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold text-gray-800">サービス一覧</h2>
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              {['サービス名', 'カテゴリ', '事業部', '状態', '提供開始'].map(h => (
                <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y">
            {data?.map((svc: {
              id: string; name: string; status: string; launchedAt: string
              category: { name: string }; businessUnit: { name: string }
            }) => (
              <tr key={svc.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <Link to={`/services/${svc.id}`} className="text-indigo-600 hover:underline font-medium">
                    {svc.name}
                  </Link>
                </td>
                <td className="px-4 py-3 text-gray-600">{svc.category.name}</td>
                <td className="px-4 py-3 text-gray-600">{svc.businessUnit.name}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${
                    svc.status === 'active' ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
                  }`}>
                    {svc.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-500">{svc.launchedAt}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
