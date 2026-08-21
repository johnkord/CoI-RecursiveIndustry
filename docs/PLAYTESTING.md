# Playtesting

## Current build

Use the exact GitHub pre-release:

- version: `0.19.0d`;
- release: https://github.com/johnkord/CoI-RecursiveIndustry/releases/tag/v0.19.0d;
- file: `RecursiveIndustry-0.19.0d.zip`;
- size: 1,726,546 bytes;
- SHA-256: `638AE395526DD40ADFE4751CB88EF4DEA43F5DCBA9B6B586BFE2C01FD2AE280E`.

Do not use GitHub's automatic source-code archives as the player package.

Version 0.20.0a is currently an unreleased source candidate. Do not substitute a
local source archive for a source-bound player ZIP. Its integrated Industrial
Control session begins only after build, package, migration, and private successor
preflight gates pass.

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

Pre-release saves containing the removed Orbital Power Relay cannot load normally
with 0.19.0d. Remove the Relay while running 0.19.0c before upgrading, or start a
new campaign.

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

For a future 0.20.0a package, also report whether Fiber topology is a meaningful
planning choice, whether Access and Backbone are distinguishable, whether a cut
is diagnosable, and whether Direct production remains understandable and useful
without Fiber. Also report whether Backbone deployment earns its higher power
cost, whether the Assurance Campus reduces repetition without making standard
validators obsolete, and whether the Campus icon is distinct from the Gateway.
For the Refinery Complex, report which product slate you chose, what demand drove
that commitment, whether broad fractionation remained a credible fallback, and
whether the Polymer or Elastomer power premium was understandable from normal UI.

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
