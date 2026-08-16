#!/usr/bin/env python3
"""Audit a packaged Recursive Industry release ZIP."""

from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
from zipfile import BadZipFile, ZipFile


ROOT = Path(__file__).resolve().parents[1]
MOD_ROOT = ROOT / "mods" / "RecursiveIndustry"


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def load_source_manifest() -> dict:
    return json.loads((MOD_ROOT / "manifest.json").read_text(encoding="utf-8"))


def expected_entries(manifest: dict) -> set[str]:
    mod_id = manifest["id"]
    bundles = {
        line.strip()
        for line in (MOD_ROOT / "AssetBundles" / "mafi_bundles.manifest")
        .read_text(encoding="utf-8")
        .splitlines()
        if line.strip()
    }
    return {
        f"{mod_id}/manifest.json",
        f"{mod_id}/config.json",
        f"{mod_id}/readme.txt",
        f"{mod_id}/changelog.txt",
        *(f"{mod_id}/{name}" for name in manifest["primary_dlls"]),
        f"{mod_id}/AssetBundles/mafi_bundles.manifest",
        *(f"{mod_id}/AssetBundles/{name}" for name in bundles),
    }


def audit_archive(path: Path) -> list[str]:
    errors: list[str] = []
    manifest = load_source_manifest()
    expected = expected_entries(manifest)
    try:
        with ZipFile(path) as archive:
            names = [entry.filename for entry in archive.infolist() if not entry.is_dir()]
            actual = set(names)
            if len(names) != len(actual):
                errors.append("archive contains duplicate paths")
            unsafe = [
                name
                for name in names
                if PurePosixPath(name).is_absolute()
                or ".." in PurePosixPath(name).parts
                or "\\" in name
            ]
            if unsafe:
                errors.append(f"archive contains unsafe paths: {unsafe}")
            if actual != expected:
                missing = sorted(expected - actual)
                extra = sorted(actual - expected)
                if missing:
                    errors.append(f"archive is missing player files: {missing}")
                if extra:
                    errors.append(f"archive contains unexpected files: {extra}")

            manifest_name = f"{manifest['id']}/manifest.json"
            if manifest_name in actual:
                packaged_manifest = json.loads(
                    archive.read(manifest_name).decode("utf-8")
                )
                if packaged_manifest != manifest:
                    errors.append("packaged manifest differs from source manifest")

            bundle_manifest_name = (
                f"{manifest['id']}/AssetBundles/mafi_bundles.manifest"
            )
            if bundle_manifest_name in actual:
                declared = {
                    line.strip()
                    for line in archive.read(bundle_manifest_name)
                    .decode("utf-8")
                    .splitlines()
                    if line.strip()
                }
                packaged_bundles = {
                    PurePosixPath(name).name
                    for name in actual
                    if name.startswith(f"{manifest['id']}/AssetBundles/")
                    and name != bundle_manifest_name
                }
                if packaged_bundles != declared:
                    errors.append("packaged bundles differ from MaFi manifest")
    except (OSError, BadZipFile, json.JSONDecodeError, UnicodeDecodeError) as exc:
        errors.append(str(exc))
    return errors


def main() -> int:
    manifest = load_source_manifest()
    default = ROOT / "dist" / f"{manifest['id']}-{manifest['version']}.zip"
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path, nargs="?", default=default)
    parser.add_argument("--json", action="store_true")
    args = parser.parse_args()
    path = args.archive if args.archive.is_absolute() else ROOT / args.archive
    errors = audit_archive(path)
    result = {
        "status": "PASS" if not errors else "FAIL",
        "path": path.relative_to(ROOT).as_posix() if path.is_relative_to(ROOT) else str(path),
        "size_bytes": path.stat().st_size if path.is_file() else None,
        "sha256": sha256(path) if path.is_file() else None,
        "errors": errors,
    }
    if args.json:
        print(json.dumps(result, indent=2))
    else:
        print(f"Recursive Industry release archive: {result['status']}")
        if result["sha256"]:
            print(f"  {result['path']} ({result['size_bytes']} bytes)")
            print(f"  SHA-256 {result['sha256']}")
        for error in errors:
            print(f"  ERROR: {error}")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
