# Cali — Phase 1: Foundation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A runnable, dark-themed MAUI app shell with 5 empty tabs, Barlow fonts, the full design-token palette, a `Cali.Core` library holding the data model + SQLite/Preferences-backed services, and a seeded local database — all covered by unit tests.

**Architecture:** Three projects. `Cali.Core` (`net10.0` class library) holds platform-agnostic models and services (settings, seed, repositories) behind small abstractions (`IKeyValueStore`, `ISeedDataProvider`, `ITicker`). `Cali` (the existing MAUI single-project app) holds UI + platform implementations of those abstractions and references Core. `Cali.Tests` (`net10.0` xUnit) references Core and tests the logic against a temp-file SQLite DB and in-memory fakes. This keeps service logic fast-testable and the MAUI project focused on UI.

**Tech Stack:** .NET 10, .NET MAUI, CommunityToolkit.Mvvm, CommunityToolkit.Maui, sqlite-net-pcl, xUnit, Barlow / Barlow Condensed (Google Fonts OFL).

## Global Constraints

- Target frameworks for `Cali`: `net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0` (unchanged from scaffold). `Cali.Core` and `Cali.Tests`: `net10.0`.
- Dark theme only: `UserAppTheme = AppTheme.Dark`, dark status bar.
- Design tokens are exact hex values from `design_handoff_calisthenics_app/README.md` "Design Tokens" — copy verbatim (e.g. `accent = #FF6B1A`, `bg = #0D0E11`).
- `seed-data.json` is the data source of truth: **18 exercises, 6 preset plans, 3 sample history records, 1 sample profile, 5 achievements, default settings**. Bundle it as a MAUI raw asset.
- Fonts come from the Google Fonts OFL repo (`https://github.com/google/fonts/raw/main/ofl/barlow/...` and `.../barlowcondensed/...`); files: `Barlow-Regular/Medium/SemiBold/Bold.ttf`, `BarlowCondensed-SemiBold/Bold.ttf`.
- Primary verification target: **Android emulator** (`Medium_Phone_API_36.0` or `pixel_7_-_api_36_0` already exist on this machine).
- **Git:** this folder is not a git repo and the user has not authorized commits. Each task's final "Checkpoint" step is a build/test-green gate, **not** a commit. If the user runs `git init` and authorizes commits, the checkpoint becomes `git add … && git commit …`.

---

## File Structure

**New project `Cali.Core/`** (`net10.0` library):
```
Cali.Core/Cali.Core.csproj
Models/Enums.cs                 Difficulty, ExerciseType, Effort, Goal, MuscleGroup (string-backed where seed uses free text)
Models/Exercise.cs              Exercise (in-memory, from seed JSON)
Models/Plan.cs                  Plan (SQLite table) + PlanExercise (SQLite table)
Models/WorkoutRecord.cs         WorkoutRecord (SQLite table)
Models/PersonalRecord.cs        PersonalRecord (SQLite table)
Models/Achievement.cs           Achievement (from seed + unlocked flag)
Models/Profile.cs               Profile (Preferences-backed POCO)
Models/AppSettings.cs           AppSettings (Preferences-backed POCO)
Models/SeedData.cs              SeedData root DTO mirroring seed-data.json
Abstractions/IKeyValueStore.cs  get/set string,bool,int (settings/preferences)
Abstractions/ISeedDataProvider.cs  Task<string> ReadSeedJsonAsync()
Abstractions/ITicker.cs         1-second tick abstraction (for the hero timer, Phase 2)
Services/IDatabase.cs           SQLiteAsyncConnection wrapper + InitAsync
Services/SettingsService.cs     AppSettings/Profile/streak/onboarding flag over IKeyValueStore
Services/ExerciseRepository.cs  loads exercises from seed JSON into memory; query helpers
Services/PlanRepository.cs      SQLite CRUD + preset seeding + duplicate
Services/HistoryRepository.cs   SQLite add/query + aggregates
Services/PrRepository.cs        SQLite upsert-if-beaten + get
Services/SeedService.cs         orchestrates first-launch seeding (idempotent)
```

**New project `Cali.Tests/`** (`net10.0` xUnit):
```
Cali.Tests/Cali.Tests.csproj
Fakes/InMemoryKeyValueStore.cs
Fakes/FileSeedDataProvider.cs   reads the real design_handoff seed-data.json by path
ModelsTests.cs, SettingsServiceTests.cs, ExerciseRepositoryTests.cs,
PlanRepositoryTests.cs, HistoryRepositoryTests.cs, PrRepositoryTests.cs, SeedServiceTests.cs
```

**Modified MAUI project `Cali/`:**
```
Cali.csproj                     + project ref to Cali.Core; + NuGet packages; bundle seed-data.json as Raw; add fonts
MauiProgram.cs                  register CommunityToolkit, fonts, DI (services + platform impls + pages/VMs)
App.xaml.cs                     UserAppTheme = Dark
AppShell.xaml                   TabBar with 5 tabs
Platform/PreferencesKeyValueStore.cs   IKeyValueStore over Microsoft.Maui.Storage.Preferences
Platform/AppPackageSeedDataProvider.cs ISeedDataProvider over FileSystem.OpenAppPackageFileAsync
Platform/DispatcherTicker.cs    ITicker over IDispatcherTimer
Resources/Styles/Colors.xaml    replace template palette with design tokens
Resources/Styles/Styles.xaml    base styles (page background, label defaults, tab bar)
Resources/Fonts/Barlow*.ttf, BarlowCondensed*.ttf   downloaded
Resources/Raw/seed-data.json    copied from design_handoff
Views/HomePage.xaml, PlansPage.xaml, ExercisesPage.xaml, ProgressPage.xaml, ProfilePage.xaml  (empty placeholders, themed)
```

---

## Task 1: `Cali.Core` project + models + seed deserialization

**Files:**
- Create: `Cali.Core/Cali.Core.csproj`, `Cali.Core/Models/Enums.cs`, `Models/Exercise.cs`, `Models/Plan.cs`, `Models/WorkoutRecord.cs`, `Models/PersonalRecord.cs`, `Models/Achievement.cs`, `Models/Profile.cs`, `Models/AppSettings.cs`, `Models/SeedData.cs`
- Create: `Cali.Tests/Cali.Tests.csproj`, `Cali.Tests/Fakes/FileSeedDataProvider.cs`, `Cali.Tests/ModelsTests.cs`
- Modify: `Cali.slnx` (add the two projects)

**Interfaces:**
- Produces: `SeedData` DTO with `Exercises: List<Exercise>`, `PredefinedPlans: List<SeedPlan>`, `Achievements`, `SampleHistory`, `SampleProfile`, `DefaultSettings`; `Exercise { string Id,Name,Primary,Secondary; Difficulty Difficulty; string Equipment; ExerciseType Type; string EasierId,HarderId,Description; List<string> Cues,Mistakes }`. Consumed by every later task.

- [ ] **Step 1: Create the Core csproj**

```xml
<!-- Cali.Core/Cali.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>Cali.Core</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
    <PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.10" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Write enums and models**

```csharp
// Cali.Core/Models/Enums.cs
namespace Cali.Core.Models;

public enum Difficulty { Beginner, Intermediate, Advanced }
public enum ExerciseType { Reps, Hold }
public enum Effort { TooEasy, JustRight, TooHard }
```

```csharp
// Cali.Core/Models/Exercise.cs
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
    public string Description { get; set; } = "";
    public List<string> Cues { get; set; } = new();
    public List<string> Mistakes { get; set; } = new();
}
```

```csharp
// Cali.Core/Models/Plan.cs
using SQLite;
namespace Cali.Core.Models;

[Table("Plans")]
public sealed class Plan
{
    [PrimaryKey] public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Preset { get; set; }
    public Difficulty Difficulty { get; set; }
    public int EstMinutes { get; set; }
    public string Muscles { get; set; } = "";
    [Ignore] public List<PlanExercise> Exercises { get; set; } = new();
}

