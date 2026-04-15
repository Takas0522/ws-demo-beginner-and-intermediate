interface Props {
  title: string
  value: string | number
  sub?: string
  colorClass?: string
}

export default function KpiCard({ title, value, sub, colorClass = 'text-gray-900' }: Props) {
  return (
    <div className="bg-white rounded-lg shadow p-5">
      <p className="text-xs text-gray-500 mb-1">{title}</p>
      <p className={`text-2xl font-bold ${colorClass}`}>{value}</p>
      {sub && <p className="text-xs text-gray-400 mt-1">{sub}</p>}
    </div>
  )
}
