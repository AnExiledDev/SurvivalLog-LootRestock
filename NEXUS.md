Name: Loot Restock
Summary: The containers you opened at exploration locations get fresh loot a few days after you explore them, from the same loot tables the game uses. You set how many days, and every location has its own range in the config, such as 3-5. A place only restocks again after you explore it again, so loot never piles up in places you skip or barely search.

Description (BBCode):
[center][img]https://sldb.core-directive.com/media/save/2-changes-stay.png[/img][/center]

Each location in Survival Log is stocked once, and survival point cards add more loot later. Once you have searched a place, it stays empty. Loot Restock brings fresh loot back to the places you keep exploring.

[size=5]Main features[/size]
[list]
[*][b]Restocks the containers you opened[/b]: when a location restocks, every container you opened since the last restock gets a new round of loot, on top of anything still inside. Containers you never opened are left alone.[/*]
[*][b]Explore to start the timer[/b]: a location counts as explored once you have opened half its containers, and you can change how many that takes. Its timer starts on the day you explored it.[/*]
[*][b]You pick how often[/b]: each location has its own range of days in the config, so restocks come as fast or as slow as you want. Set a range such as 3-5, or one number such as 2.[/*]
[*][b]No piling up[/b]: after a restock, a location waits until you explore it again. A place you skip or barely search never restocks twice.[/*]
[*][b]Same loot as the game[/b]: a restock rolls from the same loot table the game uses for that container and your character, at the location's current discovery level from survival point cards.[/*]
[*][b]Set items come back by chance[/b]: some containers hold the same items every time, like most of the Tech Company. They get their items back by chance, and you set both the chance a container refills and the chance each item comes back. A container with both set items and random loot gets only random loot. Keys, notes, story items and other one-time items never come back. A container that gets nothing back stays searched, and it doesn't count toward exploring the location again.[/*]
[/list]
A location restocks when you arrive there or move between its floors. Restocked containers show as unsearched again. Only the post-disaster exploration locations restock: Hardware Store, Deserted Supermarket, Abandoned School, Abandoned Hospital and Tech Company. Your home, camps and pre-disaster locations are not affected.

Loot Restock keeps a few numbers in your save (for each location: when its timer started, your last visit, its last restock, and how many containers that restock left empty). It does not edit game files. You can remove it at any time, and any loot it already added stays.

Works in English and Chinese.

[size=5]Known issues[/size]
A location you had already half searched before installing the mod starts its timer on the day you next arrive there.

[center][img]https://sldb.core-directive.com/img/mods-banner.jpg[/img]
[size=4][url=https://www.nexusmods.com/survivallog/mods/73]Get Mod Manager[/url] to set up all your mods in game  |  [url=https://sldb.core-directive.com/en/]Open the Survival Log wiki[/url] for items, recipes and maps[/size][/center]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.20719. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]. Install it first.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.19.0 or newer. Loot Restock does not load without it. It applies settings changed on the Mod Manager's Mods screen without restarting the game, and lets each world keep its own General settings.[/*]
[*]Optional: [url=https://www.nexusmods.com/survivallog/mods/73]Mod Manager[/url]. With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Install Exiled's Toolkit the same way. Download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\LootRestock.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]LootRestock.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder.

[size=5]Configuration[/size]
Change settings in game on the Mod Manager's Mods screen, or edit [font=Courier New]BepInEx\config\com.dev.survivallog.lootrestock.cfg[/font] while the game is closed. [url=https://github.com/AnExiledDev/SurvivalLog-LootRestock/blob/main/CONFIG.md]CONFIG.md[/url] lists every setting. Each location has its own range of days, such as [font=Courier New]3-5[/font], and [font=Courier New]ExploredShare[/font] sets how many of a location's containers you must open before it counts as explored. Each world can keep its own General settings. Pick them for a new world in Customizable Difficulty, or change them on the Mods screen while you play, and only that world changes.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord. You can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-LootRestock]github.com/AnExiledDev/SurvivalLog-LootRestock[/url]

[line]
[size=5]简体中文[/size]

在 Survival Log 里，每个地点的物资只会刷新一次，之后靠生存点卡牌追加。一个地方搜空了，就一直是空的。Loot Restock 会给你常去的地点补上新物资。

[size=5]主要功能[/size]
[list]
[*][b]给打开过的柜子补货[/b]：地点补货时，自上次补货以来你打开过的每个柜子都会再多一轮物资，叠加在柜子里剩下的东西上。没打开过的柜子不会变。[/*]
[*][b]探索后开始计时[/b]：一个地点打开过一半的柜子，就算探索过了，这个比例可以自己设置。从探索的那天开始计算补货天数。[/*]
[*][b]补货快慢由你决定[/b]：每个地点在配置里都有自己的天数范围，想多快或多慢补货都可以。可以写一个范围，比如 3-5，也可以写一个数字，比如 2。[/*]
[*][b]不会越堆越多[/b]：补货之后，地点要等你再次探索才会再补。你不去或只搜了一点的地方，不会补第二次。[/*]
[*][b]和游戏一样的物资[/b]：补货用的是游戏给这个柜子和你的角色用的同一张物资表，按这个地点当前的发现等级（来自生存点卡牌）刷新。[/*]
[*][b]固定物品按几率补回[/b]：有些柜子每次都放同样的物品，科技公司大多如此。这些柜子会按几率补回物品，一个柜子能不能补回、每件物品能不能回来，这两个几率都由你设置。既有固定物品又有随机物资的柜子，只补随机物资。钥匙、笔记、剧情物品等一次性物品不会回来。什么都没补回的柜子仍算已搜索，再次探索这个地点时不算在内。[/*]
[/list]
补货在你到达地点或在地点里换楼层时发生。补过货的柜子会重新显示为未搜索。只有灾后探索地点会补货：社区检修小屋、残破超市、废弃学校、废弃医院和科技公司。家、营地和灾前地点不受影响。

Loot Restock 会在存档里记下几个数字（每个地点的计时开始日、你上次到访、上次补货，以及那次补货有几个柜子没补回东西）。不会修改游戏文件，随时可以移除，已经补进去的物资会留下。

支持英文和中文。

[size=5]已知问题[/size]
安装模组之前已经搜过一半的地点，会从你下次到达那天开始计时。

[url=https://www.nexusmods.com/survivallog/mods/73]获取 Mod Manager[/url]，在游戏里设置所有模组。[url=https://sldb.core-directive.com/zh/]打开 Survival Log 维基[/url]，物品、配方、地图一查就有。

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.20719。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]，请先安装。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.19.0 或更新版本。没有它本模组不会加载。它让在 Mod Manager 的「模组」界面修改的设置无需重启游戏即可生效，还让每个存档可以有自己的 General 设置。[/*]
[*]可选：[url=https://www.nexusmods.com/survivallog/mods/73]Mod Manager[/url]。装了它，可以在游戏里它的「模组」界面修改每项设置，并看到每项的说明，不用打开配置文件。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]用同样的方式安装 Exiled's Toolkit，下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\LootRestock.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]LootRestock.dll[/font]。

[size=5]配置[/size]
可以在游戏内 Mod Manager 的「模组」界面修改，也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.lootrestock.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-LootRestock/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。每个地点有自己的天数范围，比如 [font=Courier New]3-5[/font]。[font=Courier New]ExploredShare[/font] 设置一个地点要打开多少柜子才算探索过。每个存档都可以有自己的 General 设置。新存档可以在「自定义难度」里选，玩的时候也可以在「模组」界面修改，只会改变当前存档。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-LootRestock]github.com/AnExiledDev/SurvivalLog-LootRestock[/url]
