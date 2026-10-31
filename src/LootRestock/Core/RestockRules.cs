using System;
using System.Collections.Generic;
using System.Linq;

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
    /// <summary>The post-disaster exploration locations a player can reach, smallest first.</summary>
    public static readonly IReadOnlyList<Location> Known = new[]
    {
        new Location(1000, "HardwareStore", "Hardware Store", "社区检修小屋", new DayRange(3, 5)),
        new Location(1004, "DesertedSupermarket", "Deserted Supermarket", "残破超市", new DayRange(4, 7)),
        new Location(1002, "AbandonedSchool", "Abandoned School", "废弃学校", new DayRange(5, 8)),
        new Location(1005, "AbandonedHospital", "Abandoned Hospital", "废弃医院", new DayRange(5, 8)),
        new Location(1003, "TechCompany", "Tech Company", "科技公司", new DayRange(6, 9)),
    };

    /// <summary>For an exploration location a later game update adds.</summary>
    public static readonly DayRange OtherDefault = new(5, 8);
}

/// <summary>
/// FixedContainerPercent is the chance an opened container with fixed items gets any of them back at a restock, and
/// FixedItemPercent the chance each single item among them comes back.
/// </summary>
public sealed record RestockSettings(
    bool Enabled,
    double ExploredShare,
    IReadOnlyDictionary<int, DayRange> Ranges,
    DayRange OtherRange,
    int FixedContainerPercent = FixedRefill.DefaultContainerPercent,
    int FixedItemPercent = FixedRefill.DefaultItemPercent)
{
    public const double DefaultShare = 0.5;

    public DayRange RangeFor(int mapId) => Ranges.TryGetValue(mapId, out var range) ? range : OtherRange;

    /// <summary>A share outside 5% to 100% (or not a number) falls back to half.</summary>
    public static double CleanShare(double share) => double.IsNaN(share) || share < 0.05 || share > 1 ? DefaultShare : share;

    /// <summary>The settings in use, in one line for the log, so a player can see their config file was read.</summary>
    public string Describe()
    {
        var days = Locations.Known.Select(l => $"{l.English} {RangeFor(l.MapId)}").Append($"other locations {OtherRange}");

        return $"{(Enabled ? "on" : "off")}, explored once {Math.Round(CleanShare(ExploredShare) * 100)}% of the containers are opened, " +
               $"fixed items come back in {FixedRefill.CleanPercent(FixedContainerPercent)}% of opened containers and {FixedRefill.CleanPercent(FixedItemPercent)}% of each one's items, " +
               $"days until a restock: {string.Join(", ", days)}";
    }
}

/// <summary>One item and how many of it.</summary>
public readonly record struct ItemStack(int ItemId, int Count);

/// <summary>
/// Containers the game fills with a fixed list of items (from its own map layout) rather than random loot. At a
/// restock, each opened one gets its list back by chance: first the container rolls whether it gets anything, then
/// each single item rolls whether it comes back, so a refill is often smaller than the first fill.
/// One-time things never come back: story items, keys, notes, keepsakes, records, wallpaper, vehicle parts, and items
/// with no price, which are one-of-a-kind finds such as the Hospital's old recorder.
/// </summary>
public static class FixedRefill
{
    public const int DefaultContainerPercent = 70;
    public const int DefaultItemPercent = 70;

    /// <summary>
    /// Config_Item categories that hold one-time things: 17 vehicle parts, 18 notes and documents, 19 keepsakes and
    /// ornaments, 20 records, 21 wallpaper and flooring, 22 keys and plans.
    /// </summary>
    public static readonly IReadOnlyCollection<int> OneTimeCategories = new HashSet<int> { 17, 18, 19, 20, 21, 22 };

    /// <summary>Whether an item may come back at a restock: not tied to a story, not a one-time kind, and with a price.</summary>
    public static bool ComesBack(int category, int story, int price) => story == 0 && price > 0 && !OneTimeCategories.Contains(category);

    /// <summary>A percent below 0 or above 100 is held to that range.</summary>
    public static int CleanPercent(int percent) => Math.Clamp(percent, 0, 100);

