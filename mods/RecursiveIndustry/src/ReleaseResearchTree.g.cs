using System;
using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Buildings.VehicleDepots;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Research;
using Mafi.Core.SpaceProgram;
using Mafi.Core.Trains;
using Mafi.Core.Vehicles.Excavators;
using Mafi.Core.Vehicles.TreeHarvesters;
using Mafi.Core.Vehicles.TreePlanters;
using Mafi.Core.Vehicles.Trucks;
using Mafi.TrainsDlc;

namespace RecursiveIndustry;

internal sealed class ReleaseResearchTree : IResearchNodesData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        ResearchNodeProto acceleratedComputing = registrator.ResearchNodeProtoBuilder
            .Start("Accelerated Computing", RecursiveIndustryIds.Research.AcceleratedComputing, costMonths: 144)
            .Description("Adds accelerator hardware and Rack I to existing Data Centers. Denser Computing is a physical investment in electronics, cooling, power, and maintenance; basic racks remain available.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.AcceleratorModule, addIconToNode: true)
            .AddProductToUnlock(RecursiveIndustryIds.Products.AcceleratorRackI, addIconToNode: true)
            .AddProductToUnlock(RecursiveIndustryIds.Products.SpentAccelerator, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AcceleratorWorks, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.AcceleratorModule)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.AcceleratorRackI)
            .AddProtoToUnlock<ServerRackProto>(RecursiveIndustryIds.ServerRacks.RackI)
            .BuildAndAdd();
        acceleratedComputing.GridPosition = new Vector2i(156, 18);
        acceleratedComputing.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.Datacenter));
        acceleratedComputing.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.TitaniumSmelting));

        ResearchNodeProto curatedDataAndModels = registrator.ResearchNodeProtoBuilder
            .Start("Curated Data and Models", RecursiveIndustryIds.Research.CuratedDataAndModels, costMonths: 168)
            .Description("Expert curation produces Dataset Archives. Hardware-intensive training uses Datasets, Lab Equipment IV, and accelerators to produce Models; spent hardware can be recovered locally.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.DatasetArchive, addIconToNode: true)
            .AddProductToUnlock(RecursiveIndustryIds.Products.ModelArchive, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CurationOffice, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ModelDevelopmentCenter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ElectronicsReclaimer, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.CurateDataset)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.HardwareIntensiveTraining)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FastAcceleratorRecovery)
            .BuildAndAdd();
        curatedDataAndModels.GridPosition = new Vector2i(162, 18);
        curatedDataAndModels.AddParent(acceleratedComputing);
        curatedDataAndModels.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.Offices));
        curatedDataAndModels.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.ResearchLab4));

        ResearchNodeProto validatedOperations = registrator.ResearchNodeProtoBuilder
            .Start("Validated Deployment", RecursiveIndustryIds.Research.ValidatedOperations, costMonths: 192)
            .Description("Validate Models into signed Control Packages. AI Operations I allocates Focus, and the AI Electronics Cell offers a local-control application. Packages are physical releases, not a universal per-batch tax.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.ValidatedControlPackage, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AIElectronicsCell, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ValidateControlPackages)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionElectronics3)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.OperationsI)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ModelArchive, 1)
            .BuildAndAdd();
        validatedOperations.GridPosition = new Vector2i(168, 18);
        validatedOperations.AddParent(curatedDataAndModels);
        validatedOperations.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.Electronics4));

        ResearchNodeProto agenticAcceleration = registrator.ResearchNodeProtoBuilder
            .Start("Agentic Hardware and Operations", RecursiveIndustryIds.Research.AgenticAcceleration, costMonths: 216)
            .Description("Optional Rack II and Office II improve density. Curation-intensive adaptation, urgent Electronics III, and precision recovery trade data, power, or time against hardware scarcity. Applications do not require this hardware tier.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.FrontierRackII, addIconToNode: true)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.UpgradeRackIToII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SalvageRackI)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.CurationIntensiveAdaptation)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ThroughputElectronics3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionAcceleratorRecovery)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.OperationsII)
            .AddProtoToUnlock<ServerRackProto>(RecursiveIndustryIds.ServerRacks.RackII)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ValidatedControlPackage, 8)
            .BuildAndAdd();
        agenticAcceleration.GridPosition = new Vector2i(180, 4);
        agenticAcceleration.AddParent(validatedOperations);

        ResearchNodeProto recursiveAcceleration = registrator.ResearchNodeProtoBuilder
            .Start("Recursive Hardware and Operations", RecursiveIndustryIds.Research.RecursiveAcceleration, costMonths: 288)
            .Description("Optional Rack III and Office III consolidate larger Computing and Focus installations. Earlier racks remain usable, upgradeable, and salvageable. Physical power, cooling, capital, and staff still matter.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.RecursiveRackIII, addIconToNode: true)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.UpgradeRackIIToIII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SalvageRackII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SalvageRackIII)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.OperationsIII)
            .AddProtoToUnlock<ServerRackProto>(RecursiveIndustryIds.ServerRacks.RackIII)
            .BuildAndAdd();
        recursiveAcceleration.GridPosition = new Vector2i(184, 4);
        recursiveAcceleration.AddParent(agenticAcceleration);

        ResearchNodeProto appliedScience = registrator.ResearchNodeProtoBuilder
            .Start("Applied AI Science", RecursiveIndustryIds.Research.AppliedScience, costMonths: 216)
            .Description("Generate Experiment Programs from curated data, Models, and Computing. Experiments are proposals until physically validated. This application needs validated deployment, not a compulsory Rack III upgrade.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.ExperimentProgram, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AIScienceInstitute, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.DevelopExperimentPrograms)
            .BuildAndAdd();
        appliedScience.GridPosition = new Vector2i(180, 10);
        appliedScience.AddParent(validatedOperations);

        ResearchNodeProto physicalValidation = registrator.ResearchNodeProtoBuilder
            .Start("Physical Validation", RecursiveIndustryIds.Research.PhysicalValidation, costMonths: 288)
            .Description("Run staffed physical trials with Experiment Programs, Lab Equipment IV, and Titanium. Validated Research Dossiers support real deployments, public knowledge, and later projects; computation alone does not supply evidence.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.ValidatedResearchDossier, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PilotScienceComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ValidatePhysicalExperiment)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ExperimentProgram, 4)
            .BuildAndAdd();
        physicalValidation.GridPosition = new Vector2i(184, 10);
        physicalValidation.AddParent(appliedScience);

        ResearchNodeProto systemsIntegration = registrator.ResearchNodeProtoBuilder
            .Start("Systems Integration", RecursiveIndustryIds.Research.SystemsIntegration, costMonths: 288)
            .Description("Combine Models, Packages, Dossiers, and Electronics IV into Frontier Programs. Programs commission industrial reconstruction and larger projects. Choose the application your island needs; freight and advanced Microchips are not compulsory prerequisites.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.FrontierProgram, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.SystemsIntegrationComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProduceFrontierProgram)
            .BuildAndAdd();
        systemsIntegration.GridPosition = new Vector2i(192, 10);
        systemsIntegration.AddParent(physicalValidation);

        ResearchNodeProto algorithmicCoDesign = registrator.ResearchNodeProtoBuilder
            .Start("Algorithmic Co-design", RecursiveIndustryIds.Research.AlgorithmicCoDesign, costMonths: 480)
            .DescriptionPerLevelWithBonus("Physically validated algorithm and hardware co-design improves research efficiency by {0}. An optional mastery investment with no new hardware, Focus, or content unlocks.", 4.Percent())
            .SetRepeatableProperties(10, IdsCore.PropertyIds.ResearchEfficiencyMultiplier, 4.Percent(), CoDesignCost)
            .SetSpacePointRequiredFrom(5)
            .BuildAndAdd();
        algorithmicCoDesign.GridPosition = new Vector2i(192, 4);
        algorithmicCoDesign.AddParent(physicalValidation);

        ResearchNodeProto recursiveEpochI = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Road Freight", RecursiveIndustryIds.Research.RecursiveEpochI, costMonths: 216)
            .Description("Deploy general autonomous Haulers and fleet/maintenance Focus through native logistics. Heavy freight, amphibious equipment, forestry, and trains are separate choices with their own native technologies.")
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousHauler)
            .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.FleetOptimization))
            .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.PredictiveMaintenance))
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ValidatedControlPackage, 8)
            .BuildAndAdd();
        recursiveEpochI.GridPosition = new Vector2i(180, 24);
        recursiveEpochI.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, recursiveEpochI, registrator.PrototypesDb.GetOrThrow<TruckProto>(Ids.Vehicles.TruckT2H), registrator.PrototypesDb.GetOrThrow<VehicleDepotProto>(Ids.Buildings.VehiclesDepotT3));

        ResearchNodeProto digitalInfrastructure = registrator.ResearchNodeProtoBuilder
            .Start("Fiber Infrastructure", RecursiveIndustryIds.Research.DigitalInfrastructure, costMonths: 144)
            .Description("Build dedicated Data-only Fiber networks with Access, Backbone, and junctions. Both widths are available for planned growth. Industrial control and Civic Knowledge are separate service applications.")
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Infrastructure.FiberJunction)
            .AddProtoToUnlock<TransportProto>(RecursiveIndustryIds.Infrastructure.AccessFiber)
            .AddProtoToUnlock<TransportProto>(RecursiveIndustryIds.Infrastructure.BackboneFiber)
            .BuildAndAdd();
        digitalInfrastructure.GridPosition = new Vector2i(180, 34);
        digitalInfrastructure.AddParent(validatedOperations);

        ResearchNodeProto industrialControl = registrator.ResearchNodeProtoBuilder
            .Start("Industrial Control Networks", RecursiveIndustryIds.Research.IndustrialControlNetworks, costMonths: 240)
            .Description("Deploy Packages as live Industrial Control Stream. Raw Electronics II and integrated capital rows need their owning machine, physical inputs, and live supply. Direct and Precision production remain independent of Fiber.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.IndustrialControlStream, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ControlDeploymentGateway, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.DeployIndustrialControl)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegrateElectronics2Direct)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegrateConstructionParts3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegrateVehicleParts2)
            .BuildAndAdd();
        industrialControl.GridPosition = new Vector2i(200, 34);
        industrialControl.AddParent(digitalInfrastructure);
        industrialControl.AddParent(systemsIntegration);

        ResearchNodeProto federatedDeployment = registrator.ResearchNodeProtoBuilder
            .Start("Federated Deployment", RecursiveIndustryIds.Research.FederatedDeployment, costMonths: 360)
            .Description("Consolidate sustained validation and control demand. The Assurance Campus preserves input ratios in larger batches; dense Gateway deployment serves a Backbone network. This unlock is not permission to build Backbone Fiber.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.DeploymentAssuranceCampus, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.BatchDeploymentAssurance)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl)
            .BuildAndAdd();
        federatedDeployment.GridPosition = new Vector2i(208, 34);
        federatedDeployment.AddParent(industrialControl);

        ResearchNodeProto civic = registrator.ResearchNodeProtoBuilder
            .Start("Civic Knowledge Systems", RecursiveIndustryIds.Research.CivicKnowledgeSystems, costMonths: 360)
            .Description("Supply learning, translation, and public-service access through a staffed Civic Model Center and Knowledge Commons. Uses its own Fiber service and needs no Industrial Control Gateway, Microchip consolidation, orbital program, or planetary megacenter.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.CivicKnowledgeStream, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CivicModelCenter, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProvideCivicKnowledge)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Settlements.KnowledgeCommons)
            .AddProtoToUnlock<PopNeedProto>(RecursiveIndustryIds.Settlements.CivicKnowledgeNeed)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ValidatedResearchDossier, 4)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ModelArchive, 16)
            .BuildAndAdd();
        civic.GridPosition = new Vector2i(192, 42);
        civic.AddParent(digitalInfrastructure);
        civic.AddParent(physicalValidation);
        civic.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.IspModule));

        ResearchNodeProto recursiveEpochII = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Microelectronics", RecursiveIndustryIds.Research.RecursiveEpochII, costMonths: 288)
            .Description("Consolidate the native twelve-stage Microchip process into one zero-worker cleanroom with 256 Computing and 8 MW. Installed control replaces recurring Packages. This is a specialist application, not a prerequisite for other industries.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AutonomousMicrochipComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegrateAutonomousMicrochips)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.FrontierProgram, 4)
            .BuildAndAdd();
        recursiveEpochII.GridPosition = new Vector2i(208, 4);
        recursiveEpochII.AddParent(systemsIntegration);
        recursiveEpochII.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.MicrochipProduction2));

        ResearchNodeProto autonomousElectronicsIntegration = registrator.ResearchNodeProtoBuilder
            .Start("Electronics Integration", RecursiveIndustryIds.Research.AutonomousElectronicsIntegration, costMonths: 300)
            .Description("Rebuild Electronics II supply with staged production or live-control integration. The staged row retains PCB and Electronics handoffs; the raw row has lower output per machine and needs Industrial Control. Compare equal output and retained suppliers.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AutonomousElectronicsIntegrationComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegrateElectronics2Intermediates)
            .BuildAndAdd();
        autonomousElectronicsIntegration.GridPosition = new Vector2i(208, 10);
        autonomousElectronicsIntegration.AddParent(systemsIntegration);
        autonomousElectronicsIntegration.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.RoboticAssembly));

        ResearchNodeProto autonomousCapitalFabrication = registrator.ResearchNodeProtoBuilder
            .Start("Capital Fabrication", RecursiveIndustryIds.Research.AutonomousCapitalFabrication, costMonths: 360)
            .Description("Choose staged lower construction/vehicle parts or integrated Construction Parts III and Vehicle Parts II with live control. Final tiers remain in the Construction Nexus. Shared suppliers are retained when other industries still need them.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AutonomousCapitalFabricationMatrix, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FabricateConstructionParts)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FabricateConstructionParts2)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FabricateConstructionParts3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FabricateVehicleParts)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FabricateVehicleParts2)
            .BuildAndAdd();
        autonomousCapitalFabrication.GridPosition = new Vector2i(208, 50);
        autonomousCapitalFabrication.AddParent(systemsIntegration);
        autonomousCapitalFabrication.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.RoboticAssembly));

        ResearchNodeProto materials = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Mineral Processing", RecursiveIndustryIds.Research.AutonomousMaterialsSystems, costMonths: 300)
            .Description("Rebuild crushing, milling, cement, concrete, and mineral-product supply. Only these source technologies are prerequisites. Direct and Precision modes remain local; Integrated Concrete requires live control.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ComminutionHub, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MineralProductsWorks, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedConcrete)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionCement)
            .BuildAndAdd();
        materials.GridPosition = new Vector2i(208, 18);
        materials.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, materials, RecursiveIndustryIds.Machines.ComminutionHub, RecursiveIndustryIds.Machines.MineralProductsWorks);

        ResearchNodeProto metallurgy = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Metallurgy and Glass", RecursiveIndustryIds.Research.AutonomousMetallurgy, costMonths: 420)
            .Description("Rebuild smelting, electrochemical metals, glass, casting, and finishing. Native source equipment and recipe knowledge remain required. Integrated Steel and Precision Steel are alternatives, not mandatory modes.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrimarySmelter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FuelSmelter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PrecisionMetalsWorks, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CastingFinishingWorks, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedSteel)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionSteel)
            .BuildAndAdd();
        metallurgy.GridPosition = new Vector2i(216, 18);
        metallurgy.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, metallurgy, RecursiveIndustryIds.Machines.PrimarySmelter, RecursiveIndustryIds.Machines.FuelSmelter, RecursiveIndustryIds.Machines.PrecisionMetalsWorks, RecursiveIndustryIds.Machines.CastingFinishingWorks);

        ResearchNodeProto process = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Refining and Fuels", RecursiveIndustryIds.Research.AutonomousProcessSystems, costMonths: 360)
            .Description("Rebuild the refinery around a flexible Direct portfolio or a selected live-control slate. Directed diesel, gas, hydrogen, polymer, and elastomer routes retain all final residuals. Medical chemistry is a separate specialization.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.RefineryComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefinery)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryDiesel)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryGas)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryHydrogen)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryPlastic)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedRefineryRubber)
            .BuildAndAdd();
        process.GridPosition = new Vector2i(208, 26);
        process.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, process, RecursiveIndustryIds.Machines.RefineryComplex);

        ResearchNodeProto chemistry = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Chemical Systems", RecursiveIndustryIds.Research.AutonomousChemistry, costMonths: 360)
            .Description("Rebuild industrial gases, fertilizer, materials chemistry, and medical supply. Source technologies remain required. Choose Integrated Fertilizer or material-saving Precision modes according to the actual bottleneck.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.GasFertilizerComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MaterialsChemistryComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MedicalChemistryComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFertilizer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFertilizer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionMedicalSupplies)
            .BuildAndAdd();
        chemistry.GridPosition = new Vector2i(216, 26);
        chemistry.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, chemistry, RecursiveIndustryIds.Machines.GasFertilizerComplex, RecursiveIndustryIds.Machines.MaterialsChemistryComplex, RecursiveIndustryIds.Machines.MedicalChemistryComplex);

        ResearchNodeProto essential = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Food and Bioprocessing", RecursiveIndustryIds.Research.AutonomousEssentialSystems, costMonths: 360)
            .Description("Rebuild food processing, Food Packs, crop/soil bioprocessing, and bioenergy. Native agriculture remains available. Water, emissions, and material recovery are a separate utility specialization.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FoodProcessingCampus, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FoodPackCampus, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CropSoilBioprocessing, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.BioenergyCenter, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackEggs)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedFoodPackTofu)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackEggs)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionFoodPackTofu)
            .BuildAndAdd();
        essential.GridPosition = new Vector2i(208, 42);
        essential.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, essential, RecursiveIndustryIds.Machines.FoodProcessingCampus, RecursiveIndustryIds.Machines.FoodPackCampus, RecursiveIndustryIds.Machines.CropSoilBioprocessing, RecursiveIndustryIds.Machines.BioenergyCenter);

        ResearchNodeProto utilities = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Water and Circular Utilities", RecursiveIndustryIds.Research.AutonomousUtilities, costMonths: 300)
            .Description("Rebuild water recovery, chilling, desalination, emissions treatment, and materials recovery. Keep all physical residuals and native maintenance depots. Food and farming research are not prerequisite branches.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.WaterUtility, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ThermalDesalinationWorks, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ThermalEmissionsUtility, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.MaterialsRecoveryCenter, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedWaterRecovery)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionWaterRecovery)
            .BuildAndAdd();
        utilities.GridPosition = new Vector2i(216, 42);
        utilities.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, utilities, RecursiveIndustryIds.Machines.WaterUtility, RecursiveIndustryIds.Machines.ThermalDesalinationWorks, RecursiveIndustryIds.Machines.ThermalEmissionsUtility, RecursiveIndustryIds.Machines.MaterialsRecoveryCenter);

        ResearchNodeProto advanced = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Advanced Manufacturing", RecursiveIndustryIds.Research.AutonomousAdvancedManufacturing, costMonths: 480)
            .Description("Rebuild precision materials, robotic components, and general manufacturing. Integrated electronics, laboratories, and mechanical parts consume live control; Direct and Precision remain local. Orbital fabrication is independent.")
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
        advanced.GridPosition = new Vector2i(216, 34);
        advanced.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, advanced, RecursiveIndustryIds.Machines.PrecisionComponentsFab, RecursiveIndustryIds.Machines.RoboticComponentsFab, RecursiveIndustryIds.Machines.GeneralManufacturingFab);

        ResearchNodeProto nuclear = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Nuclear Fuel Systems", RecursiveIndustryIds.Research.AutonomousNuclearOperations, costMonths: 480)
            .Description("Rebuild nuclear fuel preparation, reprocessing, and fabrication after the native nuclear technologies. No orbital Calibration or Space Research is required. Reactor physics, interlocks, power limits, and radioactive waste remain native.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearFuelComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearReprocessingCenter, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.NuclearFuelFabricationCell, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedUraniumRods)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionUraniumRods)
            .BuildAndAdd();
        nuclear.GridPosition = new Vector2i(224, 50);
        nuclear.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddSources(registrator, nuclear, RecursiveIndustryIds.Machines.NuclearFuelComplex, RecursiveIndustryIds.Machines.NuclearReprocessingCenter, RecursiveIndustryIds.Machines.NuclearFuelFabricationCell);

        ResearchNodeProto adaptiveAgrifood = registrator.ResearchNodeProtoBuilder
            .Start("Sensor-Guided Agriculture", RecursiveIndustryIds.Research.AdaptiveAgrifoodSystems, costMonths: 240)
            .Description("Upgrade Greenhouse II with embedded monitoring and retain native crops, weather, fertility, irrigation, fertilizer, and biological timing. Precision Irrigation trades Focus for bounded water savings. Food factories and poultry research are not prerequisites.")
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Farms.SensorGuidedGreenhouse)
            .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.PrecisionIrrigation))
            .BuildAndAdd();
        adaptiveAgrifood.GridPosition = new Vector2i(200, 48);
        adaptiveAgrifood.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, adaptiveAgrifood, registrator.PrototypesDb.GetOrThrow<FarmProto>(Ids.Buildings.FarmT4));

        ResearchNodeProto monitoredPoultry = registrator.ResearchNodeProtoBuilder
            .Start("Monitored Poultry Systems", RecursiveIndustryIds.Research.MonitoredPoultrySystems, costMonths: 192)
            .Description("Upgrade native poultry with embedded flock monitoring and fewer workers. Feed, water, growth, slaughter settings, Eggs, Chicken Carcass, and buffers remain native. No Greenhouse or synthetic-food prerequisite.")
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Farms.MonitoredPoultryFarm)
            .BuildAndAdd();
        monitoredPoultry.GridPosition = new Vector2i(200, 54);
        monitoredPoultry.AddParent(systemsIntegration);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, monitoredPoultry, registrator.PrototypesDb.GetOrThrow<AnimalFarmProto>(Ids.Buildings.ChickenFarm));

        ResearchNodeProto circularAgrifood = registrator.ResearchNodeProtoBuilder
            .Start("Circular Agrifood and Companion Care", RecursiveIndustryIds.Research.CircularAgrifoodSystems, costMonths: 360)
            .Description("Use milling and oilseed coproducts in formulated Eggs, cultured Meat, mycoprotein Trimmings, and Companion Provisions. Optional staffed Companion care provides supplied Unity. Monitoring upgrades are alternatives, not prerequisites.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.CompanionProvisions, addIconToNode: true)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.AdaptiveEggFermentation)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SerumFreeCulturedMeat)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.MycoproteinTrimmings)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.CompanionProvisions)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Settlements.CompanionAnimalCenter)
            .AddProtoToUnlock<PopNeedProto>(RecursiveIndustryIds.Settlements.CompanionCareNeed)
            .BuildAndAdd();
        circularAgrifood.GridPosition = new Vector2i(216, 56);
        circularAgrifood.AddParent(essential);

        ResearchNodeProto recursiveSystems = registrator.ResearchNodeProtoBuilder
            .Start("Recursive Systems Integration", RecursiveIndustryIds.Research.RecursiveSystemsIntegration, costMonths: 360)
            .Description("Invest in the high-capacity Recursive Integration Array while large Program demand is still ahead. Commission it with Programs rather than an endgame Expansion Project. Its greater density and material efficiency retain power, Computing, maintenance, and physical validation obligations.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.RecursiveIntegrationArray, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ScaleFrontierPrograms)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.FrontierProgram, 32)
            .BuildAndAdd();
        recursiveSystems.GridPosition = new Vector2i(208, 58);
        recursiveSystems.AddParent(systemsIntegration);
        recursiveSystems.AddParent(recursiveAcceleration);

        ResearchNodeProto recursiveEpochIII = registrator.ResearchNodeProtoBuilder
            .Start("Planetary Coordination", RecursiveIndustryIds.Research.RecursiveEpochIII, costMonths: 360)
            .Description("Consolidate twenty-five Office III equivalents into the 625,000-Focus Planetary Center, and expand extraction/contract coordination. This concentrates 1,024 Computing and recurring Package support. Other industrial and civic applications do not require this megacenter.")
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.PlanetaryCoordinationCenter)
            .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.PlanetaryExtraction))
            .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.ContractCoordination))
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.FrontierProgram, 16)
            .BuildAndAdd();
        recursiveEpochIII.GridPosition = new Vector2i(216, 6);
        recursiveEpochIII.AddParent(systemsIntegration);
        recursiveEpochIII.AddParent(recursiveAcceleration);
        recursiveEpochIII.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.CargoDepot3));

        ResearchNodeProto recursiveEpochIV = registrator.ResearchNodeProtoBuilder
            .Start("Orbital Industry", RecursiveIndustryIds.Research.RecursiveEpochIV, costMonths: 480)
            .Description("Support orbital science, fabrication, and crew provisions after native space capabilities. Calibration opens beamed power as one option. Planetary Focus expansion, terrestrial nuclear processing, and freight specialization are separate decisions.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.OrbitalPowerCalibration, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.OrbitalMissionComplex, unlockAllRecipes: false)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.OrbitalFabricationFab, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.IntegratedCrewSupplies)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.RunOrbitalScienceCampaign)
            .AddFocusToUnlock(registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(RecursiveIndustryIds.Focuses.OrbitalLiftCoordination))
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.FrontierProgram, 64)
            .AddRequirementForMinSpaceStationTier(SpaceStationProto.RESEARCH_TIER_FROM)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        recursiveEpochIV.GridPosition = new Vector2i(232, 6);
        recursiveEpochIV.AddParent(systemsIntegration);
        recursiveEpochIV.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.SpaceStationResearch));
        ReconstructionResearchPrerequisites.AddSources(registrator, recursiveEpochIV, RecursiveIndustryIds.Machines.OrbitalFabricationFab);

        ResearchNodeProto orbitalBreakthrough = registrator.ResearchNodeProtoBuilder
            .Start("Orbital Beamed Power", RecursiveIndustryIds.Research.OrbitalBreakthrough, costMonths: 216)
            .Description("Receive 240 MW through a Dossier-supported Orbital Power Array. Beamed power is an optional energy investment; terrestrial generation remains valid and this research is not required for Frontier Megaprojects.")
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Power.OrbitalPowerArray)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.OrbitalPowerCalibration, 1)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        orbitalBreakthrough.GridPosition = new Vector2i(240, 2);
        orbitalBreakthrough.AddParent(recursiveEpochIV);

        ResearchNodeProto recursiveEpochV = registrator.ResearchNodeProtoBuilder
            .Start("Frontier Megaprojects", RecursiveIndustryIds.Research.RecursiveEpochV, costMonths: 600)
            .Description("Combine sustained Program production with station hardware and construction capital into a Frontier Expansion Project. Use the earlier Integration Array when it serves remaining demand. No beamed-power research or island reset is required.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.FrontierExpansionProject, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.FrontierProjectComplex, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.AssembleFrontierExpansionProject)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.FrontierProgram, 256)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        recursiveEpochV.GridPosition = new Vector2i(240, 10);
        recursiveEpochV.AddParent(recursiveEpochIV);

        ResearchNodeProto recursiveProjectEfficiency = registrator.ResearchNodeProtoBuilder
            .Start("Frontier Construction Mandate", RecursiveIndustryIds.Research.RecursiveProjectEfficiency, costMonths: 216)
            .Description("Commission an Autonomous Construction Nexus with an Expansion Project. Choose Surge, Precision, or Recovery according to throughput, material, and recycling pressure. Program reinvestment is available earlier through Recursive Systems Integration.")
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AutonomousConstructionNexus, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProduceRecursiveConstructionParts4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProducePrecisionConstructionParts4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.RecoverConstructionParts4)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProduceRecursiveVehicleParts3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProducePrecisionVehicleParts3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.RecoverVehicleParts3)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.FrontierExpansionProject, 1)
            .SetRequireSpacePoints()
            .BuildAndAdd();
        recursiveProjectEfficiency.GridPosition = new Vector2i(248, 10);
        recursiveProjectEfficiency.AddParent(recursiveEpochV);

        ResearchNodeProto autonomousHeavyEquipment = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Heavy Freight and Mining", RecursiveIndustryIds.Research.AutonomousHeavyLogistics, costMonths: 240)
            .Description("Deploy heavy dump/tank Haulers and the Mega Excavator after their native industrial technology. Retain job control, fuel, capacity, and upkeep. No amphibious or forestry technology is required.")
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousDumpHauler)
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousTankHauler)
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousMegaExcavator)
            .BuildAndAdd();
        autonomousHeavyEquipment.GridPosition = new Vector2i(184, 48);
        autonomousHeavyEquipment.AddParent(recursiveEpochI);
        autonomousHeavyEquipment.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.VehicleAssembly3H));
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, autonomousHeavyEquipment, registrator.PrototypesDb.GetOrThrow<ExcavatorProto>(Ids.Vehicles.ExcavatorT3H));

        ResearchNodeProto amphibious = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Amphibious Equipment", RecursiveIndustryIds.Research.AutonomousAmphibiousSystems, costMonths: 216)
            .Description("Automate amphibious freight and excavation after the matching native technology. Preserve land/water travel, underwater mining, cargo roles, and upkeep without a forestry prerequisite.")
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousAmphibiousHauler)
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousAmphibiousExcavator)
            .BuildAndAdd();
        amphibious.GridPosition = new Vector2i(184, 54);
        amphibious.AddParent(recursiveEpochI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, amphibious, registrator.PrototypesDb.GetOrThrow<TruckProto>(Ids.Vehicles.TruckAmphibiousH), registrator.PrototypesDb.GetOrThrow<ExcavatorProto>(Ids.Vehicles.ExcavatorAmphibiousH));

        ResearchNodeProto forestry = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Forestry", RecursiveIndustryIds.Research.AutonomousForestry, costMonths: 144)
            .Description("Automate large-scale tree harvesting while retaining native designations, service trucks, and physical fuel and maintenance. Planting remains a separate native-technology branch.")
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousLargeTreeHarvester)
            .BuildAndAdd();
        forestry.GridPosition = new Vector2i(192, 48);
        forestry.AddParent(recursiveEpochI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, forestry, registrator.PrototypesDb.GetOrThrow<TreeHarvesterProto>(Ids.Vehicles.TreeHarvesterT2H));

        ResearchNodeProto planting = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Planting", RecursiveIndustryIds.Research.AutonomousPlanting, costMonths: 144)
            .Description("Automate tree planting while retaining native sapling supply and planting designations. This does not require amphibious mining or completion of every forestry specialization.")
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousTreePlanter)
            .BuildAndAdd();
        planting.GridPosition = new Vector2i(192, 54);
        planting.AddParent(recursiveEpochI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, planting, registrator.PrototypesDb.GetOrThrow<TreePlanterProto>(Ids.Vehicles.TreePlanterT1H));

        ResearchNodeProto dieselRailI = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Diesel Rail I", RecursiveIndustryIds.Research.AutonomousRailControl, costMonths: 144)
            .Description("Deploy the first autonomous diesel locomotive with native scheduling and zero-worker wagons. Other fuels and nuclear propulsion are not prerequisites.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousDieselLocomotiveI)
            .BuildAndAdd();
        dieselRailI.GridPosition = new Vector2i(192, 64);
        dieselRailI.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, dieselRailI, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(Ids.Trains.LocomotiveT1Diesel));

        ResearchNodeProto dieselRailII = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Diesel Rail II", RecursiveIndustryIds.Research.AutonomousDieselRailII, costMonths: 120)
            .Description("Scale the diesel fleet with its native heavy locomotive. Preserve power, fuel strategy, scheduling, and maintenance; no unrelated propulsion branch is required.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousDieselLocomotiveII)
            .BuildAndAdd();
        dieselRailII.GridPosition = new Vector2i(200, 64);
        dieselRailII.AddParent(dieselRailI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, dieselRailII, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(Ids.Trains.LocomotiveT2Diesel));

        ResearchNodeProto steamRailI = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Steam Rail I", RecursiveIndustryIds.Research.AutonomousSteamRailI, costMonths: 144)
            .Description("Deploy autonomous steam traction and its matching tender after native steam rail. Keep the native fuel and water strategy; diesel and nuclear research are optional alternatives.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveI)
            .AddProtoToUnlock<TenderWagonProto>(RecursiveIndustryIds.Trains.AutonomousSteamTenderI)
            .BuildAndAdd();
        steamRailI.GridPosition = new Vector2i(192, 70);
        steamRailI.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, steamRailI, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(Ids.Trains.LocomotiveT1Steam));

        ResearchNodeProto steamRailII = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Steam Rail II", RecursiveIndustryIds.Research.AutonomousSteamRailII, costMonths: 120)
            .Description("Scale steam traction with the native heavy locomotive and tender. This is a fuel-strategy choice, not a prerequisite for other rail families.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveII)
            .AddProtoToUnlock<TenderWagonProto>(RecursiveIndustryIds.Trains.AutonomousSteamTenderII)
            .BuildAndAdd();
        steamRailII.GridPosition = new Vector2i(200, 70);
        steamRailII.AddParent(steamRailI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, steamRailII, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(Ids.Trains.LocomotiveT2Steam));

        ResearchNodeProto electricRailI = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Electric Rail I", RecursiveIndustryIds.Research.AutonomousElectricRailI, costMonths: 144)
            .Description("Deploy autonomous electric traction on the native electrified-track network. Nuclear, hydrogen, and steam technology are not required.")
            .AddProtoToUnlock<ElectricLocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousElectricLocomotiveI)
            .BuildAndAdd();
        electricRailI.GridPosition = new Vector2i(192, 76);
        electricRailI.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, electricRailI, registrator.PrototypesDb.GetOrThrow<ElectricLocomotiveProto>(IdsTrainsDlc.LocomotiveT1Electric));

        ResearchNodeProto electricRailII = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Electric Rail II", RecursiveIndustryIds.Research.AutonomousElectricRailII, costMonths: 120)
            .Description("Scale electrified freight with the native heavy electric locomotive and its unchanged track-power needs.")
            .AddProtoToUnlock<ElectricLocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousElectricLocomotiveII)
            .BuildAndAdd();
        electricRailII.GridPosition = new Vector2i(200, 76);
        electricRailII.AddParent(electricRailI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, electricRailII, registrator.PrototypesDb.GetOrThrow<ElectricLocomotiveProto>(IdsTrainsDlc.LocomotiveT2Electric));

        ResearchNodeProto hydrogenRailI = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Hydrogen Rail I", RecursiveIndustryIds.Research.AutonomousHydrogenRailI, costMonths: 144)
            .Description("Deploy autonomous hydrogen traction after its own native source technology, retaining hydrogen production and refueling commitments.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousHydrogenLocomotiveI)
            .BuildAndAdd();
        hydrogenRailI.GridPosition = new Vector2i(208, 64);
        hydrogenRailI.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, hydrogenRailI, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(Ids.Trains.LocomotiveT1Hydrogen));

        ResearchNodeProto hydrogenRailII = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Hydrogen Rail II", RecursiveIndustryIds.Research.AutonomousHydrogenRailII, costMonths: 120)
            .Description("Scale hydrogen freight with the native heavy locomotive. Other propulsion families remain independent choices.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousHydrogenLocomotiveII)
            .BuildAndAdd();
        hydrogenRailII.GridPosition = new Vector2i(216, 64);
        hydrogenRailII.AddParent(hydrogenRailI);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, hydrogenRailII, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(Ids.Trains.LocomotiveT2Hydrogen));

        ResearchNodeProto firelessRail = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Fireless Rail", RecursiveIndustryIds.Research.AutonomousFirelessRail, costMonths: 168)
            .Description("Deploy autonomous fireless steam traction with the native high-pressure steam supply strategy. No diesel, electric, or nuclear detour is required.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousFirelessSteamLocomotive)
            .BuildAndAdd();
        firelessRail.GridPosition = new Vector2i(208, 70);
        firelessRail.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, firelessRail, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(IdsTrainsDlc.LocomotiveT1FirelessSteam));

        ResearchNodeProto turbineRail = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Turbine Rail", RecursiveIndustryIds.Research.AutonomousTurbineRail, costMonths: 216)
            .Description("Deploy autonomous turbine traction and its matching Fuel Gas tender after the native turbine technology. Preserve native fuel and consist behavior.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousTurbineLocomotive)
            .AddProtoToUnlock<TenderWagonProto>(RecursiveIndustryIds.Trains.AutonomousTurbineTender)
            .BuildAndAdd();
        turbineRail.GridPosition = new Vector2i(216, 70);
        turbineRail.AddParent(validatedOperations);
        ReconstructionResearchPrerequisites.AddNativeOwners(registrator, turbineRail, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(IdsTrainsDlc.LocomotiveT2Turbine));

        ResearchNodeProto nuclearRail = registrator.ResearchNodeProtoBuilder
            .Start("Autonomous Nuclear Rail", RecursiveIndustryIds.Research.AutonomousNuclearRail, costMonths: 240)
            .Description("Deploy the complete native nuclear cab, reactor, and condenser consist with autonomous control. Preserve nuclear fueling, safety, and train mechanics without requiring every alternative propulsion family.")
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCab)
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveReactor)
            .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCondenser)
            .BuildAndAdd();
        nuclearRail.GridPosition = new Vector2i(224, 70);
        nuclearRail.AddParent(validatedOperations);
        nuclearRail.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(IdsTrainsDlc.Research.NuclearLocomotive));

        if (registrator.PrototypesDb.TryGetProto<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousCaptainsLocomotive, out _))
        {
            ResearchNodeProto captainsRail = registrator.ResearchNodeProtoBuilder
                .Start("Autonomous Captain's Rail", RecursiveIndustryIds.Research.AutonomousCaptainsRail, costMonths: 96)
                .Description("Optional autonomous Captain's locomotive when the Supporter prototype is present. It is never a prerequisite for another application.")
                .AddProtoToUnlock<LocomotiveProto>(RecursiveIndustryIds.Trains.AutonomousCaptainsLocomotive)
                .BuildAndAdd();
            captainsRail.GridPosition = new Vector2i(208, 76);
            captainsRail.AddParent(validatedOperations);
            ReconstructionResearchPrerequisites.AddNativeOwners(registrator, captainsRail, registrator.PrototypesDb.GetOrThrow<LocomotiveProto>(IdsTrainsDlc.LocomotiveT1Captains));
        }

        Log.Info("RecursiveIndustry: RELEASE_RESEARCH_REGISTERED version=0.28.0a nodes_max=50 native_recipe_locks=true");
    }

    private static long CoDesignCost(long baseCost, int level)
    {
        Fix64 step = Fix64.One + level;
        Fix64 growth = Math.Pow(1.4, level).ToFix64();
        return (baseCost * (step + growth).HalfFast).ToLongRounded();
    }
}
