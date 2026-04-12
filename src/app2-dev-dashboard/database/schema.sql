-- ============================================================
-- App2: 開発状況確認ダッシュボード データベーススキーマ
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ────────────────────────────────────────
-- 事業部（App1 と同一IDで対応）
-- ────────────────────────────────────────
CREATE TABLE business_units (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    description TEXT,
    created_at  TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at  TIMESTAMP   NOT NULL DEFAULT now()
);

-- ────────────────────────────────────────
-- サービス（App1 と同一IDで対応）
-- ────────────────────────────────────────
CREATE TABLE services (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    business_unit_id UUID        NOT NULL REFERENCES business_units(id),
    name             VARCHAR(200) NOT NULL,
    description      TEXT,
    created_at       TIMESTAMP   NOT NULL DEFAULT now()
);
CREATE INDEX idx_services_bu ON services(business_unit_id);

-- ────────────────────────────────────────
-- 部署
-- ────────────────────────────────────────
CREATE TABLE departments (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(100)  NOT NULL,
    default_hourly_rate DECIMAL(10,2) NOT NULL DEFAULT 5000,
    description         TEXT,
    created_at          TIMESTAMP     NOT NULL DEFAULT now()
);

-- ────────────────────────────────────────
-- メンバー
-- ────────────────────────────────────────
CREATE TABLE members (
    id            UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    department_id UUID          NOT NULL REFERENCES departments(id),
    name          VARCHAR(100)  NOT NULL,
    hourly_rate   DECIMAL(10,2),  -- NULL の場合は部署デフォルト適用
    status        VARCHAR(20)   NOT NULL DEFAULT 'active'
                  CHECK (status IN ('active','inactive')),
    created_at    TIMESTAMP     NOT NULL DEFAULT now(),
    updated_at    TIMESTAMP     NOT NULL DEFAULT now()
);
CREATE INDEX idx_members_dept ON members(department_id);

-- ────────────────────────────────────────
-- プロジェクト
-- ────────────────────────────────────────
CREATE TABLE projects (
    id                 UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id         UUID           NOT NULL REFERENCES services(id),
    name               VARCHAR(200)   NOT NULL,
    description        TEXT,
    status             VARCHAR(20)    NOT NULL DEFAULT 'planning'
                       CHECK (status IN ('planning','active','on_hold','completed','cancelled')),
    planned_start_date DATE,
    planned_end_date   DATE,
    actual_start_date  DATE,
    actual_end_date    DATE,
    budget             DECIMAL(15,2)  NOT NULL DEFAULT 0,
    created_at         TIMESTAMP      NOT NULL DEFAULT now(),
    updated_at         TIMESTAMP      NOT NULL DEFAULT now()
);
CREATE INDEX idx_projects_service ON projects(service_id);
CREATE INDEX idx_projects_status  ON projects(status);

-- ────────────────────────────────────────
-- スプリント
-- ────────────────────────────────────────
CREATE TABLE sprints (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id       UUID        NOT NULL REFERENCES projects(id),
    name             VARCHAR(100) NOT NULL,
    goal             TEXT,
    status           VARCHAR(20) NOT NULL DEFAULT 'planning'
                     CHECK (status IN ('planning','active','completed')),
    start_date       DATE        NOT NULL,
    end_date         DATE        NOT NULL,
    planned_velocity INTEGER     NOT NULL DEFAULT 0,
    actual_velocity  INTEGER     NOT NULL DEFAULT 0,
    created_at       TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at       TIMESTAMP   NOT NULL DEFAULT now()
);
CREATE INDEX idx_sprints_project_status ON sprints(project_id, status);

-- ────────────────────────────────────────
-- チケット
-- ────────────────────────────────────────
CREATE TABLE tickets (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id      UUID          NOT NULL REFERENCES projects(id),
    sprint_id       UUID          REFERENCES sprints(id),
    assignee_id     UUID          REFERENCES members(id),
    title           VARCHAR(300)  NOT NULL,
    description     TEXT,
    ticket_type     VARCHAR(20)   NOT NULL DEFAULT 'task'
                    CHECK (ticket_type IN ('feature','bug','improvement','task')),
    priority        VARCHAR(20)   NOT NULL DEFAULT 'medium'
                    CHECK (priority IN ('critical','high','medium','low')),
    status          VARCHAR(20)   NOT NULL DEFAULT 'open'
                    CHECK (status IN ('open','in_progress','review','done','blocked')),
    story_points    INTEGER,
    estimated_hours DECIMAL(8,2),
    due_date        DATE,
    started_at      TIMESTAMP,
    completed_at    TIMESTAMP,
    created_at      TIMESTAMP     NOT NULL DEFAULT now(),
    updated_at      TIMESTAMP     NOT NULL DEFAULT now()
);
CREATE INDEX idx_tickets_project_status  ON tickets(project_id, status);
CREATE INDEX idx_tickets_sprint          ON tickets(sprint_id);
CREATE INDEX idx_tickets_assignee        ON tickets(assignee_id);
CREATE INDEX idx_tickets_type_priority   ON tickets(ticket_type, priority);

