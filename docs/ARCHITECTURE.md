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
- Integrated power follows each exact simultaneous source composition. Effective
  targets range from 2 MW for broad crude fractionation to 144 MW for the full
  uranium front end. Elastomer retains an additional anti-dominance premium.
- Ten Precision rows remain Fiber-free. They alter one source process for 12.5%
  lower feedstock per output at 200% recipe power.
- Four exact authored agrifood rows are also Fiber-free. Unlike Integrated rows,
  they declare product vectors directly and use the same typed port planner and
  transport-floor checks.
- Direct rows remain Fiber-free and preserve source recipe identity.
- Packages commission autonomous capital and recur only for signed deployment,
  Focus, research, and orbital artifacts. Ordinary physical production, including
  local, staged, Precision, recovery, and Nexus rows, does not consume Packages
  per batch.
- Conventional source machines and recipes remain registered and usable.

## Adaptive farms

`AdaptiveAgrifoodData` registers one exact `FarmProto` family clone and one exact
`AnimalFarmProto` family clone. It changes only stable identity, description,
custom icon, construction hardware, and workers. Constructor inputs copy native
layout, crop or animal parameters, buffers, maintenance, graphics, and special
behavior. Native `Farm` and `AnimalFarm` serializers continue to own schedules,
fertility, irrigation, flock state, slaughter settings, notifications, and
persistence.

Greenhouse II links to Sensor-Guided Greenhouse and Chicken Farm links to
Monitored Poultry Farm through native `SetNextTier`. Build 613 upgrades mutate
the existing same-family entity in place, recreate its unchanged ports, and use
the difference between target and source construction assets. No custom migration
service participates. Registration fails closed if another mod already owns
either source prototype's next-tier link.

No custom farm service, scheduler, command processor, saved state, Harmony patch,
Stream input, or direct Computing input is added. Precision Irrigation uses the
registered native `FarmWaterConsumptionMultiplier` property through the standard
Office Focus implementation.

Farm initialization creates UI-only recipe summaries from crop and animal data;
it registers no hidden `RecipeProto` IDs. The two cloned families therefore do
not collide with vanilla recipe registration.

## Companion care

`CompanionAnimalCareData` registers one icon-only countable product plus a
standard `PopNeedProto`, `UpointsCategoryProto`, and `SettlementModuleProto`.
The attached module consumes Companion Provisions and produces loose Waste in
proportion to population. The need owns 0.6 Unity and no Health. Ordinary entity
costs retain eight care workers and Maintenance II; the module consumes 250 kW.

No custom service class, global worker modifier, command processor, serializer,
or saved state participates. Circular Agrifood research explicitly unlocks both
the module and the need.

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
fails closed if the catalog does not resolve exactly 25 facilities and 231 unique
Direct bindings, 21 Integrated compositions, 10 Precision modes, and four exact
authored agrifood recipes.

At runtime, Direct binding quantities include the live source-machine multiplier.
Transport duration is raised when necessary to keep each mapped highest-tier port
within the game transport ceiling. Direct facility power must meet 110% of the
four-source-machine equivalent, rounded up to 0.5 MW. Each facility declares an
explicit maintenance tier and depot-workload quantity.

Port planning is facility-wide and type-aware. For each product kind, it takes
the maximum simultaneous product count across every row, then sums those maxima.
The seven-row Chemical Plant II shell carries up to seven inputs on its left edge.
Overflow inputs first use right-edge rows not occupied by outputs. Precision
Robotic Components uses two right inputs. General Manufacturing uses three right inputs
and one top Data input, whose `v` direction is already used by native-compatible
Assembly layouts. Right-side outputs plus overflow inputs and top-edge capacity
are audited separately.
The two earlier authored owners use explicit Data ports in their existing layouts
and are audited separately from this generated planner.

## Assets

The player package contains three dependency-free bundles:

- `producticons_84e1`: eight Foundation product UI icons.
- `cartridge_c874`: one shared cartridge mesh, eight albedos, and shared PBR maps.
- `uiicons_5287`: 91 later products, entities, vehicles, trains, policies,
  Industrial Control, Adaptive Agrifood, and companion-care identities.

Original source art is under `art/RecursiveIndustry/`. Runtime code references
compatible game prefabs by path but does not redistribute those assets.

## Dependencies

The mod references game assemblies through `COI_ROOT`; all references have
`Private=false`. The Trains expansion assembly is required. Supporter content is
resolved conditionally at registration.

No third-party runtime library is bundled.
