# Rebuilding an Industrial District

The published 0.28.0a playtest makes industrial applications independent choices
after Systems Integration, retaining their native source technologies. Civic
Knowledge uses the shared Fiber infrastructure without requiring industrial
control or Microchip consolidation. See [Progression](PROGRESSION.md).

The unpublished 0.29.0a successor adds the in-game planner and selected choices
below. Source compilation, offline arithmetic, final Unity bundles, and isolated
native UI checks pass. Testing uses the separately supplied exact candidate archive;
an arbitrary source build is not that artifact. Published 0.28.0a stays unchanged.

## Choose What to Simplify

Electronics and capital fabrication retain staged recipes. Industrial Control
Networks also enables the existing integrated rows. Both machine research and
the composition unlock are needed; construction and recurring control support
are separate commitments.

| Choice | Useful when | What remains |
| --- | --- | --- |
| Staged production | Existing intermediate suppliers also serve other industries. | More handoffs, but no Fiber input on the staged rows. |
| Deep integration | A whole chain can be retired or a new compact district is needed. | Raw-input connections, power, Computing, and live control. |
| Hybrid conversion | Only part of the old district is worth replacing. | Deliberately retained suppliers and a smaller conversion. |

At default durations, the Electronics II row with PCB and Electronics produces
128 every 80 Time, or 96 per 60 Time. The row with Copper, Rubber, Glass, Plastic,
Silicon (Poly), and Stream produces 96 every 120 Time, or 48 per 60 Time. Matching
the first row's full output therefore needs two raw facilities, not one.

Integrated Construction Parts III similarly produces 64 per 80 Time versus the
staged terminal row's 128. Its advantage is removing the lower-part handoffs,
not doubling the terminal machine's output. Native and shared upstream suppliers
must be counted before claiming a smaller total district.

## The First Deployment

One raw Electronics II facility plus one standard Gateway requires, before racks,
Fiber, and supporting production:

| Construction product | Combined quantity |
| --- | ---: |
| Construction Parts IV | 1600 |
| Electronics IV | 256 |
| Validated Control Packages | 64 |
| Frontier Programs | 8 |
| Validated Research Dossiers | 4 |

At 48 Electronics II per 60 Time, the facility needs 60 Stream per 60 Time.
A standard Gateway converts one Package into 210 Stream every 60 Time, consuming
Packages as demand permits. Continuing supply needs Models, validation inputs,
and Datasets. A reserve of already supplied Computing or Packages is useful;
another district does not automatically require another complete support chain.

| Network | Source capacity | Trunk capacity | Full-rate compositions |
| --- | ---: | ---: | ---: |
| One standard Gateway and Access | 210 | 200 | 3 |
| One standard Gateway and Backbone | 210 | 450 | 3 |
| Two standard Gateways and Backbone | 420 | 450 | 7 |
| One dense Gateway and Backbone | 420 | 450 | 7 |

The 0.29.0a Local Deployment Controller supplies 105 Stream per Package per
60 Time. Its 3-by-3 body, 250 kW, 32 Computing, two workers, and smaller capital
make a first composition easier to commission. It uses twice the Packages per
Stream of a Gateway. Controllers can be pooled; their capacity is not artificially
limited to one consumer. Central supply remains valuable for fewer installations,
staff, and Package demand rather than winning every electricity comparison.

Rates are per 60 simulation Time. A wider link does not increase its source's
output. These are capacity bounds, not guarantees of flow or priority. Both
transport tiers are available with Fiber Infrastructure; Federated Deployment
adds dense supply and bulk assurance, not permission to lay Backbone.

## Make the Conversion Voluntary

Keep the old line supplying the island while staging the new inputs and support,
or convert incrementally if spare land is scarce. Allow for temporary overlapping
power and Computing demand. Recipe changes can require different product
connections, so integration is not an automatic switch or fallback.

Once the new district meets the intended demand, choose what to retain, repurpose,
pause, or dismantle. A shared supplier still serving another factory has not been
made redundant. Spare workers and useful old equipment do not need to be removed
to count the project as a success.

## An Optional Island Renewal Project

Choose a crowded industrial parcel, rebuild its useful production more compactly,
and reuse the land for housing, improved access, another factory, or a landscape
you prefer. A shoreline is one possibility; a plateau or logistics corridor works
just as well. A constrained site can use several small cutovers instead of one
large parallel construction project.

There is no completion counter, demolition reward, or extra pollution-removal
mechanic. The result is the place you have built and the capacity it now supports.
Keeping a historical plant beside its successor is as valid as clearing the site.

## Conversion Worksheet

### In-Game Planner (0.29.0a Source)

The native Reconstruction Planner has one global entry and an **Inspect current**
action. It reads exact registered recipes, configured values, research state,
and a simulation-synchronized selected-building snapshot. The searchable catalog
covers every mod Machine binding and its declared native comparators, with 50
rows per page and up to eight selections. Farms, Offices, services, and generators
are reference-only entries rather than invented Machine recipes.

