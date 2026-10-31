# Loot Restock settings

Settings live in `BepInEx\config\com.dev.survivallog.lootrestock.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. Delete the file to get the defaults back. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## General

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | When `true`, exploration locations restock the containers you opened, a few days after you explored them. When `false`, nothing restocks. |
| `ExploredShare` | `0.5` | How many of a location's containers you must open before it counts as explored and its restock timer starts. `0.5` means half, `1` means all of them. Any value from `0.05` to `1`. |

## Days

Each setting is the number of days after you explore a location until its containers restock. Write a range such as `3-5`, or one number such as `4`. A new number in the range is picked each time. Any value from `1` to `365`. A value the mod cannot read falls back to the default, with a line in `BepInEx\LogOutput.log`.

| Setting | Default | Location |
| --- | --- | --- |
| `HardwareStore` | `3-5` | Hardware Store |
| `DesertedSupermarket` | `4-7` | Deserted Supermarket |
| `AbandonedSchool` | `5-8` | Abandoned School |
| `AbandonedHospital` | `5-8` | Abandoned Hospital |
| `TechCompany` | `6-9` | Tech Company |
| `OtherLocations` | `5-8` | Any exploration location not listed above, such as one a game update adds |

A change also applies to a location whose timer is already counting.
