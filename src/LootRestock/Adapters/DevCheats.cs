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
        var due = state.DueDay is { } d ? $"day {d}" : "not set";
        var visit = state.LastVisitDay is { } v ? $"day {v}" : "none";

        return $"{here.Name}, today is day {here.Today}: {opened} of {containers} containers opened, {(explored ? "explored" : "not explored")}. Restock {due}, last visit {visit}.";
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
    [Description("Sets this location's restock day to today. Leave and come back to test a real restock.\n把这个地点的补货日设为今天。离开再回来，就能测试真正的补货。")]
    public static string DueToday()
    {
        if (Where(out var here) is { } problem) return problem;

        var state = RestockHooks.Read(here!.MapId);

        RestockHooks.Write(here.MapId, state with { DueDay = here.Today });

        return $"{here.Name}: restock due today (day {here.Today}). Leave and come back to restock.";
    }

    [DisplayName("Reset all timers\n重置所有倒计时")]
    [Description("Clears the restock day and last visit of every location, as if the mod were just added.\n清除所有地点的补货日和上次到访记录，就像刚装上这个模组一样。")]
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
