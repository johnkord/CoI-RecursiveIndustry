#!/usr/bin/env python3
"""Generate the public Recursive Industry universal catalog source."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import re
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
DATA_PATH = ROOT / "data" / "universal-industry-catalog.json"
SOURCE_ROOT = ROOT / "mods" / "RecursiveIndustry" / "src"
IDS_PATH = SOURCE_ROOT / "RecursiveIndustryIds.UniversalIndustry.cs"
CATALOG_PATH = SOURCE_ROOT / "UniversalIndustryCatalog.g.cs"
ICONS_PATH = SOURCE_ROOT / "UniversalIndustryIcons.g.cs"


def load_catalog() -> dict[str, Any]:
    value = json.loads(DATA_PATH.read_text(encoding="utf-8"))
    if not isinstance(value, dict) or value.get("schema_version") != 1:
        raise ValueError("public universal-industry catalog schema must be 1")
    facilities = value.get("facilities")
    integrated = value.get("integrated_recipes")
    precision = value.get("precision_recipes")
    research = value.get("research_keys")
    if not isinstance(facilities, list) or len(facilities) != 19:
        raise ValueError("public catalog must contain 19 facilities")
    if not isinstance(integrated, list) or len(integrated) != 21:
        raise ValueError("public catalog must contain 21 Integrated recipes")
    if not isinstance(precision, list) or len(precision) != 10:
        raise ValueError("public catalog must contain 10 Precision recipes")
    if not isinstance(research, list) or len(research) != 5:
        raise ValueError("public catalog must contain five research keys")
    keys = [facility.get("key") for facility in facilities]
    if len(set(keys)) != 19 or any(not isinstance(key, str) for key in keys):
        raise ValueError("facility keys must be 19 unique strings")
    direct_ids = [
        binding.get("recipe_id")
        for facility in facilities
        for binding in facility.get("direct_bindings", [])
    ]
    if len(direct_ids) != 235 or len(set(direct_ids)) != 235:
        raise ValueError("public catalog must contain 235 unique Direct bindings")
    return value


def pascal(value: str) -> str:
    return "".join(
        part[:1].upper() + part[1:]
        for part in re.split(r"[^A-Za-z0-9]+", value)
        if part
    )


def cs_string(value: str) -> str:
    return '"' + value.replace("\\", "\\\\").replace('"', '\\"') + '"'


def generate_ids(data: dict[str, Any]) -> str:
    machine_lines = []
    for facility in data["facilities"]:
        member = pascal(facility["key"])
        machine_lines.append(
            f"        public static readonly MachineID {member} =\n"
            f'            Ids.Machines.CreateId("RecursiveIndustry_{member}");'
        )

    recipe_lines = []
    for recipe in (*data["integrated_recipes"], *data["precision_recipes"]):
        member = pascal(recipe["key"])
        recipe_lines.append(
            f"        public static readonly RecipeID {member} =\n"
            f'            Ids.Recipes.CreateId("RecursiveIndustry_{member}");'
        )

    research_lines = []
    for key in data["research_keys"]:
        member = pascal(key)
        research_lines.append(
            f"        public static readonly ResearchID {member} =\n"
            f'            Ids.Research.CreateId("RecursiveIndustry_{member}");'
        )

    return """using Mafi.Base;
using MachineID = Mafi.Core.Factory.Machines.MachineProto.ID;
using RecipeID = Mafi.Core.Factory.Recipes.RecipeProto.ID;
using ResearchID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace RecursiveIndustry;

public static partial class RecursiveIndustryIds
{
    public static partial class Machines
    {
%s
    }

    public static partial class Recipes
    {
%s
    }

