import { useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import axios from 'axios'
import { getServiceDetail, getStakeholders, addStakeholder, updateStakeholder, deleteStakeholder, downloadCsv } from '../api/client'
import { getToken } from '../api/authClient'
import KpiCard from '../components/KpiCard'
import {
  LineChart, Line, BarChart, Bar,
  XAxis, YAxis, Tooltip, Legend, ResponsiveContainer
} from 'recharts'

const AUTH_BASE = import.meta.env.VITE_AUTH_API_URL ?? 'http://localhost:5000'

type AuthUser = {
  id: string
  username: string
  displayName: string
  role: string
  departmentId: string | null
  departmentName: string | null
}

type Stakeholder = {
  id: string
  authUserId: string
  displayName: string
  role: string
  hourlyRate: number
  allocatedHoursMonthly: number
  monthlyCost: number
}

const ROLE_LABELS: Record<string, string> = {
  developer: '開発者',
  operator:  '運用者',
  pm:        'PM',
  pl:        'PL',
  tl:        'テックリード',
}

export default function ServiceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const qc = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['service', id],
    queryFn: () => getServiceDetail(id!),
    enabled: !!id,
  })

  const { data: stData, isLoading: stLoading } = useQuery({
    queryKey: ['stakeholders', id],
    queryFn: () => getStakeholders(id!),
    enabled: !!id,
  })

  const { data: authUsers } = useQuery<AuthUser[]>({
    queryKey: ['authUsers'],
    queryFn: async () => {
      const res = await axios.get(`${AUTH_BASE}/api/users`, {
        headers: { Authorization: `Bearer ${getToken()}` }
      })
      return res.data
    },
  })

  // フォーム状態
  const [form, setForm] = useState<{
    authUserId: string; role: string; hourlyRate: string; allocatedHours: string
  }>({ authUserId: '', role: 'developer', hourlyRate: '6000', allocatedHours: '80' })
  const [editId, setEditId] = useState<string | null>(null)

  const addMut = useMutation({
    mutationFn: () => {
      const user = authUsers?.find(u => u.id === form.authUserId)
      return addStakeholder(id!, {
        authUserId: form.authUserId,
        displayName: user?.displayName ?? form.authUserId,
        role: form.role,
        hourlyRate: Number(form.hourlyRate),
        allocatedHoursMonthly: Number(form.allocatedHours),
      })
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stakeholders', id] })
      setForm({ authUserId: '', role: 'developer', hourlyRate: '6000', allocatedHours: '80' })
    },
  })

  const updateMut = useMutation({
    mutationFn: (sid: string) => {
      const user = authUsers?.find(u => u.id === form.authUserId)
      return updateStakeholder(id!, sid, {
        authUserId: form.authUserId,
        displayName: user?.displayName ?? form.authUserId,
        role: form.role,
        hourlyRate: Number(form.hourlyRate),
        allocatedHoursMonthly: Number(form.allocatedHours),
      })
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['stakeholders', id] })
      setEditId(null)
      setForm({ authUserId: '', role: 'developer', hourlyRate: '6000', allocatedHours: '80' })
    },
  })

  const deleteMut = useMutation({
    mutationFn: (sid: string) => deleteStakeholder(id!, sid),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['stakeholders', id] }),
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>
  if (!data) return null

  const fmtYen = (n: number) => `¥${(n / 1_000_000).toFixed(1)}M`
  const fmtYenK = (n: number) => n >= 1_000_000 ? fmtYen(n) : `¥${n.toLocaleString()}`

  const stakeholders: Stakeholder[] = stData?.stakeholders ?? []
  const totalMonthlyCost: number = stData?.totalMonthlyCost ?? 0

  const handleEdit = (s: Stakeholder) => {
    setEditId(s.id)
    setForm({
      authUserId: s.authUserId,
      role: s.role,
      hourlyRate: String(s.hourlyRate),
      allocatedHours: String(s.allocatedHoursMonthly),
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-800">{data.name}</h2>
          <p className="text-sm text-gray-500 mt-1">{data.description}</p>
        </div>
        <div className="flex gap-2">
          <button onClick={() => downloadCsv('user-metrics', 'user-metrics.csv', { serviceIds: id })}
            className="text-sm bg-indigo-600 text-white px-3 py-1.5 rounded hover:bg-indigo-700">
            KPI CSV
          </button>
          <button onClick={() => downloadCsv('stakeholders', 'stakeholders.csv', { serviceIds: id })}
            className="text-sm bg-purple-600 text-white px-3 py-1.5 rounded hover:bg-purple-700">
            関係者 CSV
          </button>
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

      {/* ── 関係者管理 ─────────────────────────────────────────── */}
      <div className="bg-white rounded-lg shadow p-5 space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-gray-700">関係者管理</h3>
          <span className="text-xs text-gray-500">
            月次人件費合計: <span className="font-bold text-indigo-600">{fmtYenK(totalMonthlyCost)}</span>
          </span>
        </div>

        {stLoading ? (
          <p className="text-xs text-gray-400">読み込み中...</p>
        ) : stakeholders.length === 0 ? (
          <p className="text-xs text-gray-400">関係者が登録されていません。</p>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs text-gray-500 border-b">
                <th className="pb-1">名前</th>
                <th className="pb-1">役割</th>
                <th className="pb-1 text-right">原単価/h</th>
                <th className="pb-1 text-right">月稼働h</th>
                <th className="pb-1 text-right">月次コスト</th>
                <th className="pb-1"></th>
              </tr>
            </thead>
            <tbody>
              {stakeholders.map(s => (
                <tr key={s.id} className="border-b last:border-0">
                  <td className="py-1.5">{s.displayName}</td>
                  <td className="py-1.5">
                    <span className="px-1.5 py-0.5 rounded text-xs bg-indigo-100 text-indigo-700">
                      {ROLE_LABELS[s.role] ?? s.role}
                    </span>
                  </td>
                  <td className="py-1.5 text-right">¥{s.hourlyRate.toLocaleString()}</td>
                  <td className="py-1.5 text-right">{s.allocatedHoursMonthly}h</td>
                  <td className="py-1.5 text-right font-medium">¥{s.monthlyCost.toLocaleString()}</td>
                  <td className="py-1.5 text-right space-x-1">
                    <button onClick={() => handleEdit(s)}
                      className="text-xs text-indigo-600 hover:underline">編集</button>
                    <button onClick={() => deleteMut.mutate(s.id)}
                      className="text-xs text-red-500 hover:underline">削除</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {/* 追加 / 編集フォーム */}
        <div className="border-t pt-4">
          <p className="text-xs font-medium text-gray-600 mb-2">
            {editId ? '関係者を編集' : '関係者を追加'}
          </p>
          <div className="grid grid-cols-2 md:grid-cols-5 gap-2">
            <select
              value={form.authUserId}
              onChange={e => setForm(f => ({ ...f, authUserId: e.target.value }))}
              className="col-span-2 md:col-span-1 border rounded px-2 py-1 text-sm"
            >
              <option value="">ユーザーを選択</option>
              {(authUsers ?? []).map(u => (
                <option key={u.id} value={u.id}>{u.displayName}</option>
              ))}
            </select>
            <select
              value={form.role}
              onChange={e => setForm(f => ({ ...f, role: e.target.value }))}
              className="border rounded px-2 py-1 text-sm"
            >
              {Object.entries(ROLE_LABELS).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </select>
            <input
              type="number" placeholder="原単価(/h)" value={form.hourlyRate}
              onChange={e => setForm(f => ({ ...f, hourlyRate: e.target.value }))}
              className="border rounded px-2 py-1 text-sm"
            />
            <input
              type="number" placeholder="月稼働時間" value={form.allocatedHours}
              onChange={e => setForm(f => ({ ...f, allocatedHours: e.target.value }))}
              className="border rounded px-2 py-1 text-sm"
            />
            <div className="flex gap-1">
              <button
                onClick={() => editId ? updateMut.mutate(editId) : addMut.mutate()}
                disabled={!form.authUserId}
                className="flex-1 bg-indigo-600 text-white rounded px-3 py-1 text-sm hover:bg-indigo-700 disabled:opacity-40"
              >
                {editId ? '更新' : '追加'}
              </button>
              {editId && (
                <button
                  onClick={() => { setEditId(null); setForm({ authUserId: '', role: 'developer', hourlyRate: '6000', allocatedHours: '80' }) }}
                  className="flex-1 bg-gray-200 text-gray-700 rounded px-3 py-1 text-sm hover:bg-gray-300"
                >
                  キャンセル
                </button>
              )}
            </div>
          </div>
          {form.authUserId && form.hourlyRate && form.allocatedHours && (
            <p className="mt-1 text-xs text-gray-500">
              月次コスト: ¥{(Number(form.hourlyRate) * Number(form.allocatedHours)).toLocaleString()}
            </p>
          )}
        </div>
      </div>
    </div>
  )
}
