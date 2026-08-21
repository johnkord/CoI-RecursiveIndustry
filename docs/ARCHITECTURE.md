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
- Twenty-one generated Integrated rows compose multiple source stages and cancel
  at least one transported intermediate. Three earlier authored compositions on
  the Electronics Integration and Capital Fabrication facilities use the same
  Stream contract, for twenty-four controlled recipes on eleven owners.
- Integrated power defaults to 200%. The deeply collapsed Polymer and Elastomer
  refinery rows explicitly use 300% and 400%.
- Ten Precision rows remain Fiber-free. They alter one source process for 12.5%
  lower feedstock per output at 200% recipe power.
- Direct rows remain Fiber-free and preserve source recipe identity.
- Packages commission autonomous capital and recur only for signed deployment,
  Focus, research, and orbital artifacts. Ordinary physical production, including
  local, staged, Precision, recovery, and Nexus rows, does not consume Packages
  per batch.
- Conventional source machines and recipes remain registered and usable.

## Data and Fiber

`DataProductProto` is a direct `ProductProto` subclass with a distinct product
type. Industrial Control Stream is non-storable, non-discardable, non-recyclable,
and accepted only by the custom `:` port shape.

Access Fiber clones Pipe II movement and carries 200 units per 60 seconds.
Backbone Fiber clones Pipe III movement and carries 450. Both disable mixed
products, render no transported cargo, and use per-product coloring. Access is
the lower tier and upgrades to Backbone. The Fiber Junction uses the standard
four-way `MiniZipperProto` path.

The Control Deployment Gateway is a standard Machine. Its local row converts one
countable Validated Control Package into 210 Stream over 60 seconds. Federated
Deployment unlocks a 250%-power Backbone row that converts two Packages into 420
Stream over the same duration. Both preserve 210 Stream per Package. No service,
patch, packet graph, automatic recipe switch, or custom saved runtime state
participates in production.

Deployment Assurance is a separate standard Machine, not a universal facility or
Data owner. Its 720-second batch compresses four ordinary Package validators while
preserving exact Model Archive, Lab Equipment IV, and Electronics III ratios.

One owner consumes 60 Stream per active minute regardless of which controlled
recipe it selects. The complete eleven-owner boundary therefore demands 660/min.
Two Backbone Gateway rows provide 840/min over two 450/min Fiber paths. Recipe
count never substitutes for concurrent owner count in capacity calculations.

Stream never replaces special runtime families. Chicken Farm remains the source
of Eggs and Chicken Carcass. Native Maintenance depots retain their virtual
Maintenance output and hardcoded Computing behavior. Integrated Mechanical Parts
only collapses their upstream physical steel and assembly supply chain.

## Universal catalog

`UniversalIndustryCatalog.cs` defines the catalog types.
`UniversalIndustryCatalog.g.cs`, `RecursiveIndustryIds.UniversalIndustry.cs`, and
`UniversalIndustryIcons.g.cs` are generated from the compact public authority at
`data/universal-industry-catalog.json`. Check parity with
`python tools/generate_recursive_industry_universal_source.py`, or regenerate with
the `--write` flag. Registration
fails closed if the catalog does not resolve exactly 19 facilities and 235 unique
Direct bindings, 21 Integrated compositions, and 10 Precision modes.

At runtime, Direct binding quantities include the live source-machine multiplier.
Transport duration is raised when necessary to keep each mapped highest-tier port
within the game transport ceiling. Facility power is the greater of the authored
envelope and 125% of four-source-machine equivalent power.

Port planning is facility-wide and type-aware. For each product kind, it takes
the maximum simultaneous product count across every row, then sums those maxima.
The seven-row Chemical Plant II shell carries up to seven inputs on its left edge.
Primary Smelter, Food Pack, Nuclear Fuel, Precision Components, and General
Manufacturing each need an eighth Data-inclusive input; they place one input on
an otherwise unused right-edge row using Build 613's verified `<` input
direction. Right-side outputs plus overflow inputs may never exceed seven.
The two earlier authored owners use explicit Data ports in their existing layouts
and are audited separately from this generated planner.

## Assets

The player package contains three dependency-free bundles:

- `producticons_84e1`: eight Foundation product UI icons.
- `cartridge_c874`: one shared cartridge mesh, eight albedos, and shared PBR maps.
- `uiicons_5287`: 80 later products, entities, vehicles, trains, policies, and
  Industrial Control identities.

Original source art is under `art/RecursiveIndustry/`. Runtime code references
compatible game prefabs by path but does not redistribute those assets.

## Dependencies

The mod references game assemblies through `COI_ROOT`; all references have
`Private=false`. The Trains expansion assembly is required. Supporter content is
resolved conditionally at registration.

No third-party runtime library is bundled.
