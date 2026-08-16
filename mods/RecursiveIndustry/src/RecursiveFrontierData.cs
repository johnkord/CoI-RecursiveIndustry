using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class RecursiveFrontierData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int projectSeconds =
            mod.JsonConfig.GetInt("frontier_project_seconds", 7200);
        int programSeconds =
            mod.JsonConfig.GetInt("recursive_program_seconds", 720);
        int constructionPartsSeconds =
            mod.JsonConfig.GetInt("recursive_cp4_seconds", 80);
        int vehiclePartsSeconds =
            mod.JsonConfig.GetInt("recursive_vehicle_parts3_seconds", 32);
        int constructionTradeoffSeconds = checked(constructionPartsSeconds * 2);
        int vehicleTradeoffSeconds = checked(vehiclePartsSeconds * 2);

        var projectComplex = registrator.MachineProtoBuilder
            .Start(
                "Frontier Project Complex",
                RecursiveIndustryIds.Machines.FrontierProjectComplex)
            .Description("Combines autonomous freight capital, integrated microchips, planetary-scale station hardware, orbital science, and Frontier Programs into one physical expansion project.")
            .SetCost(
                Costs.Build
                    .CP4(2400)
                    .Product(256, Ids.Products.SpaceStationParts2)
                    .Product(256, Ids.Products.Electronics4)
                    .Product(
                        64,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(64, RecursiveIndustryIds.Products.FrontierProgram)
                    .Workers(80)
                    .MaintenanceT3(32))
            .SetElectricityConsumption(12000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(512))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(FrontierProjectLayout.Create())
            .SetPrefabPath(FrontierProjectLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.FrontierProjectComplex)
            .SetMachineSound(FrontierProjectLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(
                RecursiveIndustryIds.Recipes.AssembleFrontierExpansionProject)
            .AddInput(64, RecursiveIndustryIds.Products.FrontierProgram)
            .AddInput(256, Ids.Products.Microchips)
            .AddInput(64, Ids.Products.SpaceStationParts2)
            .AddInput(
                32,
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddInput(128, Ids.Products.VehicleParts3)
            .AddInput(256, Ids.Products.ConstructionParts4)
            .AddOutput(
                1,
                RecursiveIndustryIds.Products.FrontierExpansionProject)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.FrontierProgram, "A"),
                (Ids.Products.Microchips, "B"),
                (Ids.Products.SpaceStationParts2, "C"),
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "D"),
                (Ids.Products.VehicleParts3, "E"),
                (Ids.Products.ConstructionParts4, "F"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.FrontierExpansionProject, "X"))
            .BindTo(projectComplex, projectSeconds.Seconds());

        var integrationArray = registrator.MachineProtoBuilder
            .Start(
                "Recursive Integration Array",
                RecursiveIndustryIds.Machines.RecursiveIntegrationArray)
            .Description("An earned high-capacity Systems Integration facility that converts validated models, control, science, and electronics into eight Frontier Programs per campaign.")
            .SetCost(
                Costs.Build
                    .CP4(1800)
                    .Product(256, Ids.Products.Electronics4)
                    .Product(
                        64,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(
                        1,
                        RecursiveIndustryIds.Products.FrontierExpansionProject)
                    .Workers(60)
                    .MaintenanceT3(24))
            .SetElectricityConsumption(6000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(512))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(SystemsIntegrationLayout.Create())
            .SetPrefabPath(SystemsIntegrationLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.RecursiveIntegrationArray)
            .SetMachineSound(SystemsIntegrationLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ScaleFrontierPrograms)
            .AddInput(12, RecursiveIndustryIds.Products.ModelArchive)
            .AddInput(
                32,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(
                12,
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddInput(24, Ids.Products.Electronics4)
            .AddOutput(8, RecursiveIndustryIds.Products.FrontierProgram)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ModelArchive, "A"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "B"),
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "C"),
                (Ids.Products.Electronics4, "D"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.FrontierProgram, "X"))
            .BindTo(integrationArray, programSeconds.Seconds());

        var constructionNexus = registrator.MachineProtoBuilder
            .Start(
                "Autonomous Construction Nexus",
                RecursiveIndustryIds.Machines.AutonomousConstructionNexus)
            .Description("A zero-worker Frontier Mandate facility for Construction Parts IV and Vehicle Parts III. Surge preserves vanilla ratios at 4x throughput; Precision spends Dossiers and Packages for 12.5% more output; Recovery consumes Recyclables to displace 25% of virgin parts.")
            .SetCost(
                Costs.Build
                    .CP4(1800)
                    .Product(256, Ids.Products.Electronics4)
                    .Product(
                        64,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(
                        1,
                        RecursiveIndustryIds.Products.FrontierExpansionProject)
                    .Workers(0)
                    .MaintenanceT3(32))
            .SetElectricityConsumption(12000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(1024))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(ConstructionNexusLayout.Create())
            .SetPrefabPath(ConstructionNexusLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.AutonomousConstructionNexus)
            .SetMachineSound(ConstructionNexusLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(
                RecursiveIndustryIds.Recipes.ProduceRecursiveConstructionParts4)
            .AddInput(128, Ids.Products.ConstructionParts3)
            .AddInput(64, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(64, Ids.Products.ConstructionParts4)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.ConstructionParts3, "A"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts4, "X"))
            .BindTo(constructionNexus, constructionPartsSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(
                RecursiveIndustryIds.Recipes.ProducePrecisionConstructionParts4)
            .AddInput(128, Ids.Products.ConstructionParts3)
            .AddInput(64, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddOutput(72, Ids.Products.ConstructionParts4)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.ConstructionParts3, "A"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"),
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "D"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts4, "X"))
            .BindTo(constructionNexus, constructionTradeoffSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.RecoverConstructionParts4)
            .AddInput(96, Ids.Products.ConstructionParts3)
            .AddInput(48, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(64, Ids.Products.Recyclables)
            .AddOutput(64, Ids.Products.ConstructionParts4)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.ConstructionParts3, "A"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"),
                (Ids.Products.Recyclables, "E"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts4, "X"))
            .BindTo(constructionNexus, constructionTradeoffSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(
                RecursiveIndustryIds.Recipes.ProduceRecursiveVehicleParts3)
            .AddInput(64, Ids.Products.VehicleParts2)
            .AddInput(16, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(32, Ids.Products.VehicleParts3)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.VehicleParts2, "A"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"))
            .WithCommonOutputPorts((Ids.Products.VehicleParts3, "X"))
            .BindTo(constructionNexus, vehiclePartsSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ProducePrecisionVehicleParts3)
            .AddInput(64, Ids.Products.VehicleParts2)
            .AddInput(16, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddOutput(36, Ids.Products.VehicleParts3)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.VehicleParts2, "A"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"),
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "D"))
            .WithCommonOutputPorts((Ids.Products.VehicleParts3, "X"))
            .BindTo(constructionNexus, vehicleTradeoffSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.RecoverVehicleParts3)
            .AddInput(48, Ids.Products.VehicleParts2)
            .AddInput(12, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(32, Ids.Products.Recyclables)
            .AddOutput(32, Ids.Products.VehicleParts3)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.VehicleParts2, "A"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"),
                (Ids.Products.Recyclables, "E"))
            .WithCommonOutputPorts((Ids.Products.VehicleParts3, "X"))
            .BindTo(constructionNexus, vehicleTradeoffSeconds.Seconds());
    }
}