namespace Cali.Core.Models;

/// <summary>The static achievement definitions (source of truth = achievementshandoff.md appendix).</summary>
public static class AchievementCatalog
{
    public static readonly string[] Categories =
        { "milestones", "consistency", "volume", "feats", "skills", "explorer", "lifestyle", "secret" };

    public static string CategoryTitle(string c) => c switch
    {
        "milestones" => "Milestones",
        "consistency" => "Consistency",
        "volume" => "Volume",
        "feats" => "Feats",
        "skills" => "Skills",
        "explorer" => "Explorer",
        "lifestyle" => "Lifestyle",
        "secret" => "Secret",
        _ => c
    };

    private static Achievement A(string id, string name, string cat, string tier, int pts, string emoji, string desc, Criteria c, bool hidden = false) =>
        new() { Id = id, Name = name, Category = cat, Tier = tier, Points = pts, Emoji = emoji, Description = desc, Criteria = c, Hidden = hidden };

    private static Criteria C(string type, int count = 0, int days = 0, string family = "", int seconds = 0,
        int minutes = 0, int minExercises = 0, int hour = 0, int startHour = 0, int endHour = 0, string skillId = "", string[]? modes = null) =>
        new() { Type = type, Count = count, Days = days, Family = family, Seconds = seconds, Minutes = minutes, MinExercises = minExercises, Hour = hour, StartHour = startHour, EndHour = endHour, SkillId = skillId, Modes = modes ?? Array.Empty<string>() };

