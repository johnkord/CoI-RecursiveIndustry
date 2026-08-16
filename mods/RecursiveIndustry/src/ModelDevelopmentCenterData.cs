using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class ModelDevelopmentCenterData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int hardwareTrainingSeconds =
            mod.JsonConfig.GetInt("hardware_training_seconds", 240);
        int curationAdaptationSeconds =
            mod.JsonConfig.GetInt("curation_adaptation_seconds", 120);
        int validationSeconds = mod.JsonConfig.GetInt("validation_seconds", 180);
        int packagesPerBatch =
            mod.JsonConfig.GetInt("validation_packages_per_batch", 8);

        var machine = registrator.MachineProtoBuilder
            .Start(
                "Model Development Center",
                RecursiveIndustryIds.Machines.ModelDevelopmentCenter)
            .Description("Develops reusable Model Archives through hardware-intensive or curation-intensive campaigns, then validates them into renewable control packages.")
            .SetCost(Costs.Build.CP4(160).Workers(24).MaintenanceT3(5))
            .SetElectricityConsumption(800.Kw())
            .SetComputingConsumption(Computing.FromTFlops(24))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(VerticalSliceProofLayout.Create())
            .SetPrefabPath(VerticalSliceProofLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.ModelDevelopmentCenter)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.HardwareIntensiveTraining)
            .AddInput(8, RecursiveIndustryIds.Products.DatasetArchive)
            .AddInput(2, Ids.Products.LabEquipment4)
            .AddInput(1, RecursiveIndustryIds.Products.AcceleratorModule)
            .AddOutput(1, RecursiveIndustryIds.Products.ModelArchive)
            .AddOutput(1, RecursiveIndustryIds.Products.SpentAccelerator)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.DatasetArchive, "A"),
                (Ids.Products.LabEquipment4, "B"),
                (RecursiveIndustryIds.Products.AcceleratorModule, "C"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.ModelArchive, "X"),
                (RecursiveIndustryIds.Products.SpentAccelerator, "Y"))
            .BindTo(machine, hardwareTrainingSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.CurationIntensiveAdaptation)
            .AddInput(16, RecursiveIndustryIds.Products.DatasetArchive)
            .AddInput(4, Ids.Products.LabEquipment4)
            .AddOutput(1, RecursiveIndustryIds.Products.ModelArchive)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.DatasetArchive, "A"),
                (Ids.Products.LabEquipment4, "B"))
            .BindTo(machine, curationAdaptationSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ValidateControlPackages)
            .AddInput(1, RecursiveIndustryIds.Products.ModelArchive)
            .AddInput(2, Ids.Products.LabEquipment4)
            .AddInput(2, Ids.Products.Electronics3)
            .AddOutput(packagesPerBatch, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ModelArchive, "A"),
                (Ids.Products.LabEquipment4, "B"),
                (Ids.Products.Electronics3, "C"))
            .BindTo(machine, validationSeconds.Seconds());
    }
}