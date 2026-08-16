using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class AutonomousMicrochipData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int durationSeconds =
            mod.JsonConfig.GetInt("autonomous_microchip_seconds", 60);

        var machine = registrator.MachineProtoBuilder
            .Start(
                "Autonomous Microchip Complex",
                RecursiveIndustryIds.Machines.AutonomousMicrochipComplex)
            .Description("Integrates the complete twelve-stage microchip route into one lights-out fab that trades workers and footprint for extreme Computing, power, control validation, and concentrated inputs.")
            .SetCost(
                Costs.Build
                    .CP4(1440)
                    .Product(128, Ids.Products.Electronics4)
                    .Product(
                        32,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(4, RecursiveIndustryIds.Products.FrontierProgram)
                    .Workers(0)
                    .MaintenanceT3(24))
            .SetElectricityConsumption(8000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(256))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(AutonomousMicrochipLayout.Create())
            .SetPrefabPath(AutonomousMicrochipLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.AutonomousMicrochipComplex)
            .EnableSemiInstancedRendering()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.IntegrateAutonomousMicrochips)
            .AddInput(24, Ids.Products.SiliconWafer)
            .AddInput(32, Ids.Products.Acid)
            .AddInput(32, Ids.Products.Water)
            .AddInput(24, Ids.Products.Gold)
            .AddInput(16, Ids.Products.Electronics2)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(48, Ids.Products.Microchips)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.SiliconWafer, "A"),
                (Ids.Products.Acid, "D"),
                (Ids.Products.Water, "E"),
                (Ids.Products.Gold, "F"),
                (Ids.Products.Electronics2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"))
            .WithCommonOutputPorts((Ids.Products.Microchips, "X"))
            .BindTo(machine, durationSeconds.Seconds());
    }
}