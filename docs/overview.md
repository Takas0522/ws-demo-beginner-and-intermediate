# システム全体概要

## システム構成

本システムは、複合企業が提供する電子サービス群の「事業KPI可視化」と「開発進捗管理」を目的とした2つの独立したWebアプリケーションで構成されます。

---

## アプリケーション一覧

| アプリ | 名称 | 概要 |
|--------|------|------|
| App1 | サービスダッシュボード | 各電子サービスの利用状況・KPI情報の可視化 |
| App2 | 開発状況確認ダッシュボード | チケット・PR管理および開発プロジェクトの進捗管理 |

---

## ビジネスドメイン背景

- **会社形態**: 複合企業（conglomerate）
- **サービス形態**: あらゆる分野の電子サービスを複数提供
- **販売単位**: 事業部単位でサービスを販売
- **開発・運用単位**: 各部署（チーム）が担当サービスの開発・運用・管理を実施

---

## 技術スタック

| レイヤー | 技術 |
|----------|------|
| Frontend | React |
| Backend | ASP.NET Core WebAPI |
| Database | PostgreSQL |
| アプリ管理・起動 | .NET Aspire |

---

## ポート割り当て

| アプリ | レイヤー | ポート番号 |
|--------|----------|-----------|
| App1 サービスダッシュボード | Frontend | 3001 |
| App1 サービスダッシュボード | Backend (WebAPI) | 5001 |
| App1 サービスダッシュボード | Database (PostgreSQL) | 5433 |
| App2 開発状況確認ダッシュボード | Frontend | 3002 |
| App2 開発状況確認ダッシュボード | Backend (WebAPI) | 5002 |
| App2 開発状況確認ダッシュボード | Database (PostgreSQL) | 5434 |
| Aspire Dashboard | - | 18888 |

---

## ディレクトリ構成

```
src/
├── app1-service-dashboard/
│   ├── frontend/          # React アプリ
│   ├── backend/           # ASP.NET Core WebAPI
│   └── database/          # PostgreSQL マイグレーション・スキーマ定義
├── app2-dev-dashboard/
│   ├── frontend/          # React アプリ
│   ├── backend/           # ASP.NET Core WebAPI
│   └── database/          # PostgreSQL マイグレーション・スキーマ定義
└── aspire-host/           # .NET Aspire AppHost（全アプリの統合起動）
```

---

## 一括起動

Aspire AppHost を使用して全アプリを一括起動します。

```bash
cd src/aspire-host
dotnet run
```

または、プロジェクトルートのスクリプトを使用します。

```bash
./scripts/start-all.sh
```

各アプリ個別起動については各アプリのドキュメントを参照してください。

---

## 関連ドキュメント

- [App1 機能要件: サービスダッシュボード](./app1-service-dashboard/requirements.md)
- [App2 機能要件: 開発状況確認ダッシュボード](./app2-dev-dashboard/requirements.md)
- [データモデル: App1](./app1-service-dashboard/data-model.md)
- [データモデル: App2](./app2-dev-dashboard/data-model.md)
