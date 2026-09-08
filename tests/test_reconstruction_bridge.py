#!/usr/bin/env python3
"""Source contracts for the Epoch II reconstruction bridge."""

from __future__ import annotations

from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from audit_recursive_industry_control_network import (  # noqa: E402
    audit_legacy_research_source,
    audit_research_source,
    research_direct_parents as direct_parents,
    research_registration as registration,
)

SOURCE = ROOT / "mods" / "RecursiveIndustry" / "src"
MAIN_RESEARCH = (SOURCE / "RecursiveIndustryResearchData.cs").read_text(encoding="utf-8")
UNIVERSAL_RESEARCH = (SOURCE / "UniversalIndustryResearchData.cs").read_text(encoding="utf-8")


class ReconstructionBridgeTests(unittest.TestCase):
    def test_normal_source_audits_include_the_bridge(self) -> None:
        self.assertEqual(audit_research_source(UNIVERSAL_RESEARCH), [])
        self.assertEqual(audit_legacy_research_source(MAIN_RESEARCH), [])

    def test_each_missing_or_commented_reconstruction_parent_is_rejected(self) -> None:
        for variable in ("materials", "process", "essential", "advanced", "nuclear"):
            parent = "recursiveEpochIV" if variable == "nuclear" else "recursiveEpochIII"
            declaration = f"{variable}.AddParent({parent});"
            for replacement in ("", "// " + declaration, "/* " + declaration + " */"):
                with self.subTest(variable=variable, replacement=replacement):
                    mutated = UNIVERSAL_RESEARCH.replace(declaration, replacement)
                    self.assertIn(
                        f"{variable} bridge parent contract drift",
                        audit_research_source(mutated),
                    )

    def test_wrong_epoch_argument_is_rejected(self) -> None:
        mutated = MAIN_RESEARCH.replace(
            "Register(registrator, recursiveEpochII, recursiveEpochIII, recursiveEpochIV)",
            "Register(registrator, recursiveEpochIV, recursiveEpochIII, recursiveEpochIV)",
        )
        self.assertTrue(audit_legacy_research_source(mutated))

    def test_control_uses_epoch_two_and_ordinary_research(self) -> None:
        self.assertEqual(
            direct_parents(UNIVERSAL_RESEARCH, "industrialControl"),
            {"recursiveEpochII"},
        )
        self.assertNotIn(
            ".SetRequireSpacePoints()",
            registration(UNIVERSAL_RESEARCH, "industrialControl"),
        )

    def test_federation_has_no_extra_epoch_or_space_gate(self) -> None:
        self.assertEqual(
            direct_parents(UNIVERSAL_RESEARCH, "federatedDeployment"),
            {"industrialControl"},
        )
        block = registration(UNIVERSAL_RESEARCH, "federatedDeployment")
        self.assertNotIn(".SetRequireSpacePoints()", block)
        self.assertNotIn(".AddRequirementForLifetimeProduction(", block)

    def test_both_integration_branches_use_epoch_two_without_extra_witness(self) -> None:
        for variable in (
            "autonomousElectronicsIntegration",
            "autonomousCapitalFabrication",
        ):
            with self.subTest(variable=variable):
                self.assertEqual(direct_parents(MAIN_RESEARCH, variable), {"recursiveEpochII"})
                block = registration(MAIN_RESEARCH, variable)
                self.assertNotIn(".AddRequirementForLifetimeProduction(", block)
                self.assertIn("unlockAllRecipes: false", block)

    def test_terrestrial_and_nuclear_portfolios_have_separate_gates(self) -> None:
        for variable in ("materials", "process", "essential", "nuclear", "advanced"):
            with self.subTest(variable=variable):
                expected = "recursiveEpochIV" if variable == "nuclear" else "recursiveEpochIII"
                self.assertEqual(
                    direct_parents(UNIVERSAL_RESEARCH, variable),
                    {expected},
                )
                self.assertEqual(
                    ".SetRequireSpacePoints()" in registration(UNIVERSAL_RESEARCH, variable),
                    variable == "nuclear",
                )

    def test_native_prerequisites_cover_all_facilities_and_composed_sources(self) -> None:
        helper = (SOURCE / "ReconstructionResearchPrerequisites.cs").read_text(encoding="utf-8")
        self.assertEqual(UNIVERSAL_RESEARCH.count("ReconstructionResearchPrerequisites.AddSources("), 6)
        for token in (
            "registrator.PrototypesDb.All<ResearchNodeProto>()", "unit is IProtoUnlock",
            "facility.DirectBindings", "recipe.Sources", "recipe.SourceRecipeId",
            "required.Add(registrator.PrototypesDb.GetOrThrow<MachineProto>",
            "sourceRecipes.Add(source.RecipeId)", "target.AddParent(parent)",
        ):
            self.assertIn(token, helper)
        self.assertNotIn("Machines.OrbitalFabricationFab", registration(UNIVERSAL_RESEARCH, "advanced"))
        self.assertIn("Machines.OrbitalFabricationFab", registration(MAIN_RESEARCH, "recursiveEpochIV"))
        self.assertNotIn("Recipes.IntegratedCrewSupplies", registration(UNIVERSAL_RESEARCH, "essential"))
        self.assertIn("Recipes.IntegratedCrewSupplies", registration(MAIN_RESEARCH, "recursiveEpochIV"))
        self.assertIn("unlockedRecipes.Contains(recipe.Id)", helper)

    def test_control_keeps_both_fiber_tiers_and_local_recipe(self) -> None:
        block = registration(UNIVERSAL_RESEARCH, "industrialControl")
        for symbol in ("AccessFiber", "BackboneFiber", "FiberJunction", "DeployIndustrialControl"):
            with self.subTest(symbol=symbol):
                self.assertIn(symbol, block)
        self.assertNotIn("DeployBackboneIndustrialControl", block)


if __name__ == "__main__":
    unittest.main()