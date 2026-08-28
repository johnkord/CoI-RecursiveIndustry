# Recursive Industry mod source

This directory is the build and packaging root for Recursive Industry.

The current source version is **0.23.0b**, an unpublished new-campaign test
candidate with process-scaled power, labor, and maintenance across all custom
buildings plus corrected Fuel Station assignment for the general Autonomous
Hauler. The published 0.22.0c playtest remains immutable. Orbital power retains
one modded building: the 240 MW Array.

## Requirements

- Captain of Industry 0.8.7a Build 614.
- Trains expansion 1.0.0 or newer.
- Optional Supporter edition 1.1.0 or newer.
- Windows, .NET SDK capable of targeting .NET Framework 4.8, and `COI_ROOT` set
  to the installed game directory.

## Build

From the repository root:

```powershell
dotnet build mods/RecursiveIndustry/RecursiveIndustry.csproj -c Release
```

The project references game and DLC assemblies through `COI_ROOT` with
`Private=false`; they are never copied into the player package.

Builds deploy to `%APPDATA%/Captain of Industry/Mods/RecursiveIndustry` by
default. Disable deployment with:

```powershell
dotnet build mods/RecursiveIndustry/RecursiveIndustry.csproj -c Release `
  /p:DeployToModsFolder=false
```

Package the mod with:

```powershell
python tools/package_mod.py mods/RecursiveIndustry
```

See the repository [README](../../README.md) and [building guide](../../docs/BUILDING.md)
for the complete public workflow.
