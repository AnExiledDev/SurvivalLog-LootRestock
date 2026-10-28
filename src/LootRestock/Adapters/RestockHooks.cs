using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using LootRestock.Core;

namespace LootRestock.Adapters;

/// <summary>
/// The game fills a location's containers once, ever, in FurnitureLootManager.RefreshForCurrentMap, which runs each time
/// you arrive somewhere (it also adds a wave per survival point card). A postfix on it runs the restock cycle for
/// post-disaster exploration locations (Config_MapPoint.PointCategory 2). A restock is one more FillRandomLoot per
/// opened container, with the same loot group, count and discovery level as the game's own fill. The cycle's state
/// lives in the save as two GameCounter entries per location. The hook fails soft, back to the game's own behavior.
/// </summary>
internal static class RestockHooks
{
    private const int ExplorationCategory = 2;

    /// <summary>The game's own discovery waves skip containers with this function, so restocks do too.</summary>
    private const int NoWaveFunc = 1765;

    private static readonly HashSet<string> Reported = new();

    private static ManualLogSource? _log;
    private static RestockSettings _settings = new(true, RestockSettings.DefaultShare, new Dictionary<int, DayRange>(), Locations.OtherDefault);

    public static void Apply(Harmony harmony, ManualLogSource log)
    {
        _log = log;

        try
        {
            var original = AccessTools.Method(typeof(FurnitureLootManager), nameof(FurnitureLootManager.RefreshForCurrentMap))
                ?? throw new MissingMethodException("FurnitureLootManager.RefreshForCurrentMap");

            harmony.Patch(original, postfix: new HarmonyMethod(typeof(RestockHooks), nameof(AfterRefresh)));
            log.LogInfo("hooked the location loot refresh");
        }
        catch (Exception ex)
        {
            log.LogError($"could not hook the location loot refresh, locations never restock: {ex.Message}");
        }
    }

    public static void Use(RestockSettings settings) => _settings = settings;

    private static void AfterRefresh(FurnitureLootManager __instance)
    {
        try
        {
            if (!_settings.Enabled || __instance._locked) return;
            if (Locate(__instance) is not { } here) return;

            Arrive(here);
        }
        catch (Exception ex)
        {
            Report("restocking a location", ex);
        }
    }

    private static void Arrive(Here here)
    {
        var containers = Containers(here);
        var opened = containers.Where(c => c.Furniture.HasSearched).ToList();
        var state = new LocationState(LocationState.Decode(GameCounter.GetCounter(DueKey(here.MapId))), LocationState.Decode(GameCounter.GetCounter(VisitKey(here.MapId))));
        var arrival = RestockRules.Arrive(here.MapId, new Visit(here.Today, containers.Count, opened.Count, state), _settings);

        if (arrival.DueDay is { } due) _log?.LogInfo($"{here.Name} explored ({opened.Count} of {containers.Count} containers opened), restocks on day {due}");
        if (arrival.Restock) Restock(here, opened);

        GameCounter.SetCounter(DueKey(here.MapId), LocationState.Encode(arrival.Next.DueDay));
        GameCounter.SetCounter(VisitKey(here.MapId), LocationState.Encode(arrival.Next.LastVisitDay));
    }

    private static void Restock(Here here, List<Container> opened)
    {
        var discoverLv = ExploreDiscoveryTools.GetDiscoveryLv(here.MapId);

        foreach (var container in opened)
        {
            FurnitureLootTools.FillRandomLoot(container.Furniture, container.Group, container.Count, false, null, discoverLv, 0);
            container.Furniture.HasSearched = false;
        }

        _log?.LogInfo($"{here.Name} restocked on day {here.Today}: {opened.Count} containers");
    }

    /// <summary>The location you just arrived at, when it is a post-disaster exploration location. Upper floors count as their building.</summary>
    private static Here? Locate(FurnitureLootManager loot)
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var configs = GameSingleton.Existing<ConfigManager>();
        var role = world?._AgentManager?.GetLeadingRole();

        if (world?._GameTimeManager is not { } time || configs == null || role == null) return null;
        if (AgentTools.GetAgentComponent<UserComponent>(role) is not { } user) return null;

        var mapId = user.MapConfigId;
        var hub = configs.GetFloorGroupHub(mapId);

        if (hub > 0) mapId = hub;
        if (configs.Get_Config_MapPoint(mapId) is not { PointCategory: ExplorationCategory }) return null;

        return new Here(world, configs, mapId, loot._playerSelectId, time.GetDay(), NameOf(mapId));
    }

    /// <summary>Every container the game's own fill would fill here: unlocked, with a random loot group for your character.</summary>
    private static List<Container> Containers(Here here)
    {
        var found = new List<Container>();
        var furnitures = here.World._AgentManager?.GetLootFurnitures(here.MapId, here.Character, false);

        if (furnitures == null) return found;

        foreach (var furniture in furnitures)
        {
            if (furniture == null || furniture.IsBagLocked) continue;
            if (here.Configs.Get_Config_Furniture(furniture.AgentConfigId) is not { } config) continue;
            if (config.FurnitureFunc?.Contains(NoWaveFunc) == true) continue;

            var group = FurnitureLootManager.PickLootGroup(config, here.Character);

            if (group > 0) found.Add(new Container(furniture, group, config.LootRandomCount));
        }

        return found;
    }

    private static string NameOf(int mapId) => Locations.Known.FirstOrDefault(l => l.MapId == mapId)?.English ?? $"location {mapId}";

    private static string DueKey(int mapId) => $"lootrestock.due.{mapId}";

    private static string VisitKey(int mapId) => $"lootrestock.visit.{mapId}";

    private static void Report(string operation, Exception ex)
    {
        if (Reported.Add(operation)) _log?.LogError($"{operation} failed, the location keeps the game's own loot (each kind reported once): {ex}");
    }

    private sealed record Here(BattleLogicWorld World, ConfigManager Configs, int MapId, int Character, int Today, string Name);

    private sealed record Container(Furniture Furniture, int Group, int Count);
}
