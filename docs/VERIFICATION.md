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

Pre-release `0.19.0c` passed Array-only research inspection but retained a hidden
Relay prototype and is superseded. Pre-release `0.19.0d` removes the prototype,
stable ID, and buildable entity entirely. Its exact archive passes fresh-world registration
check on Captain of Industry 0.8.7 Build 613. Unlock All Research exposed only the
240 MW Orbital Power Array; the Relay was absent from the toolbar and global
search. The completed log passed strict audit with three known vanilla warnings
and no errors, fatals, unknown warnings, missing assets, port mismatches, or
overflows. The tested player ZIP SHA-256 is
`638AE395526DD40ADFE4751CB88EF4DEA43F5DCBA9B6B586BFE2C01FD2AE280E`.
The closed clean-world smoke log is 40,646 bytes with SHA-256
`8BA6E85996A74868251A829D611C8FB2AC5A69407EEBF4B05C796D2298E82712`.

## Remaining before stable 1.0

- One complete integrated author campaign across the final scope.
- Resolution of demonstrated gameplay, balance, copy, presentation, or
  compatibility findings.
- Independent uncoached campaigns on one exact frozen archive.
- Final clean-install, hash, compatibility, and Hub-hosted smoke checks.

Ordinary reports from the public pre-release are useful for defect discovery but
do not automatically count as independent, uncoached final-candidate evidence.

Private tester responses and detailed expectation rubrics are intentionally not
published before those sessions because doing so would coach discovery.
