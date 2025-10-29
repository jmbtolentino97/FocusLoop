using System;
using Microsoft.Extensions.Configuration;
using PomodoroTimer;

namespace FocusLoop.Services;

public static class AppConfig
{
    public static Config Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appSettings.json", optional: true)
            .Build();
        return new Config
        {
            WorkDuration = TimeSpan.Parse(configuration["WorkDuration"] ?? "00:25:00"),
            ShortBreakDuration = TimeSpan.Parse(configuration["ShortBreakDuration"] ?? "00:05:00"),
            LongBreakDuration = TimeSpan.Parse(configuration["LongBreakDuration"] ?? "00:15:00"),
            SessionsBeforeLongBreak = int.Parse(configuration["SessionsBeforeLongBreak"] ?? "4")
        };
    }
}