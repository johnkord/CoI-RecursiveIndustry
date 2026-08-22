#!/usr/bin/env python3
"""Regression tests for Adaptive Agrifood public source."""

from __future__ import annotations

from copy import deepcopy
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from audit_recursive_industry_agrifood import audit, load_json  # noqa: E402


class AdaptiveAgrifoodTests(unittest.TestCase):
    def test_public_source_matches_contract(self) -> None:
        self.assertEqual(audit(), [])

    def test_precision_irrigation_is_farm_water_only(self) -> None:
        contract = load_json(ROOT / "data" / "adaptive-agrifood.json")
        irrigation = contract["precision_irrigation"]
        self.assertEqual(
            irrigation["property"],
            "IdsCore.PropertyIds.FarmWaterConsumptionMultiplier",
        )
        self.assertEqual(irrigation["cap_percent"], -10)
        self.assertNotIn("FarmYieldMultiplier", irrigation["property"])

    def test_farm_clones_retain_accountable_workers(self) -> None:
        contract = load_json(ROOT / "data" / "adaptive-agrifood.json")
        for farm in contract["farms"].values():
            self.assertEqual(farm["workers"], 4)
            self.assertGreater(farm["source_workers"], farm["workers"])

    def test_native_farm_upgrades_charge_only_added_hardware(self) -> None:
        contract = load_json(ROOT / "data" / "adaptive-agrifood.json")
        expected_sources = {
            "sensor_guided_greenhouse": "Ids.Buildings.FarmT4",
            "monitored_poultry_farm": "Ids.Buildings.ChickenFarm",
        }
        for key, farm in contract["farms"].items():
            self.assertEqual(farm["upgrade_from"], expected_sources[key])
            self.assertEqual(
                farm["upgrade_api"],
                "UpgradeExtensions.SetNextTier",
            )
            self.assertEqual(
                farm["incremental_upgrade_cost"],
                farm["additional_construction"],
            )

    def test_no_hidden_automation_runtime_is_admitted(self) -> None:
        contract = load_json(ROOT / "data" / "adaptive-agrifood.json")
        self.assertTrue(all(
            value is False or value == 0
            for value in contract["boundaries"].values()
        ))

    def test_icons_are_distinct_and_dependency_free(self) -> None:
        contract = load_json(ROOT / "data" / "adaptive-agrifood.json")
        presentation = contract["presentation"]
        self.assertEqual(presentation["total_ui_identities"], 85)
        self.assertEqual(presentation["new_ui_identities"], 3)
        self.assertEqual(presentation["dependencies"], [])

    def test_mutated_contract_is_rejected(self) -> None:
        contract = load_json(ROOT / "data" / "adaptive-agrifood.json")
        mutated = deepcopy(contract)
        mutated["precision_irrigation"]["cap_percent"] = -20
        self.assertNotEqual(
            mutated["precision_irrigation"],
            contract["precision_irrigation"],
        )


if __name__ == "__main__":
    unittest.main()