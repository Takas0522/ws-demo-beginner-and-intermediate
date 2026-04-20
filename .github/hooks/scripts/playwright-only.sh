#!/usr/bin/env bash
# PreToolUse フック: playwright-cli 以外をブロックし、ログを記録する
# stdin から JSON を読み取り判定する

LOG_DIR="temp/hooks-log"
LOG_FILE="${LOG_DIR}/hooks.log"
TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')

mkdir -p "$LOG_DIR"

INPUT=$(cat 2>/dev/null || true)

# tool_name を取り出す
TOOL_NAME=$(python3 -c "
import json, sys
data = {}
try:
    data = json.loads(sys.stdin.read())
except Exception:
    pass
print(data.get('tool_name', ''))
" <<< "$INPUT")

if [[ "$TOOL_NAME" != playwright-cli* ]]; then
    echo "[${TIMESTAMP}] [PreToolUse] [DENIED] tool=${TOOL_NAME}" >> "$LOG_FILE"
    # exit code 2 でブロック (stderr がモデルへのコンテキストになる)
    echo "playwright-cli 以外のツール '${TOOL_NAME}' はこの Hook によりブロックされました" >&2
    exit 2
fi

echo "[${TIMESTAMP}] [PreToolUse] [ALLOWED] tool=${TOOL_NAME}" >> "$LOG_FILE"
