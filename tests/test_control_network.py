#!/usr/bin/env python3
"""Regression tests for the Industrial Control network authority."""

from __future__ import annotations

from fractions import Fraction
import json
from pathlib import Path
import re
import unittest

from tools.generate_recursive_industry_universal_source import load_catalog


ROOT = Path(__file__).resolve().parents[1]
CONTROL = json.loads(
    (ROOT / "data" / "industrial-control-network.json").read_text(
        encoding="utf-8"
    )
)
CATALOG = load_catalog()


def pascal(value: str) -> str:
    return "".join(
        part[:1].upper() + part[1:]
        for part in re.split(r"[^A-Za-z0-9]+", value)
        if part
    )


class ControlNetworkContractTests(unittest.TestCase):
    def test_schema_and_isolated_data_product_are_exact(self) -> None:
        self.assertEqual(CONTROL["schema_version"], 1)
        product = CONTROL["product"]
        self.assertEqual(product["class"], "DataProductProto")
        self.assertEqual(product["product_type"], "DataProductProto.ProductType")
        self.assertEqual(
            product["quantity_formatter"],
            "NoUnitsQuantityFormatter.Instance",
        )
        self.assertFalse(product["storable"])
        self.assertFalse(product["discardable"])
        self.assertFalse(product["waste"])
        self.assertFalse(product["recyclable"])

        infrastructure = CONTROL["infrastructure"]
        self.assertEqual(
            infrastructure["port_shape"],
            {
                "registration_id": "RecursiveIndustry_Data",
                "stable_id": "IoPortShape_RecursiveIndustry_Data",
                "layout_character": ":",
                "allowed_product_type": "DataProductProto.ProductType",
            },
        )
        self.assertFalse(infrastructure["allow_mixed_products"])
        self.assertEqual(
            infrastructure["tier_order"],
            ["access_fiber", "backbone_fiber"],
        )
        capacities = {
            item["key"]: item["capacity_per_60_seconds"]
            for item in infrastructure["transports"]
        }
        self.assertEqual(capacities, {"access_fiber": 200, "backbone_fiber": 450})

    def test_gateway_vector_and_sensitivity_tournament_are_exact(self) -> None:
        gateway = CONTROL["gateway"]
        recipe = gateway["recipe"]
        self.assertEqual(recipe["input"]["quantity"], 1)
        self.assertEqual(recipe["output"]["quantity"], 210)
        self.assertEqual(recipe["duration_seconds"], 60)
        backbone = gateway["backbone_recipe"]
        self.assertEqual(backbone["input"]["quantity"], 2)
        self.assertEqual(backbone["output"]["quantity"], 420)
        self.assertEqual(backbone["duration_seconds"], 60)
        self.assertEqual(backbone["power_multiplier_percent"], 250)
        self.assertEqual(backbone["effective_power_mw"], 2.5)
        self.assertEqual(backbone["required_transport_key"], "backbone_fiber")
        self.assertEqual(
            Fraction(recipe["output"]["quantity"], recipe["input"]["quantity"]),
            Fraction(backbone["output"]["quantity"], backbone["input"]["quantity"]),
        )
        self.assertEqual(gateway["power_mw"], 1)
        self.assertEqual(gateway["computing"], 256)
        self.assertEqual(gateway["workers"], 4)
        self.assertEqual(gateway["maintenance"]["quantity_per_month"], 8)
        self.assertEqual(
            {item["product_key"]: item["quantity"] for item in gateway["construction"]},
            {
                "ConstructionParts4": 640,
                "Electronics4": 128,
                "ValidatedControlPackage": 32,
                "FrontierProgram": 4,
                "ValidatedResearchDossier": 4,
            },
        )
        sensitivity = CONTROL["capacity_closure"]["sensitivity"]
        self.assertEqual(sensitivity["stream_per_package"], [105, 210, 420])
        self.assertEqual(sensitivity["computing"], [128, 256])
        self.assertEqual(sensitivity["power_mw"], [1, 2])

    def test_deployment_assurance_is_density_not_yield(self) -> None:
        assurance = CONTROL["deployment_assurance"]
        recipe = assurance["recipe"]
        inputs = {
            item["product_key"]: item["quantity"]
            for item in recipe["inputs"]
        }
        self.assertEqual(
            inputs,
            {"ModelArchive": 16, "LabEquipment4": 32, "Electronics3": 32},
        )
        self.assertEqual(recipe["output"]["quantity"], 128)
        self.assertEqual(recipe["duration_seconds"], 720)
        self.assertEqual(recipe["packages_per_hour"], 640)
        self.assertEqual(assurance["recurring_dossier_input"], 0)
        standard = assurance["standard_validator_equivalence"]
        self.assertEqual(standard["machine_count"], 4)
        self.assertEqual(standard["packages_per_hour"], 640)
        self.assertEqual(Fraction(inputs["ModelArchive"], 128), Fraction(1, 8))
        self.assertEqual(Fraction(inputs["LabEquipment4"], 128), Fraction(1, 4))
        self.assertEqual(Fraction(inputs["Electronics3"], 128), Fraction(1, 4))
        self.assertEqual(assurance["workers"], 48)
        self.assertGreater(assurance["power_mw"], Fraction(standard["power_mw"]))
        self.assertGreater(assurance["computing"], standard["computing"])
        self.assertGreater(
            next(
                item["quantity"]
                for item in assurance["construction"]
                if item["product_key"] == "ConstructionParts4"
            ),
            standard["construction_parts_iv"],
        )

    def test_twenty_four_controlled_compositions_match_both_owners(self) -> None:
        recipes = CONTROL["consumer_contract"]["recipes"]
        self.assertEqual(len(recipes), 24)
        self.assertEqual(len({recipe["stable_id"] for recipe in recipes}), 24)
        self.assertEqual({recipe["mode"] for recipe in recipes}, {"integrated"})
        self.assertTrue(
            all(recipe["cancelled_intermediates"] for recipe in recipes)
        )

        contract_by_key = {recipe["key"]: recipe for recipe in recipes}
        catalog_by_key = {
            recipe["key"]: recipe
            for recipe in CATALOG["integrated_recipes"]
        }
        self.assertEqual(len(catalog_by_key), 21)
        self.assertEqual(
            set(contract_by_key) - set(catalog_by_key),
            {
                "integrated_electronics2",
                "integrated_construction_parts3",
                "integrated_vehicle_parts2",
            },
        )
        for key, catalog in catalog_by_key.items():
            contract = contract_by_key[key]
            self.assertEqual(contract["owner_key"], catalog["machine"])
            self.assertEqual(
                contract["stable_id"],
                "Recipe_RecursiveIndustry_" + pascal(key),
            )
            self.assertEqual(
                contract["stream_quantity"],
                contract["effective_duration_seconds"],
            )
            self.assertEqual(
                contract["effective_duration_seconds"],
                catalog["duration_seconds"],
            )
        self.assertEqual(
            {
                key: contract_by_key[key]["stable_id"]
                for key in set(contract_by_key) - set(catalog_by_key)
            },
            {
                "integrated_electronics2": (
                    "Recipe_RecursiveIndustry_IntegrateElectronics2Direct"
                ),
                "integrated_construction_parts3": (
                    "Recipe_RecursiveIndustry_IntegrateConstructionParts3"
                ),
                "integrated_vehicle_parts2": (
                    "Recipe_RecursiveIndustry_IntegrateVehicleParts2"
                ),
            },
        )

    def test_five_directed_refinery_slates_are_stream_controlled(self) -> None:
        expected = {
            "integrated_refinery_diesel": (100, {
                "Heavy Oil", "Medium Oil", "Light Oil", "Naphtha", "Fuel Gas"
            }),
            "integrated_refinery_gas": (100, {
                "Heavy Oil", "Medium Oil", "Light Oil", "Diesel", "Naphtha"
            }),
            "integrated_refinery_hydrogen": (275, {
                "Heavy Oil", "Medium Oil", "Light Oil", "Diesel", "Naphtha", "Fuel Gas"
            }),
            "integrated_refinery_plastic": (300, {
                "Heavy Oil", "Medium Oil", "Light Oil", "Diesel", "Naphtha"
            }),
            "integrated_refinery_rubber": (400, {
                "Heavy Oil", "Medium Oil", "Light Oil", "Diesel", "Naphtha", "Fuel Gas"
            }),
        }
        controlled = {
            recipe["key"]: recipe
            for recipe in CONTROL["consumer_contract"]["recipes"]
        }
        self.assertEqual(
            {key for key in controlled if key.startswith("integrated_refinery_")},
            set(expected),
        )
        for key, (power, cancelled) in expected.items():
            recipe = controlled[key]
            self.assertEqual(recipe["owner_key"], "refinery_complex")
            self.assertEqual(recipe["power_multiplier_percent"], power)
            self.assertEqual(set(recipe["cancelled_intermediates"]), cancelled)
            self.assertEqual(
                recipe["stream_quantity"], recipe["effective_duration_seconds"]
            )

    def test_directed_refinery_source_stages_are_exact(self) -> None:
        catalog = {
            recipe["key"]: recipe
            for recipe in CATALOG["integrated_recipes"]
        }
        expected_sources = {
            "integrated_refinery_diesel": 6,
            "integrated_refinery_gas": 6,
            "integrated_refinery_hydrogen": 7,
            "integrated_refinery_plastic": 6,
            "integrated_refinery_rubber": 7,
        }
        for key, source_count in expected_sources.items():
            multiplier = 6 if key == "integrated_refinery_plastic" else 12
            self.assertEqual(catalog[key]["machine"], "refinery_complex")
            self.assertEqual(catalog[key]["batch_scale"], 1)
            self.assertEqual(len(catalog[key]["sources"]), source_count)
            self.assertEqual(
                catalog[key]["sources"][:3],
                [
                    {"recipe_id": "CrudeOilRefiningT1", "multiplier": multiplier},
                    {"recipe_id": "CrudeOilRefiningT2", "multiplier": multiplier},
                    {"recipe_id": "HeavyDistillateRefining", "multiplier": multiplier},
                ],
            )

    def test_directed_refinery_net_vectors_are_exact(self) -> None:
        directed = CONTROL["directed_refinery_contract"]
        modes = {mode["key"]: mode for mode in directed["modes"]}
        expected = {
            "integrated_refinery_diesel": (
                {"Crude Oil": 240, "Hydrogen": 54, "Oxygen": 42, "Steam (High)": 48},
                {"Diesel": 368, "Sour Water": 72, "Water": 14},
                120,
                100,
            ),
            "integrated_refinery_gas": (
                {"Crude Oil": 240, "Steam (High)": 89},
                {"Fuel Gas": 372, "Hydrogen": 74, "Sour Water": 72},
                120,
                100,
            ),
            "integrated_refinery_hydrogen": (
                {"Crude Oil": 240, "Steam (High)": 120},
                {"Carbon Dioxide": 372, "Hydrogen": 508, "Sour Water": 72},
                120,
                275,
            ),
            "integrated_refinery_plastic": (
                {"Chlorine": 92, "Crude Oil": 120, "Hydrogen": 9, "Steam (High)": 33},
                {"Exhaust": 276, "Fuel Gas": 48, "Plastic": 414, "Sour Water": 36},
                60,
                300,
            ),
            "integrated_refinery_rubber": (
                {"Crude Oil": 240, "Hydrogen": 18, "Oxygen": 30, "Steam (High)": 48, "Sulfur": 88},
                {"Rubber": 704, "Sour Water": 72, "Water": 10},
                120,
                400,
            ),
        }
        self.assertEqual(set(modes), set(expected))
        for key, (inputs, outputs, duration, power) in expected.items():
            self.assertEqual(modes[key]["inputs"], inputs)
            self.assertEqual(modes[key]["outputs"], outputs)
            self.assertEqual(modes[key]["duration_seconds"], duration)
            self.assertEqual(modes[key]["stream_quantity"], duration)
            self.assertEqual(modes[key]["power_multiplier_percent"], power)
        forbidden = set(directed["forbidden_intermediate_outputs"])
        self.assertEqual(forbidden, {"Heavy Oil", "Medium Oil", "Light Oil", "Naphtha"})
        self.assertTrue(all(forbidden.isdisjoint(mode["outputs"]) for mode in modes.values()))

    def test_all_precision_modes_are_fiber_free(self) -> None:
        precision = CONTROL["precision_contract"]
        self.assertEqual(precision["stream_inputs_per_recipe"], 0)
        self.assertEqual(precision["power_multiplier_percent"], 200)
        self.assertEqual(precision["input_per_output_ratio"], "7/8 of Direct")
        self.assertEqual(
            precision["recipe_keys"],
            [recipe["key"] for recipe in CATALOG["precision_recipes"]],
        )
        self.assertEqual(len(precision["recipe_keys"]), 10)

    def test_data_floor_never_controls_selected_sixty_per_minute_vectors(self) -> None:
        for recipe in CONTROL["consumer_contract"]["recipes"]:
            quantity = recipe["stream_quantity"]
            data_floor_seconds = (3 * quantity + 9) // 10
            self.assertLessEqual(
                data_floor_seconds,
                recipe["effective_duration_seconds"],
            )

    def test_eleven_owners_fit_their_bounded_port_shells(self) -> None:
        owners = CONTROL["owners"]
        controlled_recipes = CONTROL["consumer_contract"]["recipes"]
        self.assertEqual(len(owners), 11)
        self.assertEqual(
            {owner["key"] for owner in owners},
            {recipe["owner_key"] for recipe in controlled_recipes},
        )
        for owner in owners:
            self.assertEqual(
                owner["input_ports_with_data"],
                owner["material_input_ports"] + 1,
            )
            self.assertLessEqual(owner["physical_layout_rows"], 7)
            overflow_inputs = max(
                0,
                owner["input_ports_with_data"] - owner["physical_layout_rows"],
            )
            expected_right_inputs = min(
                overflow_inputs,
                owner["physical_layout_rows"] - owner["output_ports"],
            )
            self.assertEqual(
                owner["right_side_input_ports"],
                expected_right_inputs,
            )
            self.assertEqual(
                owner.get("top_side_input_ports", 0),
                overflow_inputs - expected_right_inputs,
            )
            self.assertLessEqual(
                owner["output_ports"] + owner["right_side_input_ports"],
                owner["physical_layout_rows"],
            )
            self.assertLessEqual(owner.get("top_side_input_ports", 0), 7)
        self.assertEqual(
            {
                owner["key"]
                for owner in owners
                if owner["right_side_input_ports"] > 0
            },
            {
                "primary_smelter",
                "food_pack_campus",
                "nuclear_fuel_complex",
                "robotic_components_fab",
                "general_manufacturing_fab",
            },
        )
        self.assertEqual(
            {
                owner["key"]: owner.get("top_side_input_ports", 0)
                for owner in owners
                if owner.get("top_side_input_ports", 0) > 0
            },
            {"general_manufacturing_fab": 1},
        )
        self.assertEqual(
            {
                owner["key"]
                for owner in owners
                if not owner.get("catalog_facility", True)
            },
            {
                "autonomous_electronics_integration_complex",
                "autonomous_capital_fabrication_matrix",
            },
        )

    def test_direct_binding_delta_is_one_exact_electronics_three_row(self) -> None:
        direct = CONTROL["direct_contract"]
        binding = direct["electronics_iii_binding"]
        self.assertEqual(direct["expected_binding_count"], 231)
        self.assertEqual(direct["stream_inputs_per_binding"], 0)
        self.assertEqual(binding["recipe_id"], "Electronics3Assembly")
        self.assertEqual(
            binding["source_export_recipe_id"],
            "Electronics3AssemblyRoboticT2",
        )
        self.assertEqual(binding["source_machine_id"], "AssemblyRoboticT2")
        self.assertEqual(binding["owner_key"], "robotic_components_fab")
        self.assertEqual(
            {item["product_key"]: item["quantity"] for item in binding["inputs"]},
            {"Microchips": 8, "Electronics2": 16},
        )
        self.assertEqual(binding["outputs"], [{"product_key": "Electronics3", "quantity": 8}])
        self.assertEqual(binding["duration_seconds"], 20)
        self.assertEqual(binding["output_per_hour"], 1440)
        self.assertEqual(
            sum(
                item["quantity"]
                for item in binding["representative_demand_per_hour"]
            ),
            4720,
        )

    def test_reference_network_capacity_and_package_closure_are_exact(self) -> None:
        closure = CONTROL["capacity_closure"]
        rate = CONTROL["consumer_contract"]["stream_units_per_active_minute"]
        gateway_rate = CONTROL["gateway"]["recipe"]["output"]["quantity"]
        gateway_seconds = CONTROL["gateway"]["recipe"]["duration_seconds"]
        gateway_per_minute = Fraction(gateway_rate * 60, gateway_seconds)

        self.assertEqual(rate, 60)
        self.assertEqual(closure["optimized_owner_count"] * rate, 660)
        self.assertEqual(closure["all_owner_demand_per_minute"], 660)
        self.assertEqual(closure["minimum_gateway_count"], 4)
        self.assertEqual(closure["federated_gateway_count"], 2)
        self.assertEqual(200 // rate, closure["access_supported_facilities"])
        self.assertEqual(200 % rate, closure["access_spare_per_minute"])
        self.assertEqual(450 // rate, closure["backbone_supported_facilities"])
        self.assertEqual(450 % rate, closure["backbone_spare_per_minute"])
        self.assertEqual(
            Fraction(closure["all_owner_demand_per_minute"] * 60, gateway_per_minute),
            Fraction(closure["steady_state_packages_per_hour"]),
        )
        self.assertEqual(
            Fraction(closure["minimum_gateway_count"] * 60),
            closure["unconstrained_gateway_packages_per_hour"],
        )
        self.assertGreater(
            Fraction(closure["steady_state_packages_per_hour"]),
            closure["validator_capacity_packages_per_hour"],
        )
        self.assertLess(
            Fraction(closure["steady_state_packages_per_hour"]),
            2 * closure["validator_capacity_packages_per_hour"],
        )
        federated = closure["federated_topology"]
        local_only = closure["local_only_topology"]
        self.assertEqual(federated["backbone_consumers"] * rate, 660)
        self.assertEqual(federated["local_consumers"] * rate, 0)
        self.assertEqual(federated["output_capacity_per_minute"], 840)
        self.assertEqual(federated["transport_capacity_per_minute"], 900)
        self.assertEqual(local_only["output_capacity_per_minute"], 840)
        self.assertEqual(local_only["transport_capacity_per_minute"], 900)
        self.assertEqual(
            Fraction(federated["steady_state_packages_per_hour"]),
            Fraction(local_only["steady_state_packages_per_hour"]),
        )
        self.assertGreater(federated["power_mw"], local_only["power_mw"])
        self.assertLess(federated["workers"], local_only["workers"])
        scale_up = closure["package_scale_up"]
        self.assertEqual(
            Fraction(scale_up["selected_mature_core_packages_per_hour"])
            + Fraction(scale_up["industrial_control_packages_per_hour"]),
            Fraction(scale_up["mature_core_with_control_packages_per_hour"]),
        )
        self.assertEqual(scale_up["mature_core_with_control_standard_validators"], 4)
        self.assertEqual(scale_up["mature_core_with_control_dense_capacity_per_hour"], 640)
        self.assertEqual(
            Fraction(scale_up["mature_core_with_control_packages_per_hour"])
            + scale_up["planetary_center_packages_per_hour"],
            Fraction(scale_up["mature_core_center_control_packages_per_hour"]),
        )
        self.assertEqual(scale_up["mature_core_center_control_standard_validators"], 8)
        self.assertEqual(
            scale_up["mature_core_center_control_dense_capacity_per_hour"],
            1280,
        )

    def test_research_gate_and_children_are_catalog_bound(self) -> None:
        research = CONTROL["research"]
        self.assertEqual(research["position"], {"x": 212, "y": 24})
        self.assertEqual(research["duration_months"], 360)
        self.assertEqual(
            research["parent_registration_id"],
            "RecursiveIndustry_RecursiveEpochV",
        )
        self.assertTrue(research["requires_space_points"])
        self.assertEqual(research["child_branch_keys"], CATALOG["research_keys"])
        self.assertEqual(
            set(research["unlocks"]),
            {
                "RecursiveIndustry_IndustrialControlStream",
                "RecursiveIndustry_ControlDeploymentGateway",
                "RecursiveIndustry_DeployIndustrialControl",
                "RecursiveIndustry_IntegrateElectronics2Direct",
                "RecursiveIndustry_IntegrateConstructionParts3",
                "RecursiveIndustry_IntegrateVehicleParts2",
                "RecursiveIndustry_AccessFiber",
                "RecursiveIndustry_BackboneFiber",
                "RecursiveIndustry_FiberJunction",
            },
        )
        federated = CONTROL["federated_deployment"]
        self.assertEqual(federated["position"], {"x": 212, "y": 30})
        self.assertEqual(federated["duration_months"], 480)
        self.assertEqual(
            federated["parent_registration_id"],
            "RecursiveIndustry_IndustrialControlNetworks",
        )
        self.assertTrue(federated["requires_space_points"])
        self.assertEqual(federated["additional_lifetime_requirements"], [])
        self.assertEqual(
            federated["transitive_campaign_gate"],
            {
                "research_registration_id": "RecursiveIndustry_RecursiveEpochV",
                "product_key": "FrontierProgram",
                "quantity": 256,
                "reason": (
                    "Industrial Control Networks already descends from Epoch V; "
                    "repeating weaker lifetime conditions only blocks sandbox and "
                    "migrated research states"
                ),
            },
        )
        self.assertEqual(len(federated["unlocks"]), 3)

    def test_research_coordinate_is_unique_and_at_least_four_units_away(self) -> None:
        source_root = ROOT / "mods" / "RecursiveIndustry" / "src"
        source = "\n".join(
            path.read_text(encoding="utf-8")
            for path in source_root.glob("*.cs")
        )
        positions = [
            (int(x), int(y))
            for x, y in re.findall(
                r"GridPosition\s*=\s*new Vector2i\((\d+),\s*(\d+)\)",
                source,
            )
        ]
        target = (
            CONTROL["research"]["position"]["x"],
            CONTROL["research"]["position"]["y"],
        )
        federated_target = (
            CONTROL["federated_deployment"]["position"]["x"],
            CONTROL["federated_deployment"]["position"]["y"],
        )
        for selected in (target, federated_target):
            self.assertEqual(positions.count(selected), 1)
            for position in positions:
                if position == selected:
                    continue
                distance_squared = (
                    (position[0] - selected[0]) ** 2
                    + (position[1] - selected[1]) ** 2
                )
                self.assertGreaterEqual(distance_squared, 16)


if __name__ == "__main__":
    unittest.main()