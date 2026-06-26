using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cali.Core.Models;

public sealed class SeedData
{
    [JsonPropertyName("exercises")] public List<SeedExercise> Exercises { get; set; } = new();
    [JsonPropertyName("predefinedPlans")] public List<SeedPlan> PredefinedPlans { get; set; } = new();
    [JsonPropertyName("achievements")] public List<SeedAchievement> Achievements { get; set; } = new();
    [JsonPropertyName("sampleHistory")] public List<SeedHistory> SampleHistory { get; set; } = new();
    [JsonPropertyName("sampleProfile")] public SeedProfile? SampleProfile { get; set; }
    [JsonPropertyName("defaultSettings")] public SeedSettings? DefaultSettings { get; set; }

    public static SeedData Parse(string json) =>
        JsonSerializer.Deserialize<SeedData>(json, Options) ?? new SeedData();

    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}

public sealed class SeedExercise
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("primary")] public string Primary { get; set; } = "";
    [JsonPropertyName("secondary")] public string Secondary { get; set; } = "";
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = "Beginner";
    [JsonPropertyName("equipment")] public string Equipment { get; set; } = "none";
    [JsonPropertyName("type")] public string Type { get; set; } = "reps";
    [JsonPropertyName("easierId")] public string EasierId { get; set; } = "";
    [JsonPropertyName("harderId")] public string HarderId { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("cues")] public List<string> Cues { get; set; } = new();
    [JsonPropertyName("mistakes")] public List<string> Mistakes { get; set; } = new();

    public Exercise ToExercise() => new()
    {
        Id = Id,
        Name = Name,
        Primary = Primary,
        Secondary = Secondary,
        Difficulty = Enum.Parse<Difficulty>(Difficulty, ignoreCase: true),
        Equipment = Equipment,
        Type = Type.Equals("hold", StringComparison.OrdinalIgnoreCase) ? ExerciseType.Hold : ExerciseType.Reps,
        EasierId = EasierId,
        HarderId = HarderId,
        Family = ExerciseFamilies.Of(Id),
        Description = Description,
        Cues = Cues,
        Mistakes = Mistakes
    };
}

public sealed class SeedPlan
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("preset")] public bool Preset { get; set; }
    [JsonPropertyName("difficulty")] public string Difficulty { get; set; } = "Beginner";
    [JsonPropertyName("estMinutes")] public int EstMinutes { get; set; }
    [JsonPropertyName("muscles")] public string Muscles { get; set; } = "";
    [JsonPropertyName("exercises")] public List<SeedPlanExercise> Exercises { get; set; } = new();
}

public sealed class SeedPlanExercise
{
    [JsonPropertyName("exerciseId")] public string ExerciseId { get; set; } = "";
    [JsonPropertyName("sets")] public int Sets { get; set; }
    [JsonPropertyName("reps")] public int? Reps { get; set; }
    [JsonPropertyName("holdSec")] public int? HoldSec { get; set; }
    [JsonPropertyName("restSec")] public int RestSec { get; set; }
}

public sealed class SeedAchievement
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("icon")] public string Icon { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public sealed class SeedHistory
{
    [JsonPropertyName("date")] public string Date { get; set; } = "";
    [JsonPropertyName("planName")] public string PlanName { get; set; } = "";
    [JsonPropertyName("durationSec")] public int DurationSec { get; set; }
    [JsonPropertyName("exercises")] public int Exercises { get; set; }
    [JsonPropertyName("effort")] public string Effort { get; set; } = "Just right";
}

public sealed class SeedProfile
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("goal")] public string Goal { get; set; } = "";
    [JsonPropertyName("level")] public string Level { get; set; } = "";
    [JsonPropertyName("bodyweightKg")] public int BodyweightKg { get; set; }
    [JsonPropertyName("maxPullups")] public int MaxPullups { get; set; }
    [JsonPropertyName("currentStreakDays")] public int CurrentStreakDays { get; set; }
    [JsonPropertyName("bestStreakDays")] public int BestStreakDays { get; set; }
}

public sealed class SeedSettings
{
    [JsonPropertyName("soundAlerts")] public bool SoundAlerts { get; set; } = true;
    [JsonPropertyName("haptics")] public bool Haptics { get; set; } = true;
    [JsonPropertyName("autoStartRest")] public bool AutoStartRest { get; set; } = true;
    [JsonPropertyName("autoProgression")] public bool AutoProgression { get; set; }
    [JsonPropertyName("reminders")] public bool Reminders { get; set; } = true;
    [JsonPropertyName("units")] public string Units { get; set; } = "metric";
}
