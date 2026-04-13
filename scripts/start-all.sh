#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PIDS=()

# ── DevContainer環境検出 ──────────────────────────────────────────────────────
# DevContainerではDockerのポートフォワーディングがlocalhost非経由のため
# Dockerデフォルトブリッジのゲートウェイ (172.17.0.1) を使用する
if [ -n "${REMOTE_CONTAINERS:-}" ] || [ -f "/.dockerenv" ]; then
  DB_HOST="${DB_HOST:-172.17.0.1}"
  echo "ℹ DevContainer検出: DB接続先 → ${DB_HOST}"
else
  DB_HOST="${DB_HOST:-localhost}"
fi

export ConnectionStrings__DefaultConnection_App1="Host=${DB_HOST};Port=5433;Database=app1_service_dashboard;Username=app1user;Password=app1password"
export ConnectionStrings__DefaultConnection_App2="Host=${DB_HOST};Port=5434;Database=app2_dev_dashboard;Username=app2user;Password=app2password"

cleanup() {
  echo ""
  echo "停止中..."
  for pid in "${PIDS[@]}"; do
    kill "$pid" 2>/dev/null || true
  done
  # Docker compose down
  docker compose -f "${REPO_ROOT}/src/app1-service-dashboard/database/docker-compose.yml" down 2>/dev/null || true
  docker compose -f "${REPO_ROOT}/src/app2-dev-dashboard/database/docker-compose.yml" down 2>/dev/null || true
  echo "全プロセスを停止しました。"
}
trap cleanup EXIT INT TERM

echo "========================================="
echo " 全アプリ一括起動"
echo "========================================="
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
echo "[1/5] データベースを起動中..."
docker compose -f "${REPO_ROOT}/src/app1-service-dashboard/database/docker-compose.yml" up -d
docker compose -f "${REPO_ROOT}/src/app2-dev-dashboard/database/docker-compose.yml" up -d

echo "      DB の準備待ち (10秒)..."
sleep 10

# ── バックエンド ビルド ────────────────────────────────────────────────────────
echo "[2/5] バックエンドをビルド中..."
dotnet build "${REPO_ROOT}/src/app1-service-dashboard/backend/App1Backend" -v:minimal 2>&1 | grep -E "^Build|error TS|error CS" || true
dotnet build "${REPO_ROOT}/src/app2-dev-dashboard/backend/App2Backend" -v:minimal 2>&1 | grep -E "^Build|error TS|error CS" || true
echo "      ビルド完了"

# ── App1 バックエンド ─────────────────────────────────────────────────────────
echo "[3/5] App1 バックエンド起動中 (port 5001)..."
cd "${REPO_ROOT}/src/app1-service-dashboard/backend/App1Backend"
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection_App1}" \
  dotnet run --no-build 2>&1 | sed 's/^/[app1-api] /' &
PIDS+=($!)

# ── App2 バックエンド ─────────────────────────────────────────────────────────
echo "[4/5] App2 バックエンド起動中 (port 5002)..."
cd "${REPO_ROOT}/src/app2-dev-dashboard/backend/App2Backend"
ConnectionStrings__DefaultConnection="${ConnectionStrings__DefaultConnection_App2}" \
  dotnet run --no-build 2>&1 | sed 's/^/[app2-api] /' &
PIDS+=($!)

echo "      バックエンドの準備待ち (5秒)..."
sleep 5

# ── App1 フロントエンド ────────────────────────────────────────────────────────
echo "[5/5] フロントエンド起動中 (port 3001, 3002)..."
cd "${REPO_ROOT}/src/app1-service-dashboard/frontend"
npm run dev 2>&1 | sed 's/^/[app1-ui] /' &
PIDS+=($!)

# ── App2 フロントエンド ────────────────────────────────────────────────────────
cd "${REPO_ROOT}/src/app2-dev-dashboard/frontend"
npm run dev 2>&1 | sed 's/^/[app2-ui] /' &
PIDS+=($!)

echo ""
echo "起動完了 🚀  (Ctrl+C で全停止)"
echo ""

# 全プロセスが終わるまで待機
wait "${PIDS[@]}"
