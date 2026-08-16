using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class AutonomousCapitalFabricationData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int constructionSeconds = mod.JsonConfig.GetInt("capital_matrix_construction_seconds", 80);
        int vehiclePartsSeconds = mod.JsonConfig.GetInt("capital_matrix_vehicle_parts_seconds", 64);
        int vehicleParts2Seconds = mod.JsonConfig.GetInt("capital_matrix_vehicle_parts2_seconds", 128);

        var machine = registrator.MachineProtoBuilder
            .Start(
                "Autonomous Capital Fabrication Matrix",
                RecursiveIndustryIds.Machines.AutonomousCapitalFabricationMatrix)
            .Description("A zero-worker capital-goods facility with exact-ratio 4x staged fabrication for Construction Parts through tier III and Vehicle Parts through tier II. Slower integrated rows compress lower-tier logistics at 2x throughput while paying higher power.")
            .SetCost(
                Costs.Build
                    .CP4(1440)
                    .Product(192, Ids.Products.Electronics4)
                    .Product(
                        48,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(8, RecursiveIndustryIds.Products.FrontierProgram)
                    .Workers(0)
                    .MaintenanceT3(28))
            .SetElectricityConsumption(8000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(512))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(AutonomousCapitalFabricationLayout.Create())
            .SetPrefabPath(AutonomousCapitalFabricationLayout.PrefabPath)
            .SetCustomIconPath(
                RecursiveIndustryIcons.AutonomousCapitalFabricationMatrix)
            .SetMachineSound(AutonomousCapitalFabricationLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.FabricateConstructionParts)
            .AddInput(256, Ids.Products.Steel)
            .AddInput(256, Ids.Products.Wood)
            .AddInput(384, Ids.Products.ConcreteSlab)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(512, Ids.Products.ConstructionParts)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Steel, "A"),
                (Ids.Products.Wood, "B"),
                (Ids.Products.ConcreteSlab, "C"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "D"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts, "X"))
            .BindTo(machine, constructionSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.FabricateConstructionParts2)
            .AddInput(512, Ids.Products.ConstructionParts)
            .AddInput(256, Ids.Products.Electronics)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(256, Ids.Products.ConstructionParts2)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.ConstructionParts, "A"),
                (Ids.Products.Electronics, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts2, "X"))
            .BindTo(machine, constructionSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.FabricateConstructionParts3)
            .AddInput(256, Ids.Products.ConstructionParts2)
            .AddInput(128, Ids.Products.Steel)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(128, Ids.Products.ConstructionParts3)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.ConstructionParts2, "A"),
                (Ids.Products.Steel, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts3, "X"))
            .BindTo(machine, constructionSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.IntegrateConstructionParts3)
            .SetPowerMultiplier(200.Percent())
            .AddInput(192, Ids.Products.Steel)
            .AddInput(128, Ids.Products.Wood)
            .AddInput(192, Ids.Products.ConcreteSlab)
            .AddInput(128, Ids.Products.Electronics)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(64, Ids.Products.ConstructionParts3)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Steel, "A"),
                (Ids.Products.Wood, "B"),
                (Ids.Products.ConcreteSlab, "C"),
                (Ids.Products.Electronics, "D"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "E"))
            .WithCommonOutputPorts((Ids.Products.ConstructionParts3, "X"))
            .BindTo(machine, constructionSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.FabricateVehicleParts)
            .AddInput(384, Ids.Products.MechanicalParts)
            .AddInput(128, Ids.Products.Electronics)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(256, Ids.Products.VehicleParts)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.MechanicalParts, "A"),
                (Ids.Products.Electronics, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"))
            .WithCommonOutputPorts((Ids.Products.VehicleParts, "X"))
            .BindTo(machine, vehiclePartsSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.FabricateVehicleParts2)
            .AddInput(256, Ids.Products.VehicleParts)
            .AddInput(128, Ids.Products.Steel)
            .AddInput(64, Ids.Products.Glass)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(128, Ids.Products.VehicleParts2)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.VehicleParts, "A"),
                (Ids.Products.Steel, "B"),
                (Ids.Products.Glass, "C"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "D"))
            .WithCommonOutputPorts((Ids.Products.VehicleParts2, "X"))
            .BindTo(machine, vehicleParts2Seconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.IntegrateVehicleParts2)
            .SetPowerMultiplier(150.Percent())
            .AddInput(160, Ids.Products.Steel)
            .AddInput(64, Ids.Products.Electronics)
            .AddInput(32, Ids.Products.Glass)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(64, Ids.Products.VehicleParts2)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Steel, "A"),
                (Ids.Products.Electronics, "B"),
                (Ids.Products.Glass, "C"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "D"))
            .WithCommonOutputPorts((Ids.Products.VehicleParts2, "X"))
            .BindTo(machine, vehicleParts2Seconds.Seconds());
    }
}