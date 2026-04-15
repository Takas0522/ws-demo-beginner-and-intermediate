# App2: 開発状況確認ダッシュボード データモデル

## エンティティ一覧

---

### BusinessUnit（事業部）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | 事業部ID（App1と同一IDで対応付け） |
| name | VARCHAR(100) | 事業部名 |
| description | TEXT | 説明 |
| created_at | TIMESTAMP | - |
| updated_at | TIMESTAMP | - |

---

### Service（サービス）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | サービスID（App1と同一IDで対応付け） |
| business_unit_id | UUID (FK) | 所属事業部 |
| name | VARCHAR(200) | サービス名 |
| description | TEXT | サービス概要 |
| created_at | TIMESTAMP | - |

---

### Department（部署）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | 部署ID |
| name | VARCHAR(100) | 部署名 |
| default_hourly_rate | DECIMAL(10,2) | 部署デフォルト時間単価（円） |
| description | TEXT | 説明 |
| created_at | TIMESTAMP | - |

---

### Member（メンバー）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | メンバーID |
| department_id | UUID (FK) | 所属部署 |
| name | VARCHAR(100) | 氏名 |
| hourly_rate | DECIMAL(10,2) | 個人時間単価（NULLの場合は部署デフォルト適用） |
| status | VARCHAR(20) | active / inactive |
| created_at | TIMESTAMP | - |
| updated_at | TIMESTAMP | - |

---

### Project（プロジェクト）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | プロジェクトID |
| service_id | UUID (FK) | 対象サービスID |
| name | VARCHAR(200) | プロジェクト名 |
| description | TEXT | 概要 |
| status | VARCHAR(20) | planning / active / on_hold / completed / cancelled |
| planned_start_date | DATE | 計画開始日 |
| planned_end_date | DATE | 計画終了日 |
| actual_start_date | DATE | 実際開始日 |
| actual_end_date | DATE | 実際終了日（NULLの場合進行中） |
| budget | DECIMAL(15,2) | プロジェクト予算（円） |
| created_at | TIMESTAMP | - |
| updated_at | TIMESTAMP | - |

インデックス: `(service_id)`, `(status)`

---

### Sprint（スプリント）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | スプリントID |
| project_id | UUID (FK) | プロジェクトID |
| name | VARCHAR(100) | スプリント名（例: Sprint 1） |
| goal | TEXT | スプリントゴール |
| status | VARCHAR(20) | planning / active / completed |
| start_date | DATE | 開始日 |
| end_date | DATE | 終了予定日 |
| planned_velocity | INTEGER | 計画ベロシティ（ストーリーポイント） |
| actual_velocity | INTEGER | 実績ベロシティ |
| created_at | TIMESTAMP | - |
| updated_at | TIMESTAMP | - |

インデックス: `(project_id, status)`

---

### Ticket（チケット）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | チケットID |
| project_id | UUID (FK) | プロジェクトID |
| sprint_id | UUID (FK, NULL) | スプリントID（未割り当ての場合NULL） |
| assignee_id | UUID (FK, NULL) | 担当メンバーID |
| title | VARCHAR(300) | タイトル |
| description | TEXT | 詳細 |
| ticket_type | VARCHAR(20) | feature / bug / improvement / task |
| priority | VARCHAR(20) | critical / high / medium / low |
| status | VARCHAR(20) | open / in_progress / review / done / blocked |
| story_points | INTEGER | ストーリーポイント（NULL可） |
| estimated_hours | DECIMAL(8,2) | 見積もり工数（時間） |
| due_date | DATE | 期限日 |
| started_at | TIMESTAMP | 作業開始日時 |
| completed_at | TIMESTAMP | 完了日時 |
| created_at | TIMESTAMP | - |
| updated_at | TIMESTAMP | - |

インデックス: `(project_id, status)`, `(sprint_id)`, `(assignee_id)`, `(ticket_type, priority)`

---

### WorkLog（工数実績ログ）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| ticket_id | UUID (FK) | チケットID |
| member_id | UUID (FK) | メンバーID |
| work_date | DATE | 作業日 |
| hours | DECIMAL(5,2) | 実績工数（時間） |
| description | TEXT | 作業内容メモ |
| hourly_rate_snapshot | DECIMAL(10,2) | 記録時点の単価（原価計算用） |
| cost | DECIMAL(12,2) | 原価（hours × hourly_rate_snapshot） |
| created_at | TIMESTAMP | - |

インデックス: `(ticket_id)`, `(member_id, work_date)`, `(work_date)`

---

### PullRequest（プルリクエスト）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| project_id | UUID (FK) | プロジェクトID |
| pr_number | INTEGER | PR番号（プロジェクト内連番） |
| title | VARCHAR(300) | PRタイトル |
| description | TEXT | PR説明 |
| author_id | UUID (FK) | 作成者メンバーID |
| status | VARCHAR(20) | open / merged / closed |
| base_branch | VARCHAR(100) | マージ先ブランチ |
| head_branch | VARCHAR(100) | 作業ブランチ |
| changed_files | INTEGER | 変更ファイル数 |
| additions | INTEGER | 追加行数 |
| deletions | INTEGER | 削除行数 |
| opened_at | TIMESTAMP | オープン日時 |
| merged_at | TIMESTAMP | マージ日時（NULLの場合未マージ） |
| closed_at | TIMESTAMP | クローズ日時 |
| created_at | TIMESTAMP | - |

インデックス: `(project_id, status)`, `(author_id)`

---

### PRReview（PRレビュー）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| pull_request_id | UUID (FK) | PRのID |
| reviewer_id | UUID (FK) | レビュアーメンバーID |
| status | VARCHAR(20) | approved / changes_requested / commented |
| submitted_at | TIMESTAMP | レビュー提出日時 |
| created_at | TIMESTAMP | - |

インデックス: `(pull_request_id)`

---

### PRTicketLink（PRとチケットの関連）

| カラム名 | 型 | 説明 |
|----------|----|------|
| pull_request_id | UUID (FK) | PRのID |
| ticket_id | UUID (FK) | チケットID |

PRIMARY KEY: `(pull_request_id, ticket_id)`

---

### SprintMetricDaily（スプリント指標日次スナップショット）

| カラム名 | 型 | 説明 |
|----------|----|------|
| id | UUID (PK) | - |
| sprint_id | UUID (FK) | スプリントID |
| date | DATE | スナップショット日 |
| remaining_story_points | INTEGER | 残ストーリーポイント（バーンダウン用） |
| remaining_hours | DECIMAL(10,2) | 残予定工数 |
| completed_tickets | INTEGER | 完了チケット数（累計） |
| created_at | TIMESTAMP | - |

インデックス: `(sprint_id, date)`

---

## 原価計算ビュー（View）

### v_project_cost_summary

```sql
-- プロジェクトごとの工数・原価サマリービュー（例）
SELECT
    p.id AS project_id,
    p.name AS project_name,
    p.budget,
    SUM(wl.hours) AS total_actual_hours,
    SUM(wl.cost) AS total_actual_cost,
    p.budget - SUM(wl.cost) AS remaining_budget,
    ROUND(SUM(wl.cost) / NULLIF(p.budget, 0) * 100, 2) AS budget_consumption_rate
FROM projects p
LEFT JOIN tickets t ON t.project_id = p.id
LEFT JOIN work_logs wl ON wl.ticket_id = t.id
GROUP BY p.id, p.name, p.budget;
```
