import { useQuery } from '@tanstack/react-query'
import { getSummary, downloadCsv } from '../api/client'
import KpiCard from '../components/KpiCard'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer, Cell
} from 'recharts'

const STATUS_COLORS: Record<string, string> = {
  active: '#475569', completed: '#94a3b8', planning: '#cbd5e1',
  open: '#3b82f6', in_progress: '#f59e0b', review: '#8b5cf6', done: '#22c55e',
}

export default function SummaryPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['summary'],
    queryFn: getSummary,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtH   = (n: number) => `${n.toLocaleString()} h`
  const fmtYen = (n: number) => `¥${(n / 1_000_000).toFixed(1)}M`

  const totalTickets = (data.ticketStats as {status:string;count:number}[])
    .reduce((s, t) => s + t.count, 0)
  const openTickets = (data.ticketStats as {status:string;count:number}[])
    .filter(t => t.status !== 'done')
    .reduce((s, t) => s + t.count, 0)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-gray-800">開発状況サマリー</h2>
        <button
          onClick={() => downloadCsv('project-summary', 'project-summary.csv')}
          className="text-sm bg-slate-600 text-white px-3 py-1.5 rounded hover:bg-slate-700"
        >
          CSV エクスポート
        </button>
      </div>

      {/* KPIカード */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        <KpiCard title="進行中プロジェクト" value={data.activeProjectCount} />
        <KpiCard title="全チケット数" value={totalTickets.toLocaleString()} />
        <KpiCard title="未完了チケット" value={openTickets.toLocaleString()} colorClass="text-amber-600" />
        <KpiCard title="今月の実績工数" value={fmtH(data.monthlyActualHours)} />
        <KpiCard title="累計開発費（実績）" value={fmtYen(data.totalActualCost)} />
        <KpiCard title="予算消化率" value={`${(data.budgetConsumptionRate ?? 0).toFixed(1)}%`}
          colorClass={data.budgetConsumptionRate <= 80 ? 'text-emerald-600' : 'text-red-500'} />
      </div>

      {/* プロジェクト ステータス分布 */}
      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="text-sm font-semibold text-gray-700 mb-4">プロジェクト ステータス分布</h3>
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data.projectsByStatus}>
            <XAxis dataKey="status" tick={{ fontSize: 11 }} />
            <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
            <Tooltip />
            <Bar dataKey="count" name="件数" radius={[4, 4, 0, 0]}>
              {(data.projectsByStatus as {status:string;count:number}[]).map(entry => (
                <Cell key={entry.status} fill={STATUS_COLORS[entry.status] ?? '#94a3b8'} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* チケット ステータス分布 */}
      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="text-sm font-semibold text-gray-700 mb-4">チケット ステータス分布</h3>
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data.ticketStats}>
            <XAxis dataKey="status" tick={{ fontSize: 11 }} />
            <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
            <Tooltip />
            <Legend />
            <Bar dataKey="count" name="チケット数" radius={[4, 4, 0, 0]}>
              {(data.ticketStats as {status:string;count:number}[]).map(entry => (
                <Cell key={entry.status} fill={STATUS_COLORS[entry.status] ?? '#94a3b8'} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}
