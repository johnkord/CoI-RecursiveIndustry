using Mafi;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Research;

namespace RecursiveIndustry;

internal static class UniversalIndustryResearchData
{
    public static void Register(
        ProtoRegistrator registrator,
        ResearchNodeProto recursiveEpochII,
        ResearchNodeProto recursiveEpochIII,
        ResearchNodeProto recursiveEpochIV)
    {
        ResearchNodeProto industrialControl = registrator.ResearchNodeProtoBuilder
            .Start(
                "Industrial Control Networks",
                RecursiveIndustryIds.Research.IndustrialControlNetworks,
                costMonths: 360)
            .Description(
                "Connects Epoch II electronics and capital production to live Industrial Control Stream. Both Fiber tiers are available for planned growth; Integrated rows still require their owning machine and physical inputs. Direct and Precision production remain independent of Fiber.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.IndustrialControlStream)
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.ControlDeploymentGateway,
                unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.DeployIndustrialControl)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.IntegrateElectronics2Direct)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.IntegrateConstructionParts3)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.IntegrateVehicleParts2)
            .AddProtoToUnlock<TransportProto>(RecursiveIndustryIds.Infrastructure.AccessFiber)
            .AddProtoToUnlock<TransportProto>(RecursiveIndustryIds.Infrastructure.BackboneFiber)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Infrastructure.FiberJunction)
            .BuildAndAdd();
        industrialControl.GridPosition = new Vector2i(212, 24);
        industrialControl.AddParent(recursiveEpochII);

        ResearchNodeProto federatedDeployment = registrator.ResearchNodeProtoBuilder
            .Start(
                "Federated Deployment",
                RecursiveIndustryIds.Research.FederatedDeployment,
                costMonths: 480)
            .Description(
                "Consolidates sustained Package and control demand after Industrial Control Networks. The Campus preserves validation material ratios in a long batch; dense Gateway deployment supplies up to seven full-rate compositions over Backbone Fiber. Standard validators and local deployment remain smaller investments.")
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.DeploymentAssuranceCampus,
                unlockAllRecipes: true)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl)
            .BuildAndAdd();
        federatedDeployment.GridPosition = new Vector2i(212, 30);
        federatedDeployment.AddParent(industrialControl);

        ResearchNodeProto materials = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Materials Systems",
                RecursiveIndustryIds.Research.AutonomousMaterialsSystems,
                costMonths: 720)
            .Description("Rebuilds bulk materials, smelting, casting, and glass after Epoch III and the source equipment technologies. Direct and Precision production need no Fiber; Integrated rows exchange intermediate handoffs for live control.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ComminutionHub, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MineralProductsWorks, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrimarySmelter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FuelSmelter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrecisionMetalsWorks, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CastingFinishingWorks, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedConcrete)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedSteel)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionCement)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionSteel)
            .BuildAndAdd();
        materials.GridPosition = new Vector2i(216, 22);
        materials.AddParent(recursiveEpochIII);
        ReconstructionResearchPrerequisites.AddSources(registrator, materials,
            RecursiveIndustryIds.Machines.ComminutionHub, RecursiveIndustryIds.Machines.MineralProductsWorks,
            RecursiveIndustryIds.Machines.PrimarySmelter, RecursiveIndustryIds.Machines.FuelSmelter,
            RecursiveIndustryIds.Machines.PrecisionMetalsWorks, RecursiveIndustryIds.Machines.CastingFinishingWorks);

        ResearchNodeProto process = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Process Systems",
                RecursiveIndustryIds.Research.AutonomousProcessSystems,
                costMonths: 720)
            .Description("Rebuilds refinery and chemical districts after Epoch III and their source technologies. Keep staged products where they serve several industries, or commit live control to a directed refinery slate. Native materials and residuals remain.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.RefineryComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.GasFertilizerComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MaterialsChemistryComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MedicalChemistryComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefinery)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryDiesel)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryGas)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryHydrogen)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryPlastic)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryRubber)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFertilizer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFertilizer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionMedicalSupplies)
            .BuildAndAdd();
        process.GridPosition = new Vector2i(216, 26);
        process.AddParent(recursiveEpochIII);
        ReconstructionResearchPrerequisites.AddSources(registrator, process,
            RecursiveIndustryIds.Machines.RefineryComplex, RecursiveIndustryIds.Machines.GasFertilizerComplex,
            RecursiveIndustryIds.Machines.MaterialsChemistryComplex, RecursiveIndustryIds.Machines.MedicalChemistryComplex);

        ResearchNodeProto essential = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Essential Systems",
                RecursiveIndustryIds.Research.AutonomousEssentialSystems,
                costMonths: 600)
            .Description("Unlocks accountable food, soil, bioenergy, water, emissions, and material-recovery facilities. Biological production remains on native farm families; special Maintenance Depots remain native.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FoodProcessingCampus, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FoodPackCampus, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CropSoilBioprocessing, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.BioenergyCenter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.WaterUtility, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ThermalDesalinationWorks, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ThermalEmissionsUtility, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MaterialsRecoveryCenter, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackEggs)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackTofu)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedWaterRecovery)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackEggs)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackTofu)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionWaterRecovery)
            .BuildAndAdd();
        essential.GridPosition = new Vector2i(220, 22);
        essential.AddParent(recursiveEpochIII);
        ReconstructionResearchPrerequisites.AddSources(registrator, essential,
            RecursiveIndustryIds.Machines.FoodProcessingCampus, RecursiveIndustryIds.Machines.FoodPackCampus,
            RecursiveIndustryIds.Machines.CropSoilBioprocessing, RecursiveIndustryIds.Machines.BioenergyCenter,
            RecursiveIndustryIds.Machines.WaterUtility, RecursiveIndustryIds.Machines.ThermalDesalinationWorks,
            RecursiveIndustryIds.Machines.ThermalEmissionsUtility, RecursiveIndustryIds.Machines.MaterialsRecoveryCenter);

        ResearchNodeProto nuclear = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Nuclear Operations",
                RecursiveIndustryIds.Research.AutonomousNuclearOperations,
                costMonths: 840)
            .Description("Unlocks an accountable high-power front-end and reprocessing complex. Reactor physics, interlocks, power levels, and radioactive waste remain native.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearFuelComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearReprocessingCenter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearFuelFabricationCell, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedUraniumRods)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionUraniumRods)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.OrbitalPowerCalibration,
                1)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        nuclear.GridPosition = new Vector2i(220, 26);
        nuclear.AddParent(recursiveEpochIV);
        ReconstructionResearchPrerequisites.AddSources(registrator, nuclear,
            RecursiveIndustryIds.Machines.NuclearFuelComplex, RecursiveIndustryIds.Machines.NuclearReprocessingCenter,
            RecursiveIndustryIds.Machines.NuclearFuelFabricationCell);

        ResearchNodeProto advanced = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Advanced Manufacturing",
                RecursiveIndustryIds.Research.AutonomousAdvancedManufacturing,
                costMonths: 720)
            .Description("Rebuilds terrestrial precision materials, robotic components, and general manufacturing after Epoch III. Electronics and Lab compositions need live control; Direct and Precision retain local control. Orbital fabrication belongs to Epoch IV.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrecisionComponentsFab, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.RoboticComponentsFab, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.GeneralManufacturingFab, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedElectronics3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedElectronics4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedLabEquipment2)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedLabEquipment3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedLabEquipment4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedMechanicalParts)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionElectronics4)
            .BuildAndAdd();
        advanced.GridPosition = new Vector2i(224, 24);
        advanced.AddParent(recursiveEpochIII);
        ReconstructionResearchPrerequisites.AddSources(registrator, advanced,
            RecursiveIndustryIds.Machines.PrecisionComponentsFab, RecursiveIndustryIds.Machines.RoboticComponentsFab,
            RecursiveIndustryIds.Machines.GeneralManufacturingFab);
        ReconstructionResearchPrerequisites.AddSources(registrator, recursiveEpochIV,
            RecursiveIndustryIds.Machines.OrbitalFabricationFab);

        ResearchNodeProto adaptiveAgrifood = registrator.ResearchNodeProtoBuilder
            .Start(
                "Adaptive Agrifood Systems",
                RecursiveIndustryIds.Research.AdaptiveAgrifoodSystems,
                costMonths: 480)
            .Description("Adds sensor-guided irrigation and labor-compressed native farm families while preserving crop schedules, weather, fertility, fertilizer, biological growth, animal care, and co-products.")
            .AddLayoutEntityToUnlock(
                RecursiveIndustryIds.Farms.SensorGuidedGreenhouse)
            .AddLayoutEntityToUnlock(
                RecursiveIndustryIds.Farms.MonitoredPoultryFarm)
            .AddFocusToUnlock(
                registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(
                    RecursiveIndustryIds.Focuses.PrecisionIrrigation))
            .BuildAndAdd();
        adaptiveAgrifood.GridPosition = new Vector2i(224, 18);
        adaptiveAgrifood.AddParent(essential);

        ResearchNodeProto circularAgrifood = registrator.ResearchNodeProtoBuilder
            .Start(
                "Circular Agrifood Systems",
                RecursiveIndustryIds.Research.CircularAgrifoodSystems,
                costMonths: 480)
            .Description("Converts unavoidable milling and oilseed coproducts into synthetic Eggs, Meat, Meat Trimmings, and optional staffed companion-animal care. Exact residuals remain physical, and ordinary poultry, composting, and energy recovery remain available.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.CompanionProvisions)
            .AddLayoutEntityToUnlock(
                RecursiveIndustryIds.Settlements.CompanionAnimalCenter)
            .AddProtoToUnlock<PopNeedProto>(
                RecursiveIndustryIds.Settlements.CompanionCareNeed)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.AdaptiveEggFermentation)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.SerumFreeCulturedMeat)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.MycoproteinTrimmings)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.CompanionProvisions)
            .BuildAndAdd();
        circularAgrifood.GridPosition = new Vector2i(228, 18);
        circularAgrifood.AddParent(adaptiveAgrifood);
        CivicKnowledgeData.RegisterResearch(registrator, industrialControl);
        Log.Info("RecursiveIndustry: RECONSTRUCTION_REGISTRATION_COMPLETE version=0.27.0a terrestrial_portfolios=4 civic_services=1");
    }
}