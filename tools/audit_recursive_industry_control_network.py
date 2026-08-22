#!/usr/bin/env python3
"""Audit the public Industrial Control network against its schema-1 authority."""

from __future__ import annotations

import json
from fractions import Fraction
from pathlib import Path
import re
from typing import Any, Iterable


ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "mods" / "RecursiveIndustry" / "src"
CONTROL_PATH = ROOT / "data" / "industrial-control-network.json"
CATALOG_PATH = ROOT / "data" / "universal-industry-catalog.json"
MANIFEST_PATH = ROOT / "mods" / "RecursiveIndustry" / "manifest.json"

REQUIRED_SOURCE_FILES = {
    "DataProductProto.cs",
    "IndustrialControlProductData.cs",
    "IndustrialControlTransportData.cs",
    "IndustrialControlGatewayData.cs",
    "DeploymentAssuranceData.cs",
    "RecursiveIndustryIds.Infrastructure.cs",
}

FORBIDDEN_RUNTIME_TOKENS = {
    "AutomaticDirectFallback": "automatic Direct fallback",
    "BindDirectWithIndustrialControlStream": "Direct Stream input",
    "IndustrialControlPacketService": "generic packet service",
    "ReverseIndustrialControl": "reverse Stream learning",
    "HarmonyPatch": "runtime patch",
}


def load_json(path: Path) -> dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"JSON root must be an object: {path}")
    return value


def require_tokens(
    errors: list[str],
    label: str,
    text: str,
    tokens: Iterable[str],
) -> None:
    for token in tokens:
        if token not in text:
            errors.append(f"{label} missing exact declaration: {token}")


def audit_data_product_source(text: str) -> list[str]:
    errors: list[str] = []
    declarations = re.findall(
        r"class\s+DataProductProto\s*:\s*([A-Za-z0-9_.]+)",
        text,
    )
    if declarations != ["ProductProto"]:
        errors.append(
            "DataProductProto must be declared exactly once and inherit ProductProto"
        )
    if "FluidProductProto" in text:
        errors.append("DataProductProto must not inherit or alias FluidProductProto")
    require_tokens(
        errors,
        "DataProductProto",
        text,
        (
            "new ProductType(typeof(DataProductProto))",
            "isStorable: false",
            "canBeDiscarded: false",
            "isWaste: false",
            "isRecyclable: false",
            "NoUnitsQuantityFormatter.Instance",
        ),
    )
    return errors


def audit_product_source(text: str) -> list[str]:
    errors: list[str] = []
    require_tokens(
        errors,
        "Industrial Control product",
        text,
        (
            "new DataProductProto(",
            "RecursiveIndustryIds.Products.IndustrialControlStream",
            '"Industrial Control Stream"',
            "RecursiveIndustryIcons.IndustrialControlStream",
        ),
    )
    return errors


def audit_transport_source(text: str) -> list[str]:
    errors: list[str] = []
    require_tokens(
        errors,
        "Industrial Control transport",
        text,
        (
            "AccessCapacityPer60 = 200",
            "BackboneCapacityPer60 = 450",
            "new IoPortShapeProto(",
            "RecursiveIndustryIds.Infrastructure.Data",
            "':'",
            "DataProductProto.ProductType",
            "Ids.Transports.PipeT2",
            "Ids.Transports.PipeT3",
            "RecursiveIndustryIcons.AccessFiber",
            "RecursiveIndustryIcons.BackboneFiber",
            "RecursiveIndustryIcons.FiberJunction",
            "allowMixedProducts: false",
            "accessFiber.SetNextTier(backboneFiber)",
            "new MiniZipperProto(",
            "GetMaxThroughputPer60For(data)",
        ),
    )
    if "allowMixedProducts: true" in text:
        errors.append("Fiber transports must not allow mixed products")
    if text.count("new IoPortShapeProto(") != 1:
        errors.append("Industrial Control must declare exactly one Data port shape")
    return errors