-- ────────────────────────────────────────
-- 工数実績ログ
-- ────────────────────────────────────────
CREATE TABLE work_logs (
    id                    UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id             UUID          NOT NULL REFERENCES tickets(id),
    member_id             UUID          NOT NULL REFERENCES members(id),
    work_date             DATE          NOT NULL,
    hours                 DECIMAL(5,2)  NOT NULL,
    description           TEXT,
    hourly_rate_snapshot  DECIMAL(10,2) NOT NULL,
    cost                  DECIMAL(12,2) NOT NULL,  -- hours × hourly_rate_snapshot
    created_at            TIMESTAMP     NOT NULL DEFAULT now()
);
CREATE INDEX idx_work_logs_ticket      ON work_logs(ticket_id);
CREATE INDEX idx_work_logs_member_date ON work_logs(member_id, work_date);
CREATE INDEX idx_work_logs_date        ON work_logs(work_date);

-- ────────────────────────────────────────
-- プルリクエスト
-- ────────────────────────────────────────
CREATE TABLE pull_requests (
    id             UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id     UUID        NOT NULL REFERENCES projects(id),
    pr_number      INTEGER     NOT NULL,
    title          VARCHAR(300) NOT NULL,
    description    TEXT,
    author_id      UUID        NOT NULL REFERENCES members(id),
    status         VARCHAR(20) NOT NULL DEFAULT 'open'
                   CHECK (status IN ('open','merged','closed')),
    base_branch    VARCHAR(100) NOT NULL DEFAULT 'main',
    head_branch    VARCHAR(100) NOT NULL,
    changed_files  INTEGER     NOT NULL DEFAULT 0,
    additions      INTEGER     NOT NULL DEFAULT 0,
    deletions      INTEGER     NOT NULL DEFAULT 0,
    opened_at      TIMESTAMP   NOT NULL DEFAULT now(),
    merged_at      TIMESTAMP,
    closed_at      TIMESTAMP,
    created_at     TIMESTAMP   NOT NULL DEFAULT now(),
    UNIQUE (project_id, pr_number)
);
CREATE INDEX idx_pr_project_status ON pull_requests(project_id, status);
CREATE INDEX idx_pr_author         ON pull_requests(author_id);

-- ────────────────────────────────────────
-- PRレビュー
-- ────────────────────────────────────────
CREATE TABLE pr_reviews (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    pull_request_id UUID        NOT NULL REFERENCES pull_requests(id),
    reviewer_id     UUID        NOT NULL REFERENCES members(id),
    status          VARCHAR(30) NOT NULL DEFAULT 'commented'
                    CHECK (status IN ('approved','changes_requested','commented')),
    submitted_at    TIMESTAMP   NOT NULL DEFAULT now(),
    created_at      TIMESTAMP   NOT NULL DEFAULT now()
);
CREATE INDEX idx_pr_reviews_pr ON pr_reviews(pull_request_id);

-- ────────────────────────────────────────
-- PRとチケットの関連
-- ────────────────────────────────────────
CREATE TABLE pr_ticket_links (
    pull_request_id UUID NOT NULL REFERENCES pull_requests(id),
    ticket_id       UUID NOT NULL REFERENCES tickets(id),
    PRIMARY KEY (pull_request_id, ticket_id)
);

-- ────────────────────────────────────────
-- スプリント指標 日次スナップショット（バーンダウン用）
-- ────────────────────────────────────────
CREATE TABLE sprint_metric_daily (
    id                      UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    sprint_id               UUID          NOT NULL REFERENCES sprints(id),
    date                    DATE          NOT NULL,
    remaining_story_points  INTEGER       NOT NULL DEFAULT 0,
    remaining_hours         DECIMAL(10,2) NOT NULL DEFAULT 0,
    completed_tickets       INTEGER       NOT NULL DEFAULT 0,
    created_at              TIMESTAMP     NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX idx_sprint_metric_date ON sprint_metric_daily(sprint_id, date);

-- ────────────────────────────────────────
-- ビュー: プロジェクト別原価サマリー
-- ────────────────────────────────────────
CREATE OR REPLACE VIEW v_project_cost_summary AS
SELECT
    p.id                                                         AS project_id,
    p.name                                                       AS project_name,
    s.id                                                         AS service_id,
    s.name                                                       AS service_name,
    bu.id                                                        AS business_unit_id,
    bu.name                                                      AS business_unit_name,
    p.status,
    p.budget,
    p.planned_start_date,
    p.planned_end_date,
    COALESCE(SUM(wl.hours), 0)                                   AS total_actual_hours,
    COALESCE(SUM(wl.cost), 0)                                    AS total_actual_cost,
    p.budget - COALESCE(SUM(wl.cost), 0)                         AS remaining_budget,
    ROUND(
        COALESCE(SUM(wl.cost), 0) / NULLIF(p.budget, 0) * 100, 2
    )                                                            AS budget_consumption_rate,
    COUNT(DISTINCT t.id)                                         AS total_tickets,
    COUNT(DISTINCT t.id) FILTER (WHERE t.status = 'done')        AS completed_tickets
FROM projects p
JOIN services s       ON s.id = p.service_id
JOIN business_units bu ON bu.id = s.business_unit_id
LEFT JOIN tickets t   ON t.project_id = p.id
LEFT JOIN work_logs wl ON wl.ticket_id = t.id
GROUP BY p.id, p.name, s.id, s.name, bu.id, bu.name,
         p.status, p.budget, p.planned_start_date, p.planned_end_date;
