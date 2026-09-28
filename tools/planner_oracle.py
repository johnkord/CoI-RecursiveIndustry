#!/usr/bin/env python3
"""Independently audit exact planner quotes exported from registered prototypes."""

from __future__ import annotations

import argparse
import csv
from fractions import Fraction
from itertools import combinations
from pathlib import Path
import xml.etree.ElementTree as ET


def amounts(element: ET.Element, tag: str, multiplier: Fraction = Fraction(1)) -> dict[str, Fraction]:
    result = {}
    for item in element.findall(tag):
        product = item.get("product")
        result[product] = result.get(product, Fraction()) + Fraction(item.get("quantity")) * multiplier
    return result


def port_bound(process: ET.Element, capacities: dict[str, Fraction]) -> Fraction:
    maximum = 60 / Fraction(process.get("Duration"))
    for side, is_input in (("Inputs", "true"), ("Outputs", "false")):
        vector = amounts(process, side)
        ports = [(set(next(csv.reader([port.get("products")]))), capacities.get(port.get("kind"), Fraction()))
                 for port in process.findall("port") if port.get("input") == is_input]
        products = sorted({product for members, _ in ports for product in members})
        if len(products) > 16:
            raise ValueError("Oracle port component exceeds its explicit 16-product limit")
        for count in range(1, len(products) + 1):
            for subset in combinations(products, count):
                demand = sum((vector[product] for product in subset), Fraction())
                capacity = sum((rate for members, rate in ports if members.intersection(subset)), Fraction())
                maximum = min(maximum, capacity / demand)
    return maximum


def audit(root: ET.Element) -> tuple[list[str], int]:
    errors = []
    planner = root.find("planner")
    if planner is None:
        return ["Missing planner export"], 0
    capacities = {row.get("kind"): Fraction(row.get("value")) for row in planner.findall("capacity")}
    native = {machine.get("id") + "|" + recipe.get("id"): (machine, recipe)
              for machine in root.findall("machine") for recipe in machine.findall("recipe")}
    count = 0
    for process in planner.findall("process"):
        key = process.get("Key")
        if key not in native:
            errors.append("Missing registered binding: " + key)
            continue
        machine, recipe = native[key]
        inputs = amounts(process, "Inputs")
        outputs = amounts(process, "Outputs")
        if inputs != amounts(recipe, "input", Fraction(recipe.get("multiplier"))) or outputs != amounts(recipe, "output", Fraction(recipe.get("multiplier"))):
            errors.append("Binding vector normalization: " + key)
        duration = Fraction(recipe.get("duration_ticks")) / Fraction(root.get("ticks_per_second"))
        if duration != Fraction(process.get("Duration")):
            errors.append("Binding duration normalization: " + key)
        raw_power = Fraction(machine.get("power_raw")) * Fraction(recipe.get("power_raw")) / Fraction(root.get("percent_hundred_raw"))
        expected_power = raw_power.numerator // raw_power.denominator / Fraction(root.get("electricity_per_kw"))
        if expected_power != Fraction(process.get("PowerKw")):
            errors.append("Native rounded recipe power: " + key)
        maximum = port_bound(process, capacities)
        for quote in process.findall("quote"):
            count += 1
            requested = Fraction(quote.get("requested"))
            product = quote.get("product")
            batches = requested / outputs[product]
            full_output = outputs[product] * maximum
            fraction_hosts = requested / full_output
            hosts = -(-fraction_hosts.numerator // fraction_hosts.denominator)
            active = batches * duration / 60
            expected = {"maximum_batches": maximum, "Hosts": hosts, "Attainable": full_output * hosts,
                        "ActiveHosts": active, "PeakPower": expected_power * hosts,
                        "IdealAveragePower": expected_power * active,
                        "EnergyPerOutput": expected_power * active * 60 / requested}
            for field, value in expected.items():
                if Fraction(quote.get(field)) != value:
                    errors.append(f"{key}/{product}/{requested}: {field}")
            if amounts(quote, "Inputs") != {item: amount * batches for item, amount in inputs.items()}:
                errors.append(f"{key}/{product}/{requested}: inputs")
            if amounts(quote, "Outputs") != {item: amount * batches for item, amount in outputs.items()}:
                errors.append(f"{key}/{product}/{requested}: co-products")
    if not count:
        errors.append("No compiled quotes were checked")
    return errors, count


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("report", type=Path)
    args = parser.parse_args()
    errors, count = audit(ET.parse(args.report).getroot())
    print("FAIL: " + "\n".join(errors) if errors else f"PASS: {count} compiled quotes match the independent Fraction oracle")
    return int(bool(errors))


if __name__ == "__main__":
    raise SystemExit(main())