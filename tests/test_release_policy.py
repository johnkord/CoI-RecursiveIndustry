import copy
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))
from audit_release_policy import audit, validate


class ReleasePolicyTests(unittest.TestCase):
    def setUp(self):
        self.policy = json.loads((ROOT / "data/release-policy.json").read_text(encoding="utf-8"))
        self.manifest = json.loads((ROOT / "mods/RecursiveIndustry/manifest.json").read_text(encoding="utf-8"))

    def test_current_candidate_policy_passes(self):
        self.assertEqual(audit(), [])

    def test_metadata_and_art_version_drift_are_rejected(self):
        for field, value in (("candidate_version", "9.0.0"), ("retained_art_versions", {"world": "9.0.0"})):
            policy = copy.deepcopy(self.policy)
            policy[field] = value
            self.assertTrue(validate(policy, self.manifest))

    def test_unsupported_game_and_save_claims_are_rejected(self):
        manifest = {**self.manifest, "min_game_version": "0.8.6c"}
        self.assertTrue(validate(self.policy, manifest))
        for value in (True, 0, "false"):
            policy = copy.deepcopy(self.policy)
            policy["support"]["add_to_existing_saves"] = value
            self.assertTrue(validate(policy, self.manifest))

    def test_stable_claim_and_removed_independent_gate_are_rejected(self):
        policy = copy.deepcopy(self.policy)
        policy["intended_stage"] = "stable"
        self.assertTrue(validate(policy, self.manifest))
        policy = copy.deepcopy(self.policy)
        policy["required_release_gates"] = []
        self.assertTrue(validate(policy, self.manifest))

    def test_default_build_does_not_deploy(self):
        import xml.etree.ElementTree as ET
        project = ET.parse(ROOT / "mods/RecursiveIndustry/RecursiveIndustry.csproj")
        setting = project.find(".//PropertyGroup/DeployToModsFolder")
        self.assertIsNotNone(setting)
        self.assertEqual(setting.text, "false")

    def test_generated_building_paths_match_clean_checkout_bytes(self):
        from generate_building_model_paths import load, OUTPUT, render
        self.assertEqual(OUTPUT.read_bytes(), render(load()).encode("utf-8"))


if __name__ == "__main__":
    unittest.main()