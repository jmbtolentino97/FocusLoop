using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using FocusLoop.Notifications;
using FocusLoop.Services;
using FocusLoop.Utilities;
using PomodoroTimer.States;
using PomodoroTimer;

namespace FocusLoop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly Timer _timer;
    private UiRunState _uiRunState = UiRunState.Stopped;

    private double _timerProgress;
    private string _remainingTimeText = "25:00";
    private bool _isPlayVisible = true;
    private bool _isPauseVisible;
    private bool _isStopVisible;
    private IBrush? _progressStroke;
    private int _dashCount = 24;

    private static readonly IBrush WorkBrush = new SolidColorBrush(Color.Parse("#2F80ED"));
    private static readonly IBrush BreakBrush = new SolidColorBrush(Color.Parse("#27AE60"));

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        var config = AppConfig.Load();
        DashCount = DashHelper.Calculate(config.WorkDuration);
        _timer = new Timer(config, UpdateTime, OnStateChanged);
        ProgressStroke = WorkBrush;
        UpdateButtons();
    }

    private void OnStateChanged(State oldState, State newState)
    {
        ProgressStroke = newState is WorkState ? WorkBrush : BreakBrush;
        UpdateButtons();
        if (_uiRunState != UiRunState.Stopped) WindowsToast.ShowStateChangeToast(oldState, newState);
    }

    private void OnPlayClicked(object? sender, RoutedEventArgs e)
    {
        _timer.Play();
        _uiRunState = UiRunState.Playing;
        UpdateButtons();
    }
    private void OnPauseClicked(object? sender, RoutedEventArgs e)
    {
        _timer.Pause();
        _uiRunState = UiRunState.Paused;
        UpdateButtons();
    }
    private void OnStopClicked(object? sender, RoutedEventArgs e)
    {
        _uiRunState = UiRunState.Stopped;
        _timer.Stop();
        UpdateButtons();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value!;
        OnPropertyChanged(name);
        return true;
    }

    public double TimerProgress { get => _timerProgress; set => SetField(ref _timerProgress, value); }
    public string RemainingTimeText { get => _remainingTimeText; set => SetField(ref _remainingTimeText, value); }
    public bool IsPlayVisible { get => _isPlayVisible; set => SetField(ref _isPlayVisible, value); }
    public bool IsPauseVisible { get => _isPauseVisible; set => SetField(ref _isPauseVisible, value); }
    public bool IsStopVisible { get => _isStopVisible; set => SetField(ref _isStopVisible, value); }
    public IBrush? ProgressStroke { get => _progressStroke; set => SetField(ref _progressStroke, value); }
    public int DashCount { get => _dashCount; set => SetField(ref _dashCount, value); }

    private void UpdateButtons()
    {
        var isWork = _timer.GetState() is WorkState;
        IsPlayVisible = _uiRunState != UiRunState.Playing;
        IsPauseVisible = _uiRunState == UiRunState.Playing && isWork;
        IsStopVisible = _uiRunState == UiRunState.Paused || !isWork;
    }

    private void UpdateTime(State state, TimeSpan remainingTime)
    {
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");
        var totalSecs = Math.Max(1.0, state.GetDuration().TotalSeconds);
        var elapsedRatio = Math.Clamp(1.0 - (remainingTime.TotalSeconds / totalSecs), 0.0, 1.0);
        TimerProgress = elapsedRatio;
    }

    private enum UiRunState { Stopped, Playing, Paused }
}