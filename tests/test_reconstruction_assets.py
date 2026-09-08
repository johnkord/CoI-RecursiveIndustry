"""Validate original reconstruction assets and dependency-aware player packaging."""

from __future__ import annotations

import json
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from bundle_manifest import bundle_files, parse_bundle_manifest  # noqa: E402


class ReconstructionAssetsTests(unittest.TestCase):
    def test_dependency_lines_are_not_filenames(self) -> None:
        self.assertEqual(bundle_files("+shared_12\nmodel_34\nshared_12\n"), {"shared_12", "model_34"})
        self.assertEqual(parse_bundle_manifest("+shared_12\nmodel_34\n")["model_34"], ("shared_12",))

    def test_bad_dependency_graphs_fail(self) -> None:
        for text in ("+shared", "../outside", "+same\nsame", "root\nroot", "+second\nfirst\n+first\nsecond"):
            with self.subTest(text=text), self.assertRaises(ValueError):
                parse_bundle_manifest(text)

    def test_four_original_models_and_three_icons_are_bound(self) -> None:
        manifest = json.loads((ROOT / "art/RecursiveIndustry/Reconstruction/asset-manifest.json").read_text(encoding="utf-8"))
        self.assertEqual(len(manifest["models"]), 4)
        self.assertEqual(len(manifest["icons"]), 3)
        self.assertEqual(len(manifest["bundles"]), 5)
        for model in manifest["models"]:
            self.assertTrue(model["root_collider"])
            self.assertGreater(model["emissive_renderers"], 0)
            self.assertGreater(model["triangles"], 0)
            self.assertLess(model["triangles"], 2000)
            self.assertGreater(model["renderer_count"], 0)
            self.assertIn(model["asset_path"].lower(), manifest["asset_paths"])
            self.assertTrue((ROOT / model["preview_path"]).is_file())
        self.assertTrue(all(path.startswith("assets/recursiveindustry/reconstruction/") for path in manifest["asset_paths"]))

    def test_center_keeps_native_layout_and_other_office_graphics(self) -> None:
        source = (ROOT / "mods/RecursiveIndustry/src/OfficeBuildingFactory.cs").read_text(encoding="utf-8")
        for token in ("vanillaOffice.Layout", "customPrefabPath ?? graphics.PrefabPath", "customPrefabPath == null && graphics.UseInstancedRendering"):
            self.assertIn(token, source)


if __name__ == "__main__":
    unittest.main()