[Table("PlanExercises")]
public sealed class PlanExercise
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public string PlanId { get; set; } = "";
    public string ExerciseId { get; set; } = "";
    public int Order { get; set; }
    public int Sets { get; set; }
    public int? Reps { get; set; }
    public int? HoldSec { get; set; }
    public int RestSec { get; set; }
}
```

```csharp
// Cali.Core/Models/WorkoutRecord.cs
using SQLite;
namespace Cali.Core.Models;

[Table("WorkoutRecords")]
public sealed class WorkoutRecord
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public DateTime DateUtc { get; set; }
    public string PlanName { get; set; } = "";
    public int DurationSec { get; set; }
    public int ExerciseCount { get; set; }
    public int TotalSets { get; set; }
    public int TotalReps { get; set; }
    public Effort Effort { get; set; }
}
```

```csharp
// Cali.Core/Models/PersonalRecord.cs
using SQLite;
namespace Cali.Core.Models;

[Table("PersonalRecords")]
public sealed class PersonalRecord
{
    [PrimaryKey] public string ExerciseId { get; set; } = "";
    public int? MaxReps { get; set; }
    public int? LongestHoldSec { get; set; }
    public DateTime AchievedUtc { get; set; }
}
```

```csharp
// Cali.Core/Models/Achievement.cs
namespace Cali.Core.Models;

public sealed class Achievement
{
    public string Id { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Unlocked { get; set; }
}
```

```csharp
// Cali.Core/Models/Profile.cs
namespace Cali.Core.Models;

public sealed class Profile
{
    public string Name { get; set; } = "Athlete";
    public string Goal { get; set; } = "Strength";
    public string Level { get; set; } = "Beginner";
    public int BodyweightKg { get; set; }
    public int MaxPullups { get; set; }
}
```

```csharp
// Cali.Core/Models/AppSettings.cs
namespace Cali.Core.Models;

public sealed class AppSettings
{
    public bool SoundAlerts { get; set; } = true;
    public bool Haptics { get; set; } = true;
    public bool AutoStartRest { get; set; } = true;
    public bool AutoProgression { get; set; }
    public bool Reminders { get; set; } = true;
    public string Units { get; set; } = "metric";
}
```

```csharp
// Cali.Core/Models/SeedData.cs
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
        Id = Id, Name = Name, Primary = Primary, Secondary = Secondary,
        Difficulty = Enum.Parse<Difficulty>(Difficulty, ignoreCase: true),
        Equipment = Equipment,
        Type = Type.Equals("hold", StringComparison.OrdinalIgnoreCase) ? ExerciseType.Hold : ExerciseType.Reps,
        EasierId = EasierId, HarderId = HarderId, Description = Description,
        Cues = Cues, Mistakes = Mistakes
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
```

- [ ] **Step 3: Create the test project + seed-file fake**

```xml
<!-- Cali.Tests/Cali.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Cali.Core\Cali.Core.csproj" />
  </ItemGroup>
</Project>
```

```csharp
// Cali.Tests/Fakes/FileSeedDataProvider.cs
using Cali.Core.Abstractions;
namespace Cali.Tests.Fakes;

public sealed class FileSeedDataProvider : ISeedDataProvider
{
    // Resolve the real handoff seed file relative to the repo root.
    public static string SeedPath
    {
        get
        {
            var dir = AppContext.BaseDirectory;
            while (dir is not null && !File.Exists(Path.Combine(dir, "design_handoff_calisthenics_app", "seed-data.json")))
                dir = Directory.GetParent(dir)?.FullName;
            if (dir is null) throw new FileNotFoundException("seed-data.json not found walking up from " + AppContext.BaseDirectory);
            return Path.Combine(dir, "design_handoff_calisthenics_app", "seed-data.json");
        }
    }
    public Task<string> ReadSeedJsonAsync() => File.ReadAllTextAsync(SeedPath);
}
```

> Note: `ISeedDataProvider` is created in Task 3 below. To keep Task 1 compiling on its own, define the one-method interface now in `Cali.Core/Abstractions/ISeedDataProvider.cs`:
> ```csharp
> namespace Cali.Core.Abstractions;
> public interface ISeedDataProvider { Task<string> ReadSeedJsonAsync(); }
> ```

- [ ] **Step 4: Write the failing model test**

```csharp
// Cali.Tests/ModelsTests.cs
using Cali.Core.Models;
using Cali.Tests.Fakes;
using Xunit;

public class ModelsTests
{
    [Fact]
    public async Task SeedData_parses_expected_counts_and_links()
    {
        var json = await new FileSeedDataProvider().ReadSeedJsonAsync();
        var seed = SeedData.Parse(json);

        Assert.Equal(18, seed.Exercises.Count);
        Assert.Equal(6, seed.PredefinedPlans.Count);
        Assert.Equal(3, seed.SampleHistory.Count);
        Assert.Equal(5, seed.Achievements.Count);

        var pushup = seed.Exercises.Single(e => e.Id == "pushup").ToExercise();
        Assert.Equal(ExerciseType.Reps, pushup.Type);
        Assert.Equal(Difficulty.Beginner, pushup.Difficulty);
        Assert.Equal("incline", pushup.EasierId);
        Assert.Equal("diamond", pushup.HarderId);

        var plank = seed.Exercises.Single(e => e.Id == "plank").ToExercise();
        Assert.Equal(ExerciseType.Hold, plank.Type);

        // every easier/harder id resolves to a real exercise
        var ids = seed.Exercises.Select(e => e.Id).ToHashSet();
        foreach (var e in seed.Exercises)
        {
            Assert.Contains(e.EasierId, ids);
            Assert.Contains(e.HarderId, ids);
        }
    }
}
```

- [ ] **Step 5: Add both projects to the solution**

```xml
<!-- Cali.slnx -->
<Solution>
  <Project Path="Cali.csproj" />
  <Project Path="Cali.Core/Cali.Core.csproj" />
  <Project Path="Cali.Tests/Cali.Tests.csproj" />
</Solution>
```

- [ ] **Step 6: Run the test — expect PASS**

Run: `dotnet test Cali.Tests/Cali.Tests.csproj`
Expected: `ModelsTests.SeedData_parses_expected_counts_and_links` PASSES (it exercises real parsing, no extra impl needed). If the count assertions fail, the seed file — not the test — is the source of truth; reconcile the numbers.

- [ ] **Step 7: Checkpoint** — `dotnet build Cali.Core/Cali.Core.csproj` and `dotnet test Cali.Tests/Cali.Tests.csproj` both green.

---

## Task 2: `SettingsService` over `IKeyValueStore`

**Files:**
- Create: `Cali.Core/Abstractions/IKeyValueStore.cs`, `Cali.Core/Services/SettingsService.cs`
- Create: `Cali.Tests/Fakes/InMemoryKeyValueStore.cs`, `Cali.Tests/SettingsServiceTests.cs`

**Interfaces:**
- Produces: `IKeyValueStore { bool GetBool(string,bool); void SetBool(string,bool); int GetInt(string,int); void SetInt(string,int); string GetString(string,string); void SetString(string,string); bool Contains(string) }`. `SettingsService { AppSettings Settings {get;} Profile Profile {get;set;} bool IsOnboardingComplete {get;set;} int CurrentStreak {get;set;} int BestStreak {get;set;} void Save(AppSettings) }`.

- [ ] **Step 1: Define the store abstraction and in-memory fake**

```csharp
// Cali.Core/Abstractions/IKeyValueStore.cs
namespace Cali.Core.Abstractions;
public interface IKeyValueStore
{
    bool GetBool(string key, bool def);
    void SetBool(string key, bool value);
    int GetInt(string key, int def);
    void SetInt(string key, int value);
    string GetString(string key, string def);
    void SetString(string key, string value);
    bool Contains(string key);
}
```

```csharp
// Cali.Tests/Fakes/InMemoryKeyValueStore.cs
using Cali.Core.Abstractions;
namespace Cali.Tests.Fakes;
public sealed class InMemoryKeyValueStore : IKeyValueStore
{
    private readonly Dictionary<string, object> _d = new();
    public bool GetBool(string k, bool def) => _d.TryGetValue(k, out var v) ? (bool)v : def;
    public void SetBool(string k, bool v) => _d[k] = v;
    public int GetInt(string k, int def) => _d.TryGetValue(k, out var v) ? (int)v : def;
    public void SetInt(string k, int v) => _d[k] = v;
    public string GetString(string k, string def) => _d.TryGetValue(k, out var v) ? (string)v : def;
    public void SetString(string k, string v) => _d[k] = v;
    public bool Contains(string k) => _d.ContainsKey(k);
}
```

- [ ] **Step 2: Write the failing test**

```csharp
// Cali.Tests/SettingsServiceTests.cs
using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class SettingsServiceTests
{
    [Fact]
    public void Defaults_then_persists_round_trip()
    {
        var store = new InMemoryKeyValueStore();
        var svc = new SettingsService(store);

        Assert.True(svc.Settings.SoundAlerts);
        Assert.False(svc.Settings.AutoProgression);
        Assert.False(svc.IsOnboardingComplete);

        var s = svc.Settings;
        s.SoundAlerts = false; s.AutoProgression = true;
        svc.Save(s);
        svc.IsOnboardingComplete = true;
        svc.CurrentStreak = 12; svc.BestStreak = 21;

        var svc2 = new SettingsService(store); // re-read from same store
        Assert.False(svc2.Settings.SoundAlerts);
        Assert.True(svc2.Settings.AutoProgression);
        Assert.True(svc2.IsOnboardingComplete);
        Assert.Equal(12, svc2.CurrentStreak);
        Assert.Equal(21, svc2.BestStreak);
    }
}
```

- [ ] **Step 3: Run — expect FAIL** (`SettingsService` does not exist).
Run: `dotnet test Cali.Tests/Cali.Tests.csproj --filter SettingsServiceTests`

- [ ] **Step 4: Implement `SettingsService`**

```csharp
// Cali.Core/Services/SettingsService.cs
using Cali.Core.Abstractions;
using Cali.Core.Models;
namespace Cali.Core.Services;

public sealed class SettingsService
{
    private readonly IKeyValueStore _kv;
    public SettingsService(IKeyValueStore kv) => _kv = kv;

