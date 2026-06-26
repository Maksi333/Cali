namespace Cali.Core.Models;

public sealed class Exercise
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Primary { get; set; } = "";
    public string Secondary { get; set; } = "";
    public Difficulty Difficulty { get; set; }
    public string Equipment { get; set; } = "none";
    public ExerciseType Type { get; set; }
    public string EasierId { get; set; } = "";
    public string HarderId { get; set; } = "";
    public string Family { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Cues { get; set; } = new();
    public List<string> Mistakes { get; set; } = new();
}
