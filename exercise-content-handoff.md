# Exercise Content Expansion — Handoff for Claude Code

## Purpose
Add the following **65 new bodyweight / calisthenics exercises** to the existing app. The app is already live; this is a content expansion only. Each exercise below is written to match the existing data schema (see the *Diamond Push-ups* reference). Implement using whatever data structure / file / DB the codebase already uses for exercises — you know the codebase, so map these fields to it.

## Data schema (per exercise)
Mirrors the existing exercise detail screen:

| Field | Notes |
|---|---|
| `name` | Display name |
| `difficulty` | One of: `Beginner`, `Intermediate`, `Advanced` |
| `muscleGroup` | One of: `Chest`, `Arms`, `Shoulders`, `Back`, `Legs`, `Core`, `Full Body` |
| `equipment` | Existing values: `none`, `bench`, `dip station`, `pull-up bar`, `bar/table`, `wall`. New values introduced below: `box` (use `bench` if you'd rather not add it), `parallettes` (use `none` if not supported). Normalize to existing vocabulary where you prefer. |
| `description` | One short benefit-focused subtitle line (the gray tagline) |
| `formCues` | Ordered list, 3 steps |
| `progressions.easier` / `progressions.harder` | Reference an exercise **by name**. May point to an existing exercise OR a new one in this batch. `—` means leave blank / no link. |
| `commonMistakes` | List, 2 items |

## Rules
- **Do not create duplicates** of the 19 exercises already in the app (listed below).
- Alternate variations of existing moves are intentional and wanted (e.g. *Wide Push-ups* alongside *Push-ups*).
- Progression links reference exercises by name — resolve to slug/ID however the codebase does it. If a referenced name isn't yet in the DB after this batch, fall back to the closest match or leave blank.
- Keep tone consistent with existing copy: punchy, second person, benefit-led.

## Already in the app — DO NOT duplicate
Push-ups · Incline Push-ups · Diamond Push-ups · Pike Push-ups · Dips · Pull-ups · Chin-ups · Inverted Rows · Squats · Lunges · Pistol Squats · Plank · Hollow Hold · Leg Raises · Mountain Climbers · Burpees · Handstand Hold · Glute Bridges · Muscle-ups

---

# NEW EXERCISES (65)

## CHEST

### Wide Push-ups
- **Difficulty:** Beginner
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** A wider hand stance hits the chest harder and eases the load on the triceps.
- **Form cues:**
  1. Place hands wider than shoulder-width, fingers slightly turned out.
  2. Lower with control until your chest nears the floor.
  3. Drive back up while keeping your body in one straight line.
- **Progressions:** Easier → Knee Push-ups · Harder → Push-ups
- **Common mistakes:** Letting the hips sag; flaring the neck and shoulders up to the ears.

### Knee Push-ups
- **Difficulty:** Beginner
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** The easiest way to build real pressing strength toward your first full push-up.
- **Form cues:**
  1. Kneel and walk your hands out so the body is straight from knees to head.
  2. Lower your chest toward the floor with elbows at about 45 degrees.
  3. Press back up without letting the hips drop or pike.
- **Progressions:** Easier → Wall Push-ups · Harder → Push-ups
- **Common mistakes:** Sticking the butt in the air; bending only at the hips instead of the elbows.

### Wall Push-ups
- **Difficulty:** Beginner
- **Muscle group:** Chest
- **Equipment:** wall
- **Description:** The gentlest push-up entry point — perfect for absolute beginners or warm-ups.
- **Form cues:**
  1. Stand arm's length from a wall, hands flat at shoulder height.
  2. Bend the elbows to bring your chest toward the wall.
  3. Push back to a straight-arm start, keeping the body rigid.
- **Progressions:** Easier → — · Harder → Knee Push-ups
- **Common mistakes:** Standing too close, which removes the resistance; letting the lower back arch.

### Decline Push-ups
- **Difficulty:** Intermediate
- **Muscle group:** Chest
- **Equipment:** bench
- **Description:** Elevating the feet shifts the focus to the upper chest and shoulders.
- **Form cues:**
  1. Place your feet on a bench with hands on the floor in push-up position.
  2. Lower until your chest reaches floor level.
  3. Press up while bracing your core to keep a straight line.
- **Progressions:** Easier → Push-ups · Harder → Pike Push-ups
- **Common mistakes:** Sagging hips under the steeper angle; flaring the elbows wide.

### Archer Push-ups
- **Difficulty:** Advanced
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** Shift your weight onto one arm at a time — a powerful step toward the one-arm push-up.
- **Form cues:**
  1. Set hands wide, one arm straight out to the side as a kickstand.
  2. Bend the working arm and lower toward that hand.
  3. Press back up, then alternate sides each rep.
- **Progressions:** Easier → Wide Push-ups · Harder → One-Arm Push-ups
- **Common mistakes:** Bending the straight "support" arm to cheat; rotating the hips instead of staying square.

### Clap Push-ups
- **Difficulty:** Advanced
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** Explosive push-ups that build power, speed, and serious upper-body force.
- **Form cues:**
  1. Lower into a controlled push-up.
  2. Drive up explosively so your hands leave the floor.
  3. Clap, then land softly with bent elbows to absorb the impact.
- **Progressions:** Easier → Push-ups · Harder → —
- **Common mistakes:** Landing on locked, stiff arms; not generating enough push to fully leave the floor.

### One-Arm Push-ups
- **Difficulty:** Advanced
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** The ultimate test of unilateral pressing strength and full-body tension.
- **Form cues:**
  1. Set feet wide for balance and place one hand behind your back.
  2. Lower under control while keeping the torso square to the floor.
  3. Press back up without twisting toward the working arm.
- **Progressions:** Easier → Archer Push-ups · Harder → —
- **Common mistakes:** Twisting the body to recruit other muscles; using a stance too narrow to balance.

### Spiderman Push-ups
- **Difficulty:** Intermediate
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** A push-up that drives a knee to the elbow, blending chest strength with core control.
- **Form cues:**
  1. Lower into a push-up while pulling one knee toward the same-side elbow.
  2. Touch the bottom of the push-up, then return the leg.
  3. Alternate sides on each repetition.
- **Progressions:** Easier → Push-ups · Harder → Archer Push-ups
- **Common mistakes:** Letting the hip rotate up; dropping the knee instead of driving it forward.

### Hindu Push-ups
- **Difficulty:** Intermediate
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** A flowing dive-bomb push-up that builds pressing strength alongside shoulder mobility.
- **Form cues:**
  1. Start in a downward-dog pike with hips high.
  2. Swoop your chest down and forward, skimming close to the floor.
  3. Press up and back to the start, leading with the hips.
- **Progressions:** Easier → Push-ups · Harder → —
- **Common mistakes:** Rushing the swoop and losing control; barely bending the elbows on the way through.

### Staggered Push-ups
- **Difficulty:** Beginner
- **Muscle group:** Chest
- **Equipment:** none
- **Description:** Offset hands load each side unevenly to prep for single-arm strength.
- **Form cues:**
  1. Place one hand forward and the other slightly back.
  2. Lower with control, keeping the body straight.
  3. Press up, then swap hand positions each set.
- **Progressions:** Easier → Knee Push-ups · Harder → Archer Push-ups
- **Common mistakes:** Letting the torso twist toward the forward hand; uneven set counts between sides.

---

## SHOULDERS

### Handstand Push-ups
- **Difficulty:** Advanced
- **Muscle group:** Shoulders
- **Equipment:** wall
- **Description:** Press your full bodyweight overhead for elite vertical pushing strength.
- **Form cues:**
  1. Kick up to a handstand against a wall with hands shoulder-width.
  2. Lower under control until the top of your head nears the floor.
  3. Press back up to a full lockout without arching the back.
- **Progressions:** Easier → Pike Push-ups · Harder → —
- **Common mistakes:** Overarching the lower back into the wall; flaring the elbows out wide.

### Elevated Pike Push-ups
- **Difficulty:** Intermediate
- **Muscle group:** Shoulders
- **Equipment:** bench
- **Description:** Feet raised on a bench steepens the angle to bridge toward handstand push-ups.
- **Form cues:**
  1. Place feet on a bench and hips high in an inverted-V pike.
  2. Lower the top of your head toward the floor between your hands.
  3. Press straight back up, keeping the hips stacked over the shoulders.
- **Progressions:** Easier → Pike Push-ups · Harder → Handstand Push-ups
- **Common mistakes:** Letting the hips drift back into a push-up; collapsing the head forward instead of down.

### Wall Walks
- **Difficulty:** Intermediate
- **Muscle group:** Shoulders
- **Equipment:** wall
- **Description:** Walk your feet up a wall to build handstand confidence and shoulder endurance.
- **Form cues:**
  1. Start in a push-up with feet at the base of a wall.
  2. Walk your feet up while stepping your hands closer to the wall.
  3. Reverse the movement slowly back to the floor.
- **Progressions:** Easier → Plank · Harder → Handstand Hold
- **Common mistakes:** Rushing and losing control on the way down; letting the lower back sag.

### Crow Pose
- **Difficulty:** Intermediate
- **Muscle group:** Shoulders
- **Equipment:** none
- **Description:** A balance hold that builds wrist, shoulder, and core strength for advanced skills.
- **Form cues:**
  1. Squat with hands planted shoulder-width on the floor.
  2. Rest your knees against the backs of your upper arms.
  3. Lean forward and lift your feet, balancing on your hands.
- **Progressions:** Easier → Plank · Harder → —
- **Common mistakes:** Looking down instead of slightly forward; placing the knees too low on the arms.

### Pseudo Planche Lean
- **Difficulty:** Intermediate
- **Muscle group:** Shoulders
- **Equipment:** none
- **Description:** Lean your weight forward over your hands to build the foundation for the planche.
- **Form cues:**
  1. Start in a push-up position with hands by your hips, fingers turned out.
  2. Lean your shoulders forward past your hands.
  3. Hold the lean with a tight core and protracted shoulders.
- **Progressions:** Easier → Plank · Harder → —
- **Common mistakes:** Letting the hips pike up; leaning from the hips rather than the shoulders.

---

## ARMS

### Bench Dips
- **Difficulty:** Beginner
- **Muscle group:** Arms
- **Equipment:** bench
- **Description:** A scalable triceps builder using a bench or chair — no dip station needed.
- **Form cues:**
  1. Sit on the edge of a bench, hands gripping beside your hips.
  2. Slide your hips off and lower by bending the elbows straight back.
  3. Press up until the arms are nearly locked.
- **Progressions:** Easier → — · Harder → Dips
- **Common mistakes:** Flaring elbows out to the sides; shrugging the shoulders toward the ears.

### Korean Dips
- **Difficulty:** Advanced
- **Muscle group:** Arms
- **Equipment:** pull-up bar
- **Description:** Dips behind a straight bar that hammer the triceps through a brutal range.
- **Form cues:**
  1. Support yourself above a bar with it behind your back.
  2. Lower under control, keeping the bar close to your body.
  3. Press back up to a full lockout.
- **Progressions:** Easier → Dips · Harder → —
- **Common mistakes:** Letting the bar drift away from the body; rushing past a controlled bottom position.

### Sphinx Push-ups
- **Difficulty:** Advanced
- **Muscle group:** Arms
- **Equipment:** none
- **Description:** A forearm-to-hand push-up that isolates the triceps like nothing else bodyweight.
- **Form cues:**
  1. Start in a forearm plank.
  2. Press up onto your hands by extending only at the elbows.
  3. Lower slowly back to the forearms, keeping the body rigid.
- **Progressions:** Easier → Diamond Push-ups · Harder → —
- **Common mistakes:** Pushing from the hips instead of the elbows; letting the lower back sag.

### Bodyweight Tricep Extensions
- **Difficulty:** Intermediate
- **Muscle group:** Arms
- **Equipment:** bar/table
- **Description:** A standing skull-crusher using a fixed bar to overload the triceps.
- **Form cues:**
  1. Grip a chest-height bar and step your feet back into a lean.
  2. Bend only at the elbows to lower your head behind the bar.
  3. Extend the arms to press yourself back to the start.
- **Progressions:** Easier → Bench Dips · Harder → Sphinx Push-ups
- **Common mistakes:** Bending at the hips or shoulders; letting the elbows drift apart.

---

## BACK

### Wide-Grip Pull-ups
- **Difficulty:** Intermediate
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** A wider grip emphasizes the lats for a broader, stronger back.
- **Form cues:**
  1. Grip the bar wider than shoulder-width, palms facing away.
  2. Pull your chest toward the bar, driving the elbows down.
  3. Lower under control to a full hang.
- **Progressions:** Easier → Pull-ups · Harder → Archer Pull-ups
- **Common mistakes:** Cutting the range short at the bottom; swinging to generate momentum.

### Negative Pull-ups
- **Difficulty:** Beginner
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** Lower slowly from the top to build the strength for your first full pull-up.
- **Form cues:**
  1. Jump or step up so your chin is over the bar.
  2. Lower yourself as slowly as possible — aim for 3 to 5 seconds.
  3. Reset and repeat from the top each rep.
- **Progressions:** Easier → Scapular Pull-ups · Harder → Pull-ups
- **Common mistakes:** Dropping too fast; relaxing the shoulders at the bottom instead of staying engaged.

### Scapular Pull-ups
- **Difficulty:** Beginner
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** Small shoulder-blade pulls that teach the first move of every pull-up.
- **Form cues:**
  1. Hang from the bar with straight arms.
  2. Without bending the elbows, pull your shoulder blades down and back.
  3. Lift your body slightly, then return to a relaxed hang.
- **Progressions:** Easier → Dead Hang · Harder → Negative Pull-ups
- **Common mistakes:** Bending the elbows; shrugging up instead of pulling the blades down.

### Archer Pull-ups
- **Difficulty:** Advanced
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** Pull to one side at a time, a key step toward the one-arm pull-up.
- **Form cues:**
  1. Take a wide grip on the bar.
  2. Pull up toward one hand while the other arm stays straight.
  3. Lower with control, then alternate sides.
- **Progressions:** Easier → Wide-Grip Pull-ups · Harder → —
- **Common mistakes:** Bending the straight arm to assist; failing to pull the chin level with the working hand.

### Commando Pull-ups
- **Difficulty:** Advanced
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** Pull up alongside the bar to each side, blasting the back and biceps.
- **Form cues:**
  1. Grip the bar with hands together, one in front of the other.
  2. Pull up so your head passes to one side of the bar.
  3. Lower, then pull up to the opposite side.
- **Progressions:** Easier → Chin-ups · Harder → —
- **Common mistakes:** Letting the body swing side to side; not alternating sides evenly.

### Dead Hang
- **Difficulty:** Beginner
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** A simple hang that builds the grip and shoulder strength every pulling move needs.
- **Form cues:**
  1. Grip the bar shoulder-width with palms facing away.
  2. Hang with arms straight and shoulders slightly engaged.
  3. Hold for time, breathing steadily.
- **Progressions:** Easier → — · Harder → Scapular Pull-ups
- **Common mistakes:** Fully relaxing the shoulders into a dead-passive hang too soon; clenching the jaw and holding the breath.

### Superman
- **Difficulty:** Beginner
- **Muscle group:** Back
- **Equipment:** none
- **Description:** A floor hold that strengthens the lower back and spinal muscles most people neglect.
- **Form cues:**
  1. Lie face down with arms extended overhead.
  2. Lift your arms, chest, and legs off the floor at once.
  3. Hold briefly, then lower with control.
- **Progressions:** Easier → — · Harder → Reverse Snow Angels
- **Common mistakes:** Cranking the neck back to look up; jerking up instead of lifting smoothly.

### Reverse Snow Angels
- **Difficulty:** Beginner
- **Muscle group:** Back
- **Equipment:** none
- **Description:** Sweeping arm circles on the floor that build the upper back and rear shoulders.
- **Form cues:**
  1. Lie face down with arms by your sides, palms down.
  2. Lift your arms slightly and sweep them overhead in an arc.
  3. Reverse the sweep back to your hips, keeping arms off the floor.
- **Progressions:** Easier → Superman · Harder → —
- **Common mistakes:** Letting the hands rest on the floor; lifting the chest instead of keeping it grounded.

### Towel Rows
- **Difficulty:** Beginner
- **Muscle group:** Back
- **Equipment:** bar/table
- **Description:** A no-bar rowing variation using a towel anchored under a door or around a post.
- **Form cues:**
  1. Loop a sturdy towel around an anchor and grip both ends.
  2. Lean back with a straight body and arms extended.
  3. Pull your chest toward your hands, squeezing the shoulder blades.
- **Progressions:** Easier → — · Harder → Inverted Rows
- **Common mistakes:** Letting the hips sag; pulling with the arms only instead of the back.

### Skin the Cat
- **Difficulty:** Advanced
- **Muscle group:** Back
- **Equipment:** pull-up bar
- **Description:** A gymnastic rotation that builds shoulder mobility and control for advanced skills.
- **Form cues:**
  1. Hang from a bar and pull your knees up toward your chest.
  2. Rotate backward, passing your legs through and overhead.
  3. Reverse the rotation slowly back to the hang.
- **Progressions:** Easier → Dead Hang · Harder → —
- **Common mistakes:** Going too deep before shoulders are ready; releasing tension and dropping through the rotation.

---

## LEGS

### Wall Sit
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** wall
- **Description:** An isometric hold that builds quad endurance and mental grit.
- **Form cues:**
  1. Slide your back down a wall until your thighs are parallel to the floor.
  2. Keep knees stacked over the ankles at 90 degrees.
  3. Hold for time, pressing your back flat into the wall.
- **Progressions:** Easier → — · Harder → Squats
- **Common mistakes:** Letting the knees drift past the toes; sliding the hips up to cheat the angle.

### Jump Squats
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** Explosive squats that build power, speed, and lower-body strength.
- **Form cues:**
  1. Lower into a squat with the chest up.
  2. Drive through the feet and jump as high as you can.
  3. Land softly into the next squat, absorbing through bent knees.
- **Progressions:** Easier → Squats · Harder → Box Jumps
- **Common mistakes:** Landing with stiff, straight legs; collapsing the knees inward on landing.

### Bulgarian Split Squats
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** bench
- **Description:** A rear-foot-elevated lunge that overloads each leg for serious single-leg strength.
- **Form cues:**
  1. Place the top of one foot on a bench behind you.
  2. Lower the front leg until the thigh is parallel to the floor.
  3. Drive through the front heel to stand back up.
- **Progressions:** Easier → Reverse Lunges · Harder → Shrimp Squats
- **Common mistakes:** Leaning too far forward; letting the front knee cave inward.

### Step-ups
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** box
- **Description:** A simple, scalable single-leg builder using any sturdy step or box.
- **Form cues:**
  1. Place one full foot on a knee-height box.
  2. Drive through that heel to stand tall on top.
  3. Lower under control and repeat, then switch legs.
- **Progressions:** Easier → — · Harder → Bulgarian Split Squats
- **Common mistakes:** Pushing off the bottom foot instead of the top leg; only half-extending the hip at the top.

### Calf Raises
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** Build stronger, more resilient calves with this simple foundational move.
- **Form cues:**
  1. Stand tall with feet hip-width apart.
  2. Push through the balls of your feet to lift your heels high.
  3. Lower slowly until your heels nearly touch the floor.
- **Progressions:** Easier → — · Harder → Single-Leg Calf Raises
- **Common mistakes:** Bouncing through reps; cutting the range short at the top.

### Single-Leg Calf Raises
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** Double the load on each calf for stronger ankles and explosive push-off.
- **Form cues:**
  1. Balance on one foot, the other tucked behind.
  2. Rise onto the ball of your foot as high as possible.
  3. Lower slowly under control, then switch legs.
- **Progressions:** Easier → Calf Raises · Harder → —
- **Common mistakes:** Using a wall for too much support; rushing the lowering phase.

### Reverse Lunges
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** A knee-friendly lunge variation that builds balance and single-leg strength.
- **Form cues:**
  1. Step one foot back and lower into a lunge.
  2. Keep the front shin vertical and torso upright.
  3. Drive through the front heel to return to standing.
- **Progressions:** Easier → — · Harder → Walking Lunges
- **Common mistakes:** Letting the front knee travel past the toes; leaning the torso forward.

### Walking Lunges
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** Lunges in motion that build leg strength, balance, and coordination.
- **Form cues:**
  1. Step forward into a lunge, both knees bending to 90 degrees.
  2. Drive through the front heel to rise and step the back foot through.
  3. Continue alternating legs as you move forward.
- **Progressions:** Easier → Reverse Lunges · Harder → Jump Squats
- **Common mistakes:** Taking steps too short and crowding the knees; letting the torso pitch forward.

### Cossack Squats
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** A deep side-to-side squat that builds strength, mobility, and hip control.
- **Form cues:**
  1. Stand wide and shift your weight onto one bent leg.
  2. Lower into a deep squat, keeping the other leg straight.
  3. Push back to center and shift to the other side.
- **Progressions:** Easier → Squats · Harder → Pistol Squats
- **Common mistakes:** Letting the heel of the bent leg lift; rounding the back at the bottom.

### Shrimp Squats
- **Difficulty:** Advanced
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** A single-leg squat holding the rear foot — a brutal test of strength and balance.
- **Form cues:**
  1. Stand on one leg and grab the opposite foot behind you.
  2. Lower until your trailing knee gently touches the floor.
  3. Drive through the standing heel to rise back up.
- **Progressions:** Easier → Bulgarian Split Squats · Harder → —
- **Common mistakes:** Crashing the back knee into the floor; losing balance from looking down.

### Nordic Curls
- **Difficulty:** Advanced
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** An eccentric hamstring move that builds powerful, injury-resistant legs.
- **Form cues:**
  1. Kneel with your ankles anchored under a heavy object or held down.
  2. Keep the body straight from knees to head and lower forward slowly.
  3. Resist as long as possible, then push off and return.
- **Progressions:** Easier → Glute Bridges · Harder → —
- **Common mistakes:** Bending at the hips to cheat; dropping fast instead of resisting the descent.

### Single-Leg Glute Bridge
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** A one-legged bridge that overloads each glute and builds hip stability.
- **Form cues:**
  1. Lie on your back, one foot planted and the other leg extended.
  2. Drive through the planted heel to lift your hips.
  3. Squeeze the glute at the top, then lower with control.
- **Progressions:** Easier → Glute Bridges · Harder → Hip Thrusts
- **Common mistakes:** Letting the hips tilt or drop on one side; arching the lower back to gain height.

### Hip Thrusts
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** bench
- **Description:** A bench-supported bridge that maximizes glute strength and power.
- **Form cues:**
  1. Rest your upper back on a bench with feet flat on the floor.
  2. Drive through your heels to lift your hips to a straight line.
  3. Squeeze the glutes hard at the top, then lower under control.
- **Progressions:** Easier → Glute Bridges · Harder → Single-Leg Glute Bridge
- **Common mistakes:** Overarching the lower back at the top; pushing through the toes instead of the heels.

### Box Jumps
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** box
- **Description:** Explosive jumps onto a raised surface that build power and athleticism.
- **Form cues:**
  1. Stand in front of a sturdy box, feet hip-width.
  2. Dip and swing the arms, then jump and land softly on top.
  3. Stand tall, then step down — don't jump down.
- **Progressions:** Easier → Jump Squats · Harder → Broad Jumps
- **Common mistakes:** Jumping back down and stressing the joints; landing with stiff, locked knees.

### Broad Jumps
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** Horizontal jumps for distance that develop raw lower-body power.
- **Form cues:**
  1. Stand with feet hip-width and swing the arms back.
  2. Drive the arms forward and jump as far as you can.
  3. Land softly on both feet with bent knees.
- **Progressions:** Easier → Jump Squats · Harder → —
- **Common mistakes:** Landing off-balance and stumbling; failing to use the arm swing for momentum.

### Skater Hops
- **Difficulty:** Intermediate
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** Lateral bounds that build single-leg power, balance, and knee stability.
- **Form cues:**
  1. Bound sideways onto one leg, landing softly.
  2. Let the other leg sweep behind for balance.
  3. Immediately bound to the opposite side and repeat.
- **Progressions:** Easier → Reverse Lunges · Harder → —
- **Common mistakes:** Landing with a stiff leg; letting the landing knee cave inward.

### Curtsy Lunges
- **Difficulty:** Beginner
- **Muscle group:** Legs
- **Equipment:** none
- **Description:** A crossover lunge that targets the glutes and inner-thigh stabilizers.
- **Form cues:**
  1. Step one leg diagonally behind the other.
  2. Lower until both knees are bent at about 90 degrees.
  3. Drive back to standing and alternate sides.
- **Progressions:** Easier → — · Harder → Walking Lunges
- **Common mistakes:** Letting the front knee collapse inward; leaning the torso too far forward.

---

## CORE

### Side Plank
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** An isometric hold that builds the obliques and lateral core stability.
- **Form cues:**
  1. Lie on your side, propped on one forearm under your shoulder.
  2. Lift your hips so the body forms a straight line.
  3. Hold for time, then switch sides.
- **Progressions:** Easier → Plank · Harder → —
- **Common mistakes:** Letting the hips sag toward the floor; rotating the chest down.

### Russian Twists
- **Difficulty:** Intermediate
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A seated rotation that targets the obliques and builds twisting strength.
- **Form cues:**
  1. Sit with knees bent and lean back to a strong V-shape.
  2. Clasp your hands and rotate your torso side to side.
  3. Tap the floor beside each hip, keeping the chest lifted.
- **Progressions:** Easier → Bicycle Crunches · Harder → Windshield Wipers
- **Common mistakes:** Moving only the arms instead of rotating the torso; rounding the back.

### Bicycle Crunches
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A dynamic crunch that hits the obliques and entire abdominal wall.
- **Form cues:**
  1. Lie on your back with hands lightly behind your head.
  2. Bring one elbow toward the opposite knee as you extend the other leg.
  3. Alternate sides in a smooth pedaling rhythm.
- **Progressions:** Easier → Crunches · Harder → V-ups
- **Common mistakes:** Yanking on the neck; racing through reps without real rotation.

### Crunches
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** The classic ab builder targeting the upper abdominal muscles.
- **Form cues:**
  1. Lie on your back, knees bent and feet flat.
  2. Curl your shoulders off the floor toward your knees.
  3. Lower with control without letting the head drop.
- **Progressions:** Easier → — · Harder → Sit-ups
- **Common mistakes:** Pulling on the neck; using momentum instead of the abs.

### Sit-ups
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A full range ab move working the entire front of the core.
- **Form cues:**
  1. Lie on your back with knees bent and feet anchored or flat.
  2. Curl all the way up until your chest meets your thighs.
  3. Lower slowly back to the floor.
- **Progressions:** Easier → Crunches · Harder → V-ups
- **Common mistakes:** Jerking up with the arms; letting the lower back slam the floor on the way down.

### V-ups
- **Difficulty:** Intermediate
- **Muscle group:** Core
- **Equipment:** none
- **Description:** Lift the arms and legs to meet in a V — a demanding full-core contraction.
- **Form cues:**
  1. Lie flat with arms extended overhead and legs straight.
  2. Lift the arms and legs simultaneously to touch over your hips.
  3. Lower both back down with control.
- **Progressions:** Easier → Sit-ups · Harder → Hollow Rock
- **Common mistakes:** Bending the knees to make it easier; using momentum to bounce up.

### Flutter Kicks
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** Small rapid leg kicks that build endurance in the lower abs.
- **Form cues:**
  1. Lie on your back with legs straight and slightly raised.
  2. Press your lower back into the floor.
  3. Kick the legs up and down in small, quick alternations.
- **Progressions:** Easier → — · Harder → Leg Raises
- **Common mistakes:** Letting the lower back arch off the floor; kicking too high and losing core tension.

### Hanging Knee Raises
- **Difficulty:** Intermediate
- **Muscle group:** Core
- **Equipment:** pull-up bar
- **Description:** Hang and raise the knees to build the lower abs and a stronger grip.
- **Form cues:**
  1. Hang from a bar with arms straight.
  2. Pull your knees up toward your chest.
  3. Lower with control, avoiding any swing.
- **Progressions:** Easier → Leg Raises · Harder → Hanging Leg Raises
- **Common mistakes:** Swinging the body for momentum; using only the hip flexors without curling the pelvis.

### Hanging Leg Raises
- **Difficulty:** Advanced
- **Muscle group:** Core
- **Equipment:** pull-up bar
- **Description:** Raise straight legs from a hang for elite lower-ab and grip strength.
- **Form cues:**
  1. Hang from a bar with straight arms and legs.
  2. Keep the legs straight and raise them to horizontal or higher.
  3. Lower slowly without swinging.
- **Progressions:** Easier → Hanging Knee Raises · Harder → Toes-to-Bar
- **Common mistakes:** Bending the knees to cheat; using a swing to throw the legs up.

### Toes-to-Bar
- **Difficulty:** Advanced
- **Muscle group:** Core
- **Equipment:** pull-up bar
- **Description:** Bring your toes all the way to the bar — a full-range hanging core feat.
- **Form cues:**
  1. Hang from a bar with an active shoulder grip.
  2. Raise straight legs to touch your toes to the bar.
  3. Lower under control to a full hang.
- **Progressions:** Easier → Hanging Leg Raises · Harder → —
- **Common mistakes:** Excessive kipping swing; not fully extending at the bottom between reps.

### L-sit
- **Difficulty:** Advanced
- **Muscle group:** Core
- **Equipment:** parallettes
- **Description:** Hold your legs out straight while supported — a serious core and hip-flexor test.
- **Form cues:**
  1. Press up on parallettes or the floor with straight arms.
  2. Lift your legs straight out to form an L.
  3. Hold for time with shoulders pressed down.
- **Progressions:** Easier → Hollow Hold · Harder → —
- **Common mistakes:** Letting the shoulders shrug up; bending the knees to hold the position.

### Dragon Flag
- **Difficulty:** Advanced
- **Muscle group:** Core
- **Equipment:** bench
- **Description:** A full-body lever that many call the ultimate core exercise.
- **Form cues:**
  1. Lie on a bench and grip behind your head for an anchor.
  2. Lift the whole body straight, balancing on the shoulders.
  3. Lower slowly with the body rigid, resisting the descent.
- **Progressions:** Easier → Hollow Hold · Harder → —
- **Common mistakes:** Bending at the hips to cheat; lowering too fast and losing the straight-body line.

### Bird Dog
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A stability move that strengthens the deep core and lower back together.
- **Form cues:**
  1. Start on all fours with a flat back.
  2. Extend one arm forward and the opposite leg back.
  3. Hold briefly, then switch sides without letting the hips twist.
- **Progressions:** Easier → — · Harder → Dead Bug
- **Common mistakes:** Letting the hips rotate; arching or rounding the back instead of staying flat.

### Dead Bug
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A back-friendly core move that teaches bracing while the limbs move.
- **Form cues:**
  1. Lie on your back with arms up and knees bent at 90 degrees.
  2. Lower the opposite arm and leg toward the floor.
  3. Return and alternate, keeping the lower back pressed down.
- **Progressions:** Easier → Bird Dog · Harder → Hollow Hold
- **Common mistakes:** Letting the lower back arch up; rushing instead of moving with control.

### Reverse Crunches
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** Curl the hips up to target the lower abs while sparing the neck.
- **Form cues:**
  1. Lie on your back with knees bent over your hips.
  2. Curl your pelvis up to lift the hips off the floor.
  3. Lower slowly without swinging the legs.
- **Progressions:** Easier → — · Harder → Leg Raises
- **Common mistakes:** Using leg momentum to swing the hips; lifting only the legs without curling the pelvis.

### Windshield Wipers
- **Difficulty:** Advanced
- **Muscle group:** Core
- **Equipment:** none
- **Description:** Rotate straight legs side to side for advanced oblique and control strength.
- **Form cues:**
  1. Lie on your back with arms out wide and legs raised straight up.
  2. Lower the legs to one side under control.
  3. Sweep them across to the other side without touching the floor.
- **Progressions:** Easier → Russian Twists · Harder → —
- **Common mistakes:** Letting the shoulders peel off the floor; dropping the legs too fast.

### Plank Shoulder Taps
- **Difficulty:** Beginner
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A plank with alternating taps that fights rotation and builds stability.
- **Form cues:**
  1. Hold a high plank with feet slightly wide.
  2. Tap one hand to the opposite shoulder.
  3. Replace it and alternate, keeping the hips still.
- **Progressions:** Easier → Plank · Harder → —
- **Common mistakes:** Rocking the hips side to side; widening the stance too far to make it trivial.

### Hollow Rock
- **Difficulty:** Intermediate
- **Muscle group:** Core
- **Equipment:** none
- **Description:** A rocking version of the hollow hold that adds dynamic core tension.
- **Form cues:**
  1. Hold a hollow body position with arms and legs off the floor.
  2. Maintain the tension and rock smoothly back and forth.
  3. Keep the lower back pressed down throughout.
- **Progressions:** Easier → Hollow Hold · Harder → V-ups
- **Common mistakes:** Losing the hollow shape mid-rock; letting the lower back gap off the floor.

---

## FULL BODY

### Bear Crawl
- **Difficulty:** Beginner
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A crawling move that builds full-body coordination, core, and shoulder strength.
- **Form cues:**
  1. Start on hands and feet with knees hovering just off the floor.
  2. Move the opposite hand and foot forward together.
  3. Keep the hips low and back flat as you crawl.
- **Progressions:** Easier → Plank · Harder → Crab Walk
- **Common mistakes:** Letting the hips sway high; lifting the knees too far off the floor.

### Crab Walk
- **Difficulty:** Beginner
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A reverse crawl that lights up the triceps, shoulders, and posterior chain.
- **Form cues:**
  1. Sit with hands behind you and feet flat, then lift your hips.
  2. Walk forward or backward using opposite hand and foot.
  3. Keep the hips lifted throughout.
- **Progressions:** Easier → Bear Crawl · Harder → —
- **Common mistakes:** Letting the hips drop toward the floor; shrugging the shoulders up to the ears.

### Inchworm
- **Difficulty:** Beginner
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A flowing move that builds core control while opening the hamstrings.
- **Form cues:**
  1. Stand tall, then hinge and walk your hands out to a plank.
  2. Hold the plank for a beat with a tight core.
  3. Walk the feet back toward the hands and stand up.
- **Progressions:** Easier → — · Harder → Burpees
- **Common mistakes:** Letting the hips sag in the plank; bending the knees excessively on the walk-out.

### Jumping Jacks
- **Difficulty:** Beginner
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A classic cardio move to raise the heart rate and warm up the whole body.
- **Form cues:**
  1. Start with feet together and arms at your sides.
  2. Jump the feet wide while raising the arms overhead.
  3. Jump back to the start in a steady rhythm.
- **Progressions:** Easier → — · Harder → Star Jumps
- **Common mistakes:** Landing flat-footed and hard; only half-raising the arms.

### High Knees
- **Difficulty:** Beginner
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A running-in-place drill that spikes the heart rate and fires the core.
- **Form cues:**
  1. Run in place, driving the knees up to hip height.
  2. Stay on the balls of your feet with a tall posture.
  3. Pump the arms in time with the legs.
- **Progressions:** Easier → — · Harder → Mountain Climbers
- **Common mistakes:** Leaning back as the knees rise; letting the knees stay low.

### Squat Thrusts
- **Difficulty:** Intermediate
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A burpee without the jump or push-up — explosive conditioning for the whole body.
- **Form cues:**
  1. Drop into a squat and plant your hands on the floor.
  2. Jump both feet back into a plank.
  3. Jump the feet forward and stand tall.
- **Progressions:** Easier → Inchworm · Harder → Burpees
- **Common mistakes:** Letting the hips pike up in the plank; rounding the back on the jump back.

### Plank Jacks
- **Difficulty:** Intermediate
- **Muscle group:** Full Body
- **Equipment:** none
- **Description:** A plank with jumping feet that adds cardio to your core training.
- **Form cues:**
  1. Hold a strong plank on hands or forearms.
  2. Jump the feet out wide, then back together.
  3. Keep the hips level and core braced throughout.
- **Progressions:** Easier → Plank · Harder → Mountain Climbers
- **Common mistakes:** Letting the hips bounce up and down; losing the straight-body line.

---

## Summary
- **65 new exercises** across all 7 muscle groups and all 3 difficulty levels.
- Difficulty spread: ~25 Beginner, ~22 Intermediate, ~18 Advanced.
- No duplicates of the existing 19; several intentional alternate variations included (push-up, pull-up, lunge, glute, and crunch families).
- New equipment values introduced: `box`, `parallettes` — normalize to existing vocabulary (`bench` / `none`) if you prefer not to add them.
- Progression links reference exercises by name from either the existing set or this batch — resolve to your slug/ID convention.
