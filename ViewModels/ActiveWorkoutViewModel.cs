using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cali.Core.Abstractions;
using Cali.Core.Models;
using Cali.Core.Services;

namespace Cali.ViewModels;

public partial class ActiveWorkoutViewModel : ObservableObject
{
    public WorkoutSessionService Session { get; }
    private readonly IFeedbackService _feedback;

    public string PlanName { get; private set; } = "";

    /// <summary>Raised when the workout ends (End button or finishing the last exercise) — go to Summary.</summary>
    public event Action? Finished;

    public ActiveWorkoutViewModel(WorkoutSessionService session, IFeedbackService feedback)
    {
        Session = session;
        _feedback = feedback;
        Session.RestElapsed += OnRestElapsed;
        Session.Finished += OnSessionFinished;
    }

    public void Start(Plan plan)
    {
        PlanName = plan.Name;
        Session.Start(plan);
    }

    private void OnRestElapsed() => _feedback.Play();
    private void OnSessionFinished() => Finished?.Invoke();

    [RelayCommand] private void CompleteSet() { Session.CompleteSet(); _feedback.Tick(); }
    [RelayCommand] private void IncRep() => Session.IncPending();
    [RelayCommand] private void DecRep() => Session.DecPending();
    [RelayCommand] private void Rest() => Session.StartRest();
    [RelayCommand] private void AddRest() => Session.AddRest();
    [RelayCommand] private void SkipRest() => Session.SkipRest();
    [RelayCommand] private void Preset(string sec) { if (int.TryParse(sec, out var s)) Session.SetRestPreset(s); }
    [RelayCommand] private void Forward() => Session.GoNext();
    [RelayCommand] private void Backward() => Session.GoPrev();
    [RelayCommand] private void End() => Session.End();

    public void Detach()
    {
        Session.RestElapsed -= OnRestElapsed;
        Session.Finished -= OnSessionFinished;
    }
}
