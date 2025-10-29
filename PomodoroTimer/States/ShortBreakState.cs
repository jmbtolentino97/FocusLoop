namespace PomodoroTimer.States
{
    public class ShortBreakState : State
    {
        bool _isStopped = false;

        public ShortBreakState(ITimer timer) : base(timer)
        {
        }

        public override TimeSpan GetDuration()
        {
            return _appTimer.GetConfig().ShortBreakDuration;
        }

        public override void Stop()
        {
            _isStopped = true;
            base.Stop();
            TransitionToNextState();
        }

        protected override void TransitionToNextState()
        {
            base.TransitionToNextState();
            if (!_isStopped)
            {
                _appTimer.GetState().Play();
            }
        }

        protected override State GetNextState()
        {
            return new WorkState(_appTimer);
        }
    }
}
