Name: Loot Restock
Summary: Exploration locations restock the containers you opened, a few days after you explored them, with the same loot the game uses. Each location has its own timer, from 3-5 days for the Gas Station to 6-9 for the Tech Company. A place restocks once per exploration, so loot never piles up where you don't go. Every range is in the config.

Description (BBCode):
[center][img]https://sldb.core-directive.com/media/save/2-changes-stay.png[/img][/center]

Each location in Survival Log is stocked once, and survival point cards add to it later. Once you have searched a place, it stays empty. Loot Restock brings fresh loot back to the places you keep exploring.

[size=5]Main features[/size]
[list]
[*][b]Restocks the containers you opened[/b]: when a location restocks, every container you opened since the last restock gets a new round of loot, on top of anything still inside.[/*]
[*][b]Explore to start the timer[/b]: a location counts as explored once you have opened half its containers. Its timer starts on the day you explored it.[/*]
[*][b]Its own timer for each location[/b]: small places like the Gas Station restock in 3-5 days, big ones like the Tech Company in 6-9. You can change each range.[/*]
[*][b]No piling up[/b]: after a restock, a location waits until you explore it again. A place you skip or only half search never restocks twice.[/*]
[*][b]Same loot as the game[/b]: a restock uses the game's own loot table for that container and your character, at the location's current discovery level.[/*]
[/list]
Restocks happen when you arrive at a location, not overnight. Restocked containers show as unsearched again. Only the six post-disaster exploration locations restock; your home, camps and pre-disaster locations are not affected.

Loot Restock keeps a few numbers in your save (each location's restock day). It does not edit game files. You can remove it at any time; loot it already added stays.

Works in English and Chinese.

[size=5]Known issues[/size]
A location you had already half searched before installing the mod starts its timer on the day you next arrive there.

[center][img]https://sldb.core-directive.com/img/mods-banner.jpg[/img]
[size=4][url=https://www.nexusmods.com/survivallog/mods/73]Get Mod Manager[/url] to set up all your mods in game  |  [url=https://sldb.core-directive.com/en/]Open the Survival Log wiki[/url] for items, recipes and maps[/size][/center]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.20151. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]. Install it first.[/*]
[*]Optional: [url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Optional, to change settings without a restart: install Exiled's Toolkit the same way. Download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\LootRestock.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]LootRestock.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder.

[size=5]Configuration[/size]
To change settings, edit [font=Courier New]BepInEx\config\com.dev.survivallog.lootrestock.cfg[/font] while the game is closed, or do it in game on the Mod Manager's Mods screen (see [url=https://github.com/AnExiledDev/SurvivalLog-LootRestock/blob/main/CONFIG.md]CONFIG.md[/url] for every setting). Each location has its own range of days, such as [font=Courier New]3-5[/font]. [font=Courier New]ExploredShare[/font] sets how much of a location you must open to count as explored. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord; you can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-LootRestock]github.com/AnExiledDev/SurvivalLog-LootRestock[/url]

[line]
[size=5]简体中文[/size]

在 Survival Log 里，每个地点的物资只会刷新一次，之后靠生存点卡牌追加。一个地方搜空了，就一直是空的。Loot Restock 会给你常去的地点补充新物资。

[size=5]主要功能[/size]
[list]
[*][b]给打开过的柜子补货[/b]：地点补货时，自上次补货以来你打开过的每个柜子都会再多一轮物资，叠加在柜子里剩下的东西上。[/*]
[*][b]探索后开始计时[/b]：一个地点打开过一半的柜子，就算探索过了，从那天开始计算补货天数。[/*]
[*][b]每个地点各有天数[/b]：加油站这样的小地方 3-5 天补货，科技公司这样的大地方 6-9 天。每个范围都可以修改。[/*]
[*][b]不会越堆越多[/b]：补货之后，地点要等你再次探索才会再补。你不去或只搜了一点的地方，不会补第二次。[/*]
[*][b]和游戏一样的物资[/b]：补货使用游戏自己为这个柜子和你的角色准备的物资表，按这个地点当前的发现等级刷新。[/*]
[/list]
补货在你到达地点时发生，不是在夜里。补过货的柜子会重新显示为未搜索。只有六个灾后探索地点会补货；家、营地和灾前地点不受影响。

Loot Restock 会在存档里记下几个数字（每个地点的补货日）。不会修改游戏文件，随时可以移除；已经补进去的物资会留下。

支持英文和中文。

[size=5]已知问题[/size]
安装模组之前已经搜过一半的地点，会从你下次到达那天开始计时。

[url=https://www.nexusmods.com/survivallog/mods/73]获取 Mod Manager[/url]，在游戏里设置所有模组。[url=https://sldb.core-directive.com/zh/]打开 Survival Log 维基[/url]，物品、配方、地图一查就有。

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.20151。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]，请先安装。[/*]
[*]可选：[url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.4.0 或更新版本。装了它，在 Mod Manager 的「模组」界面改动的设置无需重启游戏即可生效；没装的话，改动在下次启动游戏时生效。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]可选（改动设置无需重启游戏）：用同样的方式安装 Exiled's Toolkit，下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\LootRestock.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]LootRestock.dll[/font]。

[size=5]配置[/size]
可以在游戏内 Mod Manager 的「模组」界面修改，也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.lootrestock.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-LootRestock/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。每个地点有自己的天数范围，比如 [font=Courier New]3-5[/font]。[font=Courier New]ExploredShare[/font] 设置一个地点要打开多少柜子才算探索过。没装 Exiled's Toolkit 的话，在「模组」界面的改动改为在下次启动游戏时生效。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-LootRestock]github.com/AnExiledDev/SurvivalLog-LootRestock[/url]
