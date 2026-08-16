using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class SystemsIntegrationData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int frontierProgramSeconds =
            mod.JsonConfig.GetInt("frontier_program_seconds", 720);

        var machine = registrator.MachineProtoBuilder
            .Start(
                "Systems Integration Complex",
                RecursiveIndustryIds.Machines.SystemsIntegrationComplex)
            .Description("Integrates validated models, control packages, physical science, and frontier electronics into persistent Frontier Programs.")
            .SetCost(Costs.Build.CP4(320).Workers(120).MaintenanceT3(12))
            .SetElectricityConsumption(2400.Kw())
            .SetComputingConsumption(Computing.FromTFlops(128))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(SystemsIntegrationLayout.Create())
            .SetPrefabPath(SystemsIntegrationLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.SystemsIntegrationComplex)
            .SetMachineSound(SystemsIntegrationLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ProduceFrontierProgram)
            .AddInput(4, RecursiveIndustryIds.Products.ModelArchive)
            .AddInput(16, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(4, RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddInput(8, Ids.Products.Electronics4)
            .AddOutput(1, RecursiveIndustryIds.Products.FrontierProgram)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ModelArchive, "A"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "B"),
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "C"),
                (Ids.Products.Electronics4, "D"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.FrontierProgram, "X"))
            .BindTo(machine, frontierProgramSeconds.Seconds());
    }
}