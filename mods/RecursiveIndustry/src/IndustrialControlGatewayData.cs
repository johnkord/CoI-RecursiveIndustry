using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class IndustrialControlGatewayData : IModData
{
    private const string PrefabPath =
        "Assets/Base/Machines/Assembly/AssemblyT5.prefab";

    public void RegisterData(ProtoRegistrator registrator)
    {
        var gateway = registrator.MachineProtoBuilder
            .Start("Control Deployment Gateway",
                RecursiveIndustryIds.Machines.ControlDeploymentGateway)
            .Description(
                "Deploys signed Validated Control Packages as continuous Industrial Control Stream. Local deployment serves Access networks efficiently; Backbone deployment trades additional power for a denser control plane.")
            .SetCost(Costs.Build
                .CP4(640)
                .Product(128, Ids.Products.Electronics4)
                .Product(32, RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Product(4, RecursiveIndustryIds.Products.FrontierProgram)
                .Product(4, RecursiveIndustryIds.Products.ValidatedResearchDossier)
                .Workers(4)
                .MaintenanceT3(8))
            .SetElectricityConsumption(1000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(256))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(
                "   [4][4][4][4][4][4]   ",
                "A#>[4][4][4][4][4][4]>:X",
                "   [4][4][4][4][4][4]   ",
                "   [4][4][4][4][4][4]   ",
                "   [4][4][4][4][4][4]   ")
            .SetPrefabPath(PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.ControlDeploymentGateway)
            .SetMachineSound(SystemsIntegrationLayout.SoundPath)
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.DeployIndustrialControl)
            .AddInput(1, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(210, RecursiveIndustryIds.Products.IndustrialControlStream)
            .BuildAndAdd()
            .WithCommonInputPorts((RecursiveIndustryIds.Products.ValidatedControlPackage, "A"))
            .WithCommonOutputPorts((RecursiveIndustryIds.Products.IndustrialControlStream, "X"))
            .BindTo(gateway, 60.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl)
            .Description(
                "Deploys two signed Packages at Backbone rate. Preserves 210 Stream per Package while trading power for Gateway density.")
            .AddInput(2, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(420, RecursiveIndustryIds.Products.IndustrialControlStream)
            .SetPowerMultiplier(250.Percent())
            .BuildAndAdd()
            .WithCommonInputPorts((RecursiveIndustryIds.Products.ValidatedControlPackage, "A"))
            .WithCommonOutputPorts((RecursiveIndustryIds.Products.IndustrialControlStream, "X"))
            .BindTo(gateway, 60.Seconds());

        Log.Info(
            "RecursiveIndustry: INDUSTRIAL_CONTROL_GATEWAY_REGISTERED"
            + " local_input_package=1 local_output_stream=210"
            + " backbone_input_package=2 backbone_output_stream=420"
            + " duration_seconds=60 backbone_power_percent=250"
            + " computing=256 power_kw=1000 workers=4 maintenance_t3=8");
    }
}