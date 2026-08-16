#!/usr/bin/env python3
"""Regression tests for the public release ZIP auditor."""

from __future__ import annotations

from pathlib import Path
import sys
import tempfile
import unittest
from zipfile import ZIP_DEFLATED, ZipFile


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from audit_release_zip import (  # noqa: E402
    MOD_ROOT,
    audit_archive,
    expected_entries,
    load_source_manifest,
)


def write_fixture(path: Path, extra: str | None = None) -> None:
    manifest = load_source_manifest()
    entries = expected_entries(manifest)
    with ZipFile(path, "w", compression=ZIP_DEFLATED) as archive:
        for name in entries:
            if name.endswith("/manifest.json"):
                payload = (MOD_ROOT / "manifest.json").read_bytes()
            elif name.endswith("/AssetBundles/mafi_bundles.manifest"):
                payload = (
                    MOD_ROOT / "AssetBundles" / "mafi_bundles.manifest"
                ).read_bytes()
            else:
                payload = b"fixture"
            archive.writestr(name, payload)
        if extra is not None:
            archive.writestr(extra, b"forbidden")


class ReleaseArchiveTests(unittest.TestCase):
    def test_exact_inventory_passes(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "candidate.zip"
            write_fixture(path)
            self.assertEqual(audit_archive(path), [])

    def test_game_dll_is_rejected_as_extra_content(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            path = Path(temp) / "candidate.zip"
            write_fixture(path, "RecursiveIndustry/Mafi.dll")
            errors = audit_archive(path)
            self.assertTrue(any("unexpected files" in error for error in errors))


if __name__ == "__main__":
    unittest.main()
