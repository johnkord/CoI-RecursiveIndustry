#!/usr/bin/env python3
"""Exact civic service capacity and shared support, at source config defaults."""

from __future__ import annotations

import argparse
from fractions import Fraction
import json
from pathlib import Path

from model_reconstruction_bridge import additional_racks, load_source
from simulate_recursive_industry_economy import ceil_fraction


ROOT = Path(__file__).resolve().parents[1]


def load_contract(root: Path = ROOT) -> dict:
    return json.loads((root / "data/civic-knowledge.json").read_text(encoding="utf-8"))


def civic_model(population: int, link_capacity: int = 450, root: Path = ROOT) -> dict:
    if population <= 0 or link_capacity <= 0:
        raise ValueError("Population and link capacity must be positive")
    contract = load_contract(root)
    config = json.loads((root / "mods/RecursiveIndustry/config.json").read_text(encoding="utf-8"))
    recipes, machines, hashes = load_source(root)
    monthly_demand = population * Fraction(contract["commons"]["stream_per_pop_month"])
    demand_per_60 = monthly_demand * 60 / contract["month_seconds"]
    recipe = contract["recipe"]
    supply_per_center = Fraction(recipe["output"] * 60, recipe["duration_seconds"])
    center_count = ceil_fraction(demand_per_60 / supply_per_center)
    batches_per_hour = demand_per_60 * 60 / recipe["output"]
    civic_inputs = {name: batches_per_hour * amount for name, amount in recipe["inputs"].items()}

    dossiers = civic_inputs["ValidatedResearchDossier"]
    pilot_output = Fraction(3600 * config["validated_dossiers_per_batch"]["default"], config["pilot_validation_seconds"]["default"])
    pilot_count = ceil_fraction(dossiers / pilot_output)
    pilot_batches = dossiers / config["validated_dossiers_per_batch"]["default"]
    science_output = Fraction(3600 * config["experiment_programs_per_batch"]["default"], config["experiment_program_seconds"]["default"])
    science_count = ceil_fraction(pilot_batches / science_output)
    science_batches = pilot_batches / config["experiment_programs_per_batch"]["default"]
    models = civic_inputs["ModelArchive"] + science_batches
    model_recipe = recipes["CurationIntensiveAdaptation"]
    model_count = ceil_fraction(models / (model_recipe.rate("ModelArchive") * 60))
    datasets = civic_inputs["DatasetArchive"] + science_batches * 8 + models * model_recipe.inputs["DatasetArchive"]
    curation_recipe = recipes["CurateDataset"]
    curation_count = ceil_fraction(datasets / (curation_recipe.rate("DatasetArchive") * 60))
    computing = center_count * contract["center"]["computing"] + pilot_count * 8 + science_count * 64 + model_count * 24
    workers = center_count * 40 + 12 + pilot_count * 80 + science_count * 32 + model_count * 24 + curation_count * 80
    return {
        "population": population,
        "demand_per_60_time": demand_per_60,
        "center_output_per_60_time": supply_per_center,
        "required_centers": center_count,
        "center_utilization": demand_per_60 / (center_count * supply_per_center),
        "link_capacity_per_60_time": link_capacity,
        "link_sufficient": demand_per_60 <= link_capacity,
        "module_buffer_months_at_full_demand": Fraction(contract["commons"]["buffer"], monthly_demand),
        "direct_civic_inputs_per_3600_time": civic_inputs,
        "shared_support": {
            "pilot_complexes": pilot_count, "science_institutes": science_count,
            "model_development_centers": model_count, "curation_offices": curation_count,
            "total_models_per_3600_time": models, "total_datasets_per_3600_time": datasets,
            "peak_computing": computing, "rack_iii_without_installed_headroom": additional_racks(computing, 0),
            "staff_including_center_and_one_commons": workers,
        },
        "scope": "Demand model, not observed flow. Shared support combines civic and science demand before rounding. External Lab Equipment, Office Supplies, Titanium, maintenance, generation, and Data Center shell supply remain required. No research or construction time forecast.",
        "upstream_source_hashes": hashes,
    }


def audit(root: Path = ROOT) -> list[str]:
    errors = []
    contract = load_contract(root)
    manifest = json.loads((root / "mods/RecursiveIndustry/manifest.json").read_text(encoding="utf-8"))
    if manifest["version"] != contract["candidate_version"]:
        errors.append("Civic candidate version mismatch")
    source = "".join((root / "mods/RecursiveIndustry/src/CivicKnowledgeData.cs").read_text(encoding="utf-8").split())
    for token in (
        "newDataProductProto(", "healthGiven:null", "1.2.Upoints()", ".Workers(40).MaintenanceT3(6)",
        ".Workers(12).MaintenanceT2(4)", ".SetInput(stream,0.2.ToFix64(),240)",
        ".SetComputingConsumption(Computing.FromTFlops(128))", ".BindTo(center,360.Seconds())",
        ".AddOutput(1600,streamId)", ".WithCommonInputPorts(", ".WithCommonOutputPorts((streamId,\"X\"))",
        "CIVIC_KNOWLEDGE_REGISTERED",
    ):
        if token not in source:
            errors.append(f"Civic source missing {token}")
    for product, quantity in contract["recipe"]["inputs"].items():
        prefix = "Ids" if product == "OfficeSupplies" else "RecursiveIndustryIds"
        if f".AddInput({quantity},{prefix}.Products.{product})" not in source:
            errors.append(f"Civic input differs: {product}")
    research = "".join((root / "mods/RecursiveIndustry/src/ReleaseResearchTree.g.cs").read_text(encoding="utf-8").split())
    for token in ("civic.AddParent(digitalInfrastructure)", "civic.AddParent(physicalValidation)",
                  "civic.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.IspModule))",
                  "civic.GridPosition=newVector2i(192,42)"):
        if token not in research:
            errors.append(f"Civic research missing {token}")
    if ".SetRequireSpacePoints()" in source or ".AddInput(" in source and "Products.IndustrialControlStream)" in source:
        errors.append("Civic service must not require Space or consume Industrial Control")
    if contract["recipe"]["output"] != 1600 or contract["recipe"]["duration_seconds"] != 360:
        errors.append("Civic output vector drift")
    if contract["commons"]["health"] is not None or contract["commons"]["productivity"] is not None:
        errors.append("Civic must be optional Unity only")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--population", type=int, default=1000)
    parser.add_argument("--capacity", type=int, default=450)
    args = parser.parse_args()
    errors = audit()
    if errors:
        print("\n".join(errors))
        return 1
    print(json.dumps(civic_model(args.population, args.capacity), default=str, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())