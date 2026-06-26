# Cali — Release Readiness Changelog

Work against `RELEASE_TASKS.md`. Stack detected: **.NET 10 MAUI** (Android target verified on emulator
`Medium_Phone_API_36.0`), MVVM (`ViewModels/` + `Views/`, CommunityToolkit.Mvvm), local-first persistence
(`SettingsService` over Preferences; SQLite repositories via `IDatabase`). Brand accent **orange `#FF6B1A`**
(single source: `Resources/Styles/Colors.xaml` + `Theme/Tokens.cs`).

Build status: **Core + tests green (45/45), Android build clean (0 errors; only pre-existing warnings:
the known `NU1903` SQLite CVE — no fixed version exists — and a few pre-existing obsolete-API `CS0618`
warnings I did not introduce).**

---

## Usability follow-up (2026-06-25) — post-release batch

Three active-workout usability improvements (spec: `docs/superpowers/specs/2026-06-25-usability-rep-logging-keepawake-beep-design.md`). **50 Core tests pass; Android build clean; emulator-verified.**

- **Log actual reps per set** — `WorkoutSessionService` now stores a per-exercise log of the *actual* value done each set (`_setLog`) instead of a count, with a `PendingValue` shown as an inline `[ − ] 12 [ + ]` on the active screen (defaults to the target; tap to adjust). Volume, PR detection, and the saved per-exercise log now use **actual** logged reps/holds, not the plan target. Label reads "REPS · SET 2 OF 4". *(emulator-verified: stepper renders, −/+ adjusts; logging math covered by 4 new unit tests)*
- **Keep screen awake during a workout** — `DeviceDisplay.KeepScreenOn` set on the active page, released on leave. *(emulator-verified via window flags: `KEEP_SCREEN_ON` set during the workout, released after)*
- **Audible rest-end cue** — `AudioHapticService.Play()` now plays a short Android `ToneGenerator` beep, gated on the existing **Sound alerts** toggle (which is now functional). No new package (avoids the blocked MediaElement). *(builds clean + gating verified in code; audio not captured from the emulator)*

---

## P0 — Release blockers

### BUG-001 — Home profile/name icon now opens the profile
- `ViewModels/HomeViewModel.cs`: added `OpenProfileCommand` → `Shell.Current.GoToAsync("//you")`.
- `Views/HomePage.xaml`: the 46×46 avatar (≥44pt) is now a tappable control (`TapGestureRecognizer`),
  with `SemanticProperties` for screen readers; inner label is `InputTransparent` so the tap always lands.
- Routes to the "You" tab (the profile screen). Back is the standard tab behaviour; works on a fresh install
  (no profile yet).

### BUG-002 — Profile is editable and persists
- `Views/ProfilePage.xaml`: added an **Edit** affordance and an inline edit panel (Name, Goal/Level pickers,
  Bodyweight, Max pull-ups) shown via an `IsEditing` flag; read-only header/stats hide while editing.
- `ViewModels/ProfileViewModel.cs`: edit buffers + `BeginEdit`/`CancelEdit`/`SaveEdit`; saves through
  `SettingsService.Profile` (survives restart). Avatar initials recompute from the saved name. `Load()` is
  guarded so re-appearing doesn't discard an in-progress edit.
- Validation: `Cali.Core/Services/ProfileMath.ParseBounded` clamps numeric input to sensible ranges and treats
  empty/invalid as 0 (never throws). Unit standardised to **kg** (consistent; a kg/lb toggle is a future option).

### BUG-003 — "Complete Set" drives the workout state machine
- `Cali.Core/Services/WorkoutSessionService.CompleteSet` was only incrementing a counter; it now advances:
  - **mid-exercise** → record the set + auto-start rest (when enabled); the next set fills in;
  - **last set of an exercise** → rest for the just-finished exercise (when enabled) then advance to the next exercise;
  - **last set of the last exercise** → end the workout → Summary (records exactly one session).
- `GoNext()` past the last exercise now calls `End()` (stops the ticker cleanly instead of leaving it running).
- `End()` now also dismisses the rest overlay.
- Decision: rest auto-starts between exercises too (not only between sets) when *Auto-start rest* is on — better UX,
  satisfies all acceptance criteria; the final set ends with no trailing rest.
- Tests: 6 new boundary tests in `WorkoutSessionServiceTests` (mid/last set, last-of-last, single-set, single-exercise,
  hold-type, no-trailing-rest). All existing tests still pass.

### BUG-004 — Fresh state on first launch (commercial blocker)
- `Cali.Core/Services/SeedService`: **removed** the SampleProfile ("Alex"/74/14) and SampleHistory seeding entirely
  (not just gated) so a first launch is genuinely empty in every build. Predefined plans (content) and default
  settings still seed.
- `Resources/Raw/seed-data.json`: stripped the `sampleHistory` and `sampleProfile` blocks — no demo data ships at all.
- `App.InitializeDataAsync` still runs `AchievementService.SyncAsync` (now a no-op on empty history → zero unlocked).
- `ViewModels/OnboardingViewModel`: the welcome flow now **persists** the chosen Goal/Level into the profile instead
  of discarding them (push-up baseline is intentionally not mapped to pull-ups).
- Result: fresh install → onboarding shown, empty profile (neutral default name "Athlete"), 0 achievements,
  0 history, empty Progress. Tests updated: `SeedServiceTests` asserts fresh state; new `AchievementServiceTests`
  asserts an empty install unlocks nothing; `ModelsTests` updated.

