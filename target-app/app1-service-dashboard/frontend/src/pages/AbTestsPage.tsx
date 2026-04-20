import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getServices, getAbTests } from '../api/client'
import { useState } from 'react'

export default function AbTestsPage() {
  const [serviceId, setServiceId] = useState<string>('')
  const [status, setStatus] = useState<string>('')

  const { data: services } = useQuery({ queryKey: ['services'], queryFn: () => getServices({}) })
  const { data: tests, isLoading } = useQuery({
    queryKey: ['ab-tests', serviceId, status],
    queryFn: () => getAbTests(serviceId, status || undefined),
    enabled: !!serviceId,
  })

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold text-gray-800">ABテスト一覧</h2>

      {/* フィルタ */}
      <div className="flex gap-3">
        <select
          value={serviceId}
          onChange={e => setServiceId(e.target.value)}
          className="text-sm border rounded px-3 py-1.5"
        >
          <option value="">サービスを選択</option>
          {services?.map((s: { id: string; name: string }) => (
            <option key={s.id} value={s.id}>{s.name}</option>
          ))}
        </select>
        <select
          value={status}
          onChange={e => setStatus(e.target.value)}
          className="text-sm border rounded px-3 py-1.5"
        >
          <option value="">全ステータス</option>
          <option value="running">実施中</option>
          <option value="completed">完了</option>
          <option value="stopped">停止</option>
        </select>
      </div>

      {!serviceId && <p className="text-gray-400 text-sm">サービスを選択してください</p>}
      {isLoading && <p className="text-gray-500">読み込み中...</p>}

      {tests?.length === 0 && serviceId && (
        <p className="text-gray-400 text-sm">該当するABテストはありません</p>
      )}

      <div className="space-y-3">
        {tests?.map((t: {
          id: string; name: string; primaryMetric: string
          status: string; startedAt: string; endedAt?: string; variantCount: number
        }) => (
          <Link
            key={t.id}
            to={`/ab-tests/${t.id}`}
            className="block bg-white rounded-lg shadow p-4 hover:shadow-md transition-shadow"
          >
            <div className="flex items-center justify-between">
              <h3 className="font-medium text-gray-800">{t.name}</h3>
              <span className={`text-xs px-2 py-0.5 rounded-full ${
                t.status === 'running'
                  ? 'bg-blue-100 text-blue-700'
                  : t.status === 'completed'
                    ? 'bg-green-100 text-green-700'
                    : 'bg-gray-100 text-gray-500'
              }`}>
                {t.status}
              </span>
            </div>
            <p className="text-xs text-gray-500 mt-1">主要指標: {t.primaryMetric} | バリアント数: {t.variantCount}</p>
            <p className="text-xs text-gray-400 mt-0.5">{t.startedAt} 〜 {t.endedAt ?? '進行中'}</p>
          </Link>
        ))}
      </div>
    </div>
  )
}
