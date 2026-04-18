# Skill Test Harness

Skill をテストするための .NET CLI アプリケーションです。  
`mock` モードと `live` モードの両方で実行でき、受け入れ基準に基づいた自動検証を行います。

## 必要要件

- .NET 8.0 以上
- Live モード: GitHub Copilot CLI がインストールされ、ログイン済みであること

## ビルドと実行

```bash
cd src/skill-harness
dotnet restore
dotnet build
```

## コマンド

### `test` — Skill テストの実行

```bash
dotnet run -- test --skill-path <SKILL.md へのパス> [オプション]
```

| オプション | デフォルト | 説明 |
|-----------|-----------|------|
| `--skill-path` | (必須) | SKILL.md へのパス |
| `--mode` | `mock` | 実行モード: `mock` または `live` |
| `--test-cases-path` | (自動検索) | テストケース JSON ファイルのパス |
| `--verbose` | `false` | 詳細ログを出力する |

**使用例:**

```bash
# mock モードで実行
dotnet run -- test \
  --skill-path ../../.github/skills/web-analytics/SKILL.md \
  --mode mock

# live モードで実行（Copilot CLI が必要）
dotnet run -- test \
  --skill-path ../../.github/skills/web-analytics/SKILL.md \
  --mode live \
  --verbose

# テストケースファイルを明示的に指定
dotnet run -- test \
  --skill-path ../../.github/skills/web-analytics/SKILL.md \
  --test-cases-path ./my-test-cases.json
```

### `validate` — SKILL.md の検証

SKILL.md の構文を検証し、メタデータを表示します。

```bash
dotnet run -- validate --skill-path <SKILL.md へのパス>
```

## テストケース JSON の形式

テストケースは SKILL.md と同じディレクトリの `test-cases.json` に配置するか、  
`--test-cases-path` で指定します。

```json
[
  {
    "id": "tc-1",
    "description": "テストの説明",
    "prompt": "エージェントに送るプロンプト",
    "timeoutMs": 60000,
    "criteria": [
      {
        "id": "c1",
        "description": "出力に 'csv' が含まれるか",
        "type": "OutputContains",
        "expected": "csv"
      },
      {
        "id": "c2",
        "description": "許可外のツールが使われていないか",
        "type": "NoExtraTools"
      },
      {
        "id": "c3",
        "description": "コードブロックが含まれるか",
        "type": "ContainsCode"
      },
      {
        "id": "c4",
        "description": "テストコード構造が含まれるか",
        "type": "TestPasses"
      },
      {
        "id": "c5",
        "description": "直接DBアクセスがないか",
        "type": "NoDirectDb"
      }
    ]
  }
]
```

### 受け入れ基準の種別

| Type | 説明 | `expected` フィールド |
|------|------|----------------------|
| `OutputContains` | レスポンスに指定文字列が含まれるか | 必須（検索文字列） |
| `NoExtraTools` | SKILL.md の `allowed-tools` 以外のツールが使われていないか | 不要 |
| `ContainsCode` | レスポンスにコードブロックが含まれるか | 不要 |
| `TestPasses` | レスポンスにテストコード構造が含まれるか | 不要 |
| `NoDirectDb` | 直接DB操作（SQL等）が検出されないか | 不要 |

## Mock vs Live モードの比較

| 比較項目 | Mock モード | Live モード |
|---------|------------|------------|
| Copilot CLI | 不要 | 必要 |
| 実行速度 | 高速（~150ms） | 低速（モデル応答時間次第） |
| 用途 | 構造・基準の確認 | 実際の品質検証 |
| レスポンス | 固定モックデータ | 実際のLLM応答 |

## プロジェクト構成

```
src/skill-harness/
  SkillHarness.csproj      # プロジェクトファイル
  Program.cs               # エントリポイント・コマンド定義
  Models/
    SkillModels.cs          # ParsedSkill, SkillMetadata
    TestModels.cs           # SkillTestCase, AcceptanceCriteria, SkillTestResult
  Services/
    SkillParser.cs          # SKILL.md パーサー・テストケース読み込み
    TestHarness.cs          # テスト実行エンジン（mock / live）
  example-test-cases.json  # テストケース JSON のサンプル
```
