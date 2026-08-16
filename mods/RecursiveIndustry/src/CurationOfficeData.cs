using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class CurationOfficeData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int curationSeconds = mod.JsonConfig.GetInt("curation_seconds", 120);

        var machine = registrator.MachineProtoBuilder
            .Start("Curation Office", RecursiveIndustryIds.Machines.CurationOffice)
            .Description("Turns office supplies and sustained expert labor into transportable Dataset Archives for model development.")
            .SetCost(Costs.Build.CP4(80).Workers(80).MaintenanceT3(2))
            .SetElectricityConsumption(200.Kw())
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(VerticalSliceProofLayout.Create())
            .SetPrefabPath(VerticalSliceProofLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.CurationOffice)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.CurateDataset)
            .AddInput(4, Ids.Products.OfficeSupplies)
            .AddOutput(16, RecursiveIndustryIds.Products.DatasetArchive)
            .BuildAndAdd()
            .BindTo(machine, curationSeconds.Seconds());
    }
}