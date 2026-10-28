# Loot Restock

The containers you opened at exploration locations restock a few days after you explored them.

In Survival Log, each location is stocked once, and survival point cards add to it later. Once you have searched a place, it stays empty. Loot Restock brings fresh loot back to the places you keep exploring.

## Features

- **Restocks the containers you opened**: when a location restocks, every container you opened since the last restock gets a new round of loot, on top of anything still inside. Containers you never opened are left alone.
- **Explore to start the timer**: a location counts as explored once you have opened half its containers. Its timer starts on the day you explored it.
- **Its own timer for each location**: small places like the Gas Station restock in 3-5 days, big ones like the Tech Company in 6-9. You can change each range.
- **No piling up**: after a restock, a location waits until you explore it again. A place you skip or only half search never restocks twice.
- **Same loot as the game**: a restock uses the same loot table the game uses for that container and your character, at the location's current discovery level from survival point cards.
- **Test buttons**: with Cheat Menu installed, its Mod tests tab shows this location's restock state, restocks it right away, makes its restock due today, or resets every timer.

Restocks happen when you arrive at a location, not overnight. Restocked containers show as unsearched again. Only the six post-disaster exploration locations restock: Hardware Store, Gas Station, Deserted Supermarket, Abandoned School, Abandoned Hospital and Tech Company. Your home, camps and pre-disaster locations are not affected.

Loot Restock keeps a few numbers in your save (each location's restock day). It does not edit game files. You can remove it at any time; loot it already added stays.

## Requirements

- Survival Log 1.1.19611. Built for this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Optional, to change settings without a restart: install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `LootRestock-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-LootRestock/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/91).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\LootRestock.dll`

## Uninstall

Delete `LootRestock.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.lootrestock.cfg`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.lootrestock.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Loot Restock 0.1.0 started`, followed by `hooked the location loot refresh`. A line starting with `could not hook` means the mod does not work on this game version, and locations work as in the unmodded game. Each time a location is explored or restocked, the log says so, with the day.

## Known issues

- A location you had already half searched before installing the mod starts its timer on the day you next arrive there.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/LootRestock -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\LootRestock\bin\Release\LootRestock.dll`.

The unit tests cover the restock rules and need nothing from the game:

```
dotnet test tests/LootRestock.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
