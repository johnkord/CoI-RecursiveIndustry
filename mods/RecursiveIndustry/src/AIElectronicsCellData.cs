using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class AIElectronicsCellData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int precisionSeconds =
            mod.JsonConfig.GetInt("precision_electronics_seconds", 180);
        int throughputSeconds =
            mod.JsonConfig.GetInt("throughput_electronics_seconds", 60);

        var machine = registrator.MachineProtoBuilder
            .Start("AI Electronics Cell", RecursiveIndustryIds.Machines.AIElectronicsCell)
            .Description("Uses installed validated control and Computing to choose material-efficient or high-throughput Electronics III production without consuming Packages per batch.")
            .SetCost(Costs.Build.CP4(120).Workers(8).MaintenanceT3(4))
            .SetElectricityConsumption(600.Kw())
            .SetComputingConsumption(Computing.FromTFlops(12))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(VerticalSliceProofLayout.Create(includeThirdInput: false))
            .SetPrefabPath(VerticalSliceProofLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.AiElectronicsCell)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.PrecisionElectronics3)
            .AddInput(8, Ids.Products.Microchips)
            .AddInput(16, Ids.Products.Electronics2)
            .AddOutput(12, Ids.Products.Electronics3)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Microchips, "A"),
                (Ids.Products.Electronics2, "B"))
            .BindTo(machine, precisionSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ThroughputElectronics3)
            .SetPowerMultiplier(150.Percent())
            .AddInput(6, Ids.Products.Microchips)
            .AddInput(24, Ids.Products.Electronics2)
            .AddOutput(12, Ids.Products.Electronics3)
            .AddOutput(4, Ids.Products.Waste)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Microchips, "A"),
                (Ids.Products.Electronics2, "B"))
            .WithCommonOutputPorts(
                (Ids.Products.Electronics3, "X"),
                (Ids.Products.Waste, "Z"))
            .BindTo(machine, throughputSeconds.Seconds());
    }
}