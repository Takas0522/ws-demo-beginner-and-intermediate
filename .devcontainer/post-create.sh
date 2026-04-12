#!/bin/bash
set -e

# .NET Aspire ワークロード
sudo dotnet workload install aspire

# Playwright CLI (スキルインストール含む)
npm install -g @playwright/cli@latest
playwright-cli install --skills
npx playwright install --with-deps chromium

# GitHub Copilot CLI
npm install -g @githubnext/github-copilot-cli