    private const string K = "cali.";
    public AppSettings Settings => new()
    {
        SoundAlerts = _kv.GetBool(K + "soundAlerts", true),
        Haptics = _kv.GetBool(K + "haptics", true),
        AutoStartRest = _kv.GetBool(K + "autoStartRest", true),
        AutoProgression = _kv.GetBool(K + "autoProgression", false),
        Reminders = _kv.GetBool(K + "reminders", true),
        Units = _kv.GetString(K + "units", "metric"),
    };

    public void Save(AppSettings s)
    {
        _kv.SetBool(K + "soundAlerts", s.SoundAlerts);
        _kv.SetBool(K + "haptics", s.Haptics);
        _kv.SetBool(K + "autoStartRest", s.AutoStartRest);
        _kv.SetBool(K + "autoProgression", s.AutoProgression);
        _kv.SetBool(K + "reminders", s.Reminders);
        _kv.SetString(K + "units", s.Units);
    }

    public bool IsOnboardingComplete
    {
        get => _kv.GetBool(K + "onboarded", false);
        set => _kv.SetBool(K + "onboarded", value);
    }
    public int CurrentStreak { get => _kv.GetInt(K + "streak", 0); set => _kv.SetInt(K + "streak", value); }
    public int BestStreak { get => _kv.GetInt(K + "bestStreak", 0); set => _kv.SetInt(K + "bestStreak", value); }

    public Profile Profile
    {
        get => new()
        {
            Name = _kv.GetString(K + "p.name", "Athlete"),
            Goal = _kv.GetString(K + "p.goal", "Strength"),
            Level = _kv.GetString(K + "p.level", "Beginner"),
            BodyweightKg = _kv.GetInt(K + "p.bw", 0),
            MaxPullups = _kv.GetInt(K + "p.maxpu", 0),
        };
        set
        {
            _kv.SetString(K + "p.name", value.Name);
            _kv.SetString(K + "p.goal", value.Goal);
            _kv.SetString(K + "p.level", value.Level);
            _kv.SetInt(K + "p.bw", value.BodyweightKg);
            _kv.SetInt(K + "p.maxpu", value.MaxPullups);
        }
    }
}
```

- [ ] **Step 5: Run — expect PASS.** `dotnet test Cali.Tests/Cali.Tests.csproj --filter SettingsServiceTests`
- [ ] **Step 6: Checkpoint** — full `dotnet test` green.

---

## Task 3: `ExerciseRepository` (in-memory, from seed JSON)

**Files:**
- Create: `Cali.Core/Services/ExerciseRepository.cs`
- Create: `Cali.Tests/ExerciseRepositoryTests.cs`
- (`ISeedDataProvider` + `FileSeedDataProvider` already exist from Task 1.)

**Interfaces:**
- Produces: `ExerciseRepository { Task InitAsync(); IReadOnlyList<Exercise> All {get;} Exercise? Get(string id); IReadOnlyList<Exercise> Query(string? muscle, Difficulty? diff, string? search) }`. `muscle` matches `Primary` (or "All"/null = any); `search` matches Name case-insensitively.

- [ ] **Step 1: Failing test**

```csharp
// Cali.Tests/ExerciseRepositoryTests.cs
using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class ExerciseRepositoryTests
{
    private static async Task<ExerciseRepository> MakeAsync()
    {
        var repo = new ExerciseRepository(new FileSeedDataProvider());
        await repo.InitAsync();
        return repo;
    }

    [Fact]
    public async Task Loads_all_and_resolves_by_id()
    {
        var repo = await MakeAsync();
        Assert.Equal(18, repo.All.Count);
        Assert.Equal("Push-ups", repo.Get("pushup")!.Name);
        Assert.Null(repo.Get("nope"));
    }

    [Fact]
    public async Task Query_combines_muscle_difficulty_search()
    {
        var repo = await MakeAsync();
        Assert.All(repo.Query("Core", null, null), e => Assert.Equal("Core", e.Primary));
        Assert.All(repo.Query(null, Difficulty.Advanced, null), e => Assert.Equal(Difficulty.Advanced, e.Difficulty));
        Assert.Single(repo.Query(null, null, "diamond"));
        Assert.Empty(repo.Query("Legs", Difficulty.Advanced, "push"));
    }
}
```

- [ ] **Step 2: Run — expect FAIL.** `dotnet test --filter ExerciseRepositoryTests`

- [ ] **Step 3: Implement**

```csharp
// Cali.Core/Services/ExerciseRepository.cs
using Cali.Core.Abstractions;
using Cali.Core.Models;
namespace Cali.Core.Services;

public sealed class ExerciseRepository
{
    private readonly ISeedDataProvider _seed;
    private List<Exercise> _all = new();
    public ExerciseRepository(ISeedDataProvider seed) => _seed = seed;

    public async Task InitAsync()
    {
        var json = await _seed.ReadSeedJsonAsync();
        _all = SeedData.Parse(json).Exercises.Select(e => e.ToExercise()).ToList();
    }

