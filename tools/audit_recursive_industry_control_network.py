#!/usr/bin/env python3
"""Audit the public Industrial Control network against its schema-1 authority."""

from __future__ import annotations

import json
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
    expected_custom = {
        (recipe["key"], recipe["owner_key"])
        for recipe in controlled
    }
    actual_custom = {
        (recipe.get("key"), recipe.get("machine"))
        for recipe in catalog_integrated
    }
    if actual_custom != expected_custom:
        errors.append("controlled recipe keys or owners drift from the control contract")
    if len(catalog_integrated) != 17:
        errors.append("universal catalog must contain exactly 17 Integrated recipes")
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
    actual_owners = {recipe.get("machine") for recipe in catalog_integrated}
    if actual_owners != expected_owners:
        errors.append("exactly the nine contracted facilities must own controlled modes")
    if len({facility.get("key") for facility in facilities} - actual_owners) != 10:
        errors.append("exactly ten facilities must remain without a Data port")
    right_side_owners = {
        owner["key"]
        for owner in control["owners"]
        if owner.get("right_side_input_ports") == 1
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
        if not all(
            isinstance(value, int)
            for value in (material, inputs, outputs, physical_rows, right_inputs)
        ):
            errors.append(f"{owner.get('key')} port contract must contain integers")
            continue
        if inputs != material + 1:
            errors.append(f"{owner['key']} must add exactly one Data input")
        if physical_rows > 7 or right_inputs != max(0, inputs - physical_rows):
            errors.append(f"{owner['key']} physical or right-side row contract drift")
        if outputs + right_inputs > physical_rows:
            errors.append(f"{owner['key']} right edge is oversubscribed")
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
            "rightSideInputs = Math.Max(0, flatInputs.Count - layoutRows)",
            "flatOutputs.Count + rightSideInputs > layoutRows",
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
    if "RecursiveIndustry.cs" in source_files:
        errors.extend(audit_registration_source(source_files["RecursiveIndustry.cs"]))
    errors.extend(audit_forbidden_runtime(source_files))

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
            "(235 Direct, 17 compositions, 10 Fiber-free Precision, "
        "9 Data owners, Backbone deployment, 640/h assurance)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())