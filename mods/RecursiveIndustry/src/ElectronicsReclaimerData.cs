using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class ElectronicsReclaimerData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        var machine = registrator.MachineProtoBuilder
            .Start(
                "Electronics Reclaimer",
                RecursiveIndustryIds.Machines.ElectronicsReclaimer)
            .Description("Recovers a strictly limited share of spent accelerators and voluntarily retired rack capital. Every route remains materially lossy.")
            .SetCost(Costs.Build.CP4(80).Workers(12).MaintenanceT3(3))
            .SetElectricityConsumption(500.Kw())
            .SetCategories(Ids.ToolbarCategories.Waste_Solid)
            .SetLayout(
                "D#>[4][4][4][4][4][4]   ",
                "A#>[4][4][4][4][4][4]>#X",
                "B#>[4][4][4][4][4][4]>#Y",
                "C#>[5][5][4][4][4][4]>~Z",
                "E#>[5][5][4][4][4][4]   ")
            .SetPrefabPath(BuildingModelPaths.ElectronicsReclaimer)
            .SetCustomIconPath(RecursiveIndustryIcons.ElectronicsReclaimer)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.SalvageRackI)
            .AddInput(1, RecursiveIndustryIds.Products.AcceleratorRackI)
            .AddOutput(1, Ids.Products.Electronics3)
            .AddOutput(1, Ids.Products.TitaniumAlloy)
            .AddOutput(2, Ids.Products.Waste)
            .BuildAndAdd()
            .WithCommonOutputPorts(
                (Ids.Products.Electronics3, "X"),
                (Ids.Products.TitaniumAlloy, "Y"),
                (Ids.Products.Waste, "Z"))
            .BindTo(machine, 60.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.SalvageRackII)
            .AddInput(1, RecursiveIndustryIds.Products.FrontierRackII)
            .AddOutput(2, Ids.Products.Electronics4)
            .AddOutput(2, Ids.Products.TitaniumAlloy)
            .AddOutput(4, Ids.Products.Waste)
            .BuildAndAdd()
            .WithCommonOutputPorts(
                (Ids.Products.Electronics4, "X"),
                (Ids.Products.TitaniumAlloy, "Y"),
                (Ids.Products.Waste, "Z"))
            .BindTo(machine, 90.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.RemanufactureAcceleratorModules)
            .Description("Refurbishes spent accelerators with replacement components and a signed test Package. Slower and more power-intensive than new manufacture; competes with recovering Microchips and Electronics from the same spent stock.")
            .SetPowerMultiplier(200.Percent())
            .AddInput(4, RecursiveIndustryIds.Products.SpentAccelerator)
            .AddInput(2, Ids.Products.Microchips)
            .AddInput(2, Ids.Products.Electronics3)
            .AddInput(2, Ids.Products.TitaniumAlloy)
            .AddInput(1, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(4, RecursiveIndustryIds.Products.AcceleratorModule)
            .AddOutput(2, Ids.Products.Waste)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.SpentAccelerator, "A"),
                (Ids.Products.Microchips, "B"),
                (Ids.Products.Electronics3, "C"),
                (Ids.Products.TitaniumAlloy, "D"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "E"))
            .WithCommonOutputPorts((RecursiveIndustryIds.Products.AcceleratorModule, "X"), (Ids.Products.Waste, "Z"))
            .BindTo(machine, 240.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.SalvageRackIII)
            .AddInput(1, RecursiveIndustryIds.Products.RecursiveRackIII)
            .AddOutput(4, Ids.Products.Electronics4)
            .AddOutput(4, Ids.Products.TitaniumAlloy)
            .AddOutput(8, Ids.Products.Waste)
            .BuildAndAdd()
            .WithCommonOutputPorts(
                (Ids.Products.Electronics4, "X"),
                (Ids.Products.TitaniumAlloy, "Y"),
                (Ids.Products.Waste, "Z"))
            .BindTo(machine, 120.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.FastAcceleratorRecovery)
            .AddInput(1, RecursiveIndustryIds.Products.SpentAccelerator)
            .AddOutput(1, Ids.Products.Electronics2)
            .AddOutput(2, Ids.Products.Waste)
            .BuildAndAdd()
            .WithCommonOutputPorts(
                (Ids.Products.Electronics2, "X"),
                (Ids.Products.Waste, "Z"))
            .BindTo(machine, 30.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.PrecisionAcceleratorRecovery)
            .SetPowerMultiplier(150.Percent())
            .AddInput(1, RecursiveIndustryIds.Products.SpentAccelerator)
            .AddOutput(1, Ids.Products.Microchips)
            .AddOutput(2, Ids.Products.Electronics2)
            .BuildAndAdd()
            .WithCommonOutputPorts(
                (Ids.Products.Microchips, "X"),
                (Ids.Products.Electronics2, "Y"))
            .BindTo(machine, 90.Seconds());
    }
}