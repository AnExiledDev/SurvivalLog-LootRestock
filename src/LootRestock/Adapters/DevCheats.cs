using System;
using System.ComponentModel;
using LootRestock.Core;

namespace LootRestock.Adapters;

/// <summary>
/// Test buttons for Cheat Menu's Mod tests tab. Cheat Menu finds this class by its name, so neither mod needs the other.
/// Each method is one button: its DisplayName is the label and its Description the help, as "English\nChinese". What
/// it returns is shown in Cheat Menu's log.
/// </summary>
public static class DevCheats
{
    [DisplayName("Show this location\n查看当前地点")]
    [Description("Shows the containers here, how many are opened, whether that counts as explored, the restock day and the last restock.\n显示这里的柜子数量、已打开数量、是否算已探索、补货日，以及上次补货。")]
    public static string ShowThisLocation()
    {
        if (Where(out var here) is { } problem) return problem;

        var (containers, opened) = RestockHooks.Count(here!);
        var state = RestockHooks.Read(here!.MapId);
        var visit = new Visit(here.Today, containers, opened, state);
        var explored = RestockRules.IsExplored(visit.Refillable, visit.Explored, RestockHooks.Settings.ExploredShare);
        var timer = state.StartDay is { } s ? $"timer started on day {s}, restocks on day {RestockRules.DueDay(here.MapId, s, RestockHooks.Settings)}" : "no timer running";
        var lastVisit = state.LastVisitDay is { } v ? $"day {v}" : "none";
        var restocked = state.LastRestockDay is { } r ? $"day {r}" : "none";

        return $"{here.Name}, today is day {here.Today}: {RestockRules.Opened(visit)}, {(explored ? "explored" : "not explored")}. {timer}, last visit {lastVisit}, last restock {restocked}.";
    }

    [DisplayName("Show all locations\n查看所有地点")]
    [Description("Shows every post-disaster location from wherever you are: containers opened, the restock day and the last restock. The same list goes to the log when a save loads.\n在任何地方都能查看所有灾后地点：已打开的柜子、补货日和上次补货。读取存档时，同样的列表也会写进日志。")]
    public static string ShowAllLocations() =>
        RestockHooks.Overview() is { } lines ? string.Join("\n", lines) : "load a save first";

    [DisplayName("Restock here now\n立即在这里补货")]
    [Description("Restocks the opened containers here right away, whatever the timer says. Then the location waits to be explored again.\n不管倒计时，立即给这里打开过的柜子补货。之后这个地点要再次探索才会补货。")]
    public static string RestockHereNow()
    {
        if (Where(out var here) is { } problem) return problem;

        var (opened, refilled) = RestockHooks.RestockNow(here!);

        return opened == 0 ? $"{here!.Name}: no opened containers to restock" : $"{here!.Name}: {refilled} of {opened} opened containers got something back";
    }

    [DisplayName("Restock due today\n今天到补货日")]
    [Description("Makes this location's restock due by today. Leave and come back to test a real restock.\n让这个地点的补货日在今天或之前到期。离开再回来，就能测试真正的补货。")]
    public static string DueToday()
    {
        if (Where(out var here) is { } problem) return problem;

        var state = RestockHooks.Read(here!.MapId);

        // A timer started the longest range ago has run out by today, whatever it rolled. The save holds no day before 0.
        var start = Math.Max(0, here.Today - RestockHooks.Settings.RangeFor(here.MapId).Max);
        var due = RestockRules.DueDay(here.MapId, start, RestockHooks.Settings);

        RestockHooks.Write(here.MapId, state with { StartDay = start });

        // Early in a save a timer cannot have started long enough ago, so the restock day can still be ahead.
        return due <= here.Today
            ? $"{here.Name}: restock due on day {due}, today is day {here.Today}. Leave and come back to restock."
            : $"{here.Name}: the earliest the restock can be due is day {due}, today is day {here.Today}. Come back on that day to restock.";
    }

    [DisplayName("Reset all timers\n重置所有倒计时")]
    [Description("Clears the restock timer, last visit and last restock of every location, as if the mod were just added.\n清除所有地点的补货倒计时、上次到访和上次补货记录，就像刚装上这个模组一样。")]
    public static string ResetAllTimers()
    {
        if (!RestockHooks.InGame) return "load a save first";

        var cleared = 0;

        for (var mapId = RestockHooks.FirstMapId; mapId <= RestockHooks.LastMapId; mapId++)
        {
            if (RestockHooks.Read(mapId) == default) continue;

            RestockHooks.Write(mapId, default);
            cleared++;
        }

        return cleared == 0 ? "no timers to reset" : $"reset the timers of {cleared} locations";
    }

    private static string? Where(out RestockHooks.Here? here)
    {
        here = null;

        if (!RestockHooks.InGame) return "load a save first";

        here = RestockHooks.Current();

        return here == null ? "go to a post-disaster exploration location first" : null;
    }
}
