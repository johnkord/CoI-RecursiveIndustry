#!/usr/bin/env python3
"""Simulate Recursive Industry endgame support economics and balance candidates."""

from __future__ import annotations

from dataclasses import asdict, dataclass
from fractions import Fraction
import argparse
import json
import math
from pathlib import Path
from typing import Iterable

ROOT = Path(__file__).resolve().parents[1]
CATALOG_PATH = ROOT / "data" / "universal-industry-catalog.json"
CATALOG = json.loads(CATALOG_PATH.read_text(encoding="utf-8"))

RACK_III_COMPUTING = 256
RACK_III_POWER_MW = Fraction(3, 2)
RACK_III_MAINTENANCE_T3 = 12
RACK_III_COOLANT = 10

VALIDATOR_PACKAGES_PER_HOUR = 160
VALIDATOR_MODELS_PER_HOUR = 20
CURATION_MODEL_CENTER_MODELS_PER_HOUR = 30
CURATION_MODEL_CENTER_DATASETS_PER_HOUR = 480
CURATION_OFFICE_DATASETS_PER_HOUR = 480

PILOT_DOSSIERS_PER_HOUR = 10
PILOT_PROGRAMS_PER_HOUR = 10
SCIENCE_INSTITUTE_PROGRAMS_PER_HOUR = 60
SCIENCE_INSTITUTE_MODELS_PER_HOUR = 15
SCIENCE_INSTITUTE_DATASETS_PER_HOUR = 120


