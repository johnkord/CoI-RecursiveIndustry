#!/usr/bin/env python3
"""Generate exact building prefab paths from the complete model inventory."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "data/building-models.json"
OUTPUT = ROOT / "mods/RecursiveIndustry/src/BuildingModelPaths.g.cs"


def load() -> dict:
    data = json.loads(CATALOG.read_text(encoding="utf-8"))
    models = data["models"]
    if len(models) != 51 or len(data["retained_reconstruction_models"]) != 4:
        raise ValueError("Expected 51 generated and four retained building models")
    if len({row["key"] for row in models}) != len(models):
        raise ValueError("Duplicate building model")
    if sum(row.get("universal", False) for row in models) != 27:
        raise ValueError("Every universal facility must have a model")
    for row in models:
        if not re.fullmatch(r"[a-z0-9_]+", row["key"]) or min(row[field] for field in ("width", "depth", "height")) <= 0:
            raise ValueError(f"Invalid model entry: {row['key']}")
    return data


def render(data: dict) -> str:
    constants = "\n".join(f'    public const string {row["member"]} = Root + "{row["key"]}.prefab";' for row in data["models"])
    cases = "\n".join(f'            "{row["key"]}" => {row["member"]},' for row in data["models"] if row.get("universal"))
    return (
        'using System;\n\nnamespace RecursiveIndustry;\n\ninternal static class BuildingModelPaths\n{\n'
        f'    private const string Root = "{data["asset_root"]}/";\n{constants}\n\n'
        '    public static string Universal(string key) => key switch\n    {\n'
        f'{cases}\n        _ => throw new InvalidOperationException("Missing building model: " + key),\n    }};\n}}\n'
    )


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args()
    expected = render(load())
    if args.write:
        OUTPUT.write_text(expected, encoding="utf-8", newline="\n")
    elif not OUTPUT.exists() or OUTPUT.read_bytes() != expected.encode("utf-8"):
        raise ValueError("Building paths differ from the catalog; regenerate them")
    print("PASS: 51 generated building paths and 27 universal owners")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())