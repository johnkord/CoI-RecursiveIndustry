# Complete World Artwork

The unpublished 0.27.0a candidate completes the remaining mod-owned world models.
It retains all 52 building models and adds 55 original prefab assets plus two
original extruded Fiber profiles. It is not a replacement for the hosted 0.22.0c
playtest until a separate release is approved.

## Coverage

| Family | New original models |
| --- | ---: |
| Later transported products | 6 |
| Installed accelerator-rack generations | 3 |
| Fiber ports, closed ports, junction, mount, vertical connector, and flow pieces | 13 |
| Autonomous road and working vehicles | 8 |
| Tank, flatbed, and dump attachments | 8 |
| Locomotives, tenders, and the three-car nuclear consist | 17 |

The six cargo products are Experiment Programs, Validated Research Dossiers,
Frontier Programs, Frontier Expansion Projects, Orbital Power Calibration, and
Companion Provisions. Their UI icons are unchanged. Original mesh-family cargo
uses all five LOD slots, readable meshes, and complete PBR texture maps. Existing
native packing, size, orientation, and prefab-versus-mesh representation remain.

Rack cabinets retain the native four-panel visual controls. Vehicles retain
wheel and track controls, working-arm states, headlights, load signs, dump-body
and fill-level animations. Trains retain bogies, couplers, numbered signs, native
wheel/throttle controls where present, and tender fill states. No new controller
runs independently of native vehicle operation.

Access and Backbone Fiber have distinct original cable profiles. Original PBR
materials and hardware are consumed by the native transport renderer, which
continues to own routing, transforms, coloring, highlighting, and flow animation.
The mod does not implement a replacement transport shader runtime or packet engine.

## Preserved Behavior

Recipes, research, costs, capacity, fuel, speed, working timings, storage, biological
state, Civic behavior, and player configuration are unchanged. All existing
building, product, vehicle, and research IDs remain. Three new mod-owned amphibious
attachment IDs isolate their artwork from native trucks while retaining eligible
products, attachment order, cargo offsets, fill settings, and the empty-load choice.

That attachment distinction is not a save-migration guarantee. New campaigns remain
the public support baseline. The candidate preserves 0.26.0a archives and does not
modify saves during installation.

Native Data Center shells, ordinary game buildings, shared transport support
structures, standard cargo wagons, sound, and generic effects are deliberately
reused. They are not unfinished mod-specific models.

## Verification

The asset generator checks final bundled prefabs, shader compilation, required rig
nodes, readable meshes, native Fiber mesh/material extraction, train PPM LOD naming,
and sampled working animations. It renders all models and ten truck-load/train
assemblies from the actual player bundles. Separate image pairs prove that native
locomotive numbers, track offsets, and headlight strengths change visible pixels.

The production Fiber and cargo helpers are compiled against the installed MaFi
types and executed inside Unity. Standalone .NET Framework cannot execute every
MaFi collection method; it is a compile target, not the matching game runtime.

```powershell
& tools/build_world_assets.ps1
python tools/refresh_world_art_metadata.py --write
& tools/render_world_contact_sheets.ps1
& tools/test_world_native_policy.ps1
python tools/audit_world_art.py
python tools/validate_public_repo.py
python -m unittest discover -s tests -p "test_*.py"
```

Use Unity 6000.0.66f1 and the prepared official modding project. The asset build
does not deploy and copies only original world bundles. Do not rebuild the ready
candidate or any preserved predecessor merely to repeat a check.

Offline rendering and source equivalence do not prove in-game visual preference,
performance, save migration, or independent release acceptance. No repeated Civic,
production, farming, or vehicle-mechanics checklist is required for this art batch.

## Review Sheets

- [Cargo, installed racks, and Fiber hardware](../art/RecursiveIndustry/World/previews/contact-sheet-01.png).
- [Fiber flow pieces and autonomous vehicles](../art/RecursiveIndustry/World/previews/contact-sheet-02.png).
- [Load attachments and locomotives](../art/RecursiveIndustry/World/previews/contact-sheet-03.png).
- [Remaining trains and composed loads](../art/RecursiveIndustry/World/previews/contact-sheet-04.png).
- [Nuclear consist](../art/RecursiveIndustry/World/previews/contact-sheet-05.png).

## Sources

- [Exact original-art catalog](../data/world-art.json).
- [Original source and asset manifest](../art/RecursiveIndustry/World/README.md).
- [Full-art regression guard](../tools/audit_world_art.py).
- [Production Fiber graphics](../mods/RecursiveIndustry/src/FiberGraphics.cs).
- [Native attachment-preserving clones](../mods/RecursiveIndustry/src/WorldAttachmentGraphics.cs).
- [Real-MaFi graphics policy fixture](../tests/WorldArt.Policy/PolicyFixture.cs).