---

## P1 — Polish

### BUG-005 — Plan card tags no longer clip
- `Views/PlansPage.xaml`: the tag chips moved from a non-wrapping `HorizontalStackLayout` to a wrapping
  `FlexLayout` (with tail-truncation on the muscles chip) — long muscle lists wrap to a second line instead of
  running off the card edge.

### BUG-006 — Plan builder stepper values are readable
- `Views/PlanBuilderPage.xaml`: each SETS/REPS/REST stepper was three-in-a-row with the value squeezed between the
  −/+ buttons (clipped on narrow screens). Restructured to **value on top, −/+ row below** so the value is always
  fully visible and centered; the "~X min · N exercises" summary still recalculates.

### BUG-007 — Orange accent on inputs (no more blue)
- `Platforms/Android/Resources/values/colors.xml`: `colorPrimary`/`colorPrimaryDark`/`colorAccent` changed from the
  default template purple (`#512BD4`/`#2B0B98`) to the brand orange — this drives the Entry underline + text caret
  app-wide (theme-level fix; covers all current and future inputs).
- Branding cleanup: app icon + splash recoloured to the dark/orange brand with a "C" monogram (was the `.NET`
  template wordmark on purple); placeholder `ApplicationId` `com.companyname.cali` → `com.cali.app`.

---

## P2 — Recommended for commercial release

### BUG-008 — Branded media placeholder (dropped "demo")
- `Controls/StripedPlaceholder.cs`: replaced the grey stripes + literal `[ demo · X ]` text with a branded tile —
  the exercise's initial in accent orange on an accent-tinted surface. Exercise list rows now pass the name so each
  thumbnail shows its initial.

### BUG-009 — Designed empty states
- Plans ("My plans" / Predefined) and Progress now show a branded empty state (icon badge + headline + subtext + CTA)
  instead of a blank screen / wall of zeros (`PlansViewModel.IsEmpty`, `ProgressViewModel.IsEmpty`/`HasData`).
- Achievements is catalog-driven (always shows the full 54 with locked states + progress bars) — that locked grid is
  already the "not yet earned" experience, so no spurious empty state was added.

### BUG-010 — Settings persistence & behaviour
- Audit: all 5 toggles persist. *Auto-start rest*, *Haptics*, *Sound alerts* are honoured (sound gate is wired; the
  audible tone itself remains deferred pending MediaElement). *Auto-progression* and *Workout reminders* are not yet
  implemented, so rather than ship toggles that do nothing they are now **disabled with a "Coming soon" caption**.

### BUG-011 — Accessibility & contrast
- `SemanticProperties.Description` added to icon/glyph-only controls (Home avatar; Active Workout End/Prev/Rest/Next/
  +30s/Skip; plan-builder move-up/down/remove + the six steppers; Exercises search; back buttons). Decorative glyphs
  (chevrons, drag handle) marked out of the accessibility tree.
- Tap targets raised to ≥44pt (plan-builder buttons 34→44, detail/achievements back buttons, End button).
- Contrast: `TextFaint` `#6B7079` (failed WCAG AA for body text) lifted to `#888E99` (passes AA on all surfaces).
  Accent and `TextMuted` already passed and were left unchanged.

---

## Review pass (adversarial QA against acceptance criteria)

A multi-agent review of the diff surfaced two genuine issues, both fixed:
- **`WorkoutSessionService.End()` made idempotent** (`if (!Running) return;`). Without it, re-tapping **End** during the
  Summary transition — or tapping Complete/Next after `GoNext` past the last exercise — could fire `Finished` twice and
  record **two** sessions, violating the BUG-003 "exactly one session" criterion. Added a regression test
  (`End_is_idempotent_records_exactly_one_finish`).
- **Progress weekly-trend arrow** no longer shows a contradictory "↑-43%": the glyph (↑/↓) is now chosen from the sign
  of the change and the percentage is rounded. (Pre-existing display bug.)

Reviewed-but-not-changed (intentional / low-risk): an in-progress profile edit is lost only if Android destroys the
cached tab page under memory pressure (inherent to not persisting transient edit state; Shell caches the page so risk is
low); the dead `Persist()` path for the two disabled toggles is harmless.

## Notes / recommended follow-ups for the owner
- **SQLite CVE `NU1903`** is surfaced as a build warning; no fixed `SQLitePCLRaw` version exists yet — left visible.
- **App icon** is a simple "C" monogram placeholder; commission a real brand mark before store submission.
- **`ApplicationId`** set to `com.cali.app` — replace with your own reverse-domain id before publishing.
- **kg/lb unit toggle**, real exercise media, audible rest-end tone, auto-progression, and workout reminders are
  intentionally out of scope (flagged above), not regressions.
- Pre-existing `CS0618` obsolete-API warnings (`DisplayAlert`/`FadeTo`/`ScaleTo`) predate this work; a future cleanup
  can move them to the `*Async` variants.
- **No in-app privacy policy / About screen.** The app is local-first (no account, no network data collection), but
  app stores still require a privacy-policy URL — add one before submission.
- **Release packaging:** do a clean build (`obj/Release` cleared during this work — a stale arm64 `colors.xml` copy still
  held the old purple); verify the regenerated resources are orange. App version is `1.0` (build `1`) — bump per release.
- **Large-font / very-narrow-device** layouts were not exhaustively device-tested; the fixes use flexible layouts
  (FlexLayout wrap, value-on-top steppers) that should hold, but a final pass at max system font size is recommended.