    public static readonly IReadOnlyList<Achievement> All = new List<Achievement>
    {
        // ---- Milestones ----
        A("first_workout","First Rep","milestones","bronze",10,"👣","Complete your very first workout.", C("total_workouts",count:1)),
        A("workouts_5","Getting Warmed Up","milestones","bronze",10,"🏃","Complete 5 workouts.", C("total_workouts",count:5)),
        A("workouts_10","Habit Forming","milestones","bronze",10,"🔁","Complete 10 workouts.", C("total_workouts",count:10)),
        A("workouts_25","Quarter Century","milestones","silver",25,"🥈","Complete 25 workouts.", C("total_workouts",count:25)),
        A("workouts_50","Half a Hundred","milestones","silver",25,"🏅","Complete 50 workouts.", C("total_workouts",count:50)),
        A("workouts_100","Centurion","milestones","gold",50,"🏆","Complete 100 workouts.", C("total_workouts",count:100)),
        A("workouts_250","Double Century","milestones","gold",50,"🏆","Complete 250 workouts.", C("total_workouts",count:250)),
        A("workouts_500","Iron Discipline","milestones","platinum",100,"👑","Complete 500 workouts.", C("total_workouts",count:500)),

        // ---- Consistency ----
        A("streak_3","On a Roll","consistency","bronze",10,"🔥","Train 3 days in a row.", C("streak_days",days:3)),
        A("streak_7","Week Warrior","consistency","silver",25,"🔥","Train 7 days in a row.", C("streak_days",days:7)),
        A("streak_14","Fortnight Fighter","consistency","silver",25,"🔥","Train 14 days in a row.", C("streak_days",days:14)),
        A("streak_30","Unbreakable","consistency","gold",50,"🛡️","Train 30 days in a row.", C("streak_days",days:30)),
        A("streak_100","Relentless","consistency","platinum",100,"🛡️","Train 100 days in a row.", C("streak_days",days:100)),
        A("perfect_week","Perfect Week","consistency","gold",50,"📅","Work out every day of a calendar week.", C("perfect_calendar_week")),
        A("busy_month","Busy Bee","consistency","gold",50,"🗓️","Complete 20 workouts in one calendar month.", C("workouts_in_calendar_month",count:20)),
        A("weekend_warrior","Weekend Warrior","consistency","bronze",10,"🗓️","Train on both Saturday and Sunday in the same week.", C("weekend_both_days")),
        A("comeback","Comeback Kid","consistency","silver",25,"↩️","Return and train after a 14+ day break.", C("comeback_gap_days",days:14)),

        // ---- Volume ----
        A("reps_100","Hundred Club","volume","bronze",10,"💯","Perform 100 total reps.", C("total_reps",count:100)),
        A("reps_1000","Rep Machine","volume","silver",25,"⚡","Perform 1,000 total reps.", C("total_reps",count:1000)),
        A("reps_10000","Five Figures","volume","gold",50,"⚡","Perform 10,000 total reps.", C("total_reps",count:10000)),
        A("reps_50000","Volume King","volume","platinum",100,"👑","Perform 50,000 total reps.", C("total_reps",count:50000)),
        A("time_600","Time Served","volume","silver",25,"⏱️","Train for 10 cumulative hours.", C("cumulative_duration_minutes",minutes:600)),
        A("pushups_500","Push Pioneer","volume","silver",25,"💪","Perform 500 lifetime push-ups.", C("total_reps_in_family",family:"pushup",count:500)),
        A("pushups_5000","Push Master","volume","gold",50,"💪","Perform 5,000 lifetime push-ups.", C("total_reps_in_family",family:"pushup",count:5000)),
        A("pullups_250","Pull Pioneer","volume","silver",25,"🆙","Perform 250 lifetime pull-ups.", C("total_reps_in_family",family:"pullup",count:250)),
        A("pullups_2500","Pull Master","volume","gold",50,"🆙","Perform 2,500 lifetime pull-ups.", C("total_reps_in_family",family:"pullup",count:2500)),
        A("squats_1000","Leg Day Loyalist","volume","silver",25,"🦵","Perform 1,000 lifetime squats.", C("total_reps_in_family",family:"squat",count:1000)),
        A("core_1000","Core Conditioning","volume","silver",25,"🌀","Perform 1,000 lifetime core reps.", C("total_reps_in_family",family:"core",count:1000)),

        // ---- Feats ----
        A("pushups_20_set","Twenty Strong","feats","silver",25,"🎯","Do 20 push-ups in a single set.", C("single_set_reps",family:"pushup",count:20)),
        A("pullups_10_set","Pull-Up Prodigy","feats","gold",50,"🎯","Do 10 pull-ups in a single set.", C("single_set_reps",family:"pullup",count:10)),
        A("squats_50_set","Squat Squad","feats","silver",25,"🎯","Do 50 squats in a single set.", C("single_set_reps",family:"squat",count:50)),
        A("plank_120","Plank Titan","feats","silver",25,"⏲️","Hold a plank for 2 minutes.", C("single_hold_seconds",family:"plank",seconds:120)),

        // ---- Skills ----
        A("skill_pullup","First Pull-Up","skills","silver",25,"🌿","Unlock the pull-up skill.", C("skill_unlocked",skillId:"pullup")),
        A("skill_dip","Dip In","skills","silver",25,"🌿","Unlock the dip skill.", C("skill_unlocked",skillId:"dip")),
        A("skill_pistol","Pistol Pete","skills","gold",50,"🦵","Unlock the pistol squat.", C("skill_unlocked",skillId:"pistol_squat")),
        A("skill_lsit","L-Sit Legend","skills","gold",50,"🪑","Unlock the L-sit.", C("skill_unlocked",skillId:"l_sit")),
        A("skill_handstand","Handstand Hero","skills","gold",50,"🤸","Unlock the handstand.", C("skill_unlocked",skillId:"handstand")),
        A("skill_muscleup","Muscle-Up Milestone","skills","platinum",100,"🦾","Unlock the muscle-up.", C("skill_unlocked",skillId:"muscle_up")),
        A("skill_frontlever","Lever Master","skills","platinum",100,"🧗","Unlock the front lever.", C("skill_unlocked",skillId:"front_lever")),
        A("skill_planche","Planche Pursuer","skills","platinum",100,"🤲","Unlock the planche.", C("skill_unlocked",skillId:"planche")),

        // ---- Explorer ----
        A("first_plan","Architect","explorer","bronze",10,"🧱","Create your first custom plan.", C("custom_plans_created",count:1)),
        A("plans_5","Master Builder","explorer","silver",25,"🏗️","Create 5 custom plans.", C("custom_plans_created",count:5)),
        A("exercises_25","Explorer","explorer","silver",25,"🧭","Perform 25 different exercises.", C("distinct_exercises_used",count:25)),
        A("all_predefined","Sampler","explorer","silver",25,"📋","Complete every predefined plan at least once.", C("predefined_plans_all_completed")),
        A("all_modes","Mode Hopper","explorer","gold",50,"🔀","Complete a workout in every mode.", C("all_modes_completed",modes:new[]{"standard","circuit","amrap","emom","tabata"})),
        A("all_muscles_week","Well-Rounded","explorer","silver",25,"🎯","Train all muscle groups within one week.", C("distinct_muscle_groups_in_week",count:7)),

        // ---- Lifestyle ----
        A("early_bird","Early Bird","lifestyle","bronze",10,"🌅","Start a workout before 6 AM.", C("workout_start_before_hour",hour:6)),
        A("night_owl","Night Owl","lifestyle","bronze",10,"🌙","Start a workout after 10 PM.", C("workout_start_after_hour",hour:22)),
        A("lunch_beast","Lunch Break Beast","lifestyle","bronze",10,"☀️","Train between 11 AM and 2 PM.", C("workout_start_between",startHour:11,endHour:14)),
        A("marathon","Marathon Mover","lifestyle","silver",25,"⏰","Complete a workout lasting 60+ minutes.", C("single_workout_duration_min",minutes:60)),
        A("quick_dirty","Quick & Dirty","lifestyle","bronze",10,"⚡","Finish a full workout in under 10 minutes.", C("single_workout_duration_max",minutes:10,minExercises:3)),

        // ---- Secret ----
        A("double_day","Double Trouble","secret","secret",40,"✨","Complete two workouts in a single day.", C("workouts_in_single_day",count:2), hidden:true),
        A("three_prs","Overachiever","secret","secret",40,"✨","Beat 3 personal records in one workout.", C("prs_in_single_workout",count:3), hidden:true),
        A("honest_effort","Honest Effort","secret","secret",40,"✨","Log how you felt after 10 workouts.", C("effort_ratings_logged",count:10), hidden:true),
    };
}
