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

  type Project = { id: string; name: string; status: string; totalTickets: number; completedTickets: number; budget: number }
  const projects: Project[] = data.projects ?? []
  const activeProjectCount = projects.filter((p) => p.status === 'active').length
  const totalTicketCount   = projects.reduce((s, p) => s + p.totalTickets, 0)
  const progressPercent    = (p: Project) =>
    p.totalTickets > 0 ? Math.round((p.completedTickets / p.totalTickets) * 100) : 0

  return (
    <div className="space-y-6">
      <h2 className="text-xl font-bold text-gray-800">{data.name}</h2>
      <p className="text-sm text-gray-500">{data.description}</p>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <KpiCard title="進行中プロジェクト" value={activeProjectCount} />
        <KpiCard title="総チケット数" value={totalTicketCount.toLocaleString()} />
        <KpiCard title="今月の実績工数" value={`${(data.monthlyHours ?? 0).toLocaleString()} h`} />
        <KpiCard title="今月の実績原価" value={fmtYen(data.monthlyCost ?? 0)} />
        <KpiCard title="総予算" value={fmtYen(data.totalBudget ?? 0)} />
      </div>

      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="font-semibold text-gray-700 mb-3">プロジェクト一覧</h3>
        <div className="divide-y">
          {projects.map((p) => (
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
                    style={{ width: `${progressPercent(p)}%` }}
                  />
                </div>
                <span className="text-sm text-gray-500">{progressPercent(p)}%</span>
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
