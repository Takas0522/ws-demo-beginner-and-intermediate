#!/bin/bash
set -e

# .NET Aspire ワークロード
sudo dotnet workload install aspire

# Playwright CLI (スキルインストール含む)
npm install -g @playwright/cli@latest
playwright-cli install --skills
# playwright-cli が使用するバージョンのブラウザをインストール
# Chromium の OS 依存ライブラリは Dockerfile でインストール済み
playwright-cli install-browser chromium

# GitHub Copilot CLI
npm install -g @githubnext/github-copilot-cli

# uv (Python プロジェクト環境管理)
curl -LsSf https://astral.sh/uv/install.sh | sh
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