def audit_gateway_source(text: str) -> list[str]:
    errors: list[str] = []
    require_tokens(
        errors,
        "Control Deployment Gateway",
        text,
        (
            'Start("Control Deployment Gateway",',
            "RecursiveIndustryIds.Machines.ControlDeploymentGateway",
            ".CP4(640)",
            ".Product(128, Ids.Products.Electronics4)",
            ".Product(32, RecursiveIndustryIds.Products.ValidatedControlPackage)",
            ".Product(4, RecursiveIndustryIds.Products.FrontierProgram)",
            ".Product(4, RecursiveIndustryIds.Products.ValidatedResearchDossier)",
            ".Workers(24)",
            ".MaintenanceT3(12)",
            ".SetElectricityConsumption(4000.Kw())",
            ".SetComputingConsumption(Computing.FromTFlops(256))",
            '"A#>[4][4][4][4][4][4]>:X"',
            ".SetCustomIconPath(RecursiveIndustryIcons.ControlDeploymentGateway)",
            ".AddInput(1, RecursiveIndustryIds.Products.ValidatedControlPackage)",
            ".AddOutput(210, RecursiveIndustryIds.Products.IndustrialControlStream)",
            "RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl",
            ".AddInput(2, RecursiveIndustryIds.Products.ValidatedControlPackage)",
            ".AddOutput(420, RecursiveIndustryIds.Products.IndustrialControlStream)",
            ".SetPowerMultiplier(250.Percent())",
            "WithCommonInputPorts((RecursiveIndustryIds.Products.ValidatedControlPackage, \"A\"))",
            "WithCommonOutputPorts((RecursiveIndustryIds.Products.IndustrialControlStream, \"X\"))",
            ".BindTo(gateway, 60.Seconds())",
        ),
    )
    return errors


def audit_assurance_source(text: str) -> list[str]:
    errors: list[str] = []
    require_tokens(
        errors,
        "Deployment Assurance Campus",
        text,
        (
            '"Deployment Assurance Campus"',
            "RecursiveIndustryIds.Machines.DeploymentAssuranceCampus",
            ".CP4(1200)",
            ".Product(256, Ids.Products.Electronics4)",
            ".Product(64, RecursiveIndustryIds.Products.ValidatedControlPackage)",
            ".Product(16, RecursiveIndustryIds.Products.FrontierProgram)",
            ".Product(8, RecursiveIndustryIds.Products.ValidatedResearchDossier)",
            ".Workers(48)",
            ".MaintenanceT3(16)",
            ".SetElectricityConsumption(4000.Kw())",
            ".SetComputingConsumption(Computing.FromTFlops(256))",
            "RecursiveIndustryIcons.DeploymentAssuranceCampus",
            "RecursiveIndustryIds.Recipes.BatchDeploymentAssurance",
            ".AddInput(16, RecursiveIndustryIds.Products.ModelArchive)",
            ".AddInput(32, Ids.Products.LabEquipment4)",
            ".AddInput(32, Ids.Products.Electronics3)",
            ".AddOutput(128, RecursiveIndustryIds.Products.ValidatedControlPackage)",
            ".BindTo(campus, 720.Seconds())",
        ),
    )
    if "AddInput(" in text and "ValidatedResearchDossier" in text.split(
        "BatchDeploymentAssurance", 1
    )[-1]:
        errors.append("Batch Deployment Assurance must not consume recurring Dossiers")
    return errors


