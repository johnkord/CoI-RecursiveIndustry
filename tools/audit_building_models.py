#!/usr/bin/env python3
"""Check complete original-building coverage and final bundled visual contracts."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path, PurePosixPath
import re

from bundle_manifest import bundle_files
from generate_building_model_paths import render


ROOT = Path(__file__).resolve().parents[1]
ASSET_MANIFEST = "art/RecursiveIndustry/Buildings/asset-manifest.json"
OWNERS = {
    "accelerator_works": "AcceleratorWorksData.cs",
    "curation_office": "CurationOfficeData.cs",
    "model_development_center": "ModelDevelopmentCenterData.cs",
    "ai_electronics_cell": "AIElectronicsCellData.cs",
    "electronics_reclaimer": "ElectronicsReclaimerData.cs",
    "ai_science_institute": "AppliedScienceData.cs",
    "pilot_science_complex": "AppliedScienceData.cs",
    "systems_integration_complex": "SystemsIntegrationData.cs",
    "control_deployment_gateway": "IndustrialControlGatewayData.cs",
    "deployment_assurance_campus": "DeploymentAssuranceData.cs",
    "autonomous_microchip_complex": "AutonomousMicrochipData.cs",
    "capital_fabrication_matrix": "AutonomousCapitalFabricationData.cs",
    "orbital_mission_complex": "OrbitalIndustryData.cs",
    "frontier_project_complex": "RecursiveFrontierData.cs",
    "recursive_integration_array": "RecursiveFrontierData.cs",
    "autonomous_construction_nexus": "RecursiveFrontierData.cs",
    "ai_operations_i": "AIOperationsData.cs",
    "ai_operations_ii": "AIOperationsData.cs",
    "ai_operations_iii": "AIOperationsData.cs",
    "orbital_power_array": "OrbitalPowerArrayData.cs",
    "companion_animal_center": "CompanionAnimalCareData.cs",
    "sensor_guided_greenhouse": "AdaptiveAgrifoodData.cs",
    "monitored_poultry_farm": "AdaptiveAgrifoodData.cs",
}
ANIMATED = {"monitored_poultry_farm", "companion_animal_center"}
RETAINED = {"electronics_integration", "planetary_coordination_center", "civic_model_center", "knowledge_commons"}


def require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def load_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def validate_records(catalog: dict, manifest: dict) -> None:
    require(catalog["schema_version"] == manifest["schema_version"] == 1, "Building schema drift")
    expected = {row["key"]: row for row in catalog["models"]}
    models = {row["key"]: row for row in manifest["models"]}
    require(len(catalog["models"]) == len(expected) == len(manifest["models"]) == 48, "Expected 48 unique new models")
    require(set(expected) == set(models), "Missing or unassigned building model")
    require(set(catalog["retained_reconstruction_models"]) == RETAINED, "Four accepted model identities changed")
    require(manifest["new_model_count"] == 48 and manifest["total_building_count"] == 52, "Total building coverage changed")
    require({row["key"] for row in catalog["models"] if not row.get("universal")} == set(OWNERS), "Non-universal owner inventory drift")
    require({row["key"] for row in catalog["models"] if row.get("animated")} == ANIMATED, "Native animation inventory drift")
    require(len({row["geometry_sha256"] for row in models.values()}) == 48, "Duplicate building geometry")
    require(expected["monitored_poultry_farm"]["origin_z"] == -2, "Poultry native origin must remain -2 world units")
    asset_paths = set(manifest["asset_paths"])
    require(all(path.startswith("assets/recursiveindustry/buildings/") for path in asset_paths), "Foreign artwork in building bundles")
    for key, model in models.items():
        spec = expected[key]
        require(model["asset_path"] == f'{catalog["asset_root"]}/{key}.prefab', f"Wrong prefab path: {key}")
        require(model["asset_path"].lower() in asset_paths, f"Prefab not in final bundle: {key}")
        require(model["member"] == spec["member"] and model["category"] == spec["category"], f"Owner identity mismatch: {key}")
        require(model["root_collider"] and model["native_emission"], f"Missing picking or emission hook: {key}")
        require(model["recipe_sign"] == spec.get("sign", False), f"Recipe sign contract mismatch: {key}")
        require(model["native_animation"] == spec.get("animated", False), f"Native animation mismatch: {key}")
        require(model["sampled_animation_motion"] == spec.get("animated", False), f"No final sampled animation motion: {key}")
        triangles = model["triangles_per_lod"]
        require(len(triangles) == 3 and 0 < triangles[2] < triangles[1] < triangles[0] <= 6000, f"LOD triangle budget/reduction failed: {key}")
        require(triangles[2] <= triangles[0] * 0.65, f"Far LOD reduction is insufficient: {key}")
        require(len(model["renderers_per_lod"]) == 3 and all(2 <= count <= 3 for count in model["renderers_per_lod"]), f"Renderer budget failed: {key}")
        require(model["nonblank_pixels"] > 840 * 600 // 70, f"Blank final-bundle preview: {key}")
        require(model["envelope"] == [spec["width"], spec["height"], spec["depth"]], f"Envelope declaration mismatch: {key}")
        center = model["bounds_center"]
        size = model["bounds_size"]
        require(abs(center[0]) + size[0] / 2 <= spec["width"] / 2 + 0.01, f"Horizontal overhang: {key}")
        require(abs(center[2] - spec.get("origin_z", 0)) + size[2] / 2 <= spec["depth"] / 2 + 0.01, f"Depth/origin overhang: {key}")
        require(center[1] - size[1] / 2 >= -0.01 and center[1] + size[1] / 2 <= spec["height"] + 0.01, f"Height overhang: {key}")

    names = [PurePosixPath(record["path"]).name for record in manifest["bundles"]]
    require(len(names) == len(set(names)) == 49, "Expected 48 prefab bundles and one shared building bundle")
    for prefix in ("buildings", *expected):
        require(sum(bool(re.fullmatch(re.escape(prefix) + r"_[0-9a-f]{4}", name)) for name in names) == 1, f"Missing or ambiguous bundle: {prefix}")
    require({row["name"] for row in manifest["dependencies"]} == set(names), "Bundle dependency inventory mismatch")
    for row in manifest["dependencies"]:
        require(set(row["dependencies"]) <= set(names) - {row["name"]}, "Foreign or self-referencing building dependency")


def source_text(root: Path, filename: str) -> str:
    return (root / "mods/RecursiveIndustry/src" / filename).read_text(encoding="utf-8")


def validate_source(root: Path, catalog: dict) -> None:
    require(source_text(root, "BuildingModelPaths.g.cs") == render(catalog), "Generated building paths are stale")
    universal = load_json(root / "data/universal-industry-catalog.json")
    require({row["key"] for row in catalog["models"] if row.get("universal")} == {row["key"] for row in universal["successor_facilities"]}, "Universal facility/model coverage differs")
    require("string prefabPath = BuildingModelPaths.Universal(spec.Key);" in source_text(root, "UniversalIndustryData.cs"), "Universal model lookup is not used")
    for spec in catalog["models"]:
        if spec.get("universal"):
            continue
        text = source_text(root, OWNERS[spec["key"]])
        path = "BuildingModelPaths." + spec["member"]
        require(len(re.findall(r"\b" + re.escape(path) + r"\b", text)) == 1, "Missing or repeated model assignment: " + spec["key"])
        if spec.get("sign"):
            block = text.split(".SetPrefabPath(" + path + ")", 1)[-1].split(".BuildAndAdd()", 1)[0]
            require(".AddSign()" in block and ".EnableSemiInstancedRendering(" not in block, "Native sign/LOD rendering lost: " + spec["key"])
    farm = "".join(source_text(root, "AdaptiveAgrifoodData.cs").split())
    for token in ("source.CropPositions", "source.SprinklerPrefabPath", "source.SprinklerSoundPath", "source.PrefabOrigin", "source.AnimationParams", "source.Layout", "source.SetNextTier(target)"):
        require(token in farm, "Native farm field lost: " + token)
    require(farm.count("useInstancedRendering:false,useSemiInstancedRendering:false") == 2, "Both farms require full native model rendering")
    combined = "\n".join(path.read_text(encoding="utf-8") for path in (root / "mods/RecursiveIndustry/src").glob("*.cs"))
    machine_bodies = re.findall(r"registrator\.MachineProtoBuilder(.*?)\.BuildAndAdd\(\)", combined, re.S)
    require(sum(".Start(spec.Name, spec.Id)" not in body for body in machine_bodies) == 18, "Expected 18 non-universal Machine owners; update complete art coverage")
    require(combined.count("OfficeBuildingFactory.Register(") == 4, "Expected four custom Offices")
    require(combined.count("new FarmProto(") == combined.count("new AnimalFarmProto(") == 1, "Expected two native-family farms")
    require(combined.count("registrator.SettlementModuleProtoBuilder") == 2, "Expected two settlement services")
    require(combined.count("new ElectricityGeneratorFromProductProto(") == 1, "Expected one orbital generator")


def validate_identity(root: Path, record: dict) -> None:
    relative = PurePosixPath(record["path"])
    require(not relative.is_absolute() and ".." not in relative.parts and "\\" not in str(relative) and ":" not in str(relative), "Unsafe asset record path")
    payload = (root / relative).read_bytes()
    require(len(payload) == record["size_bytes"] and hashlib.sha256(payload).hexdigest().upper() == record["sha256"], "Asset identity drift: " + str(relative))


def building_bundle_names(root: Path) -> set[str]:
    return {PurePosixPath(row["path"]).name for row in load_json(root / ASSET_MANIFEST)["bundles"]}


def audit(root: Path = ROOT) -> list[str]:
    try:
        catalog = load_json(root / "data/building-models.json")
        manifest = load_json(root / ASSET_MANIFEST)
        current_version = load_json(root / "mods/RecursiveIndustry/manifest.json")["version"]
        retained_version = load_json(root / "data/world-art.json")["retained_building_art_version"]
        require(catalog["version"] in (current_version, retained_version), "Building candidate version drift")
        validate_records(catalog, manifest)
        validate_source(root, catalog)
        for record in (manifest["catalog"], manifest["generator"], *manifest["bundles"], *manifest["previews"]):
            validate_identity(root, record)
        listed = bundle_files((root / "mods/RecursiveIndustry/AssetBundles/mafi_bundles.manifest").read_text(encoding="utf-8"))
        require(building_bundle_names(root) <= listed, "Final player manifest omits building dependencies")
        return []
    except (OSError, ValueError, KeyError, TypeError) as error:
        return [str(error)]


def main() -> int:
    errors = audit()
    if errors:
        print("Building artwork: FAIL\n" + "\n".join(errors))
        return 1
    print("PASS: all 52 building owners, 48 unique new models, 144 reducing LODs, exact bundles, signs, colliders, emissions, and two native animations")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())