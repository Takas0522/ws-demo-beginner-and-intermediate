import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getBusinessUnits } from '../api/client'

export default function BusinessUnitsPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['business-units'],
    queryFn: getBusinessUnits,
  })

  if (isLoading) return <p className="text-gray-500">読み込み中...</p>

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-bold text-gray-800">事業部一覧</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {data?.map((bu: { id: string; name: string; description: string }) => (
          <Link
            key={bu.id}
            to={`/business-units/${bu.id}`}
            className="bg-white rounded-lg shadow p-5 hover:shadow-md transition-shadow"
          >
            <h3 className="font-semibold text-slate-700">{bu.name}</h3>
            <p className="text-sm text-gray-500 mt-1">{bu.description}</p>
          </Link>
        ))}
      </div>
    </div>
  )
}
