# Usability batch: per-set rep logging + keep-awake + rest-end beep

> Date: 2026-06-25 · Status: approved (design) · Stack: .NET 10 MAUI (Cali)

Three usability improvements to the active-workout experience, in priority order.

## 1. Log actual reps per set (inline −/+)

**Problem.** Today the session only counts *completed* sets; logged reps are assumed = plan target × sets.
So volume, PRs and achievements can be inaccurate (you targeted 12, did 10).

**Decision (UX).** Inline `[ − ] value [ + ]` on the active screen's big number. The value defaults to the set's
target; the user only taps −/+ when they did more/fewer, then **Complete Set** logs that exact number. No popups.

**Core — `WorkoutSessionService`:**
- Replace `int[] _doneSets` (counts) with `List<int>[] _setLog` — `_setLog[i]` holds the actual value
  (reps, or hold-seconds) of each completed set of exercise `i`.
- Add `int PendingValue` (observable) = the adjustable on-screen value. `IncPending()/DecPending()`
  step by 1 (reps) / 5 (holds), clamped 0–999. `ResetPending()` sets it to the current target; called on
  `Start`, after each non-ending `CompleteSet`, and on `GoNext`/`GoPrev`.
- `CompleteSet()` appends `PendingValue` to `_setLog[CurIdx]` (clamped to `Sets` so double-tap can't over-record),
  then resets pending. The state-machine branches (mid / advance / end) are unchanged.
- Derived: `DoneSetsForCurrent`/`DoneSetsAt` = list `.Count`; `TotalSetsDone` = Σ counts;
  `TotalRepsDone` = Σ logged reps (rep exercises). Add `BestSetAt(i)` = max logged value, `LoggedSumAt(i)` = Σ logged.
- `TargetUnitLabel` now reads e.g. `REPS · SET 2 OF 4` (was `… 4 SETS`).

**Summary — `SummaryViewModel`:** PR detection uses `BestSetAt(i)` (best *actual* set) instead of the target;
`WorkoutItemLog` records `Reps = LoggedSumAt`, `BestSetReps = BestSetAt`, `HoldSec = BestSetAt` (holds). `TotalReps`
already reads `TotalRepsDone`.

**UI — `ActiveWorkoutPage`:** the FontSize-64 value becomes `[−] {PendingValue} [+]` (small accent buttons,
≥44pt, `SemanticProperties`), bound to the session; everything else (dots, Next, rest overlay) unchanged.

**Tests:** pending defaults to target; adjusting then completing logs the adjusted value; `TotalRepsDone`/`BestSetAt`
reflect actuals not targets; pending resets on advance; holds log seconds. Existing 46 tests stay green (pending
defaults to target → identical behavior when not adjusted).

## 2. Keep screen awake during a workout — automatic

`DeviceDisplay.Current.KeepScreenOn = true` in `ActiveWorkoutPage.OnAppearing`, `= false` in `OnDisappearing`.
No setting, no Core change. (Summary is a separate page; the flag releases when the workout page disappears.)

## 3. Audible rest-end cue

In `AudioHapticService.Play()` (already invoked on rest-elapsed), add an Android `ToneGenerator` short beep under
`#if ANDROID`, gated on the existing `SoundAlerts` setting — which finally makes the "Sound alerts" toggle audible.
No new package (sidesteps the `MediaElement` workload-pin block). The instance is created, plays a brief
`PropBeep2`, and is released after a short delay.

## Out of scope
Per-set rest editing, RPE/notes, custom rep ranges — not in this batch.
