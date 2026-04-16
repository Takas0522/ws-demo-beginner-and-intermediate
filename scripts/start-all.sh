#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PIDS=()

# ── DevContainer環境検出 ──────────────────────────────────────────────────────
if [ -n "${REMOTE_CONTAINERS:-}" ] || [ -f "/.dockerenv" ]; then
  DB_HOST="${DB_HOST:-172.17.0.1}"
  echo "ℹ DevContainer検出: DB接続先 → ${DB_HOST}"
else
  DB_HOST="${DB_HOST:-localhost}"
fi

export ConnectionStrings__DefaultConnection_App1="Host=${DB_HOST};Port=5433;Database=app1_service_dashboard;Username=app1user;Password=app1password"
export ConnectionStrings__DefaultConnection_App2="Host=${DB_HOST};Port=5434;Database=app2_dev_dashboard;Username=app2user;Password=app2password"
export ConnectionStrings__DefaultConnection_Auth="Host=${DB_HOST};Port=5435;Database=auth_service;Username=authuser;Password=authpassword"

cleanup() {
  echo ""
  echo "停止中..."
  for pid in "${PIDS[@]}"; do
    kill "$pid" 2>/dev/null || true
  done
  docker compose -f "${REPO_ROOT}/target-app/app1-service-dashboard/database/docker-compose.yml" down 2>/dev/null || true
  docker compose -f "${REPO_ROOT}/target-app/app2-dev-dashboard/database/docker-compose.yml" down 2>/dev/null || true
  docker compose -f "${REPO_ROOT}/target-app/auth-service/database/docker-compose.yml" down 2>/dev/null || true
  echo "全プロセスを停止しました。"
}
trap cleanup EXIT INT TERM

echo "========================================="
echo " 全アプリ一括起動"
echo "========================================="
echo ""
echo "Auth Service (共通認証)"
echo "  Backend  : http://localhost:5000"
echo "  DB (PostgreSQL): localhost:5435"
echo ""
echo "App1 サービスダッシュボード"
echo "  Frontend : http://localhost:3001"
echo "  Backend  : http://localhost:5001"
echo "  DB (PostgreSQL): localhost:5433"
echo ""
echo "App2 開発状況確認ダッシュボード"
echo "  Frontend : http://localhost:3002"
echo "  Backend  : http://localhost:5002"
echo "  DB (PostgreSQL): localhost:5434"
echo ""

# ── データベース起動 ──────────────────────────────────────────────────────────
echo "[1/6] データベースを起動中..."
docker compose -f "${REPO_ROOT}/target-app/auth-service/database/docker-compose.yml" up -d
docker compose -f "${REPO_ROOT}/target-app/app1-service-dashboard/database/docker-compose.yml" up -d
docker compose -f "${REPO_ROOT}/target-app/app2-dev-dashboard/database/docker-compose.yml" up -d

echo "      DB の準備待ち (10秒)..."
sleep 10

# ── スキーマ・シード適用 (docker-entrypoint-initdb.d が機能しない環境でも確実に適用) ──
echo "      スキーマ/シード適用中..."

# Auth DB
docker exec -i auth-service-db psql -U authuser -d auth_service \
  < "${REPO_ROOT}/target-app/auth-service/database/schema.sql" 2>/dev/null || true
docker exec -i auth-service-db psql -U authuser -d auth_service \
  < "${REPO_ROOT}/target-app/auth-service/database/seed.sql"   2>/dev/null || true

# App1 DB
CONTAINER_APP1=$(docker ps --filter "name=app1-service-dashboard-db" --format "{{.Names}}" | head -1)
if [ -n "${CONTAINER_APP1}" ]; then
  docker exec -i "${CONTAINER_APP1}" psql -U app1user -d app1_service_dashboard \
    < "${REPO_ROOT}/target-app/app1-service-dashboard/database/schema.sql" 2>/dev/null || true
  # マイグレーション（既存DBへのカラム追加）
  docker exec -i "${CONTAINER_APP1}" psql -U app1user -d app1_service_dashboard -c \
    "CREATE TABLE IF NOT EXISTS service_stakeholders (
       id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
       service_id UUID NOT NULL REFERENCES services(id) ON DELETE CASCADE,
       auth_user_id UUID NOT NULL,
       display_name VARCHAR(100) NOT NULL,
       role VARCHAR(20) NOT NULL CHECK (role IN ('developer','operator','pm','pl','tl')),
       hourly_rate DECIMAL(10,2) NOT NULL DEFAULT 0,
       allocated_hours_monthly DECIMAL(6,1) NOT NULL DEFAULT 0,
       created_at TIMESTAMP NOT NULL DEFAULT now(),
       updated_at TIMESTAMP NOT NULL DEFAULT now(),
       UNIQUE (service_id, auth_user_id)
     );" 2>/dev/null || true
  docker exec -i "${CONTAINER_APP1}" psql -U app1user -d app1_service_dashboard \
    < "${REPO_ROOT}/target-app/app1-service-dashboard/database/seed.sql"   2>/dev/null || true
