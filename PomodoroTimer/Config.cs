namespace PomodoroTimer
{
    public class Config
    {
        public TimeSpan WorkDuration { get; set; } = TimeSpan.FromMinutes(25);
        public TimeSpan ShortBreakDuration { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan LongBreakDuration { get; set; } = TimeSpan.FromMinutes(15);
        public int SessionsBeforeLongBreak { get; set; } = 4;
    }
}
