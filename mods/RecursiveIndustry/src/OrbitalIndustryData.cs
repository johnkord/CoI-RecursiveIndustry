using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Mods;
using Mafi.Localization;

namespace RecursiveIndustry;

internal sealed class OrbitalIndustryData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RegisterOrbitalLiftPolicy(registrator);
        RegisterMissionComplex(registrator);
    }

    private static void RegisterOrbitalLiftPolicy(ProtoRegistrator registrator)
    {
        Percent effectPerStep = 5.Percent();
        LocStr1 description = Loc.Str1(
            RecursiveIndustryIds.Focuses.OrbitalLiftCoordination + "__desc",
            "+{0} rocket payload capacity",
            "Post-native orbital lift coordination Focus; {0} is a percentage.");

        LocStrFormatted Describe(int step) =>
            description.Format((step * effectPerStep).ToStringRounded(0));

        registrator.PrototypesDb.Add(new OfficeFocusWithPropertiesProto(
            RecursiveIndustryIds.Focuses.OrbitalLiftCoordination,
            maxStep: 10,
            ImmutableArray.Create(Make.Kvp(
                IdsCore.PropertyIds.RocketsCapacityMultiplier,
                effectPerStep)),
            Describe,
            baseCost: 10000,
            costIncrement: 5000,
            new OfficeFocusProto.Gfx(
                RecursiveIndustryIcons.OrbitalLiftCoordination)));
    }

    private static void RegisterMissionComplex(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int durationSeconds =
            mod.JsonConfig.GetInt("orbital_science_seconds", 3600);

        var machine = registrator.MachineProtoBuilder
            .Start(
                "Orbital Payload & Mission Complex",
                RecursiveIndustryIds.Machines.OrbitalMissionComplex)
            .Description("Packages instruments, experiments, station hardware, control systems, and propellant into long orbital science campaigns that return validated physical research dossiers.")
            .SetCost(
                Costs.Build
                    .CP4(1200)
                    .Product(128, Ids.Products.SpaceStationParts2)
                    .Product(128, Ids.Products.Electronics4)
                    .Product(
                        32,
                        RecursiveIndustryIds.Products.ValidatedControlPackage)
                    .Product(16, RecursiveIndustryIds.Products.FrontierProgram)
                    .Workers(40)
                    .MaintenanceT3(20))
            .SetElectricityConsumption(6000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(256))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(OrbitalMissionLayout.Create())
            .SetPrefabPath(OrbitalMissionLayout.PrefabPath)
            .SetCustomIconPath(RecursiveIndustryIcons.OrbitalMissionComplex)
            .EnableSemiInstancedRendering()
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.RunOrbitalScienceCampaign)
            .AddInput(
                8,
                RecursiveIndustryIds.Products.ExperimentProgram)
            .AddInput(32, Ids.Products.LabEquipment4)
            .AddInput(16, Ids.Products.SpaceStationParts2)
            .AddInput(
                8,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .AddInput(32, Ids.Products.ChemicalFuel)
            .AddInput(32, Ids.Products.Hydrogen)
            .AddOutput(
                12,
                RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddOutput(
                1,
                RecursiveIndustryIds.Products.OrbitalPowerCalibration)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ExperimentProgram, "A"),
                (Ids.Products.LabEquipment4, "F"),
                (Ids.Products.SpaceStationParts2, "B"),
                (RecursiveIndustryIds.Products.ValidatedControlPackage, "C"),
                (Ids.Products.ChemicalFuel, "D"),
                (Ids.Products.Hydrogen, "E"))
            .WithCommonOutputPorts(
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "X"),
                (RecursiveIndustryIds.Products.OrbitalPowerCalibration, "Y"))
            .BindTo(machine, durationSeconds.Seconds());
    }
}