# Elden-Ring-Weapon-Randomizer
Weapon Randomizer for Elden Ring

## How to use

Elden Ring has anticheat, so you will need to disable it using your preferred method. This mod also edits memory that could be recorded by the games internal anticheat. I would recommend you backup your saves and restore them before going back online. If you're paranoid like I am, block the game from accessing the internet, too. :)

The randomizer works on the params of your currently held weapons, so make sure you have two different weapons in hand before starting the randomizer (If you have empty hands, you'll get a random ash between the two weapons that were randomized, because both will be applied to the fist param). This randomizer will retore your weapon params when you stop it, and it will restore all weapon params once you close out. 

## Disable EAC

Make a text file called `steam_appid.txt` in your Game directory with the number `1245620` inside. Launch the game from the .exe, and you won't have EAC running anymore.

## Backup Saves
This path `%appdata%/EldenRing` should have your steam ID in it. Select the folder with the right steam ID and backup the `ER0000.sl2` in there. Note: The games backup is not sufficient. It will be contstantly overwritten by the game. Make it something different, like `ER0000.sl2.clean`

## Thank You  
**[TKGP](https://github.com/JKAnderson/)** Author of [DS Gadget](https://github.com/JKAnderson/DS-Gadget) and [Property Hook](https://github.com/JKAnderson/PropertyHook)  
**[Pav]** Author of the CE table used for most the the randomizer  

## Libraries
My fork of [Property Hook](https://github.com/Nordgaren/PropertyHook) by [TKGP](https://github.com/JKAnderson/)  
