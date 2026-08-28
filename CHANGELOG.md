# Changelog

All notable public changes to Recursive Industry will be documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [0.23.0b] - 2026-08-27

### Fixed

- Restored the Autonomous Hauler's complete Tier II tank, flatbed, and dump
  attachment set. It no longer declares a countable-only fixed product type, so
  Hydrogen Fuel Stations can select it as an attached distribution vehicle.
- Preserved the 180-capacity Autonomous Dump Hauler and Autonomous Tank Hauler
  as fixed loose/fluid specialists. No pathfinding, capacity, fuel, maintenance,
  worker, policy, or operating-envelope value changed.

### Compatibility

- This is the defect successor to the untested 0.23.0a candidate. Public
  0.22.0c and the 0.23.0a test archive remain unchanged.

## [0.23.0a] - 2026-08-27

### Changed

- Replaced the 19 broad high-power universal facilities with 25 process-scaled
  facilities while preserving 3,392 Computing and 1,024 commissioning Packages.
  Six new stable machine IDs separate fuel smelting, casting and finishing,
  thermal desalination, nuclear reprocessing, nuclear fuel fabrication, and
  robotic component assembly.
- Reduced the universal Direct envelope from 415 MW and 264 workers to 142.5 MW
  and 88 workers. The exact all-owner custom-mode envelope is 328.5 MW before
  the unchanged 14 Rack III support district.
- Replaced minimum-row maintenance inference with explicit physical tiers and
  depot-workload quantities: 203 Maintenance I, 111 Maintenance II, and 168
  Maintenance III per month across the universal portfolio.
- Set all 21 Integrated recipe power values from their simultaneous source-stage
  composition. Precision remains Fiber-free at 200% host power. Reassigned
  electronics, steel, and uranium modes to their coherent split facilities.
- Returned four zero-worker, zero-electricity, maintenance-free steam
  condensation bindings to native Large Cooling Towers.
- Retuned the Control Deployment Gateway, Microchip Complex, Electronics
  Integration, Capital Fabrication, Construction Nexus, and Companion Animal
  Center operating envelopes. The 240 MW Orbital Power Array is unchanged.

### Added

- Added six original compact facility identities to dependency-free
  `uiicons_5287`, taking the bundle from 85 to 91 assets.
- Added exact public regression coverage for facility partitioning, construction
  and Computing conservation, custom-mode ownership, support totals, research
  unlocks, and the unchanged Orbital Array boundary.

### Compatibility

- This is an unpublished new-campaign test candidate. Existing pre-release
  megafacility entities are not a supported migration boundary.
- Public 0.22.0c source, tag, release, and hosted archive remain unchanged.

## [0.22.0c] - 2026-08-22

### Changed

- Raised the exact verified manifest ceiling to Captain of Industry 0.8.7a,
  Build 614, after the reflected API and relevant controlling paths matched
  Build 613.
- Preserved all gameplay, C# source, assets, recipe vectors, and stable IDs from
  0.22.0b. Published as a GitHub prerelease after a clean fresh-world startup
  and strict complete-log audit passed.

## [0.22.0b] - 2026-08-22

### Changed

- Rebuilt the author-reviewed 0.22.0a source after clearing stale incremental
  compiler state. Normalized decompiled IL is identical; gameplay and assets are
  unchanged.
- Bound the public playtest to one clean source commit and reproducible ZIP.
- Classified migration from older placed-machine saves as unsupported and
  degraded-but-loadable after legacy molten-output connections spammed errors.

## [0.22.0a] - 2026-08-22

### Circular Agrifood

- Added four exact, Stream-free authored recipes while preserving the existing
  235 Direct, 21 Integrated, and 10 Precision contracts.
- Added Feed-hydrolysate routes for formulated Eggs, serum-free cultured Meat,
  and mycoprotein Meat Trimmings. They consume Oxygen, water, power, and
  formulation inputs and preserve Carbon Dioxide and Waste Water residuals.
- Added countable Companion Provisions and an attached, staffed Companion Animal
  Center. The optional service consumes 0.02 Provisions and returns 0.004 Waste
  per colonist per month for 0.6 Unity at full satisfaction. It grants no Health
  or worker-productivity bonus.
- Added two original UI identities and expanded dependency-free `uiicons_5287`
  from 83 to 85 assets.
- Kept poultry, composting, organic fertilizer, steam, and burning available as
  progressively lower-value Animal Feed outlets. No gas-fed Feed recipe,
  automatic fallback, synthetic carcass, or direct synthetic Food Pack was added.

### Fixed

- Fixed the 0.21.0b General Manufacturing registration failure by placing its
  final Data input on the proven top edge of the unchanged seven-row shell.
- Fixed the 0.21.0a registration failure in Integrated Electronics III by
  resolving its composition-only Electronics II stage from Assembly V. Recipe
  vectors and the 235 Direct bindings are unchanged. That first correction was
  0.21.0b; 0.21.0c corrected the port layout, and 0.21.0d adds native farm
  upgrades.

### Added

- Added native in-place upgrade links from Greenhouse II to Sensor-Guided
  Greenhouse and from Chicken Farm to Monitored Poultry Farm. Upgrade costs are
  the existing added hardware vectors; entity IDs and biological state remain
  under the native farm entities.
- Added optional Adaptive Agrifood Systems after Autonomous Essential Systems.
  It unlocks Precision Irrigation, a four-worker Sensor-Guided Greenhouse, and a
  four-worker Monitored Poultry Farm.
- Precision Irrigation applies -2% farm water per level for five levels. It does
  not change crop yield, settlement water, rain collection, or groundwater.
