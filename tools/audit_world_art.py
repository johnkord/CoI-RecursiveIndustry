#!/usr/bin/env python3
"""Check complete original world-art coverage and native visual contracts."""

from collections import Counter
import hashlib
import json
from pathlib import Path, PurePosixPath
import re

from audit_building_models import validate_identity, require
from bundle_manifest import bundle_files

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = "art/RecursiveIndustry/World/asset-manifest.json"
COUNTS = {"cargo": 6, "rack": 3, "fiber": 13, "vehicle": 8, "attachment": 8, "train": 17}
CARGO = {"experiment_program", "validated_research_dossier", "frontier_program", "frontier_expansion_project", "orbital_power_calibration", "companion_provisions"}
ATTACHMENTS = {"hauler_tank", "hauler_flatbed", "hauler_dump", "heavy_tank", "heavy_dump", "amphibious_tank", "amphibious_flatbed", "amphibious_dump"}
CONTROLS = {"locomotive_number": "_LocoNumber", "track_motion": "_TexOffset", "vehicle_lighting": "_EmissionStrength"}


def load(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def names(root: Path = ROOT) -> set[str]:
    return {PurePosixPath(row["path"]).name for row in load(root / MANIFEST)["bundles"]}


def validate_records(catalog: dict, manifest: dict) -> None:
    require(catalog["schema_version"] == manifest["schema_version"] == 1, "World-art schema drift")
    specs = {row["key"]: row for row in catalog["models"]}
    models = {row["key"]: row for row in manifest["models"]}
    require(len(catalog["models"]) == len(manifest["models"]) == len(specs) == len(models) == 55, "Expected all 55 unique world models")
    require(set(specs) == set(models), "World model assignment missing")
    require(Counter(row["category"] for row in specs.values()) == COUNTS, "World-art category coverage drift")
    require(catalog["retained_building_art_version"] == "0.26.0a", "The 52 building models must retain their existing art basis")
    require({row["key"] for row in specs.values() if row["category"] == "cargo"} == CARGO, "Six cargo identities changed")
    require({row["key"] for row in specs.values() if row["category"] == "attachment"} == ATTACHMENTS, "Eight attachment identities changed")
    paths = set(manifest["asset_paths"])
    require(all(path.startswith("assets/recursiveindustry/world/") for path in paths), "Foreign artwork in world bundles")
    for key, row in models.items():
        spec = specs[key]
        require(row["asset_path"] == f'{catalog["asset_root"]}/{key}.prefab' and row["asset_path"].lower() in paths, "Final model path missing: " + key)
        require(row["category"] == spec["category"] and row["family"] == spec["family"], "Wrong visual family: " + key)
        require(row["readable_meshes"] and row["nonblank_pixels"] >= 5000, "Unreadable or blank final model: " + key)
        triangles = row["triangles_per_lod"]
        expected_lods = 5 if spec["category"] in ("cargo", "fiber") else 3
        require(len(triangles) == expected_lods and all(0 < value <= 6000 for value in triangles), "World mesh budget/LOD count failed: " + key)
        require(all(triangles[index] >= triangles[index + 1] for index in range(len(triangles) - 1)), "LOD geometry grows at distance: " + key)
        require(len(row["renderers_per_lod"]) == expected_lods and max(row["renderers_per_lod"]) <= 24, "World renderer budget failed: " + key)
        if spec["category"] != "cargo":
            require(row["root_collider"], "Missing world-model root collider: " + key)
        if spec["category"] == "cargo":
            require(row["cargo_meshes"] == [f'{catalog["asset_root"]}/{key}-LOD{lod}.obj' for lod in range(5)], "Cargo LOD fallback gap: " + key)
            require(len(row["cargo_textures"]) == 3 and all(path.lower() in paths for path in row["cargo_textures"] + row["cargo_meshes"]), "Cargo PBR/mesh assets missing: " + key)
            require(row["cargo_import_pixels"] >= 5000 and row["cargo_import_preview"], "Imported cargo has no verified final render: " + key)
        if spec["category"] == "rack":
            require(set(row["required_renderers"]) == {f"DataCenter_Rack{index}" for index in range(1, 5)}, "Native rack panels lost: " + key)
        if spec["category"] == "fiber":
            require(row["native_mesh_material_extraction"], "Native Fiber extraction unverified: " + key)
        if spec["category"] in ("vehicle", "train", "attachment"):
            require(row["native_rig_contract"], "Native rig contract missing: " + key)
            require(row["sampled_animation_motion"] == bool(row["animation_states"]), "Native animation motion unverified: " + key)
        if spec["category"] == "train":
            require({"bogie_front", "bogie_rear", "coupler_front", "coupler_rear", "sign"} <= set(row["required_paths"]), "Native train sockets lost: " + key)
            root_required = spec.get("animate_wheels", False) or spec["family"].startswith("nuclear_")
            require((":Main" in row["animation_states"]) == root_required, "Native train root-controller presence changed: " + key)
        if spec["category"] == "attachment" and spec["family"] == "dump":
            require({":Up", ":Down", "bed/PileSmooth:Main", "bed/PileRough:Main"} <= set(row["animation_states"]), "Native dump/fill state missing: " + key)
        if spec.get("states") and spec["category"] == "vehicle":
            require({":" + state for state in spec["states"]} <= set(row["animation_states"]), "Working-vehicle state missing: " + key)
    bundles = [PurePosixPath(row["path"]).name for row in manifest["bundles"]]
    require(len(bundles) == len(set(bundles)) == 56, "Expected 55 prefab bundles and one world shared bundle")
    for prefix in ("world", *models):
        require(sum(bool(re.fullmatch(re.escape(prefix) + r"_[0-9a-f]{4}", name)) for name in bundles) == 1, "Exact world bundle absent: " + prefix)
    for row in manifest["dependencies"]:
        require(set(row["dependencies"]) <= set(bundles) - {row["name"]}, "Foreign world bundle dependency")
    require(len(manifest["assemblies"]) == 10 and all(row["nonblank_pixels"] >= 5000 for row in manifest["assemblies"]), "Composed vehicle/train previews missing")
    require(all(row["sampled_state_changes"] for row in manifest["assemblies"] if row["key"].endswith("dump")), "Composed dump motion not verified")
    require({row["key"]: row["property"] for row in manifest["controls"]} == CONTROLS and all(row["distinct_pixels"] for row in manifest["controls"]), "Native shader controls lack distinct rendered states")


def validate_source(root: Path, catalog: dict) -> None:
    source_root = root / "mods/RecursiveIndustry/src"
    read = lambda name: (source_root / name).read_text(encoding="utf-8")
    icons = read("RecursiveIndustryIcons.cs")
    models = {row["key"] for row in catalog["models"] if row["category"] in ("vehicle", "train")}
    dynamic = read("AutonomousNetworksData.cs") + read("AutonomousIndustrialVehiclesData.cs") + read("AutonomousTrainsData.cs")
    referenced = set(re.findall(r"RecursiveIndustryIcons\.(Autonomous\w+)", dynamic))
    identities = {member: key for member, key in re.findall(r'public const string (Autonomous\w+) = Root \+ "([a-z0-9_]+)\.png";', icons)}
    require({identities[member] for member in referenced} == models, "Vehicle/train model coverage differs from registration")
    require(dynamic.count("WorldModelPaths.ForIcon(") == 8, "Active dynamic model assignment count drift")
    for line in dynamic.splitlines():
        if "Assets/Base/" in line and ".prefab" in line:
            require("/Audio/" in line or "/Dust/" in line, "Active borrowed vehicle/train geometry: " + line.strip())
    cargo = read("AppliedScienceProductData.cs") + read("EpochProductData.cs") + read("CompanionAnimalCareData.cs")
    require(cargo.count("CountableProductGraphics.WithCustomModel(") == 6 and all('"' + key + '"' in cargo for key in CARGO), "Six original cargo assignments required")
    require('Assets.Base.Buildings.DataCenter.Rack_prefab' not in read("RackGenerationData.cs"), "Installed racks still borrow a model")
    require(all('"' + key + '"' in read("RackGenerationData.cs") for key in ("rack_i", "rack_ii", "rack_iii")), "Installed rack identity missing")
    transport = read("IndustrialControlTransportData.cs")
    for token in ("FiberGraphics.PortClosed", "FiberGraphics.CrossSections(", "FiberGraphics.Material", "FiberGraphics.Junction", "FiberGraphics.FlowIndicator(", "FiberGraphics.PillarAttachments(", "new TransportProto.Gfx.TransportInstancedRenderingData()"):
        require(token in transport, "Native Fiber graphics contract missing: " + token)
    require("source.Graphics.CrossSectionLods," not in transport and "source.Graphics.MaterialPath," not in transport, "Fiber still borrows native geometry/material")
    attachments = read("WorldAttachmentGraphics.cs")
    for token in ("tank.EligibleProductsFilter", "tank.KeepOnEvenIfNotNeeded", "flat.EligibleProductsFilter", "flat.Graphics.ProductRenderOffsets", "flat.KeepOnEvenIfNotNeeded", "dump.Graphics.PileTextureParams", "source.AttachmentWhenEmpty", "return replacements[index]"):
        require(token in attachments, "Native attachment contract lost: " + token)
    require("WorldAttachmentGraphics.CloneAmphibious(registrator, source.Attachments)" in dynamic and "WorldAttachmentGraphics.EmptySelection(source, attachments)" in dynamic, "Amphibious load/default mapping absent")


def audit(root: Path = ROOT) -> list[str]:
    try:
        catalog, manifest = load(root / "data/world-art.json"), load(root / MANIFEST)
        policy = load(root / "data/release-policy.json")
        require(policy["candidate_version"] == load(root / "mods/RecursiveIndustry/manifest.json")["version"], "Release candidate version drift")
        require(catalog["art_version"] == manifest["art_version"] == policy["retained_art_versions"]["world"], "Complete-art version drift")
        validate_records(catalog, manifest)
        validate_source(root, catalog)
        for row in (manifest["catalog"], *manifest["generators"], *manifest["bundles"], *manifest["previews"]):
            validate_identity(root, row)
        declared = bundle_files((root / "mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest").read_text(encoding="utf-8"))
        require(names(root) <= declared, "Player manifest omits world-art bundles")
        for control in manifest["controls"]:
            hashes = [hashlib.sha256((root / path).read_bytes()).hexdigest() for path in control["previews"]]
            require(len(set(hashes)) == 2, "Native control image evidence is identical")
        return []
    except (OSError, ValueError, KeyError, TypeError) as error:
        return [str(error)]


if __name__ == "__main__":
    errors = audit()
    print("World artwork: FAIL\n" + "\n".join(errors) if errors else "PASS: all 55 world models, native rigs, six cargo products, three racks, Fiber infrastructure, assemblies and shader controls")
    raise SystemExit(bool(errors))