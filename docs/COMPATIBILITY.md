# Compatibility

## Supported environment

| Component | Support |
| --- | --- |
| Captain of Industry | 0.8.7, verified on Build 613 |
| Manifest minimum | 0.8.6c |
| Trains expansion | Required, 1.0.0 or newer |
| Supporter edition | Optional, 1.1.0 or newer |
| Runtime platform | Windows game build |
| Build target | .NET Framework 4.8, C# 10 |

The maximum verified game version is intentionally bounded. A later game version
may load the mod, but it is unsupported until its API and behavior are reviewed.

## Saves

Enable Recursive Industry when creating a new campaign. Adding it to or removing
it from an existing normal save is unsupported. Stable prototype IDs are treated
as save contracts and should not be renamed casually.

Version 0.19.0d intentionally removes the pre-release
`RecursiveIndustry_OrbitalPowerRelay` prototype. A pre-release save containing a
placed Relay cannot load normally with 0.19.0d. Remove the Relay while running
0.19.0c before upgrading, or start a new campaign. No stable release save contract
is affected because both versions precede 1.0.

Captain of Industry recovery saves may recover part of a factory after missing or
incompatible mods, but recovery is lossy and is not a substitute for supported mod
migration.

## Mod interactions

Recursive Industry adds content and uses standard entity families. It does not
patch global methods. Conflicts remain possible when another mod:

- uses the same prototype IDs;
- occupies the same research-tree coordinates;
- mutates the same vanilla prototypes or global properties; or
- changes assumptions of the required Trains expansion.

Report the complete mod list and game log with compatibility issues.

## Network and filesystem behavior

The mod makes no network connections. Runtime behavior does not intentionally
access arbitrary files. The build deploys normal player files to Captain of
Industry's `%APPDATA%` Mods directory when deployment is enabled.
