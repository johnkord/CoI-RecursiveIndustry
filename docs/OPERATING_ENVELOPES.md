# Building operating envelopes

## Status

This table describes the unpublished 0.29.0a source successor. Native registration
and exact-value tests pass. Final artwork and isolated native UI checks also pass;
integrated fresh-world judgment remains open. Published 0.28.0a is unchanged.

## Design rule

Workers represent recurring human presence, care, experiments, governance,
hazard custody, or final acceptance. They do not scale automatically with
throughput. Routine autonomous handling and robotic assembly use zero workers.

Direct facility electricity normally uses 110% of four exact source machines at
equivalent throughput, rounded up to 0.5 MW, with a 0.5 MW orbital acceptance premium.
Water Reclamation uses the selected 100% source-equivalent floor rounded to 0.25 MW.
Recipe fuel, steam, oxygen, and other physical inputs remain unchanged.
Computing also remains a separate physical load on Rack III districts.

Integrated power follows the sum of all simultaneous source stages rather than
a generic facility multiplier. Precision remains a deliberate efficiency trade:
Direct output rate, 12.5% less physical feedstock per output, and 200% host power.

Maintenance remains physical when labor falls. Each Direct catalog uses 85% of
its largest four-source maintenance depot workload, converted to a selected
repair tier and rounded up. Nuclear front-end and reprocessing retain 100% source
workload. Tier follows repair technology, not workers or hazard alone.

## Universal portfolio

| Facility | Direct rows | Direct MW | Maximum MW | Computing | Workers | Maintenance |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Comminution Hub | 12 | 4.5 | 4.5 | 64 | 0 | I:28 |
| Mineral Products Works | 9 | 2 | 4 | 96 | 0 | I:21 |
| Arc Smelter | 10 | 26.5 | 26.5 | 192 | 0 | II:28 |
| Fuel Smelter | 7 | 1 | 1 | 64 | 0 | I:21 |
| Aluminum Works | 1 | 35.5 | 35.5 | 96 | 4 | II:28 |
| Alloy and Glass Works | 6 | 4.5 | 4.5 | 32 | 0 | II:13 |
| Casting and Finishing | 11 | 4 | 8 | 64 | 4 | I:7 |
| Refinery Complex | 17 | 2 | 8 | 256 | 0 | I:21 |
| Gas and Fertilizer | 15 | 5 | 10 | 192 | 4 | II:21 |
| Materials Chemistry | 12 | 2 | 2 | 192 | 4 | II:21 |
| Medical and Precision Chemistry | 10 | 2 | 4 | 128 | 8 | III:14 |
| Food Processing | 7 | 0.5 | 3 | 128 | 8 | I:14 |
| Food Pack | 9 | 2 | 4 | 96 | 4 | III:14 |
| Crop and Soil Bioprocessing | 13 | 1 | 2.5 | 96 | 4 | I:14 |
| Bioenergy Digestion | 12 | 0.5 | 0.5 | 96 | 0 | I:4 |
| Water Reclamation | 3 | 2.5 | 5 | 64 | 4 | I:28 |
| Process Water Chiller | 1 | 4.5 | 4.5 | 32 | 0 | I:6 |
| Thermal Desalination | 7 | 2 | 2 | 32 | 0 | I:7 |
| Thermal and Emissions | 6 | 2.5 | 2.5 | 128 | 4 | I:28 |
| Materials Recovery | 15 | 0.5 | 0.5 | 128 | 0 | I:4 |
| Nuclear Fuel Front End | 13 | 18 | 144 | 320 | 8 | III:16 |
| Nuclear Reprocessing | 4 | 9 | 9 | 128 | 16 | III:40 |
| Nuclear Fuel Fabrication | 1 | 2 | 4 | 64 | 4 | III:14 |
| Precision Materials | 2 | 9 | 9 | 128 | 4 | III:28 |
| Robotic Components | 7 | 2 | 4 | 128 | 0 | III:14 |
| General Manufacturing | 13 | 2 | 28.5 | 192 | 0 | III:14 |
| Orbital Fabrication | 8 | 2.5 | 2.5 | 256 | 8 | III:14 |

The portfolio totals 27 facilities, 231 Direct bindings, 149.5 MW Direct,
333.5 MW with every custom owner on its most expensive mode, 3,392 Computing,
88 workers, 203 Maintenance I, 111 Maintenance II, and 168 Maintenance III.
Fourteen Rack III add 21 MW and 168 Maintenance III.

The six new stable machine IDs are Fuel Smelter, Casting and Finishing Works,
Thermal Desalination Works, Nuclear Reprocessing Center, Nuclear Fuel
Fabrication Cell, and Robotic Components Fab. Parent construction capital and
Computing are partitioned across children rather than duplicated.

The 0.29.0a aluminum/alloy and treatment/chilling splits also conserve parent
capital, Computing, workers, and maintenance. Their extra process power buys
separate simultaneously usable hosts, not a universal output-rate increase.
Economy treatment uses 0.75 MW with twice the Direct duration. Integrated water
remains 4.5 MW; Precision uses 5 MW. Economy is not a low-power mode on every owner.

Four steam-condensation Direct bindings return to native Large Cooling Towers.
Those source machines already use zero workers, electricity, and maintenance.

## Other custom buildings

- Control Deployment Gateway: 1 MW, 4 workers, 8 Maintenance III.
- Local Deployment Controller: 0.25 MW, 32 Computing, 2 workers, 2 Maintenance III.
- Electronics Reclaimer repair: 1 MW active recipe power, 4 Modules per 240 Time.
- Autonomous Microchip Complex: 8 MW, 0 workers, 96 Maintenance III.
- Autonomous Electronics Integration: 2 MW, 0 workers, 16 Maintenance III.
- Autonomous Capital Fabrication: 2 MW, 0 workers, 16 Maintenance III.
- Autonomous Construction Nexus: 2 MW, 0 workers, 16 Maintenance III.
- Companion Animal Center: 250 kW, 8 workers, 4 Maintenance II.
- Orbital Power Array: unchanged at 240 MW output, 80 workers, and
  80 Maintenance III.

The general Autonomous Hauler carries the native Tier II tank, flatbed, and dump
attachment set with no fixed product type. Hydrogen Fuel Stations can therefore
assign it as a distribution truck. The 180-capacity Autonomous Dump and Tank
Haulers remain fixed loose/fluid specialists.

Early Curation, Science, validation, Systems Integration, Frontier Projects,
AI Operations, planetary coordination, farms, animal care, and orbital mission
work retain accountable human labor.

## Evidence and test boundary

Public tests check exact catalog partitioning, aggregate economics, custom-mode
ownership, split support conservation, research unlocks, and final icon/bundle
inventory. Standard
Machine, worker, recipe, and save behavior is inherited where unchanged. One
complete candidate, not a sequence of intermediate feature builds, owns the later
visual/planner observation and strict-log boundary.

## Sources

- `data/universal-industry-catalog.json`.
- `data/industrial-control-network.json`.
- `data/circular-agrifood.json`.
- `mods/RecursiveIndustry/src/UniversalIndustryData.cs`.
- `tests/test_operating_envelopes.py`.
- [Balance](BALANCE.md).
- [Architecture](ARCHITECTURE.md).