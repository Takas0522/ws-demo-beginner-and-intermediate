import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getServiceDetail, exportUrl } from '../api/client'
import KpiCard from '../components/KpiCard'
import {
  LineChart, Line, BarChart, Bar,
  XAxis, YAxis, Tooltip, Legend, ResponsiveContainer
} from 'recharts'

export default function ServiceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading } = useQuery({
    queryKey: ['service', id],
    queryFn: () => getServiceDetail(id!),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtYen = (n: number) => `¥${(n / 1_000_000).toFixed(1)}M`

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-800">{data.name}</h2>
          <p className="text-sm text-gray-500 mt-1">{data.description}</p>
        </div>
        <div className="flex gap-2">
          <a href={exportUrl('service-detail', { serviceIds: id })}
            className="text-sm bg-indigo-600 text-white px-3 py-1.5 rounded hover:bg-indigo-700">
            CSV エクスポート
          </a>
        </div>
      </div>

      {/* KPIカード */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <KpiCard title="売上合計" value={fmtYen(data.totalRevenue)} />
        <KpiCard title="原価合計" value={fmtYen(data.totalCost)} />
        <KpiCard title="粗利" value={fmtYen(data.grossProfit)} colorClass="text-emerald-600" />
        <KpiCard title="粗利率" value={`${data.grossMargin}%`} colorClass="text-emerald-600" />
        <KpiCard title="ARPU" value={`¥${data.arpu?.toLocaleString() ?? '-'}`} />
        <KpiCard title="プラン数" value={data.plans?.length ?? 0} />
      </div>

      {/* ユーザー数推移 */}
      {data.userMetrics?.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">MAU / DAU 推移</h3>
          <ResponsiveContainer width="100%" height={200}>
            <LineChart data={data.userMetrics}>
              <XAxis dataKey="date" tick={{ fontSize: 10 }} />
              <YAxis tick={{ fontSize: 10 }} />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="mau" name="MAU" stroke="#6366f1" dot={false} />
              <Line type="monotone" dataKey="dau" name="DAU" stroke="#06b6d4" dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* 原価内訳 */}
      {data.costBreakdown?.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">原価内訳</h3>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={data.costBreakdown}>
              <XAxis dataKey="costType" tick={{ fontSize: 10 }} />
              <YAxis tickFormatter={v => `¥${(v/1e6).toFixed(1)}M`} tick={{ fontSize: 10 }} />
              <Tooltip formatter={(v: unknown) => fmtYen(Number(v))} />
              <Bar dataKey="amount" name="原価" fill="#f59e0b" radius={[4,4,0,0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  )
}
