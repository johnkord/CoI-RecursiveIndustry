# Publishing

Recursive Industry is not yet declared stable. Use this document to prepare the
GitHub and COI Hub release after the integrated author and independent player
gates pass.

The current source is unpublished 0.28.0a. Source commits, local packaging,
deployment, and automated native registration do not themselves authorize a
GitHub/Hub publication or close independent acceptance.

## Current public pre-release

GitHub prerelease `v0.22.0c` is the current ordinary-playtesting candidate:

- release: `https://github.com/johnkord/CoI-RecursiveIndustry/releases/tag/v0.22.0c`;
- player ZIP: 1,994,306 bytes;
- SHA-256: `F7212C437318DB39F4997B6B3D4B984BFB3D4FC9B92015860D1F11F516A7ACD9`;
- status: pre-release, not COI Hub stable publication.

Version 0.22.0c targets exact Captain of Industry 0.8.7a Build 614 and passed its
clean fresh-world startup and strict-log gate. Do not replace, relabel, or
rebuild its hosted asset. Version 0.22.0b remains preserved as its predecessor.

The `v0.19.0d` GitHub release was retired when 0.22.0b became the public
playtest; its source tag and private evidence remain preserved. Version 0.19.0c
remains immutable superseded history. Any further fix creates a new version,
tag, and release asset.

Version 0.22.0b is for new campaigns only. Author gameplay judgment passed on
the source-equivalent 0.22.0a build, but
an old placed-machine save was degraded by incompatible legacy molten-output
connections. Clean fresh-world and independent evidence remain required for 1.0.

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
> Choose independent applications in autonomous freight, heavy equipment,
> locomotives, Microchip consolidation, civic services, planetary coordination,
> orbital science, and Frontier megaprojects. Twenty-five specialist
> facilities transform materials, refining, chemistry, food, utilities,
> nuclear fuel, and advanced manufacturing.
>
> Choose conventional production or high-capital Direct, Integrated, and
> Precision routes. Direct and Precision operation remain local and Fiber-free;
> Integrated modes consume live Industrial Control Stream over Access or Backbone Fiber.
> Efficient production spends more power rather than erasing
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

1. Complete ordinary-use judgment and classify actual findings without replaying inherited mechanics.
2. Apply demonstrated fixes in a named successor. Commit and freeze its complete source.
3. Build and package that commit in two clean locations; require byte-identical DLL and ZIP.
4. Bind all gates, source, game identity, and artifacts without rebuilding after distribution.
5. Test the frozen archive in independent uncoached new campaigns.
6. Resolve release-gate findings before a stable decision. A later `1.0.0` metadata
  successor gets its own archive and identity; inherit unaffected behavior explicitly.
7. Run:

   ```powershell
   python tools/generate_recursive_industry_universal_source.py
  python tools/audit_recursive_industry_control_network.py
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
- Accept and publish the final 1.0 COI Hub listing.

## Sources

- [Release policy](../data/release-policy.json).
- [Verification boundaries](VERIFICATION.md).
- [Package builder](../tools/package_mod.py).