def audit_catalog(
    catalog: dict[str, Any],
    control: dict[str, Any],
) -> list[str]:
    errors: list[str] = []
    facilities = catalog.get("facilities", [])
    if not isinstance(facilities, list) or len(facilities) != 19:
        return ["universal catalog must contain exactly 19 facilities"]

    direct = [
        (facility.get("key"), binding)
        for facility in facilities
        for binding in facility.get("direct_bindings", [])
    ]
    expected_direct = control["direct_contract"]["expected_binding_count"]
    if len(direct) != expected_direct:
        errors.append(
            f"universal catalog must contain {expected_direct} Direct bindings, found {len(direct)}"
        )
    direct_ids = [binding.get("recipe_id") for _, binding in direct]
    if len(set(direct_ids)) != len(direct_ids):
        errors.append("universal Direct recipe ids must be unique")

    expected_e3 = control["direct_contract"]["electronics_iii_binding"]
    e3 = [
        (owner, binding)
        for owner, binding in direct
        if binding.get("recipe_id") == expected_e3["recipe_id"]
    ]
    if e3 != [
        (
            expected_e3["owner_key"],
            {
                "recipe_id": expected_e3["recipe_id"],
                "source_machine_id": expected_e3["source_machine_id"],
            },
        )
    ]:
        errors.append(
            "Electronics3Assembly must be owned only by precision_components_fab"
        )

    controlled = control["consumer_contract"]["recipes"]
    catalog_integrated = catalog.get("integrated_recipes", [])
    catalog_precision = catalog.get("precision_recipes", [])
    controlled_by_key = {recipe["key"]: recipe for recipe in controlled}
    catalog_keys = {recipe.get("key") for recipe in catalog_integrated}
    expected_custom = {
        (key, controlled_by_key[key]["owner_key"])
        for key in catalog_keys
        if key in controlled_by_key
    }
    actual_custom = {
        (recipe.get("key"), recipe.get("machine"))
        for recipe in catalog_integrated
    }
    if actual_custom != expected_custom:
        errors.append("controlled recipe keys or owners drift from the control contract")
    if len(controlled) != 24 or len(controlled_by_key) != 24:
        errors.append("control contract must contain 24 unique Stream recipes")
    if control["consumer_contract"].get("universal_catalog_recipe_count") != 21:
        errors.append("control contract must declare 21 universal compositions")
    if control["consumer_contract"].get("authored_legacy_recipe_count") != 3:
        errors.append("control contract must declare three authored legacy compositions")
    if len(catalog_integrated) != 21:
        errors.append("universal catalog must contain exactly 21 Integrated recipes")
    legacy_keys = set(controlled_by_key) - catalog_keys
    if legacy_keys != {
        "integrated_electronics2",
        "integrated_construction_parts3",
        "integrated_vehicle_parts2",
    }:
        errors.append("authored legacy Stream recipe inventory drift")
    if len(catalog_precision) != 10:
        errors.append("universal catalog must retain exactly 10 Precision recipes")
    if control.get("precision_contract", {}).get("stream_inputs_per_recipe") != 0:
        errors.append("Precision recipes must remain Fiber-free")
    if control.get("precision_contract", {}).get("recipe_keys") != [
        recipe.get("key") for recipe in catalog_precision
    ]:
        errors.append("Precision recipe inventory drift")
    if any(not recipe.get("cancelled_intermediates") for recipe in controlled):
        errors.append("every Stream recipe must cancel a transported intermediate")
    if any(
        recipe.get("stream_quantity") != recipe.get("effective_duration_seconds")
        for recipe in controlled
    ):
        errors.append("every Stream recipe must consume one Stream per active second")

    owners = {row.get("key"): row for row in control.get("owners", [])}
    if owners.get("precision_components_fab") != {
        "key": "precision_components_fab",
        "material_input_ports": 8,
        "input_ports_with_data": 9,
        "output_ports": 1,
        "physical_layout_rows": 7,
        "right_side_input_ports": 2,
        "top_side_input_ports": 0,
    }:
        errors.append("Precision Components runtime-observed port union drift")
    if owners.get("general_manufacturing_fab") != {
        "key": "general_manufacturing_fab",
        "material_input_ports": 10,
        "input_ports_with_data": 11,
        "output_ports": 4,
        "physical_layout_rows": 7,
        "right_side_input_ports": 3,
        "top_side_input_ports": 1,
    }:
        errors.append("General Manufacturing top-edge port union drift")

    expected_advanced = {
        "integrated_electronics3": {
            "machine": "precision_components_fab",
            "batch_scale": 1,
            "duration_seconds": 120,
            "sources": [
                {"recipe_id": "ElectronicsAssembly", "multiplier": 4},
                {"recipe_id": "PCBAssembly", "multiplier": 6},
                {
                    "recipe_id": "Electronics2Assembly",
                    "multiplier": 12,
                    "source_machine_id": "AssemblyRoboticT2",
                },
                {"recipe_id": "Electronics3Assembly", "multiplier": 12},
            ],
        },
        "integrated_lab_equipment2": {
            "machine": "general_manufacturing_fab",
            "batch_scale": 1,
            "duration_seconds": 120,
            "sources": [
                {"recipe_id": "LabEquipment1Assembly", "multiplier": 15},
                {"recipe_id": "LabEquipment2Assembly", "multiplier": 30},
            ],
        },
        "integrated_lab_equipment3": {
            "machine": "general_manufacturing_fab",
            "batch_scale": 1,
            "duration_seconds": 120,
            "sources": [
                {"recipe_id": "LabEquipment1Assembly", "multiplier": 15},
                {"recipe_id": "LabEquipment2Assembly", "multiplier": 30},
                {"recipe_id": "LabEquipment3Assembly", "multiplier": 30},
            ],
        },
        "integrated_lab_equipment4": {
            "machine": "general_manufacturing_fab",
            "batch_scale": 1,
            "duration_seconds": 180,
            "sources": [
                {"recipe_id": "LabEquipment1Assembly", "multiplier": 16},
                {"recipe_id": "LabEquipment2Assembly", "multiplier": 32},
                {"recipe_id": "LabEquipment3Assembly", "multiplier": 32},
                {"recipe_id": "LabEquipment4Assembly", "multiplier": 24},
            ],
        },
    }
    catalog_by_key = {recipe["key"]: recipe for recipe in catalog_integrated}
    for key, expected in expected_advanced.items():
        actual = catalog_by_key.get(key)
        if actual is None or any(
            actual.get(field) != value for field, value in expected.items()
        ):
            errors.append(f"{key} exact source-chain contract drift")

    directed = control.get("directed_refinery_contract", {})
    directed_modes = directed.get("modes", [])
    directed_keys = {
        "integrated_refinery_diesel",
        "integrated_refinery_gas",
        "integrated_refinery_hydrogen",
        "integrated_refinery_plastic",
        "integrated_refinery_rubber",
    }
    forbidden_outputs = set(directed.get("forbidden_intermediate_outputs", []))
    if (
        directed.get("owner_key") != "refinery_complex"
        or directed.get("throughput_anchor")
        != {
            "direction": "input",
            "product": "Crude Oil",
            "source_recipe_id": "CrudeOilRefiningT1",
            "multiplier": 2,
        }
        or directed.get("stream_units_per_active_minute") != 60
        or {row.get("key") for row in directed_modes} != directed_keys
        or forbidden_outputs
        != {"Heavy Oil", "Medium Oil", "Light Oil", "Naphtha"}
    ):
        errors.append("directed refinery contract inventory or anchor drift")
    for row in directed_modes:
        if (
            row.get("stream_quantity") != row.get("duration_seconds")
            or not forbidden_outputs.isdisjoint(row.get("outputs", {}))
        ):
            errors.append(
                f"{row.get('key')} Stream cadence or intermediate-output drift"
            )

    expected_owners = {owner["key"] for owner in control["owners"]}
    actual_controlled_owners = {recipe.get("owner_key") for recipe in controlled}
    if len(expected_owners) != 11 or actual_controlled_owners != expected_owners:
        errors.append("exactly the eleven contracted facilities must own Stream modes")
    actual_catalog_owners = {recipe.get("machine") for recipe in catalog_integrated}
    expected_catalog_owners = {
        owner["key"]
        for owner in control["owners"]
        if owner.get("catalog_facility", True)
    }
    if len(actual_catalog_owners) != 9 or actual_catalog_owners != expected_catalog_owners:
        errors.append("exactly nine universal facilities must own catalog compositions")
    if len({facility.get("key") for facility in facilities} - actual_catalog_owners) != 10:
        errors.append("exactly ten facilities must remain without a Data port")
    right_side_owners = {
        owner["key"]
        for owner in control["owners"]
        if owner.get("right_side_input_ports", 0) > 0
    }
    if right_side_owners != {
        "primary_smelter",
        "food_pack_campus",
        "nuclear_fuel_complex",
        "precision_components_fab",
        "general_manufacturing_fab",
    }:
        errors.append("right-side input owner inventory drift")
    for owner in control["owners"]:
        material = owner.get("material_input_ports")
        inputs = owner.get("input_ports_with_data")
        outputs = owner.get("output_ports")
        physical_rows = owner.get("physical_layout_rows")
        right_inputs = owner.get("right_side_input_ports")
        top_inputs = owner.get("top_side_input_ports", 0)
        if not all(
            isinstance(value, int)
            for value in (
                material,
                inputs,
                outputs,
                physical_rows,
                right_inputs,
                top_inputs,
            )
        ):
            errors.append(f"{owner.get('key')} port contract must contain integers")
            continue
        if inputs != material + 1:
            errors.append(f"{owner['key']} must add exactly one Data input")
        overflow_inputs = max(0, inputs - physical_rows)
        expected_right_inputs = min(
            overflow_inputs,
            physical_rows - outputs,
        )
        if (
            physical_rows > 7
            or right_inputs != expected_right_inputs
            or top_inputs != overflow_inputs - expected_right_inputs
        ):
            errors.append(f"{owner['key']} physical or right-side row contract drift")
        if outputs + right_inputs > physical_rows:
            errors.append(f"{owner['key']} right edge is oversubscribed")
        if top_inputs > 7:
            errors.append(f"{owner['key']} top edge is oversubscribed")
    return errors


