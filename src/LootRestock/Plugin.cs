using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using ExiledToolkit.Game;
using HarmonyLib;
using LootRestock.Adapters;
using LootRestock.Core;

namespace LootRestock;

[BepInPlugin(Guid, "Loot Restock", Version)]
[BepInDependency(ToolkitGuid, ">=0.19.0")]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.lootrestock";
    private const string Version = "0.2.4";
    private const string ToolkitGuid = "com.dev.survivallog.exiledtoolkit";

    /// <summary>Each world may keep its own value of a General setting (Exiled's Toolkit). The day ranges are text, so they stay global.</summary>
    private const string WorldRule = "ModManager.WorldRule";

    public override void Load()
    {
        var enabled = Config.Bind("General", "Enabled", true, new ConfigDescription(
            "When true, post-disaster exploration locations restock the containers you opened, a few days after you explored them. When false, nothing restocks.\n" +
            "设为 true 时，灾后的探索地点会在你探索过几天后，给你打开过的柜子补充新物资。设为 false 时，不会补货。",
            null, WorldRule));
        var share = Config.Bind("General", "ExploredShare", RestockSettings.DefaultShare,
            new ConfigDescription(
                "How many of a location's containers you must open before it counts as explored and its restock timer starts. 0.5 means half. After a restock, a location only restocks again once you explore it again, so loot doesn't pile up in places you skip.\n" +
                "一个地点要打开多少比例的柜子才算探索过，并开始补货倒计时。0.5 表示一半。补货之后，要再次探索这个地点才会再补，所以你不去的地方不会越堆越多。",
                new AcceptableValueRange<double>(0.05, 1), WorldRule));
        var fixedContainers = Config.Bind("General", "FixedContainerPercent", FixedRefill.DefaultContainerPercent,
            new ConfigDescription(
                "Some containers always hold the same items, placed there by the game instead of picked at random. Most of the Tech Company is like this. When a location restocks, each of these you opened has this percent chance to get any of its items back. 0 means they never refill, 100 means they always do.\n" +
                "有些柜子里的物品是游戏固定摆放的，不是随机生成的，科技公司大多如此。地点补货时，你打开过的这类柜子各有这个百分比的几率重新放回物品。设为 0 则不会补回，设为 100 则每次都会补回。",
                new AcceptableValueRange<int>(0, 100), WorldRule));
        var fixedItems = Config.Bind("General", "FixedItemPercent", FixedRefill.DefaultItemPercent,
            new ConfigDescription(
                "When a container that always holds the same items gets them back at a restock, this is the percent chance each single item comes back, so a refill is often smaller than the first fill. Keys, notes, story items and other one-time items never come back.\n" +
                "物品固定摆放的柜子在补货时放回物品，每一件物品各有这个百分比的几率回来，所以补回的通常比第一次少。钥匙、笔记、剧情物品等一次性物品不会回来。",
                new AcceptableValueRange<int>(0, 100), WorldRule));
        var days = Locations.Known.ToDictionary(l => l, l => Config.Bind("Days", l.Key, l.Default.ToString(),
            $"How many days the {l.English} waits after you explore it before its containers restock. Write a range such as 3-5 to get a random number of days from it each time, or write one number. A change also applies to a timer that is already counting.\n" +
            $"探索{l.Chinese}之后，过多少天柜子会补货。可以写一个范围，比如 3-5，每次在范围内随机取一个天数，也可以只写一个数字。修改也会用于已经在倒计时的地点。"));
        var other = Config.Bind("Days", "OtherLocations", Locations.OtherDefault.ToString(),
            "How many days an exploration location that isn't listed here waits after you explore it before it restocks. This covers any new location a game update adds. Write a range such as 3-5, or one number. A change also applies to a timer that is already counting.\n" +
            "这里没有列出的探索地点（比如游戏更新新增的地点），探索之后过多少天补货。可以写一个范围，比如 3-5，也可以只写一个数字。修改也会用于已经在倒计时的地点。");

        Log.LogInfo($"===== Loot Restock {Version} started");
        RestockHooks.Apply(new PatchKit(new Harmony(Guid), Write), Log);

        var entries = new List<ConfigEntryBase> { enabled, share, fixedContainers, fixedItems, other };

        entries.AddRange(days.Values);
        ExiledToolkit.LiveSettings.Watch(() => RestockHooks.Use(Settings(enabled.Value, share.Value, fixedContainers.Value, fixedItems.Value, days, other)), entries.ToArray());
    }

    private RestockSettings Settings(bool enabled, double share, int fixedContainers, int fixedItems, Dictionary<Location, ConfigEntry<string>> days, ConfigEntry<string> other)
    {
        var ranges = days.ToDictionary(d => d.Key.MapId, d => Range(d.Value, d.Key.Default));

        return new RestockSettings(enabled, RestockSettings.CleanShare(share), ranges, Range(other, Locations.OtherDefault), FixedRefill.CleanPercent(fixedContainers), FixedRefill.CleanPercent(fixedItems));
    }

    private DayRange Range(ConfigEntry<string> entry, DayRange fallback)
    {
        if (DayRange.TryParse(entry.Value, out var range)) return range;

        Log.LogWarning($"{entry.Definition.Key} = \"{entry.Value}\" is not a day range like 3-5, so {fallback} is used");

        return fallback;
    }

    // A hook that could not go in is an error, so the Mod Manager can show the mod may need an update.
    private void Write(string message)
    {
        if (HookFailure.Is(message)) Log.LogError(message);
        else Log.LogInfo(message);
    }
}
