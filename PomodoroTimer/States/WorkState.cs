namespace PomodoroTimer.States
{
    internal class WorkState : State
    {
        public WorkState(ITimer timer) : base(timer)
        {
        }

        protected override TimeSpan GetDuration()
        {
            return _appTimer.GetConfig().WorkDuration;
        }

        protected override void TransitionToNextState()
        {
            _appTimer.AddPomodoro();
            base.TransitionToNextState();
            _appTimer.GetState().Play();
        }

        protected override State GetNextState()
        {
            return _appTimer.CountCompletedPomodoro() % _appTimer.GetConfig().SessionsBeforeLongBreak == 0
                ? new LongBreakState(_appTimer)
                : new ShortBreakState(_appTimer);
        }
    }
}
