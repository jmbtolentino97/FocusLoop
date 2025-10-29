using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroTimer.States
{
    public abstract class State
    {
        protected readonly ITimer _appTimer;
        protected System.Threading.Timer? _timer;
        protected TimeSpan _remainingTime;

        public State(ITimer appTimer)
        {
            _appTimer = appTimer;
            UpdateTime(GetDuration());
        }

        protected abstract State GetNextState();
        protected abstract TimeSpan GetDuration();

        protected virtual void OnTimerElapsed(object? state)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));

            if (_remainingTime <= TimeSpan.Zero)
            {
                _timer!.Dispose();
                TransitionToNextState();
                return;
            }

            UpdateTime(_remainingTime);
        }

        public virtual void Play()
        {
            if (_timer == null)
            {
                _timer = new System.Threading.Timer(OnTimerElapsed, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
            else
            {
                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }

        public virtual void Pause()
        {
            _timer!.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public virtual void Stop()
        {
            _timer!.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
            _timer = null;

            UpdateTime(GetDuration());
        }

        protected virtual void TransitionToNextState()
        {
            var nextState = GetNextState();
            _appTimer.SetState(nextState);
        }

        protected void UpdateTime(TimeSpan updatedTimeSpan)
        {
            _remainingTime = updatedTimeSpan;
            _appTimer.UpdateTime(this, _remainingTime);
        }
    }
}
