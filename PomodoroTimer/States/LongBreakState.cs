using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroTimer.States
{
    internal class LongBreakState : State
    {
        bool _isStopped = false;

        public LongBreakState(ITimer timer) : base(timer)
        {
        }

        protected override TimeSpan GetDuration()
        {
            return _appTimer.GetConfig().LongBreakDuration;
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
