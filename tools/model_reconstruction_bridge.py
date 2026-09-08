#!/usr/bin/env python3
"""Source-derived, boundary-explicit comparisons for the reconstruction bridge."""

from __future__ import annotations

import argparse
from dataclasses import dataclass
from fractions import Fraction
import hashlib
import json
from pathlib import Path
import re

from audit_recursive_industry_control_network import recipe_block
from simulate_recursive_industry_economy import (
    RACK_III_COMPUTING,
    RACK_III_COOLANT,
    RACK_III_MAINTENANCE_T3,
    RACK_III_POWER_MW,
    ceil_fraction,
)


ROOT = Path(__file__).resolve().parents[1]
RECIPE_FILES = {
    "AutonomousElectronicsIntegrationData.cs": (
        "IntegrateElectronics2Intermediates", "IntegrateElectronics2Direct",
    ),
    "AutonomousCapitalFabricationData.cs": (
        "FabricateConstructionParts", "FabricateConstructionParts2",
        "FabricateConstructionParts3", "IntegrateConstructionParts3",
        "FabricateVehicleParts", "FabricateVehicleParts2", "IntegrateVehicleParts2",
    ),
    "IndustrialControlGatewayData.cs": ("DeployIndustrialControl",),
    "ModelDevelopmentCenterData.cs": (
        "CurationIntensiveAdaptation", "ValidateControlPackages",
    ),
    "CurationOfficeData.cs": ("CurateDataset",),
    "SystemsIntegrationData.cs": ("ProduceFrontierProgram",),
}


@dataclass(frozen=True)
class Recipe:
    key: str
    owner: str
    inputs: dict[str, int]
    outputs: dict[str, int]
    duration: int
    power_percent: int

    def rate(self, product: str, material_capacity: Fraction = Fraction(450)) -> Fraction:
        if material_capacity <= 0:
            raise ValueError("Transport capacity must be positive")
        duration_bound = max(
            Fraction(self.duration),
            *(
                Fraction(quantity * 60, 200 if name == "IndustrialControlStream" else material_capacity)
                for amounts in (self.inputs, self.outputs)
                for name, quantity in amounts.items()
            ),
        )
        return self.outputs[product] * 60 / duration_bound


@dataclass(frozen=True)
class Machine:
    computing: int
    power_kw: int
    workers: int
    maintenance_t3: int
    capital: dict[str, int]


@dataclass(frozen=True)
class Headroom:
    installed_computing: int = 0
    ongoing_computing: int = 0
    reserve_computing: int = 0
    stream_per_60: Fraction = Fraction(0)
    packages_per_60: Fraction = Fraction(0)
    overlap_computing: int = 0

    def __post_init__(self) -> None:
        if any(value < 0 for value in vars(self).values()):
            raise ValueError("Headroom and demand cannot be negative")
        if self.ongoing_computing > self.installed_computing:
            raise ValueError("Existing Computing demand must be supplied")


def additional_racks(demand: int, installed: int, reserve: int = 0) -> int:
    if min(demand, installed, reserve) < 0:
        raise ValueError("Computing values cannot be negative")
    return max(0, ceil_fraction(Fraction(demand + reserve - installed, RACK_III_COMPUTING)))


def scalar(value: str, variables: dict[str, int]) -> int:
    result = int(value) if value.isdecimal() else variables[value]
    if result <= 0:
        raise ValueError(f"Expected positive recipe scalar: {value}")
    return result


def parse_recipe(text: str, owner: str, key: str, variables: dict[str, int]) -> Recipe:
    block = recipe_block(text, key)
    if block is None:
        raise ValueError(f"Missing recipe: {key}")
    products: dict[str, dict[str, int]] = {"Input": {}, "Output": {}}
    calls = re.findall(
        r"\.Add(Input|Output)\(\s*(\w+)\s*,\s*(?:RecursiveIndustryIds|Ids)"
        r"\.Products\.(\w+)\s*\)", block,
    )
    if len(calls) != len(re.findall(r"\.Add(?:Input|Output)\(", block)):
        raise ValueError(f"Unsupported product declaration in {key}")
    for direction, amount, product in calls:
        if product in products[direction]:
            raise ValueError(f"Duplicate {direction} product in {key}: {product}")
        products[direction][product] = scalar(amount, variables)
    binding = re.search(r"\.BindTo\(\w+,\s*(\w+)\.Seconds\(\)\)", block)
    if binding is None or not products["Output"]:
        raise ValueError(f"Unsupported binding or empty output in {key}")
    power = re.search(r"\.SetPowerMultiplier\((\d+)\.Percent\(\)\)", block)
    return Recipe(
        key, owner, products["Input"], products["Output"],
        scalar(binding.group(1), variables), int(power.group(1)) if power else 100,
    )


