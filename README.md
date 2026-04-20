2つのダッシュボードアプリが存在し、そのアプリケーションをターゲットとして作業を行うリポジトリです。

## アプリケーション構成

| | Auth Service | App1: サービスダッシュボード | App2: 開発状況ダッシュボード |
|---|---|---|---|
| **概要** | 共通JWT認証基盤 | 販売サービスのKPI管理 | チケット・PR・開発進捗管理 |
| **Frontend** | — | http://localhost:3001 | http://localhost:3002 |
| **Backend API** | http://localhost:5000 | http://localhost:5001 | http://localhost:5002 |
| **Swagger UI** | http://localhost:5000/swagger | http://localhost:5001/swagger | http://localhost:5002/swagger |
| **PostgreSQL** | localhost:5435 | localhost:5433 | localhost:5434 |

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

