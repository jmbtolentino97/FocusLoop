using System;

namespace FocusLoop.Utilities;

public static class DashHelper
{
    public static int Calculate(TimeSpan workDuration)
    {
        int dashCount;
        if (workDuration.TotalHours >= 1.0)
            dashCount = (int)workDuration.TotalHours - 1;
        else if (workDuration.TotalMinutes >= 1.0)
            dashCount = (int)workDuration.TotalMinutes - 1;
        else
            dashCount = (int)workDuration.TotalSeconds - 1;
        return Math.Max(9, dashCount);
    }
}