def parse_machine(text: str) -> Machine:
    def value(pattern: str, default: int | None = None) -> int:
        match = re.search(pattern, text)
        if match:
            return int(match.group(1))
        if default is not None:
            return default
        raise ValueError(f"Missing machine scalar: {pattern}")

    capital = {"ConstructionParts4": value(r"\.CP4\((\d+)\)")}
    for amount, product in re.findall(
        r"\.Product\(\s*(\d+),\s*(?:RecursiveIndustryIds|Ids)\.Products\.(\w+)\s*\)",
        text,
    ):
        capital[product] = capital.get(product, 0) + int(amount)
    return Machine(
        value(r"\.SetComputingConsumption\(Computing.FromTFlops\((\d+)\)\)", 0),
        value(r"\.SetElectricityConsumption\((\d+)\.Kw\(\)\)"),
        value(r"\.Workers\((\d+)\)"),
        value(r"\.MaintenanceT3\((\d+)\)"),
        capital,
    )


def load_source(root: Path = ROOT, overrides: dict[str, int] | None = None) -> tuple:
    mod = root / "mods" / "RecursiveIndustry"
    config_path = mod / "config.json"
    config = json.loads(config_path.read_text(encoding="utf-8"))
    values = {name: entry["default"] for name, entry in config.items()}
    for name, amount in (overrides or {}).items():
        if name not in config or type(amount) is not int:
            raise ValueError(f"Invalid config override: {name}")
        if not config[name]["min"] <= amount <= config[name]["max"]:
            raise ValueError(f"Config override outside declared range: {name}")
        values[name] = amount
    recipes: dict[str, Recipe] = {}
    machines: dict[str, Machine] = {}
    hashes = {"config.json": hashlib.sha256(config_path.read_bytes()).hexdigest()}
    for filename, members in RECIPE_FILES.items():
        path = mod / "src" / filename
        text = path.read_text(encoding="utf-8")
        variables = {
            variable: values[name]
            for variable, name in re.findall(
                r'int\s+(\w+)\s*=\s*mod.JsonConfig.GetInt\("([^"]+)"\s*,\s*\d+\)',
                text,
            )
        }
        machines[filename] = parse_machine(text)
        recipes.update({key: parse_recipe(text, filename, key, variables) for key in members})
        hashes[filename] = hashlib.sha256(path.read_bytes()).hexdigest()
    return recipes, machines, hashes


def conversion(
    recipes: dict[str, Recipe],
    machines: dict[str, Machine],
    targets: dict[str, Fraction],
    headroom: Headroom = Headroom(),
    material_capacity: Fraction = Fraction(450),
) -> dict:
    placements: dict[str, int] = {}
    external: dict[str, Fraction] = {}
    capital: dict[str, int] = {}
    computing = workers = maintenance = power_kw = 0

    def place(key: str, demand: Fraction) -> None:
        nonlocal computing, workers, maintenance, power_kw
        if demand < 0:
            raise ValueError("Production demand cannot be negative")
        if not demand:
            return
        recipe = recipes[key]
        if len(recipe.outputs) != 1 or key in placements:
            raise ValueError(f"Expected one dedicated single-output assignment: {key}")
        product, batch_output = next(iter(recipe.outputs.items()))
        count = ceil_fraction(demand / recipe.rate(product, material_capacity))
        placements[key] = count
        machine = machines[recipe.owner]
        computing += count * machine.computing
        workers += count * machine.workers
        maintenance += count * machine.maintenance_t3
        power_kw += count * machine.power_kw * recipe.power_percent // 100
        for name, amount in machine.capital.items():
            capital[name] = capital.get(name, 0) + count * amount
        for name, amount in recipe.inputs.items():
            external[name] = external.get(name, Fraction(0)) + demand * amount / batch_output

    for key, demand in targets.items():
        place(key, Fraction(demand))
    for intermediate, producer in (
        ("ConstructionParts2", "FabricateConstructionParts2"),
        ("ConstructionParts", "FabricateConstructionParts"),
    ):
        demand = external.pop(intermediate, Fraction(0))
        place(producer, demand)

    stream = external.pop("IndustrialControlStream", Fraction(0))
    stream_shortfall = max(Fraction(0), stream - headroom.stream_per_60)
    place("DeployIndustrialControl", stream_shortfall)
    packages = external.pop("ValidatedControlPackage", Fraction(0))
    package_shortfall = max(Fraction(0), packages - headroom.packages_per_60)
    place("ValidateControlPackages", package_shortfall)
    place("CurationIntensiveAdaptation", external.pop("ModelArchive", Fraction(0)))
    place("CurateDataset", external.pop("DatasetArchive", Fraction(0)))

    total_demand = headroom.ongoing_computing + computing
    racks = additional_racks(total_demand, headroom.installed_computing, headroom.reserve_computing)
    peak_racks = additional_racks(
        total_demand + headroom.overlap_computing,
        headroom.installed_computing,
        headroom.reserve_computing,
    )
    return {
        "placements": placements,
        "added_process_and_control_support_buildings": sum(placements.values()),
        "new_machine_computing_peak": computing,
        "added_rack_iii": racks,
        "cutover_added_rack_iii": peak_racks,
        "additional_rack_coolant": racks * RACK_III_COOLANT,
        "new_machine_workers": workers,
        "new_machine_maintenance_t3_per_month": maintenance,
        "additional_rack_maintenance_t3_per_month": racks * RACK_III_MAINTENANCE_T3,
        "new_machine_full_work_nameplate_kw": power_kw,
        "additional_rack_power_kw": racks * RACK_III_POWER_MW * 1000,
        "stream_demand_per_60": stream,
        "additional_gateway_stream_per_60": stream_shortfall,
        "additional_package_production_per_60": package_shortfall,
        "external_inputs_per_60": dict(sorted(external.items())),
        "construction_excluding_racks_and_external_utilities": dict(sorted(capital.items())),
        "separate_access_branches_required": ceil_fraction(stream_shortfall / 200),
    }


