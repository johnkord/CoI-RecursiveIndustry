# Original Building Family

This is the original source for 48 building models in the 0.26.0a successor.
The four accepted landmark models remain in [Reconstruction](../Reconstruction/README.md).
No game meshes, textures, prefabs, or game DLLs are redistributed here.

The [catalog](../../../data/building-models.json) owns exact assignments and
envelopes. [AllBuildingAssets](Editor/AllBuildingAssets.cs) composes original
geometry and three detail levels, authors native picking, emission, sign and
animation hooks, and validates the final bundled prefabs. Unity import identities
and generated source assets are retained in `UnitySource` for reproducibility.

[asset-manifest.json](asset-manifest.json) binds geometry signatures, render and
triangle counts, sampled animation motion, final previews, and all 49 new bundles.
The shared materials and meshes have one bundle; each building has a prefab bundle.
The complete player mod contains 57 bundles, including the eight existing bundles.

Build instructions and review sheets are in [Building Artwork](../../../docs/BUILDING_ART.md).

## Sources

- Original authored geometry and shaders in this directory.
- [Native graphics declarations](../../../mods/RecursiveIndustry/src/AdaptiveAgrifoodData.cs).
- [Verified inventory guard](../../../tools/audit_building_models.py).