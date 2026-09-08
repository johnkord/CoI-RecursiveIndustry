# Original World Models

This directory contains the original 0.27.0a cargo, rack, Fiber, vehicle, attachment,
and train artwork. The 55 prefab assets and two Fiber cross-sections complete the
remaining mod-specific world-model scope after the 52-building family.

The [catalog](../../../data/world-art.json) owns identities and authoring dimensions.
The editor sources generate original meshes, PBR textures, shaders, rig controllers,
and final-bundle validation. `UnitySource` preserves generated source assets and
their Unity import identities for reproducible builds. No game artwork is exported
or redistributed. Native API field names and compatibility references are not meshes.

[asset-manifest.json](asset-manifest.json) records 56 new bundles, all model LOD and
renderer counts, native rig nodes, sampled animations, assembled previews, and
native shader-control image pairs. The complete player inventory has 113 bundles,
including 57 byte-identical pre-existing building/product/icon bundles.

Build commands and review sheets are in [Complete World Artwork](../../../docs/WORLD_ART.md).

## Sources

- [Original geometry and bundle generator](Editor/WorldAssets.cs).
- [Original dynamic rigs](Editor/WorldDynamicAssets.cs).
- [Original Fiber hardware](Editor/WorldFiberAssets.cs).
- [Final-bundle assembly and control checks](Editor/WorldAssemblyChecks.cs).
- [Source and artifact guard](../../../tools/audit_world_art.py).