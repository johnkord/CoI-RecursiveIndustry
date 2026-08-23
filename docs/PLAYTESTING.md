# Playtesting

## Current build

Use the exact GitHub pre-release:

- version: `0.22.0b`;
- release: https://github.com/johnkord/CoI-RecursiveIndustry/releases/tag/v0.22.0b;
- file: `RecursiveIndustry-0.22.0b.zip`;
- size: 1,994,137 bytes;
- SHA-256: `1199B108737A431C9339B285C815C08B026F0A5FBAC64F5D9813B819E4E2293F`.

Do not use GitHub's automatic source-code archives as the player package.

Use only the attached player ZIP. Do not substitute a local build or GitHub's
automatic source archive.

## Install

1. Exit Captain of Industry.
2. Remove or move any older `RecursiveIndustry` folder from
   `%APPDATA%/Captain of Industry/Mods`.
3. Extract the release ZIP into the Mods directory.
4. Confirm the result contains
   `%APPDATA%/Captain of Industry/Mods/RecursiveIndustry/manifest.json`.
5. Start a new single-player campaign with Recursive Industry enabled.

Captain of Industry 0.8.7 and Trains expansion 1.0.0 or newer are required.
Supporter edition is optional. Adding or removing the mod from an existing normal
save is unsupported.

The exact archive rebuilds unchanged against v0.8.7a Build 614, whose reflected
modding surface matches Build 613. Build 614 fresh-world strict-log evidence is
still useful; the completed author continuation used an unsupported older save.

Do not load a `0.19.x`, `0.20.x`, or `0.21.x` pre-release save with 0.22.0b.
Testing an old placed-machine save produced continuous incompatible molten-output
errors because legacy connections no longer match the current universal-facility
port contract. This is an unsupported migration result, not a repair workflow.

## Useful feedback

Play naturally. Do not try to manufacture a specific outcome or prove every item
in one session. Useful reports explain:

- what you were trying to accomplish;
- the product, building, research card, or displayed recipe quantities involved;
- what constraint drove your choice;
- what you expected and what happened instead;
- whether the normal UI explained a shortage or stalled state;
- whether a conventional machine remained a credible fallback;
- whether any icon, port, model, or recipe row was confusing; and
- whether ordinary operation required repeated manual intervention.

For 0.22.0b, also report whether Fiber topology is a meaningful
planning choice, whether Access and Backbone are distinguishable, whether a cut
is diagnosable, and whether Direct production remains understandable and useful
without Fiber. Also report whether Backbone deployment earns its higher power
cost, whether the Assurance Campus reduces repetition without making standard
validators obsolete, and whether the Campus icon is distinct from the Gateway.
For the Refinery Complex, report which product slate you chose, what demand drove
that commitment, whether broad fractionation remained a credible fallback, and
whether the Polymer or Elastomer power premium was understandable from normal UI.

For Circular Agrifood, report whether poultry remains a credible joint Egg and
carcass route, whether synthetic Eggs, Meat, and Trimmings solve distinct
shortages, and whether Companion Animal Care earns its workers, power,
provisions, Waste, and 0.6-Unity reward.

For crashes or registration failures, include:

- exact game and mod versions;
- every enabled mod and DLC;
- reproduction steps; and
- a sanitized log excerpt from `%APPDATA%/Captain of Industry/Logs`.

Use the repository's bug-report template. Remove personal filesystem paths before
posting logs.

## Evidence boundary

Public pre-release feedback is valuable defect discovery. It is not automatically
counted as independent final-candidate evidence because testers may read public
source, issues, and design documentation. The later stable-release gate uses one
frozen archive and separately managed, uncoached campaigns.

Never share personal information, account identifiers, or private saves as part
of a report.
