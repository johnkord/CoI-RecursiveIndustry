import collections
import copy
from pathlib import Path
import re
import unittest

from tools import generate_research_tree as research
from tools.generate_recursive_industry_universal_source import load_catalog

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "mods/RecursiveIndustry/src"


def declared_ids() -> dict[str, set[str]]:
    result = collections.defaultdict(set)
    for path in SOURCE.glob("RecursiveIndustryIds*.cs"):
        text = path.read_text(encoding="utf-8")
        for category, body in re.findall(r"public static partial class (\w+)\s*\{([^{}]*)\}", text):
            result[category].update(re.findall(r"public static readonly [\w.]+ (\w+)\s*=", body))
    return result


def ancestors(nodes: dict, key: str) -> set[str]:
    result = set()
    for parent in nodes[key]["parents"]:
        result.add(parent)
        result.update(ancestors(nodes, parent))
    return result


class ReleaseResearchTests(unittest.TestCase):
    def setUp(self):
        self.data = research.load()
        self.nodes = {node["key"]: node for node in self.data["nodes"]}

    def test_generated_native_registrations_match(self):
        self.assertEqual(research.render(self.data).encode("utf-8"), research.OUTPUT.read_bytes())
        self.assertEqual(research.render_ids(self.data).encode("utf-8"), research.IDS.read_bytes())
        self.assertIn(": IResearchNodesData", research.render(self.data))

    def test_every_authored_unlock_has_exactly_one_owner(self):
        expected = declared_ids()
        actual = collections.defaultdict(collections.Counter)
        groups = {"products": "Products", "recipes": "Recipes", "machines": "Machines", "vehicles": "Vehicles", "focuses": "Focuses"}
        for node in self.nodes.values():
            unlocked = node["unlock"]
            for key, category in groups.items():
                actual[category].update(unlocked.get(key, []))
            for identifier in unlocked.get("buildings", []):
                category, name = identifier.split(".")
                actual[category][name] += 1
            for _, identifier in unlocked.get("protos", []):
                root, category, name = identifier.split(".")
                self.assertEqual(root, "RecursiveIndustryIds")
                actual[category][name] += 1
        categories = set(groups.values()) | {"Offices", "Farms", "Settlements", "Power", "Trains", "ServerRacks"}
        for category in sorted(categories):
            with self.subTest(category=category):
                self.assertTrue(expected[category])
                self.assertEqual(set(actual[category]), expected[category])
                self.assertTrue(all(count == 1 for count in actual[category].values()))
        self.assertEqual(set(actual["Infrastructure"]), expected["Infrastructure"] - {"Data"})
        self.assertEqual({node["id"] for node in self.nodes.values()}, expected["Research"])

    def test_native_recipe_locks_are_not_overridden(self):
        source = research.render(self.data)
        machine_count = sum(len(node["unlock"].get("machines", [])) for node in self.nodes.values())
        self.assertEqual(source.count("unlockAllRecipes: false"), machine_count)
        self.assertNotIn("unlockAllRecipes: true", source)
        facilities = [facility for node in self.nodes.values() for facility in node.get("facilities", [])]
        self.assertEqual(len(facilities), len(set(facilities)))
        self.assertEqual(len(facilities), 25)

    def test_applications_do_not_require_density_or_other_applications(self):
        for key in ("appliedScience", "physicalValidation", "systemsIntegration", "materials", "metallurgy", "process", "chemistry", "essential", "utilities", "advanced", "nuclear", "civic", "recursiveEpochIV"):
            with self.subTest(key=key):
                self.assertTrue(ancestors(self.nodes, key).isdisjoint({"agenticAcceleration", "recursiveAcceleration", "recursiveEpochI", "recursiveEpochII", "recursiveEpochIII"}))
        self.assertNotIn("industrialControl", ancestors(self.nodes, "civic"))
        self.assertNotIn("adaptiveAgrifood", ancestors(self.nodes, "circularAgrifood"))
        self.assertNotIn("monitoredPoultry", ancestors(self.nodes, "adaptiveAgrifood"))

    def test_terrestrial_nuclear_and_frontier_power_choice(self):
        self.assertNotIn("space", self.nodes["nuclear"])
        self.assertNotIn("recursiveEpochIV", ancestors(self.nodes, "nuclear"))
        self.assertNotIn("orbitalBreakthrough", ancestors(self.nodes, "recursiveEpochV"))
        self.assertNotIn("recursiveEpochV", ancestors(self.nodes, "recursiveSystems"))
        self.assertEqual(self.nodes["recursiveSystems"]["lifetime"], [["FrontierProgram", 32]])

    def test_array_capital_is_available_before_expansion_projects(self):
        source = (SOURCE / "RecursiveFrontierData.cs").read_text(encoding="utf-8")
        array = source.split("var integrationArray =", 1)[1].split(".BuildAndAdd();", 1)[0]
        self.assertRegex(array, r"\.Product\(\s*32,\s*RecursiveIndustryIds.Products.FrontierProgram\)")
        self.assertNotIn("FrontierExpansionProject", array)
        nexus = source.split("var constructionNexus =", 1)[1].split(".BuildAndAdd();", 1)[0]
        self.assertRegex(nexus, r"\.Product\(\s*1,\s*RecursiveIndustryIds.Products.FrontierExpansionProject\)")

    def test_terrestrial_nuclear_capital_has_no_orbital_item(self):
        facilities = {facility["key"]: facility for facility in load_catalog()["facilities"]}
        for key in ("nuclear_fuel_complex", "nuclear_reprocessing_center", "nuclear_fuel_fabrication_cell"):
            self.assertEqual(facilities[key]["calibration"], 0, key)
        self.assertEqual(facilities["nuclear_fuel_complex"]["selected_computing"], 320)
        self.assertEqual(facilities["nuclear_fuel_complex"]["cp4"], 2000)

    def test_rail_families_and_native_tiers_are_isolated(self):
        for key in ("dieselRailI", "dieselRailII", "steamRailI", "steamRailII", "electricRailI", "electricRailII", "hydrogenRailI", "hydrogenRailII", "firelessRail", "turbineRail", "nuclearRail", "captainsRail"):
            with self.subTest(key=key):
                node = self.nodes[key]
                self.assertTrue(node.get("native_sources") or node.get("native_parents"))
                self.assertNotIn("nuclearRail", ancestors(self.nodes, key))
                self.assertNotIn("recursiveEpochIII", ancestors(self.nodes, key))
        optional = self.nodes["captainsRail"]
        self.assertEqual(optional["optional_proto"], ["LocomotiveProto", "RecursiveIndustryIds.Trains.AutonomousCaptainsLocomotive"])
        self.assertFalse(any("captainsRail" in node["parents"] for node in self.nodes.values()))

    def test_readable_positions_and_forward_edges(self):
        nodes = list(self.nodes.values())
        for index, node in enumerate(nodes):
            for other in nodes[index + 1:]:
                self.assertTrue(abs(node["position"][0] - other["position"][0]) >= 4 or abs(node["position"][1] - other["position"][1]) >= 4, (node["key"], other["key"]))
            for parent in node["parents"]:
                self.assertLess(self.nodes[parent]["position"][0], node["position"][0], (node["key"], parent))

    def test_cycle_mutation_is_rejected(self):
        nodes = copy.deepcopy(self.data["nodes"])
        nodes[0]["parents"] = [nodes[-1]["key"]]
        with self.assertRaisesRegex(ValueError, "cycle"):
            research.order(nodes)

    def test_removed_or_commented_parents_are_rejected(self):
        source = research.render(self.data)
        for key, node in self.nodes.items():
            for parent in node["parents"]:
                declaration = f"{key}.AddParent({parent});"
                for replacement in ("", "// " + declaration, "/* " + declaration + " */"):
                    with self.subTest(key=key, parent=parent, replacement=replacement):
                        self.assertTrue(research.audit_source(source.replace(declaration, replacement)))

    def test_control_rows_do_not_bypass_their_separate_unlock(self):
        for key, forbidden in (("autonomousElectronicsIntegration", {"IntegrateElectronics2Direct"}),
                               ("autonomousCapitalFabrication", {"IntegrateConstructionParts3", "IntegrateVehicleParts2"})):
            self.assertTrue(forbidden.isdisjoint(self.nodes[key]["unlock"]["recipes"]))
            self.assertTrue(forbidden <= set(self.nodes["industrialControl"]["unlock"]["recipes"]))
            self.assertIn("Ids.Research.RoboticAssembly", self.nodes[key]["native_parents"])
            self.assertNotIn("lifetime", self.nodes[key])
        self.assertEqual(self.nodes["federatedDeployment"]["parents"], ["industrialControl"])
        self.assertNotIn("lifetime", self.nodes["federatedDeployment"])
        self.assertNotIn("space", self.nodes["federatedDeployment"])
        self.assertEqual(self.nodes["digitalInfrastructure"]["unlock"]["protos"], [
            ["TransportProto", "RecursiveIndustryIds.Infrastructure.AccessFiber"],
            ["TransportProto", "RecursiveIndustryIds.Infrastructure.BackboneFiber"],
        ])

    def test_orbital_fabrication_does_not_gate_terrestrial_food(self):
        self.assertIn("OrbitalFabricationFab", self.nodes["recursiveEpochIV"]["facilities"])
        self.assertIn("IntegratedCrewSupplies", self.nodes["recursiveEpochIV"]["unlock"]["recipes"])
        self.assertNotIn("IntegratedCrewSupplies", self.nodes["essential"]["unlock"]["recipes"])
        helper = (SOURCE / "ReconstructionResearchPrerequisites.cs").read_text(encoding="utf-8")
        for token in ("registrator.PrototypesDb.All<ResearchNodeProto>()", "unit is IProtoUnlock",
                      "facility.DirectBindings", "recipe.Sources", "recipe.SourceRecipeId",
                      "unlockedRecipes.Contains(recipe.Id)", "target.AddParent(parent)"):
            self.assertIn(token, helper)


if __name__ == "__main__":
    unittest.main()