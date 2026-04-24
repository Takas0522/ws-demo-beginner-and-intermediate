using System.Diagnostics;
using System.Text.RegularExpressions;
using GitHub.Copilot.SDK;
using SkillHarness.Models;

namespace SkillHarness.Services;

public enum TestMode { Mock, Live }

public sealed class SkillTestHarnessConfig
{
    public TestMode Mode { get; init; } = TestMode.Mock;
    public bool Verbose { get; init; }
    public string Model { get; init; } = "claude-sonnet-4-5";
}

/// <summary>
/// Skill をテストするハーネス（mock / live 両対応）
/// </summary>
public sealed class SkillTestHarness : IAsyncDisposable
{
    private readonly SkillTestHarnessConfig _config;
    private CopilotClient? _client;

    public SkillTestHarness(SkillTestHarnessConfig config)
    {
        _config = config;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_config.Mode == TestMode.Live)
        {
            _client = new CopilotClient();
            await _client.StartAsync(ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.StopAsync();
            await _client.DisposeAsync();
            _client = null;
        }
    }

    public async Task<List<SkillTestResult>> RunTestSuiteAsync(
        ParsedSkill skill,
        List<SkillTestCase> testCases,
        CancellationToken ct = default)
    {
        var results = new List<SkillTestResult>();

        foreach (var testCase in testCases)
        {
            if (_config.Verbose)
                Console.WriteLine($"  実行中: [{testCase.Id}] {testCase.Description}");

            var result = _config.Mode == TestMode.Mock
                ? RunMockTest(skill, testCase)
                : await RunLiveTestAsync(skill, testCase, ct);

            results.Add(result);

            if (_config.Verbose)
                LogResult(result);
        }

        return results;
    }

    // -----------------------------------------------------------------------
    // Mock モード
    // -----------------------------------------------------------------------

    private SkillTestResult RunMockTest(ParsedSkill skill, SkillTestCase testCase)
    {
        var sw = Stopwatch.StartNew();

        var response = GenerateMockResponse(skill);
        var output = new SkillTestOutput
        {
            Response = response,
            ToolsUsed = skill.Metadata.AllowedTools.Take(1).ToList(),  // 許可済みツールのみ使用したと仮定
            DurationMs = 150,
            Completed = true,
        };
        sw.Stop();

        var criteriaResults = EvaluateCriteria(testCase.Criteria, output, skill);
        return new SkillTestResult
        {
            TestCase = testCase,
            Skill = skill,
            CriteriaResults = criteriaResults,
            Output = output,
            Timestamp = DateTimeOffset.UtcNow.ToString("o"),
        };
    }

    private static string GetBadAnalyticsMockResponse()
    {
        // allowed-tools 違反（curl 使用）と no-direct-db 違反（SQL直接実行）を含む
        return
            "# データ収集と保存\n\n" +
            "playwright-cli でページを開いた後、curl で API からデータを取得しました。\n\n" +
            "```bash\n" +
            "playwright-cli open --headed http://localhost:3002/\n" +
            "playwright-cli close\n" +
            "curl -X GET http://localhost:3002/api/tickets -o temp/collected-data.csv\n" +
            "```\n\n" +
            "次に SQLite に直接 INSERT しました。\n\n" +
            "```python\n" +
            "import sqlite3\n\n" +
            "conn = sqlite3.connect('src/DashboardApi/Data/dashboard.db')\n" +
            "cursor = conn.cursor()\n" +
            "cursor.execute('INSERT INTO tickets (id, title, status) VALUES (?, ?, ?)', ('001', 'テスト', 'done'))\n" +
            "conn.commit()\n" +
            "conn.close()\n" +
            "```\n\n" +
            "また SqliteConnection で直接 UPDATE しました。\n\n" +
            "```csharp\n" +
            "using var conn = new SqliteConnection(\"Data Source=dashboard.db\");\n" +
            "conn.Open();\n" +
            "conn.Execute(\"UPDATE tickets SET status = 'closed' WHERE id = '001'\");\n" +
            "```\n\n" +
            "使用ツール: playwright-cli, curl, python";
    }

