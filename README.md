# サービス & 開発状況ダッシュボード

2つのダッシュボードアプリを含むモノレポです。

## アプリケーション構成

| | App1: サービスダッシュボード | App2: 開発状況ダッシュボード |
|---|---|---|
| **概要** | 販売サービスのKPI管理 | チケット・PR・開発進捗管理 |
| **Frontend** | http://localhost:3001 | http://localhost:3002 |
| **Backend API** | http://localhost:5001 | http://localhost:5002 |
| **Swagger UI** | http://localhost:5001/swagger | http://localhost:5002/swagger |
| **PostgreSQL** | localhost:5433 | localhost:5434 |

## ディレクトリ構成

```
src/
├── app1-service-dashboard/
│   ├── frontend/          # React + Vite + Tailwind CSS v4
│   ├── backend/App1Backend/  # ASP.NET Core 10 WebAPI
│   └── database/          # PostgreSQL (docker-compose + schema/seed SQL)
├── app2-dev-dashboard/
│   ├── frontend/          # React + Vite + Tailwind CSS v4
│   ├── backend/App2Backend/  # ASP.NET Core 10 WebAPI
│   └── database/          # PostgreSQL (docker-compose + schema/seed SQL)
└── aspire-host/           # .NET Aspire AppHost (Aspireワークロード利用時)
docs/                      # 機能要件・データモデル仕様
scripts/
└── start-all.sh           # 全アプリ一括起動スクリプト
```

## クイックスタート

### 前提条件

- Docker / Docker Compose
- .NET 10 SDK
- Node.js 20+

### 全アプリ一括起動

```bash
./scripts/start-all.sh
```

起動順序: PostgreSQL → バックエンド → フロントエンド

Ctrl+C で全プロセスを停止します。

### 個別起動

#### App1 データベース

```bash
cd src/app1-service-dashboard/database
docker compose up -d
# 初回のみ: スキーマ・シードデータ投入
docker exec -i app1-postgres psql -U app1user -d app1_service_dashboard < schema.sql
docker exec -i app1-postgres psql -U app1user -d app1_service_dashboard < seed.sql
```

#### App1 バックエンド

```bash
cd src/app1-service-dashboard/backend/App1Backend
dotnet run
```

#### App1 フロントエンド

```bash
cd src/app1-service-dashboard/frontend
npm install   # 初回のみ
npm run dev
```

App2 も同様の手順で起動できます（ポートは 5434/5002/3002）。

## 技術スタック

| 層 | 技術 |
|---|---|
| フロントエンド | React 18, TypeScript, Vite, Tailwind CSS v4, Recharts, TanStack Query |
| バックエンド | ASP.NET Core 10 WebAPI, Entity Framework Core 9, CsvHelper |
| データベース | PostgreSQL 16 |
| オーケストレーション | .NET Aspire (オプション) |

## ドキュメント

- [システム概要・ポート一覧](docs/overview.md)
- [App1 機能要件](docs/app1-service-dashboard/requirements.md)
- [App1 データモデル](docs/app1-service-dashboard/data-model.md)
- [App2 機能要件](docs/app2-dev-dashboard/requirements.md)
- [App2 データモデル](docs/app2-dev-dashboard/data-model.md)
