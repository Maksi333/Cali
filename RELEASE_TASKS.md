# Calisthenics App — Release Readiness & Bug Fix Brief

> **For:** Claude Code (agentic coding session)
> **Goal:** Take this app from its current state to **production-ready, commercially shippable**.
> **Authority:** You may spin up as many review passes, test runs, and sub-agents as you need. Do not stop at "it compiles" — stop at "a paying first-time user has a clean, correct, polished experience."

---

## 0. How to use this document

1. **Audit first, code second.** Before changing anything, detect the stack (framework, state management, styling system, persistence layer, navigation), and locate where the issues below actually live. Report what you found.
2. Work through the tasks in priority order (P0 → P1 → P2).
3. For each task: reproduce → fix → verify against the **Acceptance criteria** → check the box.
4. After all P0/P1 items, run the **Release checklist** and **Test plan** end-to-end.
5. Keep a running `CHANGELOG` of what you changed and why.

**Definition of done for the whole brief:** every box below is checked, the fresh-install experience is genuinely empty/onboarding-ready, no demo data ships, and all flows behave correctly on a clean install.

---

## 1. Project context _(fill in before starting)_

- **Repo path / entry point:** `<fill in>`
- **Platform(s):** `<iOS / Android / both — screenshots are Android>`
- **Framework / language:** `<detect — e.g. React Native, Flutter, native Kotlin/Swift, etc.>`
- **State / persistence:** `<detect — Redux/Zustand/Provider/Riverpod, AsyncStorage/SQLite/Hive/etc.>`
- **Design tokens / accent color:** the brand accent is **orange** (`<confirm exact hex from theme>`). The previous/secondary blue should not appear in primary UI.
- **Run / build / test commands:** `<fill in>`

---

## 2. Priority bugs & fixes

### P0 — Release blockers

---

#### BUG-001 — Home: profile/name icon does nothing

- **Where:** Home page, top-right name/avatar icon.
- **Symptom:** Tapping the icon does not open anything.
- **Expected:** Tapping it navigates to the **profile / edit-profile screen** (same destination as the "You" tab, or a dedicated edit screen).
- **Acceptance criteria:**
  - [ ] The icon is a tappable control with a proper hit target (≥44pt).
  - [ ] Tapping it routes to the profile screen.
  - [ ] Back navigation returns cleanly to Home.
  - [ ] Works on a fresh install with no profile set up yet (routes to onboarding/edit, not a crash).

---

#### BUG-002 — Profile is not editable

- **Where:** "You" / Profile page (see screenshot 3).
- **Symptom:** Name and profile fields appear static; the name field can't be tapped/edited.
- **Expected:** The user can edit their profile: **name, bodyweight, max pull-ups, goal, level**, and avatar/initials.
- **Acceptance criteria:**
  - [ ] Tapping the profile card (or an explicit "Edit" affordance) opens an editable form.
  - [ ] All fields persist across app restarts.
  - [ ] Avatar initials update from the entered name.
  - [ ] Input validation: bodyweight and max pull-ups accept reasonable numeric ranges; empty/invalid input handled gracefully.
  - [ ] Unit (kg/lb) is consistent or user-selectable — confirm and standardize.

---

#### BUG-003 — "Complete Set" does not advance the workout correctly

- **Where:** Active workout / session screen (see screenshot 1).
- **Symptom:** Pressing **Complete Set** does not reliably move the session forward.
- **Expected behavior (state machine):**
  - If the **current set is not the last set** of the exercise → start the **rest timer**, then advance to the next set.
  - If it **was the last set** of the exercise **but not the last exercise** → advance to the **next exercise** (respecting "Next: …" preview).
  - If it was the **last set of the last exercise** → **end the workout** and show a completion/summary screen.
