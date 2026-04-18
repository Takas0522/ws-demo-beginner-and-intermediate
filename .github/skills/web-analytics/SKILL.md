---
name: web-analytics
description: "開発状況ダッシュボード(http://localhost:3002/)からCSVデータを収集し、Jupyter Notebookでスクラム開発状況を分析するSkill"
allowed-tools: Bash(node:*) Bash(npm:*) Bash(uv:*) Bash(jupyter:*) create_file edit_file
---

# web-analytics Skill

開発状況ダッシュボードからチケットデータを収集し、Jupyter Notebook で可視化・分析するまでの一連の工程を自動化します。

## 処理フロー

```
[Step 1] Playwright スクリプトでデータ収集
           → temp/collected-data.csv に保存
[Step 2] uv プロジェクトのセットアップ（初回のみ）
           → src/web-analytics/ に Python 環境を構築
[Step 3] Jupyter Notebook の作成・更新
           → src/web-analytics/analysis.ipynb
```

---

## Step 1: データ収集

`src/web-analytics/collect-data.js` を実行してダッシュボードから CSV を取得します。
このスクリプトは GitHub Copilot 無しでも単独で動作します。

```bash
# 依存パッケージが未インストールの場合は先にセットアップ
cd src/web-analytics
npm install
npx playwright install chromium --with-deps

# CSV を収集して temp/collected-data.csv に保存
node collect-data.js
```

デフォルトの接続先・認証情報は `collect-data.js` 内に記載されています。
変更が必要な場合は環境変数 `APP_URL` / `APP_USERNAME` / `APP_PASSWORD` で上書きできます。

---

## Step 2: Python 環境のセットアップ（uv）

`src/web-analytics/pyproject.toml` が存在しない場合は uv プロジェクトを初期化します。

```bash
cd src/web-analytics

# uv プロジェクトを初期化（Python 3.12）
uv init --python 3.12 --no-readme

# 分析に必要なパッケージを追加
uv add pandas matplotlib japanize-matplotlib seaborn scikit-learn notebook ipykernel
```

---

## Step 3: Jupyter Notebook の作成・更新

`src/web-analytics/analysis.ipynb` を作成し、以下の分析セクションを実装します。

### Notebook の構成

1. **環境設定**
   - sys.path に `.venv/lib/python3.12/site-packages` を追加
   - IPAexGothic フォントを登録して日本語表示を有効化

2. **データ読み込みと前処理**
   - `temp/collected-data.csv` を読み込む
   - 日付列を datetime 型に変換
   - Sprint 番号を数値型に変換

3. **チケット概要（ステータス・優先度・種別の分布）**
   - 棒グラフとパイチャートで可視化

4. **スプリント別ベロシティ**
   - 完了ストーリーポイントの推移を棒グラフで表示
   - 3 スプリント移動平均を折れ線グラフで重ね描き

5. **バックログ積み上げ推移**
   - スプリントごとの「完了 / 未完了」積み上げ棒グラフ

6. **見積もりと実績の工数比較**
   - 散布図（EstimatedHours vs ActualHours）
   - 乖離率ヒストグラム

7. **ベロシティ将来予測（線形回帰）**
   - LinearRegression で次 3 スプリントを予測
   - 予測値を色分けして既存バーと並べて表示

8. **ブロッカー分析**
   - プロジェクト別・優先度別のブロッカー数を棒グラフで表示

9. **分析サマリー**
   - 上記の主要数値をテキストで一覧出力

### 日本語フォント設定（Notebook 冒頭のコードセルに必ず記述）

```python
import sys, os
from matplotlib import font_manager

_venv = os.path.join(os.path.dirname(os.path.abspath('analysis.ipynb')),
                     '.venv/lib/python3.12/site-packages')
if _venv not in sys.path:
    sys.path.insert(0, _venv)

_font = os.path.join(_venv, 'japanize_matplotlib/fonts/ipaexg.ttf')
if os.path.exists(_font):
    font_manager.fontManager.addfont(_font)
    import matplotlib.pyplot as plt
    import seaborn as sns
    sns.set_theme(style='whitegrid')
    plt.rcParams['font.family'] = 'IPAexGothic'
```

---

## 制約

- データベースへの直接アクセス（SQL 実行）は行わない
- `temp/` 以外のディレクトリへのデータ出力は行わない
- 分析コードは `src/web-analytics/` 配下にのみ作成する
