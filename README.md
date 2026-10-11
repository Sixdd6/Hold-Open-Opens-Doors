# Hold Open Opens Doors (No HugsLib)

A RimWorld 1.6 mod. When you toggle **Hold Open** on a door, the door opens or closes at once. Colonists no longer need to walk to it first.

This is a fork of [Hold Open Opens Doors](https://github.com/Schalasoft/Hold-Open-Opens-Doors) by Pope Snowball. This fork does not need HugsLib. It needs only Harmony.

- **Steam Workshop:** https://steamcommunity.com/sharedfiles/filedetails/?id=3817740629
- **Original mod:** https://steamcommunity.com/sharedfiles/filedetails/?id=1545144173

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)

## Settings

Open **Options > Mod Settings > Hold Open Opens Doors**.

| Setting | What it does |
| --- | --- |
| Enable power check | A powered door opens or closes only when it has power. This is on by default. |
| Disable on selected doors | The mod does nothing for the doors you select in the list below. This is off by default. |
| Enable logging | Writes debug messages to the game log. |

The door list holds every door type the mod finds, including doors from other mods. Powered doors are selected by default. The list is used only when **Disable on selected doors** is on.

## Changes from the original

- HugsLib is removed. The mod now uses RimWorld's own mod settings window.
- The mod builds for RimWorld 1.6.
- Door detection no longer creates test instances of each building.
- The mod has its own package ID, `Sixdd6.HoldOpenOpensDoors`.

Settings saved with the original mod do not carry over. The mod uses the defaults on first run.

Do not run this fork and the original mod together. Both patch the same Hold Open button.

## Building

You need the .NET Framework 4.7.2 targeting pack, or the NuGet package this project already references, and MSBuild from Visual Studio 2022 Build Tools.

```
msbuild Source\Hold_Open_Opens_Doors.csproj -restore -p:Configuration=Release
```

The build writes `Assemblies/Hold_Open_Opens_Doors.dll`. The first build downloads the NuGet packages.

To install, copy the repo folder into RimWorld's `Mods` folder. Enable Harmony and this mod.

## Credits

- Pope Snowball (Schalasoft): original mod
- Sixdd6: HugsLib removal and RimWorld 1.6 update