- **Acceptance criteria:**
  - [ ] Set indicator (the `1 2 3 4` chips) updates to reflect completed sets.
  - [ ] Rest timer auto-starts when "Auto-start rest timer" is enabled (it is, in settings) and is skippable.
  - [ ] Exercise transitions update the title, demo placeholder, "Next:" line, rep target, and set count.
  - [ ] The session progress bar and `1/6` step counter advance correctly.
  - [ ] Finishing the final set ends the session and records the workout (see fresh-state rules — only real completed sessions get recorded).
  - [ ] No off-by-one errors at the first or last set/exercise boundary.

---

#### BUG-004 — Ships with demo/fake data; not a fresh state for new users  ⚠️ commercial blocker

- **Where:** Profile, achievements, workout history, and anywhere seeded sample data appears.
- **Symptom:** A brand-new install shows pre-populated fake data: name "Alex", bodyweight 74 kg, **Max pull-ups 14**, "Goal: Skill · Intermediate", and (per notes) unlocked achievements and completed example workouts.
- **Expected:** A first launch must be **completely fresh**.
  - [ ] No seeded profile values (name, bodyweight, max pull-ups, goal, level) — fields start empty or drive an onboarding flow.
  - [ ] **No achievements unlocked** on first run.
  - [ ] **No completed/example workouts** in history or progress.
  - [ ] Progress charts/stats show proper **empty states**, not fabricated numbers.
  - [ ] Predefined **plans** (Full-Body Beginner, Push Day, etc.) are fine to ship — they are content, not user data — but confirm they are clearly templates and not pre-marked "done".
- **Implementation notes:**
  - Find and remove all seed/mock/fixture data paths that run on first launch or in production builds.
  - Ensure any "dev seed" only runs behind a debug flag, never in release.
  - Add a first-run check that initializes empty user state.
- **Acceptance criteria:**
  - [ ] Wipe app data / clean install → profile, achievements, and history are all empty.
  - [ ] An onboarding or empty-state prompt guides the user to set up their profile.
  - [ ] No string like "Alex", "74", "14" etc. is hardcoded into shipped user state.

---

### P1 — Polish required before launch

---

#### BUG-005 — Plan tags clip out of the card

- **Where:** Plans page, plan cards (see screenshots 2 & 4 — e.g. "Chest · Shoulders · Arms" runs off the right edge).
- **Symptom:** Long muscle-group / tag text is cut off by the card's rounded boundary.
- **Expected:** Text never bleeds past the card. Choose one consistent approach:
  - Truncate with an ellipsis (`Chest · Shoulders · …`), **or**
  - Wrap to a second line, **or**
  - Make the tag row horizontally scrollable.
- **Acceptance criteria:**
  - [ ] No tag text is visually clipped on any card.
  - [ ] Behavior is consistent across all plan cards and the longest realistic strings.
  - [ ] Verified at small and large system font sizes and on narrow devices.

---

#### BUG-006 — New Plan stepper numbers are unreadable

- **Where:** New plan / edit plan screen, SETS / REPS / REST steppers (see screenshot 5).
- **Symptom:** The value between the `−` and `+` buttons overlaps the buttons and is clipped (illegible).
- **Expected:** Each stepper shows a **clearly centered, fully visible** numeric value with comfortable spacing between the `−`, value, and `+`.
- **Likely causes to check:** container `overflow: hidden` clipping the value, absolute positioning/z-index, oversized font/line-height vs. container, or the value not having its own layout slot.
- **Acceptance criteria:**
  - [ ] SETS, REPS, and REST values are fully visible and centered.
  - [ ] `−` decrements and `+` increments with sensible min/max bounds.
  - [ ] Layout holds at large font sizes and on narrow screens.
  - [ ] The "~X min · N exercises · …" summary recalculates as values change.

---

#### BUG-007 — Exercise search underline is blue, should be orange

