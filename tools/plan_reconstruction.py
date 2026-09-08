#!/usr/bin/env python3
"""Compare bounded reconstruction projects using the existing exact source model."""

import argparse
from fractions import Fraction
import hashlib
import json
from pathlib import Path
import re

from model_reconstruction_bridge import Headroom, capital_wait, conversion, encode, load_source

ROOT = Path(__file__).resolve().parents[1]
PROJECTS = {
    "Electronics2": ("Electronics II", "IntegrateElectronics2Intermediates", "IntegrateElectronics2Direct"),
    "ConstructionParts3": ("Construction Parts III", "FabricateConstructionParts3", "IntegrateConstructionParts3"),
    "VehicleParts2": ("Vehicle Parts II", "FabricateVehicleParts2", "IntegrateVehicleParts2"),
}
FIELDS = {"schema_version", "product", "output_per_60", "material_capacity_per_60", "headroom", "config_overrides", "construction_stock", "spare_construction_rates_per_60"}
COMPUTING_FIELDS = {"installed_computing", "ongoing_computing", "reserve_computing", "overlap_computing"}
HEADROOM_FIELDS = COMPUTING_FIELDS | {"stream_per_60", "packages_per_60"}


def quantity(value, name: str, *, positive: bool = False) -> Fraction:
    if isinstance(value, bool) or not isinstance(value, (str, int, float)):
        raise ValueError(f"{name} must be a finite number or fraction")
    try:
        result = Fraction(str(value))
    except (ValueError, ZeroDivisionError) as error:
        raise ValueError(f"{name} must be a finite number or fraction") from error
    if result < 0 or positive and result == 0:
        raise ValueError(f"{name} must be {'positive' if positive else 'nonnegative'}")
    return result


def quantities(values, name: str) -> dict[str, Fraction]:
    if not isinstance(values, dict):
        raise ValueError(f"{name} must be an object")
    if any(not isinstance(key, str) or not re.fullmatch(r"[A-Za-z][A-Za-z0-9]{0,63}", key) for key in values):
        raise ValueError(f"{name} contains an invalid product key")
    return {key: quantity(value, f"{name}.{key}") for key, value in values.items()}


def plan(scenario: dict, root: Path = ROOT) -> dict:
    if not isinstance(scenario, dict) or set(scenario) - FIELDS:
        raise ValueError("Scenario contains unknown fields")
    if type(scenario.get("schema_version")) is not int or scenario["schema_version"] != 1:
        raise ValueError("Scenario schema_version must be 1")
    product = scenario.get("product")
    if not isinstance(product, str) or product not in PROJECTS:
        raise ValueError("Choose Electronics2, ConstructionParts3, or VehicleParts2")
    output = quantity(scenario.get("output_per_60"), "output_per_60", positive=True)
    capacity = quantity(scenario.get("material_capacity_per_60", 450), "material_capacity_per_60", positive=True)
    supplied = scenario.get("headroom", {})
    if not isinstance(supplied, dict) or set(supplied) - HEADROOM_FIELDS:
        raise ValueError("Unknown headroom field")
    headroom_values = {}
    for key, value in supplied.items():
        parsed = quantity(value, "headroom." + key)
        if key in COMPUTING_FIELDS:
            if parsed.denominator != 1:
                raise ValueError("Computing must be an integer: " + key)
            headroom_values[key] = int(parsed)
        else:
            headroom_values[key] = parsed
    headroom = Headroom(**headroom_values)
    stock = quantities(scenario.get("construction_stock", {}), "construction_stock")
    rates = quantities(scenario.get("spare_construction_rates_per_60", {}), "spare_construction_rates_per_60")
    overrides = scenario.get("config_overrides", {})
    if not isinstance(overrides, dict):
        raise ValueError("config_overrides must be an object")
    recipes, machines, hashes = load_source(root, overrides)
    label, staged, integrated = PROJECTS[product]
    routes = {}
    for mode, recipe_key in (("Staged", staged), ("Integrated", integrated)):
        result = conversion(recipes, machines, {recipe_key: output}, headroom, capacity)
        recipe = recipes[recipe_key]
        result["terminal_recipe"] = {"inputs": recipe.inputs, "outputs": recipe.outputs, "duration_time": recipe.duration,
                                     "transport_bounded_output_per_60": recipe.rate(product, capacity)}
        result["capital_timing"] = capital_wait(result["construction_excluding_racks_and_external_utilities"], stock, rates) if stock or rates else None
        routes[mode] = result
    return {
        "schema_version": 1,
        "product": product,
        "display_name": label,
        "requested_output_per_60": output,
        "material_capacity_per_60": capacity,
        "headroom": vars(headroom),
        "config_overrides": overrides,
        "routes": routes,
        "assumptions": [
            "Dedicated recipe assignments with curation-intensive Model adaptation and Rack III available.",
            "Only supplied spare Stream, Package capacity, and installed Computing are shared.",
            "Full-work power is installed nameplate, not utilization-weighted consumption.",
            "Rates use simulation Time; no wall-clock forecast or observed transport-flow claim.",
        ],
        "retained_suppliers": "Every listed external input still needs supply. No existing upstream building is assumed removable.",
        "exclusions": [
            "External material production, maintenance depots, generators, Data Center shells, rack construction, and transport construction.",
            "Whole-island labor/power ranking: staged PCB/Electronics suppliers are external, so their retirement or cost cannot be inferred from added-plant totals.",
            "Research completion, travel/delivery, construction duration, startup batches, and actual network topology.",
            "Save inspection, commands, automatic building placement, and automatic recipe changes.",
        ],
        "source_sha256": hashes,
        "model_sha256": hashlib.sha256((root / "tools/model_reconstruction_bridge.py").read_bytes()).hexdigest().upper(),
    }