def ceil_fraction(value: Fraction) -> int:
    return -(-value.numerator // value.denominator)


def scaled_int(value: int, scale: Fraction) -> int:
    return ceil_fraction(Fraction(value) * scale)


@dataclass(frozen=True)
class Asset:
    name: str
    power_mw: Fraction = Fraction(0)
    computing: int = 0
    workers: int = 0
    maintenance_t3: Fraction = Fraction(0)
    construction_packages: int = 0
    packages_per_hour: Fraction = Fraction(0)
    dossiers_per_hour: Fraction = Fraction(0)


@dataclass(frozen=True)
class PackageBank:
    validators: int
    model_centers: int
    curation_offices: int
    capacity_per_hour: int
    power_mw: Fraction
    computing: int
    workers: int
    maintenance_t3: int


@dataclass(frozen=True)
class DossierBank:
    pilots: int
    science_institutes: int
    model_centers: int
    curation_offices: int
    capacity_per_hour: int
    power_mw: Fraction
    computing: int
    workers: int
    maintenance_t3: int


@dataclass(frozen=True)
class Candidate:
    name: str
    universal_computing_scale: Fraction
    universal_package_scale: Fraction
    late_core_package_scale: Fraction
    pcc_computing: int
    pcc_packages_per_cycle: int
    late_core_computing_scale: Fraction
    systems_packages_per_cycle: int
    integration_array_packages_per_cycle: int
    nexus_tradeoff_packages_per_cycle: int
    array_dossiers_per_hour: int


@dataclass(frozen=True)
class ScenarioResult:
    candidate: str
    scenario: str
    power_basis: str
    asset_count: int
    construction_packages: int
    commissioning_hours: Fraction
    packages_per_hour: Fraction
    package_validators: int
    package_model_centers: int
    package_curation_offices: int
    computing: int
    rack_iii: int
    rack_power_mw: Fraction
    rack_maintenance_t3: int
    rack_coolant: int
    dossier_pilots: int
    dossier_science_institutes: int
    orbital_arrays: int
    gross_power_mw: Fraction
    power_supply_mw: int
    power_headroom_mw: Fraction
    workers: int
    gross_maintenance_t3: Fraction
    maintenance_at_focus_cap_t3: Fraction


CURRENT = Candidate(
    name="throughput_first_baseline",
    universal_computing_scale=Fraction(1),
    universal_package_scale=Fraction(1),
    late_core_package_scale=Fraction(1),
    pcc_computing=2048,
    pcc_packages_per_cycle=192,
    late_core_computing_scale=Fraction(1),
    systems_packages_per_cycle=32,
    integration_array_packages_per_cycle=72,
    nexus_tradeoff_packages_per_cycle=4,
    array_dossiers_per_hour=20,
)

MODERATE = Candidate(
    name="moderate_rebalance",
    universal_computing_scale=Fraction(1, 4),
    universal_package_scale=Fraction(3, 4),
    late_core_package_scale=Fraction(3, 4),
    pcc_computing=1536,
    pcc_packages_per_cycle=128,
    late_core_computing_scale=Fraction(3, 4),
    systems_packages_per_cycle=24,
    integration_array_packages_per_cycle=48,
    nexus_tradeoff_packages_per_cycle=2,
    array_dossiers_per_hour=10,
)

SELECTED = Candidate(
    name="selected_rebalance",
    universal_computing_scale=Fraction(1, 8),
    universal_package_scale=Fraction(1, 2),
    late_core_package_scale=Fraction(1, 2),
    pcc_computing=1024,
    pcc_packages_per_cycle=64,
    late_core_computing_scale=Fraction(1, 2),
    systems_packages_per_cycle=16,
    integration_array_packages_per_cycle=32,
    nexus_tradeoff_packages_per_cycle=1,
    array_dossiers_per_hour=10,
)

OVERPOWERED = Candidate(
    name="overpowered_sensitivity",
    universal_computing_scale=Fraction(1, 16),
    universal_package_scale=Fraction(1, 4),
    late_core_package_scale=Fraction(1, 4),
    pcc_computing=512,
    pcc_packages_per_cycle=32,
    late_core_computing_scale=Fraction(1, 4),
    systems_packages_per_cycle=8,
    integration_array_packages_per_cycle=16,
    nexus_tradeoff_packages_per_cycle=1,
    array_dossiers_per_hour=5,
)


def package_bank(demand_per_hour: Fraction) -> PackageBank:
    if demand_per_hour <= 0:
        return PackageBank(0, 0, 0, 0, Fraction(0), 0, 0, 0)
    validators = ceil_fraction(
        demand_per_hour / Fraction(VALIDATOR_PACKAGES_PER_HOUR)
    )
    model_centers = ceil_fraction(
        Fraction(validators * VALIDATOR_MODELS_PER_HOUR)
        / CURATION_MODEL_CENTER_MODELS_PER_HOUR
    )
    curation_offices = model_centers
    total_model_centers = validators + model_centers
    return PackageBank(
        validators=validators,
        model_centers=model_centers,
        curation_offices=curation_offices,
        capacity_per_hour=validators * VALIDATOR_PACKAGES_PER_HOUR,
        power_mw=Fraction(total_model_centers * 8 + curation_offices * 2, 10),
        computing=total_model_centers * 24,
        workers=total_model_centers * 24 + curation_offices * 80,
        maintenance_t3=total_model_centers * 5 + curation_offices * 2,
    )


def dossier_bank(demand_per_hour: Fraction) -> DossierBank:
    if demand_per_hour <= 0:
        return DossierBank(0, 0, 0, 0, 0, Fraction(0), 0, 0, 0)
    pilots = ceil_fraction(demand_per_hour / Fraction(PILOT_DOSSIERS_PER_HOUR))
    institutes = ceil_fraction(
        Fraction(pilots * PILOT_PROGRAMS_PER_HOUR)
        / SCIENCE_INSTITUTE_PROGRAMS_PER_HOUR
    )
    model_centers = ceil_fraction(
        Fraction(institutes * SCIENCE_INSTITUTE_MODELS_PER_HOUR)
        / CURATION_MODEL_CENTER_MODELS_PER_HOUR
    )
    model_dataset_offices = model_centers
    institute_dataset_offices = ceil_fraction(
        Fraction(institutes * SCIENCE_INSTITUTE_DATASETS_PER_HOUR)
        / CURATION_OFFICE_DATASETS_PER_HOUR
    )
    offices = model_dataset_offices + institute_dataset_offices
    return DossierBank(
        pilots=pilots,
        science_institutes=institutes,
        model_centers=model_centers,
        curation_offices=offices,
        capacity_per_hour=pilots * PILOT_DOSSIERS_PER_HOUR,
        power_mw=Fraction(pilots * 16 + institutes * 12 + model_centers * 8 + offices * 2, 10),
        computing=pilots * 8 + institutes * 64 + model_centers * 24,
        workers=pilots * 80 + institutes * 32 + model_centers * 24 + offices * 80,
        maintenance_t3=pilots * 8 + institutes * 6 + model_centers * 5 + offices * 2,
    )


def universal_assets(candidate: Candidate, mode: str = "direct") -> list[Asset]:
    precision_owners = {
        recipe["machine"] for recipe in CATALOG["precision_recipes"]
    }
    integrated_owners = {
        recipe["machine"] for recipe in CATALOG["integrated_recipes"]
    }
    result = []
    for facility in CATALOG["facilities"]:
        key = facility["key"]
        power_factor = 1
        if mode == "optimized" and key in precision_owners | integrated_owners:
            power_factor = 2
        result.append(Asset(
            name=facility["name"],
            power_mw=Fraction(facility["power_mw"] * power_factor),
            computing=scaled_int(
                facility["baseline_computing"],
                candidate.universal_computing_scale,
            ),
            workers=facility["workers"],
            maintenance_t3=Fraction(facility["maintenance_t3"]),
            construction_packages=scaled_int(
                facility["baseline_packages"],
                candidate.universal_package_scale,
            ),
        ))
    return result


def assets_for_portfolios(
    candidate: Candidate,
    portfolios: Iterable[str],
    mode: str = "direct",
) -> list[Asset]:
    wanted = set(portfolios)
    return [
        asset for asset, facility in zip(
            universal_assets(candidate, mode),
            CATALOG["facilities"],
        )
        if facility["portfolio"] in wanted
    ]


def core_assets(candidate: Candidate, include_pcc: bool) -> list[Asset]:
    scale = candidate.late_core_computing_scale
    package_scale = candidate.late_core_package_scale
    assets = [
        Asset("AI Operations III", Fraction(3, 5), 192, 100, 1, packages_per_hour=160),
        Asset(
            "Systems Integration Complex",
            Fraction(12, 5),
            128,
            120,
            12,
            packages_per_hour=Fraction(candidate.systems_packages_per_cycle * 3600, 720),
        ),
        Asset(
            "Autonomous Microchip Complex",
            8,
            scaled_int(512, scale),
            0,
            24,
            construction_packages=scaled_int(64, package_scale),
            packages_per_hour=60,
        ),
        Asset(
            "Autonomous Electronics Integration Complex",
            6,
            scaled_int(512, scale),
            0,
            20,
            construction_packages=scaled_int(64, package_scale),
            packages_per_hour=45,
        ),
        Asset(
            "Autonomous Capital Fabrication Matrix",
            8,
            scaled_int(1024, scale),
            0,
            28,
            construction_packages=scaled_int(96, package_scale),
            packages_per_hour=45,
        ),
        Asset(
            "Orbital Payload & Mission Complex",
            6,
            scaled_int(512, scale),
            40,
            20,
            construction_packages=scaled_int(64, package_scale),
            packages_per_hour=8,
        ),
        Asset(
            "Frontier Project Complex",
            12,
            scaled_int(1024, scale),
            80,
            32,
            construction_packages=scaled_int(128, package_scale),
        ),
        Asset(
            "Recursive Integration Array",
            6,
            scaled_int(1024, scale),
            60,
            24,
            construction_packages=scaled_int(128, package_scale),
            packages_per_hour=Fraction(
                candidate.integration_array_packages_per_cycle * 3600,
                720,
            ),
        ),
        Asset(
            "Autonomous Construction Nexus",
            12,
            scaled_int(2048, scale),
            0,
            32,
            construction_packages=scaled_int(128, package_scale),
            packages_per_hour=Fraction(
                candidate.nexus_tradeoff_packages_per_cycle * 3600,
                64,
            ),
        ),
    ]
    if include_pcc:
        assets.append(Asset(
            "Planetary Coordination Center",
            8,
            candidate.pcc_computing,
            250,
            24,
            construction_packages=scaled_int(256, package_scale),
            packages_per_hour=Fraction(candidate.pcc_packages_per_cycle * 3600, 360),
        ))
    return assets


def evaluate(
    candidate: Candidate,
    scenario: str,
    assets: list[Asset],
    predictive_maintenance: bool = True,
    orbital_power_closure: bool = True,
) -> ScenarioResult:
    package_demand = sum((asset.packages_per_hour for asset in assets), Fraction(0))
    packages = package_bank(package_demand)
    construction_packages = sum(asset.construction_packages for asset in assets)
    commissioning_capacity = max(
        VALIDATOR_PACKAGES_PER_HOUR,
        packages.capacity_per_hour,
    )
    commissioning_hours = Fraction(
        construction_packages,
        commissioning_capacity,
    )

    base_power = sum((asset.power_mw for asset in assets), Fraction(0)) + packages.power_mw
    base_computing = sum(asset.computing for asset in assets) + packages.computing
    base_workers = sum(asset.workers for asset in assets) + packages.workers
    base_maintenance = (
        sum((asset.maintenance_t3 for asset in assets), Fraction(0))
        + packages.maintenance_t3
    )

    arrays = max(1, ceil_fraction(base_power / 240)) if orbital_power_closure else 0
    dossiers = DossierBank(0, 0, 0, 0, 0, Fraction(0), 0, 0, 0)
    if orbital_power_closure:
        for _ in range(10):
            dossiers = dossier_bank(arrays * candidate.array_dossiers_per_hour)
            computing = base_computing + dossiers.computing
            racks = ceil_fraction(Fraction(computing, RACK_III_COMPUTING)) if computing else 0
            rack_power = racks * RACK_III_POWER_MW
            gross_power = base_power + dossiers.power_mw + rack_power
            next_arrays = max(1, ceil_fraction(gross_power / 240))
            if next_arrays == arrays:
                break
            arrays = next_arrays
        else:
            raise RuntimeError("orbital power support did not converge")

    computing = base_computing + dossiers.computing
    racks = ceil_fraction(Fraction(computing, RACK_III_COMPUTING)) if computing else 0
    rack_power = racks * RACK_III_POWER_MW
    rack_maintenance = racks * RACK_III_MAINTENANCE_T3
    gross_power = base_power + dossiers.power_mw + rack_power
    gross_maintenance = base_maintenance + dossiers.maintenance_t3 + rack_maintenance + arrays * 80
    maintenance_at_cap = gross_maintenance / 2 if predictive_maintenance else gross_maintenance
    workers = base_workers + dossiers.workers + arrays * 80

    return ScenarioResult(
        candidate=candidate.name,
        scenario=scenario,
        power_basis=(
            "orbital_self_sufficient"
            if orbital_power_closure
            else "terrestrial_grid"
        ),
        asset_count=len(assets),
        construction_packages=construction_packages,
        commissioning_hours=commissioning_hours,
        packages_per_hour=package_demand,
        package_validators=packages.validators,
        package_model_centers=packages.model_centers,
        package_curation_offices=packages.curation_offices,
        computing=computing,
        rack_iii=racks,
        rack_power_mw=rack_power,
        rack_maintenance_t3=rack_maintenance,
        rack_coolant=racks * RACK_III_COOLANT,
        dossier_pilots=dossiers.pilots,
        dossier_science_institutes=dossiers.science_institutes,
        orbital_arrays=arrays,
        gross_power_mw=gross_power,
        power_supply_mw=arrays * 240,
        power_headroom_mw=arrays * 240 - gross_power,
        workers=workers,
        gross_maintenance_t3=gross_maintenance,
        maintenance_at_focus_cap_t3=maintenance_at_cap,
    )


def scenarios(
    candidate: Candidate,
    orbital_power_closure: bool = True,
) -> list[ScenarioResult]:
    branches = {
        "materials_branch": ("bulk_materials", "metallurgy"),
        "process_branch": ("refinery", "chemistry"),
        "essential_branch": ("food_bioprocessing", "circular_utilities"),
        "nuclear_branch": ("nuclear_fuel_cycle",),
        "advanced_branch": ("advanced_fabrication",),
    }
    results = [
        evaluate(
            candidate,
            name,
            assets_for_portfolios(candidate, portfolios),
            orbital_power_closure=orbital_power_closure,
        )
        for name, portfolios in branches.items()
    ]
    all_direct = universal_assets(candidate, "direct")
    all_optimized = universal_assets(candidate, "optimized")
    core = core_assets(candidate, include_pcc=False)
    center_core = core_assets(candidate, include_pcc=True)
    results.extend((
        evaluate(candidate, "all_universal_direct", all_direct, orbital_power_closure=orbital_power_closure),
        evaluate(candidate, "all_universal_optimized", all_optimized, orbital_power_closure=orbital_power_closure),
        evaluate(candidate, "mature_core", core, orbital_power_closure=orbital_power_closure),
        evaluate(candidate, "mature_core_plus_pcc", center_core, orbital_power_closure=orbital_power_closure),
        evaluate(candidate, "release_stress_direct", center_core + all_direct, orbital_power_closure=orbital_power_closure),
        evaluate(candidate, "release_stress_optimized", center_core + all_optimized, orbital_power_closure=orbital_power_closure),
    ))
    return results


def serialize(value: object) -> object:
    if isinstance(value, Fraction):
        return str(value) if value.denominator != 1 else value.numerator
    if isinstance(value, dict):
        return {key: serialize(item) for key, item in value.items()}
    if isinstance(value, list):
        return [serialize(item) for item in value]
    return value


def report() -> dict[str, object]:
    candidates = (CURRENT, MODERATE, SELECTED, OVERPOWERED)
    return {
        "constants": {
            "rack_iii_computing": RACK_III_COMPUTING,
            "rack_iii_power_mw": RACK_III_POWER_MW,
            "rack_iii_maintenance_t3": RACK_III_MAINTENANCE_T3,
            "validator_packages_per_hour": VALIDATOR_PACKAGES_PER_HOUR,
            "orbital_array_output_mw": 240,
            "predictive_maintenance_cap": "-50%",
        },
        "package_policy": {
            "bootstrap_or_evidence_producers": {
                "construction_packages": False,
                "reason": "avoid circular dependency before first validation and keep physical evidence human-supervised",
                "examples": [
                    "Accelerator Works",
                    "Curation Office",
                    "Model Development Center",
                    "AI Science Institute",
                    "Pilot Science Complex",
                ],
            },
            "continuous_deployment": {
                "recurring_packages": True,
                "reason": "offices and mod-owned control recipes continuously deploy changing decisions",
            },
            "autonomous_capital": {
                "construction_packages": True,
                "reason": "commission installed deterministic control; Direct vanilla bindings do not burn Packages per cycle",
            },
        },
        "candidates": [serialize(asdict(candidate)) for candidate in candidates],
        "results": [
            serialize(asdict(result))
            for candidate in candidates
            for orbital_power_closure in (True, False)
            for result in scenarios(candidate, orbital_power_closure)
        ],
    }


def print_table(results: list[ScenarioResult]) -> None:
    print(
        "candidate\tscenario\tbasis\tassets\tcommission_h\tpkg_h\tvalidators\t"
        "computing\track3\tcoolant\tpower_mw\tarrays\tworkers\tmaint_cap"
    )
    for result in results:
        print(
            f"{result.candidate}\t{result.scenario}\t{result.power_basis}\t{result.asset_count}\t"
            f"{float(result.commissioning_hours):.2f}\t"
            f"{float(result.packages_per_hour):.1f}\t{result.package_validators}\t"
            f"{result.computing}\t{result.rack_iii}\t{result.rack_coolant}\t"
            f"{float(result.gross_power_mw):.1f}\t{result.orbital_arrays}\t"
            f"{result.workers}\t{float(result.maintenance_at_focus_cap_t3):.1f}"
        )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--json", action="store_true")
    parser.add_argument(
        "--power-basis",
        choices=("orbital", "terrestrial"),
        default="orbital",
    )
    args = parser.parse_args()
    if args.json:
        print(json.dumps(serialize(report()), indent=2, sort_keys=True))
    else:
        orbital_power_closure = args.power_basis == "orbital"
        print_table(
            scenarios(CURRENT, orbital_power_closure)
            + scenarios(MODERATE, orbital_power_closure)
            + scenarios(SELECTED, orbital_power_closure)
            + scenarios(OVERPOWERED, orbital_power_closure)
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())