#!/usr/bin/env python3
"""Regression tests for the public Recursive Industry economy model."""

from __future__ import annotations

from fractions import Fraction
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from simulate_recursive_industry_economy import (  # noqa: E402
    CURRENT,
    SELECTED,
    dossier_bank,
    package_bank,
    scenarios,
)


class EconomyTests(unittest.TestCase):
    def test_one_validator_closes_model_and_dataset_support(self) -> None:
        bank = package_bank(Fraction(160))
        self.assertEqual(bank.validators, 1)
        self.assertEqual(bank.model_centers, 1)
        self.assertEqual(bank.curation_offices, 1)
        self.assertEqual(bank.capacity_per_hour, 160)
        self.assertEqual(bank.computing, 48)
        self.assertEqual(bank.workers, 128)

    def test_two_pilots_close_science_support(self) -> None:
        bank = dossier_bank(Fraction(20))
        self.assertEqual(bank.pilots, 2)
        self.assertEqual(bank.science_institutes, 1)
        self.assertEqual(bank.model_centers, 1)
        self.assertEqual(bank.curation_offices, 2)
        self.assertEqual(bank.capacity_per_hour, 20)

    def test_selected_candidate_retires_rack_explosion(self) -> None:
        baseline = {result.scenario: result for result in scenarios(CURRENT)}
        selected = {result.scenario: result for result in scenarios(SELECTED)}
        self.assertEqual(baseline["process_branch"].rack_iii, 25)
        self.assertEqual(selected["process_branch"].rack_iii, 4)
        self.assertEqual(baseline["all_universal_direct"].rack_iii, 107)
        self.assertEqual(selected["all_universal_direct"].rack_iii, 14)

    def test_selected_package_district_is_bounded(self) -> None:
        selected = {result.scenario: result for result in scenarios(SELECTED)}
        self.assertEqual(selected["mature_core"].package_validators, 4)
        self.assertEqual(selected["mature_core_plus_pcc"].package_validators, 8)
        self.assertEqual(
            selected["mature_core_plus_pcc"].packages_per_hour,
            Fraction(5017, 4),
        )

    def test_selected_stress_case_remains_large(self) -> None:
        selected = {result.scenario: result for result in scenarios(SELECTED)}
        self.assertEqual(selected["release_stress_direct"].rack_iii, 34)
        self.assertEqual(selected["release_stress_direct"].orbital_arrays, 3)
        self.assertEqual(selected["release_stress_optimized"].orbital_arrays, 4)

    def test_terrestrial_counterfactual_excludes_orbital_support(self) -> None:
        terrestrial = {
            result.scenario: result
            for result in scenarios(SELECTED, orbital_power_closure=False)
        }
        process = terrestrial["process_branch"]
        universal = terrestrial["all_universal_direct"]
        optimized = terrestrial["all_universal_optimized"]
        self.assertEqual(process.orbital_arrays, 0)
        self.assertEqual(process.rack_iii, 3)
        self.assertEqual(universal.rack_iii, 14)
        self.assertEqual(universal.rack_coolant, 140)
        self.assertEqual(universal.gross_power_mw, 436)
        self.assertEqual(optimized.gross_power_mw, 658)


if __name__ == "__main__":
    unittest.main()
