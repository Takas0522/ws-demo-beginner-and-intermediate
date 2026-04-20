FROM mcr.microsoft.com/devcontainers/base:debian

# Chromium の OS 依存ライブラリ（playwright-cli が使用）
RUN apt-get update && apt-get install -y \
    libnspr4 libnss3 \
    libatk1.0-0 libatk-bridge2.0-0 \
    libcups2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxfixes3 libxrandr2 \
    libgbm1 libpango-1.0-0 libcairo2 \
    libasound2t64 libatspi2.0-0 \
    python3-venv \
    && rm -rf /var/lib/apt/lists/*

# dev-analysis 用 Python 仮想環境を /opt に事前構築
# （ワークスペースに依存しないためビルド時に固定パスへインストール）
RUN python3 -m venv /opt/dev-analysis-venv \
    && /opt/dev-analysis-venv/bin/pip install \
        pandas \
        matplotlib \
        japanize-matplotlib \
        seaborn \
        scikit-learn \
        notebook \
        ipykernel
