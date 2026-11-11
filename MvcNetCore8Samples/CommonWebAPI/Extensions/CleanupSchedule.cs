namespace CommonWebAPI.Extensions;

internal static class CleanupSchedule
{
    private static readonly TimeSpan Start = TimeSpan.FromHours(22);
    private static readonly TimeSpan End = TimeSpan.FromHours(3);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(5);

    public static TimeSpan GetDelay(DateTime localNow, DateTimeOffset utcNow, DateTimeOffset? nextRun)
    {
        var intervalDelay = nextRun.HasValue && nextRun.Value > utcNow
            ? nextRun.Value - utcNow
            : TimeSpan.Zero;

        var timeOfDay = localNow.TimeOfDay;
        var windowDelay = timeOfDay >= End && timeOfDay < Start
            ? localNow.Date.Add(Start) - localNow
            : TimeSpan.Zero;

        var delay = intervalDelay > windowDelay ? intervalDelay : windowDelay;
        return delay > MaxDelay ? MaxDelay : delay;
    }
}
