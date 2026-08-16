# Balance

## Intended pressure

Recursive Industry trades workers, footprint, and selected feedstocks for large
capital commitments, Computing, validation infrastructure, maintenance, and power.
Power remains the primary scaling pressure; the mod does not include low-power
production modes.

## Package semantics

Validated Control Packages are not applied uniformly:

- Bootstrap and physical-evidence producers remain Package-free to avoid a
  circular first-validation dependency.
- Autonomous capital consumes one-time commissioning Packages.
- Offices and authored recipes that continuously deploy changing control consume
  recurring Packages.
- Direct megafacility rows reuse vanilla recipe identities and do not add an
  arbitrary recurring Package input after commissioning.

## Selected universal economy

The current pre-release balance has:

- 19 specialist facilities.
- 3,392 total universal Computing, equivalent to 14 Rack III.
- 1,024 commissioning Packages.
- 415 MW of Direct process power before rack and support infrastructure.
- 264 retained workers.
- 462 Maintenance III per month before policy effects.

An ordinary research branch needs approximately three to four Rack III. The
mature core needs four dedicated Package validators; adding the 625,000-Focus
Planetary Coordination Center raises that modeled district to eight.

## Mode tradeoffs

- **Direct:** four times the exact source binding quantities at the source duration.
- **Integrated:** twice terminal throughput with composed inputs and outputs,
  retained final byproducts, and 200% recipe power.
- **Precision:** seven source input batches become eight output batches over twice
  the source duration at 200% recipe power. Output rate matches Direct, physical
  feedstock per output falls 12.5%, and energy per output doubles.

Integrated modes should lose when intermediate flexibility or electricity is the
binding constraint. Precision should win under feedstock pressure and lose when
power is scarce.

## Simulation method

The economy model counts active machine instances rather than summing mutually
exclusive recipe rows. It closes whole Rack III counts, rack power and coolant,
validator Model and Dataset support, Dossier science support, workers,
maintenance, and Orbital Arrays.

Two power boundaries are useful:

- **Terrestrial grid:** universal Direct load is approximately 436 MW; optimized
  operation is approximately 658 MW.
- **Orbital self-sufficient:** closing Array and Dossier support raises those loads
  to approximately 442 MW and 665 MW.

The deliberately unrealistic all-content stress case still requires 34 Rack III,
eight validators, and three to four Orbital Arrays. It is a support-explosion test,
not a prescribed factory layout.

Run the standard-library model from the repository root:

```powershell
python tools/simulate_recursive_industry_economy.py
python tools/simulate_recursive_industry_economy.py --power-basis terrestrial
python tools/simulate_recursive_industry_economy.py --json
```

## Open judgment

Integrated play still needs to determine whether three to four racks per branch,
four to eight validators, and 415 to 665 MW feel substantial without becoming
repetitive infrastructure ceremony. Those are player-experience questions, not
facts the offline model can settle.