def audit_capacity(control: dict[str, Any]) -> list[str]:
    errors: list[str] = []
    consumer = control["consumer_contract"]
    closure = control["capacity_closure"]
    owner_count = len(control["owners"])
    demand = owner_count * consumer["stream_units_per_active_minute"]
    if consumer.get("concurrent_demand_basis") != "one active recipe per owner":
        errors.append("Stream demand must be based on one active recipe per owner")
    if (
        owner_count != 11
        or closure.get("optimized_owner_count") != owner_count
        or closure.get("all_owner_demand_per_minute") != demand
        or demand != 660
    ):
        errors.append("eleven-owner Stream demand must equal 660/minute")
    gateway = control["gateway"]
    stream_per_package = Fraction(
        gateway["recipe"]["output"]["quantity"],
        gateway["recipe"]["input"]["quantity"],
    )
    package_demand = Fraction(demand * 60, stream_per_package)
    if (
        stream_per_package != 210
        or Fraction(closure.get("steady_state_packages_per_hour")) != package_demand
        or package_demand != Fraction(1320, 7)
    ):
        errors.append("control network must consume 1320/7 Packages/hour")
    federated = closure.get("federated_topology", {})
    local = closure.get("local_only_topology", {})
    if federated != {
        "backbone_gateway_count": 2,
        "local_gateway_count": 0,
        "backbone_consumers": 11,
        "local_consumers": 0,
        "output_capacity_per_minute": 840,
        "transport_capacity_per_minute": 900,
        "packages_per_hour_at_full_output": 240,
        "steady_state_packages_per_hour": "1320/7",
        "power_mw": 20,
        "computing": 512,
        "workers": 48,
        "maintenance_iii_per_month": 24,
    }:
        errors.append("two-Backbone federated topology drift")
    if (
        local.get("gateway_count") != 4
        or local.get("output_capacity_per_minute") != 840
        or local.get("transport_capacity_per_minute") != 900
    ):
        errors.append("four-local-Gateway comparison topology drift")
    return errors


