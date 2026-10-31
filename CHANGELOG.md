# Changelog

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
