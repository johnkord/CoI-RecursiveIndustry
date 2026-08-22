#!/usr/bin/env python3
"""Validate the curated Recursive Industry public source repository."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path
import re
import struct
import subprocess
from typing import Any, Iterable
from urllib.parse import unquote

from audit_recursive_industry_control_network import audit as audit_control_network
from audit_recursive_industry_agrifood import audit as audit_agrifood


ROOT = Path(__file__).resolve().parents[1]
MOD_ROOT = ROOT / "mods" / "RecursiveIndustry"
ASSET_ROOT = ROOT / "art" / "RecursiveIndustry"

REQUIRED_ROOT_FILES = {
    ".gitattributes",
    ".gitignore",
    "CHANGELOG.md",
    "CONTRIBUTING.md",
    "LICENSE",
    "NOTICE.md",
    "README.md",
    "SECURITY.md",
}
REQUIRED_DATA = {
    "adaptive-agrifood.json",
    "industrial-control-network.json",
    "universal-industry-catalog.json",
}
REQUIRED_TOOLS = {
    "audit_recursive_industry_agrifood.py",
    "audit_recursive_industry_control_network.py",
    "audit_release_zip.py",
    "freeze_recursive_industry_ui_icons.py",
    "generate_recursive_industry_universal_source.py",
    "package_mod.py",
    "simulate_recursive_industry_economy.py",
    "validate_public_repo.py",
}
REQUIRED_MEDIA = {
    "hub-thumbnail.png": (512, 512),
    "social-preview.png": (1280, 640),
}
REQUIRED_DOCS = {
    "AGRIFOOD.md",
    "ARCHITECTURE.md",
    "BALANCE.md",
    "BUILDING.md",
    "COMPATIBILITY.md",
    "DESIGN.md",
    "DEVELOPMENT_WORKFLOW.md",
    "PROGRESSION.md",
    "PUBLISHING.md",
    "PLAYTESTING.md",
    "README.md",
    "ROADMAP.md",
    "VERIFICATION.md",
}
REQUIRED_BUNDLES = {"cartridge_c874", "producticons_84e1", "uiicons_5287"}
FORBIDDEN_PARTS = {
    "bin",
    "dist",
    "game_api_snapshots",
    "internal_builds",
    "iterations",
    "logs",
    "obj",
    "validationmod",
    "verification_evidence",
}
FORBIDDEN_SUFFIXES = {".dll", ".exe", ".log", ".pdb", ".recovery", ".sav", ".zip"}
FORBIDDEN_FILE_FRAGMENTS = {
    "credentials",
    "ids_reference",
    "recipes.json",
    "secret",
    "token",
}
MARKDOWN_LINK = re.compile(r"\[[^\]]+\]\(([^)]+)\)")
VERSION = re.compile(r"^\d+\.\d+\.\d+[a-z]?$", re.IGNORECASE)


def load_json(path: Path) -> dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"JSON root must be an object: {path}")
    return value


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def png_dimensions(path: Path) -> tuple[int, int] | None:
    header = path.read_bytes()[:24]
    if len(header) != 24 or header[:8] != b"\x89PNG\r\n\x1a\n":
        return None
    return struct.unpack(">II", header[16:24])


def repository_files(root: Path) -> list[Path]:
    try:
        result = subprocess.run(
            ["git", "-C", str(root), "ls-files", "-z"],
            check=False,
            capture_output=True,
        )
    except OSError:
        result = None
    if result is not None and result.returncode == 0 and result.stdout:
        return [
            root / item.decode("utf-8")
            for item in result.stdout.split(b"\0")
            if item
        ]
    ignored = {".git", "__pycache__", "bin", "dist", "obj"}
    return sorted(
        path
        for path in root.rglob("*")
        if path.is_file() and not any(part in ignored for part in path.parts)
    )


def forbidden_path_reason(relative: Path) -> str | None:
    parts = {part.casefold() for part in relative.parts}
    forbidden_parts = parts & FORBIDDEN_PARTS
    if forbidden_parts:
        return f"forbidden path component: {sorted(forbidden_parts)[0]}"
    name = relative.name.casefold()
    if relative.suffix.casefold() in FORBIDDEN_SUFFIXES:
        return f"forbidden file type: {relative.suffix}"
    if name in {".env", "id_rsa", "id_ed25519"}:
        return "credential-like filename"
    fragment = next(
        (item for item in FORBIDDEN_FILE_FRAGMENTS if item in name),
        None,
    )
    if fragment is not None:
        return f"forbidden filename fragment: {fragment}"
    return None


def validate_manifest(errors: list[str], root: Path) -> None:
    path = root / "mods" / "RecursiveIndustry" / "manifest.json"
    try:
        manifest = load_json(path)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        errors.append(str(exc))
        return
    expected = {
        "id": "RecursiveIndustry",
        "display_name": "Recursive Industry",
        "authors": "John Kordich",
        "primary_dlls": ["RecursiveIndustry.dll"],
        "mod_dependencies": ["COI-TrainsDlc >= 1.0.0"],
        "optional_mod_dependencies": ["COI-SupporterDlc >= 1.1.0"],
        "min_game_version": "0.8.6c",
        "max_verified_game_version": "0.8.7",
    }
    for key, value in expected.items():
        if manifest.get(key) != value:
            errors.append(f"manifest {key} drift: {manifest.get(key)!r}")
    if not VERSION.fullmatch(str(manifest.get("version", ""))):
        errors.append("manifest version is invalid")
    if manifest.get("links") != [
        "https://github.com/johnkord/CoI-RecursiveIndustry"
    ]:
        errors.append("manifest source link must point to the public repository")
    if path.parent.name != manifest.get("id"):
        errors.append("mod folder and manifest id must match")
    if manifest.get("non_locking_dll_load") is not True:
        errors.append("manifest non_locking_dll_load must remain true")


def validate_bundle_inventory(errors: list[str], root: Path) -> None:
    bundle_root = root / "mods" / "RecursiveIndustry" / "AssetBundles"
    manifest_path = bundle_root / "mafi_bundles.manifest"
    if not manifest_path.is_file():
        errors.append("mafi_bundles.manifest is missing")
        return
    declared = {
        line.strip()
        for line in manifest_path.read_text(encoding="utf-8").splitlines()
        if line.strip()
    }
    actual = {
        path.name
        for path in bundle_root.iterdir()
        if path.is_file() and path.name != manifest_path.name
    }
    if declared != REQUIRED_BUNDLES:
        errors.append(f"declared bundle inventory drift: {sorted(declared)}")
    if actual != declared:
        errors.append(
            f"shipped bundle inventory differs: {sorted(actual)} != {sorted(declared)}"
        )
    sidecars = [
        path.name
        for path in bundle_root.iterdir()
        if path.is_file()
        and path.suffix.casefold() == ".manifest"
        and path.name != "mafi_bundles.manifest"
    ]
    if sidecars:
        errors.append(f"Unity sidecar manifests cannot ship: {sidecars}")


def iter_dicts(value: Any) -> Iterable[dict[str, Any]]:
    if isinstance(value, dict):
        yield value
        for item in value.values():
            yield from iter_dicts(item)
    elif isinstance(value, list):
        for item in value:
            yield from iter_dicts(item)


def validate_file_identity(
    errors: list[str],
    root: Path,
    record: dict[str, Any],
    path_key: str,
    size_key: str = "size_bytes",
    hash_key: str = "sha256",
) -> None:
    relative = record.get(path_key)
    if not isinstance(relative, str) or not relative:
        return
    path = root / relative
    if not path.is_file():
        errors.append(f"referenced file is missing: {relative}")
        return
    if size_key in record and path.stat().st_size != record[size_key]:
        errors.append(f"size drift: {relative}")
    if hash_key in record and sha256(path) != record[hash_key]:
        errors.append(f"hash drift: {relative}")


def validate_asset_manifests(errors: list[str], root: Path) -> None:
    manifests = sorted((root / "art" / "RecursiveIndustry").rglob("asset-manifest.json"))
    if len(manifests) != 3:
        errors.append(f"expected three public asset manifests, found {len(manifests)}")
    for path in manifests:
        try:
            manifest = load_json(path)
        except (OSError, json.JSONDecodeError, ValueError) as exc:
            errors.append(str(exc))
            continue
        for record in iter_dicts(manifest):
            validate_file_identity(errors, root, record, "path")
            validate_file_identity(errors, root, record, "master", "master_size_bytes", "master_sha256")
            validate_file_identity(errors, root, record, "export", "export_size_bytes", "export_sha256")
            validate_file_identity(errors, root, record, "unity_manifest", "unity_manifest_size_bytes", "unity_manifest_sha256")
        generator = manifest.get("generator_path")
        if isinstance(generator, str):
            generator_record = {
                "path": generator,
                "sha256": manifest.get("generator_sha256"),
            }
            validate_file_identity(errors, root, generator_record, "path")


def validate_ui_icon_contract(errors: list[str], root: Path) -> None:
    path = root / "art" / "RecursiveIndustry" / "UiIcons" / "asset-manifest.json"
    try:
        manifest = load_json(path)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        errors.append(str(exc))
        return

    icons = manifest.get("icons")
    if not isinstance(icons, list) or len(icons) != 85:
        errors.append(
            f"UI icon manifest must contain exactly 85 identities, found "
            f"{len(icons) if isinstance(icons, list) else 'invalid'}"
        )
        return
    required_control = {
        "industrial_control_stream",
        "control_deployment_gateway",
        "deployment_assurance_campus",
        "access_fiber",
        "backbone_fiber",
        "fiber_junction",
        "precision_irrigation",
        "sensor_guided_greenhouse",
        "monitored_poultry_farm",
        "companion_provisions",
        "companion_animal_center",
    }
    names = {icon.get("name") for icon in icons}
    if not required_control <= names:
        errors.append(
            f"UI icon manifest is missing control identities: "
            f"{sorted(required_control - names)}"
        )

    source_root = root / "mods" / "RecursiveIndustry" / "src"
    source = "\n".join(
        (source_root / name).read_text(encoding="utf-8")
        for name in (
            "RecursiveIndustryIcons.cs",
            "UniversalIndustryIcons.g.cs",
        )
    )
    root_match = re.search(r'private const string Root = "([^"]+)";', source)
    if root_match is None:
        errors.append("RecursiveIndustryIcons.Root is missing")
    else:
        icon_root = root_match.group(1)
        constants = {
            member: icon_root + filename
            for member, filename in re.findall(
                r'public const string (\w+) = Root \+ "([^"]+\.png)";',
                source,
            )
        }
        manifest_paths = {icon.get("unity_path") for icon in icons}
        if len(constants) != 85:
            errors.append(
            f"C# source must declare exactly 85 UI icon constants, found {len(constants)}"
            )
        if set(constants.values()) != manifest_paths:
            errors.append("C# UI icon paths differ from the asset manifest")

    review = manifest.get("visual_review", {})
    if review.get("status") != "PASS_STATIC_PROOF_REVIEW":
        errors.append("UI icon static proof review is not passed")
    if review.get("runtime_status") != "OPEN_INTEGRATED_0.22.0A":
        errors.append("UI icon runtime review must remain on the integrated 0.22.0a boundary")
    if review.get("tested_sizes_px") != [24, 32, 48]:
        errors.append("UI icon proof must cover 24, 32, and 48 pixels")
    if review.get("backgrounds") != ["light", "dark"]:
        errors.append("UI icon proof must cover light and dark backgrounds")
    bundle = manifest.get("bundle", {})
    if bundle.get("name") != "uiicons_5287":
        errors.append("all public UI identities must remain in uiicons_5287")
    if bundle.get("dependencies") != []:
        errors.append("uiicons_5287 must remain dependency-free")


def validate_source_contract(errors: list[str], root: Path) -> None:
    source = root / "mods" / "RecursiveIndustry" / "src"
    files = sorted(source.glob("*.cs"))
    if len(files) != 65:
        errors.append(f"expected 65 C# source files, found {len(files)}")
    required = {
        "DataProductProto.cs",
        "DeploymentAssuranceData.cs",
        "IndustrialControlGatewayData.cs",
        "IndustrialControlProductData.cs",
        "IndustrialControlTransportData.cs",
        "RecursiveIndustry.cs",
        "RecursiveIndustryIds.Infrastructure.cs",
        "RecursiveIndustryResearchData.cs",
        "OrbitalPowerArrayData.cs",
        "OrbitalPowerArrayLayout.cs",
        "UniversalIndustryCatalog.cs",
        "UniversalIndustryCatalog.g.cs",
        "UniversalIndustryData.cs",
        "UniversalIndustryResearchData.cs",
        "WorldExchangeData.cs",
        "AdaptiveAgrifoodData.cs",
        "RecursiveIndustryIds.Farms.cs",
        "CompanionAnimalCareData.cs",
        "RecursiveIndustryIds.CompanionCare.cs",
    }
    missing = required - {path.name for path in files}
    if missing:
        errors.append(f"required source files missing: {sorted(missing)}")
    combined = "\n".join(path.read_text(encoding="utf-8") for path in files)
    relay_named_files = [path.name for path in files if "Relay" in path.name]
    if relay_named_files:
        errors.append(f"Orbital Power Relay source files remain: {relay_named_files}")
    for relay_token in (
        "OrbitalPowerRelay",
        "Orbital Power Relay",
        "RecursiveIndustry_OrbitalPowerRelay",
    ):
        if relay_token in combined:
            errors.append(f"removed Orbital Power Relay token remains: {relay_token}")
    for forbidden in ("Harmony", "System.Net", "HttpClient", "WebRequest", "TcpClient", "UdpClient"):
        if forbidden in combined:
            errors.append(f"forbidden runtime surface in C# source: {forbidden}")
    catalog_path = source / "UniversalIndustryCatalog.g.cs"
    if catalog_path.is_file():
        catalog = catalog_path.read_text(encoding="utf-8")
        if catalog.count("new UniversalFacilitySpec(") != 19:
            errors.append("generated catalog must contain 19 facilities")
        if catalog.count("new UniversalDirectBindingSpec(") != 235:
            errors.append("generated catalog must contain 235 Direct bindings")
        if not catalog.startswith("// Generated by tools/"):
            errors.append("generated catalog header is missing")
    data_path = source / "UniversalIndustryData.cs"
    if data_path.is_file():
        data = data_path.read_text(encoding="utf-8")
        for token in (
            "checked(binding.SourceBinding.Multiplier * 4)",
            "expected 235 direct bindings",
            "SetPowerMultiplier(powerMultiplierPercent.Percent())",
        ):
            if token not in data:
                errors.append(f"universal runtime contract missing: {token}")

    research_path = source / "RecursiveIndustryResearchData.cs"
    orbital_path = source / "OrbitalPowerArrayData.cs"
    if research_path.is_file():
        research = research_path.read_text(encoding="utf-8")
        if "RecursiveIndustryIds.Power.OrbitalPowerArray" not in research:
            errors.append("Orbital Breakthrough must unlock the Orbital Power Array")
    if orbital_path.is_file():
        orbital = orbital_path.read_text(encoding="utf-8")
        if "RecursiveIndustryIds.Power.OrbitalPowerArray" not in orbital:
            errors.append("Orbital Power Array prototype is missing")
        if 'GetInt("orbital_power_array_seconds", 360)' not in orbital:
            errors.append("Orbital Power Array support-duration config drift")


def validate_markdown_links(errors: list[str], root: Path, files: Iterable[Path]) -> None:
    for path in files:
        if path.suffix.casefold() != ".md":
            continue
        text = path.read_text(encoding="utf-8")
        for raw_target in MARKDOWN_LINK.findall(text):
            target = raw_target.strip()
            if target.startswith(("http://", "https://", "mailto:", "#")):
                continue
            target = unquote(target.split("#", 1)[0])
            if not target:
                continue
            resolved = (path.parent / target).resolve()
            if not resolved.exists():
                errors.append(
                    f"broken Markdown link in {path.relative_to(root)}: {raw_target}"
                )


def validate(root: Path = ROOT) -> list[str]:
    errors: list[str] = []
    files = repository_files(root)
    relative_files = [path.relative_to(root) for path in files]

    missing_root = sorted(name for name in REQUIRED_ROOT_FILES if not (root / name).is_file())
    if missing_root:
        errors.append(f"required root files missing: {missing_root}")
    missing_docs = sorted(name for name in REQUIRED_DOCS if not (root / "docs" / name).is_file())
    if missing_docs:
        errors.append(f"required public docs missing: {missing_docs}")
    missing_data = sorted(name for name in REQUIRED_DATA if not (root / "data" / name).is_file())
    if missing_data:
        errors.append(f"required public data missing: {missing_data}")
    missing_tools = sorted(name for name in REQUIRED_TOOLS if not (root / "tools" / name).is_file())
    if missing_tools:
        errors.append(f"required public tools missing: {missing_tools}")
    for name, dimensions in REQUIRED_MEDIA.items():
        path = root / "media" / name
        if not path.is_file():
            errors.append(f"required public media missing: {name}")
        elif png_dimensions(path) != dimensions:
            errors.append(
                f"public media dimensions drift: {name}={png_dimensions(path)}, expected {dimensions}"
            )

    for path, relative in zip(files, relative_files):
        reason = forbidden_path_reason(relative)
        if reason is not None:
            errors.append(f"{relative.as_posix()}: {reason}")
        if path.is_symlink():
            errors.append(f"symbolic links are not allowed: {relative.as_posix()}")

    validate_manifest(errors, root)
    try:
        config = load_json(root / "mods" / "RecursiveIndustry" / "config.json")
        if not config:
            errors.append("config.json cannot be empty")
        if "orbital_power_array_seconds" not in config:
            errors.append("Orbital Power Array config is missing")
        if "orbital_power_relay_seconds" in config:
            errors.append("legacy Orbital Power Relay config must not be player-facing")
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        errors.append(str(exc))
    validate_bundle_inventory(errors, root)
    validate_asset_manifests(errors, root)
    validate_ui_icon_contract(errors, root)
    validate_source_contract(errors, root)
    errors.extend(
        f"Industrial Control: {error}"
        for error in audit_control_network(root)
    )
    errors.extend(
        f"Adaptive Agrifood: {error}"
        for error in audit_agrifood(root)
    )
    validate_markdown_links(errors, root, files)

    notice = (root / "NOTICE.md").read_text(encoding="utf-8") if (root / "NOTICE.md").is_file() else ""
    required_notice = (
        "This Mod includes short excerpts or references to Captain of Industry Game Code."
    )
    if required_notice not in notice:
        errors.append("required MaFi Games code-reference notice is missing")
    license_text = (root / "LICENSE").read_text(encoding="utf-8") if (root / "LICENSE").is_file() else ""
    if "Captain of Industry Open License (COI-Open)" not in license_text:
        errors.append("COI-Open license is missing")

    return errors


def main() -> int:
    errors = validate()
    if errors:
        print("Recursive Industry public repository: FAIL")
        for error in errors:
            print(f"  ERROR: {error}")
        return 1
    print(
        "Recursive Industry public repository: PASS "
        "(65 source files, 19 facilities, 235 Direct bindings, 2 adaptive farms, 3 bundles)"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
