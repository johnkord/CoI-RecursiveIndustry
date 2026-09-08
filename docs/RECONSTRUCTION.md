# Rebuilding an Industrial District

The unpublished reconstruction candidate makes live-control integration available from
Epoch II and terrestrial reconstruction from Epoch III. Civic Knowledge provides
a new optional settlement destination. This is not a quest system or a change
to the hosted 0.22.0c playtest.

Version 0.26.0a added [complete building artwork](BUILDING_ART.md); 0.27.0a completes
the [remaining world models](WORLD_ART.md). Both preserve the 0.25.0a gameplay
described here.

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

One raw Electronics II facility plus one local Gateway requires, before racks,
Fiber, and supporting production:

| Construction product | Combined quantity |
| --- | ---: |
| Construction Parts IV | 1600 |
| Electronics IV | 256 |
| Validated Control Packages | 64 |
| Frontier Programs | 8 |
| Validated Research Dossiers | 4 |

At 48 Electronics II per 60 Time, the facility needs 60 Stream per 60 Time.
A local Gateway converts one Package into 210 Stream every 60 Time, consuming
Packages as demand permits. Continuing supply needs Models, validation inputs,
and Datasets. A reserve of already supplied Computing or Packages is useful;
another district does not automatically require another complete support chain.

| Network | Source capacity | Trunk capacity | Full-rate compositions |
| --- | ---: | ---: | ---: |
| One local Gateway and Access | 210 | 200 | 3 |
| One local Gateway and Backbone | 210 | 450 | 3 |
| Two local Gateways and Backbone | 420 | 450 | 7 |
| One dense Gateway and Backbone | 420 | 450 | 7 |

Rates are per 60 simulation Time. A wider link does not increase its source's
output. These are capacity bounds, not guarantees of flow or priority. Both
transport tiers are available with Industrial Control; Federated Deployment
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

## Sources

- [Electronics recipes](../mods/RecursiveIndustry/src/AutonomousElectronicsIntegrationData.cs).
- [Capital recipes](../mods/RecursiveIndustry/src/AutonomousCapitalFabricationData.cs).
- [Gateway](../mods/RecursiveIndustry/src/IndustrialControlGatewayData.cs).
- [Source-derived comparison](../tools/model_reconstruction_bridge.py).
- [Progression](PROGRESSION.md) and [control authority](../data/industrial-control-network.json).