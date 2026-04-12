import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getPullRequest, exportUrl } from '../api/client'

export default function PullRequestDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading } = useQuery({
    queryKey: ['pr', id],
    queryFn: () => getPullRequest(id!),
    enabled: !!id,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const statusColor = (s: string) =>
    s === 'merged' ? 'bg-purple-100 text-purple-700'
    : s === 'open' ? 'bg-green-100 text-green-700'
    : s === 'closed' ? 'bg-red-100 text-red-600'
    : 'bg-gray-100 text-gray-500'

  const reviewStatusColor = (s: string) =>
    s === 'approved' ? 'text-green-600'
    : s === 'changes_requested' ? 'text-red-500'
    : 'text-gray-400'

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <span className="text-sm text-gray-400">#{data.prNumber}</span>
            <span className={`text-xs px-2 py-0.5 rounded-full ${statusColor(data.status)}`}>
              {data.status}
            </span>
          </div>
          <h2 className="text-xl font-bold text-gray-800">{data.title}</h2>
        </div>
        <a href={exportUrl('pull-requests')}
          className="text-sm bg-slate-600 text-white px-3 py-1.5 rounded hover:bg-slate-700">
          CSV エクスポート
        </a>
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
          <p className="text-xs text-gray-400">作成者</p>
          <p className="font-medium">{data.author?.name ?? '-'}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">ブランチ</p>
          <p className="font-mono text-xs">{data.sourceBranch} → {data.targetBranch}</p>
        </div>
        <div>
          <p className="text-xs text-gray-400">作成日</p>
          <p className="font-medium">{data.createdAt}</p>
        </div>
        {data.mergedAt && (
          <div>
            <p className="text-xs text-gray-400">マージ日</p>
            <p className="font-medium">{data.mergedAt}</p>
          </div>
        )}
      </div>

      {/* 説明 */}
      {data.description && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="font-semibold text-gray-700 mb-2">説明</h3>
          <p className="text-sm text-gray-600 whitespace-pre-wrap">{data.description}</p>
        </div>
      )}

      {/* レビュー */}
      {data.reviews?.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="font-semibold text-gray-700 mb-3">レビュー</h3>
          <div className="space-y-3">
            {data.reviews.map((r: {
              id: string; reviewStatus: string; comment?: string; reviewedAt: string
              reviewer?: { name: string }
            }) => (
              <div key={r.id} className="flex items-start gap-3 text-sm">
                <div className="w-8 h-8 rounded-full bg-slate-200 flex items-center justify-center text-xs font-bold text-slate-600 shrink-0">
                  {r.reviewer?.name?.[0] ?? '?'}
                </div>
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">{r.reviewer?.name}</span>
                    <span className={`text-xs font-semibold ${reviewStatusColor(r.reviewStatus)}`}>
                      {r.reviewStatus}
                    </span>
                    <span className="text-xs text-gray-400">{r.reviewedAt}</span>
                  </div>
                  {r.comment && <p className="text-gray-600 mt-1">{r.comment}</p>}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* 関連チケット */}
      {data.linkedTickets?.length > 0 && (
        <div className="bg-white rounded-lg shadow p-5">
          <h3 className="font-semibold text-gray-700 mb-3">関連チケット</h3>
          <div className="space-y-1">
            {data.linkedTickets.map((t: {
              id: string; title: string; status: string
            }) => (
              <Link key={t.id} to={`/tickets/${t.id}`}
                className="flex items-center gap-2 text-sm hover:text-slate-600">
                <span>{t.title}</span>
                <span className="text-xs text-gray-400">{t.status}</span>
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
