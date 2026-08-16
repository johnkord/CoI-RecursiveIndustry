using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class AutonomousElectronicsIntegrationData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int intermediateSeconds = mod.JsonConfig.GetInt("autonomous_electronics_intermediate_seconds", 80);
        int integratedSeconds = mod.JsonConfig.GetInt("autonomous_electronics_integrated_seconds", 120);

        var machine = registrator.MachineProtoBuilder
            .Start(
                "Autonomous Electronics Integration Complex",
                RecursiveIndustryIds.Machines
                    .AutonomousElectronicsIntegrationComplex)
            .Description("A zero-worker electronics facility for sustained Electronics II demand. Staged integration preserves PCB and Electronics logistics at 4x Assembly V throughput; direct integration compresses those handoffs at 2x throughput without changing material ratios.")
            .SetCost(
                Costs.Build
                    .CP4(960)
                    .Product(128, Ids.Products.Electronics4)
                    .Product(
                        32,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(4, RecursiveIndustryIds.Products.FrontierProgram)
                    .Workers(0)
                    .MaintenanceT3(20))
            .SetElectricityConsumption(6000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(256))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(AutonomousElectronicsIntegrationLayout.Create())
            .SetPrefabPath(AutonomousElectronicsIntegrationLayout.PrefabPath)
            .SetCustomIconPath(
                RecursiveIndustryIcons.AutonomousElectronicsIntegrationComplex)
            .EnableSemiInstancedRendering()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(
                RecursiveIndustryIds.Recipes
                    .IntegrateElectronics2Intermediates)
            .AddInput(128, Ids.Products.PCB)
            .AddInput(256, Ids.Products.Electronics)
            .AddInput(64, Ids.Products.PolySilicon)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(128, Ids.Products.Electronics2)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.PCB, "A"),
                (Ids.Products.Electronics, "B"),
                (Ids.Products.PolySilicon, "C"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "D"))
            .WithCommonOutputPorts((Ids.Products.Electronics2, "X"))
            .BindTo(machine, intermediateSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.IntegrateElectronics2Direct)
            .AddInput(216, Ids.Products.Copper)
            .AddInput(32, Ids.Products.Rubber)
            .AddInput(24, Ids.Products.Glass)
            .AddInput(48, Ids.Products.Plastic)
            .AddInput(48, Ids.Products.PolySilicon)
            .AddInput(
                1,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(96, Ids.Products.Electronics2)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Copper, "A"),
                (Ids.Products.Rubber, "B"),
                (Ids.Products.Glass, "C"),
                (Ids.Products.Plastic, "D"),
                (Ids.Products.PolySilicon, "E"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "F"))
            .WithCommonOutputPorts((Ids.Products.Electronics2, "X"))
            .BindTo(machine, integratedSeconds.Seconds());
    }
}