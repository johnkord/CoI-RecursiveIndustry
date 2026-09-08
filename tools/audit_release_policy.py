#!/usr/bin/env python3
"""Check the current release candidate's compatibility and retained-art boundary."""

import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def validate(policy: dict, manifest: dict) -> list[str]:
    errors = []
    if policy.get("schema_version") != 1 or policy.get("intended_stage") != "release_candidate":
        errors.append("Release policy must declare a schema-1 candidate, not stable acceptance")
    if policy.get("candidate_version") != manifest.get("version"):
        errors.append("Release policy and manifest versions differ")
    expected_game = {"minimum": "0.8.7a", "maximum_verified": "0.8.7a", "build": 614}
    if policy.get("game") != expected_game or manifest.get("min_game_version") != "0.8.7a" or manifest.get("max_verified_game_version") != "0.8.7a":
        errors.append("Release support must match the verified Build 614 target")
    if policy.get("retained_art_versions") != {"buildings": "0.26.0a", "world": "0.27.0a"}:
        errors.append("Release must preserve the verified building and world art lineage")
    expected_support = {"new_campaigns_only": True, "add_to_existing_saves": False, "remove_from_existing_saves": False, "pre_release_save_migration": False}
    support = policy.get("support", {})
    if set(support) != set(expected_support) or any(support.get(key) is not value for key, value in expected_support.items()):
        errors.append("Release must not infer unsupported save compatibility")
    if not policy.get("required_release_gates") or not any("Independent uncoached" in item for item in policy["required_release_gates"]):
        errors.append("Independent acceptance must remain a release gate")
    return errors


def audit(root: Path = ROOT) -> list[str]:
    try:
        policy = json.loads((root / "data/release-policy.json").read_text(encoding="utf-8"))
        manifest = json.loads((root / "mods/RecursiveIndustry/manifest.json").read_text(encoding="utf-8"))
        return validate(policy, manifest)
    except (OSError, ValueError, TypeError, KeyError) as error:
        return [str(error)]


if __name__ == "__main__":
    errors = audit()
    print("Release policy: FAIL\n" + "\n".join(errors) if errors else "PASS: candidate version, Build 614 support, retained art, and unsupported migration policy")
    raise SystemExit(bool(errors))