def audit_universal_source(text: str, generated_catalog: str) -> list[str]:
    errors: list[str] = []
    require_tokens(
        errors,
        "Universal Industrial Control integration",
        text,
        (
            "expected 235 direct bindings",
            "ResolveEffectiveDuration(",
            "GetMaterialTransportDurationFloor(",
            "AppendIndustrialControlInput(",
            "SetPowerMultiplier(powerMultiplierPercent.Percent())",
            "effectiveDuration.SecondsFloored",
            "GetDataTransportDurationFloor(",
            "amount.Product is DataProductProto",
            "private static readonly char[] KindOrder = { '#', '~', '\\'', '@', ':' }",
            "RightSideInputPorts",
            "TopSideInputPorts",
            "rightSideInputs = Math.Min(",
            "bodyRows - flatOutputs.Count",
            "topSideInputs = overflowInputs - rightSideInputs",
            "BuildTopInputRow(",
            'port.name + port.kind + "v"',
            '"<" + flatInputs[rightInputIndex].kind + flatInputs[rightInputIndex].name',
        ),
    )
    if "rows > 7" in text:
        errors.append("Universal source still rejects logical eight-input layouts")
    if generated_catalog.count("new UniversalDirectBindingSpec(") != 235:
        errors.append("generated catalog must contain exactly 235 Direct bindings")

    direct_loop = re.search(
        r"foreach \(ResolvedDirectBinding binding in direct\)(.*?)"
        r"foreach \(ResolvedCustomRecipe recipe in custom\)",
        text,
        flags=re.DOTALL,
    )
    if direct_loop is None:
        errors.append("could not locate the Direct binding loop")
    elif "IndustrialControlStream" in direct_loop.group(1):
        errors.append("Direct bindings must not consume Industrial Control Stream")
    if "AutomaticDirectFallback" in text:
        errors.append("Universal source must not implement automatic Direct fallback")
    precision_method = re.search(
        r"private static ResolvedCustomRecipe ResolvePrecisionRecipe\((.*?)"
        r"private static Duration ResolveEffectiveDuration\(",
        text,
        flags=re.DOTALL,
    )
    if precision_method is None:
        errors.append("could not locate Precision recipe resolver")
    elif "AppendIndustrialControlInput" in precision_method.group(1):
        errors.append("Precision recipes must not append Industrial Control Stream")
    return errors


