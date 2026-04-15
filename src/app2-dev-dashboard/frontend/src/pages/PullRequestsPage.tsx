import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getProjects, getProjectPullRequests } from '../api/client'
import { useState } from 'react'

export default function PullRequestsPage() {
  const [projectId, setProjectId] = useState<string>('')
  const [status, setStatus] = useState<string>('')

  const { data: projects } = useQuery({
    queryKey: ['projects-list'],
    queryFn: () => getProjects({}),
  })

  const { data: prs, isLoading } = useQuery({
    queryKey: ['prs', projectId, status],
    queryFn: () => getProjectPullRequests(projectId, status || undefined),
    enabled: !!projectId,
  })

  const statusColor = (s: string) =>
    s === 'merged' ? 'bg-purple-100 text-purple-700'
    : s === 'open' ? 'bg-green-100 text-green-700'
    : s === 'closed' ? 'bg-red-100 text-red-600'
    : 'bg-gray-100 text-gray-500'

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold text-gray-800">プルリクエスト一覧</h2>

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
          <option value="open">オープン</option>
          <option value="merged">マージ済み</option>
          <option value="closed">クローズ</option>
        </select>
      </div>

      {!projectId && <p className="text-gray-400 text-sm">プロジェクトを選択してください</p>}
      {isLoading && <p className="text-gray-500">読み込み中...</p>}

      {prs && (
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="min-w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                {['#', 'タイトル', '担当者', 'ブランチ', 'ステータス', 'レビュー数', '作成日'].map(h => (
                  <th key={h} className="px-4 py-3 text-left text-xs font-medium text-gray-500">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y">
              {prs.map((pr: {
                id: string; prNumber: number; title: string; status: string
                sourceBranch: string; targetBranch: string; createdAt: string; reviewCount: number
                author?: { name: string }
              }) => (
                <tr key={pr.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-gray-400">#{pr.prNumber}</td>
                  <td className="px-4 py-3">
                    <Link to={`/pull-requests/${pr.id}`} className="text-slate-700 hover:underline font-medium">
                      {pr.title}
                    </Link>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{pr.author?.name ?? '-'}</td>
                  <td className="px-4 py-3 text-xs text-gray-500 font-mono">
                    {pr.sourceBranch} → {pr.targetBranch}
                  </td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusColor(pr.status)}`}>
                      {pr.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-gray-600">{pr.reviewCount}</td>
                  <td className="px-4 py-3 text-gray-500">{pr.createdAt}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
