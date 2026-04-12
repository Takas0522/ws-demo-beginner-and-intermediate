import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getAbTestDetail, exportUrl } from '../api/client'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, Legend, ReferenceLine, ResponsiveContainer
} from 'recharts'

export default function AbTestDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading } = useQuery({
    queryKey: ['ab-test', id],
    queryFn: () => getAbTestDetail(id!),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  // バリアント比較用データ（プライマリ指標のみ）
  const chartData = data.variants?.map((v: {
    id: string; name: string; trafficAllocation: number
    results: { metricName: string; metricValue: number; isStatisticallySignificant: boolean }[]
  }) => {
    const primary = v.results.find(
      (r: { metricName: string }) => r.metricName === data.primaryMetric
    )
    return {
      name: v.name,
      value: primary ? parseFloat((primary.metricValue * 100).toFixed(4)) : 0,
      significant: primary?.isStatisticallySignificant ?? false,
    }
  })

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-800">{data.name}</h2>
          <p className="text-sm text-gray-500 mt-1">{data.description}</p>
        </div>
        <a href={exportUrl('ab-tests')} className="text-sm bg-indigo-600 text-white px-3 py-1.5 rounded hover:bg-indigo-700">
          CSV エクスポート
        </a>
      </div>

      {/* メタ情報 */}
      <div className="bg-white rounded-lg shadow p-5 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
        <div>
          <p className="text-gray-400 text-xs">サービス</p>
          <p className="font-medium">{data.service?.name}</p>
        </div>
        <div>
          <p className="text-gray-400 text-xs">主要指標</p>
          <p className="font-medium">{data.primaryMetric}</p>
        </div>
        <div>
          <p className="text-gray-400 text-xs">期間</p>
          <p className="font-medium">{data.startedAt} 〜 {data.endedAt ?? '進行中'}</p>
        </div>
        <div>
          <p className="text-gray-400 text-xs">ステータス</p>
          <p className="font-medium">{data.status}</p>
        </div>
      </div>

      {/* バリアント比較グラフ */}
      <div className="bg-white rounded-lg shadow p-5">
        <h3 className="text-sm font-semibold text-gray-700 mb-4">バリアント比較（{data.primaryMetric}）</h3>
        <ResponsiveContainer width="100%" height={220}>
          <BarChart data={chartData}>
            <XAxis dataKey="name" tick={{ fontSize: 11 }} />
            <YAxis tickFormatter={v => `${v}%`} tick={{ fontSize: 10 }} />
            <Tooltip formatter={(v: unknown) => [`${(Number(v) * 100).toFixed(4)}%`, data.primaryMetric]} />
            <Legend />
            <ReferenceLine y={chartData?.[0]?.value} stroke="#94a3b8" strokeDasharray="4 2" label={{ value: 'Control', fontSize: 10 }} />
            <Bar dataKey="value" name={data.primaryMetric} fill="#6366f1" radius={[4,4,0,0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* バリアント詳細テーブル */}
      {data.variants?.map((v: {
        id: string; name: string; trafficAllocation: number
        results: { metricName: string; sampleSize: number; metricValue: number
          pValue?: number; confidenceIntervalLower?: number; confidenceIntervalUpper?: number
          isStatisticallySignificant: boolean }[]
      }) => (
        <div key={v.id} className="bg-white rounded-lg shadow p-5">
          <div className="flex items-center gap-2 mb-3">
            <h4 className="font-semibold text-gray-800">{v.name}</h4>
            {data.winnerVariantId === v.id && (
              <span className="text-xs bg-yellow-100 text-yellow-700 px-2 py-0.5 rounded-full">🏆 勝者</span>
            )}
            <span className="text-xs text-gray-400">割当: {v.trafficAllocation}%</span>
          </div>
          <table className="min-w-full text-xs">
            <thead>
              <tr className="text-gray-500">
                {['指標名', 'N', '指標値', 'p値', '信頼区間', '有意差'].map(h => (
                  <th key={h} className="text-left pb-1 pr-3">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {v.results?.map((r, i) => (
                <tr key={i}>
                  <td className="py-1 pr-3 font-medium">{r.metricName}</td>
                  <td className="py-1 pr-3">{r.sampleSize.toLocaleString()}</td>
                  <td className="py-1 pr-3">{(r.metricValue * 100).toFixed(2)}%</td>
                  <td className="py-1 pr-3">{r.pValue?.toFixed(5) ?? '-'}</td>
                  <td className="py-1 pr-3">
                    {r.confidenceIntervalLower != null
                      ? `[${(r.confidenceIntervalLower*100).toFixed(2)}%, ${(r.confidenceIntervalUpper!*100).toFixed(2)}%]`
                      : '-'}
                  </td>
                  <td className="py-1">
                    {r.isStatisticallySignificant
                      ? <span className="text-green-600 font-semibold">✓ 有意</span>
                      : <span className="text-gray-400">-</span>
                    }
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ))}
    </div>
  )
}
