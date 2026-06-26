# Cali — Bodyweight Calisthenics App (.NET MAUI) — Design Spec

**Date:** 2026-06-23
**Status:** Approved (brainstorming → ready for implementation plan)
**Source of truth for visuals:** `design_handoff_calisthenics_app/` (README.md, IMPLEMENTATION_MAUI.md, seed-data.json, screenshots/, Calisthenics App.dc.html prototype).

This spec captures the **engineering** decisions for recreating the handoff prototype as a .NET MAUI app. The handoff already dictates the visual spec (colors, type, spacing, per-screen layout); that is not re-derived here — it is referenced. This document records architecture, project structure, data/persistence, navigation, the timer service, asset strategy, and the phased build order.

---

## 1. Goal & constraints

Recreate the 10-screen, dark-themed, **local-first** bodyweight calisthenics app faithfully (hifi) using MAUI controls. Must feel athletic and fast mid-workout: large tap targets, minimal typing, glanceable numbers. No account, offline, on-device persistence. The signature/hero experience is the **Active Workout with live timers** (count-up session + count-down rest, sound + haptics).

**Scope of this build:** all 10 screens end-to-end + persistence + backup.
**Out of scope (future phase):** handoff §6.8 enhancements — skill tree, auto-progression, circuit/AMRAP/EMOM/Tabata modes, warm-up/cooldown, calendar, reminders/notifications. (`autoProgression` setting toggle is shown but inert.)

## 2. Platform & tooling

- **.NET 10 MAUI**, single project. Existing csproj targets `net10.0-android/-ios/-maccatalyst/-windows`. Keep all targets; **primary verification target = Android emulator**.
- Theme: **dark only** — `UserAppTheme = AppTheme.Dark`, dark system status bar.
- Packages to add: `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui` (MediaElement, FileSaver, behaviors), `sqlite-net-pcl`.
- Fonts: **Barlow** (400/500/600/700) + **Barlow Condensed** (600/700), downloaded from the Google Fonts OFL repo into `Resources/Fonts/`, registered in `MauiProgram`, exposed as `StaticResource` font keys.

## 3. Architecture

- **MVVM** with `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`). One ViewModel per page.
- **DI**: services, ViewModels, and pages registered in `MauiProgram`. ViewModels receive services via constructor injection.
- **Singleton `WorkoutSessionService`** owns all live workout state and the single `IDispatcherTimer` (1 s tick) that drives both timers. The hero — see §7.
- **Repositories** over SQLite: `ExerciseRepository`, `PlanRepository` (+ plan-exercises), `HistoryRepository`, `PrRepository`. `SettingsService` wraps `Preferences` (toggles, streak counters, onboarding flag, profile basics). `SeedService` seeds the DB from `seed-data.json` on first launch. `BackupService` handles JSON export/import. `AudioHapticService` centralizes rest-end sound + vibration (settings-gated).

## 4. Project structure

```
/Models        Exercise, Plan, PlanExercise, WorkoutRecord, PersonalRecord, Achievement, Profile, AppSettings, enums (MuscleGroup, Difficulty, ExerciseType, Effort)
/Services      WorkoutSessionService, SeedService, SettingsService, BackupService, AudioHapticService,
               ExerciseRepository, PlanRepository, HistoryRepository, PrRepository, IDatabase (SQLiteAsyncConnection wrapper)
/ViewModels    HomeVM, PlansVM, ExercisesVM, ExerciseDetailVM, PlanBuilderVM, ActiveWorkoutVM, SummaryVM, ProgressVM, ProfileVM, OnboardingVM
/Views         HomePage, PlansPage, ExercisesPage, ExerciseDetailPage, PlanBuilderPage, ActiveWorkoutPage, SummaryPage, ProgressPage, ProfilePage, OnboardingPage
/Controls      Card, Chip, StatTile, DifficultyBadge, SetDots, SegmentedToggle, StripedPlaceholder, RingTimer, VolumeBarChart, PillToggle, MetaChipRow
/Resources/Styles   Colors.xaml (tokens), Styles.xaml (control styles)
/Resources/Images   hand-built SVG icon set
/Resources/Fonts    Barlow-*.ttf, BarlowCondensed-*.ttf
/Resources/Raw      seed-data.json (bundled), rest-end sound asset
```

ViewModels and controls are kept small and single-purpose so each is understandable and testable in isolation.

## 5. Theming & design tokens

Replace the template `Colors.xaml` with the handoff token set (README "Design Tokens"):

| Token | Hex |
|---|---|
| `bg` | `#0D0E11` |
| `surface` | `#15171C` |
| `surface-2` | `#1C1F26` |
| `line` | `#262A33` |
| `line-soft` | `#1C1F26` |
| `text` | `#F3F4F6` |
| `text-muted` | `#9499A3` |
| `text-faint` | `#6B7079` |
| `accent` | `#FF6B1A` |
| `accent-grad` | `#FF6B1A → #FF8C42` (135°) |
| `accent-tint` | `#251710` |
| `accent-on` | `#0D0E11` |
| `success` | `#3ECF8E` |
| `danger` | `#FF4D6B` |
| `danger-x` | `#FF4D4D` |

