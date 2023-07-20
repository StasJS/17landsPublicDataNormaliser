using System.Diagnostics;

namespace Util;

internal static class NumberExtensions
{
    private const long OneKb = 1024;
    private const long OneMb = OneKb * 1024;
    private const long OneGb = OneMb * 1024;
    private const long OneTb = OneGb * 1024;

    public static string ToPrettySize(this int value, int decimalPlaces = 0)
    {
        return ((long)value).ToPrettySize(decimalPlaces);
    }

    public static string ToPrettySize(this long value, int decimalPlaces = 0)
    {
        var asTb = Math.Round((double)value / OneTb, decimalPlaces);
        var asGb = Math.Round((double)value / OneGb, decimalPlaces);
        var asMb = Math.Round((double)value / OneMb, decimalPlaces);
        var asKb = Math.Round((double)value / OneKb, decimalPlaces);
        string chosenValue = asTb > 1 ? string.Format("{0}Tb", asTb)
            : asGb > 1 ? string.Format("{0}Gb", asGb)
            : asMb > 1 ? string.Format("{0}Mb", asMb)
            : asKb > 1 ? string.Format("{0}Kb", asKb)
            : string.Format("{0}B", Math.Round((double)value, decimalPlaces));
        return chosenValue;
    }
}

public static class MemoryProfiling
{
    public static void Profile(string toProfileDescription, Action toProfile)
    {
        long memoryBefore = 0;
        long memoryAfter = 0;
        var duration = Timing.Time(() =>
        {
            memoryBefore = GC.GetTotalMemory(true);
            toProfile();
            memoryAfter = GC.GetTotalMemory(true);
        });
        Console.WriteLine($"{toProfileDescription}. Memory before: {memoryBefore.ToPrettySize()}. Memory after: {memoryAfter.ToPrettySize()}. Profiling duration: {duration.TotalSeconds}s");
    }
}

public static class Timing
{
    public static (T, TimeSpan) Time<T>(Func<T> func)
    {
        var stopWatch = Stopwatch.StartNew();
        var result = func();
        stopWatch.Stop();
        return (result, stopWatch.Elapsed);
    }

    public static TimeSpan Time(Action action)
    {
        var stopWatch = Stopwatch.StartNew();
        action();
        stopWatch.Stop();
        return stopWatch.Elapsed;
    }
}