def audit_research_source(text: str) -> list[str]:
    errors: list[str] = []
    require_tokens(
        errors,
        "Industrial Control research",
        text,
        (
            '"Industrial Control Networks"',
            "RecursiveIndustryIds.Research.IndustrialControlNetworks",
            "costMonths: 360",
            "industrialControl.GridPosition = new Vector2i(212, 24)",
            "industrialControl.AddParent(recursiveEpochV)",
            ".AddProductToUnlock(RecursiveIndustryIds.Products.IndustrialControlStream)",
            ".AddMachineToUnlock(",
            "RecursiveIndustryIds.Machines.ControlDeploymentGateway",
            "unlockAllRecipes: false",
            ".AddRecipeToUnlock(RecursiveIndustryIds.Recipes.DeployIndustrialControl)",
            "RecursiveIndustryIds.Recipes.IntegrateElectronics2Direct",
            "RecursiveIndustryIds.Recipes.IntegrateConstructionParts3",
            "RecursiveIndustryIds.Recipes.IntegrateVehicleParts2",
            ".AddProtoToUnlock<TransportProto>(RecursiveIndustryIds.Infrastructure.AccessFiber)",
            ".AddProtoToUnlock<TransportProto>(RecursiveIndustryIds.Infrastructure.BackboneFiber)",
            ".AddLayoutEntityToUnlock(RecursiveIndustryIds.Infrastructure.FiberJunction)",
            ".SetRequireSpacePoints()",
            '"Federated Deployment"',
            "RecursiveIndustryIds.Research.FederatedDeployment",
            "costMonths: 480",
            "federatedDeployment.GridPosition = new Vector2i(212, 30)",
            "federatedDeployment.AddParent(industrialControl)",
            "RecursiveIndustryIds.Machines.DeploymentAssuranceCampus",
            "RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl",
                "RecursiveIndustryIds.Recipes.IntegratedRefineryDiesel",
                "RecursiveIndustryIds.Recipes.IntegratedRefineryGas",
                "RecursiveIndustryIds.Recipes.IntegratedRefineryHydrogen",
                "RecursiveIndustryIds.Recipes.IntegratedRefineryPlastic",
                "RecursiveIndustryIds.Recipes.IntegratedRefineryRubber",
                "RecursiveIndustryIds.Recipes.IntegratedElectronics3",
                "RecursiveIndustryIds.Recipes.IntegratedLabEquipment2",
                "RecursiveIndustryIds.Recipes.IntegratedLabEquipment3",
                "RecursiveIndustryIds.Recipes.IntegratedLabEquipment4",
        ),
    )
    federated_section = text[
        text.find("ResearchNodeProto federatedDeployment"):
        text.find("ResearchNodeProto materials")
    ]
    if not federated_section:
        errors.append("could not locate Federated Deployment research section")
    elif "AddRequirementForLifetimeProduction" in federated_section:
        errors.append(
            "Federated Deployment must not duplicate Epoch V lifetime requirements"
        )
    if text.count(".AddParent(industrialControl)") != 6:
        errors.append(
            "Federated Deployment and all five universal branches must parent Industrial Control Networks"
        )
    branch_section = text[text.find("ResearchNodeProto materials") :]
    if ".AddParent(recursiveEpochV)" in branch_section:
        errors.append("universal research branches must not parent Recursive Epoch V directly")
    return errors


def recipe_block(text: str, member: str) -> str | None:
    match = re.search(
        r"RecursiveIndustryIds\.Recipes\s*\.\s*" + re.escape(member),
        text,
    )
    if match is None:
        return None
    end = text.find(".BindTo(", match.end())
    if end < 0:
        return None
    end = text.find(";", end)
    return text[match.start() : end + 1] if end >= 0 else None