    /// <summary>
    /// What one opened container gets back. <paramref name="next"/> gives numbers from 0 up to but not including 1,
    /// as Random.NextDouble does; a roll below the percent succeeds.
    /// </summary>
    public static List<ItemStack> Roll(IEnumerable<ItemStack> items, int containerPercent, int itemPercent, Func<double> next)
    {
        var back = new List<ItemStack>();

        if (next() * 100 >= CleanPercent(containerPercent)) return back;

        foreach (var item in items)
        {
            var kept = 0;

            for (var i = 0; i < item.Count; i++)
            {
                if (next() * 100 < CleanPercent(itemPercent)) kept++;
            }

            if (kept > 0) back.Add(item with { Count = kept });
        }

        return back;
    }

    /// <summary>
    /// What one opened container gets at a restock: random loot when it has a loot group, and its fixed items by chance.
    /// </summary>
    public static Refill Plan(int group, IReadOnlyList<ItemStack> items, int containerPercent, int itemPercent, Func<double> next) =>
        new(group > 0, items.Count > 0 ? Roll(items, containerPercent, itemPercent, next) : new List<ItemStack>());
}

/// <summary>What one container gets at a restock: whether it gets random loot, and which fixed items come back.</summary>
public readonly record struct Refill(bool Random, List<ItemStack> Fixed);

/// <summary>
/// What the mod remembers about one location, in the save. StartDay is the day its timer started, null while it waits
/// to be explored. The due day is worked out from it on each arrival, so a changed day range applies to a running timer.
/// LastRestockDay is the day the mod last added loot there. Unrefilled is how many opened containers the last restock
/// left empty: they stay opened, so they must not count toward exploring the location again.
/// </summary>
public readonly record struct LocationState(int? StartDay, int? LastVisitDay, int? LastRestockDay = null, int Unrefilled = 0)
{
    /// <summary>The save holds whole numbers where 0 means unset, and day 0 is a real day, so each day is stored plus one.</summary>
    public static int Encode(int? day) => day is { } d && d >= 0 ? d + 1 : 0;

    public static int? Decode(int stored) => stored > 0 ? stored - 1 : null;
}

/// <summary>What one visit to a location found: the day, and how many of its containers there are and have been opened.</summary>
public readonly record struct Visit(int Today, int Containers, int Opened, LocationState State)
{
    /// <summary>The containers that count toward exploring: all of them less the ones the last restock left empty.</summary>
    public int Refillable => Math.Max(0, Containers - State.Unrefilled);

    /// <summary>The opened containers that count toward exploring, less the ones the last restock left empty.</summary>
    public int Explored => Math.Max(0, Opened - State.Unrefilled);
}

/// <summary>
/// The result of arriving at a location. Restock means: add a wave of loot to every container opened since the last
/// restock, then mark those containers unopened again. Started is true when this visit started the timer. DueDay is the
/// day the running timer runs out, null while the location waits to be explored.
/// </summary>
public readonly record struct Arrival(bool Restock, LocationState Next, bool Started, int? DueDay);

/// <summary>
/// The restock cycle of one location. The game's own first fill counts as stocked. Once half the containers (the
/// explored share) have been opened, the location's timer starts, dated to the visit where that happened. The timer
/// runs for a number of days rolled from the location's day range as set at each arrival. When the timer runs out, the
/// next arrival restocks the opened containers and marks them unopened, and the location waits to be explored again.
/// A location nobody explores never restocks twice, so the mod's loot cannot pile up there.
/// </summary>
public static class RestockRules
{
    public static bool IsExplored(int containers, int opened, double share) =>
        containers > 0 && opened >= NeededToExplore(containers, share);

    /// <summary>How many of the containers must be opened for the location to count as explored, at least one.</summary>
    public static int NeededToExplore(int containers, double share) =>
        Math.Max(1, (int)Math.Ceiling(RestockSettings.CleanShare(share) * containers - 1e-9));

    /// <summary>The day a timer started on <paramref name="startDay"/> runs out, with the location's day range as set now.</summary>
    public static int DueDay(int mapId, int startDay, RestockSettings settings) => startDay + Roll(mapId, startDay, settings.RangeFor(mapId));

