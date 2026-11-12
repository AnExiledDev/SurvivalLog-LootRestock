# Changelog

## 0.2.4

- Clearer wording in the settings, the Mod Manager description and the Cheat Menu test buttons. The "Restock due today" button is now called "Make restock due today".
- Made for Survival Log 1.1.20719.

## 0.2.3

- A container with both set items and random loot now gets only random loot at a restock. Before, it could also get its set items back.
- Made for Survival Log 1.1.20719.

## 0.2.2

- The Mod Manager no longer says a restart is needed after you change a setting. Every setting already applied right away.
- Made for Survival Log 1.1.20719.

## 0.2.1

- Now uses shared code from Exiled's Toolkit, so it needs Exiled's Toolkit 0.19.0 or newer.
- Made for Survival Log 1.1.20719.

## 0.2.0

- Each world can keep its own `Enabled`, `ExploredShare`, `FixedContainerPercent` and `FixedItemPercent` settings. With Exiled's Toolkit 0.17.0 or newer, you can pick them for a new world in Customizable Difficulty, or change them for the world you're playing on the Mod Manager's Mods screen. The Days settings are the same for every world.
- Made for Survival Log 1.1.20151.

## 0.1.7

- Fixed: the old recorder in the Abandoned Hospital could come back at a restock. Items with no price are one-of-a-kind finds like the recorder, and they never come back now.
- Fixed: containers that got nothing back at a restock counted toward exploring the location again, so a barely searched Tech Company could restock twice. Only the containers that can refill count now, and a location where nothing refilled never restarts its timer.
- A container with both set items and random loot gets a round of random loot, and its set items come back by chance.
- The location list in the log shows the day each explored location's timer started, its restock day, and how many containers got nothing back at the last restock.
- A container only gets back the set items the game placed in it at that location, so a game update can't mix in items from somewhere else.

## 0.1.6

- Fixed: containers that always hold the same items never restocked, so the Tech Company, where almost every container is like that, never restocked at all. They now get their items back, except keys, notes, story items and other one-time items.
- New settings `FixedContainerPercent` and `FixedItemPercent` (both 70 by default) set the chance a container with set items gets anything back, and the chance each single item in it comes back. A refill is often smaller than the first fill, and some containers stay empty.
- A container that gets nothing back at a restock stays searched.
- The location list in the log now says when a location is explored and its timer will start on your next visit. Before, it asked you to open containers you had already opened.

## 0.1.5

- Fixed: the location list in the log showed 0 of 0 containers for every location you weren't at. It now reads the counts from the save, and a location you have never visited shows as not visited yet.

## 0.1.4

- When a save loads, `BepInEx\LogOutput.log` lists every post-disaster location, even the ones you aren't at, with how many containers are opened, the restock day and the day it last restocked.
- The log shows the settings in use when the game starts and after each change.
- With Cheat Menu installed, a new Mod tests button shows the same list in game.

## 0.1.3

- Fixed: changing a location's days did nothing until its next restock. The new number of days now applies to a timer that is already counting.
- Each time you arrive at a post-disaster location, `BepInEx\LogOutput.log` says how many of its containers are opened and on which day it restocks.

## 0.1.2

- The Days settings list only the locations you can visit.

## 0.1.1

- With Cheat Menu installed, its Mod tests tab has Loot Restock buttons to show where this location's timer stands, restock it right away, make its restock due today and reset every timer.

## 0.1.0

- First release.
- The post-disaster exploration locations restock the containers you opened, a few days after you explored them.
- A location counts as explored once you have opened half its containers, and you can change how many that takes. It restocks only once each time you explore it, so loot never piles up in places you skip.
- Each location has its own range of days in the config, so restocks come as often or as rarely as you like.