    public static partial class Research
    {
%s
    }
}
""" % ("\n\n".join(machine_lines), "\n\n".join(recipe_lines), "\n\n".join(research_lines))


def direct_rows(facility: dict[str, Any]) -> str:
    return ",\n".join(
        "                new UniversalDirectBindingSpec(%s, %s)"
        % (
            cs_string(binding["recipe_id"]),
            cs_string(binding["source_machine_id"]),
        )
        for binding in facility["direct_bindings"]
    )


def generate_facilities(data: dict[str, Any]) -> str:
    entries = []
    for facility in data["facilities"]:
        member = pascal(facility["key"])
        entries.append(
            """        new UniversalFacilitySpec(
            %s,
            %s,
            RecursiveIndustryIds.Machines.%s,
            %s,
            powerMw: %d,
            computing: %d,
            workers: %d,
            maintenanceT3: %d,
            cp4: %d,
            electronics4: %d,
            packages: %d,
            programs: %d,
            dossiers: %d,
            calibration: %d,
            directBindings: new[]
            {
%s
            })"""
            % (
                cs_string(facility["key"]),
                cs_string(facility["name"]),
                member,
                f"RecursiveIndustryIcons.{member}",
                facility["power_mw"],
                facility["selected_computing"],
                facility["workers"],
                facility["maintenance_t3"],
                facility["cp4"],
                facility["electronics4"],
                facility["selected_packages"],
                facility["programs"],
                facility["dossiers"],
                facility["calibration"],
                direct_rows(facility),
            )
        )
    return ",\n".join(entries)


def generate_integrated(data: dict[str, Any]) -> str:
    entries = []
    for recipe in data["integrated_recipes"]:
        sources = ", ".join(
            "new UniversalSourceRecipeSpec(%s, %d)"
            % (cs_string(source["recipe_id"]), source["multiplier"])
            for source in recipe["sources"]
        )
        entries.append(
            """        new UniversalIntegratedRecipeSpec(
            RecursiveIndustryIds.Recipes.%s,
            %s,
            %s,
            batchScale: %d,
            durationSeconds: %d,
            powerMultiplierPercent: %d,
            sources: new[] { %s })"""
            % (
                pascal(recipe["key"]),
                cs_string(recipe["name"]),
                cs_string(recipe["machine"]),
                recipe["batch_scale"],
                recipe["duration_seconds"],
                recipe.get("power_multiplier_percent", 200),
                sources,
            )
        )
    return ",\n".join(entries)


def generate_precision(data: dict[str, Any]) -> str:
    entries = []
    for recipe in data["precision_recipes"]:
        entries.append(
            """        new UniversalPrecisionRecipeSpec(
            RecursiveIndustryIds.Recipes.%s,
            %s,
            %s,
            %s)"""
            % (
                pascal(recipe["key"]),
                cs_string(recipe["name"]),
                cs_string(recipe["machine"]),
                cs_string(recipe["source_recipe_id"]),
            )
        )
    return ",\n".join(entries)


def generate_catalog(data: dict[str, Any]) -> str:
    return """// Generated by tools/generate_recursive_industry_universal_source.py.
namespace RecursiveIndustry;

internal static class UniversalIndustryCatalog
{
    public static readonly UniversalFacilitySpec[] Facilities =
    {
%s
    };

    public static readonly UniversalIntegratedRecipeSpec[] IntegratedRecipes =
    {
%s
    };

    public static readonly UniversalPrecisionRecipeSpec[] PrecisionRecipes =
    {
%s
    };
}
""" % (generate_facilities(data), generate_integrated(data), generate_precision(data))


def generate_icons(data: dict[str, Any]) -> str:
    lines = [
        f'    public const string {pascal(facility["key"])} = Root + "{facility["key"]}.png";'
        for facility in data["facilities"]
    ]
    return """// Generated by tools/generate_recursive_industry_universal_source.py.
namespace RecursiveIndustry;

internal static partial class RecursiveIndustryIcons
{
%s
}
""" % "\n".join(lines)


def generated_files() -> dict[Path, str]:
    data = load_catalog()
    return {
        IDS_PATH: generate_ids(data),
        CATALOG_PATH: generate_catalog(data),
        ICONS_PATH: generate_icons(data),
    }


def check() -> list[str]:
    return [
        path.name
        for path, expected in generated_files().items()
        if not path.is_file() or path.read_text(encoding="utf-8") != expected
    ]


def write() -> list[str]:
    changed = []
    for path, content in generated_files().items():
        if path.is_file() and path.read_text(encoding="utf-8") == content:
            continue
        path.write_text(content, encoding="utf-8", newline="\n")
        changed.append(path.name)
    return changed


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--write",
        action="store_true",
        help="write generated files instead of checking them",
    )
    args = parser.parse_args()
    if args.write:
        changed = write()
        print(
            "Universal-industry public source: "
            + ("updated " + ", ".join(changed) if changed else "no-op")
        )
        return 0
    drift = check()
    if drift:
        print("Universal-industry public source: FAIL")
        for name in drift:
            print(f"  ERROR: generated source drift: {name}")
        return 1
    print("Universal-industry public source: PASS")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
