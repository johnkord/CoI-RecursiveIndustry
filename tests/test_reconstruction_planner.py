import copy
from fractions import Fraction
import json
from pathlib import Path
import sys
import unittest
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))
import plan_reconstruction as planner
import planner_oracle


class ReconstructionPlannerTests(unittest.TestCase):
    def setUp(self):
        self.scenario = json.loads((ROOT / "data/reconstruction-scenario.example.json").read_text(encoding="utf-8"))

    def test_equal_output_is_not_equal_building_count(self):
        result = planner.plan(self.scenario)
        self.assertEqual(result["requested_output_per_60"], 96)
        self.assertEqual(result["routes"]["Staged"]["placements"]["IntegrateElectronics2Intermediates"], 1)
        self.assertEqual(result["routes"]["Integrated"]["placements"]["IntegrateElectronics2Direct"], 2)
        self.assertIn("Electronics2", result["routes"]["Integrated"]["terminal_recipe"]["outputs"])

    def test_all_three_projects_produce_both_routes(self):
        for product in planner.PROJECTS:
            with self.subTest(product=product):
                scenario = {**self.scenario, "product": product}
                result = planner.plan(scenario)
                self.assertEqual(set(result["routes"]), {"Staged", "Integrated"})
                self.assertEqual(result["routes"]["Staged"]["stream_demand_per_60"], 0)
                self.assertGreater(result["routes"]["Integrated"]["stream_demand_per_60"], 0)

    def test_cutover_capacity_is_separate_from_steady_state(self):
        self.scenario["headroom"] = {"installed_computing": 4096, "ongoing_computing": 512, "overlap_computing": 4096}
        result = planner.plan(self.scenario)
        for route in result["routes"].values():
            self.assertEqual(route["added_rack_iii"], 0)
            self.assertGreater(route["cutover_added_rack_iii"], 0)

    def test_spare_support_is_not_bought_twice(self):
        baseline = planner.plan(self.scenario)["routes"]["Integrated"]
        self.scenario["headroom"].update({"stream_per_60": "200", "packages_per_60": "10"})
        shared = planner.plan(self.scenario)["routes"]["Integrated"]
        self.assertLess(shared["added_process_and_control_support_buildings"], baseline["added_process_and_control_support_buildings"])
        self.assertEqual(shared["additional_gateway_stream_per_60"], 0)

    def test_unspecified_stock_does_not_claim_a_wait_time(self):
        for route in planner.plan(self.scenario)["routes"].values():
            self.assertIsNone(route["capital_timing"])

    def test_supplied_capital_shortfall_is_reported(self):
        self.scenario["construction_stock"] = {"ConstructionParts4": "1"}
        result = planner.plan(self.scenario)["routes"]["Integrated"]["capital_timing"]
        self.assertIsNone(result["parallel_supply_lower_bound_time"])
        self.assertIn("FrontierProgram", result["blocking_products"])

    def test_fully_funded_capital_has_zero_supply_wait(self):
        needed = planner.plan(self.scenario)["routes"]["Integrated"]["construction_excluding_racks_and_external_utilities"]
        self.scenario["construction_stock"] = needed
        result = planner.plan(self.scenario)["routes"]["Integrated"]["capital_timing"]
        self.assertEqual(result["parallel_supply_lower_bound_time"], 0)
        self.assertEqual(result["blocking_products"], [])

    def test_exact_fraction_inputs_and_config_are_retained(self):
        self.scenario["output_per_60"] = "1/10"
        result = planner.plan(self.scenario)
        self.assertEqual(result["requested_output_per_60"], Fraction(1, 10))
        self.assertEqual(planner.quantity(0.1, "amount"), Fraction(1, 10))
        self.assertTrue(result["source_sha256"])

    def test_invalid_numbers_and_unknown_fields_are_rejected(self):
        for value in (0, -1, "NaN", "Infinity", "1/0", True, None):
            with self.subTest(value=value):
                with self.assertRaises(ValueError):
                    planner.plan({**self.scenario, "output_per_60": value})
        with self.assertRaisesRegex(ValueError, "unknown fields"):
            planner.plan({**self.scenario, "ouput_per_60": 10})
        with self.assertRaises(ValueError):
            planner.plan({**self.scenario, "product": "unmodeled"})

    def test_invalid_headroom_or_unrelated_negative_stock_is_rejected(self):
        for headroom in ({"installed_computing": 1, "ongoing_computing": 2}, {"reserve_computing": "1/2"}, {"mystery": 1}):
            with self.subTest(headroom=headroom), self.assertRaises(ValueError):
                planner.plan({**self.scenario, "headroom": headroom})
        with self.assertRaises(ValueError):
            planner.plan({**self.scenario, "construction_stock": {"UnusedProduct": -1}})

    def test_report_is_deterministic_and_keeps_boundary_visible(self):
        scenario = copy.deepcopy(self.scenario)
        result = planner.plan(scenario)
        rendered = planner.markdown(result)
        self.assertEqual(result, planner.plan(scenario))
        self.assertEqual(scenario, self.scenario)
        self.assertIn("No existing upstream building is assumed removable", rendered)
        self.assertIn("cutover", rendered)
        self.assertIn("Remaining External Supply", rendered)
        self.assertIn("Rack III available", rendered)
        self.assertIn("no wall-clock forecast", rendered.lower())
        self.assertIn("Terminal Recipe Rows", rendered)
        self.assertIn("Silicon (Poly)", rendered)

    def test_committed_example_matches_current_source(self):
        expected = planner.markdown(planner.plan(self.scenario))
        self.assertEqual((ROOT / "docs/CONVERSION_WORKSHEET.md").read_text(encoding="utf-8"), expected)


