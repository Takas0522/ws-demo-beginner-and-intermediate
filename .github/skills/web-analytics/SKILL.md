---
name: web-analytics
description: "playwright-cli で http://localhost:3002/ から開発チケットデータを収集し、temp/collected-data.csv に保存後、src/web-analysis/ の Jupyter Notebook で分析・可視化する Skill"
allowed-tools: Bash(playwright-cli:*) Bash(uv:*) Bash(python:*) Bash(jupyter:*) EditFile WriteFile
---

# Web Analytics Skill

開発状況ダッシュボード（`http://localhost:3002/`）からチケットデータを収集し、
Jupyter Notebook で分析・可視化するまでの一連の手順を定義します。

---

## Phase 1: データ収集（playwright-cli）

### 1-1. ブラウザを起動してログイン

```bash
playwright-cli open --headed http://localhost:3002/
playwright-cli snapshot
```

スナップショットでログインフォームの ref を確認し、認証情報を入力してログインする。
ログイン後、チケット一覧ページへ遷移する。

### 1-2. チケットデータの CSV エクスポート

ページ上の「CSVエクスポート」ボタン（またはエクスポート機能）を探してクリックする。

```bash
playwright-cli snapshot
# エクスポートボタンの ref を確認してクリック
playwright-cli click <エクスポートボタンの ref>
playwright-cli snapshot
```

ダウンロードされたファイルまたはページ上に表示されたデータを取得する。
取得できない場合は playwright-cli eval でテーブルデータを直接抽出する:

```bash
playwright-cli eval "
  const rows = Array.from(document.querySelectorAll('table tr'));
  const csv = rows.map(r =>
    Array.from(r.querySelectorAll('td,th'))
      .map(c => JSON.stringify(c.innerText.trim()))
      .join(',')
  ).join('\n');
  return csv;
"
```

### 1-3. CSV を temp/ に保存

取得したデータを `temp/collected-data.csv` に保存する。
ファイルが既に存在する場合は上書きする。

```bash
# 保存確認
head -3 temp/collected-data.csv
wc -l temp/collected-data.csv
```

### 1-4. ブラウザを閉じる

```bash
playwright-cli close
```

---

## Phase 2: Python / uv プロジェクトのセットアップ

### 2-1. プロジェクトがなければ新規作成

```bash
ls src/web-analysis/ 2>/dev/null || uv init src/web-analysis
```

### 2-2. 必要なパッケージを追加

```bash
cd src/web-analysis
uv add pandas matplotlib seaborn plotly nbformat ipykernel jupyter
```

日本語フォントが必要な場合（日本語列名を含む CSV）:

```bash
uv add japanize-matplotlib
```

---

## Phase 3: Jupyter Notebook の構築

`src/web-analysis/analysis.ipynb` を作成し、以下のセルを含める。

### セル 1: ライブラリのインポート

```python
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib
import seaborn as sns
import japanize_matplotlib  # 日本語フォント対応
from pathlib import Path
```

### セル 2: データ読み込み

```python
csv_path = Path("../../temp/collected-data.csv")
df = pd.read_csv(csv_path)
print(f"データ件数: {len(df)} 件")
print(f"カラム: {list(df.columns)}")
df.head()
```

### セル 3: 基本統計・欠損確認

```python
print("=== 基本統計 ===")
print(df.describe())
print("\n=== 欠損値 ===")
print(df.isnull().sum())
```

### セル 4 以降: 可視化（データ内容に応じて生成）

取得したデータの列を確認し、以下のような可視化を実施する。

**ステータス別チケット数**:
```python
status_counts = df["Status"].value_counts()
fig, ax = plt.subplots(figsize=(8, 5))
status_counts.plot(kind="bar", ax=ax, color="steelblue")
ax.set_title("ステータス別チケット数")
ax.set_xlabel("ステータス")
ax.set_ylabel("件数")
plt.tight_layout()
plt.savefig("status_distribution.png", dpi=150)
plt.show()
```

**優先度別チケット数**:
```python
priority_counts = df["Priority"].value_counts()
fig, ax = plt.subplots(figsize=(6, 6))
ax.pie(priority_counts, labels=priority_counts.index, autopct="%1.1f%%", startangle=90)
ax.set_title("優先度別チケット割合")
plt.tight_layout()
plt.savefig("priority_distribution.png", dpi=150)
plt.show()
```

**部署別チケット数**:
```python
dept_counts = df["DepartmentName"].value_counts()
fig, ax = plt.subplots(figsize=(10, 5))
dept_counts.plot(kind="barh", ax=ax)
ax.set_title("部署別チケット数")
ax.set_xlabel("件数")
plt.tight_layout()
plt.savefig("department_distribution.png", dpi=150)
plt.show()
```

**スプリント別完了率（スクラム分析）**:
```python
sprint_summary = df.groupby("SprintName")["Status"].apply(
    lambda x: (x == "done").sum() / len(x) * 100
).reset_index(name="完了率(%)")
fig, ax = plt.subplots(figsize=(10, 5))
ax.bar(sprint_summary["SprintName"], sprint_summary["完了率(%)"], color="mediumseagreen")
ax.set_title("スプリント別完了率")
ax.set_xlabel("スプリント")
ax.set_ylabel("完了率 (%)")
ax.set_ylim(0, 100)
plt.xticks(rotation=45, ha="right")
plt.tight_layout()
plt.savefig("sprint_completion.png", dpi=150)
plt.show()
```

---

## 制約

- `playwright-cli`・`uv`・`python`・`jupyter`・`EditFile`・`WriteFile` 以外のツールは使用しない
- `http://localhost:3002/` 以外のエンドポイントにはアクセスしない
- 収集した CSV の内容を改変しない（`temp/collected-data.csv` は読み取り専用）
- データベースへの直接アクセスは行わない

---

## 完了条件

- [ ] `temp/collected-data.csv` にデータが保存されている
- [ ] `src/web-analysis/analysis.ipynb` が作成されている
- [ ] Notebook を実行すると各グラフが出力される
- [ ] `src/web-analysis/pyproject.toml` が存在し uv プロジェクトが構成されている
