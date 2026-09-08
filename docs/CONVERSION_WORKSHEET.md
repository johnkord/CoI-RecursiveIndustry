# Electronics II Conversion Worksheet

Target: 96 per 60 simulation Time. Material link bound: 450 per 60 Time.

These are added facilities and support, not whole-island totals. Both routes deliver the same requested output under the stated supply bounds.

## Terminal Recipe Rows

| Route | Inputs Per Batch | Outputs Per Batch | Nominal Time | Output Bound / 60 Time |
| --- | --- | --- | ---: | ---: |
| Staged | 128 PCB + 256 Electronics + 64 Silicon (Poly) | 128 Electronics II | 80 | 96 |
| Integrated | 216 Copper + 32 Rubber + 24 Glass + 48 Plastic + 48 Silicon (Poly) + 120 Industrial Control Stream | 96 Electronics II | 120 | 48 |

## Added Facilities and Support

| Added Requirement | Staged | Integrated |
| --- | ---: | ---: |
| Process and support buildings | 1 | 6 |
| Peak machine Computing | 256 | 816 |
| Rack III, steady state | 0 | 2 |
| Rack III, during cutover | 1 | 3 |
| Machine workers | 0 | 132 |
| Machine full-work power, kW | 2000 | 10800 |
| New rack power, kW | 0 | 3000 |
| Machine Maintenance III/month | 16 | 52 |
| Rack Maintenance III/month | 0 | 24 |
| Stream demand/60 Time | 0 | 120 |
| New Package production/60 Time | 0 | 4/7 |

## Remaining External Supply

Every listed external input still needs supply. No existing upstream building is assumed removable.

| Product / 60 Time | Staged | Integrated |
| --- | ---: | ---: |
| Copper | 0 | 216 |
| Electronics | 192 | 0 |
| Electronics III | 0 | 1/7 |
| Glass | 0 | 24 |
| Lab Equipment IV | 0 | 3/7 |
| Office Supplies | 0 | 2/7 |
| PCB | 96 | 0 |
| Plastic | 0 | 48 |
| Silicon (Poly) | 48 | 48 |
| Rubber | 0 | 32 |

## Commissioning Capital

Excludes racks, Data Center shells, external utilities, and transports.

| Product | Staged | Integrated |
| --- | ---: | ---: |
| Construction Parts IV | 960 | 2960 |
| Electronics IV | 128 | 384 |
| Frontier Programs | 4 | 12 |
| Validated Control Packages | 32 | 96 |
| Validated Research Dossiers | 0 | 4 |

Staged capital timing: not estimated; stock and spare production rates were not entered.

Integrated capital timing: not estimated; stock and spare production rates were not entered.

## Boundary

- Dedicated recipe assignments with curation-intensive Model adaptation and Rack III available.
- Only supplied spare Stream, Package capacity, and installed Computing are shared.
- Full-work power is installed nameplate, not utilization-weighted consumption.
- Rates use simulation Time; no wall-clock forecast or observed transport-flow claim.
- External material production, maintenance depots, generators, Data Center shells, rack construction, and transport construction.
- Whole-island labor/power ranking: staged PCB/Electronics suppliers are external, so their retirement or cost cannot be inferred from added-plant totals.
- Research completion, travel/delivery, construction duration, startup batches, and actual network topology.
- Save inspection, commands, automatic building placement, and automatic recipe changes.

## Sources

- Current player recipe declarations and configuration, bound by the JSON report's source hashes.
- [Exact Fraction model](../tools/model_reconstruction_bridge.py).
