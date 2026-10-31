using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using LootRestock.Adapters;
using LootRestock.Core;

namespace LootRestock;

[BepInPlugin(Guid, "Loot Restock", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.lootrestock";
    private const string Version = "0.1.3";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var enabled = Config.Bind("General", "Enabled", true,
            "When true, post-disaster exploration locations restock the containers you opened, a few days after you explored them. When false, nothing restocks.\n" +
            "设为 true 时，灾后的探索地点会在你探索过几天后，给你打开过的柜子补充新物资。设为 false 时，不会补货。");
        var share = Config.Bind("General", "ExploredShare", RestockSettings.DefaultShare,
            new ConfigDescription(
                "How many of a location's containers you must open before it counts as explored and its restock timer starts. 0.5 means half. A location that is not explored again does not restock again, so loot does not pile up in places you skip.\n" +
                "一个地点要打开多少比例的柜子才算探索过，并开始补货倒计时。0.5 表示一半。没有再次探索的地点不会再次补货，所以你不去的地方不会越堆越多。",
                new AcceptableValueRange<double>(0.05, 1)));
        var days = Locations.Known.ToDictionary(l => l, l => Config.Bind("Days", l.Key, l.Default.ToString(),
            $"Days after you explore the {l.English} until its containers restock, picked at random in this range each time. Write it as 3-5, or as one number. A change also applies to a timer already counting.\n" +
            $"探索{l.Chinese}之后，过多少天柜子会补货，每次在这个范围内随机。写成 3-5，或者只写一个数字。修改也会用于已经在倒计时的地点。"));
        var other = Config.Bind("Days", "OtherLocations", Locations.OtherDefault.ToString(),
            "Days until a restock for an exploration location that is not listed above, such as one a game update adds. Write it as 3-5, or as one number. A change also applies to a timer already counting.\n" +
            "上面没有列出的探索地点（比如游戏更新新增的地点）补货需要的天数。写成 3-5，或者只写一个数字。修改也会用于已经在倒计时的地点。");

        Log.LogInfo($"===== Loot Restock {Version} started");
        RestockHooks.Apply(new Harmony(Guid), Log);

        var entries = new List<ConfigEntryBase> { enabled, share, other };

        entries.AddRange(days.Values);
        OptionalLive.Watch(() => RestockHooks.Use(Settings(enabled.Value, share.Value, days, other)), entries.ToArray());
    }

    private RestockSettings Settings(bool enabled, double share, Dictionary<Location, ConfigEntry<string>> days, ConfigEntry<string> other)
    {
        var ranges = days.ToDictionary(d => d.Key.MapId, d => Range(d.Value, d.Key.Default));

        return new RestockSettings(enabled, RestockSettings.CleanShare(share), ranges, Range(other, Locations.OtherDefault));
    }

    private DayRange Range(ConfigEntry<string> entry, DayRange fallback)
    {
        if (DayRange.TryParse(entry.Value, out var range)) return range;

        Log.LogWarning($"{entry.Definition.Key} = \"{entry.Value}\" is not a day range like 3-5, so {fallback} is used");

        return fallback;
    }
}
