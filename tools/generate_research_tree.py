#!/usr/bin/env python3
"""Generate native research registrations from the reviewed release tree."""

import argparse
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "data/research-tree.json"
OUTPUT = ROOT / "mods/RecursiveIndustry/src/ReleaseResearchTree.g.cs"
IDS = ROOT / "mods/RecursiveIndustry/src/RecursiveIndustryIds.ReleaseResearch.g.cs"
DOC = ROOT / "docs/RESEARCH_TREE.md"
REFERENCES = {
    "industrial-control-network.json": {"research": "industrialControl", "federated_deployment": "federatedDeployment"},
    "adaptive-agrifood.json": {"research": "adaptiveAgrifood", "poultry_research": "monitoredPoultry"},
    "circular-agrifood.json": {"research": "circularAgrifood"},
    "civic-knowledge.json": {"research": "civic"},
}


def load(root: Path = ROOT) -> dict:
    data = json.loads((root / "data/research-tree.json").read_text(encoding="utf-8"))
    nodes = data["nodes"]
    if data["schema_version"] != 1 or len({node["key"] for node in nodes}) != len(nodes) or len({node["id"] for node in nodes}) != len(nodes):
        raise ValueError("Research schema, keys, or ids are invalid")
    known = {node["key"] for node in nodes}
    for node in nodes:
        if not re.fullmatch(r"[A-Za-z][A-Za-z0-9]*", node["key"]) or not re.fullmatch(r"[A-Za-z][A-Za-z0-9]*", node["id"]):
            raise ValueError("Invalid research identifier")
        if not set(node["parents"]) <= known or node["key"] in node["parents"]:
            raise ValueError("Unknown or self research parent: " + node["key"])
        if node["cost_months"] <= 0 or len(node["position"]) != 2:
            raise ValueError("Invalid research cost/position: " + node["key"])
        if node.get("repeatable") and any(node["unlock"].values()):
            raise ValueError("Repeatable research cannot unlock content")
    order(nodes)
    return data


def node(key: str, root: Path = ROOT) -> dict:
    return next(item for item in load(root)["nodes"] if item["key"] == key)


def contract_references(root: Path = ROOT) -> dict[Path, str]:
    result = {}
    for name, fields in REFERENCES.items():
        path = root / "data" / name
        data = json.loads(path.read_text(encoding="utf-8"))
        for field, key in fields.items():
            data[field] = {"catalog": "research-tree.json", "key": key}
        result[path] = json.dumps(data, indent=2, ensure_ascii=True) + "\n"
    return result


def order(nodes: list[dict]) -> list[dict]:
    pending = list(nodes)
    result, added = [], set()
    while pending:
        available = next((node for node in pending if set(node["parents"]) <= added), None)
        if available is None:
            raise ValueError("Research cycle: " + ", ".join(node["key"] for node in pending))
        pending.remove(available)
        added.add(available["key"])
        result.append(available)
    return result


def literal(value: str) -> str:
    return json.dumps(value, ensure_ascii=True)


