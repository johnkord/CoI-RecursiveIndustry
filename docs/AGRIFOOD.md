# Adaptive Agrifood

## Existing farm-to-plate coverage

Recursive Industry already automates most work after the farm gate: milling,
fermentation, baking, food processing, Food Pack assembly, Crew Supplies,
logistics, water recovery, compost, organic fertilizer, and digestion.

It does not turn fertilizer and water directly into packaged food. Native farms
retain weather, fertility, crop rotation, irrigation, fertilizer choice, growth
time, and harvested crops. Native Chicken Farms retain flock growth, feed, water,
starvation, slaughter controls, Eggs, and Chicken Carcass.

## Precision Irrigation

Precision Irrigation is a five-level Office Focus:

```text
-2% farm water per level
-10% cap
8,000 Focus first level
+4,000 Focus per following level
80,000 Focus to cap
```

It changes no crop yield, settlement water, rain collection, fertilizer use, or
groundwater. Existing Crop Yield research and Focus remain the output choice;
Precision Irrigation addresses one input pressure.

## Native farm upgrades

Adaptive Agrifood Systems extends two native tier chains:

| Upgrade | Workers | Incremental upgrade cost |
| --- | --- | ---: | --- |
| Greenhouse II to Sensor-Guided Greenhouse | 20 to 4 | 64 Electronics IV, 16 Packages, 4 Programs |
| Chicken Farm to Monitored Poultry Farm | 12 to 4 | 32 Electronics IV, 8 Packages, 2 Programs |

Both use the game's native in-place `SetNextTier` path. The existing entity ID,
layout, ports, crop or flock state, buffers, schedules, controls, notifications,
and persistence remain under `Farm` or `AnimalFarm`. Because each target cost is
its source cost plus automation hardware, the native differential upgrade cost is
exactly the added hardware shown above. The upgrades trade late capital for
labor; they do not improve yield, water, fertilizer, feed, growth, footprint,
power, or maintenance.

Native farms remain the lower-capital fallback. Four workers remain for agronomy,
animal care, hygiene, maintenance, and exceptions.

## Circular Agrifood Systems

Circular Agrifood Systems is an optional child of Adaptive Agrifood Systems. It
does not replace poultry or turn Animal Feed into a universal biomass currency.
It adds four explicit, Fiber-free choices to the existing autonomous campuses:

| Recipe | Inputs | Outputs | Time | Process power |
| --- | --- | --- | ---: | ---: |
| Adaptive Egg Fermentation | 32 Animal Feed, 8 Cooking Oil, 24 Oxygen, 16 Water | 28 Eggs, 16 Carbon Dioxide, 12 Waste Water | 60s | 30 MW |
| Serum-Free Cultured Meat | 40 Animal Feed, 8 Sugar, 32 Oxygen, 24 Water, 4 Salt | 20 Meat, 8 Meat Trimmings, 16 Carbon Dioxide, 16 Waste Water | 60s | 36 MW |
| Mycoprotein Trimmings | 32 Animal Feed, 8 Cooking Oil, 24 Oxygen, 16 Water | 36 Meat Trimmings, 16 Carbon Dioxide, 12 Waste Water | 60s | 30 MW |
| Companion Provisions | 60 Animal Feed, 12 Meat Trimmings, 4 Cooking Oil, 8 Water, 4 Plastic | 80 Companion Provisions, 4 Waste Water | 60s | 18 MW |

The synthetic rows represent internal grinding, enzymatic hydrolysis,
sterilization, controlled fermentation or cell culture, separation, and final
formulation. `Eggs` means a whole-egg culinary ingredient assembled from
fermented protein and an oil phase, not a shell egg. `Meat` is cultured mince,
not a structured whole cut. `Meat Trimmings` is the easiest processed biomass
and fat blend, useful for Sausage or digestion.

The Food Processing Campus retains 24 workers and 128 Computing. That is an
intentional operating cost for media characterization, contamination control,
adaptive feeding, separation, and quality assurance. The Crop and Soil Center
retains 16 workers and 96 Computing. No recipe consumes Industrial Control
Stream, Packages, Programs, or Dossiers per batch.

### Why poultry remains useful

Four Chicken Farms consume 60 Animal Feed and 72 Water per minute and jointly
produce 28 Eggs plus 40 Chicken Carcass. Those carcasses can become 20 Meat plus
8 Meat Trimmings. Poultry therefore wins when the player values both outputs and
has water, land, and animal-care capacity. Synthetic rows win when one output is
needed without its coupled Egg or carcass stream, but pay much more power and
industrial input complexity.

## Companion animal care

Companion Provisions is a countable, packaged consumer product. The attached
Companion Animal Center is an optional native settlement service:

```text
0.02 Companion Provisions / colonist / month
0.004 Waste / colonist / month
250 kW
8 workers
0.6 Unity at full satisfaction
no Health or worker-productivity bonus
```

One 80-Provision batch supports 4,000 colonists for one month. A 1,000-person
settlement consumes 20 Provisions, embedding 15 Animal Feed, 3 Meat Trimmings,
1 Cooking Oil, 2 Water, and 1 Plastic per month. This is consequential but not a
universal Feed sink: the Integrated Plant Food Pack row alone produces 33.75
Animal Feed per minute, more than one 1,000-person service consumes.

Supply failure removes only this service's Unity. It does not block housing,
harm population, disable workers, or simulate pet mortality. Care workers,
electricity, packaging, Waste, and the modest reward represent both benefits and
responsibilities. The implementation uses standard `PopNeedProto` and
`SettlementModuleProto` behavior and adds no custom saved state.

## Existing Feed value ladder

The new rows supplement rather than remove the existing outlets:

```text
12 Animal Feed -> 6 Compost / 60s
6 Compost + 12 Dirt + 6 Water -> 24 Fertilizer (Organic) / 20s
12 Animal Feed + 8 Water -> 8 Steam (High) + 6 Exhaust / 10s
6 Animal Feed -> 2 Air Pollution / 10s
```

Poultry and synthetic ingredients are high-value conversion, companion care is
population-scaled discretionary demand, fertilizer closes nutrients, steam is
an emergency energy outlet, and burning is the visibly poor last resort.

No Sugar-fed or Fuel-Gas-fed Animal Feed recipe is included. If a future
resilience route converts 64 Fuel Gas into 60 Feed, the current worst recovery
path returns at most 52.5 Fuel Gas from those 60 Feed, before Oxygen, power, and
other inputs. That hypothetical route still requires a complete graph audit.

Aquaculture and insect conversion remain deferred. Both could use Feed
credibly, but an honest implementation needs a biological entity with growth,
water, mortality, care, and persistence rather than another ordinary recipe.

## Explicit boundaries

Adaptive Agrifood adds no:

- automatic crop scheduling;
- farm-to-Food-Pack mega-recipe;
- crop-yield bonus;
- zero-worker biological production;
- Industrial Control Stream or direct Computing input;
- custom farm runtime service or saved state;
- traceability commodity or blockchain token;
- spoilage, recall, contamination, or cold-chain simulation; or
- automatic deletion of Animal Feed, Meat Trimmings, Compost, or other residuals.

Circular Agrifood additionally adds no synthetic Chicken Carcass, direct
synthetic Food Pack, automatic recipe fallback, gas-fed Feed, recurring Package,
direct Health, or worker-productivity bonus.

Captain of Industry 0.8.7 has no native food-lot, contamination, recall,
spoilage, refrigeration, or product-temperature state. Those labels would add
cost without a simulated failure they can prevent.

## Verification

The compact authorities are `data/adaptive-agrifood.json` and
`data/circular-agrifood.json`. Run:

```powershell
python tools/audit_recursive_industry_agrifood.py
python tools/audit_recursive_industry_circular_agrifood.py
python tools/validate_public_repo.py
python -m unittest discover -s tests -p "test_*.py"
```

Offline checks own source parity, exact vectors, balance comparisons, cycle
guards, and generated presentation. Runtime still needs one integrated
fresh-world session covering registration, representative recipe execution,
settlement service satisfaction/starvation, the two native upgrades, and
fresh-process persistence.

## Sources

- FAO, *Precision fermentation: With a focus on food safety*, 2025:
	<https://doi.org/10.4060/cd4448en>.
- Aro et al., "Production of bovine beta-lactoglobulin and hen egg ovalbumin by
	Trichoderma reesei using precision fermentation technology," 2023:
	<https://doi.org/10.1016/j.foodres.2022.112131>.
- Humbird, "Scale-up economics for cultured meat," 2021:
	<https://doi.org/10.1002/bit.27848>.
- Combe et al., "NMR metabolomics of plant and yeast-based hydrolysates for cell
	culture media applications," 2024:
	<https://doi.org/10.1016/j.crfs.2024.100855>.
- Ho et al., "Applications and analysis of hydrolysates in animal cell culture,"
	2021: <https://doi.org/10.1186/s40643-021-00443-w>.
- NIH, "The Power of Pets," 2018:
	<https://newsinhealth.nih.gov/2018/02/power-pets>.
- CDC, "Ways to Stay Healthy Around Animals," updated 2026:
	<https://www.cdc.gov/healthy-pets/about/index.html>.