class NativePlannerOracleTests(unittest.TestCase):
        def report(self):
                return ET.fromstring('''<prototypes ticks_per_second="10" electricity_per_kw="10" percent_hundred_raw="100">
                    <machine id="Host" power_raw="100"><recipe id="Recipe" duration_ticks="600" multiplier="2" power_raw="100">
                        <input product="Feed" quantity="2"/><output product="Part" quantity="1"/><output product="Waste" quantity="1"/>
                    </recipe></machine>
                    <planner><capacity kind="Unit" value="3"/>
                        <process Key="Host|Recipe" Duration="60" PowerKw="10">
                            <Inputs product="Feed" quantity="4"/><Outputs product="Part" quantity="2"/><Outputs product="Waste" quantity="2"/>
                            <port input="true" kind="Unit" products="Feed"/>
                            <quote product="Part" requested="1" maximum_batches="3/4" Hosts="1" Attainable="3/2" ActiveHosts="1/2"
                                         PeakPower="10" IdealAveragePower="5" EnergyPerOutput="300">
                                <Inputs product="Feed" quantity="2"/><Outputs product="Part" quantity="1"/><Outputs product="Waste" quantity="1"/>
                            </quote>
                        </process>
                    </planner></prototypes>''')

        def test_independent_fraction_quote_matches(self):
                self.assertEqual(planner_oracle.audit(self.report()), ([], 1))

        def test_wrong_quotes_and_lost_coproducts_fail(self):
                for field in ("Hosts", "Attainable", "ActiveHosts", "PeakPower", "IdealAveragePower", "EnergyPerOutput", "maximum_batches"):
                        root = self.report()
                        root.find("planner/process/quote").set(field, "99")
                        self.assertTrue(planner_oracle.audit(root)[0], field)
                root = self.report()
                root.find("planner/process/quote/Outputs[@product='Waste']").set("quantity", "0")
                self.assertTrue(planner_oracle.audit(root)[0])

        def test_binding_multiplier_drift_fails(self):
                root = self.report()
                root.find("machine/recipe").set("multiplier", "3")
                self.assertTrue(any("normalization" in error for error in planner_oracle.audit(root)[0]))


if __name__ == "__main__":
    unittest.main()