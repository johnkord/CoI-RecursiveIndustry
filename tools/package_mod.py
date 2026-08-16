#!/usr/bin/env python3
"""Build a deterministic, policy-safe Captain of Industry mod zip."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys
from zipfile import ZIP_DEFLATED, ZipFile, ZipInfo


ROOT = Path(__file__).resolve().parents[1]
FORBIDDEN_DLL_PREFIXES = ("Mafi", "UnityEngine")
FIXED_TIMESTAMP = (2020, 1, 1, 0, 0, 0)


def load_manifest(mod_dir: Path) -> dict:
    path = mod_dir / "manifest.json"
    try:
        manifest = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise SystemExit(f"Manifest not found: {path}") from exc
    except json.JSONDecodeError as exc:
        raise SystemExit(f"Invalid manifest JSON in {path}: {exc}") from exc
    if not isinstance(manifest, dict):
        raise SystemExit(f"Manifest must be a JSON object: {path}")
    return manifest


def resolve_primary_dll(mod_dir: Path, name: str, configuration: str) -> Path:
    candidates = (
        mod_dir / name,
        mod_dir / "bin" / configuration / name,
    )
    for path in candidates:
        if path.is_file():
            return path
    raise SystemExit(
        f"Primary DLL {name!r} was not found in {mod_dir} or "
        f"{mod_dir / 'bin' / configuration}. Build the mod first."
    )


def package_entries(
    mod_dir: Path,
    manifest: dict,
    configuration: str,
    include_symbols: bool,
) -> list[tuple[Path, Path]]:
    mod_id = manifest.get("id")
    if not isinstance(mod_id, str) or not mod_id:
        raise SystemExit("Manifest id must be a non-empty string.")
    if mod_dir.name != mod_id:
        raise SystemExit(
            f"Mod folder {mod_dir.name!r} must match manifest id {mod_id!r}."
        )
    primary_dlls = manifest.get("primary_dlls")
    if not isinstance(primary_dlls, list) or not primary_dlls:
        raise SystemExit("Manifest primary_dlls must be a non-empty list.")

    entries: list[tuple[Path, Path]] = []
    for name in ("manifest.json", "config.json", "readme.txt", "changelog.txt"):
        source = mod_dir / name
        if source.is_file():
            entries.append((source, Path(mod_id) / name))

    for name in primary_dlls:
        if not isinstance(name, str) or not name.lower().endswith(".dll"):
            raise SystemExit(f"Invalid primary DLL entry: {name!r}")
        if name.startswith(FORBIDDEN_DLL_PREFIXES):
            raise SystemExit(f"Refusing to package game or engine DLL: {name}")
        source = resolve_primary_dll(mod_dir, name, configuration)
        entries.append((source, Path(mod_id) / name))
        if include_symbols:
            pdb = source.with_suffix(".pdb")
            if pdb.is_file():
                entries.append((pdb, Path(mod_id) / pdb.name))

    asset_root = mod_dir / "AssetBundles"
    if asset_root.is_dir():
        for source in sorted(path for path in asset_root.rglob("*") if path.is_file()):
            if source.suffix.lower() == ".dll":
                raise SystemExit(f"Refusing to package DLL from AssetBundles: {source}")
            relative = source.relative_to(mod_dir)
            entries.append((source, Path(mod_id) / relative))

    archive_paths = [archive for _, archive in entries]
    if len(archive_paths) != len(set(archive_paths)):
        raise SystemExit("Package contains duplicate archive paths.")
    return entries


def write_zip(output: Path, entries: list[tuple[Path, Path]]) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    with ZipFile(output, "w", compression=ZIP_DEFLATED, compresslevel=9) as archive:
        for source, archive_path in sorted(entries, key=lambda entry: entry[1].as_posix()):
            info = ZipInfo(archive_path.as_posix(), date_time=FIXED_TIMESTAMP)
            info.compress_type = ZIP_DEFLATED
            info.external_attr = 0o100644 << 16
            archive.writestr(info, source.read_bytes(), compress_type=ZIP_DEFLATED, compresslevel=9)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mod", type=Path, help="mod source folder")
    parser.add_argument("--configuration", default="Release")
    parser.add_argument("--output-dir", type=Path, default=ROOT / "dist")
    parser.add_argument("--symbols", action="store_true")
    args = parser.parse_args(argv)

    mod_dir = args.mod
    if not mod_dir.is_absolute():
        mod_dir = ROOT / mod_dir
    output_dir = args.output_dir
    if not output_dir.is_absolute():
        output_dir = ROOT / output_dir

    manifest = load_manifest(mod_dir)
    entries = package_entries(
        mod_dir,
        manifest,
        args.configuration,
        args.symbols,
    )
    output = output_dir / f"{manifest['id']}-{manifest['version']}.zip"
    write_zip(output, entries)
    size = output.stat().st_size
    print(f"Packaged {len(entries)} file(s) to {output} ({size} bytes)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