def render(data: dict) -> str:
    output = [
        "using System;", "using Mafi;", "using Mafi.Base;", "using Mafi.Core;",
        "using Mafi.Core.Buildings.Farms;", "using Mafi.Core.Buildings.Offices;", "using Mafi.Core.Buildings.VehicleDepots;",
        "using Mafi.Core.Factory.Datacenters;", "using Mafi.Core.Factory.Transports;", "using Mafi.Core.Mods;",
        "using Mafi.Core.Population;", "using Mafi.Core.Research;", "using Mafi.Core.SpaceProgram;", "using Mafi.Core.Trains;",
        "using Mafi.Core.Vehicles.Excavators;", "using Mafi.Core.Vehicles.TreeHarvesters;", "using Mafi.Core.Vehicles.TreePlanters;",
        "using Mafi.Core.Vehicles.Trucks;", "using Mafi.TrainsDlc;", "", "namespace RecursiveIndustry;", "",
        "internal sealed class ReleaseResearchTree : IResearchNodesData", "{", "    public void RegisterData(ProtoRegistrator registrator)", "    {",
    ]
    for node in order(data["nodes"]):
        indent = "        "
        optional = node.get("optional_proto")
        if optional:
            output += [f"        if (registrator.PrototypesDb.TryGetProto<{optional[0]}>({optional[1]}, out _))", "        {"]
            indent += "    "
        key = node["key"]
        output += [f"{indent}ResearchNodeProto {key} = registrator.ResearchNodeProtoBuilder", f'{indent}    .Start({literal(node["title"])}, RecursiveIndustryIds.Research.{node["id"]}, costMonths: {node["cost_months"]})']
        repeat = node.get("repeatable")
        if repeat:
            output += [f'{indent}    .DescriptionPerLevelWithBonus({literal(node["description"])}, {repeat["bonus_percent"]}.Percent())',
                       f'{indent}    .SetRepeatableProperties({repeat["levels"]}, IdsCore.PropertyIds.ResearchEfficiencyMultiplier, {repeat["bonus_percent"]}.Percent(), CoDesignCost)']
            if "space_from_level" in repeat:
                output.append(f'{indent}    .SetSpacePointRequiredFrom({repeat["space_from_level"]})')
        else:
            output += [f'{indent}    .Description({literal(node["description"])})']
        unlocked = node["unlock"]
        for product in unlocked.get("products", []):
            output.append(f"{indent}    .AddProductToUnlock(RecursiveIndustryIds.Products.{product}, addIconToNode: true)")
        for machine in unlocked.get("machines", []):
            output.append(f"{indent}    .AddMachineToUnlock(RecursiveIndustryIds.Machines.{machine}, unlockAllRecipes: false)")
        for recipe in unlocked.get("recipes", []):
            output.append(f"{indent}    .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.{recipe})")
        for building in unlocked.get("buildings", []):
            output.append(f"{indent}    .AddLayoutEntityToUnlock(RecursiveIndustryIds.{building})")
        for vehicle in unlocked.get("vehicles", []):
            output.append(f"{indent}    .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.{vehicle})")
        for focus in unlocked.get("focuses", []):
            output.append(f"{indent}    .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.{focus}))")
        for proto_type, identifier in unlocked.get("protos", []):
            output.append(f"{indent}    .AddProtoToUnlock<{proto_type}>({identifier})")
        for product, quantity in node.get("lifetime", []):
            output.append(f"{indent}    .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.{product}, {quantity})")
        if node.get("station_tier"):
            output.append(f"{indent}    .AddRequirementForMinSpaceStationTier(SpaceStationProto.RESEARCH_TIER_FROM)")
        if node.get("space"):
            output.append(f"{indent}    .SetRequireSpacePoints()")
        output += [f"{indent}    .BuildAndAdd();", f'{indent}{key}.GridPosition = new Vector2i({node["position"][0]}, {node["position"][1]});']
        for parent in node["parents"]:
            output.append(f"{indent}{key}.AddParent({parent});")
        for parent in node.get("native_parents", []):
            output.append(f"{indent}{key}.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>({parent}));")
        if node.get("facilities"):
            fields = ", ".join("RecursiveIndustryIds.Machines." + name for name in node["facilities"])
            output.append(f"{indent}ReconstructionResearchPrerequisites.AddSources(registrator, {key}, {fields});")
        if node.get("native_sources"):
            fields = ", ".join(f"registrator.PrototypesDb.GetOrThrow<{proto_type}>({identifier})" for proto_type, identifier in node["native_sources"])
            output.append(f"{indent}ReconstructionResearchPrerequisites.AddNativeOwners(registrator, {key}, {fields});")
        if optional:
            output.append("        }")
        output.append("")
    output += [f'        Log.Info("RecursiveIndustry: RELEASE_RESEARCH_REGISTERED version={data["candidate_version"]} nodes_max={len(data["nodes"])} native_recipe_locks=true");',
               "    }", "", "    private static long CoDesignCost(long baseCost, int level)", "    {",
               "        return checked(baseCost * (level == 0 ? 1 : 3));", "    }", "}", ""]
    return "\n".join(output)


