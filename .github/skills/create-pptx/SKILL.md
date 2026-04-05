---
name: create-pptx
description: "マークダウンやテキスト情報からPowerPointスライドを作成する。Use when: パワーポイント作成、PPTX生成、スライド作成、プレゼン資料作成、PowerPoint、プレゼンテーション"
argument-hint: "マークダウンファイルパスまたはスライド内容の説明"
---

# PowerPoint スライド作成スキル

マークダウンファイルやテキスト情報をもとに、ダークテーマのモダンな PowerPoint プレゼンテーションを生成する。

## テンプレート情報

テンプレートファイル: [template_dark.pptx](./resources/template_dark.pptx)

### デザイン仕様

| 項目 | 値 |
|------|-----|
| スライドサイズ | 16:9 ワイドスクリーン (13.333 x 7.5 インチ) |
| フォント | メイリオ |
| 背景色 | ダークネイビー `#1E1E2E` |
| サーフェス色 | `#2A2A3C` |
| アクセント(青) | `#009BF5` |
| アクセント(緑) | `#00D4AA` |
| アクセント(紫) | `#AA70FF` |
| テキスト(主) | `#F0F0F5` |
| テキスト(副) | `#A0A0B0` |
| 区切り線 | `#3A3A4E` |

### 利用可能なスライドレイアウト

テンプレートには以下の7種類のスライドが含まれる。生成スクリプトではこれらを **レイアウトタイプ** として組み合わせてスライドを構成する。

| レイアウトID | 名前 | 用途 | テンプレートスライド |
|---|---|---|---|
| `title` | タイトルスライド | プレゼン冒頭。タイトル・サブタイトル・発表者情報 | スライド1 |
| `section` | セクション区切り | 大きなセクションの開始。番号+タイトル+説明 | スライド2 |
| `content` | コンテンツ | 標準的な本文スライド。ヘッダー+箇条書き | スライド3 |
| `two-column` | 2カラム | 左右対比・比較・並列情報 | スライド4 |
| `three-cards` | 3カード | 3つのポイントを並列に強調 | スライド5 |
| `visual-text` | ビジュアル+テキスト | 画像/図表エリア+テキスト説明 | スライド6 |
| `table` | テーブル | 表形式のデータ表示。ヘッダー行+データ行 | ー（動的生成） |
| `ending` | エンディング | 締めくくり。Thank You + 連絡先 | スライド7 |

## 手順

### ステップ1: スライド構成の設計

ユーザーから提示された情報（マークダウンファイル、テキスト、要件など）を分析し、以下の形式でスライド構成を設計する。

**構成設計のルール:**
1. 情報の階層構造を分析し、大見出し(h1/h2)をセクション区切り、小見出し(h3以下)をコンテンツスライドに対応させる
2. 各スライドに最適なレイアウトタイプを選定する
3. 1スライドに詰め込みすぎない（箇条書きは最大5〜6項目）
4. 比較・対比情報は `two-column`、3つの要素は `three-cards` を選ぶ
5. 図表や画像が必要な場面では `visual-text` を選ぶ
6. 表形式のデータ（比較表、一覧、スケジュール等）は `table` を選ぶ
7. 最初のスライドは必ず `title`、最後は `ending` にする

**構成の出力フォーマット（ユーザーに提示して確認を取る）:**

```
## スライド構成案

1. [title] プレゼンテーションタイトル
   - サブタイトル: ○○
   - 発表者: ○○

2. [section] セクション1: ○○
   - 概要: ○○

3. [content] ○○について
   - ポイント1
   - ポイント2
   - ポイント3

4. [two-column] ○○ vs △△
   - 左: ○○の説明
   - 右: △△の説明

5. [three-cards] 3つの特徴
   - カード1: ○○
   - カード2: △△
   - カード3: □□

...

N. [ending] Thank You
   - 連絡先: ○○
```

**ユーザーに構成を確認し、承認を得てからステップ2に進む。**

### ステップ2: スライドの生成

承認された構成をもとに、生成スクリプト [generate_pptx.py](./scripts/generate_pptx.py) を使用してPowerPointファイルを生成する。

#### 生成スクリプトの使い方

1. スライド定義JSONファイルを作成する
2. 生成スクリプトを実行する

**スライド定義JSONの形式:**

```json
{
  "output": "/temp/output.pptx",
  "slides": [
    {
      "layout": "title",
      "title": "プレゼンテーションタイトル",
      "subtitle": "サブタイトル",
      "author": "発表者名　|　所属　|　日付"
    },
    {
      "layout": "section",
      "number": "01",
      "title": "セクションタイトル",
      "description": "セクションの概要"
    },
    {
      "layout": "content",
      "title": "スライドタイトル",
      "body": "• ポイント1\n• ポイント2\n• ポイント3",
      "footer": "Confidential"
    },
    {
      "layout": "two-column",
      "title": "比較タイトル",
      "left_heading": "左見出し",
      "left_body": "左カラムの内容",
      "right_heading": "右見出し",
      "right_body": "右カラムの内容"
    },
    {
      "layout": "three-cards",
      "title": "3つのポイント",
      "cards": [
        {"number": "01", "heading": "見出し1", "body": "説明1"},
        {"number": "02", "heading": "見出し2", "body": "説明2"},
        {"number": "03", "heading": "見出し3", "body": "説明3"}
      ]
    },
    {
      "layout": "visual-text",
      "title": "ビジュアル+テキスト",
      "image_placeholder": "画像・図表エリア",
      "content_heading": "コンテンツ見出し",
      "content_body": "説明テキスト",
      "image_path": "(オプション) 画像ファイルパス"
    },
    {
      "layout": "table",
      "title": "データ一覧",
      "headers": ["項目", "値", "備考"],
      "rows": [
        ["項目A", "100", "説明A"],
        ["項目B", "200", "説明B"],
        ["項目C", "300", "説明C"]
      ],
      "footer": "出典: ○○"
    },
    {
      "layout": "ending",
      "message": "Thank You",
      "submessage": "ご清聴ありがとうございました",
      "contact": "email@example.com"
    }
  ]
}
```

#### 実行コマンド

```bash
# 仮想環境セットアップ（初回のみ）
python3 -m venv /tmp/pptx-env
source /tmp/pptx-env/bin/activate
pip install python-pptx

# スライド生成
source /tmp/pptx-env/bin/activate
python3 .github/skills/create-pptx/scripts/generate_pptx.py <スライド定義JSON> [--template <テンプレートパス>]
```

テンプレートパスのデフォルト: `.github/skills/create-pptx/resources/template_dark.pptx`

## 注意事項

- 生成されたPPTXの出力先は、ユーザーから指示がなければ `/temp/` に格納する
- `python-pptx` がインストールされていない場合は、仮想環境を作成してインストールする
- テンプレートのレイアウト `[6]` (Blank) をベースとして使用する
- スライド枚数が多い場合はセクション区切りを適切に挿入する
