"""Exercise complete building coverage and the final visual-contract failures."""

from copy import deepcopy
from pathlib import Path
import sys
import unittest
from unittest.mock import patch


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

import audit_building_models as building_art


class BuildingModelTests(unittest.TestCase):
    def setUp(self) -> None:
        self.catalog = building_art.load_json(ROOT / "data/building-models.json")
        self.manifest = building_art.load_json(ROOT / building_art.ASSET_MANIFEST)

    def test_all_final_artwork_and_assignments_pass(self) -> None:
        self.assertEqual(building_art.audit(), [])

    def test_missing_model_fails(self) -> None:
        self.manifest["models"].pop()
        with self.assertRaisesRegex(ValueError, "51 unique"):
            building_art.validate_records(self.catalog, self.manifest)

    def test_collider_emission_and_sign_fail_closed(self) -> None:
        for field in ("root_collider", "native_emission", "recipe_sign"):
            with self.subTest(field=field):
                changed = deepcopy(self.manifest)
                changed["models"][0][field] = False
                with self.assertRaises(ValueError):
                    building_art.validate_records(self.catalog, changed)

    def test_missing_native_animation_fails(self) -> None:
        for field in ("native_animation", "sampled_animation_motion"):
            with self.subTest(field=field):
                changed = deepcopy(self.manifest)
                next(model for model in changed["models"] if model["key"] == "monitored_poultry_farm")[field] = False
                with self.assertRaisesRegex(ValueError, "animation"):
                    building_art.validate_records(self.catalog, changed)

    def test_unreduced_lod_and_overhang_fail(self) -> None:
        for field, value in (("triangles_per_lod", [100, 100, 100]), ("bounds_center", [40, 4, 0])):
            with self.subTest(field=field):
                changed = deepcopy(self.manifest)
                changed["models"][0][field] = value
                with self.assertRaises(ValueError):
                    building_art.validate_records(self.catalog, changed)

    def test_duplicate_geometry_fails(self) -> None:
        self.manifest["models"][1]["geometry_sha256"] = self.manifest["models"][0]["geometry_sha256"]
        with self.assertRaisesRegex(ValueError, "Duplicate"):
            building_art.validate_records(self.catalog, self.manifest)

    def test_foreign_dependency_and_asset_fail(self) -> None:
        changed = deepcopy(self.manifest)
        changed["dependencies"][0]["dependencies"] = ["base_game_mesh"]
        with self.assertRaisesRegex(ValueError, "Foreign"):
            building_art.validate_records(self.catalog, changed)
        self.manifest["asset_paths"].append("assets/base/borrowed.prefab")
        with self.assertRaisesRegex(ValueError, "Foreign"):
            building_art.validate_records(self.catalog, self.manifest)

    def test_wrong_poultry_origin_fails(self) -> None:
        next(model for model in self.catalog["models"] if model["key"] == "monitored_poultry_farm")["origin_z"] = 2
        with self.assertRaisesRegex(ValueError, "origin"):
            building_art.validate_records(self.catalog, self.manifest)

    def test_missing_assignment_or_sign_fails(self) -> None:
        read_source = building_art.source_text
        for before, after in (("BuildingModelPaths.AcceleratorWorks", "VerticalSliceProofLayout.PrefabPath"), (".AddSign()", "")):
            def changed(root: Path, filename: str) -> str:
                source = read_source(root, filename)
                return source.replace(before, after) if filename == "AcceleratorWorksData.cs" else source
            with self.subTest(before=before), patch.object(building_art, "source_text", changed), self.assertRaises(ValueError):
                building_art.validate_source(ROOT, self.catalog)


if __name__ == "__main__":
    unittest.main()