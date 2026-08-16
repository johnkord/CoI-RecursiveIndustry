using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class AppliedScienceData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int programSeconds =
            mod.JsonConfig.GetInt("experiment_program_seconds", 240);
        int programsPerBatch =
            mod.JsonConfig.GetInt("experiment_programs_per_batch", 4);
        int pilotSeconds =
            mod.JsonConfig.GetInt("pilot_validation_seconds", 360);
        int dossiersPerBatch =
            mod.JsonConfig.GetInt("validated_dossiers_per_batch", 1);

        var institute = registrator.MachineProtoBuilder
            .Start(
                "AI Science Institute",
                RecursiveIndustryIds.Machines.AIScienceInstitute)
            .Description("Uses curated data, validated models, and continuous Computing to develop physical Experiment Programs for real-world testing.")
            .SetCost(Costs.Build.CP4(200).Workers(32).MaintenanceT3(6))
            .SetElectricityConsumption(1200.Kw())
            .SetComputingConsumption(Computing.FromTFlops(64))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(VerticalSliceProofLayout.Create())
            .SetPrefabPath(VerticalSliceProofLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.AiScienceInstitute)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        var pilot = registrator.MachineProtoBuilder
            .Start(
                "Pilot Science Complex",
                RecursiveIndustryIds.Machines.PilotScienceComplex)
            .Description("Runs labor- and material-intensive physical trials that turn Experiment Programs into Validated Research Dossiers.")
            .SetCost(Costs.Build.CP4(240).Workers(80).MaintenanceT3(8))
            .SetElectricityConsumption(1600.Kw())
            .SetComputingConsumption(Computing.FromTFlops(8))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(VerticalSliceProofLayout.Create())
            .SetPrefabPath(VerticalSliceProofLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.PilotScienceComplex)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.DevelopExperimentPrograms)
            .AddInput(8, RecursiveIndustryIds.Products.DatasetArchive)
            .AddInput(1, RecursiveIndustryIds.Products.ModelArchive)
            .AddOutput(programsPerBatch, RecursiveIndustryIds.Products.ExperimentProgram)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.DatasetArchive, "A"),
                (RecursiveIndustryIds.Products.ModelArchive, "B"))
            .BindTo(institute, programSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ValidatePhysicalExperiment)
            .AddInput(1, RecursiveIndustryIds.Products.ExperimentProgram)
            .AddInput(16, Ids.Products.LabEquipment4)
            .AddInput(4, Ids.Products.TitaniumAlloy)
            .AddOutput(
                dossiersPerBatch,
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddOutput(12, Ids.Products.Recyclables)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ExperimentProgram, "A"),
                (Ids.Products.LabEquipment4, "B"),
                (Ids.Products.TitaniumAlloy, "C"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "X"),
                (Ids.Products.Recyclables, "Z"))
            .BindTo(pilot, pilotSeconds.Seconds());
    }
}