def render_ids(data: dict) -> str:
    output = ["using Mafi.Base;", "using Mafi.Core.Research;", "", "namespace RecursiveIndustry;", "",
              "public static partial class RecursiveIndustryIds", "{", "    public static partial class Research", "    {"]
    for node in data["nodes"]:
        if node.get("new_id"):
            output += [f'        public static readonly ResearchNodeProto.ID {node["id"]} = Ids.Research.CreateId("RecursiveIndustry_{node["id"]}");']
    return "\n".join(output + ["    }", "}", ""])


def render_doc(data: dict) -> str:
    nodes = {item["key"]: item for item in data["nodes"]}
    output = ["# Research Tree", "", f"Candidate: {data['candidate_version']}. New campaigns only.", "",
              "The shared foundation is accelerator hardware, curated Models, and validated deployment. "
              "Research and physical validation lead to Systems Integration; applications then branch independently.", "",
              "Rack II/III and Office II/III are optional density investments. They do not gate ordinary "
              "industrial conversions, Civic Knowledge, or orbital projects. The Planetary Center and "
              "Recursive Integration Array are the explicit scale-up exceptions.", "",
              "The Array opens after 32 lifetime Programs and costs 32 Programs to commission. Its "
              "eight-Program recipe is available while major construction demand remains. The Nexus still "
              "requires an Expansion Project. Beamed power is optional for Frontier Megaprojects; "
              "terrestrial nuclear processing requires neither Calibration nor Space Research.", "",
              "Costs below are research months before efficiency modifiers, not wall-clock waiting times. "
              "Listed parents are mod branches. Native source technologies, capital products, logistics, "
              "and supplied power/Computing remain required. Direct recipes retain their native research locks.", ""]
    for lane in dict.fromkeys(item["lane"] for item in data["nodes"]):
        output += ["## " + lane.title(), "", "| Research | Months | Mod Parents |", "| --- | ---: | --- |"]
        for item in data["nodes"]:
            if item["lane"] != lane:
                continue
            parents = ", ".join(nodes[parent]["title"] for parent in item["parents"]) or "Native entry technologies"
            suffix = " (optional Supporter content)" if item.get("optional_proto") else ""
            output.append(f"| {item['title']}{suffix} | {item['cost_months']} | {parents} |")
        output.append("")
    output += ["## Sources", "", "- [Canonical research catalog](../data/research-tree.json).",
               "- [Generated native registrations](../mods/RecursiveIndustry/src/ReleaseResearchTree.g.cs).",
               "- [Native source prerequisite resolver](../mods/RecursiveIndustry/src/ReconstructionResearchPrerequisites.cs).", ""]
    return "\n".join(output)


def audit_source(text: str, root: Path = ROOT) -> list[str]:
    return [] if text.replace("\r\n", "\n") == render(load(root)) else ["research registrations differ from the canonical catalog"]


def audit(root: Path = ROOT) -> list[str]:
    try:
        data = load(root)
        expected = {
            root / OUTPUT.relative_to(ROOT): render(data),
            root / IDS.relative_to(ROOT): render_ids(data),
            root / DOC.relative_to(ROOT): render_doc(data),
            **contract_references(root),
        }
        return [f"research projection differs: {path.relative_to(root)}" for path, content in expected.items()
                if not path.is_file() or path.read_text(encoding="utf-8") != content]
    except (OSError, ValueError, KeyError, TypeError) as error:
        return [str(error)]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args()
    data = load()
    files = {OUTPUT: render(data), IDS: render_ids(data), DOC: render_doc(data), **contract_references()}
    for path, expected in files.items():
        if args.write:
            path.write_text(expected, encoding="utf-8", newline="\n")
        elif not path.exists() or path.read_text(encoding="utf-8") != expected:
            raise ValueError("Generated research source differs: " + path.name)
    print(f"PASS: {len(data['nodes'])} research definitions, acyclic generation, explicit recipe ownership")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())