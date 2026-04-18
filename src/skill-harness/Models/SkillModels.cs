namespace SkillHarness.Models;

/// <summary>
/// SKILL.md のフロントマターから取得したメタデータ
/// </summary>
public sealed class SkillMetadata
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? License { get; init; }
    public string? Compatibility { get; init; }
    public List<string> AllowedTools { get; init; } = [];
}

/// <summary>
/// パース済み Skill（メタデータ + インストラクション）
/// </summary>
public sealed class ParsedSkill
{
    public SkillMetadata Metadata { get; init; } = new();
    public string Instructions { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
}