    private static string GetWebAnalyticsMockResponse()
    {
        return
            "# タスク完了: web-analytics\n\n" +
            "## Step 1: データ収集チェック\n\n" +
            "temp/collected-data.csv が存在しないため、データ収集を開始します。\n" +
            "すでに存在する場合はスキップします。\n\n" +
            "```bash\n" +
            "node .github/skills/web-analytics/collect-data.js\n" +
            "```\n\n" +
            "playwright-cli でログインし、KPI データをエクスポートしました。\n\n" +
            "```bash\n" +
            "playwright-cli open http://localhost:3001/login\n" +
            "playwright-cli fill e10 \"admin\"\n" +
            "playwright-cli fill e13 \"password123\"\n" +
            "playwright-cli click e14\n" +
            "playwright-cli close\n" +
            "```\n\n" +
            "## Step 2: CSV 保存\n\n" +
            "5,521 件のレコードを temp/collected-data.csv に保存しました。\n\n" +
            "## Step 3: Jupyter Notebook 構築\n\n" +
            "src/kpi-analysis/kpi_analysis.ipynb を作成し、以下の分析を実施しました。\n\n" +
            "```python\n" +
            "import pandas as pd\n" +
            "import matplotlib.pyplot as plt\n" +
            "import matplotlib_fontja\n\n" +
            "df = pd.read_csv('../../temp/collected-data.csv')\n" +
            "df['Date'] = pd.to_datetime(df['Date'])\n" +
            "df['YearMonth'] = df['Date'].dt.to_period('M')\n\n" +
            "monthly = df.groupby('YearMonth')['Revenue'].sum()\n" +
            "monthly.plot(kind='bar')\n" +
            "plt.title('月次売上推移')\n" +
            "plt.savefig('src/kpi-analysis/01_revenue.png', dpi=120)\n" +
            "plt.show()\n" +
            "```\n\n" +
            "## Step 4: クリーンアップ\n\n" +
            "分析完了後、収集データを削除しました。\n\n" +
            "```bash\n" +
            "rm temp/collected-data.csv\n" +
            "```\n\n" +
            "✓ 完了: src/kpi-analysis/kpi_analysis.ipynb を作成しました。\n\n" +
            "使用ツール: playwright-cli, node";
    }

    private static string GetCsvImporterMockResponse()
    {
        return
            "# タスク完了: csv-importer\n\n" +
            "## Step 1: CSV ファイルの読み込み\n\n" +
            "temp/collected-data.csv を読み込みました。\n\n" +
            "## Step 2: API へのインポートリクエスト\n\n" +
            "POST /api/v1/import エンドポイントに curl でリクエストを送信しました。\n\n" +
            "```bash\n" +
            "CSV_CONTENT=$(cat \"temp/collected-data.csv\")\n" +
            "RESPONSE=$(curl -s -X POST http://localhost:5000/api/v1/import \\\n" +
            "  -H \"Content-Type: application/json\" \\\n" +
            "  -d \"{\\\"csvContent\\\": $(echo \\\"$CSV_CONTENT\\\" | python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()))')}\")\n" +
            "echo \"$RESPONSE\"\n" +
            "```\n\n" +
            "## Step 3: 結果の報告\n\n" +
            "レスポンス:\n\n" +
            "```json\n" +
            "{ \"data\": { \"rowCount\": 120, \"buIds\": [\"BU001\"], \"message\": \"120 件を取り込みました\" } }\n" +
            "```\n\n" +
            "| 項目 | 値 |\n" +
            "|------|-----|\n" +
            "| success | true |\n" +
            "| count | 120 |\n" +
            "| message | 120 件を取り込みました |\n\n" +
            "ファイルが見つかりません の場合は success: false、count: 0 を報告します。\n\n" +
            "使用ツール: ReadFile, curl";
    }

    private static string GenerateMockResponse(ParsedSkill skill)
    {
        // スキル固有のモックレスポンスを返す
        if (skill.Metadata.Name == "web-analytics")
            return GetWebAnalyticsMockResponse();
        if (skill.Metadata.Name == "bad-analytics")
            return GetBadAnalyticsMockResponse();
        if (skill.Metadata.Name == "csv-importer")
            return GetCsvImporterMockResponse();

        var name = skill.Metadata.Name;
        var tools = string.Join(", ", skill.Metadata.AllowedTools.DefaultIfEmpty("(none)"));
        return
            $"# タスク完了: {name}\n\n" +
            "スキルの指示に従い、タスクを実行しました。\n\n" +
            "```typescript\n" +
            $"// {name} の実装例\n" +
            "export async function execute(options: Options): Promise<Result> {\n" +
            "  console.log(\"Task executed successfully\");\n" +
            "  return { success: true };\n" +
            "}\n" +
            "```\n\n" +
            "## テスト\n\n" +
            "```typescript\n" +
            $"describe(\"{name}\", () => {{\n" +
            "  it(\"should execute successfully\", async () => {\n" +
            "    const result = await execute(testOptions);\n" +
            "    expect(result.success).toBe(true);\n" +
            "  });\n" +
            "});\n" +
            "```\n\n" +
            $"使用ツール: {tools}";
    }