**Alternatives** are mutually exclusive choices against the same headroom.
**Combined project** purchases shared support once across the selected consumers.
Each process selects one target output and a rate per 60 simulation Time.
Co-products remain simultaneous outputs needing sinks; they are not free credits.
Different selected recipes require dedicated hosts, even on the same building type.

The Process view separates requested output, transport capacity bounds, peak power,
ideal utilization-based power, and process energy per output. Added support uses
explicitly selected suppliers, unlocked racks, Data Center slots, and typed transport
tiers. Spare supply defaults to zero and means guaranteed supplied capacity, not
momentary idle output. Cycles, missing research, unavailable transports, invalid
numbers, and work limits produce explicit incomplete states, not cheaper totals.

Conversion keeps existing commitments during cutover. Only explicitly declared
retirement reduces steady demand. Construction stock and spare production give
a parallel supply lower bound, excluding delivery, construction, startup, and
temporary storage. External suppliers remain outside known costs; no exact total
island area, retirement recommendation, or payback is implied.

The window sends no gameplay commands and saves no comparison state. Its worker
uses detached exact values with a 25 ms budget and bounded graph expansion.
Save/world changes, edits, close, and disposal invalidate pending generations.
Isolated native rendering and form callbacks pass. Live game integration and
player judgment remain the integrated author-test boundary.

### Offline Worksheet

The optional offline planner compares staged and integrated Electronics II,
Construction Parts III, or Vehicle Parts II at the same requested output. It
requires Python 3.12 and this source checkout, not the game or a save export.

```console
python tools/plan_reconstruction.py data/reconstruction-scenario.example.json
python tools/plan_reconstruction.py data/reconstruction-scenario.example.json --json
```

The [example scenario](../data/reconstruction-scenario.example.json) and
[generated worksheet](CONVERSION_WORKSHEET.md) show the supported fields.
`output_per_60` and spare supply rates accept exact fractions such as `1/7`.
Computing fields are integers. `overlap_computing` means additional temporary
load beyond the entered ongoing demand and new facilities; do not count the same
old equipment twice. Optional `construction_stock` and
`spare_construction_rates_per_60` use product keys and produce only a parallel
supply lower bound, not a construction schedule.

This worksheet assumes curation-intensive Model adaptation and Rack III are
available. Its totals include added process/control support, with remaining
external supply listed separately. It does not rank whole-island labor or power,
decide that a shared supplier is redundant, or issue game commands.

## Contributor Comparison

Run `python tools/model_reconstruction_bridge.py` for a deterministic,
source-derived report. It reads configured recipe defaults, counts dedicated
assignments, closes control's Package/Model/Dataset demand, and distinguishes
installed headroom from new racks and temporary cutover capacity.

The report exposes external material inputs and excludes their factories,
maintenance depots, generation, Data Center shells, rack construction, and layout
geometry. It is not a whole-island cost or payback forecast. Its preparation
example is only a lower bound using explicitly supplied spare rates; research,
construction, transport, and startup are additional obligations.

## Selective Breakthroughs (0.29.0a Source)

Aluminum Works keeps the high-power aluminum cell. Alloy and Glass Works handles
the lighter metallurgy portfolio without paying that cell's power class. Water
Reclamation and Process Water Chiller likewise have separate installations.
The splits conserve parent capital, Computing, staffing, and maintenance.

Efficient Water Processing unlocks two Economy treatment rows: native quantities
at 4x, twice the source duration, and 750 kW on the existing treatment host.
Equal output needs twice the buildings and Computing of Direct treatment. It can
win electricity when rack headroom exists and lose at a rack-rounding boundary.
Integrated water recovery stays at 4.5 MW; Precision is 5 MW.

Validated Deployment unlocks repair on the existing Electronics Reclaimer:
4 Spent Accelerators + 2 Microchips + 2 Electronics III + 2 Titanium Alloy +
1 Package produce 4 Accelerator Modules + 2 Waste in 240 Time at 1 MW.
Repair saves replacement components but is slower and uses four times the process
energy per Module of new manufacture. Reusing intact racks, recovering electronics,
and take-back remain alternatives. The Package pays for tested redeployment; it
does not restore recurring Packages to ordinary manufacturing.

## Sources

- [Electronics recipes](../mods/RecursiveIndustry/src/AutonomousElectronicsIntegrationData.cs).
- [Capital recipes](../mods/RecursiveIndustry/src/AutonomousCapitalFabricationData.cs).
- [Gateway](../mods/RecursiveIndustry/src/IndustrialControlGatewayData.cs).
- [Source-derived comparison](../tools/model_reconstruction_bridge.py).
- [Scenario planner](../tools/plan_reconstruction.py).
- [Exact native planner](../mods/RecursiveIndustry/src/Planner/PlannerCalculator.cs)
	and [Fraction oracle](../tools/planner_oracle.py).
- [Progression](PROGRESSION.md) and [control authority](../data/industrial-control-network.json).