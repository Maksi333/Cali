# Handoff: Bodyweight Calisthenics App

## Overview
A mobile-first, dark-themed calisthenics training app for bodyweight-only workouts (no
weights, minimal equipment). It must feel athletic, motivating, and fast to use mid-workout:
large tap targets, minimal typing, numbers readable at a glance. The app is **local-first** —
plans and history live on-device, persist between sessions, and work offline with no account.

The prototype covers the core flow end-to-end: onboarding → home/dashboard → plans → plan
builder → exercise library → exercise detail with progressions → **active workout with live
timers (the hero)** → rest countdown → post-workout summary → progress/history → profile/settings.

## About the Design Files
The file in this bundle (`Calisthenics App.dc.html`) is a **design reference created in HTML**
— an interactive prototype showing the intended look and behavior. It is **not production code
to copy directly.**

This app is being built with **.NET MAUI**. The task is to **recreate these designs as MAUI
pages/views in XAML** (with C# code-behind or MVVM view-models), using MAUI's layout primitives
(`Grid`, `VerticalStackLayout`, `FlexLayout`, `CollectionView`, `Border`, `Shell` for tab
navigation), `Microsoft.Maui.Storage.Preferences`/SQLite for local persistence, and platform
features for haptics (`HapticFeedback`) and audio. Use the prototype for exact layout, color,
type, spacing, and interaction values — not as markup to port.

The prototype runs as a single self-contained component; open `Calisthenics App.dc.html` in a
browser to interact with it (tabs, timers, filters, and navigation all work).

## Fidelity
**High-fidelity (hifi).** Final colors, typography, spacing, radii, and interactions are
intended to be implemented as specified below. Recreate the UI faithfully using MAUI controls
and styles.

---

## Design Tokens

### Colors
| Token | Hex | Use |
|---|---|---|
| `bg` | `#0D0E11` | App background (near-black, cool) |
| `bg-warm-glow` | radial `#1A130C → #0D0E11` | Onboarding / active-workout / summary backgrounds |
| `surface` | `#15171C` | Cards, list rows, inputs |
| `surface-2` | `#1C1F26` | Inset chips, toggles-off track, secondary fills |
| `line` | `#262A33` | Borders / hairlines |
| `line-soft` | `#1C1F26` | Divider lines inside grouped lists |
| `text` | `#F3F4F6` | Primary text |
| `text-muted` | `#9499A3` | Secondary text |
| `text-faint` | `#6B7079` | Tertiary labels, inactive nav |
| `accent` | `#FF6B1A` | Primary action, timers, progress, active states |
| `accent-grad` | `linear-gradient(135deg,#FF6B1A,#FF8C42)` | Streak card, PR banner |
| `accent-tint` | `#251710` | Accent-on-dark chip background |
| `accent-on` | `#0D0E11` | Text/icon on accent fills |
| `success` | `#3ECF8E` | Beginner difficulty, "too easy / just right (easier)" |
| `danger` | `#FF4D6B` | Advanced difficulty, "too hard" |
| `danger-x` | `#FF4D4D` | "Common mistakes" ✕ marks |

Difficulty colors: Beginner `#3ECF8E`, Intermediate `#FF6B1A`, Advanced `#FF4D6B`
(badge = color text + `color + 55` alpha border, transparent fill).

### Typography
- **Display / headings / numerals:** `Barlow Condensed`, weights 600 & 700.
- **Body / UI:** `Barlow`, weights 400/500/600/700.
- Both are Google Fonts — bundle the `.ttf` files in the MAUI app and register via
  `fonts.AddFont(...)`. (Apple/Android substitute: any condensed grotesk for display.)

| Role | Family | Size | Weight | Notes |
|---|---|---|---|---|
| Screen title (Plans/Exercises/Progress) | Barlow Condensed | 32 | 700 | line-height ~1 |
| Greeting H1 (Home) | Barlow Condensed | 30 | 700 | |
| Card title (plan/exercise/section) | Barlow Condensed | 18–28 | 600–700 | |
| Hero session timer | Barlow Condensed | 26 | 700 | letter-spacing 1px, accent |
| Active exercise name | Barlow Condensed | 34 | 700 | |
| Rep/target numeral | Barlow Condensed | 64 | 700 | line-height .9 |
| Rest countdown numeral | Barlow Condensed | 84 | 700 | accent |
| Streak numeral | Barlow Condensed | 52 | 700 | on accent gradient |
| Body copy | Barlow | 14–16 | 400–500 | line-height 1.45–1.55 |
| Chips / meta | Barlow | 11–13 | 600–700 | |
| Kicker / eyebrow | Barlow Condensed | 10–13 | 700 | uppercase, letter-spacing 2–3px, accent |
| Nav label | Barlow | 10 | 700 | letter-spacing .3px |

### Spacing & shape
- Screen horizontal padding: **22px**. Section gaps: 12–22px.
- Card padding: 14–22px. Card radius: **16–22px**. Chip/button radius: 9–15px. Pills: 7–11px.
- Large CTA buttons: height 54–62px, radius 15–18px.
- Inputs / search: height 46–52px, radius 13–14px.
- Bottom nav height **84px** (incl. 20px bottom safe-area), top status bar **46px**.
- Phone frame in prototype: **390 × 844** (use as the iPhone reference; MAUI auto-scales).

### Shadows / effects
- Frame shadow (prototype only): `0 30px 80px rgba(0,0,0,.55)`.
- Bottom nav: `rgba(13,14,17,.9)` + `backdrop-filter: blur(16px)`, top border `#1C1F26`.
- Demo/image areas: 45° striped placeholder `repeating-linear-gradient(45deg,#15171C 0–8px,#1A1D23 8–16px)` — replace with real looping demos / illustrations.

---

## Screens / Views

> Tab bar (bottom, persistent on the 5 main tabs): **Home ◆ · Plans ▦ · Exercises ≣ · Progress ▲ · You ●**. Active item = accent `#FF6B1A`; inactive = `#6B7079`. Implement with MAUI `Shell` `TabBar` or a custom bottom bar. Hidden during onboarding and all full-screen overlays (workout, summary, builder, exercise detail).

### 1. Onboarding (4 steps, skippable)
- **Purpose:** capture goal → experience → push-up baseline to recommend a starting plan/difficulty.
- **Layout:** full-screen, warm radial bg, 64px top padding. App logomark (54px, accent rounded square, "C"), kicker, 44px Barlow Condensed title, muted body, then tappable option cards (`surface`, `line` border, 14px radius, label + muted hint, left-aligned). Progress dots row at bottom (active dot = 22px wide accent pill). Full-width accent CTA + "Skip".
- **Steps:** 1 Welcome (CTA "Get started"), 2 Goal (Build strength / Learn skills / Lose fat / Mobility), 3 Experience (Brand new / Some / Experienced), 4 Baseline push-ups (0–5 / 6–15 / 16+, CTA "Build my plan").
- **Behavior:** tapping an option or CTA advances; last step → app. Skip → app.

### 2. Home
- **Purpose:** dashboard + one-tap workout start.
- **Components:** greeting header + 46px circular avatar "AX"; **streak card** (accent gradient, "CURRENT STREAK", 52px "12 days", 🔥 + "Best: 21"); **today's plan card** (kicker, "Push Day", meta chips ⏱28 min / 6 exercises / muscles, full-width accent **▶ START WORKOUT** → opens active workout); 3-up quick-stat grid (This week 4 / Time 48m / New PRs 3).

### 3. Plans
- **Purpose:** browse predefined plans, manage own.
- **Components:** title + accent **+ New** (→ builder). Segmented toggle **Predefined / My plans** (active segment = accent fill). Plan cards: 3px **left accent bar** (`#FF6B1A` preset, `#3A3F49` custom), Barlow Condensed name, **PRESET**/**CUSTOM** badge (preset = accent-tint, custom = surface-2), meta chips (difficulty / ⏱duration / muscles). Tapping a card starts that workout (in prototype). Preset plans: Full-Body Beginner, Push Day, Pull Day, Leg Day, Core Crusher, Upper-Body Burnout. Custom: My Morning Flow, Skill Practice.

### 4. Plan Builder (full-screen overlay, slides up)
- **Purpose:** name a plan and configure exercises.
- **Components:** header `Cancel | New plan | Save`. Editable name input (Barlow Condensed 22). Summary line ("~16 min · N exercises · Full Body"). Per-exercise card: drag handle ⠿, name, 🗑 remove, then 3 inset stat boxes **Sets / Reps (or "Hold" for isometrics) / Rest**. Dashed **+ Add exercise** button.
- **MAUI note:** implement reorder via `CollectionView` drag or up/down; stat boxes are tappable steppers; persist on Save.

### 5. Exercises (library)
- **Purpose:** filterable bodyweight exercise library.
- **Components:** title; search field (🔍 + input); horizontally-scrolling **muscle chips** (All, Chest, Back, Shoulders, Arms, Core, Legs, Full Body); **difficulty chips** (All / Beginner / Intermediate / Advanced); count label; exercise rows (46px striped thumb, name + "primary · equipment" sub, difficulty badge, ›). Filters combine (muscle AND difficulty AND search). Tapping a row → detail.

### 6. Exercise Detail (full-screen overlay, slides up)
- **Purpose:** learn the movement and scale it.
- **Components:** 240px striped demo header (`[ looping demo · Name ]` placeholder) + circular ✕ back. Name + difficulty badge; muscle chips (primary accent-tint, secondary + equipment); description; numbered **Form cues**; **Progressions** — two cards: **← Easier** (success-green eyebrow) and **Harder →** (accent eyebrow), each tappable to navigate to that exercise's detail (this is the signature feature — keep it prominent); **Common mistakes** with red ✕. 19 exercises seeded (push-ups, incline/diamond/pike push-ups, dips, pull/chin-ups, inverted rows, squats, lunges, pistol squats, glute bridges, planks, hollow holds, leg raises, mountain climbers, burpees, handstand holds).

### 7. Active Workout — **HERO** (full-screen overlay)
- **Purpose:** run the workout with always-visible timers; uncluttered, big numbers/buttons.
- **Layout (top→bottom):** top bar `End | SESSION count-up timer (accent) | exercise position N/6`; thin accent **progress bar**; 200px striped demo; exercise name (34) + "Next: …"; big **64px rep/hold target** beside **set dots** (filled accent as completed); bottom: full-width accent **✓ COMPLETE SET** (62px) + row of **‹ Prev / ⏱ Rest / Next ›**.
- **Behavior:**
  - **Session timer** counts **up** every second while running, always visible.
  - **Complete set** marks one set done (fills next dot) and **auto-starts the rest countdown**.
  - **Rest timer overlay:** dimmed full cover, "REST", 240px ring, **84px countdown**, **+30s** and **Skip rest →**, plus presets **30s / 60s / 90s** (active preset = accent). Counts down to 0 then dismisses — on MAUI **fire sound + haptic/vibration** at end (respect settings toggles).
  - Next on last exercise → Summary. End → Summary. Prev/Next cancel any active rest.

### 8. Post-Workout Summary (full-screen overlay, celebratory)
- **Components:** animated check (accent circle with expanding ring pulse + pop-in ✓), "WORKOUT COMPLETE" kicker, "Nailed it. 💪", plan + count. 2×2 stat grid (Duration = actual session time, Exercises, Total sets, Total reps). Accent-gradient **PR banner** 🏆 ("New personal record! Diamond push-ups · 18 reps"). Effort segmented control **Too easy / Just right / Too hard** (selected fills its semantic color). Full-width accent **SAVE TO HISTORY** → returns to app on Progress tab and appends to history.

### 9. Progress
- **Components:** 2×2 stat cards (This week 4 sessions / Total volume 1.2k reps / Time trained 5.4 hrs / Longest streak 21 days); **Weekly volume** bar chart (7 bars, current day highlighted accent, others `#2D323B`, "↑14% vs last wk"); horizontally-scrolling **Achievements** badges (unlocked full opacity, locked ~0.35: 🥇 First workout, 🔥 7-day streak, 💯 100 in a day, 🤸 First handstand, 🦾 First muscle-up); **Recent workouts** history rows (date chip, name, "min · N exercises", effort badge). Tone: encouraging, never shaming.

### 10. Profile / Settings
- **Components:** profile header (58px accent avatar "AX", name, "Goal: Skill · Intermediate"); 2-up stats (Bodyweight 74 kg / Max pull-ups 14); grouped **toggle list** (Sound alerts ✓, Vibration/haptics ✓, Auto-start rest timer ✓, Auto-progression ✗, Workout reminders ✓ — accent track when on); **Export data backup** and **Import backup** buttons. (Backup = export/import all plans + history to a file — important for local-first.)

---

## Interactions & Behavior
- **Navigation:** 5-tab bottom bar; full-screen overlays for workout, summary, builder, and exercise detail slide up (`@keyframes slideup`, ~0.25–0.3s ease, translateY 100%→0). Overlays hide the tab bar.
- **Timers:** one 1-second tick drives both the count-up session timer and the countdown rest timer. Format `m:ss`. On MAUI use a `IDispatcherTimer`; keep timers running/accurate if the app backgrounds mid-workout.
- **Rest end:** sound + vibration/haptic (gated by settings); user can +30s or skip.
- **Progressions:** easier/harder cards re-target the detail view to the linked exercise.
- **Animations:** summary check `pop` 0.5s + repeating `ring` 1.4s pulse; progress bar width transition 0.3s; toggle knob slide.
- **Motion intent:** subtle satisfying feedback on set complete, PR, finish, achievement unlock. Keep active-workout & rest screens uncluttered.

## State Management
- `screen`: `onboarding | app`; `onbStep` (0–3).
- `tab`: `home | plans | exercises | progress | profile`; `overlay`: `null | detail | builder | workout | summary`.
- Workout: `running` (bool), `sessionSec`, `curIdx` (exercise index), `doneSets` (map idx→count), `restActive`, `restSec`, `restTotal` (preset 30/60/90).
- Library filters: `muscle`, `diff`, `q` (search).
- Detail: `detailId`. Builder: `builderName`. Summary: `effort` (0/1/2).
- **Persistence (local-first):** user-created plans, full workout history, streak, PRs, profile, and all settings persist on device (SQLite or `Preferences` + JSON). Export/import to a file for backup.

## Assets
- **Fonts:** Barlow & Barlow Condensed (Google Fonts) — bundle and register in MAUI.
- **Exercise demos & muscle diagrams:** the prototype uses striped placeholders labeled
  `[ looping demo · … ]`. Replace with real looping animations/illustrations and a small
  highlighted-muscle diagram per exercise. No production imagery is included in this bundle.
- **Icons:** glyph/emoji placeholders (◆ ▦ ≣ ▲ ● for nav, 🔥 🏆 ▶ ✓ etc.). Swap for the
  app's real icon set in MAUI (e.g. a font icon pack or SVG resources).

## Files
- `Calisthenics App.dc.html` — the full interactive prototype (all screens & flows). Open in a browser to interact.
- `support.js` — runtime that powers the prototype; reference only, not for porting.
- `IMPLEMENTATION_MAUI.md` — step-by-step .NET MAUI build guide: control mapping, the timer/session service, persistence, and recommended build order.
- `seed-data.json` — all 19 exercises (with cues, mistakes, progressions, muscle/difficulty/equipment), the 6 predefined plans (with sets/reps/holds/rest), skill goals, achievements, default settings, and sample history/profile. Use to seed the local DB on first launch.
- `screenshots/` — rendered reference of every screen:
  `01-onboarding`, `02-home`, `03-plans`, `04-exercises`, `05-exercise-detail`,
  `06-active-workout`, `07-rest-timer`, `08-summary`, `09-progress`, `10-profile`.
