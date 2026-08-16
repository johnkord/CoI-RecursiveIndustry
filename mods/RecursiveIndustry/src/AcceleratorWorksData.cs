using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

internal sealed class AcceleratorWorksData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int rackIModules = mod.JsonConfig.GetInt("rack_i_accelerator_modules", 4);
        int rackIIModules = mod.JsonConfig.GetInt("rack_ii_accelerator_modules", 8);
        int rackIIIModules = mod.JsonConfig.GetInt("rack_iii_accelerator_modules", 16);
        int rackIBuildSeconds = mod.JsonConfig.GetInt("rack_i_build_seconds", 120);
        int rackIIUpgradeSeconds = mod.JsonConfig.GetInt("rack_ii_upgrade_seconds", 240);
        int rackIIIUpgradeSeconds = mod.JsonConfig.GetInt("rack_iii_upgrade_seconds", 360);

        string[] layout =
        {
            "   [4][4][4][4][4][4]   ",
            "A#>[4][4][4][4][4][4]   ",
            "B#>[4][4][4][4][4][4]>#X",
            "C#>[5][5][4][4][4][4]   ",
            "   [5][5][4][4][4][4]   ",
        };

        var machine = registrator.MachineProtoBuilder
            .Start("Accelerator Works", RecursiveIndustryIds.Machines.AcceleratorWorks)
            .Description("Builds accelerator modules and integrates prior-generation capital into installable Rack I, Rack II, and Rack III hardware.")
            .SetCost(Costs.Build.CP4(120).Workers(24).MaintenanceT3(4))
            .SetElectricityConsumption(500.Kw())
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(layout)
            .SetPrefabPath("Assets/Base/Machines/Assembly/AssemblyT5.prefab")
            .SetCustomIconPath(RecursiveIndustryIcons.AcceleratorWorks)
            .SetMachineSound("Assets/Base/Machines/Assembly/AssemblyT4/AssemblerSound.prefab")
            .EnableSemiInstancedRendering(ImmutableArray.Create("sign"))
            .AddSign()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.AcceleratorModule)
            .AddInput(2, Ids.Products.Microchips)
            .AddInput(2, Ids.Products.Electronics3)
            .AddInput(2, Ids.Products.TitaniumAlloy)
            .AddOutput(2, RecursiveIndustryIds.Products.AcceleratorModule)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (Ids.Products.Microchips, "A"),
                (Ids.Products.Electronics3, "B"),
                (Ids.Products.TitaniumAlloy, "C"))
            .BindTo(machine, 60.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.AcceleratorRackI)
            .AddInput(rackIModules, RecursiveIndustryIds.Products.AcceleratorModule)
            .AddInput(2, Ids.Products.Server)
            .AddInput(4, Ids.Products.Electronics3)
            .AddOutput(1, RecursiveIndustryIds.Products.AcceleratorRackI)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.AcceleratorModule, "A"),
                (Ids.Products.Server, "B"),
                (Ids.Products.Electronics3, "C"))
            .BindTo(machine, rackIBuildSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.UpgradeRackIToII)
            .AddInput(1, RecursiveIndustryIds.Products.AcceleratorRackI)
            .AddInput(rackIIModules, RecursiveIndustryIds.Products.AcceleratorModule)
            .AddInput(4, Ids.Products.Electronics4)
            .AddOutput(1, RecursiveIndustryIds.Products.FrontierRackII)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.AcceleratorRackI, "A"),
                (RecursiveIndustryIds.Products.AcceleratorModule, "B"),
                (Ids.Products.Electronics4, "C"))
            .BindTo(machine, rackIIUpgradeSeconds.Seconds());

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.UpgradeRackIIToIII)
            .AddInput(1, RecursiveIndustryIds.Products.FrontierRackII)
            .AddInput(rackIIIModules, RecursiveIndustryIds.Products.AcceleratorModule)
            .AddInput(8, Ids.Products.Electronics4)
            .AddOutput(1, RecursiveIndustryIds.Products.RecursiveRackIII)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.FrontierRackII, "A"),
                (RecursiveIndustryIds.Products.AcceleratorModule, "B"),
                (Ids.Products.Electronics4, "C"))
            .BindTo(machine, rackIIIUpgradeSeconds.Seconds());
    }
}