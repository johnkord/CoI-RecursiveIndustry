"""Reject incomplete world art and lost native dynamic controls."""

from copy import deepcopy
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))
import audit_world_art as world


class WorldArtTests(unittest.TestCase):
    def setUp(self) -> None:
        self.catalog = world.load(ROOT / "data/world-art.json")
        self.manifest = world.load(ROOT / world.MANIFEST)

    def test_complete_final_artwork_passes(self) -> None:
        self.assertEqual(world.audit(), [])

    def test_missing_model_and_wrong_category_fail(self) -> None:
        changed = deepcopy(self.manifest)
        changed["models"].pop()
        with self.assertRaises(ValueError):
            world.validate_records(self.catalog, changed)
        self.manifest["models"][0]["category"] = "vehicle"
        with self.assertRaises(ValueError):
            world.validate_records(self.catalog, self.manifest)

    def test_cargo_lod_and_pbr_gaps_fail(self) -> None:
        for field in ("cargo_meshes", "cargo_textures"):
            changed = deepcopy(self.manifest)
            changed["models"][0][field].pop()
            with self.subTest(field=field), self.assertRaises(ValueError):
                world.validate_records(self.catalog, changed)

    def test_native_rig_control_gaps_fail(self) -> None:
        cases = (("rack_i", "required_renderers", []), ("fiber_port", "native_mesh_material_extraction", False), ("autonomous_hauler", "native_rig_contract", False), ("autonomous_steam_locomotive_i", "animation_states", []), ("heavy_dump", "animation_states", [":Up", ":Down"]))
        for key, field, value in cases:
            changed = deepcopy(self.manifest)
            next(row for row in changed["models"] if row["key"] == key)[field] = value
            with self.subTest(key=key), self.assertRaises(ValueError):
                world.validate_records(self.catalog, changed)

    def test_uncontrolled_train_animation_fails(self) -> None:
        row = next(row for row in self.manifest["models"] if row["key"] == "autonomous_diesel_locomotive_i")
        row["animation_states"] = [":Main"]
        row["sampled_animation_motion"] = True
        with self.assertRaisesRegex(ValueError, "root-controller"):
            world.validate_records(self.catalog, self.manifest)

    def test_foreign_assets_fail(self) -> None:
        self.manifest["asset_paths"].append("assets/base/vehicles/borrowed.prefab")
        with self.assertRaisesRegex(ValueError, "Foreign"):
            world.validate_records(self.catalog, self.manifest)

    def test_blank_or_missing_shader_controls_fail(self) -> None:
        for field, value in (("nonblank_pixels", 0), ("readable_meshes", False)):
            changed = deepcopy(self.manifest)
            changed["models"][0][field] = value
            with self.subTest(field=field), self.assertRaises(ValueError):
                world.validate_records(self.catalog, changed)
        self.manifest["controls"][0]["distinct_pixels"] = False
        with self.assertRaises(ValueError):
            world.validate_records(self.catalog, self.manifest)


if __name__ == "__main__":
    unittest.main()