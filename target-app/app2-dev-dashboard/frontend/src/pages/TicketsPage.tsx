import { useQuery } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router-dom'
import { getProjects, getProjectTickets } from '../api/client'
import { useState } from 'react'

export default function TicketsPage() {
  const [searchParams] = useSearchParams()
  const defaultProject = searchParams.get('projectId') ?? ''
  const [projectId, setProjectId] = useState(defaultProject)
  const [status, setStatus] = useState<string>('')

  const { data: projects } = useQuery({
    queryKey: ['projects-list'],
    queryFn: () => getProjects({}),
  })

  const { data: tickets, isLoading } = useQuery({
    queryKey: ['tickets', projectId, status],
    queryFn: () => getProjectTickets(projectId, { status: status || undefined }),
    enabled: !!projectId,
  })

  const statusColor = (s: string) =>
    s === 'done' ? 'bg-green-100 text-green-700'
    : s === 'in_progress' ? 'bg-blue-100 text-blue-700'
    : s === 'review' ? 'bg-purple-100 text-purple-700'
    : 'bg-gray-100 text-gray-500'

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold text-gray-800">チケット一覧</h2>

      {/* フィルタ */}
      <div className="flex gap-3">
        <select value={projectId} onChange={e => setProjectId(e.target.value)}
          className="text-sm border rounded px-3 py-1.5">
          <option value="">プロジェクトを選択</option>
          {projects?.map((p: { id: string; name: string }) => (
            <option key={p.id} value={p.id}>{p.name}</option>
          ))}
        </select>
        <select value={status} onChange={e => setStatus(e.target.value)}
          className="text-sm border rounded px-3 py-1.5">
          <option value="">全ステータス</option>
          <option value="open">未着手</option>
          <option value="in_progress">進行中</option>
          <option value="review">レビュー待ち</option>
          <option value="done">完了</option>
        </select>
      </div>

      {!projectId && <p className="text-gray-400 text-sm">プロジェクトを選択してください</p>}
      {isLoading && <p className="text-gray-500">読み込み中...</p>}

      {tickets && (
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="min-w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                {['タイトル', '種別', '優先度', '担当者', 'SP', 'ステータス'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y">
              {tickets.map((t: {
                id: string; title: string; ticketType: string; priority: string; storyPoints?: number; status: string
                assignee?: { name: string }
              }) => (
                <tr key={t.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <Link to={`/tickets/${t.id}`} className="text-slate-700 hover:underline font-medium">
                      {t.title}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{t.ticketType}</td>
                  <td className={`px-4 py-3 font-medium text-xs ${
                    t.priority === 'high' ? 'text-red-500'
                    : t.priority === 'medium' ? 'text-amber-600'
                    : 'text-gray-400'
                  }`}>{t.priority}</td>
                  <td className="px-4 py-3 text-gray-600">{t.assignee?.name ?? '未割当'}</td>
                  <td className="px-4 py-3 text-gray-600">{t.storyPoints ?? '-'}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColor(t.status)}`}>
                      {t.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
