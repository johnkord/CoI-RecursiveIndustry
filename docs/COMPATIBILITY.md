# Compatibility

## Supported environment

| Component | Support |
| --- | --- |
| Captain of Industry | 0.8.7a, Build 614 exact successor target |
| Manifest minimum | 0.8.6c |
| Trains expansion | Required, 1.0.0 or newer |
| Supporter edition | Optional, 1.1.0 or newer |
| Runtime platform | Windows game build |
| Build target | .NET Framework 4.8, C# 10 |

The maximum verified game version is intentionally bounded. A later game version
may load the mod, but it is unsupported until its API and behavior are reviewed.
Build 614 has the same reflected ids, commands, selected public API, and relevant
controlling paths as Build 613. The 0.22.0b DLL and ZIP also reproduce
byte-for-byte against Build 614, and the author continued a long gameplay review
across the update. Version 0.22.0c raises the exact manifest ceiling to 0.8.7a
without changing gameplay. Publishing that successor remains gated on one clean
fresh-world startup and strict full-log audit.

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

Version 0.20.0a adds Data input capacity to eleven existing facilities and a Stream
input to twenty-four cross-stage composition recipes. Precision, local, staged,
and recovery rows remain Fiber-free.
Until a copy of an affected 0.19.0d save
is tested and classified, migration into 0.20.0a is unsupported. Start a new
campaign for the candidate. Do not add shadow machines solely to preserve a
pre-release save.

Version 0.21.0a failed registration before world creation because Integrated
Electronics III referenced its composition-only Electronics II stage through the
Direct-only source map. Version 0.21.0b resolved that stage, then exposed a second
registration failure: General Manufacturing required fifteen directional ports
on a fourteen-edge shell. Version 0.21.0c keeps the seven-row shell and places the
one remaining Data input on its proven top edge. Neither correction changes recipe
vectors or the 235 Direct bindings.

Version 0.21.0c adds two new stable farm prototype IDs and one Focus/research
branch without replacing or mutating vanilla farms. Existing 0.20.0a worlds are
not a supported migration baseline because 0.20.0a runtime testing remained open.
Start a new campaign for the Adaptive Agrifood candidate. The special farm
families still require fresh registration and save/reload evidence.

Version 0.21.0d links Greenhouse II and Chicken Farm to those stable targets as
native next tiers. Existing placed source entities are not replaced during load;
the link only exposes an unlocked in-place upgrade. Completing an upgrade keeps
the entity ID and same-family saved state. Another mod that already assigns a
next tier to either source is an explicit incompatibility and registration fails
instead of silently overwriting that link.

Version 0.22.0a adds one countable product, one settlement need, one attached
settlement module, four recipe IDs, and one research node. It does not alter
existing product kinds, farm save fields, Direct recipe identities, Fiber, or
custom serialization. Because no 0.21.x build is a supported migration baseline,
start a new campaign for the Circular Agrifood candidate. Runtime registration
and fresh-process service persistence remain open until the integrated session.

Version 0.22.0b changes no gameplay. It is the clean source-reproducible build
of the author-reviewed 0.22.0a implementation. Normalized decompiled IL is
identical. The same new-campaign-only compatibility boundary applies.

Version 0.22.0c is a compatibility-metadata successor. It changes the manifest
version and exact verified game ceiling only; all gameplay, assets, recipe
vectors, and stable IDs remain those of 0.22.0b.

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
