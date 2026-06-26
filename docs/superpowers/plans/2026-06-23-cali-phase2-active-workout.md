# Cali — Phase 2: Active Workout + Rest Timer (HERO) — Implementation Plan

**Goal:** A full-screen Active Workout flow with always-visible count-up session timer, set tracking, accent progress bar, and a count-down Rest overlay that fires sound + haptics at zero — driven by a single, fully unit-tested `WorkoutSessionService`.

**Architecture:** `WorkoutSessionService` (in `Cali.Core`, an `ObservableObject`) owns all live state and subscribes to `ITicker`. It's unit-tested against a `FakeTicker` that advances time deterministically. The `ActiveWorkoutPage` binds directly to the service via `ActiveWorkoutViewModel`. `AudioHapticService` (platform) reacts to the service's `RestElapsed` event, gated by settings.

**Tech Stack:** Adds `CommunityToolkit.Mvvm` to `Cali.Core`. Reuses Phase 1 services. Audio via a simple platform sound (Phase 2 uses `Vibration`/`HapticFeedback` + a short tone; full `MediaElement` deferred until the toolkit/MAUI-version question is resolved).

## Global Constraints (inherited from Phase 1 spec + design tokens)

- Session timer counts **up** every tick while `Running` — continues during rest (rest is an overlay, not a pause).
- Rest decrements when `RestActive`; at 0 → `RestActive=false`, raise `RestElapsed`, fire sound+haptic **iff** settings allow.
- Complete-set fills the next set dot and auto-starts rest **iff** `AutoStartRest` setting is on.
- Progress = `(CurIdx + DoneSetsForCurrent / TotalSetsForCurrent) / ExerciseCount`.
- Hero visuals (screenshots 06/07): big numerals Barlow Condensed (rep target 64, rest countdown 84), accent `#FF6B1A`, warm radial bg, ≥56px buttons, uncluttered.

---

## `WorkoutSessionService` contract (the hero)

```csharp
// State (observable):
bool Running;               int SessionSeconds;     int CurIdx;
bool RestActive;            int RestSeconds;        int RestTotal;
IReadOnlyList<WorkoutItem> Items;   // resolved on Start

// Derived (computed/observable):
int ExerciseCount;          WorkoutItem? Current;   WorkoutItem? Next;
int DoneSetsForCurrent;     int TotalSetsForCurrent;
string SessionDisplay;      // m:ss   e.g. "0:02"
string RestDisplay;         // m:ss
double Progress;            // 0..1
int TargetValue;            bool CurrentIsHold;     // 64px numeral + "REPS"/"HOLD" label

// Commands:
void Start(Plan plan);          // resolves Items via ExerciseRepository, Running=true, ticker.Start
void CompleteSet();             // ++doneSets[CurIdx] (cap at total); auto-StartRest if setting on
void StartRest(int? total=null);// RestActive=true, RestTotal=total ?? Current.RestSec, RestSeconds=RestTotal
void AddRest(int sec=30);       // RestSeconds += sec
void SkipRest();                // RestActive=false
void SetRestPreset(int sec);    // RestTotal=RestSeconds=sec (during rest)
void Next();                    // cancel rest; CurIdx++ ; if past last -> raise Finished
void Prev();                    // cancel rest; CurIdx-- (>=0)
void End();                     // Running=false, ticker.Stop, raise Finished

// Events:
event Action? RestElapsed;      // -> sound + haptic (settings-gated, wired in the page/VM)
event Action? Finished;         // -> navigate to Summary (Phase 5; Phase 2 just closes modal)

// WorkoutItem: { Exercise Exercise; int Sets; int? Reps; int? HoldSec; int RestSec }
```

Tick handler: `if (!Running) return; SessionSeconds++; if (RestActive){ RestSeconds--; if (RestSeconds<=0){ RestActive=false; RestElapsed?.Invoke(); } }`

---

## Task 1: `WorkoutSessionService` + TDD against `FakeTicker`

**Files:** Create `Cali.Core/Services/WorkoutSessionService.cs`, `Cali.Core/Models/WorkoutItem.cs`; add `CommunityToolkit.Mvvm` to `Cali.Core.csproj`; create `Cali.Tests/Fakes/FakeTicker.cs`, `Cali.Tests/WorkoutSessionServiceTests.cs`.

