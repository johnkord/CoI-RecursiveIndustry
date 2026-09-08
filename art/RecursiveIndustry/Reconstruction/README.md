# Reconstruction Assets

Original procedural models and bitmap icons for the 0.25 reconstruction candidate.
The family contains an electronics integration hall, Civic Model Center, Knowledge
Commons, and Planetary Coordination Center. Their declared bounds preserve the
corresponding gameplay footprints; no ports or collision layouts were enlarged.

The [editor generator](Editor/ReconstructionAssets.cs) creates the geometry,
materials, and three civic icons. [IndustrialSurface.shader](IndustrialSurface.shader)
is the original surface shader. `UnitySource/` preserves generated source assets
and import GUIDs for repeatable editor builds. No extracted game meshes, materials,
textures, shaders, or DLLs are included.

Build from the public repository with:

```powershell
./tools/build_reconstruction_assets.ps1
```

The tool requires the configured external ExampleMod Unity project and Unity
6000.0.66f1. It does not launch Captain of Industry or deploy the player mod.
MaFi separates four prefab bundles from the shared reconstruction bundle; the
generated player manifest includes those dependencies. Existing cartridge and
legacy icon bundles are not replaced.

Previews are rendered offline and checked for nonblank pixels. They establish
asset appearance, mesh presence, and declared bounds, not in-game placement,
lighting, construction previews, or performance. Those observations belong to
the integrated candidate test.

## Sources

- Original generator and shader above.
- [Generated asset manifest](asset-manifest.json).
- Unity 6000.0.66f1 editor APIs and MaFi's standard asset-bundle builder.