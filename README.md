# Spireside Together

Spireside Together is a Slay the Spire 2 mod that enables public multiplayer with strangers by enabling hosts to create public lobbies and guests to search with a server browser.

## Features
This mod adds a server browser that can search up to 200 public lobbies with filtering by hostname, description, or game version.

Hosts can create public lobbies from the Spireside Together Multiplayer menu and copy their lobby ids.

Guests can join directly by lobby ids or connect to lobbies found in the server browser.

## Dependencies
This mod depends on the following for chat support:

- [lemonSpire2](https://www.nexusmods.com/slaythespire2/mods/29?tab=description) >= v0.7.4
- [RitsuLib](https://www.nexusmods.com/slaythespire2/mods/137?tab=description)

## Compatability
You must be running v0.105.0 or higher for STS2 to run lemonSpire2. You cannot join lobbies with conflicting game versions.

Currently behavior with mismatched mods between players is untested and can likely break compatibility.

## Installation
### Dependencies
This mod uses lemonspire2 for chat. Download it and its dependency, RitsuLib either from github (reccomended) or nexusmods and extract them into the mods folder at your STS2 install location: `*\steamapps\common\Slay the Spire 2\mods`

**lemonspire2:** [github](https://github.com/freude916/lemonSpire2/releases/tag/v0.7.4)
[nexusmods](https://www.nexusmods.com/slaythespire2/mods/29?tab=description)

**RitsuLib:**
[github](https://github.com/BAKAOLC/STS2-RitsuLib/releases/tag/v0.2.40)
[nexusmods](https://www.nexusmods.com/slaythespire2/mods/137?tab=description)

### Download spireside-together
To install spireside-together, download the zip folder in the [latest release](https://github.com/omgyukiel/spireside-together/releases) and extract it to your mods folder in your `*\steamapps\common\Slay the Spire 2\mods`

---
## Development Requirements

- Slay the Spire 2 installed through Steam.
- A working Slay the Spire 2 modding setup with BaseLib.
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
