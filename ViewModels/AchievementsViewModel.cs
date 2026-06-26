using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Cali.Core.Models;
using Cali.Core.Services;
using Microsoft.Maui.Graphics;

namespace Cali.ViewModels;

public partial class AchievementsViewModel : ObservableObject
{
    private readonly AchievementService _achievements;
    public AchievementsViewModel(AchievementService achievements) => _achievements = achievements;

    [ObservableProperty] private int unlocked;
    [ObservableProperty] private int total;
    [ObservableProperty] private string subtitle = "";
    [ObservableProperty] private string pct = "0%";
    [ObservableProperty] private double pctFraction;

    public ObservableCollection<AchGroupVM> Groups { get; } = new();

    public async Task LoadAsync()
    {
        var st = await _achievements.GetStatusAsync();
        Unlocked = st.Count(s => s.Unlocked);
        Total = st.Count;
        var points = st.Where(s => s.Unlocked).Sum(s => s.Points);
        Subtitle = $"{points} pts · {Unlocked} of {Total} unlocked";
        PctFraction = Total == 0 ? 0 : (double)Unlocked / Total;
        Pct = $"{(int)Math.Round(PctFraction * 100)}%";

        Groups.Clear();
        foreach (var cat in AchievementCatalog.Categories)
        {
            var items = st.Where(s => s.Category == cat).Select(s => new AchItemVM(s)).ToList();
            if (items.Count > 0) Groups.Add(new AchGroupVM(AchievementCatalog.CategoryTitle(cat), items));
        }
    }
}

public sealed class AchGroupVM
{
    public string Title { get; }
    public IReadOnlyList<AchItemVM> Items { get; }
    public AchGroupVM(string title, IReadOnlyList<AchItemVM> items) { Title = title; Items = items; }
}

public sealed class AchItemVM
{
    public string Emoji { get; }
    public string Name { get; }
    public string Desc { get; }
    public bool Locked { get; }
    public bool ShowProgress { get; }
    public double ProgFraction { get; }
    public string ProgText { get; }
    public string Tag { get; }

    public Color TierColor { get; }
    public Color BorderColor { get; }
    public Color NameColor { get; }
    public Color IconBg { get; }
    public double IconOpacity { get; }
    public Color TagBg { get; }
    public Color TagText { get; }

    public AchItemVM(AchievementStatus s)
    {
        Locked = !s.Unlocked;
        bool mask = s.Hidden && Locked;

        Emoji = mask ? "❓" : s.Emoji;
        Name = mask ? "???" : s.Name;
        Desc = mask ? "Hidden — keep training to reveal it." : s.Desc;

        TierColor = TierColors.Of(s.Tier);
        BorderColor = Locked ? Tokens.Line : TierColor;
        NameColor = Locked ? Tokens.TextMuted : Tokens.Text;
        IconBg = Locked ? Tokens.Surface2 : TierColor.WithAlpha(0.18f);
        IconOpacity = Locked ? 0.4 : 1;

        ShowProgress = Locked && !mask && s.Target > 1;
        ProgFraction = s.Target == 0 ? 0 : Math.Min(1, (double)s.Cur / s.Target);
        ProgText = $"{s.Cur} / {s.Target}";

        Tag = Locked ? "LOCKED" : "✓";
        TagBg = Locked ? Tokens.Surface2 : TierColor;
        TagText = Locked ? Tokens.TextFaint : Tokens.AccentOn;
    }
}
