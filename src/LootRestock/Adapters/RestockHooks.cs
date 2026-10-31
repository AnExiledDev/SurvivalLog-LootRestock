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
/// opened container that has a random loot group, with the same loot group, count and discovery level as the game's own
/// fill. Containers the map layout (Config_GuidanceFurniture) fills with a fixed list of items get that list back by
/// chance (FixedRefill), less the one-time items. The cycle's state
/// lives in the save as GameCounter entries per location: the day its timer started, the day of the last visit and the
/// day of the last restock. A postfix on BattleLogicWorld.StartBattle logs every location's state when a save loads.
/// The hooks fail soft, back to the game's own behavior.
/// </summary>
internal static class RestockHooks
{
    private const int ExplorationCategory = 2;

    /// <summary>The map ids post-disaster locations use. Besides the known ones, only ids with a saved state are listed.</summary>
    internal const int FirstMapId = 1000;
    internal const int LastMapId = 1099;

    /// <summary>The game's own discovery waves skip containers with this function, so restocks do too.</summary>
    private const int NoWaveFunc = 1765;

    private static readonly HashSet<string> Reported = new();
    private static readonly Random Dice = new();

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

        try
        {
            var original = AccessTools.Method(typeof(BattleLogicWorld), nameof(BattleLogicWorld.StartBattle))
                ?? throw new MissingMethodException("BattleLogicWorld.StartBattle");

            harmony.Patch(original, postfix: new HarmonyMethod(typeof(RestockHooks), nameof(AfterLoad)));
            log.LogInfo("hooked the save load");
        }
        catch (Exception ex)
        {
            log.LogError($"could not hook the save load, the log has no location overview when a save loads: {ex.Message}");
        }
    }

    /// <summary>Takes the settings at start and after each live change, and logs them so the log shows what was read.</summary>
    public static void Use(RestockSettings settings)
    {
        _settings = settings;
        _log?.LogInfo($"settings: {settings.Describe()}");
    }

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

    /// <summary>Runs after a save has loaded and its arrival is done, so the overview shows what the save holds now.</summary>
    private static void AfterLoad()
    {
        try
        {
            if (Overview() is not { } lines) return;

            foreach (var line in lines) _log?.LogInfo(line);
        }
        catch (Exception ex)
        {
            Report("listing the locations at load", ex);
        }
    }

    /// <summary>
    /// The settings in use, then one line per post-disaster location: the known ones, and any other that has a saved
    /// state. Reads only, from wherever the player is. Null while no save is loaded.
    /// </summary>
    internal static List<string>? Overview()
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var configs = GameSingleton.Existing<ConfigManager>();

        if (world?._GameTimeManager is not { } time || world._FurnitureLootManager is not { } loot || configs == null) return null;

        var today = time.GetDay();
        var lines = new List<string> { $"day {today}, settings: {_settings.Describe()}" };
        var known = Locations.Known.Select(l => l.MapId).ToList();
        var others = Enumerable.Range(FirstMapId, LastMapId - FirstMapId + 1).Where(id => !known.Contains(id) && Read(id) != default);

        foreach (var mapId in known.Concat(others))
        {
            var here = new Here(world, configs, mapId, loot._playerSelectId, today, NameOf(mapId));
            var live = Count(here);

            // The game loads only the location the player is at; any other is read from the save.
            if ((live.Containers > 0 ? live : Saved(here)) is { } count)
            {
                lines.Add($"{here.Name}: {RestockRules.Status(mapId, today, count.Containers, count.Opened, Read(mapId), _settings)}");
            }
            else
            {
                lines.Add($"{here.Name}: not visited yet");
            }
        }

        return lines;
    }

    /// <summary>
    /// A location's containers as the save holds them, counted the way <see cref="Containers"/> counts loaded ones.
    /// Each post-disaster location is a chapter of its own, and the save keeps every chapter's agents, written each time
    /// the player leaves one. Null when the save has nothing for it: the player has never been there.
    /// </summary>
    private static (int Containers, int Opened)? Saved(Here here)
    {
        var chapters = GameSingleton.Existing<GameSaveManager>()?.gameSave?.CurSave?.ChapterAgentMap;
        var chapter = here.Configs.Get_Config_MapPoint(here.MapId)?.ChapterID ?? here.MapId;

        if (chapters == null || !chapters.TryGetValue(chapter, out var agents) || agents == null) return null;

        var layout = new Layout(here);
        int containers = 0, opened = 0;

        foreach (var agent in agents)
        {
            if (agent == null || agent.InstanceIdType != InstanceIdType.Furniture || agent.IsBagLocked) continue;
            if (!here.Configs.IsSameFloorScope(here.MapId, agent.MapConfigId)) continue;

            // A bag furniture is created from its own config id, as the game's load does.
            var configId = agent.IsBagFurniture && agent.BagFurnitureConfigId != 0 ? agent.BagFurnitureConfigId : agent.AgentConfigId;

            if (layout.Of(configId) is null) continue;

            containers++;

            if (agent.HasSearched) opened++;
        }

        return (containers, opened);
    }

    private static void Arrive(Here here)
    {
        var containers = Containers(here);
        var opened = containers.Where(c => c.Furniture.HasSearched).ToList();
        var state = Read(here.MapId);
        var arrival = RestockRules.Arrive(here.MapId, new Visit(here.Today, containers.Count, opened.Count, state), _settings);
        var count = $"{opened.Count} of {containers.Count} containers opened";

        // One line per arrival, so the log of any session shows where each location's timer stands.
        if (arrival.Started) _log?.LogInfo($"{here.Name} explored on day {here.Today} ({count}), restocks on day {arrival.DueDay}");
        else if (arrival.Restock) _log?.LogInfo($"{here.Name} on day {here.Today}: {count}, restock was due on day {arrival.DueDay}");
        else if (arrival.DueDay is { } due) _log?.LogInfo($"{here.Name} on day {here.Today}: {count}, restocks on day {due}");
        else _log?.LogInfo($"{here.Name} on day {here.Today}: {count}, the restock timer starts once {RestockRules.NeededToExplore(containers.Count, _settings.ExploredShare)} are opened");

        if (arrival.Restock) Restock(here, opened);

        Write(here.MapId, arrival.Next);
    }

    // ----- for the test cheats (DevCheats) -----

    /// <summary>Where you are now, when it is a post-disaster exploration location.</summary>
    internal static Here? Current() => GameSingleton.Existing<BattleLogicWorld>()?._FurnitureLootManager is { } loot ? Locate(loot) : null;

    internal static bool InGame => GameSingleton.Existing<BattleLogicWorld>()?._GameTimeManager != null;

    internal static int Today => GameSingleton.Existing<BattleLogicWorld>()?._GameTimeManager?.GetDay() ?? 0;

    internal static RestockSettings Settings => _settings;

    /// <summary>This location's containers: how many there are and how many are opened.</summary>
    internal static (int Containers, int Opened) Count(Here here)
    {
        var containers = Containers(here);

        return (containers.Count, containers.Count(c => c.Furniture.HasSearched));
    }

    /// <summary>Restocks the opened containers here now, whatever the timer says, and waits to be explored again as a restock does.</summary>
    internal static int RestockNow(Here here)
    {
        var opened = Containers(here).Where(c => c.Furniture.HasSearched).ToList();

        if (opened.Count == 0) return 0;

        Restock(here, opened);
        Write(here.MapId, new LocationState(null, here.Today, here.Today));

        return opened.Count;
    }

    internal static LocationState Read(int mapId)
    {
        var start = LocationState.Decode(GameCounter.GetCounter(StartKey(mapId)));
        var visit = LocationState.Decode(GameCounter.GetCounter(VisitKey(mapId)));
        var restocked = LocationState.Decode(GameCounter.GetCounter(RestockedKey(mapId)));

        // A save from 0.1.2 or earlier holds the due day instead of the start day.
        if (start == null && LocationState.Decode(GameCounter.GetCounter(OldDueKey(mapId))) is { } oldDue)
        {
            start = RestockRules.StartOfOldTimer(mapId, oldDue, visit, _settings);
        }

        return new LocationState(start, visit, restocked);
    }

    internal static void Write(int mapId, LocationState state)
    {
        GameCounter.SetCounter(StartKey(mapId), LocationState.Encode(state.StartDay));
        GameCounter.SetCounter(VisitKey(mapId), LocationState.Encode(state.LastVisitDay));
        GameCounter.SetCounter(RestockedKey(mapId), LocationState.Encode(state.LastRestockDay));

        if (GameCounter.GetCounter(OldDueKey(mapId)) != 0) GameCounter.SetCounter(OldDueKey(mapId), 0);
    }

    internal static string NameOf(int mapId) => Locations.Known.FirstOrDefault(l => l.MapId == mapId)?.English ?? $"location {mapId}";

    private static void Restock(Here here, List<Container> opened)
    {
        var discoverLv = ExploreDiscoveryTools.GetDiscoveryLv(here.MapId);
        int random = 0, withFixed = 0, fixedNone = 0;

        foreach (var container in opened)
        {
            var added = false;

            if (container.Group > 0)
            {
                FurnitureLootTools.FillRandomLoot(container.Furniture, container.Group, container.Count, false, null, discoverLv, 0);
                added = true;
                random++;
            }

            if (container.Fixed.Count > 0)
            {
                var back = FixedRefill.Roll(container.Fixed, _settings.FixedContainerPercent, _settings.FixedItemPercent, Dice.NextDouble);

                if (AddFixed(here, container.Furniture, back) > 0)
                {
                    added = true;
                    withFixed++;
                }
                else
                {
                    fixedNone++;
                }
            }

            // A container that got nothing back stays opened, so it does not look full when it is empty.
            if (added) container.Furniture.HasSearched = false;
        }

        _log?.LogInfo($"{here.Name} restocked on day {here.Today}: {opened.Count} opened containers, {random} got random loot, {withFixed} got fixed items back, {fixedNone} with fixed items got none this time");
    }

    /// <summary>
    /// Puts fixed items into a container the way the game's own map layout fill does: in stacks no bigger than the
    /// item's stack limit, fresh from this hour. Returns how many items went in.
    /// </summary>
    private static int AddFixed(Here here, Furniture furniture, List<ItemStack> items)
    {
        if (here.World._ItemManager is not { } manager) return 0;

        var hour = here.World._GameTimeManager?.GetHour() ?? 0;
        var added = 0;

        foreach (var item in items)
        {
            if (here.Configs.Get_Config_Item(item.ItemId) is not { } config) continue;

            var limit = config.StackLimit > 0 ? config.StackLimit : 1;

            for (var left = item.Count; left > 0;)
            {
                var count = Math.Min(left, limit);

                if (!manager.TryAddItemToBag(furniture.InstanceId, item.ItemId, count, hour, 24 * config.Life, 0, 1f, null, 0, null, null, 0, 0, false, false, false, false, 0)) break;

                added += count;
                left -= count;
            }
        }

        return added;
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

    /// <summary>
    /// Every container the game fills here: unlocked, with a random loot group for your character or a fixed list of
    /// items from the map layout that can come back. Upper floors count as their building.
    /// </summary>
    private static List<Container> Containers(Here here)
    {
        var found = new List<Container>();
        var furnitures = here.World._AgentManager?.GetAllFurnitures();

        if (furnitures == null) return found;

        var layout = new Layout(here);

        foreach (var furniture in furnitures)
        {
            if (furniture == null || furniture.IsBagLocked) continue;
            if (!here.Configs.IsSameFloorScope(here.MapId, furniture.MapConfigId)) continue;
            if (layout.Of(furniture.AgentConfigId) is not { } kind) continue;

            found.Add(new Container(furniture, kind.Group, kind.Count, kind.Fixed));
        }

        return found;
    }

    private static string StartKey(int mapId) => $"lootrestock.start.{mapId}";

    private static string OldDueKey(int mapId) => $"lootrestock.due.{mapId}";

    private static string VisitKey(int mapId) => $"lootrestock.visit.{mapId}";

    private static string RestockedKey(int mapId) => $"lootrestock.restocked.{mapId}";

    private static void Report(string operation, Exception ex)
    {
        if (Reported.Add(operation)) _log?.LogError($"{operation} failed, the location keeps the game's own loot (each kind reported once): {ex}");
    }

    internal sealed record Here(BattleLogicWorld World, ConfigManager Configs, int MapId, int Character, int Today, string Name);

    private sealed record Container(Furniture Furniture, int Group, int Count, List<ItemStack> Fixed);

    /// <summary>What the game fills one container with: a random loot group (0 for none) with its count, and fixed items that can come back.</summary>
    private sealed record Kind(int Group, int Count, List<ItemStack> Fixed);

    /// <summary>
    /// A location's map layout, which says what each container is filled with. The layout is per chapter (each
    /// post-disaster location is one), keyed by the furniture's config id, with one list for everyone and one per
    /// character (ItemIdList_P1 to _P4), as AgentManager.LoadFurnitureFromGuidance reads them.
    /// </summary>
    private sealed class Layout
    {
        private readonly Here _here;
        private readonly Dictionary<int, Config_GuidanceFurniture> _entries = new();

        public Layout(Here here)
        {
            _here = here;

            var chapter = here.Configs.Get_Config_MapPoint(here.MapId)?.ChapterID ?? here.MapId;
            var entries = here.Configs.GetGuidanceFurnitureByChapterId(chapter);

            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry == null || (entry.PlayerSelectId != 0 && entry.PlayerSelectId != here.Character)) continue;

                _entries.TryAdd(entry.FurnitureId, entry);
            }
        }

        /// <summary>What a furniture with this config id is filled with, or null when it is not a container the mod restocks.</summary>
        public Kind? Of(int configId)
        {
            if (_here.Configs.Get_Config_Furniture(configId) is not { } config) return null;
            if (config.FurnitureFunc?.Contains(NoWaveFunc) == true) return null;

            var group = FurnitureLootManager.PickLootGroup(config, _here.Character);
            var items = _entries.TryGetValue(configId, out var entry) ? FixedItems(entry) : new List<ItemStack>();

            return group > 0 || items.Count > 0 ? new Kind(group, config.LootRandomCount, items) : null;
        }

        private List<ItemStack> FixedItems(Config_GuidanceFurniture entry)
        {
            var items = new List<ItemStack>();

            Collect(items, entry.ItemIdList, entry.ItemCountList);

            switch (_here.Character)
            {
                case 1: Collect(items, entry.ItemIdList_P1, entry.ItemCountList_P1); break;
                case 2: Collect(items, entry.ItemIdList_P2, entry.ItemCountList_P2); break;
                case 3: Collect(items, entry.ItemIdList_P3, entry.ItemCountList_P3); break;
                case 4: Collect(items, entry.ItemIdList_P4, entry.ItemCountList_P4); break;
            }

            return items;
        }

        /// <summary>The game skips a list whose ids and counts differ in length, so this does too.</summary>
        private void Collect(List<ItemStack> items, Il2CppSystem.Collections.Generic.List<int>? ids, Il2CppSystem.Collections.Generic.List<int>? counts)
        {
            if (ids == null || counts == null || ids.Count != counts.Count) return;

            for (var i = 0; i < ids.Count; i++)
            {
                if (counts[i] <= 0 || _here.Configs.Get_Config_Item(ids[i]) is not { } item) continue;
                if (FixedRefill.ComesBack(item.Category, item.Story)) items.Add(new ItemStack(ids[i], counts[i]));
            }
        }
    }
}
