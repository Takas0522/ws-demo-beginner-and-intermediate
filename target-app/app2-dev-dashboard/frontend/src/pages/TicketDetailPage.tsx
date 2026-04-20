import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getTicket, downloadCsv } from '../api/client'

export default function TicketDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading } = useQuery({
    queryKey: ['ticket', id],
    queryFn: () => getTicket(id!),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const statusColor = (s: string) =>
    s === 'done' ? 'bg-green-100 text-green-700'
    : s === 'in_progress' ? 'bg-blue-100 text-blue-700'
    : s === 'review' ? 'bg-purple-100 text-purple-700'
    : 'bg-gray-100 text-gray-500'

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <span className={`text-xs px-2 py-0.5 rounded-full ${statusColor(data.status)}`}>
              {data.status}
            </span>
            <span className="text-xs text-gray-400">{data.ticketType}</span>
          </div>
          <h2 className="text-xl font-bold text-gray-800">{data.title}</h2>
        </div>
        <button onClick={() => downloadCsv('tasks', 'tasks.csv')}
          className="text-sm bg-slate-600 text-white px-3 py-1.5 rounded hover:bg-slate-700">
          CSV エクスポート
        </button>
      </div>

      {/* 基本情報 */}
      <div className="bg-white rounded-lg shadow p-5 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
        <div>
          <p className="text-xs text-gray-400">プロジェクト</p>
          <Link to={`/projects/${data.project?.id}`} className="font-medium hover:underline text-slate-700">
            {data.project?.name}
          </Link>
        </div>
        <div>
          <p className="text-xs text-gray-400">スプリント</p>
          <p className="font-medium">{data.sprint?.name ?? '-'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">担当者</p>
          <p className="font-medium">{data.assignee?.name ?? '未割当'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">優先度</p>
          <p className={`font-medium ${
            data.priority === 'high' ? 'text-red-500' : data.priority === 'medium' ? 'text-amber-600' : 'text-gray-600'
          }`}>{data.priority}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">ストーリーポイント</p>
          <p className="font-medium">{data.storyPoints ?? '-'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">予定工数</p>
          <p className="font-medium">{data.estimatedHours ? `${data.estimatedHours} h` : '-'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">実績工数</p>
          <p className="font-medium">{data.actualHours ? `${data.actualHours} h` : '-'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">期日</p>
          <p className="font-medium">{data.dueDate ?? '-'}</p>
        </div>
      </div>

      {/* 説明 */}
      {data.description && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="font-semibold text-gray-700 mb-2">説明</h3>
          <p className="text-sm text-gray-600 whitespace-pre-wrap">{data.description}</p>
        </div>
      )}

      {/* 工数ログ */}
      {data.workLogs?.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="font-semibold text-gray-700 mb-3">工数ログ</h3>
          <table className="min-w-full text-xs">
            <thead>
              <tr className="text-gray-500">
                {['日付', 'メンバー', '工数', 'メモ'].map(h => (
                  <th key={h} className="text-left pb-1 pr-3">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data.workLogs.map((w: {
                id: string; logDate: string; member?: { name: string }; hours: number; description?: string
              }) => (
                <tr key={w.id}>
                  <td className="py-1 pr-3">{w.logDate}</td>
                  <td className="py-1 pr-3">{w.member?.name}</td>
                  <td className="py-1 pr-3">{w.hours} h</td>
                  <td className="py-1">{w.description ?? '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* 関連PR */}
      {data.pullRequests?.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="font-semibold text-gray-700 mb-3">関連PR</h3>
          <div className="space-y-1">
            {data.pullRequests.map((pr: {
              id: string; title: string; status: string; prNumber: number
            }) => (
              <Link key={pr.id} to={`/pull-requests/${pr.id}`}
                className="flex items-center gap-2 text-sm hover:text-slate-600">
                <span className="text-gray-400">#{pr.prNumber}</span>
                <span>{pr.title}</span>
                <span className="text-xs text-gray-400">{pr.status}</span>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
