#!/usr/bin/env python3
"""Audit Circular Agrifood source against its compact public authority."""

from __future__ import annotations

import json
from pathlib import Path
import re
from typing import Any

from generate_recursive_industry_universal_source import load_catalog


ROOT = Path(__file__).resolve().parents[1]


def load_json(path: Path) -> dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"JSON root must be an object: {path}")
    return value


def normalized(path: Path) -> str:
    return "".join(path.read_text(encoding="utf-8").split())


def require_tokens(
    errors: list[str],
    label: str,
    text: str,
    tokens: tuple[str, ...],
) -> None:
    for token in tokens:
        if token not in text:
            errors.append(f"{label} missing exact declaration: {token}")


def canonical_amounts(rows: list[dict[str, Any]]) -> dict[str, int]:
    return {row["product"]: row["quantity"] for row in rows}


def audit(root: Path = ROOT) -> list[str]:
    errors: list[str] = []
    contract = load_json(root / "data" / "circular-agrifood.json")
    catalog = load_catalog()
    manifest = load_json(root / "mods" / "RecursiveIndustry" / "manifest.json")
    icons = load_json(
        root / "art" / "RecursiveIndustry" / "UiIcons" / "asset-manifest.json"
    )

    if contract.get("schema_version") != 1:
        errors.append("Circular Agrifood schema must be 1")
    if manifest.get("version") != contract.get("candidate_version"):
        errors.append("Circular Agrifood manifest version drift")

    expected_counts = contract.get("catalog_contract", {})
    actual_direct = sum(
        len(facility.get("direct_bindings", []))
        for facility in catalog.get("facilities", [])
    )
    actual_counts = {
        "facilities": len(catalog.get("facilities", [])),
        "direct_bindings": actual_direct,
        "integrated_recipes": len(catalog.get("integrated_recipes", [])),
        "precision_recipes": len(catalog.get("precision_recipes", [])),
        "authored_recipes": len(catalog.get("authored_recipes", [])),
    }
    for key, value in actual_counts.items():
        if expected_counts.get(key) != value:
            errors.append(f"Circular Agrifood catalog count drift: {key}")

    contract_recipes = contract.get("authored_recipes", {})
    catalog_recipes = {
        recipe["key"]: recipe for recipe in catalog.get("authored_recipes", [])
    }
    if set(contract_recipes) != set(catalog_recipes):
        errors.append("Circular Agrifood authored recipe inventory drift")
    else:
        for key, expected in contract_recipes.items():
            actual = catalog_recipes[key]
            if (
                actual.get("machine") != expected.get("machine")
                or actual.get("duration_seconds") != expected.get("duration_seconds")
                or actual.get("power_multiplier_percent")
                != expected.get("power_multiplier_percent")
                or canonical_amounts(actual.get("inputs", []))
                != expected.get("inputs")
                or canonical_amounts(actual.get("outputs", []))
                != expected.get("outputs")
            ):
                errors.append(f"{key} exact vector drift")

    care = contract.get("companion_care", {})
    if (
        care.get("unity_at_full_satisfaction") != 0.6
        or care.get("health") != 0
        or care.get("worker_productivity") != 0
        or care.get("provisions_per_pop_per_month") != 0.02
        or care.get("waste_per_pop_per_month") != 0.004
        or care.get("workers") != 8
        or care.get("population_supported_per_recipe_batch") != 4000
        or care.get("feed_embodied_per_1000_pop_month") != 15
    ):
        errors.append("Companion care service vector drift")

    guards = contract.get("guards", {})
    if (
        guards.get("gas_fed_feed_recipe_implemented") is not False
        or guards.get("maximum_returned_fuel_gas_from_60_feed") != 52.5
        or guards.get("companion_service_clears_integrated_plant_surplus_alone")
        is not False
        or any(
            guards.get(key) is not False
            for key in (
                "automatic_fallback",
                "custom_saved_state",
                "industrial_control_stream_input",
                "recurring_packages",
                "direct_health_bonus",
                "worker_productivity_bonus",
                "synthetic_chicken_carcass",
                "direct_synthetic_food_pack",
            )
        )
    ):
        errors.append("Circular Agrifood guard drift")

    service = normalized(
        root
        / "mods"
        / "RecursiveIndustry"
        / "src"
        / "CompanionAnimalCareData.cs"
    )
    require_tokens(
        errors,
        "Companion care source",
        service,
        (
            "newCountableProductProto(",
            "RecursiveIndustryIds.Products.CompanionProvisions",
            "newPopNeedProto(",
            "0.6.Upoints()",
            "healthGiven:null",
            ".Workers(8)",
            ".SetElectricityConsumed(250.Kw())",
            ".SetInput(provisions,0.02.ToFix64(),160)",
            ".SetOutput(waste,0.004.ToFix64(),64)",
            "RecursiveIndustry:COMPANION_ANIMAL_CARE_REGISTERED",
        ),
    )
    for forbidden in (
        "SetComputingConsumption",
        "IndustrialControlStream",
        "Harmony",
        "HealthData(",
    ):
        if forbidden in service:
            errors.append(f"Companion care source introduces forbidden surface: {forbidden}")

    research = normalized(
        root
        / "mods"
        / "RecursiveIndustry"
        / "src"
        / "UniversalIndustryResearchData.cs"
    )
    match = re.search(
        r"ResearchNodeProtocircularAgrifood=.*?circularAgrifood.AddParent\(adaptiveAgrifood\);",
        research,
    )
    if match is None:
        errors.append("Circular Agrifood research section is missing")
    else:
        section = match.group(0)
        require_tokens(
            errors,
            "Circular Agrifood research",
            section,
            (
                '"CircularAgrifoodSystems"',
                "RecursiveIndustryIds.Research.CircularAgrifoodSystems",
                "costMonths:480",
                "RecursiveIndustryIds.Settlements.CompanionAnimalCenter",
                "AddProtoToUnlock<PopNeedProto>(RecursiveIndustryIds.Settlements.CompanionCareNeed)",
                "RecursiveIndustryIds.Recipes.AdaptiveEggFermentation",
                "RecursiveIndustryIds.Recipes.SerumFreeCulturedMeat",
                "RecursiveIndustryIds.Recipes.MycoproteinTrimmings",
                "RecursiveIndustryIds.Recipes.CompanionProvisions",
                "circularAgrifood.GridPosition=newVector2i(228,18)",
                ".SetRequireSpacePoints()",
            ),
        )
        if "AddRequirementForLifetimeProduction" in section:
            errors.append("Circular Agrifood must inherit its parent lifetime gate")

    presentation = contract.get("presentation", {})
    icon_names = {row.get("name") for row in icons.get("icons", [])}
    if (
        presentation.get("total_ui_identities") != 91
        or presentation.get("new_ui_identities") != 2
        or presentation.get("dependencies") != []
        or not {
            "companion_provisions",
            "companion_animal_center",
        } <= icon_names
    ):
        errors.append("Circular Agrifood presentation contract drift")
    return errors


def main() -> int:
    errors = audit()
    if errors:
        print("Recursive Industry Circular Agrifood: FAIL")
        for error in errors:
            print(f"  ERROR: {error}")
        return 1
    print(
        "Recursive Industry Circular Agrifood: PASS "
        "(4 authored recipes, optional companion care, 91 UI identities)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())