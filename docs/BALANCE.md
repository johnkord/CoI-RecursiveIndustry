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
- Offices, Gateway deployment, and recurring research or orbital artifacts
  consume Packages because they apply signed releases over time.
- Control Deployment Gateways convert recurring Packages into conserved live
  Industrial Control Stream.
- Ordinary physical manufacturing does not consume Packages after commissioning.
  This includes Direct, local, staged, Precision, recovery, Electronics III,
  Microchips, Electronics II, capital fabrication, and Construction Nexus rows.

## Selected universal economy

The current pre-release balance has:

- 19 specialist facilities.
- 3,392 total universal Computing, equivalent to 14 Rack III.
- 1,024 commissioning Packages.
- 415 MW of Direct process power before rack and support infrastructure.
- 264 retained workers.
- 462 Maintenance III per month before policy effects.

An ordinary research branch needs approximately three to four Rack III. Before
Industrial Control, the mature core consumes 408 Packages/hour and needs three
standard validators. Adding the 625,000-Focus Planetary Coordination Center raises
that modeled district to 1,048/hour and seven validators. The complete Control
network raises the composed boundaries to four and eight validators respectively.

## Mode tradeoffs

- **Direct:** four times the exact source binding quantities at the source duration.
- **Integrated:** twice the declared terminal or chain-feed anchor with composed
  inputs and outputs, transported intermediates cancelled, retained final
  byproducts, and 60 Stream per active minute. Power defaults to 200%; Polymer
  uses 300% and Elastomer 400%.
- **Precision:** seven source input batches become eight output batches over twice
  the source duration at 200% recipe power. Output rate matches Direct, physical
  feedstock per output falls 12.5%, and energy per output doubles. Precision is
  local process optimization and consumes no Stream.

Integrated modes should lose when intermediate flexibility or electricity is the
binding constraint. Precision should win under feedstock pressure and lose when
power is scarce.

## Directed petrochemical slates

The six Refinery strategies all process at the existing Integrated anchor of 2x
vanilla crude-feed throughput. Broad fractionation retains Heavy Oil and Naphtha
for flexible downstream routing. Five directed slates consume every Heavy,
Medium, and Light Oil and every Naphtha intermediate to make Diesel, Fuel Gas
plus Hydrogen, deep Hydrogen, Plastic, or Rubber.

Directed Diesel, gas, and Hydrogen use 200% recipe power, or 60 MW. Polymer uses
300%, or 90 MW. Elastomer uses 400%, or 120 MW. The higher endpoint premiums
prevent Plastic and Rubber from beating their existing Direct rows on both
footprint and electricity per output. All rows retain Sour Water and any final
CO2, Exhaust, Water, Fuel Gas, or Hydrogen residual.

## Industrial Control capacity

- Access Fiber carries 200 Stream per minute, enough for three continuously
  optimized facilities with 20 spare.
- Backbone Fiber carries 450 per minute, enough for seven with 30 spare.
- One local Gateway produces 210 per minute. Eleven owners demand 660, so the
  local-only comparison installs four Gateways and provides 840/minute.
- Federated Deployment adds a 250%-power row producing 420 per minute from two
  Packages. Two Backbone-mode Gateways replace four local Gateways, provide the
  same 840/minute output over 900/minute of Fiber capacity, and preserve 210
  Stream per Package. They use 20 rather than 16 MW while reducing Gateway
  workers from 96 to 48 and Computing from 1,024 to 512.
- Demand backpressure consumes $1320/7$, approximately 188.57, Packages per hour.
  Both unconstrained topologies consume 240/hour. One standard validator is no
  longer enough; two close the live network with useful margin.

The selected all-eleven local-Gateway model closes at 5,280 Computing, 21 Rack
III, 210 coolant, 616 workers, and 834 Maintenance III per month before policy
effects. The federated topology reduces those values to 4,768 Computing, 19 Rack
III, 190 coolant, 568 workers, and 786 Maintenance III while spending 1 MW more.
The simulator also evaluates 105, 210, and 420 Stream per Package, 128 and 256
Computing per Gateway, and 2 and 4 MW per Gateway.

## Deployment assurance density

The Deployment Assurance Campus converts 16 Model Archives, 32 Lab Equipment
IV, and 32 Electronics III into 128 Packages every 720 seconds. Its 640/hour
output equals four standard validators and preserves their exact material ratios.

One Campus supplies 640 Packages/hour for the $4176/7$, approximately 596.57/hour,
mature-core-plus-Control boundary. Two Campuses supply 1,280/hour for the
$8656/7$, approximately 1,236.57/hour, Center boundary. Standard validators remain
the lower-capital bootstrap and incremental choice rather than a mandatory trim
line. Each Campus uses 4 MW, 256 Computing, 16 Maintenance III/month, and 1,200
Construction Parts IV plus additional endgame capital. It is a density choice,
not a material-efficiency upgrade.

## Electronics III correction

The Precision Components Fab adds the exact four-copy Assembly V row: 8
Microchips plus 16 Electronics II to 8 Electronics III every 20 seconds. It
produces 1,440 Electronics III per hour and consumes no Stream or Packages.
A representative 4,720/hour mature load requires four Fabs, seven Throughput AI
Electronics Cells, or fourteen Assembly V lines. The Fab trades much higher
Microchip, power, Computing, and maintenance demand for compact production with
no Package delivery or Waste handling.

## Simulation method

The economy model counts active machine instances rather than summing mutually
exclusive recipe rows. It closes whole Rack III counts, rack power and coolant,
validator Model and Dataset support, Dossier science support, workers,
maintenance, and Orbital Arrays.

Two power boundaries are useful:

- **Terrestrial grid:** universal Direct load is approximately 436 MW; optimized
  operation across every custom owner is approximately 736 MW.
- **Orbital self-sufficient:** closing Array and Dossier support raises those loads
  to approximately 442 MW and 744.8 MW.

The deliberately unrealistic all-content stress case still requires 34 Rack III,
seven validators, and three to four Orbital Arrays. It is a support-explosion test,
not a prescribed factory layout.

Run the standard-library model from the repository root:

```powershell
python tools/simulate_recursive_industry_economy.py
python tools/simulate_recursive_industry_economy.py --power-basis terrestrial
python tools/simulate_recursive_industry_economy.py --json
```

## Open judgment

Integrated play still needs to determine whether three to four racks per branch,
bulk Campus plus standard trim validation, and 415 to 683 MW feel substantial
without becoming repetitive infrastructure ceremony. Those are player-experience
questions, not facts the offline model can settle.

## Composition boundaries

The food portfolio integrates ordinary Mill, Baking Unit, Food Processor, and
Assembly V stages. Egg recipes begin with Eggs. Poultry recipes begin with Chicken
Carcass and preserve Animal Feed and Meat Trimmings. Chicken Farm remains separate
because it is a biological producer with Eggs and Carcass co-products.

Integrated Mechanical Parts composes iron smelting, steelmaking, casting, and
assembly to produce physical Mechanical Parts. Maintenance I to III remain native
MaintenanceDepotProto services; Stream does not manufacture virtual Maintenance.

Raw Electronics III composes Electronics, PCB, Electronics II, and Electronics
III assembly. Lab Equipment II through IV progressively compose and cancel their
lower Lab tiers. Raw Electronics IV remains excluded: preserving all of its
independent physical domains would exceed the useful facility shell and remove
too much logistics choice.