- Both farm upgrades use native special-entity families and preserve every
  crop, animal, growth, input, output, maintenance, control, and persistence path.
- Added three custom agrifood identities to dependency-free `uiicons_5287`.

- Added non-storable Industrial Control Stream, Data-only Access Fiber and
  Backbone Fiber, a four-way Fiber Junction, and a Control Deployment Gateway.
- Added Industrial Control Networks after Recursive Epoch V and made it the
  parent of all five universal-industry branches.
- Added optional Federated Deployment with a long-batch Deployment Assurance
  Campus and a 250%-power Backbone deployment row on the existing Gateway.
- Added six original UI identities and expanded dependency-free `uiicons_5287`
  from 74 to 80 assets.
- Added a schema-1 control-network authority, source auditor, negative fixtures,
  topology simulator, and Gateway sensitivity tournament.

### Changed

- Twenty-four cross-stage compositions consume one Industrial Control Stream per
  active second. Twenty-one belong to the universal catalog; raw Electronics II
  and integrated Construction Parts III and Vehicle Parts II remain on their
  earlier specialist facilities. Ten Precision modes remain Fiber-free.
- Removed recurring Package inputs from eighteen ordinary physical-manufacturing
  rows across Electronics III, Microchips, Electronics II, capital fabrication,
  and the Construction Nexus. Packages remain construction capital and recurring
  inputs only where signed deployment, Focus, research, or orbital artifacts are
  actually applied.
- Added Stream-controlled raw Electronics III and Lab Equipment II, III, and IV
  compositions. They cancel their transported lower tiers, preserve exact source
  vectors, run at 200% power, and leave Electronics IV as a practical partially
  integrated endpoint rather than collapsing every advanced domain.
- Backbone deployment converts 2 Packages into 420 Stream/60s, preserving the
  local row's exact 210 Stream-per-Package yield while allowing one Gateway to
  serve seven Backbone consumers.
- Batch Deployment Assurance converts 16 Model Archives, 32 Lab Equipment IV,
  and 32 Electronics III into 128 Packages/720s. It equals four standard
  validators without changing material yield or adding recurring Dossiers.
- Added Directed Diesel, Gas and Hydrogen, Deep Hydrogen, Polymer, and Elastomer
  Integrated refinery slates. All run at 2x crude-feed throughput, consume live
  Stream, cancel transported oil fractions, and retain every final residual.
- Polymer uses 300% recipe power and Elastomer uses 400%; other Integrated rows
  remain at 200%.
- Added Integrated Mechanical Parts from crushed ore through smelting, casting,
  and assembly. It supplies native Maintenance depots without replacing their
  special service behavior.
- Added Integrated Crew Provisioning from Chicken Carcass, Wheat, Medical
  Supplies II, and Plastic. Chicken Farm remains a separate biological boundary.
- Deepened Integrated Electronics IV to produce and cancel Electronics III as
  well as Lens inside the composition.
- Added the registered `Electronics3Assembly` recipe to the Precision Components
  Fab as the 235th Fiber-free Direct binding. The external recipe export names
  the same Assembly V row `Electronics3AssemblyRoboticT2`.
- Corrected facility-wide typed port planning for five logical eight-input
  owners. They retain the seven-row Chemical Plant II shell and place one input
  on an unused right-edge row; recipe vectors and ownership are unchanged.
- Removed redundant 64-Program and 8-Dossier conditions from Federated
  Deployment. Industrial Control Networks is now its sole availability parent;
  the node still costs 480 months of Space Research. Epoch V already requires
  256 lifetime Frontier Programs, so the weaker duplicate gates only blocked
  sandbox and migrated research states.

### Compatibility

- Version 0.20.0a changes eleven facility layouts and twenty-four composition
  recipe inputs. Migration from pre-release saves is not yet supported; start a new
  campaign for this candidate.

## [0.19.0d] - 2026-08-16

### Removed

- Removed the Orbital Power Relay prototype, stable ID, and build-menu entity
  entirely. Orbital power now has one modded building: the 240 MW Array.

### Compatibility

- Pre-release saves containing a placed Orbital Power Relay cannot load this
  version normally. Start a new campaign or remove the Relay before upgrading.

## [0.19.0c] - 2026-08-16

### Changed

- Removed the strictly dominated 30 MW Orbital Power Relay from new-game
  progression. Orbital Breakthrough now unlocks only the 240 MW Array; the Relay
  prototype remains registered solely for old pre-release saves.

### Added

- Public source repository, contributor documentation, and policy-safe packaging.
- Physical accelerator hardware, Dataset and Model Archives, validated control,
  AI Operations, Applied Science, and Recursive Epoch progression.
- Autonomous road, heavy-equipment, and rail variants using native game behavior.
- Planetary coordination, orbital science and power, and two bounded World
  Exchange contracts.
- Nineteen high-power industrial megafacilities with Direct, Integrated, and
  Precision production choices.
- Original product, transported-cargo, building, vehicle, train, and policy art.
- Offline repository validation and deterministic player packaging.

### Known limitations

- No public stable release has been declared yet.
- Enable the mod when creating a new campaign. Adding or removing it from an
  existing save is unsupported.
- Final balance, comprehension, and cross-surface visual judgment remain subject
  to integrated author and independent player testing.
- Custom world-space building models are outside the gameplay-first 1.0 scope.

[0.19.0c]: https://github.com/johnkord/CoI-RecursiveIndustry/releases/tag/v0.19.0c
[0.19.0d]: https://github.com/johnkord/CoI-RecursiveIndustry/releases/tag/v0.19.0d
