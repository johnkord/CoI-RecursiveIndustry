#!/usr/bin/env python3
"""Audit Adaptive Agrifood source against its compact public authority."""

from __future__ import annotations

import json
from pathlib import Path
import re
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
CONTRACT_PATH = ROOT / "data" / "adaptive-agrifood.json"
SOURCE_ROOT = ROOT / "mods" / "RecursiveIndustry" / "src"


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


def audit(root: Path = ROOT) -> list[str]:
    errors: list[str] = []
    contract = load_json(root / "data" / "adaptive-agrifood.json")
    manifest = load_json(root / "mods" / "RecursiveIndustry" / "manifest.json")
    icons = load_json(
        root / "art" / "RecursiveIndustry" / "UiIcons" / "asset-manifest.json"
    )

    if contract.get("schema_version") != 1:
        errors.append("Adaptive Agrifood schema must be 1")
    if manifest.get("version") != contract.get("candidate_version"):
        errors.append("Adaptive Agrifood manifest version drift")

    irrigation = contract.get("precision_irrigation", {})
    if (
        irrigation.get("property")
        != "IdsCore.PropertyIds.FarmWaterConsumptionMultiplier"
        or irrigation.get("effect_percent_per_level") != -2
        or irrigation.get("levels") != 5
        or irrigation.get("cap_percent") != -10
        or irrigation.get("first_focus_cost") != 8000
        or irrigation.get("focus_cost_increment") != 4000
        or irrigation.get("total_focus_cost") != 80000
    ):
        errors.append("Precision Irrigation vector or Focus curve drift")
    if set(irrigation.get("excluded_properties", [])) != {
        "FarmYieldMultiplier",
        "SettlementWaterConsumptionMultiplier",
        "RainYieldMultiplier",
        "GroundWaterReplenishWhenLow",
    }:
        errors.append("Precision Irrigation exclusion boundary drift")

    farms = contract.get("farms", {})
    expected_farms = {
        "sensor_guided_greenhouse": (
            "FarmProto",
            "Ids.Buildings.FarmT4",
            20,
            {"Electronics4": 64, "ValidatedControlPackage": 16, "FrontierProgram": 4},
        ),
        "monitored_poultry_farm": (
            "AnimalFarmProto",
            "Ids.Buildings.ChickenFarm",
            12,
            {"Electronics4": 32, "ValidatedControlPackage": 8, "FrontierProgram": 2},
        ),
    }
    if set(farms) != set(expected_farms):
        errors.append("Adaptive Agrifood farm inventory drift")
    else:
        for key, (family, source, source_workers, construction) in expected_farms.items():
            row = farms[key]
            if (
                row.get("runtime_family") != family
                or row.get("source") != source
                or row.get("upgrade_from") != source
                or row.get("upgrade_api") != "UpgradeExtensions.SetNextTier"
                or row.get("source_workers") != source_workers
                or row.get("workers") != 4
                or row.get("additional_construction") != construction
                or row.get("incremental_upgrade_cost") != construction
                or not row.get("preserved_fields")
            ):
                errors.append(f"{key} clone or cost contract drift")

    data_path = root / "mods" / "RecursiveIndustry" / "src" / "AdaptiveAgrifoodData.cs"
    research_path = root / "mods" / "RecursiveIndustry" / "src" / "UniversalIndustryResearchData.cs"
    data = normalized(data_path)
    research = normalized(research_path)
    require_tokens(
        errors,
        "Adaptive Agrifood source",
        data,
        (
            "GetOrThrow<FarmProto>(Ids.Buildings.FarmT4)",
            "GetOrThrow<AnimalFarmProto>(Ids.Buildings.ChickenFarm)",
            "newFarmProto(",
            "newAnimalFarmProto(",
            "IdsCore.PropertyIds.FarmWaterConsumptionMultiplier",
            "maxStep:5",
            "effectPerStep=-2.Percent()",
            "baseCost:8000",
            "costIncrement:4000",
            "workers:4",
            "electronics,64,controlPackages,16,programs,4",
            "electronics,32,controlPackages,8,programs,2",
            "FarmProtosensorGuidedGreenhouse=registrator.PrototypesDb.Add(newFarmProto(",
            "AnimalFarmProtomonitoredPoultryFarm=registrator.PrototypesDb.Add(newAnimalFarmProto(",
            'LinkUpgrade(source,sensorGuidedGreenhouse,"GreenhouseII")',
            'LinkUpgrade(source,monitoredPoultryFarm,"ChickenFarm")',
            "whereTProto:IProtoWithUpgrade",
            "source.Upgrade.NextTier.HasValue",
            "source.SetNextTier(target)",
            '"RecursiveIndustry:ADAPTIVE_AGRIFOOD_UPGRADES_LINKED"',
            "source.YieldMultiplier",
            "source.DemandsMultiplier",
            "source.AnimalsBornPer100AnimalsPerMonth",
            "source.FoodPerAnimalPerMonth",
            "source.ProducedPerAnimalPerMonth",
            "RecursiveIndustryIcons.PrecisionIrrigation",
            "RecursiveIndustryIcons.SensorGuidedGreenhouse",
            "RecursiveIndustryIcons.MonitoredPoultryFarm",
        ),
    )
    for forbidden in (
        "FarmYieldMultiplier",
        "SettlementWaterConsumptionMultiplier",
        "RainYieldMultiplier",
        "GroundWaterReplenishWhenLow",
        "FarmAssignCropCmd",
        "FarmCommandsProcessor",
        "IndustrialControlStream",
        "SetComputingConsumption",
        "Harmony",
    ):
        if forbidden in data:
            errors.append(f"Adaptive Agrifood source introduces forbidden surface: {forbidden}")

    research_match = re.search(
        r"ResearchNodeProtoadaptiveAgrifood=.*?adaptiveAgrifood.AddParent\(essential\);",
        research,
    )
    if research_match is None:
        errors.append("Adaptive Agrifood research section is missing")
    else:
        section = research_match.group(0)
        require_tokens(
            errors,
            "Adaptive Agrifood research",
            section,
            (
                '"AdaptiveAgrifoodSystems"',
                "RecursiveIndustryIds.Research.AdaptiveAgrifoodSystems",
                "costMonths:480",
                "RecursiveIndustryIds.Farms.SensorGuidedGreenhouse",
                "RecursiveIndustryIds.Farms.MonitoredPoultryFarm",
                "RecursiveIndustryIds.Focuses.PrecisionIrrigation",
                "adaptiveAgrifood.GridPosition=newVector2i(224,18)",
                ".SetRequireSpacePoints()",
            ),
        )
        if "AddRequirementForLifetimeProduction" in section:
            errors.append("Adaptive Agrifood must not duplicate its parent lifetime gate")

    all_source = "\n".join(
        path.read_text(encoding="utf-8")
        for path in sorted((root / "mods" / "RecursiveIndustry" / "src").glob("*.cs"))
    )
    positions = [
        (int(x), int(y))
        for x, y in re.findall(
            r"GridPosition\s*=\s*new Vector2i\((\d+),\s*(\d+)\)",
            all_source,
        )
    ]
    target = (224, 18)
    if positions.count(target) != 1:
        errors.append("Adaptive Agrifood research coordinate must be unique")
    for position in positions:
        if position == target:
            continue
        distance_squared = (
            (position[0] - target[0]) ** 2
            + (position[1] - target[1]) ** 2
        )
        if distance_squared < 16:
            errors.append(
                f"Adaptive Agrifood research is too close to {position}"
            )

    boundaries = contract.get("boundaries", {})
    if any(value is not False and value != 0 for value in boundaries.values()):
        errors.append("Adaptive Agrifood excluded-runtime boundary drift")

    presentation = contract.get("presentation", {})
    icon_names = {row.get("name") for row in icons.get("icons", [])}
    required_icons = {
        "precision_irrigation",
        "sensor_guided_greenhouse",
        "monitored_poultry_farm",
    }
    if (
        presentation.get("bundle") != "uiicons_5287"
        or presentation.get("total_ui_identities") != 85
        or presentation.get("new_ui_identities") != 3
        or presentation.get("dependencies") != []
        or not required_icons <= icon_names
    ):
        errors.append("Adaptive Agrifood presentation contract drift")
    return errors


def main() -> int:
    errors = audit()
    if errors:
        print("Recursive Industry Adaptive Agrifood: FAIL")
        for error in errors:
            print(f"  ERROR: {error}")
        return 1
    print(
        "Recursive Industry Adaptive Agrifood: PASS "
        "(Precision Irrigation, 2 native farm upgrades, 85 UI identities)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())