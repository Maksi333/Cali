namespace Cali.Core.Models;

/// <summary>Coarse movement family per exercise — powers volume-by-family and feat achievements.</summary>
public static class ExerciseFamilies
{
    private static readonly Dictionary<string, string> Map = new()
    {
        // ---- Push (horizontal + vertical presses all share the push-up volume family) ----
        ["pushup"] = "pushup", ["incline"] = "pushup", ["diamond"] = "pushup", ["pike"] = "pushup",
        ["widepushup"] = "pushup", ["kneepushup"] = "pushup", ["wallpushup"] = "pushup", ["declinepushup"] = "pushup",
        ["archerpushup"] = "pushup", ["clappushup"] = "pushup", ["onearmpushup"] = "pushup", ["spidermanpushup"] = "pushup",
        ["hindupushup"] = "pushup", ["staggeredpushup"] = "pushup", ["hspu"] = "pushup", ["elevatedpike"] = "pushup",
        ["sphinxpushup"] = "pushup",
        // ---- Dip ----
        ["dips"] = "dip", ["benchdips"] = "dip", ["koreandips"] = "dip",
        // ---- Pull ----
        ["pullup"] = "pullup", ["chinup"] = "pullup", ["rows"] = "pullup", ["muscleup"] = "pullup",
        ["widepullup"] = "pullup", ["negativepullup"] = "pullup", ["scapularpullup"] = "pullup",
        ["archerpullup"] = "pullup", ["commandopullup"] = "pullup", ["towelrows"] = "pullup",
        // ---- Squat / lunge / lower-body push ----
        ["squat"] = "squat", ["lunge"] = "squat", ["pistol"] = "squat",
        ["jumpsquat"] = "squat", ["bulgariansplit"] = "squat", ["stepup"] = "squat", ["reverselunge"] = "squat",
        ["walkinglunge"] = "squat", ["cossacksquat"] = "squat", ["shrimpsquat"] = "squat", ["boxjump"] = "squat",
        ["broadjump"] = "squat", ["skaterhops"] = "squat", ["curtsylunge"] = "squat",
        // ---- Plank ----
        ["plank"] = "plank",
        // ---- Core ----
        ["hollow"] = "core", ["legraise"] = "core", ["mtnclimber"] = "core",
        ["russiantwist"] = "core", ["bicyclecrunch"] = "core", ["crunch"] = "core", ["situp"] = "core",
        ["vup"] = "core", ["flutterkick"] = "core", ["hangingkneeraise"] = "core", ["hanginglegraise"] = "core",
        ["toestobar"] = "core", ["dragonflag"] = "core", ["birddog"] = "core", ["deadbug"] = "core",
        ["reversecrunch"] = "core", ["windshieldwiper"] = "core", ["planktap"] = "core", ["hollowrock"] = "core",
        // No family (isometric holds power per-exercise feats; isolation / posterior-chain / cardio carry no volume family):
        //   deadhang, wallsit, lsit, sideplank, crowpose, pseudoplanche, wallwalk, tricepext, superman, revsnowangel,
        //   skinthecat, calfraise, singlecalfraise, nordiccurl, singlegltbridge, hipthrust, glutebridge, burpee,
        //   handstand, bearcrawl, crabwalk, inchworm, jumpingjack, highknee, squatthrust, plankjacks
    };

    public static string Of(string exerciseId) => Map.TryGetValue(exerciseId, out var f) ? f : "";
}
