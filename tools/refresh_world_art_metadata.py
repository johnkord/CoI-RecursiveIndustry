#!/usr/bin/env python3
"""Bind the shared player bundle list without rebuilding legacy artwork."""

import argparse
import hashlib
import json
from pathlib import Path

from bundle_manifest import bundle_files

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "art/RecursiveIndustry/ProductModels/production/v1-cartridge-family/asset-manifest.json"
PLAYER = ROOT / "mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args()
    value = json.loads(MANIFEST.read_text(encoding="utf-8"))
    payload = PLAYER.read_bytes()
    expected = {"path": PLAYER.relative_to(ROOT).as_posix(), "size_bytes": len(payload),
                "sha256": hashlib.sha256(payload).hexdigest().upper(), "bundles": sorted(bundle_files(payload.decode("utf-8")))}
    if args.write:
        value["mafi_manifest"] = expected
        MANIFEST.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    elif value["mafi_manifest"] != expected:
        raise ValueError("Shared player manifest metadata differs; regenerate current metadata only")
    print(f"PASS: shared player manifest binds {len(expected['bundles'])} exact bundles")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())