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
    private const int FirstMapId = 1000;
    private const int LastMapId = 1099;

    [DisplayName("Show this location\n查看当前地点")]
    [Description("Shows the containers here, how many are opened, whether that counts as explored, and the restock day.\n显示这里的柜子数量、已打开数量、是否算已探索，以及补货日。")]
    public static string ShowThisLocation()
    {
        if (Where(out var here) is { } problem) return problem;

        var (containers, opened) = RestockHooks.Count(here!);
        var state = RestockHooks.Read(here!.MapId);
        var explored = RestockRules.IsExplored(containers, opened, RestockHooks.Settings.ExploredShare);
        var timer = state.StartDay is { } s ? $"timer started on day {s}, restocks on day {RestockRules.DueDay(here.MapId, s, RestockHooks.Settings)}" : "no timer running";
        var visit = state.LastVisitDay is { } v ? $"day {v}" : "none";

        return $"{here.Name}, today is day {here.Today}: {opened} of {containers} containers opened, {(explored ? "explored" : "not explored")}. {timer}, last visit {visit}.";
    }

    [DisplayName("Restock here now\n立即在这里补货")]
    [Description("Restocks the opened containers here right away, whatever the timer says. Then the location waits to be explored again.\n不管倒计时，立即给这里打开过的柜子补货。之后这个地点要再次探索才会补货。")]
    public static string RestockHereNow()
    {
        if (Where(out var here) is { } problem) return problem;

        var count = RestockHooks.RestockNow(here!);

        return count == 0 ? $"{here!.Name}: no opened containers to restock" : $"{here!.Name}: restocked {count} containers";
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

        return $"{here.Name}: restock due on day {due}, today is day {here.Today}. Leave and come back to restock.";
    }

    [DisplayName("Reset all timers\n重置所有倒计时")]
    [Description("Clears the restock timer and last visit of every location, as if the mod were just added.\n清除所有地点的补货倒计时和上次到访记录，就像刚装上这个模组一样。")]
    public static string ResetAllTimers()
    {
        if (!RestockHooks.InGame) return "load a save first";

        var cleared = 0;

        for (var mapId = FirstMapId; mapId <= LastMapId; mapId++)
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
