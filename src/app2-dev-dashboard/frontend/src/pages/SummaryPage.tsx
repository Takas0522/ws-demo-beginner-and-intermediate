import { useQuery } from '@tanstack/react-query'
import { getSummary, exportUrl } from '../api/client'
import KpiCard from '../components/KpiCard'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ResponsiveContainer
} from 'recharts'

export default function SummaryPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['summary'],
    queryFn: getSummary,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtH = (n: number) => `${n.toLocaleString()} h`
  const fmtYen = (n: number) => `¥${(n / 1_000_000).toFixed(1)}M`

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-gray-800">開発状況サマリー</h2>
        <a
          href={exportUrl('project-summary')}
          className="text-sm bg-slate-600 text-white px-3 py-1.5 rounded hover:bg-slate-700"
        >
          CSV エクスポート
        </a>
      </div>

      {/* KPIカード */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        <KpiCard title="進行中プロジェクト" value={data.activeProjectCount} />
        <KpiCard title="全チケット数" value={data.totalTicketCount.toLocaleString()} />
        <KpiCard title="未着手/進行中" value={data.openTicketCount.toLocaleString()} colorClass="text-amber-600" />
        <KpiCard title="実績工数" value={fmtH(data.totalActualHours)} />
        <KpiCard title="開発費（実績）" value={fmtYen(data.totalActualCost)} />
        <KpiCard title="全体CPI" value={data.overallCpi?.toFixed(2)} colorClass={data.overallCpi >= 1 ? 'text-emerald-600' : 'text-red-500'} />
      </div>

      {/* 事業部別プロジェクト数 */}
      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="text-sm font-semibold text-gray-700 mb-4">事業部別 プロジェクト数</h3>
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data.businessUnitStats}>
            <XAxis dataKey="businessUnitName" tick={{ fontSize: 10 }} />
            <YAxis allowDecimals={false} tick={{ fontSize: 10 }} />
            <Tooltip />
            <Legend />
            <Bar dataKey="activeProjectCount" name="進行中" fill="#475569" stackId="a" radius={[0,0,0,0]} />
            <Bar dataKey="completedProjectCount" name="完了" fill="#94a3b8" stackId="a" radius={[4,4,0,0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* 事業部別コスト */}
      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="text-sm font-semibold text-gray-700 mb-4">事業部別 原価（実績 vs 計画）</h3>
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={data.businessUnitStats}>
            <XAxis dataKey="businessUnitName" tick={{ fontSize: 10 }} />
            <YAxis tickFormatter={v => `¥${(v/1e6).toFixed(1)}M`} tick={{ fontSize: 10 }} />
            <Tooltip formatter={(v: unknown) => fmtYen(Number(v))} />
            <Legend />
            <Bar dataKey="plannedCost" name="計画原価" fill="#cbd5e1" radius={[4,4,0,0]} />
            <Bar dataKey="actualCost" name="実績原価" fill="#475569" radius={[4,4,0,0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}