    public IReadOnlyList<Exercise> All => _all;
    public Exercise? Get(string id) => _all.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Exercise> Query(string? muscle, Difficulty? diff, string? search)
    {
        IEnumerable<Exercise> q = _all;
        if (!string.IsNullOrEmpty(muscle) && muscle != "All")
            q = q.Where(e => e.Primary == muscle);
        if (diff is { } d)
            q = q.Where(e => e.Difficulty == d);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        return q.ToList();
    }
}
```

- [ ] **Step 4: Run — expect PASS.** **Step 5: Checkpoint** — full `dotnet test` green.

---

## Task 4: `IDatabase` + `PlanRepository` (SQLite, preset seeding, duplicate)

**Files:**
- Create: `Cali.Core/Services/IDatabase.cs`, `Cali.Core/Services/PlanRepository.cs`
- Create: `Cali.Tests/PlanRepositoryTests.cs`

**Interfaces:**
- Produces: `IDatabase { SQLiteAsyncConnection Conn {get;} Task InitAsync() }`, `Database(string path)` ctor. `PlanRepository { Task<List<Plan>> GetAllAsync(); Task<List<Plan>> GetPresetsAsync(); Task<List<Plan>> GetUserPlansAsync(); Task<Plan?> GetByIdAsync(string id); Task SaveAsync(Plan plan); Task DeleteAsync(string id); Task<Plan> DuplicateAsync(string id); Task SeedPresetsAsync(List<SeedPlan> seedPlans) }`. `GetByIdAsync` populates `plan.Exercises` ordered by `Order`.

- [ ] **Step 1: Implement `IDatabase` (no test of its own — exercised via repos)**

```csharp
// Cali.Core/Services/IDatabase.cs
using SQLite;
using Cali.Core.Models;
namespace Cali.Core.Services;

public interface IDatabase { SQLiteAsyncConnection Conn { get; } Task InitAsync(); }

public sealed class Database : IDatabase
{
    public SQLiteAsyncConnection Conn { get; }
    public Database(string path) => Conn = new SQLiteAsyncConnection(path);
    public async Task InitAsync()
    {
        await Conn.CreateTableAsync<Plan>();
        await Conn.CreateTableAsync<PlanExercise>();
        await Conn.CreateTableAsync<WorkoutRecord>();
        await Conn.CreateTableAsync<PersonalRecord>();
    }
}
```

- [ ] **Step 2: Failing test**

```csharp
// Cali.Tests/PlanRepositoryTests.cs
using Cali.Core.Models;
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class PlanRepositoryTests : IAsyncLifetime
{
    private string _path = "";
    private Database _db = null!;
    private PlanRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        _db = new Database(_path);
        await _db.InitAsync();
        _repo = new PlanRepository(_db);
        var seed = SeedData.Parse(await new FileSeedDataProvider().ReadSeedJsonAsync());
        await _repo.SeedPresetsAsync(seed.PredefinedPlans);
    }
    public Task DisposeAsync() { try { File.Delete(_path); } catch { } return Task.CompletedTask; }

    [Fact]
    public async Task Seeds_six_presets_with_exercises()
    {
        var presets = await _repo.GetPresetsAsync();
        Assert.Equal(6, presets.Count);
        var push = await _repo.GetByIdAsync("push-day");
        Assert.NotNull(push);
        Assert.Equal(6, push!.Exercises.Count);
        Assert.Equal("pushup", push.Exercises[0].ExerciseId);
    }

    [Fact]
    public async Task Seeding_is_idempotent()
    {
        var seed = SeedData.Parse(await new FileSeedDataProvider().ReadSeedJsonAsync());
        await _repo.SeedPresetsAsync(seed.PredefinedPlans); // second call
        Assert.Equal(6, (await _repo.GetPresetsAsync()).Count);
    }

    [Fact]
    public async Task Duplicate_creates_editable_user_plan()
    {
        var dup = await _repo.DuplicateAsync("push-day");
        Assert.False(dup.Preset);
        Assert.Contains("Copy", dup.Name);
        Assert.Equal(6, dup.Exercises.Count);
        var users = await _repo.GetUserPlansAsync();
        Assert.Single(users);
    }

    [Fact]
    public async Task Save_then_delete_user_plan()
    {
        var p = new Plan { Id = "u1", Name = "My Flow", Preset = false, Difficulty = Difficulty.Beginner,
            EstMinutes = 10, Muscles = "Full Body",
            Exercises = { new PlanExercise { ExerciseId = "squat", Order = 0, Sets = 3, Reps = 12, RestSec = 60 } } };
        await _repo.SaveAsync(p);
        Assert.NotNull(await _repo.GetByIdAsync("u1"));
        await _repo.DeleteAsync("u1");
        Assert.Null(await _repo.GetByIdAsync("u1"));
    }
}
```

- [ ] **Step 3: Run — expect FAIL.** `dotnet test --filter PlanRepositoryTests`

- [ ] **Step 4: Implement `PlanRepository`**

```csharp
// Cali.Core/Services/PlanRepository.cs
using Cali.Core.Models;
namespace Cali.Core.Services;

public sealed class PlanRepository
{
    private readonly IDatabase _db;
    public PlanRepository(IDatabase db) => _db = db;

    public async Task<List<Plan>> GetAllAsync()
    {
        var plans = await _db.Conn.Table<Plan>().ToListAsync();
        foreach (var p in plans) p.Exercises = await LoadExercisesAsync(p.Id);
        return plans;
    }
    public async Task<List<Plan>> GetPresetsAsync() => (await GetAllAsync()).Where(p => p.Preset).ToList();
    public async Task<List<Plan>> GetUserPlansAsync() => (await GetAllAsync()).Where(p => !p.Preset).ToList();

    public async Task<Plan?> GetByIdAsync(string id)
    {
        var p = await _db.Conn.FindAsync<Plan>(id);
        if (p is null) return null;
        p.Exercises = await LoadExercisesAsync(id);
        return p;
    }

    private async Task<List<PlanExercise>> LoadExercisesAsync(string planId) =>
        (await _db.Conn.Table<PlanExercise>().Where(x => x.PlanId == planId).ToListAsync())
        .OrderBy(x => x.Order).ToList();

    public async Task SaveAsync(Plan plan)
    {
        await _db.Conn.InsertOrReplaceAsync(plan);
        await _db.Conn.ExecuteAsync("DELETE FROM PlanExercises WHERE PlanId = ?", plan.Id);
        for (int i = 0; i < plan.Exercises.Count; i++)
        {
            var pe = plan.Exercises[i];
            pe.Id = 0; pe.PlanId = plan.Id; pe.Order = i;
            await _db.Conn.InsertAsync(pe);
        }
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Conn.ExecuteAsync("DELETE FROM PlanExercises WHERE PlanId = ?", id);
        await _db.Conn.DeleteAsync<Plan>(id);
    }

    public async Task<Plan> DuplicateAsync(string id)
    {
        var src = await GetByIdAsync(id) ?? throw new InvalidOperationException($"Plan {id} not found");
        var copy = new Plan
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = src.Name + " (Copy)",
            Preset = false,
            Difficulty = src.Difficulty,
            EstMinutes = src.EstMinutes,
            Muscles = src.Muscles,
            Exercises = src.Exercises.Select(e => new PlanExercise
            {
                ExerciseId = e.ExerciseId, Order = e.Order, Sets = e.Sets,
                Reps = e.Reps, HoldSec = e.HoldSec, RestSec = e.RestSec
            }).ToList()
        };
        await SaveAsync(copy);
        return copy;
    }

    public async Task SeedPresetsAsync(List<SeedPlan> seedPlans)
    {
        foreach (var sp in seedPlans)
        {
            if (await _db.Conn.FindAsync<Plan>(sp.Id) is not null) continue; // idempotent
            var plan = new Plan
            {
                Id = sp.Id, Name = sp.Name, Preset = sp.Preset,
                Difficulty = Enum.Parse<Difficulty>(sp.Difficulty, true),
                EstMinutes = sp.EstMinutes, Muscles = sp.Muscles,
                Exercises = sp.Exercises.Select((e, i) => new PlanExercise
                {
                    ExerciseId = e.ExerciseId, Order = i, Sets = e.Sets,
                    Reps = e.Reps, HoldSec = e.HoldSec, RestSec = e.RestSec
                }).ToList()
            };
            await SaveAsync(plan);
        }
    }
}
```

> Note on `Guid.NewGuid()`: allowed here (runtime app/test code). The workflow-script restriction on `Guid`/`Date` applies only to Workflow JS scripts, not to C# app code.

- [ ] **Step 5: Run — expect PASS.** **Step 6: Checkpoint** — full `dotnet test` green.

---

## Task 5: `HistoryRepository` + `PrRepository`

**Files:**
- Create: `Cali.Core/Services/HistoryRepository.cs`, `Cali.Core/Services/PrRepository.cs`
- Create: `Cali.Tests/HistoryRepositoryTests.cs`, `Cali.Tests/PrRepositoryTests.cs`

**Interfaces:**
- Produces: `HistoryRepository { Task AddAsync(WorkoutRecord); Task<List<WorkoutRecord>> GetRecentAsync(int n); Task<int> CountThisWeekAsync(DateTime nowUtc); Task<int> TotalRepsAsync() }`. `PrRepository { Task<PersonalRecord?> GetAsync(string exerciseId); Task<bool> TryBeatRepsAsync(string exerciseId, int reps, DateTime utc); Task<bool> TryBeatHoldAsync(string exerciseId, int holdSec, DateTime utc) }` — returns true when a new PR was set.

- [ ] **Step 1: Failing tests**

```csharp
// Cali.Tests/HistoryRepositoryTests.cs
using Cali.Core.Models;
using Cali.Core.Services;
using Xunit;

