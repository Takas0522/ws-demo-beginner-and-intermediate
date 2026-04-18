#!/bin/bash
set -e

# .NET Aspire ワークロード
sudo dotnet workload install aspire

# Playwright CLI (スキルインストール含む)
npm install -g @playwright/cli@latest
playwright-cli install --skills
# playwright-cli が使用するバージョンのブラウザをインストール
playwright-cli install-browser chromium
# Chromium の OS 依存ライブラリをインストール
sudo apt-get install -y \
  libnspr4 libnss3 \
  libatk1.0-0 libatk-bridge2.0-0 \
  libcups2 libxkbcommon0 \
  libxcomposite1 libxdamage1 libxfixes3 libxrandr2 \
  libgbm1 libpango-1.0-0 libcairo2 \
  libasound2t64 libatspi2.0-0

# GitHub Copilot CLI
npm install -g @githubnext/github-copilot-cli

# uv (Python プロジェクト環境管理)
curl -LsSf https://astral.sh/uv/install.sh | sh
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc
