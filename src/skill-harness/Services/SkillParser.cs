using System.Text.Json;
using SkillHarness.Models;

namespace SkillHarness.Services;

/// <summary>
/// SKILL.md ファイルをパースして ParsedSkill を返す
/// </summary>
public static class SkillParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public static async Task<ParsedSkill> ParseAsync(string skillPath)
    {
        if (!File.Exists(skillPath))
            throw new FileNotFoundException($"SKILL.md が見つかりません: {skillPath}");

        var content = await File.ReadAllTextAsync(skillPath);
        return Parse(content, skillPath);
    }

    public static ParsedSkill Parse(string content, string path)
    {
        // --- フロントマター抽出 ---
        var (frontmatter, instructions) = ExtractFrontmatter(content);
        var metadata = ParseFrontmatter(frontmatter);

        if (string.IsNullOrWhiteSpace(metadata.Name))
            throw new InvalidOperationException("SKILL.md の frontmatter に 'name' フィールドが必要です");
        if (string.IsNullOrWhiteSpace(metadata.Description))
            throw new InvalidOperationException("SKILL.md の frontmatter に 'description' フィールドが必要です");

        return new ParsedSkill
        {
            Metadata = metadata,
            Instructions = instructions.Trim(),
            Path = path,
        };
    }

    private static (string Frontmatter, string Body) ExtractFrontmatter(string content)
    {
        content = content.TrimStart();
        if (!content.StartsWith("---"))
            return (string.Empty, content);

        var firstEnd = content.IndexOf('\n', 3);
        if (firstEnd < 0)
            return (string.Empty, content);

        var secondStart = content.IndexOf("\n---", firstEnd);
        if (secondStart < 0)
            return (string.Empty, content);

        var frontmatter = content.Substring(firstEnd + 1, secondStart - firstEnd - 1);
        var body = content.Substring(secondStart + 4).TrimStart('\r', '\n');
        return (frontmatter, body);
    }

    private static SkillMetadata ParseFrontmatter(string yaml)
    {
        var name = string.Empty;
        var description = string.Empty;
        string? license = null;
        string? compatibility = null;
        var allowedTools = new List<string>();

        foreach (var line in yaml.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#') || string.IsNullOrWhiteSpace(trimmed))
                continue;

            var colonIdx = trimmed.IndexOf(':');
            if (colonIdx < 0) continue;

            var key = trimmed[..colonIdx].Trim().ToLowerInvariant();
            var value = trimmed[(colonIdx + 1)..].Trim().Trim('"', '\'');

            switch (key)
            {
                case "name":
                    name = value;
                    break;
                case "description":
                    description = value;
                    break;
                case "license":
                    license = value;
                    break;
                case "compatibility":
                    compatibility = value;
                    break;
                case "allowed-tools":
                case "allowedtools":
                    // スペース区切り or カンマ区切り
                    allowedTools.AddRange(
                        value.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries));
                    break;
            }
        }

        return new SkillMetadata
        {
            Name = name,
            Description = description,
            License = license,
            Compatibility = compatibility,
            AllowedTools = allowedTools,
        };
    }

    /// <summary>
    /// テストケース JSON ファイルを読み込む
    /// </summary>
    public static async Task<List<SkillTestCase>> LoadTestCasesAsync(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"テストケースファイルが見つかりません: {jsonPath}");

        var json = await File.ReadAllTextAsync(jsonPath);
        return JsonSerializer.Deserialize<List<SkillTestCase>>(json, JsonOptions)
               ?? throw new InvalidOperationException("テストケース JSON のデシリアライズに失敗しました");
    }

    /// <summary>
    /// SKILL.md と同じディレクトリの test-cases.json を自動検索
    /// </summary>
    public static string? FindDefaultTestCasesPath(string skillPath)
    {
        var dir = Path.GetDirectoryName(skillPath);
        if (dir is null) return null;

        var candidate = Path.Combine(dir, "test-cases.json");
        return File.Exists(candidate) ? candidate : null;
    }
}
