#!/usr/bin/env python3
"""Exact-rate and installed-capacity regressions for bridge comparisons."""

from fractions import Fraction
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from model_reconstruction_bridge import (  # noqa: E402
    Headroom, additional_racks, capital_wait, conversion, load_source, report,
)


class ReconstructionEconomyTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.recipes, cls.machines, cls.hashes = load_source()

    def test_installed_capacity_and_reserve(self) -> None:
        for demand, installed, reserve, expected in (
            (768, 1024, 0, 0), (1025, 1024, 0, 1),
            (768, 1024, 512, 1), (900, 768, 0, 1), (256, 1024, 0, 0),
        ):
            with self.subTest(demand=demand, installed=installed, reserve=reserve):
                self.assertEqual(additional_racks(demand, installed, reserve), expected)

    def test_equal_output_requires_two_raw_electronics_facilities(self) -> None:
        self.assertEqual(self.recipes["IntegrateElectronics2Intermediates"].rate("Electronics2"), 96)
        self.assertEqual(self.recipes["IntegrateElectronics2Direct"].rate("Electronics2"), 48)
        result = conversion(self.recipes, self.machines, {"IntegrateElectronics2Direct": Fraction(96)})
        self.assertEqual(result["placements"]["IntegrateElectronics2Direct"], 2)
        self.assertEqual(result["stream_demand_per_60"], 120)

    def test_first_control_deployment_closes_package_model_and_dataset_demand(self) -> None:
        result = conversion(self.recipes, self.machines, {"IntegrateElectronics2Direct": Fraction(48)})
        self.assertEqual(result["placements"], {
            "IntegrateElectronics2Direct": 1, "DeployIndustrialControl": 1,
            "ValidateControlPackages": 1, "CurationIntensiveAdaptation": 1, "CurateDataset": 1,
        })
        self.assertEqual(result["new_machine_computing_peak"], 560)
        self.assertEqual(result["added_rack_iii"], 3)
        self.assertEqual(result["new_machine_workers"], 132)
        self.assertEqual(result["additional_package_production_per_60"], Fraction(2, 7))
        for intermediate in ("IndustrialControlStream", "ValidatedControlPackage", "ModelArchive", "DatasetArchive"):
            self.assertNotIn(intermediate, result["external_inputs_per_60"])
        self.assertEqual(result["external_inputs_per_60"]["LabEquipment4"], Fraction(3, 14))

    def test_shared_support_is_not_purchased_again(self) -> None:
        spare = Headroom(1024, 512, stream_per_60=Fraction(150), packages_per_60=Fraction(1))
        result = conversion(self.recipes, self.machines, {"IntegrateElectronics2Direct": Fraction(48)}, spare)
        self.assertEqual(result["placements"], {"IntegrateElectronics2Direct": 1})
        self.assertEqual(result["added_rack_iii"], 0)
        self.assertEqual(result["construction_excluding_racks_and_external_utilities"], {
            "ConstructionParts4": 960, "Electronics4": 128,
            "ValidatedControlPackage": 32, "FrontierProgram": 4,
        })

    def test_capital_integration_preserves_material_boundary(self) -> None:
        staged = conversion(self.recipes, self.machines, {"FabricateConstructionParts3": Fraction(96)})
        integrated = conversion(
            self.recipes, self.machines, {"IntegrateConstructionParts3": Fraction(96)},
            Headroom(stream_per_60=Fraction(120)),
        )
        self.assertEqual(staged["external_inputs_per_60"], integrated["external_inputs_per_60"])
        self.assertEqual(staged["added_process_and_control_support_buildings"], 3)
        self.assertEqual(integrated["placements"]["IntegrateConstructionParts3"], 2)

    def test_combined_projects_share_one_support_chain(self) -> None:
        combined = conversion(self.recipes, self.machines, {
            "IntegrateElectronics2Direct": Fraction(48), "IntegrateConstructionParts3": Fraction(48),
        })
        self.assertEqual(combined["placements"]["DeployIndustrialControl"], 1)
        self.assertEqual(combined["placements"]["ValidateControlPackages"], 1)
        self.assertEqual(combined["additional_package_production_per_60"], Fraction(4, 7))

    def test_cutover_can_need_more_racks_than_final_operation(self) -> None:
        result = conversion(
            self.recipes, self.machines, {"IntegrateConstructionParts3": Fraction(96)},
            Headroom(2048, 768, overlap_computing=512),
        )
        self.assertEqual(result["added_rack_iii"], 1)
        self.assertEqual(result["cutover_added_rack_iii"], 3)

    def test_transport_limit_is_not_free_recipe_throughput(self) -> None:
        recipe = self.recipes["IntegrateElectronics2Direct"]
        self.assertLess(recipe.rate("Electronics2", Fraction(60)), recipe.rate("Electronics2"))
        with self.assertRaises(ValueError):
            recipe.rate("Electronics2", Fraction(0))

    def test_configuration_changes_rates_and_support(self) -> None:
        recipes, _, _ = load_source(overrides={"autonomous_electronics_integrated_seconds": 240})
        self.assertEqual(recipes["IntegrateElectronics2Direct"].rate("Electronics2"), 24)
        self.assertEqual(recipes["IntegrateElectronics2Direct"].inputs["IndustrialControlStream"], 240)
        with self.assertRaises(ValueError):
            load_source(overrides={"validation_seconds": 0})

    def test_unsupplied_capital_is_a_blocker_not_zero_wait(self) -> None:
        self.assertEqual(capital_wait({"FrontierProgram": 8}, {"FrontierProgram": 4}, {})["blocking_products"], ["FrontierProgram"])
        result = capital_wait({"FrontierProgram": 8}, {"FrontierProgram": 4}, {"FrontierProgram": Fraction(1, 12)})
        self.assertEqual(result["parallel_supply_lower_bound_time"], 2880)

    def test_invalid_headroom_and_demand_are_rejected(self) -> None:
        with self.assertRaises(ValueError):
            Headroom(256, 512)
        with self.assertRaises(ValueError):
            additional_racks(-1, 0)
        with self.assertRaises(ValueError):
            conversion(self.recipes, self.machines, {"IntegrateElectronics2Direct": Fraction(-1)})

    def test_report_is_deterministic_and_exposes_its_boundary(self) -> None:
        self.assertEqual(report(), report())
        self.assertEqual(len(self.hashes), 7)
        self.assertTrue(report()["exclusions"])
        self.assertEqual(report()["prepared_capital_wait_example"]["result"]["parallel_supply_lower_bound_time"], 2880)


if __name__ == "__main__":
    unittest.main()