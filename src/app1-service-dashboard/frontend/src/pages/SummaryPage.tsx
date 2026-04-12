import { useQuery } from '@tanstack/react-query'
import { getSummary, exportUrl } from '../api/client'
import KpiCard from '../components/KpiCard'
import {
  BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, Tooltip, Legend, ResponsiveContainer
} from 'recharts'

const COLORS = ['#6366f1','#06b6d4','#10b981','#f59e0b','#ef4444']

export default function SummaryPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['summary'],
    queryFn: () => getSummary(),
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtYen = (n: number) =>
    n >= 1_000_000 ? `¥${(n / 1_000_000).toFixed(1)}M` : `¥${n.toLocaleString()}`

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-gray-800">全社 KPI サマリー</h2>
        <a
          href={exportUrl('kpi-summary')}
          className="text-sm bg-indigo-600 text-white px-3 py-1.5 rounded hover:bg-indigo-700"
        >
          CSV エクスポート
        </a>
      </div>

      {/* KPIカード */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        <KpiCard title="総売上" value={fmtYen(data.totalRevenue)} />
        <KpiCard title="総原価" value={fmtYen(data.totalCost)} />
        <KpiCard title="粗利" value={fmtYen(data.grossProfit)} colorClass="text-emerald-600" />
        <KpiCard title="粗利率" value={`${data.grossMargin}%`} colorClass="text-emerald-600" />
        <KpiCard title="MAU（合計）" value={data.totalMau.toLocaleString()} sub="最新月次" />
        <KpiCard title="提供中サービス数" value={data.activeServiceCount} />
      </div>

      {/* 事業部別売上 */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">事業部別売上</h3>
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={data.businessUnitRevenue}>
              <XAxis dataKey="businessUnitName" tick={{ fontSize: 10 }} />
              <YAxis tickFormatter={v => `¥${(v/1e6).toFixed(0)}M`} tick={{ fontSize: 10 }} />
              <Tooltip formatter={(v: unknown) => fmtYen(Number(v))} />
              <Bar dataKey="revenue" name="売上" fill="#6366f1" radius={[4,4,0,0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">事業部別売上比率</h3>
          <ResponsiveContainer width="100%" height={240}>
            <PieChart>
              <Pie
                data={data.businessUnitRevenue}
                dataKey="revenue"
                nameKey="businessUnitName"
                cx="50%"
                cy="50%"
                outerRadius={90}
                label={({ name, percent }: { name?: string; percent?: number }) =>
                  `${name ?? ''}: ${((percent ?? 0) * 100).toFixed(1)}%`
                }
              >
                {data.businessUnitRevenue.map((_: unknown, i: number) => (
                  <Cell key={i} fill={COLORS[i % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(v: unknown) => fmtYen(Number(v))} />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  )
}
