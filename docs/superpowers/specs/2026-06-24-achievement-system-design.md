# Cali — Achievement System — Design Spec

**Date:** 2026-06-24
**Status:** Approved (decisions confirmed) — ready to implement
**Builds on:** the completed Cali app (`docs/superpowers/specs/2026-06-23-cali-calisthenics-app-design.md`). Replaces the placeholder achievement display in `ProgressViewModel` (an on-the-fly `switch`) with a real, persisted, celebrated system.

## Goal
Turn the static achievement badges into a working system: evaluate unlock criteria from on-device data, **persist** unlock state (so a *new* unlock can be detected), and **celebrate** newly-earned achievements with an animated banner on the Progress screen.

## The 5 achievements (from `seed-data.json`) + criteria
Criteria are inferred from the names (the handoff specifies names/icons only):

| id | icon | name | unlock criterion |
|---|---|---|---|
| `first-workout` | 🥇 | First workout | ≥ 1 saved workout in history |
| `streak-7` | 🔥 | 7-day streak | best streak ≥ 7 |
| `hundred-day` | 💯 | 100 in a day | ≥ 100 total reps in a single calendar day (sum across that day's workouts) |
| `first-handstand` | 🤸 | First handstand | a PersonalRecord exists for exercise `handstand` |
| `first-muscleup` | 🦾 | First muscle-up | a PersonalRecord exists for exercise `muscleup` |

## Decisions (confirmed with user)
- **Celebration:** animated pop-in **banner on the Progress screen** after Save to History (which already lands on Progress). Auto-dismisses; tone is subtle/encouraging.
- **Muscle-up:** **add a `muscleup` exercise** to the seed so 🦾 has a real data path. Also rewire `pullup.harderId = "muscleup"` so the library progression chain reaches it.

## Data model
- New SQLite table `AchievementUnlock { [PrimaryKey] string Id; DateTime UnlockedUtc }`. Registered in `Database.InitAsync`.
- **Seed:** add `muscleup` to `Cali/Resources/Raw/seed-data.json` (→ 19 exercises). Fields: `id:"muscleup"`, `name:"Muscle-ups"`, `primary:"Back"`, `secondary:"Arms, Chest"`, `difficulty:"Advanced"`, `equipment:"pull-up bar"`, `type:"reps"`, `easierId:"pullup"`, `harderId:"muscleup"`, with description/cues/mistakes consistent with the other entries. Set `pullup.harderId = "muscleup"`.
- The design-handoff `seed-data.json` stays pristine (reference). `Cali.Tests/Fakes/FileSeedDataProvider` is repointed to read `Cali/Resources/Raw/seed-data.json` so tests validate the app's actual seed (single source of truth). Count assertions updated 18 → 19.

## `AchievementService` (`Cali.Core/Services/AchievementService.cs`, singleton)
- **Pure / unit-tested:** `static HashSet<string> EvaluateMet(IReadOnlyList<WorkoutRecord> history, IReadOnlyList<PersonalRecord> prs, int bestStreak)` — implements the criteria table.
- `Task<List<string>> SyncAsync(DateTime nowUtc, bool celebrate)` — loads history (HistoryRepository), prs (PrRepository), bestStreak (SettingsService); computes met; inserts `AchievementUnlock` rows for met ids not already persisted; if `celebrate`, appends those net-new ids to `PendingCelebrations`; returns the net-new ids.
- `Task<List<AchievementStatus>> GetStatusAsync()` — all seed achievements (id/icon/name from `ISeedDataProvider`) joined with the persisted unlocked set. Drives the Progress badges (`Unlocked` flag → opacity).
- `List<string> PendingCelebrations { get; }` — FIFO queue drained by the Progress view.
- Owns `AchievementUnlock` persistence directly via `IDatabase` (small surface; no separate repo).

`AchievementStatus { string Id, Icon, Name; bool Unlocked }`.

## Evaluation timing (where Sync is called)
- **App startup** (`App.InitializeDataAsync`, after seed): `await SyncAsync(now, celebrate:false)` — silently backfills seed-met achievements so they're already unlocked before any Progress view (no false celebration).
- **Save to History** (`SummaryViewModel.SaveToHistory`, after history + PRs written): `await SyncAsync(now, celebrate:true)` — genuinely-earned unlocks get queued.
- **Backup import** (`ProfileViewModel.Import`, after `ImportJsonAsync`): `await SyncAsync(now, celebrate:false)` — re-unlock from imported data, no celebration.
- **Progress load** (`ProgressViewModel.LoadAsync`): `GetStatusAsync()` for badges; drain `PendingCelebrations` → trigger the banner. **Does not Sync** (so it never celebrates seed-met).

## UI
- **Badges:** `ProgressViewModel` builds its achievement list from `GetStatusAsync()` (remove the old `switch`). `AchievementVM.ItemOpacity` unchanged (1.0 / 0.35).
- **Celebration banner:** new element at the top of `ProgressPage` — accent-gradient `Border` with the icon + "Achievement unlocked — {name}". Bound to `CelebrationVisible` / `CelebrationIcon` / `CelebrationText`. Code-behind animates a pop-in (scale 0→1 SpringOut + fade) on appear, then auto-hides after ~3 s (or tap to dismiss). If multiple are queued, show the first this load (rest surface on subsequent loads — acceptable; typically ≤1 per workout).

## Backup
No change to `BackupData`. Achievements are re-derived from imported history/PRs via the import-time `SyncAsync`, so unlock state is not separately serialized.

## Tests
- `AchievementServiceTests`:
  - `EvaluateMet` — each criterion individually; `hundred-day` boundary (99 → not met, 100 → met, split across two same-day workouts → met); empty inputs → none.
  - `SyncAsync` against a temp SQLite DB — first call returns met set + persists; second call returns empty (idempotent); `celebrate:true` populates `PendingCelebrations`, `celebrate:false` does not.
- Update `ModelsTests` / `ExerciseRepositoryTests`: 18 → 19 exercises; `muscleup` present and its `easierId`/`harderId` resolve; `pullup.harderId == "muscleup"`.

## Out of scope
Date-based streak auto-increment (still uses the seeded best streak); achievement detail/share screens; additional achievements beyond the seed's five.