def audit_physical_recipe_semantics(source_files: dict[str, str]) -> list[str]:
    errors: list[str] = []
    recipes = {
        "AIElectronicsCellData.cs": (
            "PrecisionElectronics3",
            "ThroughputElectronics3",
        ),
        "AutonomousMicrochipData.cs": ("IntegrateAutonomousMicrochips",),
        "AutonomousElectronicsIntegrationData.cs": (
            "IntegrateElectronics2Intermediates",
            "IntegrateElectronics2Direct",
        ),
        "AutonomousCapitalFabricationData.cs": (
            "FabricateConstructionParts",
            "FabricateConstructionParts2",
            "FabricateConstructionParts3",
            "IntegrateConstructionParts3",
            "FabricateVehicleParts",
            "FabricateVehicleParts2",
            "IntegrateVehicleParts2",
        ),
        "RecursiveFrontierData.cs": (
            "ProduceRecursiveConstructionParts4",
            "ProducePrecisionConstructionParts4",
            "RecoverConstructionParts4",
            "ProduceRecursiveVehicleParts3",
            "ProducePrecisionVehicleParts3",
            "RecoverVehicleParts3",
        ),
    }
    stream_recipes = {
        "IntegrateElectronics2Direct": ("SetPowerMultiplier(200.Percent())", "F"),
        "IntegrateConstructionParts3": ("SetPowerMultiplier(200.Percent())", "E"),
        "IntegrateVehicleParts2": ("SetPowerMultiplier(150.Percent())", "E"),
    }
    for filename, members in recipes.items():
        text = source_files.get(filename, "")
        for member in members:
            block = recipe_block(text, member)
            if block is None:
                errors.append(f"could not locate ordinary manufacturing recipe {member}")
                continue
            if "ValidatedControlPackage" in block:
                errors.append(f"{member} must not consume recurring Packages")
            if member in stream_recipes:
                power, port = stream_recipes[member]
                if (
                    "IndustrialControlStream" not in block
                    or power not in block
                    or (
                        "RecursiveIndustryIds.Products.IndustrialControlStream, "
                        f'"{port}"'
                    ) not in block
                ):
                    errors.append(f"{member} Stream, Data port, or power contract drift")
            elif "IndustrialControlStream" in block:
                errors.append(f"{member} must remain Stream-free")
    layout_contracts = {
        "AIElectronicsCellData.cs": "VerticalSliceProofLayout.Create(includeThirdInput: false)",
        "AutonomousMicrochipLayout.cs": '"      D@vF#vB#v   E@v      "',
        "ConstructionNexusLayout.cs": '"   [4][4][4][4][4][4]   "',
    }
    for filename, token in layout_contracts.items():
        if token not in source_files.get(filename, ""):
            errors.append(f"{filename} obsolete Package port removal drift")
    return errors


def audit_legacy_research_source(text: str) -> list[str]:
    errors: list[str] = []
    electronics = text[
        text.find("ResearchNodeProto autonomousElectronicsIntegration") :
        text.find("ResearchNodeProto recursiveEpochIII")
    ]
    capital = text[
        text.find("ResearchNodeProto autonomousCapitalFabrication") :
        text.find("ResearchNodeProtoBuilder.State heavyEquipmentBuilder")
    ]
    for label, section, required, forbidden in (
        (
            "Autonomous Electronics Integration",
            electronics,
            ("unlockAllRecipes: false", "IntegrateElectronics2Intermediates"),
            ("IntegrateElectronics2Direct",),
        ),
        (
            "Autonomous Capital Fabrication",
            capital,
            (
                "unlockAllRecipes: false",
                "FabricateConstructionParts",
                "FabricateConstructionParts2",
                "FabricateConstructionParts3",
                "FabricateVehicleParts",
                "FabricateVehicleParts2",
            ),
            ("IntegrateConstructionParts3", "IntegrateVehicleParts2"),
        ),
    ):
        if not section:
            errors.append(f"could not locate {label} research section")
            continue
        require_tokens(errors, label, section, required)
        for token in forbidden:
            if token in section:
                errors.append(f"{label} must not unlock late Stream recipe {token}")
    return errors


