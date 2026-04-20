using ConsoleAppFramework;
using SkillHarness.Services;
using SkillHarness.Models;

var app = ConsoleApp.Create();

// ─────────────────────────────────────────────
// test コマンド: Skill のテストを実行する
// ─────────────────────────────────────────────
app.Add("test", async (
    /// <summary>SKILL.md へのパス</summary>
    string skillPath,
    /// <summary>実行モード: mock (デフォルト) または live</summary>
    string mode = "mock",
    /// <summary>テストケース JSON ファイルのパス (省略時は SKILL.md と同じディレクトリの test-cases.json を使用)</summary>
    string? testCasesPath = null,
    /// <summary>詳細ログを出力する</summary>
    bool verbose = false,
    CancellationToken cancellationToken = default) =>
{
    Console.WriteLine("=== Skill Test Harness ===");
    Console.WriteLine($"Skill:   {skillPath}");
    Console.WriteLine($"Mode:    {mode}");
    Console.WriteLine();

    // SKILL.md のパース
    ParsedSkill skill;
    try
    {
        skill = await SkillParser.ParseAsync(skillPath);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"✗ SKILL.md のパースに失敗しました: {ex.Message}");
        Console.ResetColor();
        return;
    }

    Console.WriteLine($"Name:    {skill.Metadata.Name}");
    Console.WriteLine($"Desc:    {skill.Metadata.Description}");
    if (skill.Metadata.AllowedTools.Count > 0)
        Console.WriteLine($"Tools:   {string.Join(", ", skill.Metadata.AllowedTools)}");
    Console.WriteLine();

    // テストケースの読み込み
    var tcPath = testCasesPath
        ?? SkillParser.FindDefaultTestCasesPath(skillPath);

    if (tcPath is null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Error.WriteLine("テストケースファイルが見つかりません。");
        Console.Error.WriteLine("SKILL.md と同じディレクトリに test-cases.json を配置するか、");
        Console.Error.WriteLine("--test-cases-path オプションでパスを指定してください。");
        Console.ResetColor();
        return;
    }

    List<SkillTestCase> testCases;
    try
    {
        testCases = await SkillParser.LoadTestCasesAsync(tcPath);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"✗ テストケースの読み込みに失敗しました: {ex.Message}");
        Console.ResetColor();
        return;
    }

    Console.WriteLine($"テストケース数: {testCases.Count}");
    foreach (var tc in testCases)
        Console.WriteLine($"  [{tc.Id}] {tc.Description} ({tc.Criteria.Count} 基準)");
    Console.WriteLine();

    // ハーネスの初期化と実行
    var testMode = mode.Equals("live", StringComparison.OrdinalIgnoreCase)
        ? TestMode.Live
        : TestMode.Mock;

    await using var harness = new SkillTestHarness(new SkillTestHarnessConfig
    {
        Mode = testMode,
        Verbose = verbose,
    });

    try
    {
        await harness.InitializeAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"✗ ハーネスの初期化に失敗しました: {ex.Message}");
        Console.ResetColor();
        return;
    }

    var results = await harness.RunTestSuiteAsync(skill, testCases, cancellationToken);

    // サマリー出力
    var passed = results.Count(r => r.Passed);
    var failed = results.Count - passed;

    Console.WriteLine();
    Console.WriteLine("=== テスト結果 ===");

    foreach (var result in results)
    {
        var icon = result.Passed ? "✓" : "✗";
        Console.ForegroundColor = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"\n{icon} [{result.TestCase.Id}] {result.TestCase.Description}");
        Console.ResetColor();
        Console.WriteLine($"  実行時間: {result.Output.DurationMs}ms");

        foreach (var cr in result.CriteriaResults)
        {
            var crIcon = cr.Passed ? "  ✓" : "  ✗";
            Console.ForegroundColor = cr.Passed ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write(crIcon);
            Console.ResetColor();
            Console.WriteLine($" {cr.Criterion.Description}: {cr.Message}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("=== サマリー ===");
    Console.WriteLine($"合計:   {results.Count}");

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"成功:   {passed}");
    Console.ResetColor();

    if (failed > 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"失敗:   {failed}");
        Console.ResetColor();
    }
    else
    {
        Console.WriteLine($"失敗:   {failed}");
    }
});

// ─────────────────────────────────────────────
// validate コマンド: SKILL.md の構文を検証する
// ─────────────────────────────────────────────
app.Add("validate", async (
    /// <summary>SKILL.md へのパス</summary>
    string skillPath) =>
{
    try
    {
        var skill = await SkillParser.ParseAsync(skillPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ SKILL.md は有効です");
        Console.ResetColor();
        Console.WriteLine($"  Name:          {skill.Metadata.Name}");
        Console.WriteLine($"  Description:   {skill.Metadata.Description}");
        if (skill.Metadata.AllowedTools.Count > 0)
            Console.WriteLine($"  AllowedTools:  {string.Join(", ", skill.Metadata.AllowedTools)}");
        Console.WriteLine($"  Instructions:  {skill.Instructions.Length} 文字");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"✗ 無効: {ex.Message}");
        Console.ResetColor();
    }
});

await app.RunAsync(args);
