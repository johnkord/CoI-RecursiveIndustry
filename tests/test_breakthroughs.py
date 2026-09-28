from fractions import Fraction
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))
from generate_recursive_industry_universal_source import load_catalog
from generate_research_tree import load as load_research
from model_reconstruction_bridge import parse_machine, parse_recipe

SOURCE = ROOT / "mods/RecursiveIndustry/src"


class BreakthroughTests(unittest.TestCase):
    def test_selected_power_classes_and_conserved_totals(self):
        facilities = {row["key"]: row for row in load_catalog()["facilities"]}
        expected = {"precision_metals_works": (35500,96,4,28), "alloy_glass_works": (4500,32,0,13),
                    "water_utility": (2500,64,4,28), "process_water_chiller": (4500,32,0,6)}
        for key, values in expected.items():
            self.assertEqual(tuple(facilities[key][field] for field in ("power_kw","selected_computing","workers","maintenance_per_month")), values)
        self.assertEqual(sum(row["selected_computing"] for row in facilities.values()),3392)
        self.assertEqual(sum(row["selected_packages"] for row in facilities.values()),1024)
        self.assertEqual(len(facilities),27)
        bindings = [binding["recipe_id"] for row in facilities.values() for binding in row["direct_bindings"]]
        self.assertEqual(len(bindings),231)
        self.assertEqual(len(set(bindings)),231)

    def test_power_classes_have_exact_native_sources(self):
        facilities = {row["key"]:row for row in load_catalog()["facilities"]}
        expected = {"precision_metals_works":{"AluminumCell"}, "alloy_glass_works":{"AlloyMixer","CopperElectrolysis","GlassMakerT2","SiliconCrystallizer"},
                    "water_utility":{"WaterTreatmentPlant"}, "process_water_chiller":{"WaterChiller"}}
        for key, sources in expected.items():
            self.assertEqual({row["source_machine_id"] for row in facilities[key]["direct_bindings"]},sources)

    def test_economy_is_exact_source_owned_and_local(self):
        data = load_catalog()
        self.assertEqual({row["source_recipe_id"] for row in data["economy_recipes"]},{"WaterTreatment","WaterTreatmentT2"})
        self.assertTrue(all(row["machine"] == "water_utility" and row["power_multiplier_percent"] == 30 for row in data["economy_recipes"]))
        source = (SOURCE / "UniversalIndustryData.cs").read_text(encoding="utf-8")
        block = source.split("private static ResolvedCustomRecipe ResolveEconomyRecipe(",1)[1].split("private static Duration ResolveEffectiveDuration(",1)[0]
        self.assertIn("source.SourceBinding.Multiplier * 4",block)
        self.assertIn("source.SourceBinding.Duration * 2",block)
        self.assertIn("output.TriggerAtStart",block)
        self.assertNotIn("IndustrialControlStream",block)
        self.assertNotIn("ValidatedControlPackage",block)

    def test_water_modes_have_the_selected_absolute_power(self):
        data = load_catalog()
        water = next(row for row in data["facilities"] if row["key"] == "water_utility")
        integrated = next(row for row in data["integrated_recipes"] if row["key"] == "integrated_water_recovery")
        self.assertEqual(water["power_kw"] * Fraction(integrated["power_multiplier_percent"],100),4500)
        self.assertEqual(water["power_kw"] * 2,5000)
        self.assertEqual((water["source_power_percent"],water["power_rounding_kw"]),(100,250))

    def test_repair_vector_and_ports_are_exact(self):
        source = (SOURCE / "ElectronicsReclaimerData.cs").read_text(encoding="utf-8")
        recipe = parse_recipe(source,"reclaimer","RemanufactureAcceleratorModules",{})
        self.assertEqual(recipe.inputs,{"SpentAccelerator":4,"Microchips":2,"Electronics3":2,"TitaniumAlloy":2,"ValidatedControlPackage":1})
        self.assertEqual(recipe.outputs,{"AcceleratorModule":4,"Waste":2})
        self.assertEqual((recipe.duration,recipe.power_percent),(240,200))
        self.assertIn('"D#>[4][4][4][4][4][4]   "',source)
        self.assertIn('"E#>[5][5][4][4][4][4]   "',source)
        self.assertNotIn("UseAllRecipesAtStartOrAfterUnlock",source)

    def test_local_controller_uses_selected_capital_and_yield(self):
        source = (SOURCE / "IndustrialControlGatewayData.cs").read_text(encoding="utf-8")
        local = source.split("var local =",1)[1]
        machine = parse_machine(local)
        recipe = parse_recipe(local,"local","DeployLocalIndustrialControl",{})
        self.assertEqual((machine.computing,machine.power_kw,machine.workers,machine.maintenance_t3),(32,250,2,2))
        self.assertEqual(machine.capital,{"ConstructionParts4":160,"Electronics4":32,"ValidatedControlPackage":8,"FrontierProgram":1,"ValidatedResearchDossier":1})
        self.assertEqual(recipe.inputs,{"ValidatedControlPackage":1})
        self.assertEqual(recipe.outputs,{"IndustrialControlStream":105})
        self.assertEqual(recipe.duration,60)

    def test_new_unlocks_are_in_the_selected_branches(self):
        nodes = {row["key"]:row for row in load_research()["nodes"]}
        self.assertEqual(len(nodes),51)
        self.assertIn("RemanufactureAcceleratorModules",nodes["validatedOperations"]["unlock"]["recipes"])
        self.assertIn("LocalDeploymentController",nodes["industrialControl"]["unlock"]["machines"])
        self.assertEqual(nodes["efficientWaterProcessing"]["parents"],["utilities"])
        self.assertNotIn("lifetime",nodes["efficientWaterProcessing"])
        self.assertNotIn("space",nodes["efficientWaterProcessing"])
        self.assertEqual(sum(row["lane"] == "rail" for row in nodes.values()),12)

    def test_codesign_cost_refresh_and_cap_are_safe(self):
        node = next(row for row in load_research()["nodes"] if row["key"] == "algorithmicCoDesign")
        self.assertEqual(node["cost_months"],240)
        self.assertEqual(node["repeatable"],{"levels":2,"bonus_percent":100,"cost_multipliers":[1,3]})
        source = (SOURCE / "ReleaseResearchTree.g.cs").read_text(encoding="utf-8")
        self.assertIn("return checked(baseCost * (level == 0 ? 1 : 3));",source)
        self.assertNotIn("SetSpacePointRequiredFrom",source)

    def test_model_inventory_matches_selected_sizes(self):
        data = json.loads((ROOT / "data/building-models.json").read_text(encoding="utf-8"))
        models = {row["key"]:row for row in data["models"]}
        self.assertEqual(len(models) + len(data["retained_reconstruction_models"]),55)
        for key, dimensions in {"control_deployment_gateway":(8,8),"local_deployment_controller":(6,6),
                                "precision_metals_works":(16,12),"refinery_complex":(16,16),
                                "nuclear_fuel_complex":(16,16),"orbital_power_array":(20,16),"process_water_chiller":(8,8)}.items():
            self.assertEqual((models[key]["width"],models[key]["depth"]),dimensions)


if __name__ == "__main__":
    unittest.main()