# Cali — Achievements System (53) — Implementation Spec

**Date:** 2026-06-24
**Source of truth:** the engineering handoff `achievementshandoff.md` (53 achievements, typed criteria engine). This doc captures the C# mapping, the data enablers added, and the honest gaps. Supersedes the earlier 16-achievement catalog.

## Scope decision (confirmed)
Build the **full** system + add the missing tracking. Result: ~49/53 earnable; dormant = `all_modes` (no alternate modes) and the `l_sit` / `front_lever` / `planche` skills (no matching exercise). Everything else is wired to real data.

## Data enablers (added)
1. **`Exercise.Family`** (`pushup|pullup|squat|dip|core|plank|""`) on the model + every seed exercise. Powers volume-by-family + feats.
2. **Per-workout exercise log:** `WorkoutRecord.ItemsJson` (serialized `WorkoutItemLog[]` = `{ExerciseId, Family, Muscle, Reps, Sets, HoldSec}`) + `WorkoutRecord.PrCount`. Written at Save to History from the live session. sqlite-net auto-adds the new columns. Powers reps-by-family, distinct-exercises, muscle-groups-in-a-week, `prs_in_single_workout`.
3. **Real streaks:** `StreakCalculator.Compute(localWorkoutDays)` → `(current, best)` from history dates (local tz). Replaces the seeded streak everywhere (Home, Progress, achievements).
4. **PR-derived skill unlocks:** `skillId → exerciseId` map (`pullup→pullup, dip→dips, pistol_squat→pistol, handstand→handstand, muscle_up→muscleup`); a skill is "unlocked" when that exercise has a PR. `l_sit/front_lever/planche` have no exercise → dormant.

## Engine (Cali.Core, TDD)
- **`Criteria`** — one class, `Type` + optional params (Count/Days/Family/Seconds/Minutes/MinExercises/Hour/StartHour/EndHour/SkillId/Modes).
- **`Achievement`** — `{Id, Name, Description, Category, Tier, Points, Emoji, Hidden, Criteria}`.
- **`AchievementCatalog.All`** — the 53 from the appendix (verbatim ids/criteria; an emoji assigned per item; points by tier: bronze 10 / silver 25 / gold 50 / platinum 100 / secret 40).
- **`StatsContext`** — precomputed aggregates (§5 of the handoff).
- **`StatsContextBuilder.Build(history, prs, userPlanCount, presetNames, exerciseFamilies, exerciseMuscles, tz)`** — reductions + date bucketing (streaks, perfect week, busy month, weekend, comeback gap, day/month counts, muscle-groups-in-week, time-of-day, families, PR-derived skills/feats).
- **`CriteriaEvaluator.Passes(Criteria, StatsContext)`** — switch over the 25 types. Also `Progress(Criteria, StatsContext) → (cur, target)` for UI hints (countables; booleans = 0/1).
- **`AchievementService`** — `SyncAsync(now, celebrate)` (build stats → newly-passing locked achievements → persist `AchievementUnlock{Id, UnlockedUtc}` → queue celebrations), `GetStatusAsync()` (all 53 + unlocked + cur/target + hidden masking), `TotalPoints`/`UnlockedCount`. Idempotent; retroactive (startup full eval). Reuses the existing `AchievementUnlock` table.

## Evaluation timing (unchanged)
Startup (silent retroactive backfill), after Save to History (celebrate → summary toasts), after backup import (silent).

## UI
- **Progress tab:** a short badge strip (unlocked-first) + **"{unlocked}/{total} · See all ›"** button → opens the grid. The standalone Progress celebration banner is **removed** (replaced by summary toasts per the handoff).
- **AchievementsPage (modal grid):** header `‹` back, "Achievements", "{points} pts · {unlocked}/{total} unlocked", overall progress bar; sections by **category** (milestones, consistency, volume, feats, skills, explorer, lifestyle, secret); each card = emoji tile (dim if locked), name, description, **tier color** accent, progress bar + `cur / target` for locked countables, ✓ / LOCKED tag. **Secret + locked → "???"** (name/desc masked) with a mystery style.
- **Summary unlock toasts:** after Save, newly-unlocked achievements animate in as queued, tier-colored toasts on the Summary, then it navigates to Progress.

## Tiers
bronze `#CD7F32`, silver `#AEB4BE`, gold `#FFC93C`, platinum `#67E8F9`, secret `#A77BF3`.

## Tests
`StreakCalculator` (runs, current vs best, gaps), `StatsContextBuilder` (perfect week, weekend, comeback, month/day counts, muscle-week, family reduction from logs, PR→skill/feat), `CriteriaEvaluator` (every `type`, boundaries, local-time hour logic), `AchievementService` (unlock-once/idempotent, retroactive over fixture history, hidden masking, points sum). Update existing seed/exercise tests for `Family`.

## Honest gaps (dormant achievements)
`all_modes` (alternate workout modes not implemented); `skill_lsit` / `skill_frontlever` / `skill_planche` (no matching exercise). All render in the grid (locked, with 0 progress); they unlock automatically once those features/exercises exist (same engine).
