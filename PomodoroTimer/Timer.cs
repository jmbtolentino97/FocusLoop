using PomodoroTimer.States;

namespace PomodoroTimer
{
    public class Timer : ITimer
    {
        private State _state;
        private Action<State, TimeSpan>? _updateTimeCallback;
        private Action<State, State>? _onStateChangeCallback;
        private int _completedWorkSessions = 0;
        private Config _config;

        public Timer(Config? config = null, Action<State, TimeSpan>? updateTimeCallback = null, Action<State, State>? onStateChangeCallback = null)
        {
            _updateTimeCallback = updateTimeCallback;
            _onStateChangeCallback = onStateChangeCallback;
            _config = config ?? new Config();
            _state = new WorkState(this);
        }

        public Config GetConfig()
        {
            return _config;
        }

        public void Play()
        {
            _state.Play();
        }

        public void Pause()
        {
            _state.Pause();
        }

        public void Stop()
        {
            _state.Stop();
        }

        public State GetState()
        {
            return _state;
        }

        public void SetState(State state)
        {
            var oldState = _state;
            _state = state;
            _onStateChangeCallback?.Invoke(oldState, state);
        }

        public void UpdateTime(State state, TimeSpan remainingTime)
        {
            _updateTimeCallback?.Invoke(state, remainingTime);
        }

        public int CountCompletedPomodoro()
        {
            return _completedWorkSessions;
        }

        public void AddPomodoro()
        {
            _completedWorkSessions++;
        }
    }
}
