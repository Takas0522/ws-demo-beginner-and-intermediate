# サービス & 開発状況ダッシュボード

2つのダッシュボードアプリと共通認証サービスを含むモノレポです。

## アプリケーション構成

| | Auth Service | App1: サービスダッシュボード | App2: 開発状況ダッシュボード |
|---|---|---|---|
| **概要** | 共通JWT認証基盤 | 販売サービスのKPI管理 | チケット・PR・開発進捗管理 |
| **Frontend** | — | http://localhost:3001 | http://localhost:3002 |
| **Backend API** | http://localhost:5000 | http://localhost:5001 | http://localhost:5002 |
| **Swagger UI** | http://localhost:5000/swagger | http://localhost:5001/swagger | http://localhost:5002/swagger |
| **PostgreSQL** | localhost:5435 | localhost:5433 | localhost:5434 |

## ディレクトリ構成

```
src/
├── auth-service/
│   ├── backend/AuthService/   # ASP.NET Core 10 WebAPI (JWT発行・検証)
│   └── database/              # PostgreSQL (docker-compose + schema/seed SQL)
├── app1-service-dashboard/
│   ├── frontend/              # React + Vite + Tailwind CSS v4
│   ├── backend/App1Backend/   # ASP.NET Core 10 WebAPI
│   └── database/              # PostgreSQL (docker-compose + schema/seed SQL)
├── app2-dev-dashboard/
│   ├── frontend/              # React + Vite + Tailwind CSS v4
│   ├── backend/App2Backend/   # ASP.NET Core 10 WebAPI
│   └── database/              # PostgreSQL (docker-compose + schema/seed SQL)
└── aspire-host/               # .NET Aspire AppHost (Aspireワークロード利用時)
docs/                          # 機能要件・データモデル仕様
scripts/
├── start-all.sh               # 全アプリ一括起動スクリプト
└── stop-all.sh                # 全アプリ一括停止スクリプト
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

起動順序: PostgreSQL (全3DB) → Auth Service → App1/App2 バックエンド → フロントエンド

Ctrl+C で全プロセスを停止します。別ウィンドウから止める場合:

```bash
./scripts/stop-all.sh
```

### 認証について

全ページへのアクセスにはログインが必要です。ログインページでユーザー名とパスワードを入力するとJWTが発行され、以降のAPIリクエストに自動付与されます。

**テスト用アカウント（パスワードはすべて `password123`）**

| ユーザー名 | 表示名 | 権限 | 所属部門 |
|---|---|---|---|
| `admin` | システム管理者 | admin | — |
| `saas_dev` | SaaS開発 太郎 | user | SaaSプロダクト開発部 |
| `game_dev` | ゲーム開発 花子 | user | ゲーム開発部 |
| `media_dev` | メディア開発 次郎 | user | メディアプロダクト部 |
| `fintech_dev` | フィンテック 三郎 | user | フィンテック開発部 |
| `health_dev` | ヘルスケア 四郎 | user | ヘルスケア開発部 |
| `qa_engineer` | QAエンジニア 五郎 | user | QA・テスト部 |
| `sre_engineer` | SREエンジニア 六郎 | user | インフラ・SRE部 |
| `designer` | デザイナー 七郎 | user | デザイン部 |

**JWT仕様**

| 項目 | 値 |
|---|---|
| 発行元 (Issuer) | `auth-service` |
| 対象 (Audience) | `dashboard-apps` |
| 有効期限 | 8時間 |
| 検証方式 | 共有シークレット（App1/App2がローカル検証） |

### 個別起動

#### Auth Service

```bash
cd src/auth-service/database
docker compose up -d
# 初回のみ: スキーマ・シードデータ投入
docker exec -i auth-service-db psql -U authuser -d auth_service < schema.sql
docker exec -i auth-service-db psql -U authuser -d auth_service < seed.sql

cd ../backend/AuthService
dotnet run
```

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
| 認証 | JWT Bearer (共有シークレット方式), BCrypt.Net |
| データベース | PostgreSQL 16 |
| オーケストレーション | .NET Aspire (オプション) |

## ドキュメント

- [システム概要・ポート一覧](docs/overview.md)
- [App1 機能要件](docs/app1-service-dashboard/requirements.md)
- [App1 データモデル](docs/app1-service-dashboard/data-model.md)
- [App2 機能要件](docs/app2-dev-dashboard/requirements.md)
- [App2 データモデル](docs/app2-dev-dashboard/data-model.md)
