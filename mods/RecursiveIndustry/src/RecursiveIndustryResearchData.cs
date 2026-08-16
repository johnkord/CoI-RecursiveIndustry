using System;
using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Mods;
using Mafi.Core.Research;
using Mafi.Core.SpaceProgram;
using Mafi.Core.Trains;
using Mafi.TrainsDlc;

namespace RecursiveIndustry;

internal sealed class RecursiveIndustryResearchData : IResearchNodesData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        ResearchNodeProto acceleratedComputing = registrator.ResearchNodeProtoBuilder
            .Start(
                "Accelerated Computing",
                RecursiveIndustryIds.Research.AcceleratedComputing,
                costMonths: 144)
            .Description("Adds vertical Computing to existing Data Centers. Rack I trades advanced electronics and accelerator capital for better slot and power efficiency; Basic racks remain the cheap horizontal path.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.AcceleratorModule,
                addIconToNode: true)
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.AcceleratorRackI,
                addIconToNode: true)
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.SpentAccelerator,
                addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AcceleratorWorks)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.AcceleratorModule)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.AcceleratorRackI)
            .AddProtoToUnlock<ServerRackProto>(RecursiveIndustryIds.ServerRacks.RackI)
            .BuildAndAdd();

        acceleratedComputing.GridPosition = new Vector2i(156, 18);
        acceleratedComputing.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.Datacenter));
        acceleratedComputing.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.TitaniumSmelting));

        ResearchNodeProto curatedDataAndModels = registrator.ResearchNodeProtoBuilder
            .Start(
                "Curated Data and Models",
                RecursiveIndustryIds.Research.CuratedDataAndModels,
                costMonths: 168)
            .Description("Turns expert labor and Office Supplies into Datasets, then develops reusable Models through hardware- or curation-intensive campaigns. Use hardware when Datasets are scarce, or curation when accelerator capital and time are scarce.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.DatasetArchive,
                addIconToNode: true)
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.ModelArchive,
                addIconToNode: true)
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.CurationOffice,
                unlockAllRecipes: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ModelDevelopmentCenter)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.HardwareIntensiveTraining)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.ElectronicsReclaimer)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.FastAcceleratorRecovery)
            .BuildAndAdd();

        curatedDataAndModels.GridPosition = new Vector2i(160, 18);
        curatedDataAndModels.AddParent(acceleratedComputing);
        curatedDataAndModels.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.Offices));

        ResearchNodeProto validatedOperations = registrator.ResearchNodeProtoBuilder
            .Start(
                "Validated Operations",
                RecursiveIndustryIds.Research.ValidatedOperations,
                costMonths: 192)
            .Description("Converts the first Model into renewable deployment Packages. AI Operations spends workers, Computing, and Packages on shared Focus, while precision Electronics III provides a physical first application; vanilla production remains the resilient fallback.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.ValidatedControlPackage,
                addIconToNode: true)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ValidateControlPackages)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.OperationsI)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AIElectronicsCell)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionElectronics3)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.ModelArchive,
                1)
            .BuildAndAdd();

        validatedOperations.GridPosition = new Vector2i(164, 18);
        validatedOperations.AddParent(curatedDataAndModels);

        ResearchNodeProto agenticAcceleration = registrator.ResearchNodeProtoBuilder
            .Start(
                "Agentic Acceleration",
                RecursiveIndustryIds.Research.AgenticAcceleration,
                costMonths: 216)
            .Description("Rack II and Office II expand one sector from assistance to agentic coordination. Curation-intensive adaptation, urgent Electronics, and precision recovery trade more data, power, or time for different bottlenecks; Spent Accelerators can later remain local or enter take-back, and prior routes remain valid.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.FrontierRackII,
                addIconToNode: true)
            .AddProtoToUnlock<ServerRackProto>(RecursiveIndustryIds.ServerRacks.RackII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.UpgradeRackIToII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SalvageRackI)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.OperationsII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.CurationIntensiveAdaptation)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ThroughputElectronics3)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.PrecisionAcceleratorRecovery)
            .AddProductIcon(RecursiveIndustryIds.Products.DatasetArchive)
            .AddProductIcon(Ids.Products.Electronics3)
            .AddProductIcon(Ids.Products.Microchips)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.ValidatedControlPackage,
                8)
            .BuildAndAdd();

        agenticAcceleration.GridPosition = new Vector2i(168, 20);
        agenticAcceleration.AddParent(validatedOperations);

        ResearchNodeProto recursiveAcceleration = registrator.ResearchNodeProtoBuilder
            .Start(
                "Recursive Acceleration",
                RecursiveIndustryIds.Research.RecursiveAcceleration,
                costMonths: 288)
            .Description("Rack III and Office III create broad but finite abundance with improved Computing density. Older racks remain deployable, upgradeable, or salvageable; Packages, power, cooling, and workers still limit expansion.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.RecursiveRackIII,
                addIconToNode: true)
            .AddProtoToUnlock<ServerRackProto>(RecursiveIndustryIds.ServerRacks.RackIII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.UpgradeRackIIToIII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SalvageRackII)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.SalvageRackIII)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Offices.OperationsIII)
            .BuildAndAdd();

        recursiveAcceleration.GridPosition = new Vector2i(172, 18);
        recursiveAcceleration.AddParent(agenticAcceleration);

        ResearchNodeProto appliedScience = registrator.ResearchNodeProtoBuilder
            .Start(
                "Applied Science",
                RecursiveIndustryIds.Research.AppliedScience,
                costMonths: 216)
            .Description("Uses mature Datasets, Models, and Computing to generate Experiment Programs. Programs are proposals, not proof; they become useful only through physical validation.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.ExperimentProgram,
                addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.AIScienceInstitute)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.DevelopExperimentPrograms)
            .BuildAndAdd();

        appliedScience.GridPosition = new Vector2i(180, 18);
        appliedScience.AddParent(recursiveAcceleration);
        appliedScience.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.ResearchLab4));

        ResearchNodeProto physicalValidation = registrator.ResearchNodeProtoBuilder
            .Start(
                "Physical Validation",
                RecursiveIndustryIds.Research.PhysicalValidation,
                costMonths: 288)
            .Description("Turns Experiment Programs, Lab Equipment IV, and Titanium into Dossiers through labor-intensive trials. Dossiers unlock evidence-dependent progress and can later remain internal or license Sulfur, while Pilot throughput becomes the new constraint.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.ValidatedResearchDossier,
                addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.PilotScienceComplex)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.ValidatePhysicalExperiment)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.ExperimentProgram,
                4)
            .BuildAndAdd();

        physicalValidation.GridPosition = new Vector2i(184, 18);
        physicalValidation.AddParent(appliedScience);

        ResearchNodeProto systemsIntegration = registrator.ResearchNodeProtoBuilder
            .Start(
                "Systems Integration",
                RecursiveIndustryIds.Research.SystemsIntegration,
                costMonths: 288)
            .Description("Combines Models, Packages, Dossiers, and Electronics IV into Frontier Programs. This recurring project chain proves the complete AI district is operating and becomes the substrate for every Recursive Epoch.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.FrontierProgram,
                addIconToNode: true)
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.SystemsIntegrationComplex)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.ProduceFrontierProgram)
            .BuildAndAdd();

        systemsIntegration.GridPosition = new Vector2i(188, 18);
        systemsIntegration.AddParent(physicalValidation);
        systemsIntegration.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.Electronics4));

        ResearchNodeProto recursiveEpochI = registrator.ResearchNodeProtoBuilder
            .Start(
                "Recursive Epoch I: Autonomous Networks",
                RecursiveIndustryIds.Research.RecursiveEpochI,
                costMonths: 216)
            .Description("Removes direct workers from selected freight roles while preserving native depots, fuel, maintenance, routing, and capacity. Use autonomous Haulers where labor is limiting; conventional vehicles remain lower-capital fallbacks.")
            .AddProductIcon(RecursiveIndustryIds.Products.FrontierProgram)
            .AddFocusToUnlock(
                registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(
                    RecursiveIndustryIds.Focuses.FleetOptimization))
            .AddFocusToUnlock(
                registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(
                    RecursiveIndustryIds.Focuses.PredictiveMaintenance))
            .AddVehicleToUnlock(RecursiveIndustryIds.Vehicles.AutonomousHauler)
            .AddVehicleToUnlock(
                RecursiveIndustryIds.Vehicles.AutonomousDumpHauler)
            .AddVehicleToUnlock(
                RecursiveIndustryIds.Vehicles.AutonomousTankHauler)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                1)
            .BuildAndAdd();

        recursiveEpochI.GridPosition = new Vector2i(192, 18);
        recursiveEpochI.AddParent(systemsIntegration);

        ResearchNodeProto recursiveEpochII = registrator.ResearchNodeProtoBuilder
            .Start(
                "Recursive Epoch II: Lights-Out Industry",
                RecursiveIndustryIds.Research.RecursiveEpochII,
                costMonths: 288)
            .Description("Consolidates a sustained twelve-stage Microchip line into one zero-worker complex. It wins on labor and footprint but concentrates raw inputs, 512 Computing, 8 MW, Packages, and maintenance; the conventional line remains safer at low demand.")
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.AutonomousMicrochipComplex)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.IntegrateAutonomousMicrochips)
            .AddProductIcon(Ids.Products.Microchips)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                4)
            .BuildAndAdd();

        recursiveEpochII.GridPosition = new Vector2i(196, 18);
        recursiveEpochII.AddParent(recursiveEpochI);
        recursiveEpochII.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.MicrochipProduction2));

        ResearchNodeProto autonomousElectronicsIntegration = registrator
            .ResearchNodeProtoBuilder
            .Start(
                "Autonomous Electronics Integration",
                RecursiveIndustryIds.Research.AutonomousElectronicsIntegration,
                costMonths: 360)
            .Description("Consolidates sustained Electronics II demand. Staged integration gives 4x throughput while retaining PCB and Electronics logistics; direct integration gives 2x with fewer handoffs. Assembly V remains preferable for intermittent demand.")
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines
                    .AutonomousElectronicsIntegrationComplex,
                unlockAllRecipes: true)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                8)
            .BuildAndAdd();

        autonomousElectronicsIntegration.GridPosition = new Vector2i(200, 26);
        autonomousElectronicsIntegration.AddParent(recursiveEpochII);
        autonomousElectronicsIntegration.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.RoboticAssembly));

        ResearchNodeProto recursiveEpochIII = registrator.ResearchNodeProtoBuilder
            .Start(
                "Recursive Epoch III: Planetary Coordination",
                RecursiveIndustryIds.Research.RecursiveEpochIII,
                costMonths: 360)
            .Description("Turns local deposits, world mines, two bounded contracts, cargo ships, and ports into one planetary supply portfolio. License Dossiers for Sulfur or return Spent Accelerators for Recyclables only when those imports beat local use. The 625,000-Focus Center ends large-scale Office replication but requires a four-validator Package district and concentrates failure.")
            .AddFocusToUnlock(
                registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(
                    RecursiveIndustryIds.Focuses.PlanetaryExtraction))
            .AddFocusToUnlock(
                registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(
                    RecursiveIndustryIds.Focuses.ContractCoordination))
            .AddLayoutEntityToUnlock(
                RecursiveIndustryIds.Offices.PlanetaryCoordinationCenter)
            .AddProductIcon(RecursiveIndustryIds.Products.FrontierProgram)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                16)
            .BuildAndAdd();

        recursiveEpochIII.GridPosition = new Vector2i(200, 18);
        recursiveEpochIII.AddParent(recursiveEpochII);
        recursiveEpochIII.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.CargoDepot3));

        ResearchNodeProto autonomousCapitalFabrication = registrator
            .ResearchNodeProtoBuilder
            .Start(
                "Autonomous Capital Fabrication",
                RecursiveIndustryIds.Research.AutonomousCapitalFabrication,
                costMonths: 480)
            .Description("Consolidates sustained lower construction and vehicle-part production. Staged rows give 4x throughput; integrated rows give 2x with fewer handoffs and higher power. Ordinary Assemblies remain the low-capital fallback, while final tiers stay in the Nexus.")
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.AutonomousCapitalFabricationMatrix,
                unlockAllRecipes: true)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                32)
            .BuildAndAdd();

        autonomousCapitalFabrication.GridPosition = new Vector2i(204, 26);
        autonomousCapitalFabrication.AddParent(recursiveEpochIII);
        autonomousCapitalFabrication.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.RoboticAssembly));

        ResearchNodeProtoBuilder.State heavyEquipmentBuilder =
            registrator.ResearchNodeProtoBuilder
                .Start(
                    "Autonomous Heavy Equipment",
                    RecursiveIndustryIds.Research.AutonomousHeavyLogistics,
                    costMonths: 408)
                .Description("Removes direct workers from amphibious freight, mining, mega excavation, forestry, and planting while preserving native jobs and upkeep. Build it when field labor, rather than fuel, maintenance, or vehicle capital, limits expansion.")
                .AddVehicleToUnlock(
                    RecursiveIndustryIds.Vehicles.AutonomousAmphibiousHauler)
                .AddVehicleToUnlock(
                    RecursiveIndustryIds.Vehicles.AutonomousAmphibiousExcavator)
                .AddVehicleToUnlock(
                    RecursiveIndustryIds.Vehicles.AutonomousMegaExcavator)
                .AddVehicleToUnlock(
                    RecursiveIndustryIds.Vehicles.AutonomousLargeTreeHarvester)
                .AddVehicleToUnlock(
                    RecursiveIndustryIds.Vehicles.AutonomousTreePlanter);

        ResearchNodeProto autonomousHeavyEquipment = heavyEquipmentBuilder
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                32)
            .BuildAndAdd();

        autonomousHeavyEquipment.GridPosition = new Vector2i(204, 22);
        autonomousHeavyEquipment.AddParent(recursiveEpochIII);
        autonomousHeavyEquipment.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.VehicleAssembly3H));
        autonomousHeavyEquipment.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.VehiclesAmphibiousH));
        autonomousHeavyEquipment.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.TreePlanting));

        ResearchNodeProtoBuilder.State railControlBuilder =
            registrator.ResearchNodeProtoBuilder
                .Start(
                    "Autonomous Rail Control",
                    RecursiveIndustryIds.Research.AutonomousRailControl,
                    costMonths: 480)
                .Description("Removes locomotive workers across existing fuel and consist strategies while reusing zero-worker wagons and native train networks. Build it when rail labor matters; conventional locomotives remain cheaper.");

        bool hasAutonomousTrains = registrator.PrototypesDb
            .TryGetProto<LocomotiveProto>(
                RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveI,
                out _);
        if (hasAutonomousTrains)
        {
            railControlBuilder
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveI)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveII)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousDieselLocomotiveI)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousDieselLocomotiveII)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousHydrogenLocomotiveI)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousHydrogenLocomotiveII)
                .AddProtoToUnlock<TenderWagonProto>(
                    RecursiveIndustryIds.Trains.AutonomousSteamTenderI)
                .AddProtoToUnlock<TenderWagonProto>(
                    RecursiveIndustryIds.Trains.AutonomousSteamTenderII)
                .AddProtoToUnlock<ElectricLocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousElectricLocomotiveI)
                .AddProtoToUnlock<ElectricLocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousElectricLocomotiveII)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousFirelessSteamLocomotive)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousTurbineLocomotive)
                .AddProtoToUnlock<TenderWagonProto>(
                    RecursiveIndustryIds.Trains.AutonomousTurbineTender)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCab)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveReactor)
                .AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCondenser);

            if (registrator.PrototypesDb.TryGetProto<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousCaptainsLocomotive,
                    out _))
            {
                railControlBuilder.AddProtoToUnlock<LocomotiveProto>(
                    RecursiveIndustryIds.Trains.AutonomousCaptainsLocomotive);
            }
        }

        ResearchNodeProto autonomousRailControl = railControlBuilder
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                32)
            .BuildAndAdd();

        autonomousRailControl.GridPosition = new Vector2i(208, 26);
        autonomousRailControl.AddParent(recursiveEpochIII);
        if (hasAutonomousTrains)
        {
            autonomousRailControl.AddParent(
                registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                    Ids.Research.HydrogenLocomotiveT2));
            autonomousRailControl.AddParent(
                registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                    IdsTrainsDlc.Research.ElectricLocoT2));
            autonomousRailControl.AddParent(
                registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                    IdsTrainsDlc.Research.FirelessLocomotive));
            autonomousRailControl.AddParent(
                registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                    IdsTrainsDlc.Research.TurbineLocomotive));
            autonomousRailControl.AddParent(
                registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                    IdsTrainsDlc.Research.NuclearLocomotive));
        }

        ResearchNodeProto recursiveEpochIV = registrator.ResearchNodeProtoBuilder
            .Start(
                "Recursive Epoch IV: Orbital Industry",
                RecursiveIndustryIds.Research.RecursiveEpochIV,
                costMonths: 480)
            .Description("Converts Programs, Packages, station hardware, propellant, and Space Research into long orbital science campaigns. Orbital Lift Coordination raises payload capacity, but launches and physical mission support remain limiting.")
            .AddFocusToUnlock(
                registrator.PrototypesDb.GetOrThrow<OfficeFocusProto>(
                    RecursiveIndustryIds.Focuses.OrbitalLiftCoordination))
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.OrbitalMissionComplex)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.RunOrbitalScienceCampaign)
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.OrbitalPowerCalibration,
                addIconToNode: true)
            .AddProductIcon(
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                64)
            .AddRequirementForMinSpaceStationTier(
                SpaceStationProto.RESEARCH_TIER_FROM)
            .SetRequireSpacePoints()
            .BuildAndAdd();

        recursiveEpochIV.GridPosition = new Vector2i(204, 18);
        recursiveEpochIV.AddParent(recursiveEpochIII);
        recursiveEpochIV.AddParent(
            registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(
                Ids.Research.SpaceStationResearch));

        ResearchNodeProto orbitalBreakthrough = registrator.ResearchNodeProtoBuilder
            .Start(
                "Orbital Breakthrough: Beamed Power",
                RecursiveIndustryIds.Research.OrbitalBreakthrough,
                costMonths: 216)
            .Description("Completed Calibrations unlock a 240 MW Orbital Power Array. It continuously consumes Dossiers for targeting and control while terrestrial power remains the independent fallback.")
            .AddLayoutEntityToUnlock(
                RecursiveIndustryIds.Power.OrbitalPowerArray)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.OrbitalPowerCalibration,
                1)
            .SetRequireSpacePoints()
            .BuildAndAdd();

        orbitalBreakthrough.GridPosition = new Vector2i(208, 22);
        orbitalBreakthrough.AddParent(recursiveEpochIV);

        ResearchNodeProto recursiveEpochV = registrator.ResearchNodeProtoBuilder
            .Start(
                "Recursive Epoch V: Recursive Frontier",
                RecursiveIndustryIds.Research.RecursiveEpochV,
                costMonths: 600)
            .Description("Combines every mature sector into one Frontier Expansion Project. The Project does not reset the island; it qualifies a choice between faster Program reinvestment and autonomous Construction Parts IV or Vehicle Parts III production.")
            .AddProductToUnlock(
                RecursiveIndustryIds.Products.FrontierExpansionProject,
                addIconToNode: true)
            .AddMachineToUnlock(
                RecursiveIndustryIds.Machines.FrontierProjectComplex)
            .AddRecipeToUnlock(
                RecursiveIndustryIds.Recipes.AssembleFrontierExpansionProject)
            .AddRequirementForLifetimeProduction(
                RecursiveIndustryIds.Products.FrontierProgram,
                256)
            .SetRequireSpacePoints()
            .BuildAndAdd();

        recursiveEpochV.GridPosition = new Vector2i(208, 18);
        recursiveEpochV.AddParent(orbitalBreakthrough);

        ResearchNodeProto recursiveProjectEfficiency =
            registrator.ResearchNodeProtoBuilder
                .Start(
                    "Frontier Mandates",
                    RecursiveIndustryIds.Research.RecursiveProjectEfficiency,
                    costMonths: 216)
                .Description("Spends one Expansion Project on either an Integration Array or Construction Nexus. Reinvest to accelerate future Programs, or choose Surge, Precision, and Recovery production according to throughput, material, and recycling pressure.")
                .AddMachineToUnlock(
                    RecursiveIndustryIds.Machines.RecursiveIntegrationArray)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.ScaleFrontierPrograms)
                .AddMachineToUnlock(
                    RecursiveIndustryIds.Machines.AutonomousConstructionNexus)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.ProduceRecursiveConstructionParts4)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.ProducePrecisionConstructionParts4)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.RecoverConstructionParts4)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.ProduceRecursiveVehicleParts3)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.ProducePrecisionVehicleParts3)
                .AddRecipeToUnlock(
                    RecursiveIndustryIds.Recipes.RecoverVehicleParts3)
                .AddProductIcon(RecursiveIndustryIds.Products.FrontierProgram)
                .AddRequirementForLifetimeProduction(
                    RecursiveIndustryIds.Products.FrontierExpansionProject,
                    1)
                .SetRequireSpacePoints()
                .BuildAndAdd();

        recursiveProjectEfficiency.GridPosition = new Vector2i(212, 18);
        recursiveProjectEfficiency.AddParent(recursiveEpochV);

        ResearchNodeProto algorithmicCoDesign = registrator.ResearchNodeProtoBuilder
            .Start(
                "Algorithmic Co-design",
                RecursiveIndustryIds.Research.AlgorithmicCoDesign,
                costMonths: 480)
            .DescriptionPerLevelWithBonus(
                "+{0} to research efficiency from physically validated algorithm and hardware co-design. Each level is an expensive mastery commitment and creates no new hardware or Focus.",
                4.Percent())
            .SetRepeatableProperties(
                10,
                IdsCore.PropertyIds.ResearchEfficiencyMultiplier,
                4.Percent(),
                ExtremeCost)
            .SetSpacePointRequiredFrom(5)
            .BuildAndAdd();

        algorithmicCoDesign.GridPosition = new Vector2i(188, 22);
        algorithmicCoDesign.AddParent(physicalValidation);

        UniversalIndustryResearchData.Register(registrator, recursiveEpochV);
    }

    private static long ExtremeCost(long baseCost, int level)
    {
        Fix64 step = Fix64.One + level;
        Fix64 growth = Math.Pow(1.4, level).ToFix64();
        return (baseCost * (step + growth).HalfFast).ToLongRounded();
    }
}