    // -----------------------------------------------------------------------
    // Live モード
    // -----------------------------------------------------------------------

    private async Task<SkillTestResult> RunLiveTestAsync(
        ParsedSkill skill,
        SkillTestCase testCase,
        CancellationToken ct)
    {
        if (_client is null)
            throw new InvalidOperationException("InitializeAsync() を先に呼び出してください");

        var sw = Stopwatch.StartNew();
        var output = new SkillTestOutput();

        try
        {
            // AllowedTools が指定されている場合はセッションのツールを制限
            List<string>? availableTools = skill.Metadata.AllowedTools.Count > 0
                ? skill.Metadata.AllowedTools
                : null;

            var sessionConfig = new SessionConfig
            {
                Model = _config.Model,
                OnPermissionRequest = PermissionHandler.ApproveAll,
                SystemMessage = new SystemMessageConfig
                {
                    Mode = SystemMessageMode.Append,
                    Content = BuildSystemPrompt(skill),
                },
                AvailableTools = availableTools,
            };

            await using var session = await _client.CreateSessionAsync(sessionConfig, ct);

            var toolsUsed = new List<string>();
            var responseBuilder = new System.Text.StringBuilder();
            var done = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var timeout = new CancellationTokenSource(testCase.TimeoutMs);
            timeout.Token.Register(() => done.TrySetException(
                new TimeoutException($"テストがタイムアウトしました ({testCase.TimeoutMs}ms)")));

            using var _ = session.On(evt =>
            {
                switch (evt)
                {
                    case AssistantMessageEvent msg:
                        responseBuilder.Append(msg.Data.Content);
                        break;
                    case ToolExecutionStartEvent tool:
                        toolsUsed.Add(tool.Data.ToolName);
                        if (_config.Verbose)
                            Console.WriteLine($"    [tool] {tool.Data.ToolName}");
                        break;
                    case SessionIdleEvent:
                        done.TrySetResult(responseBuilder.ToString());
                        break;
                    case SessionErrorEvent err:
                        done.TrySetException(new Exception($"セッションエラー: {err.Data.Message}"));
                        break;
                }
            });

            await session.SendAsync(new MessageOptions { Prompt = testCase.Prompt }, ct);
            var response = await done.Task;

            sw.Stop();
            output.Response = response;
            output.ToolsUsed = toolsUsed;
            output.DurationMs = sw.ElapsedMilliseconds;
            output.Completed = true;
        }
        catch (Exception ex)
        {
            sw.Stop();
            output.Response = $"[エラー] {ex.Message}";
            output.DurationMs = sw.ElapsedMilliseconds;
            output.Completed = false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    警告: {ex.Message}");
            Console.ResetColor();
        }

        var criteriaResults = EvaluateCriteria(testCase.Criteria, output, skill);
        return new SkillTestResult
        {
            TestCase = testCase,
            Skill = skill,
            CriteriaResults = criteriaResults,
            Output = output,
            Timestamp = DateTimeOffset.UtcNow.ToString("o"),
        };
    }

    private static string BuildSystemPrompt(ParsedSkill skill)
    {
        return $"""

            <activated_skill>
              <name>{skill.Metadata.Name}</name>
              <description>{skill.Metadata.Description}</description>
              <instructions>
            {skill.Instructions}
              </instructions>
            </activated_skill>

            上記のスキル指示に従って作業してください。
            """;
    }

    // -----------------------------------------------------------------------
    // 受け入れ基準の評価
    // -----------------------------------------------------------------------

    private static List<CriterionResult> EvaluateCriteria(
        List<AcceptanceCriteria> criteria,
        SkillTestOutput output,
        ParsedSkill skill)
    {
        return criteria.Select(c => EvaluateSingle(c, output, skill)).ToList();
    }

    private static CriterionResult EvaluateSingle(
        AcceptanceCriteria criterion,
        SkillTestOutput output,
        ParsedSkill skill)
    {
        return criterion.Type switch
        {
            CriterionType.OutputContains => EvaluateOutputContains(criterion, output),
            CriterionType.NoExtraTools   => EvaluateNoExtraTools(criterion, output, skill),
            CriterionType.ContainsCode   => EvaluateContainsCode(criterion, output),
            CriterionType.TestPasses     => EvaluateTestPasses(criterion, output),
            CriterionType.NoDirectDb     => EvaluateNoDirectDb(criterion, output),
            _ => new CriterionResult
            {
                Criterion = criterion,
                Passed = false,
                Message = $"未知の基準タイプ: {criterion.Type}",
            },
        };
    }