def capital_wait(requirements: dict, stock: dict, spare_rates_per_60: dict) -> dict:
    waits: dict[str, Fraction] = {}
    blocked: list[str] = []
    for product, quantity in requirements.items():
        deficit = max(Fraction(0), Fraction(quantity) - Fraction(stock.get(product, 0)))
        rate = Fraction(spare_rates_per_60.get(product, 0))
        if rate < 0 or Fraction(stock.get(product, 0)) < 0:
            raise ValueError("Stock and spare production cannot be negative")
        if deficit and rate <= 0:
            blocked.append(product)
        elif deficit:
            waits[product] = deficit * 60 / rate
    return {
        "parallel_supply_lower_bound_time": None if blocked else max(waits.values(), default=Fraction(0)),
        "blocking_products": sorted(blocked),
        "per_product_time": dict(sorted(waits.items())),
        "scope": "Spare supplied rates only; excludes research, construction duration, delivery, and startup batches.",
    }


def report(root: Path = ROOT) -> dict:
    recipes, machines, hashes = load_source(root)
    shared = Headroom(1024, 512, stream_per_60=Fraction(150), packages_per_60=Fraction(1))
    cases = {
        "electronics_raw_48_first": conversion(recipes, machines, {"IntegrateElectronics2Direct": Fraction(48)}),
        "electronics_raw_96_first": conversion(recipes, machines, {"IntegrateElectronics2Direct": Fraction(96)}),
        "electronics_raw_48_shared": conversion(recipes, machines, {"IntegrateElectronics2Direct": Fraction(48)}, shared),
        "capital_staged_96": conversion(recipes, machines, {"FabricateConstructionParts3": Fraction(96)}),
        "capital_integrated_96_first": conversion(recipes, machines, {"IntegrateConstructionParts3": Fraction(96)}),
        "combined_electronics48_capital48_first": conversion(recipes, machines, {
            "IntegrateElectronics2Direct": Fraction(48), "IntegrateConstructionParts3": Fraction(48),
        }),
        "capital_integrated_96_cutover": conversion(
            recipes, machines, {"IntegrateConstructionParts3": Fraction(96)},
            Headroom(2048, 768, overlap_computing=512),
        ),
    }
    entry = cases["electronics_raw_48_first"]["construction_excluding_racks_and_external_utilities"]
    prepared = {**entry, "FrontierProgram": 4}
    wait = capital_wait(entry, prepared, {
        "FrontierProgram": recipes["ProduceFrontierProgram"].rate("FrontierProgram"),
    })
    return {
        "schema_version": 1,
        "source_sha256": hashes,
        "units": "Recipe rates per 60 simulation Time; maintenance per game month; no wall-clock forecast.",
        "assumptions": [
            "Dedicated recipe assignments, configured defaults, material capacity 450 per 60 Time.",
            "Separate Access supply branches as needed; source/trunk capacities are bounds, not flow proof.",
            "Shared case starts with 1024 Computing, 512 ongoing demand, 150 Stream and 1 Package per 60 Time of supplied spare capacity.",
            "Package renewal uses dedicated validation, curation-adaptation, and Dataset production with source-derived rates.",
            "Peak Computing reserves all new assigned machines; full-work electricity is nameplate, not average consumption.",
        ],
        "exclusions": [
            "External material production, maintenance depots, power generation, Data Center shells and rack construction, transport geometry and construction.",
            "Electronics staged upstream PCB/Electronics suppliers are not modeled; do not infer their retirement or compare partial boundaries.",
            "No native research graph, migration, runtime, player-preference, or full-island cost claim.",
        ],
        "recipe_output_bounds_per_60": {
            key: {product: recipe.rate(product) for product in recipe.outputs}
            for key, recipe in sorted(recipes.items())
        },
        "cases": cases,
        "prepared_capital_wait_example": {
            "stock": prepared,
            "spare_frontier_programs_per_60": recipes["ProduceFrontierProgram"].rate("FrontierProgram"),
            "result": wait,
        },
    }


def encode(value: object) -> str:
    if isinstance(value, Fraction):
        return str(value)
    raise TypeError(type(value).__name__)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    payload = json.dumps(report(), default=encode, indent=2, sort_keys=True) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(payload, encoding="utf-8")
        print(f"Wrote source-bound bridge comparison: {args.output}")
    else:
        print(payload, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())