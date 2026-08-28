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

from generate_recursive_industry_universal_source import load_catalog

ROOT = Path(__file__).resolve().parents[1]
CATALOG_PATH = ROOT / "data" / "universal-industry-catalog.json"
CATALOG = load_catalog()
CONTROL_PATH = ROOT / "data" / "industrial-control-network.json"
CONTROL = json.loads(CONTROL_PATH.read_text(encoding="utf-8"))

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


@dataclass(frozen=True)
class ControlScenarioResult:
    scenario: str
    transport: str
    optimized_owner_count: int
    gateway_count: int
    local_gateway_count: int
    backbone_gateway_count: int
    stream_demand_per_minute: int
    stream_supply_per_minute: int
    gateway_headroom_per_minute: int
    transport_capacity_per_minute: int
    transport_headroom_per_minute: int
    package_demand_basis: str
    steady_state_packages_per_hour: Fraction
    unconstrained_packages_per_hour: int
    support: ScenarioResult


@dataclass(frozen=True)
class PackageScaleResult:
    scenario: str
    demand_per_hour: Fraction
    model_archives_per_hour: Fraction
    standard_validators: int
    standard_capacity_per_hour: int
    standard_workers: int
    standard_power_mw: Fraction
    standard_computing: int
    standard_maintenance_t3: int
    standard_construction_parts_iv: int
    assurance_campuses: int
    trim_validators: int
    dense_capacity_per_hour: int
    dense_workers: int
    dense_power_mw: Fraction
    dense_computing: int
    dense_maintenance_t3: int
    dense_construction_parts_iv: int


@dataclass(frozen=True)
class ControlSensitivityResult:
    stream_per_package: int
    gateway_computing: int
    gateway_power_mw: int
    gateway_count: int
    steady_state_packages_per_hour: Fraction
    unconstrained_packages_per_hour: int
    support: ScenarioResult


@dataclass(frozen=True)
class ElectronicsIIIResult:
    representative_demand_per_hour: int
    direct_fab_output_per_hour: int
    required_direct_fabs: int
    direct_fab_capacity_per_hour: int
    direct_fab_headroom_per_hour: int
    required_assembly_v_lines: int
    required_throughput_cells: int


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


