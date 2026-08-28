#!/usr/bin/env python3
"""Regression tests for Circular Agrifood public source."""

from __future__ import annotations

from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from audit_recursive_industry_circular_agrifood import audit, load_json  # noqa: E402


class CircularAgrifoodTests(unittest.TestCase):
    def test_public_source_matches_contract(self) -> None:
        self.assertEqual(audit(), [])

    def test_catalog_counts_preserve_existing_modes(self) -> None:
        contract = load_json(ROOT / "data" / "circular-agrifood.json")
        counts = contract["catalog_contract"]
        self.assertEqual(counts["direct_bindings"], 231)
        self.assertEqual(counts["integrated_recipes"], 21)
        self.assertEqual(counts["precision_recipes"], 10)
        self.assertEqual(counts["authored_recipes"], 4)
        self.assertTrue(counts["all_authored_recipes_are_stream_free"])

    def test_companion_recipe_supports_4000_colonists(self) -> None:
        contract = load_json(ROOT / "data" / "circular-agrifood.json")
        recipe = contract["authored_recipes"]["companion_provisions"]
        care = contract["companion_care"]
        output = recipe["outputs"]["RecursiveIndustry.CompanionProvisions"]
        supported = output / care["provisions_per_pop_per_month"]
        self.assertEqual(supported, 4000)

    def test_1000_colonists_embed_15_feed_per_month(self) -> None:
        contract = load_json(ROOT / "data" / "circular-agrifood.json")
        recipe = contract["authored_recipes"]["companion_provisions"]
        care = contract["companion_care"]
        provisions = 1000 * care["provisions_per_pop_per_month"]
        feed = provisions * recipe["inputs"]["AnimalFeed"] / 80
        self.assertEqual(feed, 15)

    def test_companion_care_is_unity_only(self) -> None:
        care = load_json(
            ROOT / "data" / "circular-agrifood.json"
        )["companion_care"]
        self.assertEqual(care["unity_at_full_satisfaction"], 0.6)
        self.assertEqual(care["health"], 0)
        self.assertEqual(care["worker_productivity"], 0)
        self.assertGreater(care["workers"], 0)

    def test_hypothetical_gas_feed_cycle_stays_negative(self) -> None:
        contract = load_json(ROOT / "data" / "circular-agrifood.json")
        guards = contract["guards"]
        self.assertFalse(guards["gas_fed_feed_recipe_implemented"])
        self.assertLess(
            guards["maximum_returned_fuel_gas_from_60_feed"],
            guards["hypothetical_gas_fed_feed_input"],
        )
        for recipe in contract["authored_recipes"].values():
            if "maximum_fuel_gas_recovery" not in recipe:
                continue
            equivalent_feed = (
                guards["hypothetical_gas_fed_feed_output"]
                * recipe["maximum_fuel_gas_recovery"]
                / recipe["inputs"]["AnimalFeed"]
            )
            self.assertLess(
                equivalent_feed,
                guards["hypothetical_gas_fed_feed_input"],
            )

    def test_existing_overflow_paths_remain_available(self) -> None:
        alternatives = load_json(
            ROOT / "data" / "circular-agrifood.json"
        )["existing_alternatives"]
        self.assertEqual(alternatives["compost"]["outputs"]["Compost"], 6)
        self.assertEqual(
            alternatives["organic_fertilizer"]["outputs"]["FertilizerOrganic"],
            24,
        )
        self.assertEqual(alternatives["steam"]["outputs"]["SteamHigh"], 8)


if __name__ == "__main__":
    unittest.main()