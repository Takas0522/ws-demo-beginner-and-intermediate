import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getProjectDetail, getProjectSprints, getProjectTickets, downloadCsv } from '../api/client'
import KpiCard from '../components/KpiCard'
import {
  LineChart, Line, BarChart, Bar,
  XAxis, YAxis, Tooltip, Legend, ResponsiveContainer
} from 'recharts'

export default function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>()

  const { data, isLoading } = useQuery({
    queryKey: ['project', id],
    queryFn: () => getProjectDetail(id!),
    enabled: !!id,
  })
  const { data: sprints } = useQuery({
    queryKey: ['project-sprints', id],
    queryFn: () => getProjectSprints(id!),
    enabled: !!id,
  })
  const { data: tickets } = useQuery({
    queryKey: ['project-tickets', id],
    queryFn: () => getProjectTickets(id!),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtYen = (n: number) => `¥${(n / 1_000_000).toFixed(1)}M`
  const evm = data.evm

  // EVM折れ線グラフ用データ（月次サマリーがあれば使う）
  const evmChartData = data.monthlyEvm ?? []

  // バーンダウン（最新スプリント）
  const latestSprint = sprints?.[0]
  const burndownData = latestSprint?.dailyMetrics ?? []

  // ベロシティ（スプリント別）
  const velocityData = sprints?.map((s: {
    name: string; completedPoints: number; plannedPoints: number
  }) => ({
    name: s.name,
    completed: s.completedPoints,
    planned: s.plannedPoints,
  })) ?? []

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-800">{data.name}</h2>
          <p className="text-sm text-gray-500 mt-1">{data.description}</p>
        </div>
        <button onClick={() => downloadCsv('project-summary', 'project-summary.csv')}
          className="text-sm bg-slate-600 text-white px-3 py-1.5 rounded hover:bg-slate-700">
          CSV エクスポート
        </button>
      </div>

      {/* EVM KPIカード */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <KpiCard title="PV（計画価値）" value={fmtYen(evm?.pv ?? 0)} />
        <KpiCard title="EV（出来高）" value={fmtYen(evm?.ev ?? 0)} />
        <KpiCard title="AC（実績原価）" value={fmtYen(evm?.ac ?? 0)} />
        <KpiCard title="BAC（予算）" value={fmtYen(data.budget ?? 0)} />
        <KpiCard title="CPI" value={evm?.cpi?.toFixed(2) ?? '-'}
          colorClass={(evm?.cpi ?? 0) >= 1 ? 'text-emerald-600' : 'text-red-500'} />
        <KpiCard title="SPI" value={evm?.spi?.toFixed(2) ?? '-'}
          colorClass={(evm?.spi ?? 0) >= 1 ? 'text-emerald-600' : 'text-amber-500'} />
        <KpiCard title="EAC（完了時予測）" value={fmtYen(evm?.eac ?? 0)} />
        <KpiCard title="VAC（予算差額）" value={fmtYen(evm?.vac ?? 0)}
          colorClass={(evm?.vac ?? 0) >= 0 ? 'text-emerald-600' : 'text-red-500'} />
      </div>

      {/* EVM チャート */}
      {evmChartData.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">EVM推移（PV / EV / AC）</h3>
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={evmChartData}>
              <XAxis dataKey="month" tick={{ fontSize: 10 }} />
              <YAxis tickFormatter={v => `¥${(v/1e6).toFixed(0)}M`} tick={{ fontSize: 10 }} />
              <Tooltip formatter={(v: unknown) => fmtYen(Number(v))} />
              <Legend />
              <Line type="monotone" dataKey="pv" name="PV" stroke="#94a3b8" dot={false} />
              <Line type="monotone" dataKey="ev" name="EV" stroke="#475569" dot={false} />
              <Line type="monotone" dataKey="ac" name="AC" stroke="#ef4444" dot={false} strokeDasharray="4 2" />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* バーンダウン */}
      {burndownData.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-2">
            バーンダウン — {latestSprint?.name}
          </h3>
          <ResponsiveContainer width="100%" height={200}>
            <LineChart data={burndownData}>
              <XAxis dataKey="date" tick={{ fontSize: 10 }} />
              <YAxis tick={{ fontSize: 10 }} />
              <Tooltip />
              <Legend />
              <Line type="monotone" dataKey="remainingPoints" name="残SP" stroke="#475569" dot={false} />
              <Line type="monotone" dataKey="idealPoints" name="理想" stroke="#94a3b8" strokeDasharray="4 2" dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* ベロシティ */}
      {velocityData.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="text-sm font-semibold text-gray-700 mb-4">スプリントベロシティ</h3>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={velocityData}>
              <XAxis dataKey="name" tick={{ fontSize: 10 }} />
              <YAxis allowDecimals={false} tick={{ fontSize: 10 }} />
              <Tooltip />
              <Legend />
              <Bar dataKey="planned" name="計画SP" fill="#cbd5e1" radius={[4,4,0,0]} />
              <Bar dataKey="completed" name="完了SP" fill="#475569" radius={[4,4,0,0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* チケットサマリー */}
      <div className="bg-white rounded-lg shadow p-5">
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-semibold text-gray-700">チケット</h3>
          <Link to={`/tickets?projectId=${id}`} className="text-sm text-slate-600 hover:underline">
            全て見る →
          </Link>
        </div>
        <div className="divide-y max-h-60 overflow-y-auto">
          {tickets?.slice(0, 10).map((t: {
            id: string; title: string; status: string; priority: string; assignee?: { name: string }
          }) => (
            <Link key={t.id} to={`/tickets/${t.id}`}
              className="flex items-center justify-between py-2 hover:text-slate-600">
              <span className="text-sm">{t.title}</span>
              <div className="flex items-center gap-2">
                <span className="text-xs text-gray-400">{t.assignee?.name ?? '未割当'}</span>
                <span className={`text-xs px-2 py-0.5 rounded-full ${
                  t.status === 'done' ? 'bg-green-100 text-green-700'
                  : t.status === 'in_progress' ? 'bg-blue-100 text-blue-700'
                  : 'bg-gray-100 text-gray-500'
                }`}>{t.status}</span>
                <span className={`text-xs ${t.priority === 'high' ? 'text-red-500' : 'text-gray-400'}`}>
                  {t.priority}
                </span>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  )
}