    private static CriterionResult EvaluateOutputContains(
        AcceptanceCriteria criterion, SkillTestOutput output)
    {
        if (string.IsNullOrEmpty(criterion.Expected))
        {
            return new CriterionResult
            {
                Criterion = criterion,
                Passed = false,
                Message = "expected フィールドが必要です",
            };
        }

        var found = output.Response.Contains(criterion.Expected, StringComparison.OrdinalIgnoreCase);
        return new CriterionResult
        {
            Criterion = criterion,
            Passed = found,
            Message = found
                ? $"'{criterion.Expected}' が出力に含まれています"
                : $"'{criterion.Expected}' が出力に含まれていません",
        };
    }

    private static CriterionResult EvaluateNoExtraTools(
        AcceptanceCriteria criterion, SkillTestOutput output, ParsedSkill skill)
    {
        if (skill.Metadata.AllowedTools.Count == 0)
        {
            return new CriterionResult
            {
                Criterion = criterion,
                Passed = true,
                Message = "allowedTools が未設定のため制約なし",
            };
        }

        var extras = output.ToolsUsed
            .Where(t => !skill.Metadata.AllowedTools.Contains(t, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var passed = extras.Count == 0;
        return new CriterionResult
        {
            Criterion = criterion,
            Passed = passed,
            Message = passed
                ? $"許可されたツールのみ使用しました: {string.Join(", ", output.ToolsUsed.DefaultIfEmpty("(none)"))}"
                : $"許可外のツールが使用されました: {string.Join(", ", extras)}",
        };
    }

    private static CriterionResult EvaluateContainsCode(
        AcceptanceCriteria criterion, SkillTestOutput output)
    {
        var hasCode = Regex.IsMatch(output.Response, @"```\w*\n[\s\S]*?```");
        return new CriterionResult
        {
            Criterion = criterion,
            Passed = hasCode,
            Message = hasCode ? "コードブロックが含まれています" : "コードブロックが見つかりません",
        };
    }

    private static CriterionResult EvaluateTestPasses(
        AcceptanceCriteria criterion, SkillTestOutput output)
    {
        var hasTests = output.Response.Contains("describe(") ||
                       output.Response.Contains("it(") ||
                       output.Response.Contains("test(") ||
                       output.Response.Contains("expect(") ||
                       output.Response.Contains("[Test]") ||   // xUnit / MSTest
                       output.Response.Contains("[Fact]");     // xUnit

        return new CriterionResult
        {
            Criterion = criterion,
            Passed = hasTests,
            Message = hasTests ? "テストコード構造が検出されました" : "テストコード構造が見つかりません",
        };
    }

    private static CriterionResult EvaluateNoDirectDb(
        AcceptanceCriteria criterion, SkillTestOutput output)
    {
        // SQL や直接DBアクセスパターンを検出
        var directDbPatterns = new[] { "INSERT INTO", "UPDATE ", "DELETE FROM", "CREATE TABLE",
                                       "SqliteConnection", "SqlConnection", "DbContext" };

        var found = directDbPatterns.Any(p =>
            output.Response.Contains(p, StringComparison.OrdinalIgnoreCase));

        return new CriterionResult
        {
            Criterion = criterion,
            Passed = !found,
            Message = found
                ? "直接DBアクセスのパターンが検出されました（APIを経由してください）"
                : "直接DBアクセスは検出されませんでした",
        };
    }

    // -----------------------------------------------------------------------
    // ログ出力
    // -----------------------------------------------------------------------

    private static void LogResult(SkillTestResult result)
    {
        var status = result.Passed ? "✓ PASS" : "✗ FAIL";
        var color = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;

        Console.ForegroundColor = color;
        Console.WriteLine($"\n{status}: {result.TestCase.Description}");
        Console.ResetColor();

        foreach (var cr in result.CriteriaResults)
        {
            var icon = cr.Passed ? "  ✓" : "  ✗";
            Console.ForegroundColor = cr.Passed ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write(icon);
            Console.ResetColor();
            Console.WriteLine($" {cr.Criterion.Description}: {cr.Message}");
        }
    }
}
