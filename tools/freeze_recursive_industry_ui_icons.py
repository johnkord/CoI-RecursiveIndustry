#!/usr/bin/env python3
"""Freeze Recursive Industry UI icon art and bundle metadata."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
ART_ROOT = ROOT / "art" / "RecursiveIndustry" / "UiIcons"
SOURCE_PATHS = (
    ROOT / "mods" / "RecursiveIndustry" / "src" / "RecursiveIndustryIcons.cs",
    ROOT / "mods" / "RecursiveIndustry" / "src" / "UniversalIndustryIcons.g.cs",
)
GENERATOR_PATH = ROOT / "tools" / "generate_recursive_industry_ui_icons.ps1"
BUNDLE_NAME = "uiicons_5287"
BUNDLE_PATH = ROOT / "mods" / "RecursiveIndustry" / "AssetBundles" / BUNDLE_NAME
UNITY_MANIFEST_PATH = ART_ROOT / "unity" / f"{BUNDLE_NAME}.manifest"
OUTPUT_PATH = ART_ROOT / "asset-manifest.json"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def relative(path: Path) -> str:
    return path.relative_to(ROOT).as_posix()


def parse_ui_icon_constants(text: str) -> dict[str, str]:
    root_match = re.search(r'private const string Root = "([^"]+)";', text)
    if not root_match:
        raise ValueError("RecursiveIndustryIcons.Root is missing")
    icon_root = root_match.group(1)
    return {
        member: icon_root + filename
        for member, filename in re.findall(
            r'public const string (\w+) = Root \+ "([^"]+\.png)";',
            text,
        )
    }


def parse_unity_manifest(path: Path) -> tuple[str, int, set[str], list[str]]:
    text = path.read_text(encoding="utf-8")
    unity_match = re.search(r"^UnityVersion:\s*(\S+)$", text, flags=re.MULTILINE)
    crc_match = re.search(r"^CRC:\s*(\d+)$", text, flags=re.MULTILINE)
    assets = set(re.findall(r"^- (Assets/[^\r\n]+)$", text, flags=re.MULTILINE))
    dependency_match = re.search(
        r"^Dependencies:\s*\[(.*?)\]$", text, flags=re.MULTILINE
    )
    if not unity_match or not crc_match or dependency_match is None:
        raise ValueError(f"Unity bundle manifest is incomplete: {path}")
    dependencies = [
        value.strip()
        for value in dependency_match.group(1).split(",")
        if value.strip()
    ]
    return unity_match.group(1), int(crc_match.group(1)), assets, dependencies


def file_record(path: Path) -> dict[str, object]:
    return {
        "path": relative(path),
        "size_bytes": path.stat().st_size,
        "sha256": sha256(path),
    }


def build_manifest(generated_date: str) -> dict[str, object]:
    constants = parse_ui_icon_constants(
        "\n".join(path.read_text(encoding="utf-8") for path in SOURCE_PATHS)
    )
    if len(constants) != 85:
        raise ValueError(f"expected 85 UI icon constants, found {len(constants)}")

    unity_version, crc, assets, dependencies = parse_unity_manifest(
        UNITY_MANIFEST_PATH
    )
    expected_assets = set(constants.values())
    if assets != expected_assets:
        raise ValueError(
            f"Unity asset list drift: {sorted(assets)} != {sorted(expected_assets)}"
        )
    if dependencies:
        raise ValueError(f"UI icon bundle has dependencies: {dependencies}")

    icons: list[dict[str, object]] = []
    for member, unity_path in constants.items():
        name = Path(unity_path).stem
        master = ART_ROOT / "masters" / f"{name}-master.png"
        export = ART_ROOT / "exports" / f"{name}.png"
        icons.append({
            "name": name,
            "constant": member,
            "unity_path": unity_path,
            "master": file_record(master),
            "export": file_record(export),
        })

    return {
        "schema_version": 1,
        "generator": "Deterministic System.Drawing flat-symbol generator",
        "generator_path": relative(GENERATOR_PATH),
        "generator_sha256": sha256(GENERATOR_PATH),
        "generated_date": generated_date,
        "unity_version": unity_version,
        "visual_review": {
            "status": "PASS_STATIC_PROOF_REVIEW",
            "runtime_status": "OPEN_PUBLIC_PLAYTEST_0.22.0B",
            "tested_sizes_px": [24, 32, 48],
            "backgrounds": ["light", "dark"],
            "size_proof": file_record(
                ART_ROOT / "proofs" / "all-ui-icons-size-proof.png"
            ),
            "grayscale_proof": file_record(
                ART_ROOT / "proofs" / "all-ui-icons-grayscale-proof.png"
            ),
        },
        "bundle": {
            "name": BUNDLE_NAME,
            "path": relative(BUNDLE_PATH),
            "size_bytes": BUNDLE_PATH.stat().st_size,
            "sha256": sha256(BUNDLE_PATH),
            "crc": crc,
            "dependencies": dependencies,
            "unity_manifest": relative(UNITY_MANIFEST_PATH),
            "unity_manifest_sha256": sha256(UNITY_MANIFEST_PATH),
        },
        "icons": icons,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--generated-date", required=True)
    args = parser.parse_args()

    manifest = build_manifest(args.generated_date)
    OUTPUT_PATH.write_text(
        json.dumps(manifest, indent=2) + "\n",
        encoding="utf-8",
    )
    print(
        f"Recursive Industry UI icon manifest: {OUTPUT_PATH} "
        f"({len(manifest['icons'])} icons, {manifest['bundle']['name']})"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())