public class HistoryRepositoryTests : IAsyncLifetime
{
    private string _path = "";
    private Database _db = null!;
    private HistoryRepository _repo = null!;
    public async Task InitializeAsync()
    {
        _path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        _db = new Database(_path); await _db.InitAsync();
        _repo = new HistoryRepository(_db);
    }
    public Task DisposeAsync() { try { File.Delete(_path); } catch { } return Task.CompletedTask; }

    [Fact]
    public async Task Add_and_query_recent_and_week_and_reps()
    {
        var now = new DateTime(2026, 6, 23, 12, 0, 0, DateTimeKind.Utc);
        await _repo.AddAsync(new WorkoutRecord { DateUtc = now.AddDays(-1), PlanName = "Push Day", DurationSec = 1600, ExerciseCount = 6, TotalSets = 18, TotalReps = 120, Effort = Effort.JustRight });
        await _repo.AddAsync(new WorkoutRecord { DateUtc = now.AddDays(-10), PlanName = "Leg Day", DurationSec = 1200, ExerciseCount = 4, TotalSets = 12, TotalReps = 90, Effort = Effort.TooHard });

        var recent = await _repo.GetRecentAsync(10);
        Assert.Equal(2, recent.Count);
        Assert.Equal("Push Day", recent[0].PlanName); // newest first
        Assert.Equal(1, await _repo.CountThisWeekAsync(now));
        Assert.Equal(210, await _repo.TotalRepsAsync());
    }
}
```

```csharp
// Cali.Tests/PrRepositoryTests.cs
using Cali.Core.Services;
using Xunit;

public class PrRepositoryTests : IAsyncLifetime
{
    private string _path = "";
    private Database _db = null!;
    private PrRepository _repo = null!;
    public async Task InitializeAsync()
    {
        _path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        _db = new Database(_path); await _db.InitAsync();
        _repo = new PrRepository(_db);
    }
    public Task DisposeAsync() { try { File.Delete(_path); } catch { } return Task.CompletedTask; }

    [Fact]
    public async Task Reps_pr_only_beats_when_higher()
    {
        var utc = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(await _repo.TryBeatRepsAsync("pushup", 20, utc));   // first = PR
        Assert.False(await _repo.TryBeatRepsAsync("pushup", 18, utc));  // lower = no
        Assert.True(await _repo.TryBeatRepsAsync("pushup", 25, utc));   // higher = PR
        Assert.Equal(25, (await _repo.GetAsync("pushup"))!.MaxReps);
    }

    [Fact]
    public async Task Hold_pr_tracks_longest()
    {
        var utc = new DateTime(2026, 6, 23, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(await _repo.TryBeatHoldAsync("plank", 45, utc));
        Assert.False(await _repo.TryBeatHoldAsync("plank", 30, utc));
        Assert.Equal(45, (await _repo.GetAsync("plank"))!.LongestHoldSec);
    }
}
```

- [ ] **Step 2: Run — expect FAIL.**

- [ ] **Step 3: Implement both repositories**

```csharp
// Cali.Core/Services/HistoryRepository.cs
using Cali.Core.Models;
namespace Cali.Core.Services;

public sealed class HistoryRepository
{
    private readonly IDatabase _db;
    public HistoryRepository(IDatabase db) => _db = db;

    public Task AddAsync(WorkoutRecord r) => _db.Conn.InsertAsync(r);

    public async Task<List<WorkoutRecord>> GetRecentAsync(int n) =>
        (await _db.Conn.Table<WorkoutRecord>().ToListAsync())
        .OrderByDescending(r => r.DateUtc).Take(n).ToList();

    public async Task<int> CountThisWeekAsync(DateTime nowUtc)
    {
        var weekAgo = nowUtc.AddDays(-7);
        return (await _db.Conn.Table<WorkoutRecord>().ToListAsync())
            .Count(r => r.DateUtc >= weekAgo && r.DateUtc <= nowUtc);
    }

    public async Task<int> TotalRepsAsync() =>
        (await _db.Conn.Table<WorkoutRecord>().ToListAsync()).Sum(r => r.TotalReps);
}
```

```csharp
// Cali.Core/Services/PrRepository.cs
using Cali.Core.Models;
namespace Cali.Core.Services;

public sealed class PrRepository
{
    private readonly IDatabase _db;
    public PrRepository(IDatabase db) => _db = db;

    public Task<PersonalRecord?> GetAsync(string exerciseId) =>
        _db.Conn.FindAsync<PersonalRecord>(exerciseId)!;

    public async Task<bool> TryBeatRepsAsync(string exerciseId, int reps, DateTime utc)
    {
        var pr = await _db.Conn.FindAsync<PersonalRecord>(exerciseId);
        if (pr is not null && pr.MaxReps >= reps) return false;
        pr ??= new PersonalRecord { ExerciseId = exerciseId };
        pr.MaxReps = reps; pr.AchievedUtc = utc;
        await _db.Conn.InsertOrReplaceAsync(pr);
        return true;
    }

    public async Task<bool> TryBeatHoldAsync(string exerciseId, int holdSec, DateTime utc)
    {
        var pr = await _db.Conn.FindAsync<PersonalRecord>(exerciseId);
        if (pr is not null && pr.LongestHoldSec >= holdSec) return false;
        pr ??= new PersonalRecord { ExerciseId = exerciseId };
        pr.LongestHoldSec = holdSec; pr.AchievedUtc = utc;
        await _db.Conn.InsertOrReplaceAsync(pr);
        return true;
    }
}
```

- [ ] **Step 4: Run — expect PASS.** **Step 5: Checkpoint** — full `dotnet test` green.

---

## Task 6: `SeedService` (first-launch orchestration)

**Files:**
- Create: `Cali.Core/Services/SeedService.cs`
- Create: `Cali.Tests/SeedServiceTests.cs`

**Interfaces:**
- Consumes: `ISeedDataProvider`, `IDatabase`, `PlanRepository`, `HistoryRepository`, `SettingsService`.
- Produces: `SeedService { Task EnsureSeededAsync() }` — idempotent (guarded by a `SettingsService` flag): seeds preset plans, sample history (only if history empty), default settings + sample profile/streak on first run.

- [ ] **Step 1: Failing test**

```csharp
// Cali.Tests/SeedServiceTests.cs
using Cali.Core.Services;
using Cali.Tests.Fakes;
using Xunit;

public class SeedServiceTests
{
    private static (SeedService svc, PlanRepository plans, HistoryRepository hist, SettingsService settings, string path) Make()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cali-{Guid.NewGuid():N}.db3");
        var db = new Database(path); db.InitAsync().GetAwaiter().GetResult();
        var plans = new PlanRepository(db);
        var hist = new HistoryRepository(db);
        var settings = new SettingsService(new InMemoryKeyValueStore());
        var seedProv = new FileSeedDataProvider();
        var svc = new SeedService(seedProv, plans, hist, settings);
        return (svc, plans, hist, settings, path);
    }

