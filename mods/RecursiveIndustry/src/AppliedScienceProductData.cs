using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Products;

namespace RecursiveIndustry;

internal sealed class AppliedScienceProductData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        CountableProductProto labEquipment4 =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(
                Ids.Products.LabEquipment4);
        CountableProductProto officeSupplies =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(
                Ids.Products.OfficeSupplies);

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.ExperimentProgram,
            "Experiment Program",
            CountableProductGraphics.WithCustomModel(
                labEquipment4,
                RecursiveIndustryIcons.ExperimentProgram,
                "experiment_program")));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.ValidatedResearchDossier,
            "Validated Research Dossier",
            CountableProductGraphics.WithCustomModel(
                officeSupplies,
                RecursiveIndustryIcons.ValidatedResearchDossier,
                "validated_research_dossier")));
    }
}