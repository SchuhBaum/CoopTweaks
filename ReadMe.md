## CoopTweaks
###### Version: 0.1.2

This is a mod for Rain World v1.9.

### Description
Includes various tweaks:
- (Artificer Stun) Artificer's parry does not stun players. But it does knock them back even when JollyCoop's friendly fire setting is turned off.
- (Deaf Beep) Mutes the tinnitus beep when near explosions.
- (Item Blinking) Nearby items only blink even you can pick them up.
- (Release Grasp) Other slugcats stop grabbing you when you press jump.
- (Region Gates) Region gates don't wait for players to stand still.
- (Slow Motion) Removes or reduces the slow motion effect in most situations. In addition, the mushroom effect is shared with other players.
- (Slugcat Collision) Slugcats don't collide with each other.
- (SlugOnBack) You can only drop slugcats from your back when holding down and grab.  
  
This mod is a port of the tweaks from the mod JollyCoopFixesAndStuff for Rain World v1.5.

### Installation
0. Update Rain World to version 1.9 if needed.
1. Download the file  `CoopTweaks.zip` from [Releases](https://github.com/SchuhBaum/CoopTweaks/releases/tag/v0.1.2).
2. Extract its content in the folder `[Steam]\SteamApps\common\Rain World\RainWorld_Data\StreamingAssets\mods`.
3. Start the game as normal. In the main menu select `Remix` and enable the mod. 

### Bug reports & FAQ
See the corresponding sections on the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2928752589) for the mod `SBCameraScroll`.

### Contact
If you have feedback, you can message me on Discord `@SchuhBaum#7246` or write an email to SchuhBaum71@gmail.com.

### License
There are two licenses available - MIT and Unlicense. You can choose which one you want to use. 

### Changelog
v0.1.2:
- (Item Blinking) Fixed a bug where spearmaster could not put spears directly to its back.
- Restored compatibility with SBCameraScroll.
- IL hooks should no longer be logged multiple times when other mods add these IL hooks as well.
- Option specific hooks are no longer initialized every cycle. Instead they are initialized when starting the game or changing the options.

v0.1.0:
- Initial release.
- (SlugOnBack) Removed the ability to directly throw slugcats from back.
- (Slow Motion) Included the ReducedSlowMotion mod since it is a tweak from JollyCoopFixesAndStuff as well.
- (Slow Motion) Updated description.
- (Slow Motion) This option was not visible in the Remix options menu.
- (Deaf Beep) This option was not visible in the Remix options menu.
- Fixed a bug where slugcat npcs would crash the game.
- (Slugcat Collision) Fixed a bug when one of the slugcats would be removed from the room before the collision checks were done.
- (Slow Motion) Fixed a bug where the mushroom counter was not shared when using spearmaster.
- (Artificer Stun) When enabled, ignores JollyCoop's friendly fire setting. Otherwise the knock-back might be ignored as well.