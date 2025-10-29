namespace PomodoroTimer.States
{
    internal class ShortBreakState : State
    {
        bool _isStopped = false;

        public ShortBreakState(ITimer timer) : base(timer)
        {
        }

        protected override TimeSpan GetDuration()
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
