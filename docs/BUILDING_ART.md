# Complete Building Artwork

The unpublished 0.26.0a successor replaces the remaining 48 building shells with
original artwork. Together with the four reconstruction models, all 52 mod-owned
building prototypes have their own model. The currently hosted 0.22.0c playtest
remains unchanged.

## Coverage

| Building group | Models |
| --- | ---: |
| Universal process facilities | 25 |
| Other production, research, control, and Civic machines | 18 |
| AI Operations offices and Planetary Center | 4 |
| Native-family farms | 2 |
| Settlement services | 2 |
| Orbital Power Array | 1 |

Process equipment distinguishes furnaces, casting, electrolysis, distillation,
fermentation, water treatment, cleanrooms, fabrication, and research. Office
tiers increase in height and mass. Greenhouse roofing stays translucent so the
native crop renderer can remain visible.

Vehicles, trains, Fiber and conveyor structures, rack products, and vanilla Data
Center shells are not part of this building-replacement batch. Existing product
and UI artwork remains unchanged.

## Runtime Contracts

No layout, port, recipe, quantity, duration, cost, research gate, product ID, or
simulation state is changed. Farm graphics retain native origins, crop positions,
sprinkler declarations, and biological behavior. Poultry and Companion ventilation
use the native looping animation state; the mod adds no simulation controller.

The four earlier models gain root picking colliders and compatible native
emission materials. The completed author's log exposed those integration defects
despite positive visual and Civic feedback. That feedback is inherited only for
what was reported, not treated as a clean-log, persistence, or stable-release pass.

The 48 new models use three strictly reducing LODs and at most three renderers per
visible level. Each has a root collider and native emission hook. Machines that
already had a recipe sign retain its renderer and product-texture shader inputs.
Geometry bounds, final bundle contents, shader properties, and native animation
motion are checked offline. Triangle budgets are not an in-game FPS claim.

## Build And Inspect

Use Unity 6000.0.66f1 and the prepared official modding project. Close that editor
before the synchronous batch build. These commands do not deploy the game mod:

```powershell
& tools/build_reconstruction_assets.ps1
& tools/build_all_building_assets.ps1
& tools/render_building_contact_sheets.ps1
python tools/generate_building_model_paths.py
python tools/audit_building_models.py
python tools/validate_public_repo.py
```

Run both asset builders in that order to produce the composite 57-bundle player
manifest. Only original source assets and their preserved Unity import identities
are used. Previews are rendered from the final loadable bundles, avoiding cached
editor-mesh previews. Do not rebuild a frozen, already-tested archive in place.

- [Research and core industry](../art/RecursiveIndustry/Buildings/previews/contact-sheet-01.png).
- [Offices, farms, metallurgy, and refining](../art/RecursiveIndustry/Buildings/previews/contact-sheet-02.png).
- [Chemistry, utilities, nuclear fuel, and fabrication](../art/RecursiveIndustry/Buildings/previews/contact-sheet-03.png).

Ordinary in-game visual feedback remains the author boundary. There is no new
Civic mechanic to replay and no requirement to repeat every production chain.
This candidate is not a new public migration or stable-release claim.

## Sources

- [Exact building catalog](../data/building-models.json).
- [Original generator and asset receipt](../art/RecursiveIndustry/Buildings/README.md).
- [Coverage and negative-case guard](../tools/audit_building_models.py).
- [Native farm clone declarations](../mods/RecursiveIndustry/src/AdaptiveAgrifoodData.cs).
- [Civic service declarations](../mods/RecursiveIndustry/src/CivicKnowledgeData.cs).