using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.Configuration;
using PomodoroTimer;
using PomodoroTimer.States;
using FocusLoop.Notifications;

namespace FocusLoop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private double _timerProgress;
    private string _remainingTimeText = "25:00";
    private Timer _timer;
    private UiRunState _uiRunState = UiRunState.Stopped;

    private bool _isPlayVisible = true;
    private bool _isPauseVisible = false;
    private bool _isStopVisible = false;
    private IBrush? _progressStroke;

    private static readonly IBrush WorkBrush = new SolidColorBrush(Color.Parse("#2F80ED"));
    private static readonly IBrush BreakBrush = new SolidColorBrush(Color.Parse("#27AE60"));

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        WindowsToast.EnsureRegistration();

        var config = GetTimerConfig();
        SetDashCountFromWorkDuration(config.WorkDuration);

        _timer = new Timer(config, UpdateTime, OnStateChanged);
        ProgressStroke = WorkBrush;
        UpdateButtons();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public double TimerProgress
    {
        get => _timerProgress;
        set
        {
            if (value.Equals(_timerProgress)) return;
            _timerProgress = value;
            OnPropertyChanged();
        }
    }

    public string RemainingTimeText
    {
        get => _remainingTimeText;
        set
        {
            if (value == _remainingTimeText) return;
            _remainingTimeText = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlayVisible
    {
        get => _isPlayVisible;
        set
        {
            if (value == _isPlayVisible) return;
            _isPlayVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsPauseVisible
    {
        get => _isPauseVisible;
        set
        {
            if (value == _isPauseVisible) return;
            _isPauseVisible = value;
            OnPropertyChanged();
        }
    }

    public bool IsStopVisible
    {
        get => _isStopVisible;
        set
        {
            if (value == _isStopVisible) return;
            _isStopVisible = value;
            OnPropertyChanged();
        }
    }

    public IBrush? ProgressStroke
    {
        get => _progressStroke;
        set
        {
            if (ReferenceEquals(value, _progressStroke)) return;
            _progressStroke = value;
            OnPropertyChanged();
        }
    }

    private Config GetTimerConfig()
    {
        var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appSettings.json", true).Build();
        return new Config()
        {
            WorkDuration = TimeSpan.Parse(configuration["WorkDuration"] ?? "00:25:00"),
            ShortBreakDuration = TimeSpan.Parse(configuration["ShortBreakDuration"] ?? "00:05:00"),
            LongBreakDuration = TimeSpan.Parse(configuration["LongBreakDuration"] ?? "00:15:00"),
            SessionsBeforeLongBreak = int.Parse(configuration["SessionsBeforeLongBreak"] ?? "4")
        };
    }

    private void UpdateTime(State state, TimeSpan remainingTime)
    {
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");
        SyncProgress(state, remainingTime);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
        _timer.Stop();
        _uiRunState = UiRunState.Stopped;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        IsPlayVisible = _uiRunState != UiRunState.Playing;
        IsPauseVisible = _uiRunState == UiRunState.Playing && _timer.GetState() is WorkState;
        IsStopVisible = _uiRunState == UiRunState.Paused || _timer.GetState() is not WorkState;
    }

    private void SyncProgress(State state, TimeSpan remaining)
    {
        var totalSecs = Math.Max(1.0, state.GetDuration().TotalSeconds);
        var elapsedRatio = Math.Clamp(1.0 - (remaining.TotalSeconds / totalSecs), 0.0, 1.0);
        TimerProgress = elapsedRatio;
    }

    private void SetDashCountFromWorkDuration(TimeSpan workDuration)
    {
        int dashCount;
        if (workDuration.TotalHours >= 1.0)
            dashCount = (int)workDuration.TotalHours - 1;
        else if (workDuration.TotalMinutes >= 1.0)
            dashCount = (int)workDuration.TotalMinutes - 1;
        else
            dashCount = (int)workDuration.TotalSeconds - 1;

        dashCount = Math.Max(9, dashCount);
        if (CircularProgress != null)
            CircularProgress.DashCount = dashCount;
    }

    private void OnStateChanged(State oldState, State newState)
    {
        UpdateProgressBrush(newState);
        UpdateButtons();
        WindowsToast.ShowStateChangeToast(oldState, newState);
    }

    private void UpdateProgressBrush(State newState)
    {
        ProgressStroke = newState is WorkState ? WorkBrush : BreakBrush;
    }

    private enum UiRunState
    {
        Stopped,
        Playing,
        Paused
    }
}