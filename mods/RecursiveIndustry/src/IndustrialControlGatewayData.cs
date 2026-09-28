using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class IndustrialControlGatewayData : IModData
{
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
                "   [4][4][4][4]   ",
                "A#>[4][4][4][4]>:X",
                "   [4][4][4][4]   ",
                "   [4][4][4][4]   ")
            .SetPrefabPath(BuildingModelPaths.ControlDeploymentGateway)
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

        var local = registrator.MachineProtoBuilder
            .Start("Local Deployment Controller", RecursiveIndustryIds.Machines.LocalDeploymentController)
            .Description("A compact controller for an isolated first deployment. Delivers 105 Stream per Package, half the central Gateway's yield, while requiring less capital and Computing. Multiple controllers may share a Fiber network within its capacity.")
            .SetCost(Costs.Build.CP4(160)
                .Product(32, Ids.Products.Electronics4)
                .Product(8, RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Product(1, RecursiveIndustryIds.Products.FrontierProgram)
                .Product(1, RecursiveIndustryIds.Products.ValidatedResearchDossier)
                .Workers(2).MaintenanceT3(2))
            .SetElectricityConsumption(250.Kw())
            .SetComputingConsumption(Computing.FromTFlops(32))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout("   [3][3][3]   ", "A#>[3][3][3]>:X", "   [3][3][3]   ")
            .SetPrefabPath(BuildingModelPaths.LocalDeploymentController)
            .SetCustomIconPath(RecursiveIndustryIcons.LocalDeploymentController)
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.DeployLocalIndustrialControl)
            .AddInput(1, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddOutput(105, RecursiveIndustryIds.Products.IndustrialControlStream)
            .BuildAndAdd()
            .WithCommonInputPorts((RecursiveIndustryIds.Products.ValidatedControlPackage, "A"))
            .WithCommonOutputPorts((RecursiveIndustryIds.Products.IndustrialControlStream, "X"))
            .BindTo(local, 60.Seconds());
        Log.Info("RecursiveIndustry: LOCAL_CONTROL_REGISTERED packages=1 stream=105 duration=60 computing=32 power_kw=250 workers=2");
    }
}