using Mafi;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Research;

namespace RecursiveIndustry;

internal static class UniversalIndustryResearchData
{
    public static void Register(
        ProtoRegistrator registrator,
        ResearchNodeProto recursiveEpochV)
    {
        ResearchNodeProto industrialControl = registrator.ResearchNodeProtoBuilder
            .Start(
                "Industrial Control Networks",
                RecursiveIndustryIds.Research.IndustrialControlNetworks,
                costMonths: 360)
            .Description(
                "Establishes a Data-only Fiber control plane for cross-stage Integrated production. Direct and Precision recipes retain local control and run without Fiber; Integrated compositions require continuous Industrial Control Stream from a Control Deployment Gateway.")
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
            .SetRequireSpacePoints()
            .BuildAndAdd();
        industrialControl.GridPosition = new Vector2i(212, 24);
        industrialControl.AddParent(recursiveEpochV);

        ResearchNodeProto federatedDeployment = registrator.ResearchNodeProtoBuilder
            .Start(
                "Federated Deployment",
                RecursiveIndustryIds.Research.FederatedDeployment,
                costMonths: 480)
            .Description(
                "Scales validated deployment without weakening assurance. A long-batch Campus compresses four standard validation lines, while Backbone deployment preserves 210 Stream per Package and trades power for Gateway density.")
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.DeploymentAssuranceCampus,
                unlockAllRecipes: true)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        federatedDeployment.GridPosition = new Vector2i(212, 30);
        federatedDeployment.AddParent(industrialControl);

        ResearchNodeProto materials = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Materials Systems",
                RecursiveIndustryIds.Research.AutonomousMaterialsSystems,
                costMonths: 720)
            .Description("Unlocks high-power, lower-maintenance megafacilities for comminution, mineral products, primary smelting, precision metals, and glass. Direct rows preserve vanilla recipe identity; Integrated and Precision modes remain optional.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ComminutionHub)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MineralProductsWorks)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrimarySmelter)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrecisionMetalsWorks)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedConcrete)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedSteel)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionCement)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionSteel)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                64)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        materials.GridPosition = new Vector2i(216, 22);
        materials.AddParent(industrialControl);

        ResearchNodeProto process = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Process Systems",
                RecursiveIndustryIds.Research.AutonomousProcessSystems,
                costMonths: 720)
            .Description("Unlocks refinery, gas, fertilizer, materials chemistry, and medical-process megafacilities. Precision spends power to reduce feedstock; cyclic oil and Hydrogen conversions retain exact Direct ratios.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.RefineryComplex)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.GasFertilizerComplex)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MaterialsChemistryComplex)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MedicalChemistryComplex)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefinery)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryDiesel)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryGas)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryHydrogen)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryPlastic)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryRubber)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFertilizer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFertilizer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionMedicalSupplies)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                64)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        process.GridPosition = new Vector2i(216, 26);
        process.AddParent(industrialControl);

        ResearchNodeProto essential = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Essential Systems",
                RecursiveIndustryIds.Research.AutonomousEssentialSystems,
                costMonths: 600)
            .Description("Unlocks accountable food, soil, bioenergy, water, emissions, and material-recovery facilities. Farms and special Maintenance Depots remain native.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FoodProcessingCampus)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FoodPackCampus)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CropSoilBioprocessing)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.BioenergyCenter)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.WaterUtility)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ThermalEmissionsUtility)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MaterialsRecoveryCenter)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackEggs)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackTofu)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedCrewSupplies)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedWaterRecovery)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackEggs)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackTofu)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionWaterRecovery)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                48)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        essential.GridPosition = new Vector2i(220, 22);
        essential.AddParent(industrialControl);

        ResearchNodeProto nuclear = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Nuclear Operations",
                RecursiveIndustryIds.Research.AutonomousNuclearOperations,
                costMonths: 840)
            .Description("Unlocks an accountable high-power front-end and reprocessing complex. Reactor physics, interlocks, power levels, and radioactive waste remain native.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearFuelComplex)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedUraniumRods)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionUraniumRods)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                64)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.OrbitalPowerCalibration,
                1)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        nuclear.GridPosition = new Vector2i(220, 26);
        nuclear.AddParent(industrialControl);

        ResearchNodeProto advanced = registrator.ResearchNodeProtoBuilder
            .Start(
                "Autonomous Advanced Manufacturing",
                RecursiveIndustryIds.Research.AutonomousAdvancedManufacturing,
                costMonths: 720)
            .Description("Unlocks precision-component, general-manufacturing, and orbital fabs. New Stream compositions build Electronics III and Lab Equipment II through IV from their physical feedstocks without replacing the earlier Electronics Integration or Capital Fabrication owners.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrecisionComponentsFab)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.GeneralManufacturingFab)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.OrbitalFabricationFab)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedElectronics3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedElectronics4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedLabEquipment2)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedLabEquipment3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedLabEquipment4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedMechanicalParts)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionElectronics4)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                64)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        advanced.GridPosition = new Vector2i(224, 24);
        advanced.AddParent(industrialControl);
    }
}