fi

# App2 DB
CONTAINER_APP2=$(docker ps --filter "name=app2-dev-dashboard-db" --format "{{.Names}}" | head -1)
if [ -n "${CONTAINER_APP2}" ]; then
  docker exec -i "${CONTAINER_APP2}" psql -U app2user -d app2_dev_dashboard \
    < "${REPO_ROOT}/target-app/app2-dev-dashboard/database/schema.sql" 2>/dev/null || true
  # マイグレーション（既存DBへのカラム追加）
  docker exec -i "${CONTAINER_APP2}" psql -U app2user -d app2_dev_dashboard -c \
    "ALTER TABLE members ADD COLUMN IF NOT EXISTS auth_user_id UUID NULL;
     CREATE INDEX IF NOT EXISTS idx_members_auth_user ON members(auth_user_id);" 2>/dev/null || true
  docker exec -i "${CONTAINER_APP2}" psql -U app2user -d app2_dev_dashboard \
    < "${REPO_ROOT}/target-app/app2-dev-dashboard/database/seed.sql"   2>/dev/null || true
fi

# ── バックエンド ビルド ────────────────────────────────────────────────────────
echo "[2/6] バックエンドをビルド中..."
dotnet build "${REPO_ROOT}/target-app/auth-service/backend/AuthService" -v:minimal 2>&1 | grep -E "^Build|error TS|error CS" || true
dotnet build "${REPO_ROOT}/target-app/app1-service-dashboard/backend/App1Backend" -v:minimal 2>&1 | grep -E "^Build|error TS|error CS" || true
dotnet build "${REPO_ROOT}/target-app/app2-dev-dashboard/backend/App2Backend" -v:minimal 2>&1 | grep -E "^Build|error TS|error CS" || true
echo "      ビルド完了"

# ── Auth Service バックエンド ──────────────────────────────────────────────────
echo "[3/6] Auth Service バックエンド起動中 (port 5000)..."
cd "${REPO_ROOT}/target-app/auth-service/backend/AuthService"
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection_Auth}" \
  dotnet run --no-build 2>&1 | sed 's/^/[auth-api] /' &
PIDS+=($!)

echo "      Auth Service の準備待ち (5秒)..."
sleep 5

# ── App1 バックエンド ─────────────────────────────────────────────────────────
echo "[4/6] App1 バックエンド起動中 (port 5001)..."
cd "${REPO_ROOT}/target-app/app1-service-dashboard/backend/App1Backend"
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection_App1}" \
  dotnet run --no-build 2>&1 | sed 's/^/[app1-api] /' &
PIDS+=($!)

# ── App2 バックエンド ─────────────────────────────────────────────────────────
echo "[5/6] App2 バックエンド起動中 (port 5002)..."
cd "${REPO_ROOT}/target-app/app2-dev-dashboard/backend/App2Backend"
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection_App2}" \
  dotnet run --no-build 2>&1 | sed 's/^/[app2-api] /' &
PIDS+=($!)

echo "      バックエンドの準備待ち (5秒)..."
sleep 5

# ── フロントエンド ─────────────────────────────────────────────────────────────
echo "[6/6] フロントエンド起動中 (port 3001, 3002)..."
cd "${REPO_ROOT}/target-app/app1-service-dashboard/frontend"
npm run dev 2>&1 | sed 's/^/[app1-ui] /' &
PIDS+=($!)

cd "${REPO_ROOT}/target-app/app2-dev-dashboard/frontend"
npm run dev 2>&1 | sed 's/^/[app2-ui] /' &
PIDS+=($!)

echo ""
echo "起動完了 🚀  (Ctrl+C で全停止)"
echo ""

wait "${PIDS[@]}"
