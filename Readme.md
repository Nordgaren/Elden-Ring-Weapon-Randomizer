# Elden-Ring-Weapon-Randomizer
Weapon Randomizer for Elden Ring

[![GitHub all releases](https://img.shields.io/github/downloads/Nordgaren/Elden-Ring-Weapon-Randomizer/total)](https://github.com/Nordgaren/Elden-Ring-Weapon-Randomizer/releases/latest)

# WARNING  
Backup your saves before using this tool, and restore the backups before going online.  

## Requirements 
* [.NET 5 Desktop Runtime x64](https://download.visualstudio.microsoft.com/download/pr/b1902c77-e022-4b3e-a01a-e8830df936ff/09d0957435bf8c37eae11b4962d4221b/windowsdesktop-runtime-5.0.15-win-x64.exe)  
* [.NET 4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=49981)


## How to use
Elden Ring has anticheat, so you will need to disable it using your preferred method. This mod also edits memory that could be recorded by
the games internal anticheat. I would recommend you backup your saves
and restore them before going back online. If you're paranoid like I am,
block the game from accessing the internet, too. :)

The randomizer works on the params of your currently held weapons, so make sure you have ALL different weapons with no ash equipped in
each slot you want to randomize before starting the randomizer (If you have empty hands, you'll get a random ash between the two weapons that were
randomized, because both will be applied to the fist param).

Start the game, then start "Elden Ring Weapon Randomizer.exe" and choose your settings. Make sure you follow the above directions, check "Randomize", and you will be good to go!

This randomizer will restore your weapon params when you stop it, and it will
restore all weapon params to the state they were in when the app launched, once you close out.

Format for ERItemCategories.txt `Multiplier Path/To/WeaponList.txt Infusible`. The multiplier is how many times this list will be put into the big list of randomized weapons. The path is the path to the text file with the list of `ID Name` formatted weapons. Infusible is `true` or `false`. Use `true` for melee weapons (Unique weapons get passed on for infusion no matter what) and `false` for items like bows and casting tools.  

## Disable EAC

Make a text file called `steam_appid.txt` in your Game directory with the number `1245620` inside. Launch the game from the .exe, and you won't have EAC running anymore.

## Backup Saves
This path `%appdata%/EldenRing` should have your steam ID in it. Select the folder with the right steam ID and backup the `ER0000.sl2` in there. Note: The games backup is not sufficient. It will be contstantly overwritten by the game. Make it something different, like `ER0000.sl2.clean`

## Thank You  
**[TKGP](https://github.com/JKAnderson/)** Author of [DS Gadget](https://github.com/JKAnderson/DS-Gadget) and [Property Hook](https://github.com/JKAnderson/PropertyHook)  
**Pav** Author of the CE table used for most the the randomizer  

## Libraries
My fork of [Property Hook](https://github.com/Nordgaren/PropertyHook) by [TKGP](https://github.com/JKAnderson/)  

## Ko-fi
[If you wish to support my mod making habit](https://ko-fi.com/nordgaren)

# Change Log  
### Version 1  

* Changed format for categories. The format is now: `Multiplier Path/To/WeaponList.txt Infusible`. The multiplier 

### Version 1  

* Release