    [Fact]
    public async Task First_run_seeds_then_second_run_is_noop()
    {
        var (svc, plans, hist, settings, path) = Make();
        try
        {
            await svc.EnsureSeededAsync();
            Assert.Equal(6, (await plans.GetPresetsAsync()).Count);
            Assert.Equal(3, (await hist.GetRecentAsync(50)).Count);
            Assert.True(settings.IsOnboardingComplete == false); // seeding does NOT complete onboarding
            Assert.Equal(12, settings.CurrentStreak);            // from sampleProfile
            Assert.Equal(21, settings.BestStreak);

            await svc.EnsureSeededAsync(); // idempotent
            Assert.Equal(6, (await plans.GetPresetsAsync()).Count);
            Assert.Equal(3, (await hist.GetRecentAsync(50)).Count);
        }
        finally { try { File.Delete(path); } catch { } }
    }
}
```

- [ ] **Step 2: Run — expect FAIL.**

- [ ] **Step 3: Implement `SeedService`**

```csharp
// Cali.Core/Services/SeedService.cs
using Cali.Core.Abstractions;
using Cali.Core.Models;
namespace Cali.Core.Services;

public sealed class SeedService
{
    private readonly ISeedDataProvider _seed;
    private readonly PlanRepository _plans;
    private readonly HistoryRepository _history;
    private readonly SettingsService _settings;
    private const string SeededKey = "cali.seeded";

    public SeedService(ISeedDataProvider seed, PlanRepository plans, HistoryRepository history, SettingsService settings)
    { _seed = seed; _plans = plans; _history = history; _settings = settings; }