def universal_assets(
    candidate: Candidate,
    mode: str = "direct",
    optimized_keys: set[str] | None = None,
) -> list[Asset]:
    optimized_power_factors: dict[str, Fraction] = {}
    for recipe in CATALOG["precision_recipes"]:
        optimized_power_factors[recipe["machine"]] = max(
            optimized_power_factors.get(recipe["machine"], Fraction(1)),
            Fraction(2),
        )
    for recipe in CATALOG["integrated_recipes"]:
        factor = Fraction(recipe.get("power_multiplier_percent", 200), 100)
        optimized_power_factors[recipe["machine"]] = max(
            optimized_power_factors.get(recipe["machine"], Fraction(1)),
            factor,
        )
    for recipe in CATALOG["authored_recipes"]:
        factor = Fraction(recipe["power_multiplier_percent"], 100)
        optimized_power_factors[recipe["machine"]] = max(
            optimized_power_factors.get(recipe["machine"], Fraction(1)),
            factor,
        )
    precision_owners = {
        recipe["machine"] for recipe in CATALOG["precision_recipes"]
    }
    integrated_owners = {
        recipe["machine"] for recipe in CATALOG["integrated_recipes"]
    }
    authored_owners = {
        recipe["machine"] for recipe in CATALOG["authored_recipes"]
    }
    if optimized_keys is None:
        optimized_keys = (
            precision_owners | integrated_owners | authored_owners
            if mode == "optimized"
            else set()
        )
    result = []
    for facility in CATALOG["facilities"]:
        key = facility["key"]
        power_factor = (
            optimized_power_factors[key]
            if key in optimized_keys
            else Fraction(1)
        )
        result.append(Asset(
            name=facility["name"],
            power_mw=Fraction(facility["power_kw"], 1000) * power_factor,
            computing=scaled_int(
                facility["baseline_computing"],
                candidate.universal_computing_scale,
            ),
            workers=facility["workers"],
            maintenance_t3=(
                Fraction(facility["maintenance_per_month"])
                if facility["maintenance_tier"] == "III"
                else Fraction(facility["maintenance_per_month"], 2)
            ),
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
            96,
            construction_packages=scaled_int(64, package_scale),
        ),
        Asset(
            "Autonomous Electronics Integration Complex",
            2,
            scaled_int(512, scale),
            0,
            16,
            construction_packages=scaled_int(64, package_scale),
        ),
        Asset(
            "Autonomous Capital Fabrication Matrix",
            2,
            scaled_int(1024, scale),
            0,
            16,
            construction_packages=scaled_int(96, package_scale),
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
            2,
            scaled_int(2048, scale),
            0,
            16,
            construction_packages=scaled_int(128, package_scale),
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


def legacy_control_assets(
    candidate: Candidate,
    optimized_keys: set[str],
) -> list[Asset]:
    scale = candidate.late_core_computing_scale
    package_scale = candidate.late_core_package_scale
    electronics_key = "autonomous_electronics_integration_complex"
    capital_key = "autonomous_capital_fabrication_matrix"
    return [
        Asset(
            "Autonomous Electronics Integration Complex",
            4 if electronics_key in optimized_keys else 2,
            scaled_int(512, scale),
            0,
            16,
            construction_packages=scaled_int(64, package_scale),
        ),
        Asset(
            "Autonomous Capital Fabrication Matrix",
            4 if capital_key in optimized_keys else 2,
            scaled_int(1024, scale),
            0,
            16,
            construction_packages=scaled_int(96, package_scale),
        ),
    ]


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


def control_gateway_asset(
    gateway_count: int,
    packages_per_hour: Fraction,
    computing_per_gateway: int,
    power_per_gateway_mw: int,
) -> Asset:
    gateway = CONTROL["gateway"]
    construction = {
        item["product_key"]: item["quantity"]
        for item in gateway["construction"]
    }
    return Asset(
        name="Control Deployment Gateway network",
        power_mw=Fraction(gateway_count * power_per_gateway_mw),
        computing=gateway_count * computing_per_gateway,
        workers=gateway_count * gateway["workers"],
        maintenance_t3=Fraction(
            gateway_count * gateway["maintenance"]["quantity_per_month"]
        ),
        construction_packages=(
            gateway_count * construction["ValidatedControlPackage"]
        ),
        packages_per_hour=packages_per_hour,
    )


def mixed_control_gateway_asset(
    local_gateway_count: int,
    backbone_gateway_count: int,
    packages_per_hour: Fraction,
) -> Asset:
    gateway = CONTROL["gateway"]
    construction = {
        item["product_key"]: item["quantity"]
        for item in gateway["construction"]
    }
    gateway_count = local_gateway_count + backbone_gateway_count
    power = (
        local_gateway_count * gateway["power_mw"]
        + backbone_gateway_count
        * gateway["backbone_recipe"]["effective_power_mw"]
    )
    return Asset(
        name="Federated Control Deployment Gateway network",
        power_mw=Fraction(power),
        computing=gateway_count * gateway["computing"],
        workers=gateway_count * gateway["workers"],
        maintenance_t3=Fraction(
            gateway_count * gateway["maintenance"]["quantity_per_month"]
        ),
        construction_packages=(
            gateway_count * construction["ValidatedControlPackage"]
        ),
        packages_per_hour=packages_per_hour,
    )


def control_scenarios(
    candidate: Candidate = SELECTED,
    orbital_power_closure: bool = False,
) -> list[ControlScenarioResult]:
    owner_keys = tuple(owner["key"] for owner in CONTROL["owners"])
    rate = CONTROL["consumer_contract"]["stream_units_per_active_minute"]
    gateway = CONTROL["gateway"]
    gateway_rate = gateway["recipe"]["output"]["quantity"]
    gateway_seconds = gateway["recipe"]["duration_seconds"]
    gateway_per_minute = gateway_rate * 60 // gateway_seconds
    backbone_rate = gateway["backbone_recipe"]["output"]["quantity"]
    topologies = (
        ("no_control_direct", "none", owner_keys[:0], 0, False),
        ("three_facility_access", "access_fiber", owner_keys[:3], 200, False),
        ("seven_facility_backbone", "backbone_fiber", owner_keys[:7], 450, False),
        ("seven_facility_federated", "backbone_fiber", owner_keys[:7], 450, True),
        ("all_eleven_optimized", "two_backbone_fibers", owner_keys, 900, False),
        ("all_eleven_federated", "two_backbone_fibers", owner_keys, 900, True),
    )
    results = []
    for name, transport, optimized, transport_capacity, federated in topologies:
        stream_demand = len(optimized) * rate
        backbone_gateway_count = (
            ceil_fraction(Fraction(stream_demand, backbone_rate))
            if federated and stream_demand
            else 0
        )
        remaining_demand = stream_demand - backbone_gateway_count * backbone_rate
        local_gateway_count = (
            ceil_fraction(Fraction(remaining_demand, gateway_per_minute))
            if remaining_demand
            else 0
        )
        gateway_count = local_gateway_count + backbone_gateway_count
        steady_packages = (
            Fraction(stream_demand * 60, gateway_rate)
            if stream_demand
            else Fraction(0)
        )
        unconstrained_packages = (
            local_gateway_count + backbone_gateway_count * 2
        ) * 60
        assets = universal_assets(
            candidate,
            "direct",
            optimized_keys=set(optimized),
        )
        assets.extend(legacy_control_assets(candidate, set(optimized)))
        if gateway_count:
            assets.append(mixed_control_gateway_asset(
                local_gateway_count,
                backbone_gateway_count,
                steady_packages,
            ))
        support = evaluate(
            candidate,
            name,
            assets,
            orbital_power_closure=orbital_power_closure,
        )
        supply = (
            local_gateway_count * gateway_per_minute
            + backbone_gateway_count * backbone_rate
        )
        results.append(ControlScenarioResult(
            scenario=name,
            transport=transport,
            optimized_owner_count=len(optimized),
            gateway_count=gateway_count,
            local_gateway_count=local_gateway_count,
            backbone_gateway_count=backbone_gateway_count,
            stream_demand_per_minute=stream_demand,
            stream_supply_per_minute=supply,
            gateway_headroom_per_minute=supply - stream_demand,
            transport_capacity_per_minute=transport_capacity,
            transport_headroom_per_minute=transport_capacity - stream_demand,
            package_demand_basis="steady_state_backpressure",
            steady_state_packages_per_hour=steady_packages,
            unconstrained_packages_per_hour=unconstrained_packages,
            support=support,
        ))
    return results


def package_scale_scenarios() -> list[PackageScaleResult]:
    selected = {
        result.scenario: result
        for result in scenarios(SELECTED, orbital_power_closure=False)
    }
    stream_packages = Fraction(
        CONTROL["capacity_closure"]["steady_state_packages_per_hour"]
    )
    assurance = CONTROL["deployment_assurance"]
    assurance_cp4 = next(
        item["quantity"]
        for item in assurance["construction"]
        if item["product_key"] == "ConstructionParts4"
    )
    cases = (
        (
            "mature_core_with_control",
            selected["mature_core"].packages_per_hour + stream_packages,
        ),
        (
            "mature_core_center_control",
            selected["mature_core_plus_pcc"].packages_per_hour
            + stream_packages,
        ),
    )
    results = []
    for name, demand in cases:
        standard_validators = ceil_fraction(
            demand / VALIDATOR_PACKAGES_PER_HOUR
        )
        assurance_campuses, trim_validators = divmod(standard_validators, 4)
        results.append(PackageScaleResult(
            scenario=name,
            demand_per_hour=demand,
            model_archives_per_hour=demand / 8,
            standard_validators=standard_validators,
            standard_capacity_per_hour=(
                standard_validators * VALIDATOR_PACKAGES_PER_HOUR
            ),
            standard_workers=standard_validators * 24,
            standard_power_mw=Fraction(standard_validators * 4, 5),
            standard_computing=standard_validators * 24,
            standard_maintenance_t3=standard_validators * 5,
            standard_construction_parts_iv=standard_validators * 160,
            assurance_campuses=assurance_campuses,
            trim_validators=trim_validators,
            dense_capacity_per_hour=(
                assurance_campuses * assurance["recipe"]["packages_per_hour"]
                + trim_validators * VALIDATOR_PACKAGES_PER_HOUR
            ),
            dense_workers=(
                assurance_campuses * assurance["workers"]
                + trim_validators * 24
            ),
            dense_power_mw=(
                assurance_campuses * Fraction(assurance["power_mw"])
                + trim_validators * Fraction(4, 5)
            ),
            dense_computing=(
                assurance_campuses * assurance["computing"]
                + trim_validators * 24
            ),
            dense_maintenance_t3=(
                assurance_campuses
                * assurance["maintenance"]["quantity_per_month"]
                + trim_validators * 5
            ),
            dense_construction_parts_iv=(
                assurance_campuses * assurance_cp4
                + trim_validators * 160
            ),
        ))
    return results


def control_sensitivity_tournament(
    candidate: Candidate = SELECTED,
    orbital_power_closure: bool = False,
) -> list[ControlSensitivityResult]:
    closure = CONTROL["capacity_closure"]
    sensitivity = closure["sensitivity"]
    stream_demand = closure["all_owner_demand_per_minute"]
    optimized_keys = {owner["key"] for owner in CONTROL["owners"]}
    results = []
    for stream_per_package in sensitivity["stream_per_package"]:
        for computing in sensitivity["computing"]:
            for power_mw in sensitivity["power_mw"]:
                gateway_count = ceil_fraction(
                    Fraction(stream_demand, stream_per_package)
                )
                steady_packages = Fraction(
                    stream_demand * 60,
                    stream_per_package,
                )
                unconstrained_packages = gateway_count * 60
                assets = universal_assets(
                    candidate,
                    "direct",
                    optimized_keys=optimized_keys,
                )
                assets.append(control_gateway_asset(
                    gateway_count,
                    steady_packages,
                    computing,
                    power_mw,
                ))
                name = (
                    "control_sensitivity_"
                    + f"stream_{stream_per_package}_computing_{computing}_power_{power_mw}"
                )
                results.append(ControlSensitivityResult(
                    stream_per_package=stream_per_package,
                    gateway_computing=computing,
                    gateway_power_mw=power_mw,
                    gateway_count=gateway_count,
                    steady_state_packages_per_hour=steady_packages,
                    unconstrained_packages_per_hour=unconstrained_packages,
                    support=evaluate(
                        candidate,
                        name,
                        assets,
                        orbital_power_closure=orbital_power_closure,
                    ),
                ))
    return results


def electronics_iii_balance() -> ElectronicsIIIResult:
    binding = CONTROL["direct_contract"]["electronics_iii_binding"]
    demand = sum(
        item["quantity"]
        for item in binding["representative_demand_per_hour"]
    )
    output = binding["output_per_hour"]
    direct_fabs = math.ceil(demand / output)
    capacity = direct_fabs * output
    return ElectronicsIIIResult(
        representative_demand_per_hour=demand,
        direct_fab_output_per_hour=output,
        required_direct_fabs=direct_fabs,
        direct_fab_capacity_per_hour=capacity,
        direct_fab_headroom_per_hour=capacity - demand,
        required_assembly_v_lines=math.ceil(demand / 360),
        required_throughput_cells=math.ceil(demand / 720),
    )


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
                "reason": "offices, signed deployment, and recurring research or orbital artifacts continuously apply changing decisions",
            },
            "autonomous_capital": {
                "construction_packages": True,
                "recurring_packages": False,
                "reason": "commission installed deterministic control once; ordinary physical manufacturing does not burn Packages per batch",
            },
        },
        "candidates": [serialize(asdict(candidate)) for candidate in candidates],
        "results": [
            serialize(asdict(result))
            for candidate in candidates
            for orbital_power_closure in (True, False)
            for result in scenarios(candidate, orbital_power_closure)
        ],
        "industrial_control": {
            "scenarios": [
                serialize(asdict(result))
                for result in control_scenarios()
            ],
            "sensitivity_tournament": [
                serialize(asdict(result))
                for result in control_sensitivity_tournament()
            ],
            "electronics_iii": serialize(asdict(electronics_iii_balance())),
            "deployment_scale": [
                serialize(asdict(result))
                for result in package_scale_scenarios()
            ],
        },
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