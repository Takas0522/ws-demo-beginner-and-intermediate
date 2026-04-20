import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getBusinessUnitSummary } from '../api/client'
import KpiCard from '../components/KpiCard'

export default function BusinessUnitDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading } = useQuery({
    queryKey: ['bu-summary', id],
    queryFn: () => getBusinessUnitSummary(id!),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtYen = (n: number) => `¥${(n / 1_000_000).toFixed(1)}M`

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-bold text-gray-800">{data.name}</h2>
      <p className="text-sm text-gray-500">{data.description}</p>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <KpiCard title="売上" value={fmtYen(data.revenue)} />
        <KpiCard title="原価" value={fmtYen(data.cost)} />
        <KpiCard title="粗利" value={fmtYen(data.grossProfit)} colorClass="text-emerald-600" />
        <KpiCard title="粗利率" value={`${data.grossMargin}%`} colorClass="text-emerald-600" />
        <KpiCard title="MAU" value={data.totalMau.toLocaleString()} />
      </div>

      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="font-semibold text-gray-700 mb-3">担当サービス</h3>
        <div className="divide-y">
          {data.services?.map((svc: { id: string; name: string; status: string; categoryName: string }) => (
            <Link
              key={svc.id}
              to={`/services/${svc.id}`}
              className="flex items-center justify-between py-3 hover:text-indigo-600"
            >
              <div>
                <span className="font-medium">{svc.name}</span>
                <span className="ml-2 text-xs text-gray-400">{svc.categoryName}</span>
              </div>
              <span className={`text-xs px-2 py-0.5 rounded-full ${
                svc.status === 'active' ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'
              }`}>
                {svc.status}
              </span>
            </Link>
          ))}
        </div>
      </div>
    </div>
  )
}