**Tests (write first, must fail, then implement):**
1. `Session_counts_up_each_tick` — Start(push-day); fire 3 ticks → SessionSeconds==3, SessionDisplay=="0:03".
2. `Resolves_items_from_plan` — Items.Count==6 for push-day; Current.Exercise.Name=="Push-ups"; Next.Exercise.Name=="Pike Push-ups".
3. `CompleteSet_fills_dot_and_autostarts_rest` — settings AutoStartRest=true; CompleteSet → DoneSetsForCurrent==1, RestActive==true, RestTotal==Current.RestSec (60).
4. `CompleteSet_no_autostart_when_setting_off` — AutoStartRest=false; CompleteSet → RestActive==false, DoneSetsForCurrent==1.
5. `Rest_counts_down_and_fires_elapsed` — StartRest(3); fire 3 ticks → RestActive==false, RestSeconds==0, RestElapsed fired once; session also advanced 3.
6. `AddRest_and_presets` — StartRest(30); AddRest() → RestSeconds==60? (30+30); SetRestPreset(90) → RestTotal==90 && RestSeconds==90.
7. `Skip_cancels_rest`.
8. `Next_and_Prev_move_index_and_cancel_rest`; `Next_past_last_raises_Finished`.
9. `Progress_formula` — at CurIdx=1 with 1/3 done of 6 exercises → Progress==(1+1/3)/6.

**Key impl notes:** `Start` uses injected `ExerciseRepository` (already InitAsync'd at app start) to map each `PlanExercise`→`WorkoutItem`. Service is `ObservableObject`; derived props raise change notifications in setters of the backing state. `SessionDisplay`/`RestDisplay` format `mm:ss` as `m:ss` (minutes not zero-padded, seconds 2-digit).

## Task 2: `AudioHapticService` + settings-gated rest-end feedback

**Files:** Create `Cali/Platform/AudioHapticService.cs` (interface `IFeedbackService` in Core). On `Play()`: if Haptics setting → `HapticFeedback.Default.Perform(HapticFeedbackType.LongPress)` + `Vibration.Default.Vibrate(400ms)`; if Sound setting → play a short bundled tone (`Resources/Raw/rest-end.wav`) via platform audio (Android `RingtoneManager`/`ToneGenerator` fallback if MediaElement unavailable). Wire in the page: subscribe `WorkoutSessionService.RestElapsed += () => feedback.Play()`.

> Audio fallback: if no clean cross-platform player without CommunityToolkit.Maui, Phase 2 ships **haptic + system notification tone** via Android `ToneGenerator`; full custom sound revisited with MediaElement in a later phase. Log which path is used.

## Task 3: Reusable controls

**Files:** `Cali/Controls/StripedPlaceholder.cs` (GraphicsView 45° `#15171C`/`#1A1D23` stripes + centered `[ demo · {Name} ]`), `Cali/Controls/SetDots.cs` (bindable `Total`/`Done`, draws filled-accent vs hollow dots), `Cali/Controls/RingTimer.cs` (GraphicsView: accent ring, optional progress arc).

## Task 4: `ActiveWorkoutViewModel` + `ActiveWorkoutPage` (hero) + rest overlay

**Files:** `Cali/ViewModels/ActiveWorkoutViewModel.cs`, `Cali/Views/ActiveWorkoutPage.xaml(.cs)`.
- Layout top→bottom (match screenshot 06): top bar `End | SESSION m:ss (accent) | N/6`; thin accent progress bar (`Progress`); 200px `StripedPlaceholder`; exercise name (Barlow Condensed 34) + "Next: …"; 64px target numeral + "REPS·N SETS" + `SetDots`; bottom `✓ COMPLETE SET` (62px) + `‹ Prev / ⏱ Rest / Next ›`.
- Rest overlay (screenshot 07): dimmed full cover, "REST", `RingTimer` 240px, 84px `RestDisplay`, `+30s` / `Skip rest →`, presets 30/60/90 (active = accent). Visible bound to `RestActive`.
- Page subscribes `RestElapsed`→feedback, `Finished`→`Navigation.PopModalAsync()` (Summary in Phase 5).

## Task 5: Launch + wire + emulator verify

**Files:** temporary launch entry — add a `▶ Start sample workout` button to `HomePage` that resolves the seeded `push-day` plan and `Navigation.PushModalAsync(new ActiveWorkoutPage(...))`. (Replaced by Plans-tab card tap in Phase 3.)
- Verify on emulator: session timer ticks up; Complete Set fills a dot + opens rest overlay; rest counts down, +30s/skip work, presets switch; at 0 a vibration fires; Prev/Next move exercises. Screenshot active + rest, compare to 06/07.

## Self-Review checklist
- Coverage: timer up ✓(T1.1), resolve items ✓(T1.2), complete-set+autorest ✓(T1.3/4), rest countdown+elapsed ✓(T1.5), +30/skip/presets ✓(T1.6/7), next/prev/finished ✓(T1.8), progress ✓(T1.9), sound+haptic ✓(T2), hero UI ✓(T4), rest overlay ✓(T4), launch+verify ✓(T5).
- No placeholders in shipped code; the audio-fallback note is a real platform decision, logged at runtime.
