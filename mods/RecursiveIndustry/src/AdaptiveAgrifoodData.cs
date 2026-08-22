using System;
using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Economy;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;

namespace RecursiveIndustry;

internal sealed class AdaptiveAgrifoodData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RegisterPrecisionIrrigation(registrator);

        ProductProto electronics = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(Ids.Products.Electronics4);
        ProductProto controlPackages = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(
                RecursiveIndustryIds.Products.ValidatedControlPackage);
        ProductProto programs = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(RecursiveIndustryIds.Products.FrontierProgram);

        RegisterSensorGuidedGreenhouse(
            registrator,
            electronics,
            controlPackages,
            programs);
        RegisterMonitoredPoultryFarm(
            registrator,
            electronics,
            controlPackages,
            programs);
        Log.Info(
            "RecursiveIndustry: ADAPTIVE_AGRIFOOD_UPGRADES_LINKED "
            + "greenhouse=FarmT4->RecursiveIndustry_SensorGuidedGreenhouse "
            + "poultry=ChickenFarm->RecursiveIndustry_MonitoredPoultryFarm");
    }

    private static void RegisterPrecisionIrrigation(
        ProtoRegistrator registrator)
    {
        Percent effectPerStep = -2.Percent();
        LocStr1 description = Loc.Str1(
            RecursiveIndustryIds.Focuses.PrecisionIrrigation + "__desc",
            "{0} farm water consumption",
            "Sensor-guided irrigation changes farm water demand only; {0} is a signed percentage.");

        LocStrFormatted Describe(int step) =>
            description.Format((step * effectPerStep).ToStringRounded(0));

        registrator.PrototypesDb.Add(new OfficeFocusWithPropertiesProto(
            RecursiveIndustryIds.Focuses.PrecisionIrrigation,
            maxStep: 5,
            ImmutableArray.Create(Make.Kvp(
                IdsCore.PropertyIds.FarmWaterConsumptionMultiplier,
                effectPerStep)),
            Describe,
            baseCost: 8000,
            costIncrement: 4000,
            new OfficeFocusProto.Gfx(
                RecursiveIndustryIcons.PrecisionIrrigation)));
    }

    private static void RegisterSensorGuidedGreenhouse(
        ProtoRegistrator registrator,
        ProductProto electronics,
        ProductProto controlPackages,
        ProductProto programs)
    {
        FarmProto source = registrator.PrototypesDb
            .GetOrThrow<FarmProto>(Ids.Buildings.FarmT4);
        FarmProto sensorGuidedGreenhouse = registrator.PrototypesDb.Add(new FarmProto(
            RecursiveIndustryIds.Farms.SensorGuidedGreenhouse,
            Proto.CreateStr(
                RecursiveIndustryIds.Farms.SensorGuidedGreenhouse,
                "Sensor-Guided Greenhouse",
                "A labor-compressed Greenhouse II with embedded crop, substrate, and climate monitoring. It preserves native crop schedules, fertility, water, fertilizer, yield, weather, and maintenance behavior."),
            source.Layout,
            WithAutonomousHardware(
                source.Costs,
                electronics,
                64,
                controlPackages,
                16,
                programs,
                4),
            source.WaterCollectedPerDay,
            source.FertilityReplenishPerDay,
            source.YieldMultiplier,
            source.DemandsMultiplier,
            source.HasIrrigationAndFertilizerSupport,
            source.IsGreenhouse,
            source.WaterEvaporationPerDay,
            WithCustomIcon(
                source.Graphics,
                RecursiveIndustryIcons.SensorGuidedGreenhouse),
            source.ConstructionDurationPerProduct));
            LinkUpgrade(source, sensorGuidedGreenhouse, "Greenhouse II");
    }

    private static void RegisterMonitoredPoultryFarm(
        ProtoRegistrator registrator,
        ProductProto electronics,
        ProductProto controlPackages,
        ProductProto programs)
    {
        AnimalFarmProto source = registrator.PrototypesDb
            .GetOrThrow<AnimalFarmProto>(Ids.Buildings.ChickenFarm);
        AnimalFarmProto monitoredPoultryFarm = registrator.PrototypesDb.Add(new AnimalFarmProto(
            RecursiveIndustryIds.Farms.MonitoredPoultryFarm,
            Proto.CreateStr(
                RecursiveIndustryIds.Farms.MonitoredPoultryFarm,
                "Monitored Poultry Farm",
                "A labor-compressed Chicken Farm with embedded flock monitoring. It preserves native births, feed, water, starvation, growth, slaughter controls, Eggs, Chicken Carcass, and buffer behavior."),
            source.Layout,
            WithAutonomousHardware(
                source.Costs,
                electronics,
                32,
                controlPackages,
                8,
                programs,
                2),
            source.Animal,
            source.AnimalsCapacity,
            source.CarcassProto,
            source.CarcassOutpotPortName,
            source.AnimalsBornPer100AnimalsPerMonth,
            source.CarcassMultiplier,
            source.FoodPerAnimalPerMonth,
            source.WaterPerAnimalPerMonth,
            source.ProducedPerAnimalPerMonth,
            source.FoodBufferCapacity,
            source.WaterBufferCapacity,
            source.CarcassBufferCapacity,
            source.ProducedBufferCapacity,
            source.AnimationParams,
            WithCustomIcon(
                source.Graphics,
                RecursiveIndustryIcons.MonitoredPoultryFarm)));
        LinkUpgrade(source, monitoredPoultryFarm, "Chicken Farm");
    }

    private static void LinkUpgrade<TProto>(
        TProto source,
        TProto target,
        string sourceName)
        where TProto : IProtoWithUpgrade
    {
        if (source.Upgrade.NextTier.HasValue)
        {
            throw new InvalidOperationException(
                sourceName + " already has a next-tier prototype; Adaptive Agrifood "
                + "cannot replace another mod's upgrade link.");
        }
        source.SetNextTier(target);
    }

    private static EntityCosts WithAutonomousHardware(
        EntityCosts source,
        ProductProto electronics,
        int electronicsQuantity,
        ProductProto controlPackages,
        int controlPackageQuantity,
        ProductProto programs,
        int programQuantity)
    {
        AssetValue construction = source.BaseConstructionCost
            + new AssetValue(electronics, electronicsQuantity.Quantity())
            + new AssetValue(
                controlPackages,
                controlPackageQuantity.Quantity())
            + new AssetValue(programs, programQuantity.Quantity());
        return new EntityCosts(
            construction,
            source.DefaultPriority,
            workers: 4,
            source.Maintenance);
    }

    private static FarmProto.Gfx WithCustomIcon(
        FarmProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            source.CropPositions,
            source.SprinklerPrefabPath,
            source.SprinklerSoundPath,
            source.PrefabOrigin,
            Option<string>.Some(iconPath),
            source.Color,
            source.HideBlockedPortsIcon,
            source.VisualizedLayers,
            source.Categories,
            source.UseInstancedRendering,
            source.UseSemiInstancedRendering,
            source.DisableEmptyChildrenStripping);

    private static Mafi.Core.Entities.Static.Layout.LayoutEntityProto.Gfx
        WithCustomIcon(
            Mafi.Core.Entities.Static.Layout.LayoutEntityProto.Gfx source,
            string iconPath) =>
        new(
            source.PrefabPath,
            source.PrefabOrigin,
            Option<string>.Some(iconPath),
            source.Color,
            source.HideBlockedPortsIcon,
            source.VisualizedLayers,
            source.Categories,
            source.UseInstancedRendering,
            source.UseSemiInstancedRendering,
            maxRenderedLod: source.MaxRenderedLod,
            disableEmptyChildrenStripping: source.DisableEmptyChildrenStripping,
            removeUndergroundVertices: source.RemoveUndergroundVertices,
            yawForGeneratedIcon: source.YawForGeneratedIcon,
            canBePickedUnderground: source.CanBePickedUnderground,
            doNotFlipModel: source.DoNotFlipModel);
}