Difficulty: Beginner `#3ECF8E`, Intermediate `#FF6B1A`, Advanced `#FF4D6B` (badge = colored text + `color@55` border, transparent fill). Warm radial glow background (`#1A130C → #0D0E11`) for Onboarding / Active Workout / Summary.

**Type keys** (Barlow Condensed for display/numerals, Barlow for body/UI) per README "Typography" table. **Spacing:** screen padding 22; card radius 16–22; CTA height 54–62; inputs 46–52; bottom nav 84 (incl. 20 safe-area); top status 46. Reference phone frame 390×844.

`Styles.xaml` defines implicit + keyed styles for recurring primitives (cards, chips, CTA buttons, segmented toggles) so pages stay declarative.

## 6. Navigation

- **Shell `TabBar`** with 5 tabs: Home, Plans, Exercises, Progress, You. Active = accent `#FF6B1A`, inactive = `#6B7079`. **Decision:** start with Shell TabBar styled to the dark/84px spec; fall back to a custom bottom `Grid` bar (blur) only if Shell fidelity proves insufficient.
- **Full-screen modal flows** (Onboarding, Active Workout, Summary, Plan Builder, Exercise Detail) via `Navigation.PushModalAsync`, bottom **slide-up** transition (~0.25–0.3s), tab bar hidden.
- **Onboarding** shows on first launch only (gated on a `Preferences` flag); "Skip" or completing step 4 sets the flag → Home tab.
- Honor safe-areas (notch / home indicator).

## 7. Hero — `WorkoutSessionService`

Singleton holding session state + one `IDispatcherTimer` ticking at 1 s.

- **State:** `Running` (bool), `SessionSeconds`, `CurIdx`, `DoneSets` (map idx→count), `RestActive`, `RestSeconds`, `RestTotal` (preset 30/60/90), current `Plan`.
- **Session timer** counts **up** every second while running; always visible top, formatted `m:ss`, accent.
- **Rest countdown:** when `RestActive`, decrement `RestSeconds`; at 0 → `RestActive=false`, fire **sound** (`MediaElement`/audio) + **haptic/vibration** (`HapticFeedback`/`Vibration`), both gated by settings.
- **Complete set** → increment done-set count for current exercise (fills next dot); if `autoStartRest`, set `RestActive=true`, `RestSeconds=RestTotal`.
- **Rest controls:** +30s (`RestSeconds+=30`), Skip (`RestActive=false`), presets 30/60/90 set `RestTotal`/`RestSeconds`. Prev/Next cancel any active rest.
- **Progress bar value** = `(CurIdx + DoneSets/totalSets) / exerciseCount`.
- **Background safety:** persist `sessionStartUtc`; on resume recompute elapsed so the timer stays accurate if backgrounded mid-workout.
- Keep the screen uncluttered: big numerals (64–84 Barlow Condensed), big buttons (≥56).

## 8. Data model & persistence

**SQLite** (`sqlite-net-pcl`) for relational data; **`Preferences`** for flags/counters.

- `Exercise` — seeded, read-only. Fields per `seed-data.json`: `id`, `name`, `primary`, `secondary`, `difficulty`, `equipment`, `type` (reps|hold), `easierId`, `harderId`, `description`, `cues[]`, `mistakes[]`.
- `Plan` — `id`, `name`, `preset` (bool), `difficulty`, `estMinutes`, `muscles`; presets seeded read-only but **duplicatable**, user plans editable.
- `PlanExercise` — `planId`, `exerciseId`, `order`, `sets`, `reps?`, `holdSec?`, `restSec`.
- `WorkoutRecord` — `date`, `planName`, `durationSec`, `exercises`, `totalSets`, `totalReps`, `effort`.
- `PersonalRecord` — per exercise: max reps / longest hold; compared on workout completion.
- `Preferences`: settings toggles (sound, haptics, autoStartRest, autoProgression, reminders), `units`, streak counters (current/best), onboarding-complete flag, profile basics (name, goal, level, bodyweightKg, maxPullups).

