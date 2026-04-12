-- ============================================================
-- App1: サービスダッシュボード データベーススキーマ
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ────────────────────────────────────────
-- 事業部
-- ────────────────────────────────────────
CREATE TABLE business_units (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    description TEXT,
    created_at  TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at  TIMESTAMP   NOT NULL DEFAULT now()
);

-- ────────────────────────────────────────
-- サービスカテゴリ
-- ────────────────────────────────────────
CREATE TABLE service_categories (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    description TEXT
);

-- ────────────────────────────────────────
-- サービス
-- ────────────────────────────────────────
CREATE TABLE services (
    id               UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    business_unit_id UUID        NOT NULL REFERENCES business_units(id),
    category_id      UUID        NOT NULL REFERENCES service_categories(id),
    name             VARCHAR(200) NOT NULL,
    description      TEXT,
    launched_at      DATE,
    status           VARCHAR(20) NOT NULL DEFAULT 'active'
                     CHECK (status IN ('active','suspended','discontinued')),
    created_at       TIMESTAMP   NOT NULL DEFAULT now(),
    updated_at       TIMESTAMP   NOT NULL DEFAULT now()
);
CREATE INDEX idx_services_business_unit ON services(business_unit_id);
CREATE INDEX idx_services_category      ON services(category_id);
CREATE INDEX idx_services_status        ON services(status);

-- ────────────────────────────────────────
-- サービスプラン
-- ────────────────────────────────────────
CREATE TABLE service_plans (
    id         UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id UUID           NOT NULL REFERENCES services(id),
    name       VARCHAR(100)   NOT NULL,
    price      DECIMAL(12,2)  NOT NULL DEFAULT 0,
    is_paid    BOOLEAN        NOT NULL DEFAULT false,
    created_at TIMESTAMP      NOT NULL DEFAULT now()
);
CREATE INDEX idx_service_plans_service ON service_plans(service_id);

-- ────────────────────────────────────────
-- ユーザー指標 日次集計
-- ────────────────────────────────────────
CREATE TABLE user_metric_daily (
    id                  UUID     PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id          UUID     NOT NULL REFERENCES services(id),
    date                DATE     NOT NULL,
    mau                 INTEGER  NOT NULL DEFAULT 0,
    dau                 INTEGER  NOT NULL DEFAULT 0,
    new_users           INTEGER  NOT NULL DEFAULT 0,
    churned_users       INTEGER  NOT NULL DEFAULT 0,
    total_subscriptions INTEGER  NOT NULL DEFAULT 0,
    created_at          TIMESTAMP NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX idx_umd_service_date ON user_metric_daily(service_id, date);

-- ────────────────────────────────────────
-- 売上 日次集計
-- ────────────────────────────────────────
CREATE TABLE revenue_daily (
    id                 UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id         UUID           NOT NULL REFERENCES services(id),
    plan_id            UUID           NOT NULL REFERENCES service_plans(id),
    date               DATE           NOT NULL,
    amount             DECIMAL(15,2)  NOT NULL DEFAULT 0,
    subscription_count INTEGER        NOT NULL DEFAULT 0,
    created_at         TIMESTAMP      NOT NULL DEFAULT now()
);
CREATE INDEX idx_revenue_service_date ON revenue_daily(service_id, date);
CREATE INDEX idx_revenue_plan_date    ON revenue_daily(plan_id, date);

-- ────────────────────────────────────────
-- 原価 日次集計
-- ────────────────────────────────────────
CREATE TABLE cost_daily (
    id          UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id  UUID           NOT NULL REFERENCES services(id),
    date        DATE           NOT NULL,
    cost_type   VARCHAR(50)    NOT NULL
                CHECK (cost_type IN ('infrastructure','license','labor','other')),
    amount      DECIMAL(15,2)  NOT NULL DEFAULT 0,
    description TEXT,
    created_at  TIMESTAMP      NOT NULL DEFAULT now()
);
CREATE INDEX idx_cost_service_date ON cost_daily(service_id, date);

-- ────────────────────────────────────────
-- ABテスト
-- ────────────────────────────────────────
CREATE TABLE ab_tests (
    id                UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id        UUID        NOT NULL REFERENCES services(id),
    name              VARCHAR(200) NOT NULL,
    description       TEXT,
    primary_metric    VARCHAR(100) NOT NULL,
    started_at        DATE        NOT NULL,
    ended_at          DATE,
    status            VARCHAR(20) NOT NULL DEFAULT 'running'
                      CHECK (status IN ('running','completed','stopped')),
    winner_variant_id UUID,       -- FK追加はVariant作成後
    created_at        TIMESTAMP   NOT NULL DEFAULT now()
);
CREATE INDEX idx_ab_tests_service ON ab_tests(service_id);
CREATE INDEX idx_ab_tests_status  ON ab_tests(status);

-- ────────────────────────────────────────
-- ABテスト バリアント
-- ────────────────────────────────────────
CREATE TABLE ab_test_variants (
    id                 UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    ab_test_id         UUID          NOT NULL REFERENCES ab_tests(id),
    name               VARCHAR(100)  NOT NULL,
    description        TEXT,
    traffic_allocation DECIMAL(5,2)  NOT NULL DEFAULT 50.00,
    created_at         TIMESTAMP     NOT NULL DEFAULT now()
);
CREATE INDEX idx_variants_ab_test ON ab_test_variants(ab_test_id);

ALTER TABLE ab_tests
    ADD CONSTRAINT fk_winner_variant
    FOREIGN KEY (winner_variant_id) REFERENCES ab_test_variants(id);

-- ────────────────────────────────────────
-- ABテスト 測定結果
-- ────────────────────────────────────────
CREATE TABLE ab_test_results (
    id                          UUID           PRIMARY KEY DEFAULT gen_random_uuid(),
    variant_id                  UUID           NOT NULL REFERENCES ab_test_variants(id),
    metric_name                 VARCHAR(100)   NOT NULL,
    sample_size                 INTEGER        NOT NULL DEFAULT 0,
    metric_value                DECIMAL(15,6)  NOT NULL,
    p_value                     DECIMAL(10,6),
    confidence_interval_lower   DECIMAL(15,6),
    confidence_interval_upper   DECIMAL(15,6),
    is_statistically_significant BOOLEAN       NOT NULL DEFAULT false,
    recorded_at                 TIMESTAMP      NOT NULL DEFAULT now()
);
CREATE INDEX idx_results_variant_metric ON ab_test_results(variant_id, metric_name);