- **Where:** Exercises page, search field (see screenshot 6 — blue underline).
- **Symptom:** The active/underline color is blue, inconsistent with the orange accent used everywhere else.
- **Expected:** The search field underline/focus color matches the brand **orange** accent.
- **Implementation note:** This is often a default input focus/caret color from the framework — fix it at the theme/token level so it doesn't reappear on other inputs (also check the "My Plan" title underline and any other text inputs).
- **Acceptance criteria:**
  - [ ] Search underline (default and focused) is orange.
  - [ ] No stray blue accents remain on inputs/carets app-wide.

---

### P2 — Recommended for a commercial release

- [ ] **BUG-008 — Exercise/demo media:** the workout screen shows `[ demo · Push-ups ]` and exercise list items show empty placeholder thumbnails. Decide: ship real demo images/animations, or design an intentional placeholder. A "demo" label looks unfinished in a paid product.
- [ ] **BUG-009 — Empty states everywhere:** "My plans", Progress, and Achievements need designed empty states with a clear call to action.
- [ ] **BUG-010 — Settings persistence:** confirm Sound alerts, Vibration/haptics, Auto-start rest timer, Auto-progression, Workout reminders all persist and actually drive behavior.
- [ ] **BUG-011 — Accessibility & contrast:** verify tap targets, contrast ratios (orange-on-dark, grey secondary text), dynamic type, and screen-reader labels on icon-only buttons (End, Prev/Rest/Next, profile icon).

---

## 3. Release checklist (run after P0/P1)

- [ ] **Fresh install audit:** wipe data, reinstall — profile empty, zero achievements, zero workout history, empty progress, onboarding shown.
- [ ] **No debug/seed/mock data** reachable in a release build.
- [ ] **Navigation:** every screen reachable and every back path correct; no dead-end or crash.
- [ ] **Persistence:** profile, settings, custom plans, and completed sessions survive force-quit and reboot.
- [ ] **Theming:** single source of truth for the orange accent; no rogue colors.
- [ ] **Layout robustness:** longest strings, smallest/largest fonts, narrow + large screens, dark mode.
- [ ] **Edge cases:** empty plan, single-exercise plan, single-set exercise, very large rep/rest values.
- [ ] **Performance:** no jank on the timer, set transitions, or list scrolling.
- [ ] **Build:** release build compiles clean with no warnings you introduced; bump version; verify app icon, name, and splash.
- [ ] **Legal/store basics:** privacy policy link, no placeholder copyright/about text, correct app metadata.

---

## 4. Test plan

Add or extend automated tests where the project supports them, then do a manual pass.

**Automated (where feasible):**
- [ ] Unit test the workout state machine: set → rest → next set → next exercise → end (cover first/last boundaries).
- [ ] Unit test "fresh state" initialization: no achievements, no history, empty profile.
- [ ] Component/snapshot tests for the plan card (clipping) and the stepper (readability + bounds).

**Manual smoke test (clean install):**
1. [ ] Launch fresh → complete onboarding / set up profile → values persist.
2. [ ] Home name icon → profile screen → edit a field → returns and persists.
3. [ ] Start "Full-Body Beginner" → complete every set → verify rest timers, exercise transitions, and that the **final set ends the workout** and records exactly one session.
4. [ ] Create a new plan → tweak SETS/REPS/REST (numbers readable, summary updates) → save → appears under "My plans".
5. [ ] Plans page → confirm no clipped tags.
6. [ ] Exercises search → underline is orange → filters work.
7. [ ] Re-check Achievements/Progress reflect only the one real session above.

---

## 5. Suggested working order for Claude Code

1. **Recon pass** — map the stack, theme tokens, navigation, persistence, and where seed data lives. Post findings.
2. **P0 fixes** — BUG-001, 002, 003, 004 (fresh state is the most important commercial blocker).
3. **P1 fixes** — BUG-005, 006, 007.
4. **Review pass** — spin up a review/QA agent to diff against acceptance criteria.
5. **P2 + Release checklist + Test plan.**
6. **Final verification pass** on a clean install, then summarize all changes in the CHANGELOG.

> When you finish, report: what was changed, what was found beyond this list, and anything you recommend deferring (with rationale) so the owner can make the release call.
