namespace Cali.Core.Models;

/// <summary>A typed unlock rule. Only the fields relevant to <see cref="Type"/> are used.</summary>
public sealed class Criteria
{
    public required string Type { get; init; }
    public int Count { get; init; }
    public int Days { get; init; }
    public string Family { get; init; } = "";
    public int Seconds { get; init; }
    public int Minutes { get; init; }
    public int MinExercises { get; init; }
    public int Hour { get; init; }
    public int StartHour { get; init; }
    public int EndHour { get; init; }
    public string SkillId { get; init; } = "";
    public string[] Modes { get; init; } = Array.Empty<string>();
}
