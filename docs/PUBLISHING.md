# Publishing

Recursive Industry is not yet declared stable. Use this document to prepare the
GitHub and COI Hub release after the integrated author and independent player
gates pass.

## Repository metadata

**Repository:** `johnkord/CoI-RecursiveIndustry`

**Description:**

> A Captain of Industry endgame mod about building a physical AI economy, from accelerator racks and validated models to autonomous industry, planetary coordination, and frontier-scale megaprojects.

**Topics:**

```text
captain-of-industry
captain-of-industry-mod
coi-modding
csharp
game-mod
factory-game
endgame
automation
```

## COI Hub listing

**Title:** Recursive Industry

**Development status:** Stable only after the final 1.0 player gates pass. Use
Beta for any earlier public preview.

**Short description:**

> Build a physical AI economy with accelerator racks, validated models, autonomous logistics, high-power megafactories, planetary coordination, and orbital industry.

**Suggested categories:** Content, Production, Research, Vehicles, Power, World
map, Endgame.

**Long description:**

> Recursive Industry extends a mature island beyond vanilla Computing and
> research. Manufacture accelerator hardware, curate physical datasets, train
> models, run experiments, and validate renewable control packages before
> deploying automation.
>
> Progress through finite Recursive Epochs into autonomous freight, heavy
> equipment, locomotives, lights-out microchips, planetary coordination, orbital
> science, and frontier-scale industrial projects. Nineteen specialist
> megafacilities transform materials, refining, chemistry, food, utilities,
> nuclear fuel, and advanced manufacturing.
>
> Choose conventional production or high-capital Direct, Integrated, and
> Precision routes. Efficient production spends more power rather than erasing
> costs: raw materials, electricity, Computing, maintenance, logistics,
> validation, and selected human work remain part of the planning problem.
>
> Requires the Trains expansion. Supporter edition is optional. Start a new
> campaign with the mod enabled. No network connections.

**License:** COI-Open Version 1.0.

**Source and support:**
`https://github.com/johnkord/CoI-RecursiveIndustry`

## Media

Use `media/hub-thumbnail.png` as the initial square thumbnail and
`media/social-preview.png` as the repository social image. Replace or supplement
them with real in-game screenshots before the stable listing:

1. Research tree showing the complete branch without overlap.
2. Data Center with accelerator racks and surrounding support district.
3. Model and validation production with visible product identities.
4. One large megafacility with ports visibly on the building body.
5. Direct versus Precision recipe rows with displayed quantities.
6. Autonomous road or rail logistics in ordinary operation.
7. Planetary Coordination Center and its Package/Computing support.
8. Orbital Power Array or Frontier Project complex.

Do not use sandbox-only compositions as the only gameplay media.

## Final release sequence

1. Complete the integrated author campaign and classify findings.
2. Apply only demonstrated fixes and set the manifest to `1.0.0`.
3. Build, validate, and package one exact archive.
4. Commit the source and require a post-commit deterministic rebuild.
5. Test that frozen archive in independent uncoached campaigns.
6. Resolve release-gate findings without adding new scope.
7. Run:

   ```powershell
   python tools/generate_recursive_industry_universal_source.py
   python tools/validate_public_repo.py
   python -m unittest discover -s tests -p "test_*.py"
   dotnet build mods/RecursiveIndustry/RecursiveIndustry.csproj -c Release `
     /p:DeployToModsFolder=false
   python tools/package_mod.py mods/RecursiveIndustry
  python tools/audit_release_zip.py
   Get-FileHash dist/RecursiveIndustry-1.0.0.zip -Algorithm SHA256
   ```

8. Extract the ZIP into an empty Mods directory and smoke-test those exact bytes.
9. Tag the source commit `v1.0.0` and attach the ZIP plus SHA-256 to a GitHub
   Release. Do not use GitHub's automatic source archive as the player package.
10. Upload the same ZIP to COI Hub, select COI-Open, review automated code
    analysis, and verify dependency/version presentation.
11. Install the Hub-hosted package through the game and perform a final fresh-world
    smoke test.

## Human actions that cannot be automated here

- Accept the COI Hub terms and COI-Open selection through the author's account.
- Complete the campaign judgments and independent player sessions.
- Capture and approve final in-game screenshots.
- Press **Publish** on the GitHub Release and COI Hub listing.