    public async Task EnsureSeededAsync()
    {
        var data = SeedData.Parse(await _seed.ReadSeedJsonAsync());
        await _plans.SeedPresetsAsync(data.PredefinedPlans); // idempotent itself

        // Sample history only when there is none yet.
        if ((await _history.GetRecentAsync(1)).Count == 0)
        {
            foreach (var h in data.SampleHistory)
                await _history.AddAsync(new WorkoutRecord
                {
                    DateUtc = DateTime.SpecifyKind(DateTime.Parse(h.Date), DateTimeKind.Utc),
                    PlanName = h.PlanName, DurationSec = h.DurationSec, ExerciseCount = h.Exercises,
                    TotalSets = 0, TotalReps = 0,
                    Effort = h.Effort.StartsWith("Too easy") ? Effort.TooEasy
                           : h.Effort.StartsWith("Too hard") ? Effort.TooHard : Effort.JustRight
                });
        }

        // One-time profile/settings seed.
        if (!_settings.IsKeySet(SeededKey))
        {
            if (data.SampleProfile is { } p)
            {
                _settings.Profile = new Profile { Name = p.Name, Goal = p.Goal, Level = p.Level, BodyweightKg = p.BodyweightKg, MaxPullups = p.MaxPullups };
                _settings.CurrentStreak = p.CurrentStreakDays;
                _settings.BestStreak = p.BestStreakDays;
            }
            if (data.DefaultSettings is { } s)
                _settings.Save(new AppSettings { SoundAlerts = s.SoundAlerts, Haptics = s.Haptics, AutoStartRest = s.AutoStartRest, AutoProgression = s.AutoProgression, Reminders = s.Reminders, Units = s.Units });
            _settings.MarkKey(SeededKey);
        }
    }
}
```

> Add two helpers to `SettingsService` (Task 2 file): `public bool IsKeySet(string key) => _kv.Contains(key);` and `public void MarkKey(string key) => _kv.SetBool(key, true);`. Update Task 2 when implementing, or add here — keep the names exact.

- [ ] **Step 4: Run — expect PASS.** **Step 5: Checkpoint** — full `dotnet test` green.

---

## Task 7: MAUI packages, project reference, and platform implementations

**Files:**
- Modify: `Cali.csproj` (add `ProjectReference` to `Cali.Core`; add NuGet packages; bundle seed; add fonts placeholder)
- Create: `Cali/Platform/PreferencesKeyValueStore.cs`, `Cali/Platform/AppPackageSeedDataProvider.cs`, `Cali/Platform/DispatcherTicker.cs`
- Create: `Cali.Core/Abstractions/ITicker.cs`
- Copy: `design_handoff_calisthenics_app/seed-data.json` → `Cali/Resources/Raw/seed-data.json`

**Interfaces:**
- Produces: `ITicker { event Action Tick; void Start(); void Stop(); bool IsRunning {get;} }` (1-second cadence). Platform impls of `IKeyValueStore`, `ISeedDataProvider`, `ITicker`.

- [ ] **Step 1: Add packages + project ref to `Cali.csproj`** (inside the existing `<ItemGroup>` with package refs)

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
<PackageReference Include="CommunityToolkit.Maui" Version="9.1.0" />
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
```
```xml
<ItemGroup>
  <ProjectReference Include="Cali.Core\Cali.Core.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Copy the seed file** to `Cali/Resources/Raw/seed-data.json` (the scaffold's `<MauiAsset Include="Resources\Raw\**" .../>` already bundles it with logical name `seed-data.json`).

- [ ] **Step 3: Define `ITicker` + platform implementations**

```csharp
// Cali.Core/Abstractions/ITicker.cs
namespace Cali.Core.Abstractions;
public interface ITicker { event Action? Tick; void Start(); void Stop(); bool IsRunning { get; } }
```

```csharp
// Cali/Platform/PreferencesKeyValueStore.cs
using Cali.Core.Abstractions;
using Microsoft.Maui.Storage;
namespace Cali.Platform;
public sealed class PreferencesKeyValueStore : IKeyValueStore
{
    public bool GetBool(string k, bool d) => Preferences.Get(k, d);
    public void SetBool(string k, bool v) => Preferences.Set(k, v);
    public int GetInt(string k, int d) => Preferences.Get(k, d);
    public void SetInt(string k, int v) => Preferences.Set(k, v);
    public string GetString(string k, string d) => Preferences.Get(k, d);
    public void SetString(string k, string v) => Preferences.Set(k, v);
    public bool Contains(string k) => Preferences.ContainsKey(k);
}
```

```csharp
// Cali/Platform/AppPackageSeedDataProvider.cs
using Cali.Core.Abstractions;
using Microsoft.Maui.Storage;
namespace Cali.Platform;
public sealed class AppPackageSeedDataProvider : ISeedDataProvider
{
    public async Task<string> ReadSeedJsonAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("seed-data.json");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
```

```csharp
// Cali/Platform/DispatcherTicker.cs
using Cali.Core.Abstractions;
namespace Cali.Platform;
public sealed class DispatcherTicker : ITicker
{
    private readonly IDispatcherTimer _timer;
    public event Action? Tick;
    public DispatcherTicker(IDispatcher dispatcher)
    {
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += (_, _) => Tick?.Invoke();
    }
    public bool IsRunning => _timer.IsRunning;
    public void Start() { if (!_timer.IsRunning) _timer.Start(); }
    public void Stop() { if (_timer.IsRunning) _timer.Stop(); }
}
```

- [ ] **Step 4: Checkpoint** — `dotnet build Cali.csproj -f net10.0-android` succeeds (UI wiring comes in Task 9–10; this verifies packages + refs + platform classes compile).

---

## Task 8: Design tokens (`Colors.xaml`) + base styles (`Styles.xaml`)

**Files:**
- Modify: `Cali/Resources/Styles/Colors.xaml` (replace template palette), `Cali/Resources/Styles/Styles.xaml` (base styles), `Cali/App.xaml.cs` (force dark)

- [ ] **Step 1: Replace `Colors.xaml` contents** with the design tokens

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <Color x:Key="Bg">#0D0E11</Color>
    <Color x:Key="Surface">#15171C</Color>
    <Color x:Key="Surface2">#1C1F26</Color>
    <Color x:Key="Line">#262A33</Color>
    <Color x:Key="LineSoft">#1C1F26</Color>
    <Color x:Key="TextPrimary">#F3F4F6</Color>
    <Color x:Key="TextMuted">#9499A3</Color>
    <Color x:Key="TextFaint">#6B7079</Color>
    <Color x:Key="Accent">#FF6B1A</Color>
    <Color x:Key="AccentEnd">#FF8C42</Color>
    <Color x:Key="AccentTint">#251710</Color>
    <Color x:Key="AccentOn">#0D0E11</Color>
    <Color x:Key="Success">#3ECF8E</Color>
    <Color x:Key="Danger">#FF4D6B</Color>
    <Color x:Key="DangerX">#FF4D4D</Color>
</ResourceDictionary>
```

- [ ] **Step 2: Replace `Styles.xaml`** with minimal base styles (page bg + default label color + Shell tab colors). Keep only what's needed to make empty pages render on-theme; control styles come per-screen later.

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    <Style TargetType="ContentPage" ApplyToDerivedTypes="True">
        <Setter Property="BackgroundColor" Value="{StaticResource Bg}" />
    </Style>
    <Style TargetType="Label">
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="FontFamily" Value="Barlow" />
    </Style>
    <Style TargetType="Shell" ApplyToDerivedTypes="True">
        <Setter Property="Shell.BackgroundColor" Value="{StaticResource Bg}" />
        <Setter Property="Shell.ForegroundColor" Value="{StaticResource Accent}" />
        <Setter Property="Shell.TabBarBackgroundColor" Value="{StaticResource Bg}" />
        <Setter Property="Shell.TabBarTitleColor" Value="{StaticResource Accent}" />
        <Setter Property="Shell.TabBarUnselectedColor" Value="{StaticResource TextFaint}" />
    </Style>
</ResourceDictionary>
```

- [ ] **Step 3: Force dark theme** in `App.xaml.cs`

```csharp
public App()
{
    InitializeComponent();
    UserAppTheme = AppTheme.Dark;
    MainPage = new AppShell();
}
```
> If the scaffold's `App` uses `CreateWindow`/`Shell` differently in .NET 10, set `UserAppTheme = AppTheme.Dark;` in the constructor and keep the existing window/shell wiring.

- [ ] **Step 4: Checkpoint** — `dotnet build Cali.csproj -f net10.0-android` succeeds.

---

## Task 9: Download + register Barlow fonts

**Files:**
- Create: `Cali/Resources/Fonts/Barlow-Regular.ttf`, `Barlow-Medium.ttf`, `Barlow-SemiBold.ttf`, `Barlow-Bold.ttf`, `BarlowCondensed-SemiBold.ttf`, `BarlowCondensed-Bold.ttf`
- Modify: `Cali/MauiProgram.cs` (register fonts)

- [ ] **Step 1: Download the six TTFs** (PowerShell, `-UseBasicParsing` required in this environment)

```powershell
$dir = "Cali/Resources/Fonts"
$base = "https://github.com/google/fonts/raw/main/ofl"
$map = @{
  "Barlow-Regular.ttf"          = "$base/barlow/Barlow-Regular.ttf"
  "Barlow-Medium.ttf"           = "$base/barlow/Barlow-Medium.ttf"
  "Barlow-SemiBold.ttf"         = "$base/barlow/Barlow-SemiBold.ttf"
  "Barlow-Bold.ttf"             = "$base/barlow/Barlow-Bold.ttf"
  "BarlowCondensed-SemiBold.ttf"= "$base/barlowcondensed/BarlowCondensed-SemiBold.ttf"
  "BarlowCondensed-Bold.ttf"    = "$base/barlowcondensed/BarlowCondensed-Bold.ttf"
}
foreach ($k in $map.Keys) {
  Invoke-WebRequest -Uri $map[$k] -OutFile (Join-Path $dir $k) -UseBasicParsing -MaximumRedirection 5
}
```
Expected: 6 `.ttf` files 90–130 KB each in `Resources/Fonts/`. Verify each is a real TTF (first bytes `00 01 00 00`), not an HTML error page.

- [ ] **Step 2: Register fonts in `MauiProgram.cs`** (replace the OpenSans block; also register CommunityToolkit + DI — full file shown in Task 10 Step 2). Font lines:

```csharp
.ConfigureFonts(fonts =>
{
    fonts.AddFont("Barlow-Regular.ttf", "Barlow");
    fonts.AddFont("Barlow-Medium.ttf", "BarlowMedium");
    fonts.AddFont("Barlow-SemiBold.ttf", "BarlowSemiBold");
    fonts.AddFont("Barlow-Bold.ttf", "BarlowBold");
    fonts.AddFont("BarlowCondensed-SemiBold.ttf", "BarlowCondensedSemiBold");
    fonts.AddFont("BarlowCondensed-Bold.ttf", "BarlowCondensedBold");
})
```

- [ ] **Step 3: Checkpoint** — `dotnet build Cali.csproj -f net10.0-android` succeeds (fonts embed without error).

---

## Task 10: Shell `TabBar` + 5 empty themed pages + full DI wiring + emulator smoke test

**Files:**
- Modify: `Cali/AppShell.xaml` (5-tab `TabBar`), `Cali/MauiProgram.cs` (full DI), delete `MainPage.xaml(.cs)` usage from Shell
- Create: `Cali/Views/HomePage.xaml(.cs)`, `PlansPage`, `ExercisesPage`, `ProgressPage`, `ProfilePage`

**Interfaces:**
- Consumes: all Core services + platform impls.
- Produces: a runnable app showing 5 dark tabs with accent selection.

- [ ] **Step 1: AppShell with 5 tabs**

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell x:Class="Cali.AppShell"
       xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:v="clr-namespace:Cali.Views"
       Title="Cali" FlyoutBehavior="Disabled">
    <TabBar>
        <ShellContent Title="Home"      ContentTemplate="{DataTemplate v:HomePage}" Route="home" />
        <ShellContent Title="Plans"     ContentTemplate="{DataTemplate v:PlansPage}" Route="plans" />
        <ShellContent Title="Exercises" ContentTemplate="{DataTemplate v:ExercisesPage}" Route="exercises" />
        <ShellContent Title="Progress"  ContentTemplate="{DataTemplate v:ProgressPage}" Route="progress" />
        <ShellContent Title="You"       ContentTemplate="{DataTemplate v:ProfilePage}" Route="you" />
    </TabBar>
</Shell>
```
> Tab icons are added in Phase 7 polish (hand-built SVGs). Titles suffice for the smoke test.

- [ ] **Step 2: Full `MauiProgram.cs`** (fonts + CommunityToolkit + DI for services, platform impls, pages)

```csharp
using CommunityToolkit.Maui;
using Cali.Core.Abstractions;
using Cali.Core.Services;
using Cali.Platform;
using Cali.Views;
using Microsoft.Extensions.Logging;

namespace Cali;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Barlow-Regular.ttf", "Barlow");
                fonts.AddFont("Barlow-Medium.ttf", "BarlowMedium");
                fonts.AddFont("Barlow-SemiBold.ttf", "BarlowSemiBold");
                fonts.AddFont("Barlow-Bold.ttf", "BarlowBold");
                fonts.AddFont("BarlowCondensed-SemiBold.ttf", "BarlowCondensedSemiBold");
                fonts.AddFont("BarlowCondensed-Bold.ttf", "BarlowCondensedBold");
            });

        // Abstractions → platform impls
        builder.Services.AddSingleton<IKeyValueStore, PreferencesKeyValueStore>();
        builder.Services.AddSingleton<ISeedDataProvider, AppPackageSeedDataProvider>();
        builder.Services.AddTransient<ITicker>(sp => new DispatcherTicker(
            Application.Current!.Dispatcher));

        // Core services
        builder.Services.AddSingleton<IDatabase>(_ =>
            new Database(Path.Combine(FileSystem.AppDataDirectory, "cali.db3")));
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<ExerciseRepository>();
        builder.Services.AddSingleton<PlanRepository>();
        builder.Services.AddSingleton<HistoryRepository>();
        builder.Services.AddSingleton<PrRepository>();
        builder.Services.AddSingleton<SeedService>();

        // Pages
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<PlansPage>();
        builder.Services.AddTransient<ExercisesPage>();
        builder.Services.AddTransient<ProgressPage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
```

- [ ] **Step 3: Initialize DB + seed on startup.** In `App.xaml.cs` constructor (fire-and-forget with await on a background init), or better, in `AppShell` code-behind `OnAppearing`. Minimal approach — add to `App.xaml.cs`:

```csharp
public App(IServiceProvider services)
{
    InitializeComponent();
    UserAppTheme = AppTheme.Dark;
    MainPage = new AppShell();
    _ = InitializeAsync(services);
}

private static async Task InitializeAsync(IServiceProvider services)
{
    await services.GetRequiredService<IDatabase>().InitAsync();
    await services.GetRequiredService<ExerciseRepository>().InitAsync();
    await services.GetRequiredService<SeedService>().EnsureSeededAsync();
}
```
> Register `App` for DI: `builder.Services.AddSingleton<App>();` is not used — MAUI news up `App` via `UseMauiApp<App>()`. Use constructor injection by registering it: change `UseMauiApp<App>()` stays; to get `IServiceProvider` into `App`, add a parameterless `App()` that resolves via `Application.Current.Handler.MauiContext.Services` OR keep the `_ = InitializeAsync(...)` call inside `OnStart()` of `App` using `Handler.MauiContext.Services`. Implementer: prefer overriding `protected override void OnStart()` and resolving services from `Handler!.MauiContext!.Services` to avoid ctor-injection friction.

- [ ] **Step 4: Five empty themed pages.** Each identical but for class/namespace/title. `HomePage.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Cali.Views.HomePage" Title="Home"
             BackgroundColor="{StaticResource Bg}">
    <VerticalStackLayout Padding="22" Spacing="12" VerticalOptions="Center">
        <Label Text="Home" FontFamily="BarlowCondensedBold" FontSize="32" TextColor="{StaticResource TextPrimary}" />
        <Label Text="Phase 1 placeholder" FontSize="14" TextColor="{StaticResource TextMuted}" />
    </VerticalStackLayout>
</ContentPage>
```
```csharp
// HomePage.xaml.cs
namespace Cali.Views;
public partial class HomePage : ContentPage { public HomePage() => InitializeComponent(); }
```
Repeat for `PlansPage` (Title="Plans", label "Plans"), `ExercisesPage` ("Exercises"), `ProgressPage` ("Progress"), `ProfilePage` (Title="You", label "You"). Each needs the matching `x:Class`/namespace and a trivial code-behind.

- [ ] **Step 5: Remove the old `MainPage`** references (Shell no longer routes to it). Delete `MainPage.xaml`/`MainPage.xaml.cs` or leave unreferenced; ensure no build error.

- [ ] **Step 6: Boot the emulator and run**

```powershell
$emu = Join-Path $env:LOCALAPPDATA 'Android\Sdk\emulator\emulator.exe'
Start-Process $emu -ArgumentList '-avd','Medium_Phone_API_36.0','-netdelay','none','-netspeed','full'
# wait for boot
$adb = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
& $adb wait-for-device
dotnet build Cali.csproj -t:Run -f net10.0-android
```
Expected: app launches showing a dark screen with a bottom tab bar of 5 tabs (Home/Plans/Exercises/Progress/You); selected tab label/icon in accent orange `#FF6B1A`, others faint grey. No crash; DB seeded (no exception in logs).

- [ ] **Step 7: Screenshot + verify** against the dark theme expectation (full-screen fidelity comes per-screen in later phases).

```powershell
$adb = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools\adb.exe'
& $adb exec-out screencap -p > phase1-tabs.png
```

- [ ] **Step 8: Checkpoint** — `dotnet test` green (all service tests) AND app runs on the emulator with 5 themed tabs.

---

## Self-Review (Phase 1)

- **Spec coverage (Phase 1 portions of the spec §2–§5, §8, §12.1):** packages ✓ (T7), fonts ✓ (T9), tokens/theme ✓ (T8), Shell+5 tabs ✓ (T10), SQLite+seed ✓ (T4–T6), settings/Preferences ✓ (T2), 18 exercises / 6 plans / 3 history asserted ✓ (T1,T3,T4,T6), Android emulator verify ✓ (T10). The `ExerciseRepository`-in-memory decision is a deliberate refinement of "seed exercises into the DB" — exercises are static; documented in spec §8.
- **Placeholder scan:** none — every step has concrete code/commands. The two prose `> Note:` blocks resolve real friction (ITicker timing of definition; App DI bootstrapping) rather than deferring work.
- **Type consistency:** `SettingsService` gains `IsKeySet`/`MarkKey` (used by `SeedService`) — noted in Task 6. `ITicker`/`IKeyValueStore`/`ISeedDataProvider` signatures match between Core definitions and platform impls. Repo method names match between Interfaces blocks and test usages.

---

## Roadmap — Phases 2–8 (each gets its own detailed plan, written just-in-time)

Each phase below is an independently testable deliverable that builds on Phase 1. Its detailed bite-sized plan is authored when we reach it (mirroring this document's depth), so the per-screen XAML is designed against the real, running Phase 1 app rather than guessed up front.

- **Phase 2 — Active Workout + Rest timer (hero).** `WorkoutSessionService` (uses `ITicker`; count-up session, count-down rest, complete-set→auto-rest, +30/skip/presets, progress formula, background-resume via persisted `sessionStartUtc`) — **TDD against a fake `ITicker`** that advances time deterministically. `AudioHapticService` (settings-gated). `ActiveWorkoutPage` + rest overlay, `SetDots`, `RingTimer`, `StripedPlaceholder` controls. Verify on emulator against `06-active-workout` / `07-rest-timer`.
- **Phase 3 — Plans tab.** `PlansVM`, Predefined/My segmented toggle, plan cards (left accent bar, PRESET/CUSTOM badge, meta chips), start-workout from card. Verify vs `03-plans`.
- **Phase 4 — Plan Builder.** Modal, name input, per-exercise stepper cards, add/remove/reorder, duplicate/delete, persist via `PlanRepository`. Round-trip test.
- **Phase 5 — Summary.** `SummaryVM`, save `WorkoutRecord`, PR detection via `PrRepository` + banner, effort control, celebration animation. Verify vs `08-summary`.
- **Phase 6 — Exercises library + Exercise Detail.** `ExercisesVM` (muscle/difficulty/search filters via `ExerciseRepository.Query`), rows; `ExerciseDetailVM` with easier/harder progression navigation (signature feature). Verify vs `04`/`05`.
- **Phase 7 — Onboarding, Home, Progress, Profile/Settings + backup.** Onboarding 4-step gate; Home dashboard (streak/today's plan/quick stats); Progress (stat cards, weekly bar chart, achievements, history); Profile (toggles wired to `SettingsService`, Export/Import via `BackupService` + `CommunityToolkit.Maui.FileSaver`). Hand-built SVG nav icons land here. Verify vs `01`,`02`,`09`,`10`.
- **Phase 8 — Motion & polish.** Slide-up modal transitions, set-complete tick, summary check/ring animation, safe-areas, final fidelity pass across all screenshots.
