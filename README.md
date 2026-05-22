# Spireside Together

Spireside Together is a Slay the Spire 2 mod that enables public multiplayer with strangers by enabling hosts to create public lobbies and guests to search with a server browser.

For installation instructions, please scroll down to the installation instructions below! Kindly report any issues or post discussion [here](https://github.com/omgyukiel/spireside-together/issues). 

## Features
This mod adds a server browser that can search up to 100 public lobbies with filtering by hostname, description, or game version.

Hosts can create public lobbies from the Spireside Together Multiplayer menu and copy their lobby ids.

Guests can join directly by lobby ids or connect to lobbies found in the server browser.

**Known Issue: Currently there is no support for reconnects when a player disconnects. This may unfortunately hang the game.**

## Dependencies
This mod inheritss from BaseLib aand depends on lemonSpire2 for chat support:
- [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2/releases) >= v3.1.2
- [lemonSpire2](https://github.com/freude916/lemonSpire2/releases/tag/v0.7.4) >= v0.7.4
- [RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib/releases/tag/v0.2.40)
- Slay the Spire 2 >= v0.105.0 (as of 05/21/2026 is on the beta branch)
## Compatability
You must be running v0.105.0 or higher for STS2 to run lemonSpire2. You cannot join lobbies with conflicting base game versions.

**Currently behavior with mismatched mods between players is untested and can likely break compatibility.**

## Installation

### Review the compatability section of this README

### 1. Update your STS2 to use the beta-branach (temporary)
This mod depends on lemonSpire2 so strangers can chat and communicate with eachother. lemonSpire2 requires your base game vesion to be >= v0.105.0.

As of 5/21/2026, this version is only available on the beta branch, when it is officially released this step will be removed.
To update your game:
1. Right click Slay the Spire 2 in your Steam library, click on `properties`
2. In `Game Versions & Betas` click on public-beta
3. Steam will update and patch your game
### 2. Download dependencies
This mod uses lemonSpire2 and RitsuLib for chat, and inherits from BaseLib.

1. Download lemonSpire2, RitsuLib, and BaseLib zip files from github (reccomended) or nexusmods. 
2. Extract the zip folders into the mods folder at your STS2 install location: `*\steamapps\common\Slay the Spire 2\mods`
   3. You can find your install folder by right clicking STS2 in your Steam library, clicking properties, then click "Browse" in "Installed Files."
   4. Create the "mods" folder if it does not exist yet

#### Download Links:
- **BaseLib-StS2** [github](https://github.com/Alchyr/BaseLib-StS2/releases) [nexusmods](https://www.nexusmods.com/slaythespire2/mods/103?tab=description)
- **lemonSpire2:** [github](https://github.com/freude916/lemonSpire2/releases/tag/v0.7.4)
[nexusmods](https://www.nexusmods.com/slaythespire2/mods/29?tab=description)
- **RitsuLib:**
[github](https://github.com/BAKAOLC/STS2-RitsuLib/releases/tag/v0.2.40)
[nexusmods](https://www.nexusmods.com/slaythespire2/mods/137?tab=description)

### 3. Download spireside-together
1. Download the zip folder in the [latest release](https://github.com/omgyukiel/spireside-together/releases)
2. Extract the zip folder to your mods folder in your `*\steamapps\common\Slay the Spire 2\mods`
3. Launch the game and you should see a new menu option!
      4. If this is your first time using mods your game may shutdown on the first launch
### 4. (Optional) Sync vanilla saves with modded
If this is your first time using mods you may think you have corrupted your saves files, but STS2 just points to a different folder for modded play throughs!

You can find mods to sync vanilla gamess like [this](https://www.nexusmods.com/slaythespire2/mods/372) or manually copy your vanilla saves to your modded by doing the following:
1. Open STS2
2. Open the console by pressing the `~` key
3. Type `open saves` and press enter
4. You should see a folder like `C:\...\SlayTheSpire2\steam\{your_id}\modded\profile1\saves`
5. Go up a few paths until you're out of the "modded" folder. You should see three profiles, copy your desired profile into the modded folder to replace the existing one. 

---
## Contributing Requirements

- Slay the Spire 2 installed through Steam.

- .NET SDK compatible with the project target framework.
- Visual Studio, Rider, or another C# editor.

## Development

Create local machine build settings first:

```bash
cp Directory.Build.props.example Directory.Build.props
```

Then edit `Directory.Build.props` for your local paths. `GodotPath` is required for `dotnet publish` because publishing exports the scene assets into `SpiresideTogether.pck`.

Restore and build the project with:

```bash
dotnet restore
dotnet build
```

If Slay the Spire 2 is not installed at the default path, pass the install path explicitly:

```bash
dotnet build /p:Sts2Path="/path/to/Slay the Spire 2"
```

The template build copies the compiled mod DLL and manifest into the game's `mods/SpiresideTogether/` folder when the game path is configured correctly.

To package Godot scene assets into the mod folder, run:

```bash
dotnet publish
```

---
## License

See [LICENSE](LICENSE).

## Disclaimer

This is an experimental mod for an Early Access game. Game updates may break compatibility, and multiplayer behavior may change as Slay the Spire 2 evolves.
