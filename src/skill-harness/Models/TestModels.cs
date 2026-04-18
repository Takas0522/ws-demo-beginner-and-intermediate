using System.Text.Json.Serialization;

namespace SkillHarness.Models;

/// <summary>
/// 受け入れ基準の種別
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CriterionType
{
    /// <summary>レスポンスに期待する文字列が含まれるか</summary>
    OutputContains,
    /// <summary>AllowedTools 以外のツールが使われていないか</summary>
    NoExtraTools,
    /// <summary>レスポンスにコードブロックが含まれるか</summary>
    ContainsCode,
    /// <summary>レスポンスにテストコード構造が含まれるか</summary>
    TestPasses,
    /// <summary>直接DBアクセスが行われていないか</summary>
    NoDirectDb,
}

/// <summary>
/// 受け入れ基準の定義
/// </summary>
public sealed class AcceptanceCriteria
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CriterionType Type { get; init; }
    /// <summary>output_contains で使用する期待値（部分一致）</summary>
    public string? Expected { get; init; }
}

/// <summary>
/// テストケースの定義
/// </summary>
public sealed class SkillTestCase
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    /// <summary>エージェントに送るプロンプト</summary>
    public string Prompt { get; init; } = string.Empty;
    public List<AcceptanceCriteria> Criteria { get; init; } = [];
    /// <summary>タイムアウト (ms)。デフォルト 60000</summary>
    public int TimeoutMs { get; init; } = 60_000;
}

/// <summary>
/// テスト実行の出力データ
/// </summary>
public sealed class SkillTestOutput
{
    public string Response { get; set; } = string.Empty;
    public List<string> ToolsUsed { get; set; } = [];
    public long DurationMs { get; set; }
    public bool Completed { get; set; }
}

/// <summary>
/// 単一の受け入れ基準の評価結果
/// </summary>
public sealed class CriterionResult
{
    public AcceptanceCriteria Criterion { get; init; } = default!;
    public bool Passed { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// 1 テストケースの全体結果
/// </summary>
public sealed class SkillTestResult
{
    public SkillTestCase TestCase { get; init; } = default!;
    public ParsedSkill Skill { get; init; } = default!;
    public List<CriterionResult> CriteriaResults { get; init; } = [];
    public bool Passed => CriteriaResults.Count > 0 && CriteriaResults.All(r => r.Passed);
    public SkillTestOutput Output { get; init; } = new();
    public string Timestamp { get; init; } = DateTimeOffset.UtcNow.ToString("o");
}
