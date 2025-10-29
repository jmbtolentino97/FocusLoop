using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using PomodoroTimer;
using PomodoroTimer.States;

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
    private TimeSpan _phaseTotalDuration = TimeSpan.Zero;
    private string? _lastStateName;
    private IBrush? _progressStroke;

    private static readonly IBrush WorkBrush = new SolidColorBrush(Color.Parse("#2F80ED"));
    private static readonly IBrush BreakBrush = new SolidColorBrush(Color.Parse("#27AE60"));

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _timer = new Timer(GetTimerConfig(), UpdateTime);
        ProgressStroke = WorkBrush;
        UpdateButtons(_timer.GetState());
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
            ShortBreakDuration = TimeSpan.Parse(configuration["ShortBreakDuration"] ?? "00:05"),
            LongBreakDuration = TimeSpan.Parse(configuration["LongBreakDuration"] ?? "00:15"),
            SessionsBeforeLongBreak = int.Parse(configuration["SessionsBeforeLongBreak"] ?? "4")
        };
    }

    private void UpdateTime(State state, TimeSpan remainingTime)
    {
        RemainingTimeText = remainingTime.ToString(@"mm\:ss");
        SyncProgress(state, remainingTime);
        UpdateProgressBrush(state);
        UpdateButtons(state);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Button handlers (hook these up to your timer logic as needed)
    private void OnPlayClicked(object? sender, RoutedEventArgs e)
    {
        _timer.Play();
        _uiRunState = UiRunState.Playing;
        UpdateButtons(_timer.GetState());
    }

    private void OnPauseClicked(object? sender, RoutedEventArgs e)
    {
        _timer.Pause();
        _uiRunState = UiRunState.Paused;
        UpdateButtons(_timer.GetState());
    }

    private void OnStopClicked(object? sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _uiRunState = UiRunState.Stopped;
        UpdateButtons(_timer.GetState());
    }

    private void UpdateButtons(State state)
    {
        switch (_uiRunState)
        {
            case UiRunState.Playing:
                IsPlayVisible = false;
                IsPauseVisible = true;
                IsStopVisible = false;
                break;
            case UiRunState.Paused:
                IsPlayVisible = true;
                IsPauseVisible = false;
                IsStopVisible = true;
                break;
            case UiRunState.Stopped:
            default:
                IsPlayVisible = true;
                IsPauseVisible = false;
                IsStopVisible = false;
                break;
        }
    }

    private void SyncProgress(State state, TimeSpan remaining)
    {
        var stateName = state.GetType().Name;
        if (_lastStateName != stateName || remaining > _phaseTotalDuration || _phaseTotalDuration == TimeSpan.Zero)
        {
            // Capture the phase's total duration at the first tick of a new phase
            _phaseTotalDuration = remaining;
            _lastStateName = stateName;
            UpdateDashCountForPhase(_phaseTotalDuration);
        }

        var totalSecs = Math.Max(1.0, _phaseTotalDuration.TotalSeconds);
        var elapsedRatio = Math.Clamp(1.0 - (remaining.TotalSeconds / totalSecs), 0.0, 1.0);
        TimerProgress = elapsedRatio;
    }

    private void UpdateDashCountForPhase(TimeSpan total)
    {
        int dashCount;
        if (total.TotalHours >= 1.0)
        {
            dashCount = (int)Math.Round(total.TotalHours) - 1; // e.g., 6h -> 5
        }
        else if (total.TotalMinutes >= 1.0)
        {
            dashCount = (int)Math.Round(total.TotalMinutes) - 1; // e.g., 25m -> 24
        }
        else
        {
            dashCount = (int)Math.Round(total.TotalSeconds) - 1; // e.g., 10s -> 9
        }

        dashCount = Math.Max(1, dashCount);

        // Ensure UI-thread update
        Dispatcher.UIThread.Post(() =>
        {
            if (CircularProgress != null)
            {
                CircularProgress.DashCount = dashCount;
            }
        });
    }

    private void UpdateProgressBrush(State state)
    {
        var name = state.GetType().Name;
        var isBreak = string.Equals(name, "ShortBreakState", StringComparison.Ordinal)
                      || string.Equals(name, "LongBreakState", StringComparison.Ordinal);
        ProgressStroke = isBreak ? BreakBrush : WorkBrush;
    }

    private enum UiRunState
    {
        Stopped,
        Playing,
        Paused
    }
}