**Seeding:** `SeedService` idempotently seeds **18 exercises** (seed-data.json is source of truth; README prose's "19" is an off-by-one), **6 preset plans**, sample history (3 records) + sample profile on first launch (flag-gated) so screens render alive. Achievements (5) and skill goals (8) loaded from seed for display.

**PRs:** track max reps / longest hold per exercise; surface a Summary celebration (accent-gradient PR banner) when beaten.

**Backup:** `BackupService` Export = serialize all plans + history (+ profile/settings) to a JSON file via `CommunityToolkit.Maui.FileSaver`/share sheet; Import = pick file + merge. No account/login.

## 9. Assets

- **Icons:** hand-built SVG vector set (~16) in `/Resources/Images`, recolored per state (active/inactive nav, on-accent). Covers: 5 nav glyphs (home/plans/exercises/progress/you), play, check, flame, trophy, clock, search, plus, minus, drag-handle, trash, back-x, chevron. Replaces all prototype emoji/glyph placeholders.
- **Demo/illustration areas:** reusable `StripedPlaceholder` control reproducing the 45° `#15171C/#1A1D23` repeating stripe with a centered `[ demo · Name ]` label (handoff sanctions placeholders until real demos exist). A real muscle-diagram/looping-demo pipeline is future work.
- **Rest-end sound:** a short bundled audio asset in `Resources/Raw`.
- **Fonts:** Barlow + Barlow Condensed `.ttf` (OFL) downloaded into `Resources/Fonts`.

## 10. Screens (reference)

Implement all 10 per handoff README "Screens / Views" (exact tokens/specs there) and `screenshots/`. (The Rest overlay — item 8 below — is a sub-state of Active Workout, listed separately for clarity, so the list shows 11 entries for 10 screens.)

1. **Onboarding** — 4 steps, skippable; warm radial bg; option cards; progress dots; accent CTA.
2. **Home** — greeting + avatar; streak card (accent gradient, 52px count); today's plan card → Start Workout; 3-up quick stats.
3. **Plans** — `+ New`; Predefined/My segmented toggle; plan cards (3px left accent bar, PRESET/CUSTOM badge, meta chips); tap → start.
4. **Plan Builder** (modal) — Cancel/New plan/Save; editable name; summary line; per-exercise card (drag handle, name, remove, Sets/Reps-or-Hold/Rest stepper boxes); + Add exercise.
5. **Exercises** — search; muscle chips; difficulty chips (combine AND); count label; rows (striped thumb, name, primary·equipment, difficulty badge, ›) → detail.
6. **Exercise Detail** (modal) — 240px striped demo + ✕ back; name + difficulty badge; muscle chips; description; numbered form cues; **Progressions** (← Easier success-green / Harder → accent, each navigates to that exercise — signature feature); common mistakes (red ✕).
7. **Active Workout** (modal, **hero**) — top bar End / SESSION count-up / N/6; accent progress bar; 200px striped demo; exercise name + Next; 64px rep/hold target + set dots; ✓ COMPLETE SET (62px) + Prev/Rest/Next.
8. **Rest overlay** — dimmed cover, REST, 240px ring, 84px countdown, +30s / Skip, presets 30/60/90; on 0 → sound+haptic.
9. **Summary** (modal, celebratory) — animated check + ring pulse; WORKOUT COMPLETE; plan + count; 2×2 stat grid; PR banner; effort segmented (Too easy/Just right/Too hard, semantic color); SAVE TO HISTORY → Progress tab + append history.
10. **Progress** — 2×2 stat cards; weekly volume bar chart (current day accent); achievements (unlocked/locked); recent workouts rows. Encouraging tone.
11. **Profile/Settings** — profile header (accent avatar, name, goal·level); 2-up stats; grouped toggle list (accent track on); Export/Import backup buttons.

## 11. Motion & polish

Slide-up modal transitions; set-complete tick feedback; summary check `pop` (0.5s) + repeating `ring` pulse (1.4s) via `Animation`/`ScaleTo`; progress-bar width transition; toggle knob slide. Calm, encouraging stats — never shaming. Respect sound/haptics settings everywhere.

## 12. Build order (phased; each phase ends runnable + verified on Android emulator)

1. **Foundation** — add packages; download/register Barlow fonts; tokens (`Colors.xaml`) + `Styles.xaml`; Shell + 5 empty tab pages; SQLite + `SeedService`; `SettingsService`. **First action: confirm MAUI Android workload + an emulator are available; surface immediately if not.**
2. **Active Workout + Rest timer** (hero) against a seeded plan — timers, set dots, progress bar, sound + haptics, background safety.
3. **Plans** tab (presets from seed) → start a workout from a card.
4. **Plan Builder** — create/edit/duplicate/delete, persist.
5. **Summary** → save to history; PRs + celebration animation.
6. **Exercises** library (filters + search) → **Exercise Detail** with easier/harder progressions.
7. **Onboarding**; **Progress** (stats, weekly chart, achievements, history); **Profile/Settings** + backup export/import.
8. **Motion & polish** — transitions, set-complete tick, summary animation, safe-areas, final fidelity pass vs screenshots.

## 13. Verification & "done"

- Each phase: `dotnet build` clean for `net10.0-android`, then launch on the Android emulator and screenshot the relevant screens to compare against `design_handoff_calisthenics_app/screenshots/`.
- A phase is done when it compiles, runs, the new screen(s) match the reference within reason, and persistence (where relevant) survives an app restart.
- **Risks:** (a) MAUI Android workload/emulator availability on this machine — checked in Phase 1; (b) Shell TabBar fidelity vs the 84px/blur spec — fallback to custom bar; (c) downloading binary `.ttf` from Google Fonts requires outbound network.

## 14. Decisions log

- Full 10-screen build, all platforms targeted, Android emulator as primary verify target.
- Barlow fonts downloaded from Google Fonts (OFL).
- Hand-built SVG icon set (not a font pack, not emoji).
- Shell TabBar first; custom bottom bar only if needed.
- §6.8 enhancements out of scope.
- `seed-data.json` is the data source of truth (18 exercises).
