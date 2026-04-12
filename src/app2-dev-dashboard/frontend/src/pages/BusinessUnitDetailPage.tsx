import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
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
        <KpiCard title="進行中プロジェクト" value={data.activeProjectCount} />
        <KpiCard title="総チケット数" value={data.totalTicketCount.toLocaleString()} />
        <KpiCard title="実績工数" value={`${data.totalActualHours?.toLocaleString()} h`} />
        <KpiCard title="実績原価" value={fmtYen(data.totalActualCost)} />
        <KpiCard title="CPI" value={data.overallCpi?.toFixed(2)}
          colorClass={data.overallCpi >= 1 ? 'text-emerald-600' : 'text-red-500'} />
        <KpiCard title="SPI" value={data.overallSpi?.toFixed(2)}
          colorClass={data.overallSpi >= 1 ? 'text-emerald-600' : 'text-amber-500'} />
      </div>

      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="font-semibold text-gray-700 mb-3">プロジェクト一覧</h3>
        <div className="divide-y">
          {data.projects?.map((p: {
            id: string; name: string; status: string; progressPercent: number
          }) => (
            <Link
              key={p.id}
              to={`/projects/${p.id}`}
              className="flex items-center justify-between py-3 hover:text-slate-600"
            >
              <span className="font-medium">{p.name}</span>
              <div className="flex items-center gap-3">
                <div className="w-32 bg-gray-100 rounded-full h-1.5">
                  <div
                    className="bg-slate-500 h-1.5 rounded-full"
                    style={{ width: `${p.progressPercent}%` }}
                  />
                </div>
                <span className="text-sm text-gray-500">{p.progressPercent}%</span>
                <span className={`text-xs px-2 py-0.5 rounded-full ${
                  p.status === 'active'
                    ? 'bg-blue-100 text-blue-700'
                    : p.status === 'completed'
                      ? 'bg-green-100 text-green-700'
                      : 'bg-gray-100 text-gray-500'
                }`}>
                  {p.status}
                </span>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  )
}
