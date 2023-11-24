using System;
using Hangfire;

namespace SugarTalk.Core.Jobs;

public interface IRecurringJob : IJob
{
    string CronExpression { get; }

    TimeZoneInfo TimeZone => null;
}