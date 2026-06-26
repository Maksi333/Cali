namespace Cali.Core.Models;

/// <summary>Coarse movement family per exercise — powers volume-by-family and feat achievements.</summary>
public static class ExerciseFamilies
{
    private static readonly Dictionary<string, string> Map = new()
    {
        ["pushup"] = "pushup", ["incline"] = "pushup", ["diamond"] = "pushup", ["pike"] = "pushup",
        ["dips"] = "dip",
        ["pullup"] = "pullup", ["chinup"] = "pullup", ["rows"] = "pullup", ["muscleup"] = "pullup",
        ["squat"] = "squat", ["lunge"] = "squat", ["pistol"] = "squat",
        ["plank"] = "plank",
        ["hollow"] = "core", ["legraise"] = "core", ["mtnclimber"] = "core",
        // burpee, handstand, glutebridge: no family
    };

    public static string Of(string exerciseId) => Map.TryGetValue(exerciseId, out var f) ? f : "";
}
