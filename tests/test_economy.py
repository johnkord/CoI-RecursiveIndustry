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
    control_scenarios,
    control_sensitivity_tournament,
    dossier_bank,
    electronics_iii_balance,
    package_bank,
    package_scale_scenarios,
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
        self.assertEqual(selected["mature_core"].packages_per_hour, 408)
        self.assertEqual(selected["mature_core"].package_validators, 3)
        self.assertEqual(selected["mature_core_plus_pcc"].package_validators, 7)
        self.assertEqual(
            selected["mature_core_plus_pcc"].packages_per_hour,
            1048,
        )

    def test_selected_stress_case_remains_large(self) -> None:
        selected = {result.scenario: result for result in scenarios(SELECTED)}
        self.assertEqual(selected["release_stress_direct"].rack_iii, 34)
        self.assertEqual(selected["release_stress_direct"].orbital_arrays, 2)
        self.assertEqual(selected["release_stress_optimized"].orbital_arrays, 2)
        self.assertGreater(
            selected["release_stress_optimized"].gross_power_mw,
            selected["release_stress_direct"].gross_power_mw,
        )

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
        self.assertEqual(universal.gross_power_mw, Fraction(327, 2))
        self.assertEqual(optimized.gross_power_mw, Fraction(699, 2))

    def test_composed_modes_set_exact_power_ceiling(self) -> None:
        direct = {
            result.scenario: result
            for result in scenarios(SELECTED, orbital_power_closure=False)
        }
        self.assertEqual(
            direct["all_universal_optimized"].gross_power_mw
            - direct["all_universal_direct"].gross_power_mw,
            186,
        )

    def test_control_topologies_have_exact_capacity_and_package_closure(self) -> None:
        control = {result.scenario: result for result in control_scenarios()}
        direct = control["no_control_direct"]
        access = control["three_facility_access"]
        backbone = control["seven_facility_backbone"]
        federated_backbone = control["seven_facility_federated"]
        all_eleven = control["all_eleven_optimized"]
        federated_all_eleven = control["all_eleven_federated"]

        self.assertEqual(direct.gateway_count, 0)
        self.assertEqual(direct.stream_demand_per_minute, 0)
        self.assertEqual(access.optimized_owner_count, 3)
        self.assertEqual(access.gateway_count, 1)
        self.assertEqual(access.transport_headroom_per_minute, 20)
        self.assertEqual(backbone.optimized_owner_count, 7)
        self.assertEqual(backbone.gateway_count, 2)
        self.assertEqual(backbone.gateway_headroom_per_minute, 0)
        self.assertEqual(backbone.transport_headroom_per_minute, 30)
        self.assertEqual(federated_backbone.gateway_count, 1)
        self.assertEqual(federated_backbone.local_gateway_count, 0)
        self.assertEqual(federated_backbone.backbone_gateway_count, 1)
        self.assertEqual(federated_backbone.stream_supply_per_minute, 420)
        self.assertEqual(all_eleven.optimized_owner_count, 11)
        self.assertEqual(all_eleven.gateway_count, 4)
        self.assertEqual(all_eleven.stream_demand_per_minute, 660)
        self.assertEqual(all_eleven.stream_supply_per_minute, 840)
        self.assertEqual(all_eleven.transport_capacity_per_minute, 900)
        self.assertEqual(all_eleven.transport_headroom_per_minute, 240)
        self.assertEqual(all_eleven.steady_state_packages_per_hour, Fraction(1320, 7))
        self.assertEqual(all_eleven.unconstrained_packages_per_hour, 240)
        self.assertEqual(all_eleven.support.package_validators, 2)
        self.assertEqual(all_eleven.support.package_model_centers, 2)
        self.assertEqual(all_eleven.support.package_curation_offices, 2)
        self.assertEqual(all_eleven.support.computing, 5280)
        self.assertEqual(all_eleven.support.rack_iii, 21)
        self.assertEqual(all_eleven.support.rack_coolant, 210)
        self.assertEqual(all_eleven.support.workers, 360)
        self.assertEqual(all_eleven.support.gross_maintenance_t3, 665)
        self.assertEqual(federated_all_eleven.gateway_count, 2)
        self.assertEqual(federated_all_eleven.local_gateway_count, 0)
        self.assertEqual(federated_all_eleven.backbone_gateway_count, 2)
        self.assertEqual(federated_all_eleven.stream_supply_per_minute, 840)
        self.assertEqual(
            federated_all_eleven.steady_state_packages_per_hour,
            all_eleven.steady_state_packages_per_hour,
        )
        self.assertEqual(federated_all_eleven.unconstrained_packages_per_hour, 240)
        self.assertLess(
            federated_all_eleven.support.gross_power_mw,
            all_eleven.support.gross_power_mw,
        )
        self.assertLess(
            federated_all_eleven.support.workers,
            all_eleven.support.workers,
        )

    def test_assurance_campus_compresses_exact_validator_blocks(self) -> None:
        scale = {result.scenario: result for result in package_scale_scenarios()}
        mature = scale["mature_core_with_control"]
        center = scale["mature_core_center_control"]
        self.assertEqual(mature.demand_per_hour, Fraction(4176, 7))
        self.assertEqual(mature.standard_validators, 4)
        self.assertEqual(mature.assurance_campuses, 1)
        self.assertEqual(mature.trim_validators, 0)
        self.assertEqual(mature.dense_capacity_per_hour, 640)
        self.assertEqual(center.demand_per_hour, Fraction(8656, 7))
        self.assertEqual(center.standard_validators, 8)
        self.assertEqual(center.assurance_campuses, 2)
        self.assertEqual(center.trim_validators, 0)
        self.assertEqual(center.dense_capacity_per_hour, 1280)
        for result in (mature, center):
            self.assertEqual(
                result.standard_capacity_per_hour,
                result.dense_capacity_per_hour,
            )
            self.assertLess(result.dense_workers, result.standard_workers)
            self.assertGreater(result.dense_power_mw, result.standard_power_mw)
            self.assertGreater(result.dense_computing, result.standard_computing)
            self.assertGreater(
                result.dense_construction_parts_iv,
                result.standard_construction_parts_iv,
            )

    def test_control_sensitivity_tournament_covers_all_selected_axes(self) -> None:
        tournament = control_sensitivity_tournament()
        self.assertEqual(len(tournament), 12)
        self.assertEqual(
            {
                (
                    result.stream_per_package,
                    result.gateway_computing,
                    result.gateway_power_mw,
                )
                for result in tournament
            },
            {
                (stream, computing, power)
                for stream in (105, 210, 420)
                for computing in (128, 256)
                for power in (1, 2)
            },
        )
        selected = next(
            result
            for result in tournament
            if (
                result.stream_per_package,
                result.gateway_computing,
                result.gateway_power_mw,
            ) == (210, 256, 1)
        )
        self.assertEqual(selected.gateway_count, 4)
        self.assertEqual(selected.steady_state_packages_per_hour, Fraction(1320, 7))
        self.assertEqual(selected.unconstrained_packages_per_hour, 240)
        self.assertEqual(selected.support.package_validators, 2)

    def test_electronics_three_supply_matches_representative_demand(self) -> None:
        balance = electronics_iii_balance()
        self.assertEqual(balance.representative_demand_per_hour, 4720)
        self.assertEqual(balance.direct_fab_output_per_hour, 1440)
        self.assertEqual(balance.required_direct_fabs, 4)
        self.assertEqual(balance.direct_fab_capacity_per_hour, 5760)
        self.assertEqual(balance.direct_fab_headroom_per_hour, 1040)
        self.assertEqual(balance.required_assembly_v_lines, 14)
        self.assertEqual(balance.required_throughput_cells, 7)


if __name__ == "__main__":
    unittest.main()
