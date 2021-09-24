# Chrono Ark Gameplay Plugins :baguette_bread: :watch: :hedgehog:

### [**한국어 번역 Korean Translation**](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/blob/master/KoreanREADME.md)
Thanks to [windypanda1](https://github.com/fwqefwqef) Korean for translation!

Repo for various [Chrono Ark](https://store.steampowered.com/app/1188930/Chrono_Ark/) plugins modifying course of the game. 
Modifications can include new game modes, new or altered items or cards, quality of life improvements and so on.

Contributions are welcome and feed back is greatly appreciated!

Made using  [Harmony](https://github.com/pardeike/Harmony) and [BepInEx](https://github.com/BepInEx/BepInEx)

---
Currently only a handful of plugins are available but hopefully more will be added in the future.
## Installation

Step 1. Download and set-up BepInEx. Refer to [official instructions](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_mono.html).

Step 2. [Download mods](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases) and put selected .dll files to `BepInEx/plugins` folder.

### *OR*

Download with BepInEx included. 64-bit version [[here]](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases/download/1.1.1/allplugins_BepInEx_x64_included-13-09-2021.zip), 32-bit version [[here]](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases/download/1.1.1/allplugins_BepInEx_x86_included-13-09-2021.zip). \
Extract to `<Chrono Ark dir>\x64\Master` for 64-bit version or `<Chrono Ark dir>\x86\Master` for 32-bit (BepInEx folder should be in the same directory where ChronoArk.exe is).

### Step-by-step guide:
1. Download [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.15). Make sure to download correct version (32-bit or 64-bit)
2. Locate local Chrono Ark files. Probably at `C:\Program Files (x86)\Steam\steamapps\common\Chrono Ark\` Otherwise, go to Steam > Library > Chrono Ark > Manage(:gear: icon) > Properties > Local files > Browse
3. Extract BepInEx in `<Chrono Ark dir>\x64\Master` if using 64-bit version or `<Chrono Ark dir>\x86\Master` if using 32-bit
4. Launch Chrono Ark (from Steam) and BeInEx will generate relevant files including `BepInEx\plugins` folder. Close the game.
5. [Download plugins](https://github.com/Neoshrimp/ChronoArk-gameplay-plugins/releases/download/1.1.1/CAv1.62_allplugins-13-09-2021.zip). Place extracted dlls into plugins folder.
6. (OPTIONAL) Backup save data. Unnecessary as none of the mods affect it directly or advance meta/story progression for free. If still desired refer to [this](https://steamcommunity.com/app/1188930/discussions/1/4917340730760337347/).
7. Enjoy!

Some plugins like *More cursed battles* will generate .cfg files located at `<dir with ChronoArk.exe>\BepInEx\config` after being launched for the first time. Close game and modify .cfg file (txt editor will do) to configure plugin.

## Plugin List

* ### More cursed battles :feelsgood:
  Adds more cursed battles to every stage. Amount of gold rewarded by killing cursed enemies is reduced. Receive 2 identified lifting scrolls at the start. Cursed battles in Sanctuary work properly now and optionally offer better rewards. All these values are configurable in generated .cfg file. Works independent of the game difficulty.
* ### Lift Curse card placement :scroll:
  Lift Curse cards received during cursed battles are placed at the bottom of the hand instead of top. Makes Azar's passive and some Helia's cards a bit better on early turns.
* ### Better Pain Equals Happiness! :carrot:
  Buffs Huz's Pain Equals Happiness red card. Extends Transit Pain buff duration by one turn and adds additional healing when receiving pain damage from allies.
* ### Alternative Shadow Curtain :dagger:
  Trisha's Shadow Curtain can only be used Once but can be set as a fixed skill. No longer busted when used with mirror images but better compared to 1.56 shadow curtain.
* ### Elemental trio debuff persistence removal :fire:
  Sanctuary elemental trio debuffs no longer remain after battle.
  
  

## Uninstallation
Delete .dll files from `plugin` folder and optionally plugin configuration files from `config` folder.
