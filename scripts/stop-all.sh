#!/usr/bin/env bash
# 全アプリ停止スクリプト
# start-all.sh をバックグラウンドで起動した場合や、
# プロセスが残存している場合に使用してください。

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "========================================="
echo " 全アプリ停止"
echo "========================================="

# ── バックエンドプロセス停止 ──────────────────────────────────────────────────
stop_by_port() {
  local port=$1
  local pid
  pid=$(ss -tlnp "sport = :${port}" 2>/dev/null | grep -oP 'pid=\K[0-9]+' | head -1)
  if [ -n "$pid" ]; then
    echo "  port $port → PID $pid を停止"
    kill "$pid" 2>/dev/null || true
  else
    echo "  port $port → プロセスなし"
  fi
}

echo "[1/3] バックエンドを停止中..."
stop_by_port 5000
stop_by_port 5001
stop_by_port 5002

echo "[2/3] フロントエンドを停止中..."
stop_by_port 3001
stop_by_port 3002

# ── Docker DB停止 ────────────────────────────────────────────────────────────
echo "[3/3] データベースを停止中..."
docker compose -f "${REPO_ROOT}/src/auth-service/database/docker-compose.yml" down 2>/dev/null \
  && echo "  Auth DB 停止" || echo "  Auth DB: 既に停止済み"
docker compose -f "${REPO_ROOT}/src/app1-service-dashboard/database/docker-compose.yml" down 2>/dev/null \
  && echo "  App1 DB 停止" || echo "  App1 DB: 既に停止済み"
docker compose -f "${REPO_ROOT}/src/app2-dev-dashboard/database/docker-compose.yml" down 2>/dev/null \
  && echo "  App2 DB 停止" || echo "  App2 DB: 既に停止済み"

echo ""
echo "全サービスを停止しました ✅"