def display(product: str) -> str:
    custom = {"ValidatedControlPackage": "Validated Control Packages", "DatasetArchive": "Dataset Archives", "ModelArchive": "Model Archives", "FrontierProgram": "Frontier Programs", "ValidatedResearchDossier": "Validated Research Dossiers", "PolySilicon": "Silicon (Poly)"}
    if product in custom:
        return custom[product]
    result = re.sub(r"([a-z])([A-Z])", r"\1 \2", product)
    for tier, roman in (("2", "II"), ("3", "III"), ("4", "IV")):
        if result.endswith(tier):
            result = result[:-1] + " " + roman
    return result


def markdown(report: dict) -> str:
    routes = report["routes"]
    lines = [f"# {report['display_name']} Conversion Worksheet", "",
             f"Target: {report['requested_output_per_60']} per 60 simulation Time. Material link bound: {report['material_capacity_per_60']} per 60 Time.", "",
             "These are added facilities and support, not whole-island totals. Both routes deliver the same requested output under the stated supply bounds.", "",
             "## Terminal Recipe Rows", "", "| Route | Inputs Per Batch | Outputs Per Batch | Nominal Time | Output Bound / 60 Time |",
             "| --- | --- | --- | ---: | ---: |"]
    for mode, route in routes.items():
        recipe = route["terminal_recipe"]
        inputs = " + ".join(f"{amount} {display(product)}" for product, amount in recipe["inputs"].items())
        outputs = " + ".join(f"{amount} {display(product)}" for product, amount in recipe["outputs"].items())
        lines.append(f"| {mode} | {inputs} | {outputs} | {recipe['duration_time']} | {recipe['transport_bounded_output_per_60']} |")
    lines += ["", "## Added Facilities and Support", "", "| Added Requirement | Staged | Integrated |", "| --- | ---: | ---: |"]
    metrics = (("Process and support buildings", "added_process_and_control_support_buildings"),
               ("Peak machine Computing", "new_machine_computing_peak"), ("Rack III, steady state", "added_rack_iii"),
               ("Rack III, during cutover", "cutover_added_rack_iii"), ("Machine workers", "new_machine_workers"),
               ("Machine full-work power, kW", "new_machine_full_work_nameplate_kw"), ("New rack power, kW", "additional_rack_power_kw"),
               ("Machine Maintenance III/month", "new_machine_maintenance_t3_per_month"), ("Rack Maintenance III/month", "additional_rack_maintenance_t3_per_month"),
               ("Stream demand/60 Time", "stream_demand_per_60"), ("New Package production/60 Time", "additional_package_production_per_60"))
    for label, field in metrics:
        lines.append(f"| {label} | {routes['Staged'][field]} | {routes['Integrated'][field]} |")
    lines += ["", "## Remaining External Supply", "", report["retained_suppliers"], "",
              "| Product / 60 Time | Staged | Integrated |", "| --- | ---: | ---: |"]
    products = set(routes["Staged"]["external_inputs_per_60"]) | set(routes["Integrated"]["external_inputs_per_60"])
    for product in sorted(products):
        lines.append(f"| {display(product)} | {routes['Staged']['external_inputs_per_60'].get(product, 0)} | {routes['Integrated']['external_inputs_per_60'].get(product, 0)} |")
    lines += ["", "## Commissioning Capital", "", "Excludes racks, Data Center shells, external utilities, and transports.", "",
              "| Product | Staged | Integrated |", "| --- | ---: | ---: |"]
    field = "construction_excluding_racks_and_external_utilities"
    for product in sorted(set(routes["Staged"][field]) | set(routes["Integrated"][field])):
        lines.append(f"| {display(product)} | {routes['Staged'][field].get(product, 0)} | {routes['Integrated'][field].get(product, 0)} |")
    for mode, route in routes.items():
        timing = route["capital_timing"]
        if timing is None:
            text = "not estimated; stock and spare production rates were not entered"
        elif timing["blocking_products"]:
            text = "not funded by the supplied stock/rates: " + ", ".join(display(product) for product in timing["blocking_products"])
        else:
            text = str(timing["parallel_supply_lower_bound_time"]) + " simulation Time lower bound, excluding delivery, construction, and startup"
        lines += ["", f"{mode} capital timing: {text}."]
    lines += ["", "## Boundary", ""] + ["- " + item for item in report["assumptions"] + report["exclusions"]]
    lines += ["", "## Sources", "", "- Current player recipe declarations and configuration, bound by the JSON report's source hashes.",
              "- [Exact Fraction model](../tools/model_reconstruction_bridge.py).", ""]
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("scenario", type=Path)
    parser.add_argument("--json", action="store_true", dest="as_json")
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    try:
        report = plan(json.loads(args.scenario.read_text(encoding="utf-8")))
    except (OSError, ValueError, TypeError, KeyError) as error:
        parser.error(str(error))
    result = json.dumps(report, default=encode, indent=2) + "\n" if args.as_json else markdown(report)
    if args.output:
        args.output.write_text(result, encoding="utf-8", newline="\n")
        print("Wrote conversion worksheet: " + str(args.output))
    else:
        print(result, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())