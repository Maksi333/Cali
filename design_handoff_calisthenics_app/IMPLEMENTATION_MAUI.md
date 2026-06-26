# MAUI Implementation Guide

A practical map from the HTML prototype to a **.NET MAUI** build. Pair this with `README.md`
(exact tokens + per-screen specs), `seed-data.json` (DB seed), and `screenshots/`.

## 1. Project setup
- Target **net8.0 (or later)-android / -ios**. Single-project MAUI app.
- **Theme:** dark only. Set `UserAppTheme = AppTheme.Dark`; define all colors in `Resources/Styles/Colors.xaml` using the tokens from the README. Drive the system status bar to a dark style.
- **Fonts:** add `Barlow-Regular/Medium/SemiBold/Bold.ttf` and `BarlowCondensed-SemiBold/Bold.ttf` to `Resources/Fonts/`, register in `MauiProgram` via `fonts.AddFont(...)`, expose as `StaticResource` font keys.
- **Architecture:** MVVM with `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`). One ViewModel per page; a shared `WorkoutSessionService` for the live workout/timer state.

## 2. Navigation
- Use **Shell** with a bottom `TabBar` for the 5 tabs (Home, Plans, Exercises, Progress, You).
- Full-screen flows — **Onboarding, Active Workout, Summary, Plan Builder, Exercise Detail** —
  push as modal pages (`Navigation.PushModalAsync`) so the tab bar is hidden. Use a bottom
  slide-up transition to match the prototype.
- Onboarding shows on first launch only (gate on a `Preferences` flag); "Skip" or completing
  step 4 sets the flag and navigates to the Home tab.

## 3. Layout primitives mapping
| Prototype pattern | MAUI control |
|---|---|
| Scrolling screen | `ScrollView` > `VerticalStackLayout` (padding 22) |
| Card | `Border` (StrokeShape `RoundRectangle`, radius 16–22, Stroke `line`, Background `surface`) |
| Stat / chip grid | `Grid` with `ColumnDefinitions="*,*,*"` and `RowSpacing/ColumnSpacing` |
| Horizontal chip rows | `ScrollView Orientation="Horizontal"` > `HorizontalStackLayout` |
| Exercise / history / plan lists | `CollectionView` with `ItemTemplate` |
| Plan card left accent bar | `Border` with a 3px-wide colored `BoxView`/left border element |
| Bottom tab bar | Shell `TabBar` (or custom `Grid` bar with blur) |
| Segmented toggle (Predefined/My, effort) | `Grid` of two/three `Button`s, bind selected style |
| Settings toggles | `Grid` row + `Switch` (recolor `OnColor` = accent) or custom pill toggle |
| Demo / illustration area | `Image` (looping GIF/MP4 via a media control) — striped `Border` placeholder until assets exist |
| Progress bars / volume bars | `BoxView`/`Border` with bound width/height, or a chart lib |

## 4. The hero: timers (most important)
Build a single `WorkoutSessionService` (singleton) holding session state and an
`IDispatcherTimer` ticking at 1s:
- **Session timer** — counts **up** (`SessionSeconds++`) while a workout is active; always
  visible at the top, formatted `m:ss`.
- **Rest countdown** — when `RestActive`, decrement `RestSeconds`; at 0, set `RestActive=false`,
  then fire **sound** (`MediaElement`/`Plugin.Maui.Audio`) and **vibration**
  (`HapticFeedback.Perform` / `Vibration.Vibrate`) — both gated by settings.
- **Complete set** → increments the done-set count for the current exercise (fills the next set
  dot) and, if `autoStartRest`, sets `RestActive=true`, `RestSeconds=RestTotal`.
- Rest controls: **+30s** (`RestSeconds+=30`), **Skip** (`RestActive=false`), presets 30/60/90s
  set `RestTotal`/`RestSeconds`.
- **Background safety:** persist `sessionStartUtc`; on resume recompute elapsed so the timer
  stays accurate if the app is backgrounded mid-workout.
- Progress bar value = `(curIdx + doneSets/totalSets) / exerciseCount`.
- Keep this screen uncluttered: big numerals (64–84px Barlow Condensed), big buttons (≥56px).

## 5. Local-first persistence
- Use **SQLite** (`sqlite-net-pcl`) for plans, plan-exercises, workout history, and PRs;
  use **`Preferences`** for settings flags, streak counters, and the onboarding-complete flag.
- Seed the DB from `seed-data.json` on first launch (exercises + 6 predefined plans + sample
  history/profile so screens render alive). Mark predefined plans read-only/duplicatable;
  user plans are editable.
- **Backup:** Export = serialize all plans + history to a JSON file via `FileSaver`/share sheet;
  Import = pick a file and merge. No account/login anywhere.
- Track **PRs** (max reps / longest hold per exercise) and surface a celebration when beaten.

## 6. Build order (recommended)
1. **Shell + theme + fonts + tab scaffolding** (empty pages).
2. **Active Workout + Rest timer** (the hero) against a hard-coded plan — get the timers right.
3. **Plans tab** (predefined from seed) → start a workout from a card.
4. **Plan Builder** → create/edit/duplicate/delete, persist.
5. **Summary** → save to history; **Progress/History**.
6. **Exercise library** (filters + search) → **Exercise Detail** with progressions.
7. **Onboarding**, **Profile/Settings**, backup export/import.
8. Enhancements (skill tree, auto-progression, circuit/AMRAP/EMOM/Tabata modes, warm-up/cooldown,
   calendar, reminders/notifications) — see the original prompt's "Enhancements".

## 7. Motion & polish
- Slide-up modal transitions; set-complete tick feedback; summary celebration (scale-in check +
  pulsing ring) via `Animation`/`this.ScaleTo`. Charts/stats stay calm and encouraging — never
  shaming. Respect the sound/haptics settings everywhere.

## 8. Gotchas
- Don't ship the HTML/`support.js` — they're references only.
- Replace all glyph/emoji placeholders (nav icons, ▶ ✓ 🔥 🏆) with a real MAUI icon set.
- Replace striped placeholder areas with real looping demos + a highlighted-muscle diagram per exercise.
- Honor safe-areas (notch / home indicator) — the prototype's 46px top / 84px bottom approximate this.
