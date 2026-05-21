# Spireside Together

Spireside Together is a Slay the Spire 2 mod that enables public co-op lobbies without needing to be friends on Steam. Version 1 with server browers is wip. Currently you can install the mod and handle direct connections.

## Install

Installation instructions will be added when the mod is implemented

## Requirements

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

## Repository Notes

This project was generated from the Alchyr Slay the Spire 2 mod template and currently uses the blank BaseLib/Harmony mod structure.

## License

See [LICENSE](LICENSE).

## Disclaimer

This is an experimental mod for an Early Access game. Game updates may break compatibility, and multiplayer behavior may change as Slay the Spire 2 evolves.
