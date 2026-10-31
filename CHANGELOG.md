# Changelog

## 0.1.7

- Fixed: the old recorder in the Abandoned Hospital could come back at a restock. Items with no price, which are one of a kind, never come back now.
- Fixed: containers that got nothing back at a restock counted toward exploring the location again, so a barely searched Tech Company could restock twice. Only the containers that can refill count now, and a location where nothing refilled never restarts its timer.
- A container that has both set items and a random loot table gets a round of random loot and its set items by chance.
- The location list in the log shows the day an explored location's timer counts from and its restock day, and how many containers got nothing back at the last restock.
- Only the map layout rows the game itself uses for a location are read, so a game update that reuses a furniture number elsewhere cannot give a container the wrong items.

## 0.1.6

- Fixed: containers the game fills with set items instead of random loot never restocked, so the Tech Company, which has almost only these, never restocked at all. They now get their items back, minus keys, notes, story items and other one-time items.
- Two new settings, `FixedContainerPercent` and `FixedItemPercent` (both 70 by default): the chance each such container gets anything back, and the chance each single item in it comes back. A refill is often smaller than the first fill, and some containers stay empty.
- A container that gets nothing back at a restock stays searched.
- The location list in the log says when a location is explored and its timer starts on the next visit, instead of asking you to open containers you already opened.

## 0.1.5

- Fixed: the location list in the log showed 0 of 0 containers for every location you weren't at. It now reads them from the save, and a location you have never visited says so.

## 0.1.4

- When a save loads, `BepInEx\LogOutput.log` lists every post-disaster location, even ones you are far from: containers opened, the restock day, and the day it last restocked.
- The log shows the settings in use when the game starts and after each change.
- With Cheat Menu installed, a new Mod tests button shows the same list in game.

## 0.1.3

- Fixed: changing a location's days did nothing until its next restock. The new number of days now applies to a timer that is already counting.
- Each time you arrive at a post-disaster location, `BepInEx\LogOutput.log` says how many of its containers are opened and on which day it restocks.

## 0.1.2

- The Days settings list only the locations you can visit.

## 0.1.1

- With Cheat Menu installed, its Mod tests tab has Loot Restock buttons: show this location's restock state, restock here now, make the restock due today, and reset all timers.

## 0.1.0

- First release.
- The post-disaster exploration locations restock the containers you opened, a few days after you explored them.
- A location counts as explored once you have opened half its containers. It restocks only once per exploration, so loot never piles up in places you skip.
- Each location has its own range of days in the config, so restocks come as often or as rarely as you like.