    /// <summary>Days until the restock, the same for the same location and timer start so a reload does not reroll it.</summary>
    public static int Roll(int mapId, int startDay, DayRange range)
    {
        var span = (ulong)(range.Max - range.Min + 1);

        return range.Min + (int)(Mix(((ulong)(uint)mapId << 32) ^ (uint)startDay) % span);
    }

    public static Arrival Arrive(int mapId, Visit visit, RestockSettings settings)
    {
        var start = visit.State.StartDay;
        var started = false;

        if (start == null && IsExplored(visit.Refillable, visit.Explored, settings.ExploredShare))
        {
            // The opening happened on the last visit; with no visit on record (the mod was just added), it counts as today.
            start = Math.Min(visit.State.LastVisitDay ?? visit.Today, visit.Today);
            started = true;
        }

        int? due = start is { } s ? DueDay(mapId, s, settings) : null;
        var restock = due is { } d && visit.Today >= d && visit.Opened > 0;

        var next = restock ? new LocationState(null, visit.Today, visit.Today) : visit.State with { StartDay = start, LastVisitDay = visit.Today };

        return new Arrival(restock, next, started, due);
    }

    /// <summary>
    /// The state after a restock that refilled <paramref name="refilled"/> containers and left <paramref name="unrefilled"/>
    /// opened ones empty. A restock that added nothing keeps the earlier last-restock day.
    /// </summary>
    public static LocationState AfterRestock(LocationState before, int today, int refilled, int unrefilled) =>
        new(null, today, refilled > 0 ? today : before.LastRestockDay, unrefilled);

    /// <summary>How many containers are opened, for the log, with the ones the last restock left empty.</summary>
    public static string Opened(Visit visit)
    {
        var line = $"{visit.Opened} of {visit.Containers} containers opened";

        return visit.State.Unrefilled > 0 ? $"{line} ({visit.State.Unrefilled} of them got nothing back at the last restock)" : line;
    }

    /// <summary>How many more containers must be opened before the location counts as explored again.</summary>
    public static int StillToOpen(Visit visit, double share) =>
        Math.Max(0, NeededToExplore(visit.Refillable, share) - visit.Explored);

    /// <summary>
    /// Where one location's cycle stands, in one line for the log: how much is opened, the timer, and the last restock.
    /// Nothing here changes the location.
    /// </summary>
    public static string Status(int mapId, int today, int containers, int opened, LocationState state, RestockSettings settings)
    {
        var visit = new Visit(today, containers, opened, state);
        var explored = IsExplored(visit.Refillable, visit.Explored, settings.ExploredShare);

        // An explored location's timer is dated to the last visit as soon as the player arrives again.
        var start = state.StartDay ?? (explored ? Math.Min(state.LastVisitDay ?? today, today) : null);
        var since = state.StartDay != null ? $"timer started on day {start}" : $"explored on day {start}";
        var timer = start is { } s
            ? DueDay(mapId, s, settings) is var due && today >= due
                ? $"{since}, restock was due on day {due} and happens on the next visit"
                : $"{since}, restocks on day {due}"
            : $"no timer, it starts once {StillToOpen(visit, settings.ExploredShare)} more are opened";
        var restocked = state.LastRestockDay is { } day ? $"last restocked on day {day}" : "not restocked yet";

        return $"{Opened(visit)}, {timer}, {restocked}";
    }

    /// <summary>
    /// The start day for a timer saved by version 0.1.2 or earlier, which kept only its due day. The start day that
    /// rolls that same due day with today's range is used, so an unchanged setting keeps its day. When the range has
    /// changed, the timer counts from the last visit, never later than the old due day allows, and never before day 0.
    /// </summary>
    public static int StartOfOldTimer(int mapId, int oldDueDay, int? lastVisitDay, RestockSettings settings)
    {
        var range = settings.RangeFor(mapId);
        var latest = Math.Min(lastVisitDay ?? int.MaxValue, oldDueDay - range.Min);
        var earliest = Math.Max(0, oldDueDay - range.Max);

        for (var start = latest; start >= earliest; start--)
        {
            if (start + Roll(mapId, start, range) == oldDueDay) return start;
        }

        // The save cannot hold a day before day 0.
        return Math.Max(0, Math.Min(lastVisitDay ?? int.MaxValue, oldDueDay - range.Max));
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
