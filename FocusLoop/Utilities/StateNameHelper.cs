using PomodoroTimer.States;

namespace FocusLoop.Utilities;

public static class StateNameHelper
{
    public static string GetFriendlyName(State state)
        => state switch
        {
            WorkState => "Work",
            ShortBreakState => "Short Break",
            LongBreakState => "Long Break",
            _ => state.GetType().Name
        };
}