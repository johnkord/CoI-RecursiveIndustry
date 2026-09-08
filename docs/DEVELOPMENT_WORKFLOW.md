# Development Workflow

## Source of truth

The public `CoI-RecursiveIndustry` repository is the canonical source for:

- player C# code and stable prototype IDs;
- manifest, configuration, player readme, and changelog;
- original source art and shipped asset bundles;
- the compact universal-industry catalog and generated C# snapshots;
- the public economy simulator, tests, documentation, and packaging tools; and
- GitHub and COI Hub release archives.

The private `coi-mods` repository remains the authority for:

- game-derived recipe, ID, and API data;
- decompilation and compatibility research;
- ValidationMod and proof fixtures;
- detailed positive and negative evidence;
- private tester responses and uncoached test rubrics; and
- historical immutable candidate records.

Do not independently edit the same gameplay source in both repositories. That
creates silent divergence and makes it unclear which commit owns a release.

## Normal change

1. Work in this public repository, using a branch when the change workflow calls for one.
2. Change player source, public models, art, and docs here.
3. Run the public offline suite and local game build.
4. When exact game-derived data is needed, query or export it from the private
   repository without copying broad datasets into this repository.
5. Run private compatibility/evidence checks against the public branch or commit.
6. Bring back only conclusions, compact public authorities, or player-source
   fixes that are safe to publish.
7. Merge the public pull request. Record its commit in private evidence when the
   change closes a game-bound claim.

## Moving an internal prototype into public source

Occasionally a risky idea may begin as a quarantined private proof. Promote it
only after its architecture is selected:

1. Inventory the exact player files and stable IDs that belong in the mod.
2. Port the smallest implementation into a public branch. Do not copy
   ValidationMod, logs, saves, decompiled source, API snapshots, raw recipe/ID
   exports, or private test responses.
3. Re-express any required game-derived table as a compact, reviewed public
   authority, as `data/universal-industry-catalog.json` does.
4. Add an offline guard that fails if the compact authority and generated source
   disagree.
5. Run `python tools/validate_public_repo.py` before committing.

There is intentionally no automatic whole-repository sync. The two repositories
have different legal, privacy, and evidence boundaries.

## Release flow

One public commit owns one candidate package. Build and package from a clean
checkout of that commit, then record its DLL and ZIP hashes in private release
evidence. Do not rebuild or overwrite an archive after testers receive it.

The hosted playtest is `v0.22.0c`; preserve its exact archive and tag. The current
source candidate is `0.28.0a`, with complete original art, independent research,
and bounded conversion decision support. Earlier 0.24 through 0.27 candidate
archives and evidence are immutable. Do not rebuild a tested candidate or rewrite
its original evidence to fit a successor.

Builds do not deploy by default. A release requires a committed source identity,
clean-checkout reproducibility, exact archive/deployment parity, and the acceptance
gates in [Verification](VERIFICATION.md). New-campaign-only support is explicit;
unsupported migration is not a gate to advertise as passed. Commit and package
preparation do not authorize pushing, tagging, or publishing.

After a public release:

- fixes begin from the released public tag;
- the private repository imports the public commit identity, not a hand-copied
  source fork; and
- game-update research remains private until its minimal compatibility change is
  ready for a public branch.

## Sources

- [Release policy](../data/release-policy.json).
- [Build project](../mods/RecursiveIndustry/RecursiveIndustry.csproj).
- [Publishing boundary](PUBLISHING.md).