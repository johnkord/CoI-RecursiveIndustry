# Recursive Industry 0.22.0c Build 614 compatibility candidate

This metadata-only successor targets Captain of Industry 0.8.7a Build 614.
Gameplay, C# source, assets, recipe vectors, stable IDs, and new-campaign-only
save policy remain unchanged from published 0.22.0b.

## Compatibility

- Exact target: Captain of Industry 0.8.7a, Build 614.
- Manifest minimum: 0.8.6c.
- Trains expansion 1.0.0 or newer is required.
- Supporter edition 1.1.0 or newer is optional.

Build 614 retains the same reflected ids, console commands, selected public API,
and relevant Machine, Farm, AnimalFarm, settlement-service, product-rendering,
and research paths as Build 613. Rebuilding 0.22.0b against Build 614 reproduced
its DLL and ZIP exactly.

## Publication gate

Version 0.22.0c is not published yet. Before release, the exact candidate must
complete one new sandbox world startup, one Unlock All Research action, normal
exit, and strict audit of the complete log. No recipe execution, building
placement, farm or service operation, checkpoint capture, or save/reload test is
required.

The published 0.22.0b tag, ZIP, release notes, and hashes remain immutable.