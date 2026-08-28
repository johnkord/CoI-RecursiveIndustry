#!/usr/bin/env python3
"""Regression tests for the 0.23.0a building operating envelopes."""

from __future__ import annotations

from collections import Counter
from fractions import Fraction
import json
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from generate_recursive_industry_universal_source import load_catalog  # noqa: E402


PARENTS = {
    "fuel_smelter": "primary_smelter",
    "casting_finishing_works": "precision_metals_works",
    "thermal_desalination_works": "water_utility",
    "nuclear_reprocessing_center": "nuclear_fuel_complex",
    "nuclear_fuel_fabrication_cell": "nuclear_fuel_complex",
    "robotic_components_fab": "precision_components_fab",
}
NEW_KEYS = set(PARENTS)
RETIRED = {
    "SteamDepletedCondensationT2",
    "SteamHpCondensationT2",
    "SteamLpCondensationT2",
    "SteamSpCondensationT2",
}


def normalized(path: Path) -> str:
    return "".join(path.read_text(encoding="utf-8").split())


class OperatingEnvelopeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.catalog = load_catalog()
        cls.raw = json.loads(
            (ROOT / "data" / "universal-industry-catalog.json").read_text(
                encoding="utf-8"
            )
        )

    def test_successor_inventory_and_aggregates_are_exact(self) -> None:
        facilities = self.catalog["facilities"]
        direct_ids = [
            binding["recipe_id"]
            for facility in facilities
            for binding in facility["direct_bindings"]
        ]
        self.assertEqual(len(facilities), 25)
        self.assertEqual(len({facility["key"] for facility in facilities}), 25)
        self.assertEqual(len(direct_ids), 231)
        self.assertEqual(len(set(direct_ids)), 231)
        self.assertTrue(RETIRED.isdisjoint(direct_ids))
        self.assertEqual(sum(row["power_kw"] for row in facilities), 142500)
        self.assertEqual(sum(row["selected_computing"] for row in facilities), 3392)
        self.assertEqual(sum(row["workers"] for row in facilities), 88)
        maintenance = Counter()
        for row in facilities:
            maintenance[row["maintenance_tier"]] += row["maintenance_per_month"]
        self.assertEqual(maintenance, {"I": 203, "II": 111, "III": 168})
        depot_seconds = (
            Fraction(maintenance["I"], 8)
            + Fraction(maintenance["II"], 8)
            + Fraction(maintenance["III"], 4)
        )
        self.assertEqual(depot_seconds, Fraction(325, 4))

    def test_split_support_and_construction_are_conserved(self) -> None:
        source = {row["key"]: row for row in self.raw["facilities"]}
        successor = self.catalog["facilities"]
        fields = (
            "selected_computing",
            "cp4",
            "electronics4",
            "selected_packages",
            "programs",
            "dossiers",
            "calibration",
        )
        for parent in set(PARENTS.values()):
            children = [
                row
                for row in successor
                if PARENTS.get(row["key"], row["key"]) == parent
            ]
            for field in fields:
                self.assertEqual(
                    sum(row[field] for row in children),
                    source[parent][field],
                    f"{parent} {field}",
                )

    def test_custom_mode_owners_and_maximum_power_are_exact(self) -> None:
        integrated = {
            row["key"]: row for row in self.catalog["integrated_recipes"]
        }
        precision = {
            row["key"]: row for row in self.catalog["precision_recipes"]
        }
        self.assertEqual(
            integrated["integrated_electronics3"]["machine"],
            "robotic_components_fab",
        )
        self.assertEqual(
            integrated["integrated_electronics4"]["machine"],
            "robotic_components_fab",
        )
        self.assertEqual(precision["precision_steel"]["machine"], "casting_finishing_works")
        self.assertEqual(
            precision["precision_uranium_rods"]["machine"],
            "nuclear_fuel_fabrication_cell",
        )
        self.assertEqual(
            precision["precision_electronics4"]["machine"],
            "robotic_components_fab",
        )

        maximums = Counter({row["key"]: 100 for row in self.catalog["facilities"]})
        for row in integrated:
            spec = integrated[row]
            maximums[spec["machine"]] = max(
                maximums[spec["machine"]],
                spec["power_multiplier_percent"],
            )
        for spec in precision.values():
            maximums[spec["machine"]] = max(maximums[spec["machine"]], 200)
        for spec in self.catalog["authored_recipes"]:
            maximums[spec["machine"]] = max(
                maximums[spec["machine"]],
                spec["power_multiplier_percent"],
            )
        power_by_key = {
            row["key"]: row["power_kw"] for row in self.catalog["facilities"]
        }
        maximum_power_kw = sum(
            power_by_key[key] * multiplier // 100
            for key, multiplier in maximums.items()
        )
        self.assertEqual(maximum_power_kw, 328500)

    def test_non_universal_values_and_array_boundary_are_exact(self) -> None:
        source = ROOT / "mods" / "RecursiveIndustry" / "src"
        expected = {
            "IndustrialControlGatewayData.cs": (
                ".Workers(4)",
                ".MaintenanceT3(8)",
                ".SetElectricityConsumption(1000.Kw())",
            ),
            "AutonomousMicrochipData.cs": (
                ".Workers(0)",
                ".MaintenanceT3(96)",
                ".SetElectricityConsumption(8000.Kw())",
            ),
            "AutonomousElectronicsIntegrationData.cs": (
                ".Workers(0)",
                ".MaintenanceT3(16)",
                ".SetElectricityConsumption(2000.Kw())",
            ),
            "AutonomousCapitalFabricationData.cs": (
                ".Workers(0)",
                ".MaintenanceT3(16)",
                ".SetElectricityConsumption(2000.Kw())",
            ),
            "CompanionAnimalCareData.cs": (
                ".Workers(8)",
                ".MaintenanceT2(4)",
                ".SetElectricityConsumed(250.Kw())",
            ),
        }
        for filename, tokens in expected.items():
            text = normalized(source / filename)
            for token in tokens:
                self.assertIn(token, text, filename)

        frontier = normalized(source / "RecursiveFrontierData.cs")
        nexus = frontier[frontier.index('Start("AutonomousConstructionNexus"'):]
        self.assertIn(".Workers(0).MaintenanceT3(16)", nexus)
        self.assertIn(".SetElectricityConsumption(2000.Kw())", nexus)

        orbital = normalized(source / "OrbitalPowerArrayData.cs")
        self.assertIn(".Workers(80).MaintenanceT3(80)", orbital)
        self.assertIn("240000.Kw()", orbital)

    def test_new_facilities_are_unlocked_and_have_icons(self) -> None:
        source = ROOT / "mods" / "RecursiveIndustry" / "src"
        research = (source / "UniversalIndustryResearchData.cs").read_text(
            encoding="utf-8"
        )
        for key in NEW_KEYS:
            member = "".join(part.title() for part in key.split("_"))
            self.assertIn(f"RecursiveIndustryIds.Machines.{member}", research)

        icons = json.loads(
            (ROOT / "art" / "RecursiveIndustry" / "UiIcons" / "asset-manifest.json")
            .read_text(encoding="utf-8")
        )
        names = {row["name"] for row in icons["icons"]}
        self.assertTrue(NEW_KEYS <= names)
        self.assertEqual(len(names), 91)


if __name__ == "__main__":
    unittest.main()