# Verification

Recursive Industry uses automation first, then reserves human play for claims
that code and arithmetic cannot establish.

## Public offline checks

`python tools/validate_public_repo.py` verifies:

- manifest, configuration, ID, dependency, and source inventory;
- package-safe asset-bundle inventory;
- absence of game DLLs, build outputs, logs, archives, and private evidence;
- required policy notices and public documentation; and
- checked-in generated catalog sentinels.

`python tools/package_mod.py mods/RecursiveIndustry` creates a deterministic ZIP
with one `RecursiveIndustry/` root and refuses to include game or engine DLLs.

## Game-bound checks

A complete candidate is built on Windows against the installed game assemblies
and checked for:

- zero-warning compilation;
- clean deployment and package parity;
- fresh-world registration;
- research unlock dispatch;
- bundle loading and product rendering;
- typed ports, recipe completion, power, Computing, and maintenance declarations;
- normal exit and strict log review; and
- save plus fresh-process reload where custom state is involved.

The gameplay source exported to this repository came from a release-preparation
candidate that passed 302 internal automated tests with three expected skips,
both zero-warning builds, deterministic packaging, and deployed file parity.
Those results do not replace the remaining player-experience gates.

Pre-release `0.19.0c` also passes one exact-archive fresh/sandbox registration
check on Captain of Industry 0.8.7 Build 613. Unlock All Research exposed only the
240 MW Orbital Power Array at Orbital Breakthrough; the legacy 30 MW Relay did
not appear. The completed log passed strict audit with two known vanilla warnings
and no errors, fatals, unknown warnings, missing assets, port mismatches, or
overflows. The tested player ZIP SHA-256 is
`ACF2447F36D090E260BCDBA601DF12EDAC7CB1C53446CEF878FA4A75624FC0E5`.

## Remaining before stable 1.0

- One complete integrated author campaign across the final scope.
- Resolution of demonstrated gameplay, balance, copy, presentation, or
  compatibility findings.
- Independent uncoached campaigns on one exact frozen archive.
- Final clean-install, hash, compatibility, and Hub-hosted smoke checks.

Private tester responses and detailed expectation rubrics are intentionally not
published before those sessions because doing so would coach discovery.
