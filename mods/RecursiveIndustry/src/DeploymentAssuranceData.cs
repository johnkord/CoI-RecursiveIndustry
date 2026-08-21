using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class DeploymentAssuranceData : IModData
{
    private static readonly string[] Layout =
    {
        "   [4][4][4][4][4][4]   ",
        "A#>[4][4][4][4][4][4]>#X",
        "B#>[4][4][4][4][4][4]   ",
        "C#>[5][5][4][4][4][4]   ",
        "   [5][5][4][4][4][4]   ",
    };

    public void RegisterData(ProtoRegistrator registrator)
    {
        var campus = registrator.MachineProtoBuilder
            .Start(
                "Deployment Assurance Campus",
                RecursiveIndustryIds.Machines.DeploymentAssuranceCampus)
            .Description(
                "Runs long assurance campaigns that compress four standard Package validation lines without changing the Model, Lab Equipment, or Electronics cost of each signed deployment bundle.")
            .SetCost(Costs.Build
                .CP4(1200)
                .Product(256, Ids.Products.Electronics4)
                .Product(64, RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Product(16, RecursiveIndustryIds.Products.FrontierProgram)
                .Product(8, RecursiveIndustryIds.Products.ValidatedResearchDossier)
                .Workers(48)
                .MaintenanceT3(16))
            .SetElectricityConsumption(4000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(256))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(Layout)
            .SetPrefabPath(VerticalSliceProofLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.DeploymentAssuranceCampus)
            .SetMachineSound(VerticalSliceProofLayout.SoundPath)
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.BatchDeploymentAssurance)
            .Description(
                "Batches sixteen ordinary validation campaigns into one long run. Material ratios remain identical to standard Package validation.")
            .AddInput(16, RecursiveIndustryIds.Products.ModelArchive)
            .AddInput(32, Ids.Products.LabEquipment4)
            .AddInput(32, Ids.Products.Electronics3)
            .AddOutput(128, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ModelArchive, "A"),
                (Ids.Products.LabEquipment4, "B"),
                (Ids.Products.Electronics3, "C"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "X"))
            .BindTo(campus, 720.Seconds());

        Log.Info(
            "RecursiveIndustry: DEPLOYMENT_ASSURANCE_CAMPUS_REGISTERED"
            + " models=16 lab_equipment_iv=32 electronics_iii=32"
            + " packages=128 duration_seconds=720 packages_per_hour=640"
            + " computing=256 power_kw=4000 workers=48 maintenance_t3=16");
    }
}