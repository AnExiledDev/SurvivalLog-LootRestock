using System;
using System.Collections.Generic;

namespace LootRestock.Core;

/// <summary>A number of in-game days, from Min to Max, both included.</summary>
public readonly record struct DayRange(int Min, int Max)
{
    public const int Longest = 365;

    /// <summary>Reads "3-5" or a single "4". Anything else, a zero, or a range past a year is refused.</summary>
    public static bool TryParse(string? text, out DayRange range)
    {
        range = default;

        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Split('-');

        if (parts.Length > 2) return false;
        if (!int.TryParse(parts[0].Trim(), out var min)) return false;

        var max = min;

        if (parts.Length == 2 && !int.TryParse(parts[1].Trim(), out max)) return false;
        if (min < 1 || max < min || max > Longest) return false;

        range = new DayRange(min, max);

        return true;
    }

    public override string ToString() => Min == Max ? Min.ToString() : $"{Min}-{Max}";
}

/// <summary>A post-disaster location the mod knows by name, with its own default day range.</summary>
public sealed record Location(int MapId, string Key, string English, string Chinese, DayRange Default);

public static class Locations
{
    /// <summary>The six post-disaster exploration locations of game 1.1.19611, smallest first.</summary>
    public static readonly IReadOnlyList<Location> Known = new[]
    {
        new Location(1000, "HardwareStore", "Hardware Store", "社区检修小屋", new DayRange(3, 5)),
        new Location(1001, "GasStation", "Gas Station", "加油站", new DayRange(3, 5)),
        new Location(1004, "DesertedSupermarket", "Deserted Supermarket", "残破超市", new DayRange(4, 7)),
        new Location(1002, "AbandonedSchool", "Abandoned School", "废弃学校", new DayRange(5, 8)),
        new Location(1005, "AbandonedHospital", "Abandoned Hospital", "废弃医院", new DayRange(5, 8)),
        new Location(1003, "TechCompany", "Tech Company", "科技公司", new DayRange(6, 9)),
    };

    /// <summary>For an exploration location a later game update adds.</summary>
    public static readonly DayRange OtherDefault = new(5, 8);
}

public sealed record RestockSettings(bool Enabled, double ExploredShare, IReadOnlyDictionary<int, DayRange> Ranges, DayRange OtherRange)
{
    public const double DefaultShare = 0.5;

    public DayRange RangeFor(int mapId) => Ranges.TryGetValue(mapId, out var range) ? range : OtherRange;

    /// <summary>A share outside 5% to 100% (or not a number) falls back to half.</summary>
    public static double CleanShare(double share) => double.IsNaN(share) || share < 0.05 || share > 1 ? DefaultShare : share;
}

/// <summary>What the mod remembers about one location, in the save. Null means not set.</summary>
public readonly record struct LocationState(int? DueDay, int? LastVisitDay)
{
    /// <summary>The save holds whole numbers where 0 means unset, and day 0 is a real day, so each day is stored plus one.</summary>
    public static int Encode(int? day) => day is { } d && d >= 0 ? d + 1 : 0;

    public static int? Decode(int stored) => stored > 0 ? stored - 1 : null;
}

/// <summary>What one visit to a location found: the day, and how many of its containers there are and have been opened.</summary>
public readonly record struct Visit(int Today, int Containers, int Opened, LocationState State);

/// <summary>
/// The result of arriving at a location. Restock means: add a wave of loot to every container opened since the last
/// restock, then mark those containers unopened again. TimerStartDay and DueDay are set when this visit started the timer.
/// </summary>
public readonly record struct Arrival(bool Restock, LocationState Next, int? TimerStartDay, int? DueDay);

/// <summary>
/// The restock cycle of one location. The game's own first fill counts as stocked. Once half the containers (the
/// explored share) have been opened, the location's timer starts, dated to the visit where that happened. When the
/// timer runs out, the next arrival restocks the opened containers and marks them unopened, and the location waits to
/// be explored again. A location nobody explores never restocks twice, so the mod's loot cannot pile up there.
/// </summary>
public static class RestockRules
{
    public static bool IsExplored(int containers, int opened, double share) =>
        containers > 0 && opened > 0 && opened >= RestockSettings.CleanShare(share) * containers;

    /// <summary>Days until the restock, the same for the same location and timer start so a reload does not reroll it.</summary>
    public static int Roll(int mapId, int startDay, DayRange range)
    {
        var span = (ulong)(range.Max - range.Min + 1);

        return range.Min + (int)(Mix(((ulong)(uint)mapId << 32) ^ (uint)startDay) % span);
    }

    public static Arrival Arrive(int mapId, Visit visit, RestockSettings settings)
    {
        var due = visit.State.DueDay;
        int? started = null;

        if (due == null && IsExplored(visit.Containers, visit.Opened, settings.ExploredShare))
        {
            // The opening happened on the last visit; with no visit on record (the mod was just added), it counts as today.
            var start = Math.Min(visit.State.LastVisitDay ?? visit.Today, visit.Today);

            started = start;
            due = start + Roll(mapId, start, settings.RangeFor(mapId));
        }

        var restock = due is { } d && visit.Today >= d && visit.Opened > 0;

        return new Arrival(restock, new LocationState(restock ? null : due, visit.Today), started, started == null ? null : due);
    }

    // splitmix64 finalizer: spreads neighbouring locations and days apart.
    private static ulong Mix(ulong x)
    {
        x += 0x9E3779B97F4A7C15UL;
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;

        return x ^ (x >> 31);
    }
}