def audit_registration_source(text: str) -> list[str]:
    errors: list[str] = []
    ordered = (
        "RegisterData<IndustrialControlProductData>()",
        "RegisterData<IndustrialControlTransportData>()",
        "RegisterData<IndustrialControlGatewayData>()",
        "RegisterData<DeploymentAssuranceData>()",
        "RegisterData<UniversalIndustryData>()",
        "RegisterDataWithInterface<IResearchNodesData>()",
    )
    positions = [text.find(token) for token in ordered]
    if any(position < 0 for position in positions):
        errors.append("Industrial Control registration sequence is incomplete")
    elif positions != sorted(positions):
        errors.append(
            "Industrial Control must register Product, Transport, Gateway, Assurance, Universal, Research"
        )
    return errors


def audit_forbidden_runtime(source_files: dict[str, str]) -> list[str]:
    errors: list[str] = []
    for name, text in source_files.items():
        for token, label in FORBIDDEN_RUNTIME_TOKENS.items():
            if token in text:
                errors.append(f"{name} introduces forbidden {label}: {token}")
    return errors


def audit(root: Path = ROOT) -> list[str]:
    errors: list[str] = []
    source_root = root / "mods" / "RecursiveIndustry" / "src"
    source_files = {
        path.name: path.read_text(encoding="utf-8")
        for path in source_root.glob("*.cs")
    }
    missing = REQUIRED_SOURCE_FILES - set(source_files)
    if missing:
        errors.append(f"missing Industrial Control source files: {sorted(missing)}")

    control = load_json(root / "data" / "industrial-control-network.json")
    catalog = load_json(root / "data" / "universal-industry-catalog.json")
    if control.get("schema_version") != 1:
        errors.append("industrial-control-network schema must be 1")
    errors.extend(audit_catalog(catalog, control))
    errors.extend(audit_capacity(control))

    combined = "\n".join(source_files.values())
    declarations = re.findall(r"class\s+DataProductProto\b", combined)
    if len(declarations) != 1:
        errors.append(
            f"public source must contain exactly one DataProductProto class, found {len(declarations)}"
        )

    if "DataProductProto.cs" in source_files:
        errors.extend(audit_data_product_source(source_files["DataProductProto.cs"]))
    if "IndustrialControlProductData.cs" in source_files:
        errors.extend(audit_product_source(source_files["IndustrialControlProductData.cs"]))
    if "IndustrialControlTransportData.cs" in source_files:
        errors.extend(audit_transport_source(source_files["IndustrialControlTransportData.cs"]))
    if "IndustrialControlGatewayData.cs" in source_files:
        errors.extend(audit_gateway_source(source_files["IndustrialControlGatewayData.cs"]))
    if "DeploymentAssuranceData.cs" in source_files:
        errors.extend(audit_assurance_source(source_files["DeploymentAssuranceData.cs"]))
    if "UniversalIndustryData.cs" in source_files:
        errors.extend(audit_universal_source(
            source_files["UniversalIndustryData.cs"],
            source_files.get("UniversalIndustryCatalog.g.cs", ""),
        ))
    if "UniversalIndustryResearchData.cs" in source_files:
        errors.extend(audit_research_source(
            source_files["UniversalIndustryResearchData.cs"]
        ))
    if "RecursiveIndustryResearchData.cs" in source_files:
        errors.extend(audit_legacy_research_source(
            source_files["RecursiveIndustryResearchData.cs"]
        ))
    if "RecursiveIndustry.cs" in source_files:
        errors.extend(audit_registration_source(source_files["RecursiveIndustry.cs"]))
    errors.extend(audit_forbidden_runtime(source_files))
    errors.extend(audit_physical_recipe_semantics(source_files))

    manifest = load_json(root / "mods" / "RecursiveIndustry" / "manifest.json")
    dependencies = manifest.get("mod_dependencies", []) + manifest.get(
        "optional_mod_dependencies", []
    )
    if any("FiberProof" in dependency for dependency in dependencies):
        errors.append("the standalone Fiber proof must not be a player dependency")
    return errors


def main() -> int:
    errors = audit()
    if errors:
        print("Recursive Industry Industrial Control network: FAIL")
        for error in errors:
            print(f"  ERROR: {error}")
        return 1
    print(
        "Recursive Industry Industrial Control network: PASS "
        "(235 Direct, 21 universal plus 3 legacy compositions, "
        "10 Fiber-free Precision, 11 Data owners, two-Backbone deployment, "
        "640/h assurance)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())