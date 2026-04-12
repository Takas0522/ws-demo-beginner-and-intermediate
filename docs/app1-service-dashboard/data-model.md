# App1: サービスダッシュボード データモデル

## エンティティ一覧

---

### BusinessUnit（事業部）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | 事業部ID |
| name | VARCHAR(100) | 事業部名 |
| description | TEXT | 説明 |
| created_at | TIMESTAMP | 作成日時 |
| updated_at | TIMESTAMP | 更新日時 |

---

### ServiceCategory（サービスカテゴリ）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | カテゴリID |
| name | VARCHAR(100) | カテゴリ名（例: SaaS, ゲーム, メディア, EC等） |
| description | TEXT | 説明 |

---

### Service（サービス）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | サービスID |
| business_unit_id | UUID (FK) | 所属事業部 |
| category_id | UUID (FK) | サービスカテゴリ |
| name | VARCHAR(200) | サービス名 |
| description | TEXT | サービス概要 |
| launched_at | DATE | 提供開始日 |
| status | VARCHAR(20) | active / suspended / discontinued |
| created_at | TIMESTAMP | - |
| updated_at | TIMESTAMP | - |

---

### ServicePlan（サービスプラン）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | プランID |
| service_id | UUID (FK) | サービスID |
| name | VARCHAR(100) | プラン名（例: Free, Basic, Pro, Enterprise） |
| price | DECIMAL(12,2) | 月額単価 |
| is_paid | BOOLEAN | 有料プランフラグ |
| created_at | TIMESTAMP | - |

---

### UserMetricDaily（ユーザー指標日次集計）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| service_id | UUID (FK) | サービスID |
| date | DATE | 集計日 |
| mau | INTEGER | 月次アクティブユーザー数 |
| dau | INTEGER | 日次アクティブユーザー数 |
| new_users | INTEGER | 新規登録ユーザー数 |
| churned_users | INTEGER | 解約ユーザー数 |
| total_subscriptions | INTEGER | 有効契約数（累計） |
| created_at | TIMESTAMP | - |

インデックス: `(service_id, date)`

---

### RevenueDaily（売上日次集計）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| service_id | UUID (FK) | サービスID |
| plan_id | UUID (FK) | サービスプランID |
| date | DATE | 集計日 |
| amount | DECIMAL(15,2) | 売上金額 |
| subscription_count | INTEGER | 該当プランの有効契約数 |
| created_at | TIMESTAMP | - |

インデックス: `(service_id, date)`, `(plan_id, date)`

---

### CostDaily（原価日次集計）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| service_id | UUID (FK) | サービスID |
| date | DATE | 集計日 |
| cost_type | VARCHAR(50) | infrastructure / license / labor / other |
| amount | DECIMAL(15,2) | 原価金額 |
| description | TEXT | 内訳メモ |
| created_at | TIMESTAMP | - |

インデックス: `(service_id, date)`

---

### ABTest（ABテスト）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | ABテストID |
| service_id | UUID (FK) | 対象サービスID |
| name | VARCHAR(200) | テスト名 |
| description | TEXT | 仮説・目的 |
| primary_metric | VARCHAR(100) | 主要指標名（例: conversion_rate） |
| started_at | DATE | 開始日 |
| ended_at | DATE | 終了日（NULLの場合進行中） |
| status | VARCHAR(20) | running / completed / stopped |
| winner_variant_id | UUID (FK, NULL) | 勝者バリアントID |
| created_at | TIMESTAMP | - |

---

### ABTestVariant（ABテストバリアント）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | バリアントID |
| ab_test_id | UUID (FK) | ABテストID |
| name | VARCHAR(100) | バリアント名（例: Control, Treatment A） |
| description | TEXT | バリアント内容説明 |
| traffic_allocation | DECIMAL(5,2) | トラフィック割当率（%） |
| created_at | TIMESTAMP | - |

---

### ABTestResult（ABテスト測定結果）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| variant_id | UUID (FK) | バリアントID |
| metric_name | VARCHAR(100) | 指標名 |
| sample_size | INTEGER | サンプルユーザー数 |
| metric_value | DECIMAL(15,6) | 指標値 |
| p_value | DECIMAL(10,6) | p値（Control比） |
| confidence_interval_lower | DECIMAL(15,6) | 信頼区間下限 |
| confidence_interval_upper | DECIMAL(15,6) | 信頼区間上限 |
| is_statistically_significant | BOOLEAN | 統計的有意性フラグ |
| recorded_at | TIMESTAMP | 記録日時 |

インデックス: `(variant_id, metric_name)`
