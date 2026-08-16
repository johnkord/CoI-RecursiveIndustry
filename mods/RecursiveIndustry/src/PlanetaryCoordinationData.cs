using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Localization;

namespace RecursiveIndustry;

internal sealed class PlanetaryCoordinationData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RegisterPlanetaryExtraction(registrator);
        RegisterContractCoordination(registrator);
        RegisterCoordinationCenter(registrator);
    }

    private static void RegisterPlanetaryExtraction(ProtoRegistrator registrator)
    {
        Percent localPerStep = 10.Percent();
        Percent outputPerStep = 10.Percent();
        Percent reservePerStep = 20.Percent();
        LocStr3 description = Loc.Str3(
            RecursiveIndustryIds.Focuses.PlanetaryExtraction + "__desc",
            "+{0} local mined yield, +{1} world-mine output, and +{2} finite world-mine reserves",
            "Post-native planetary extraction Focus; values are percentages.");

        LocStrFormatted Describe(int step) => description.Format(
            (step * localPerStep).ToStringRounded(0),
            (step * outputPerStep).ToStringRounded(0),
            (step * reservePerStep).ToStringRounded(0));

        registrator.PrototypesDb.Add(new OfficeFocusWithPropertiesProto(
            RecursiveIndustryIds.Focuses.PlanetaryExtraction,
            maxStep: 10,
            ImmutableArray.Create(
                Make.Kvp(
                    IdsCore.PropertyIds.MiningMultiplier,
                    localPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.WorldMinesEfficiency,
                    outputPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.WorldMinesReserveMultiplier,
                    reservePerStep)),
            Describe,
            baseCost: 5000,
            costIncrement: 2500,
            new OfficeFocusProto.Gfx(
                RecursiveIndustryIcons.PlanetaryExtraction)));
    }

    private static void RegisterContractCoordination(ProtoRegistrator registrator)
    {
        Percent importPerStep = 10.Percent();
        Percent unityPerStep = -5.Percent();
        LocStr2 description = Loc.Str2(
            RecursiveIndustryIds.Focuses.ContractCoordination + "__desc",
            "+{0} contract import quantity and {1} contract Unity cost",
            "Post-native contract coordination Focus; values are percentages.");

        LocStrFormatted Describe(int step) => description.Format(
            (step * importPerStep).ToStringRounded(0),
            (step * unityPerStep).ToStringRounded(0));

        registrator.PrototypesDb.Add(new OfficeFocusWithPropertiesProto(
            RecursiveIndustryIds.Focuses.ContractCoordination,
            maxStep: 10,
            ImmutableArray.Create(
                Make.Kvp(
                    IdsCore.PropertyIds.ContractsProfitMultiplier,
                    importPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.ContractsUnityCostMultiplier,
                    unityPerStep)),
            Describe,
            baseCost: 6000,
            costIncrement: 3000,
            new OfficeFocusProto.Gfx(
                RecursiveIndustryIcons.ContractCoordination)));
    }

    private static void RegisterCoordinationCenter(ProtoRegistrator registrator)
    {
        ProductProto controlPackage = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(
                RecursiveIndustryIds.Products.ValidatedControlPackage);
        ProductProto recyclables = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(Ids.Products.Recyclables);

        OfficeBuildingFactory.Register(
            registrator,
            RecursiveIndustryIds.Offices.PlanetaryCoordinationCenter,
            "Planetary Coordination Center",
            "A landmark megacenter that consolidates twenty-five AI Operations III Offices into 625,000 Focus while concentrating Computing, power, Package, maintenance, and failure risk.",
            Ids.Buildings.OfficeBuildingT3,
            RecursiveIndustryIcons.PlanetaryCoordinationCenter,
            controlPackage,
            recyclables,
            Costs.Build.CP4(3200)
                .Workers(250)
                .MaintenanceT3(24)
                .Product(512, Ids.Products.Electronics4)
                .Product(
                    128,
                    RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Product(16, RecursiveIndustryIds.Products.FrontierProgram),
            8000,
            1024,
            64,
            249900,
            inputBuffer: 512,
            outputBuffer: 512);
    }
}