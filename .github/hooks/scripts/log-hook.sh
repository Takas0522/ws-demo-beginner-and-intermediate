#!/usr/bin/env bash
# 共通ログスクリプト: すべての Agent Hook イベントで使用する
# 使い方: bash log-hook.sh <EventName>
# stdin から JSON を読み取り、 temp/hooks-log/hooks.log に記録する

EVENT_ARG="${1:-unknown}"

LOG_DIR="temp/hooks-log"
LOG_FILE="${LOG_DIR}/hooks.log"
RAW_DIR="${LOG_DIR}/raw"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
TIMESTAMP_FILE=$(date '+%Y%m%d_%H%M%S')

mkdir -p "$LOG_DIR" "$RAW_DIR"

# stdin の JSON を読み取る (ない場合は空文字)
INPUT=$(cat 2>/dev/null || true)

# デバッグ用: stdin の生データをイベント別ファイルに保存
echo "$INPUT" > "${RAW_DIR}/${TIMESTAMP_FILE}_${EVENT_ARG}.json"

# Python で主要フィールドを抽出し、raw JSON も整形して出力 (jq 非依存)
PARSED=$(python3 - "$EVENT_ARG" "$INPUT" <<'PYEOF' 2>/dev/null
import json, sys
raw = sys.argv[2] if len(sys.argv) > 2 else ''
data = {}
try:
    data = json.loads(raw)
except Exception:
    pass

# 引数で渡されたイベント名を優先、なければ stdin JSON から取得
event     = sys.argv[1] if len(sys.argv) > 1 else data.get('hook_event_name', 'unknown')
tool      = data.get('tool_name', '')
tool_id   = data.get('tool_use_id', '')
session   = data.get('session_id', '')
agent_id  = data.get('agent_id', '')
agent_type= data.get('agent_type', '')
trigger   = data.get('trigger', '')
source    = data.get('source', '')
prompt    = data.get('prompt', '')[:100] if data.get('prompt') else ''

parts = [f"event={event}"]
if session:   parts.append(f"session={session}")
if tool:      parts.append(f"tool={tool}")
if tool_id:   parts.append(f"tool_id={tool_id}")
if agent_id:  parts.append(f"agent_id={agent_id}")
if agent_type:parts.append(f"agent_type={agent_type}")
if trigger:   parts.append(f"trigger={trigger}")
if source:    parts.append(f"source={source}")
if prompt:    parts.append(f"prompt={prompt}")

# raw JSON を1行に整形して追記 (取得できるフィールドの確認用)
raw_oneline = json.dumps(data, ensure_ascii=False) if data else raw.strip()
parts.append(f"raw={raw_oneline}")

print(" | ".join(parts))
PYEOF
)

echo "[${TIMESTAMP}] ${PARSED}" >> "$LOG_FILE"
