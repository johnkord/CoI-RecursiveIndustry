# Architecture

## Runtime shape

Recursive Industry is a content-focused `DataOnlyMod` targeting .NET Framework
4.8 and C# 10. Content is registered through Captain of Industry's prototype
system. The mod does not use Harmony, replace the simulation loop, or make network
connections.

Registration is divided into small `IModData` classes under
`mods/RecursiveIndustry/src/`. Stable IDs live in partial
`RecursiveIndustryIds` classes grouped by prototype category.

## Content contracts

- Products use explicit game product kinds and standard logistics.
- Machines use standard `Machine`, `DataCenter`, Office, vehicle, train, world
  contract, and generator behavior.
- Multi-product recipes declare explicit common input and output port mappings.
- Universal Direct rows bind existing `RecipeProto` identities instead of cloning
  hundreds of recipes.
- Integrated and Precision rows are the only twenty authored universal recipe IDs.
- Conventional source machines and recipes remain registered and usable.

## Universal catalog

`UniversalIndustryCatalog.cs` defines the catalog types.
`UniversalIndustryCatalog.g.cs`, `RecursiveIndustryIds.UniversalIndustry.cs`, and
`UniversalIndustryIcons.g.cs` are generated from the compact public authority at
`data/universal-industry-catalog.json`. Check parity with
`python tools/generate_recursive_industry_universal_source.py`, or regenerate with
the `--write` flag. Registration
fails closed if the catalog does not resolve exactly 19 facilities and 234 unique
Direct bindings.

At runtime, Direct binding quantities include the live source-machine multiplier.
Transport duration is raised when necessary to keep each mapped highest-tier port
within the game transport ceiling. Facility power is the greater of the authored
envelope and 125% of four-source-machine equivalent power.

## Assets

The player package contains three dependency-free bundles:

- `producticons_84e1`: eight Foundation product UI icons.
- `cartridge_c874`: one shared cartridge mesh, eight albedos, and shared PBR maps.
- `uiicons_5287`: later products, entities, vehicles, trains, and policies.

Original source art is under `art/RecursiveIndustry/`. Runtime code references
compatible game prefabs by path but does not redistribute those assets.

## Dependencies

The mod references game assemblies through `COI_ROOT`; all references have
`Private=false`. The Trains expansion assembly is required. Supporter content is
resolved conditionally at registration.

No third-party runtime library is bundled.
