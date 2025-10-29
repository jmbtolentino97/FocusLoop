using PomodoroTimer.States;

namespace PomodoroTimer
{
    public interface ITimer
    {
        Config GetConfig();
        State GetState();
        void SetState(State state);
        void UpdateTime(State state, TimeSpan remainingTime);
        int CountCompletedPomodoro();
        void AddPomodoro();
    }
}
