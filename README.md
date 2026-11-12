# Loot Restock

The containers you opened at exploration locations get fresh loot a few days after you explore them, and you set how many days.

In Survival Log, each location is stocked once, and survival point cards add more loot later. Once you have searched a place, it stays empty. Loot Restock brings fresh loot back to the places you keep exploring.

## Features

- **Restocks the containers you opened**: when a location restocks, every container you opened since the last restock gets a new round of loot, on top of anything still inside. Containers you never opened are left alone.
- **Explore to start the timer**: a location counts as explored once you have opened half its containers, and you can change how many that takes. Its timer starts on the day you explored it.
- **You pick how often**: each location has its own range of days in the config, so restocks come as fast or as slow as you want. Set a range such as 3-5, or one number such as 2.
- **No piling up**: after a restock, a location waits until you explore it again. A place you skip or barely search never restocks twice.
- **Same loot as the game**: a restock rolls from the same loot table the game uses for that container and your character, at the location's current discovery level from survival point cards.
- **Set items come back by chance**: some containers hold the same items every time, like most of the Tech Company. They get their items back by chance, and you set both the chance a container refills and the chance each item comes back. A container with both set items and random loot gets only random loot. Keys, notes, story items and other one-time items never come back. A container that gets nothing back stays searched, and it doesn't count toward exploring the location again.
- **Test buttons**: with Cheat Menu installed, its Mod tests tab has buttons to show where this location's timer stands, list every location, restock this location right away, make its restock due today, or reset every timer.

A location restocks when you arrive there or move between its floors. Restocked containers show as unsearched again. Only the post-disaster exploration locations restock: Hardware Store, Deserted Supermarket, Abandoned School, Abandoned Hospital and Tech Company. Your home, camps and pre-disaster locations are not affected.

Loot Restock keeps a few numbers in your save (for each location: when its timer started, your last visit, its last restock, and how many containers that restock left empty). It does not edit game files. You can remove it at any time, and any loot it already added stays.

## Requirements

- Survival Log 1.1.20719. A game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.19.0 or newer. Loot Restock does not load without it. It applies settings changed on the Mod Manager's Mods screen without restarting the game, and lets each world keep its own General settings.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `LootRestock-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-LootRestock/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/127).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\LootRestock.dll`

## Uninstall

Delete `LootRestock.dll` from the `BepInEx\plugins` folder. To remove its settings too, also delete `BepInEx\config\com.dev.survivallog.lootrestock.cfg`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.lootrestock.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. See [CONFIG.md](CONFIG.md) for every setting. Each world can keep its own General settings. Pick them for a new world in Customizable Difficulty, or change them on the Mods screen while you play, and only that world changes.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Loot Restock 0.2.4 started`, followed by `hooked the location loot refresh` and `hooked the save load`. A line starting with `could not hook` means part of the mod doesn't work on this game version, and the line says what stops working. The `settings:` line shows the settings the mod read, at start and after each change. When a save loads, the log lists every post-disaster location: how many of its containers are opened, its restock day or how many more you need to open to start its timer, and when it last restocked. A location you have never visited shows as `not visited yet`. Each time you arrive at a location, the log adds a line for it.

## Known issues

- A location you had already half searched before installing the mod starts its timer on the day you next arrive there.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log`.

To reach me directly, message `exiled.dev` on Discord. You can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/LootRestock -c Release -p:GameDir="C:\path\to\Survival Log" -p:ToolkitDll="C:\path\to\ExiledToolkit.dll"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\LootRestock\bin\Release\LootRestock.dll`.

The unit tests cover the restock rules and need nothing from the game:

```
dotnet test tests/LootRestock.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
