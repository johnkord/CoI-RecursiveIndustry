#!/usr/bin/env python3
"""Regression tests for the public repository boundary."""

from __future__ import annotations

from pathlib import Path
import json
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from validate_public_repo import (  # noqa: E402
    forbidden_path_reason,
    validate,
)
from generate_recursive_industry_universal_source import (  # noqa: E402
    check as check_generated,
    load_catalog,
)


class PublicRepositoryTests(unittest.TestCase):
    def test_repository_passes(self) -> None:
        self.assertEqual(validate(), [])

    def test_generated_source_matches_public_catalog(self) -> None:
        self.assertEqual(check_generated(), [])

    def test_composition_only_source_has_explicit_runtime_binding(self) -> None:
        catalog = load_catalog()
        direct_ids = {
            binding["recipe_id"]
            for facility in catalog["facilities"]
            for binding in facility["direct_bindings"]
        }
        composition_only = [
            source
            for recipe in catalog["integrated_recipes"]
            for source in recipe["sources"]
            if source["recipe_id"] not in direct_ids
        ]
        self.assertEqual(
            composition_only,
            [
                {
                    "recipe_id": "Electronics2Assembly",
                    "multiplier": 12,
                    "source_machine_id": "AssemblyRoboticT2",
                }
            ],
        )
        runtime = (
            ROOT
            / "mods"
            / "RecursiveIndustry"
            / "src"
            / "UniversalIndustryData.cs"
        ).read_text(encoding="utf-8")
        self.assertIn("directByRecipe.TryGetValue(source.RecipeId, out binding)", runtime)
        self.assertIn("source.SourceMachineId", runtime)

    def test_general_manufacturing_uses_one_top_edge_input(self) -> None:
        control = json.loads(
            (ROOT / "data" / "industrial-control-network.json").read_text(
                encoding="utf-8"
            )
        )
        owners = {row["key"]: row for row in control["owners"]}
        self.assertEqual(
            owners["general_manufacturing_fab"],
            {
                "key": "general_manufacturing_fab",
                "material_input_ports": 10,
                "input_ports_with_data": 11,
                "output_ports": 4,
                "physical_layout_rows": 7,
                "right_side_input_ports": 3,
                "top_side_input_ports": 1,
            },
        )
        runtime = (
            ROOT
            / "mods"
            / "RecursiveIndustry"
            / "src"
            / "UniversalIndustryData.cs"
        ).read_text(encoding="utf-8")
        self.assertIn("BuildTopInputRow", runtime)
        self.assertIn('port.name + port.kind + "v"', runtime)
        self.assertIn('" top_side_inputs=" + ports.TopSideInputPorts', runtime)

    def test_private_evidence_path_is_forbidden(self) -> None:
        reason = forbidden_path_reason(
            Path("data/verification_evidence/session.json")
        )
        self.assertIsNotNone(reason)

    def test_validation_mod_is_forbidden(self) -> None:
        reason = forbidden_path_reason(Path("mods/ValidationMod/proof.cs"))
        self.assertIsNotNone(reason)

    def test_player_zip_is_not_source(self) -> None:
        reason = forbidden_path_reason(Path("dist/RecursiveIndustry-1.0.0.zip"))
        self.assertIsNotNone(reason)

    def test_normal_mod_source_is_allowed(self) -> None:
        reason = forbidden_path_reason(
            Path("mods/RecursiveIndustry/src/RecursiveIndustry.cs")
        )
        self.assertIsNone(reason)

    def test_orbital_relay_is_completely_removed(self) -> None:
        source = ROOT / "mods" / "RecursiveIndustry" / "src"
        research = (source / "RecursiveIndustryResearchData.cs").read_text(
            encoding="utf-8"
        )
        orbital = (source / "OrbitalPowerArrayData.cs").read_text(
            encoding="utf-8"
        )
        ids = (source / "RecursiveIndustryIds.Power.cs").read_text(encoding="utf-8")
        config = json.loads(
            (ROOT / "mods" / "RecursiveIndustry" / "config.json").read_text(
                encoding="utf-8"
            )
        )
        self.assertNotIn("RecursiveIndustryIds.Power.OrbitalPowerRelay", research)
        self.assertIn("RecursiveIndustryIds.Power.OrbitalPowerArray", research)
        self.assertNotIn("OrbitalPowerRelay", orbital)
        self.assertNotIn("OrbitalPowerRelay", ids)
        self.assertFalse((source / "OrbitalPowerRelayData.cs").exists())
        self.assertFalse((source / "OrbitalPowerRelayLayout.cs").exists())
        self.assertIn("RecursiveIndustryIds.Power.OrbitalPowerArray", orbital)
        self.assertIn("orbital_power_array_seconds", config)
        self.assertNotIn("orbital_power_relay_seconds", config)


if __name__ == "__main__":
    unittest.main()
