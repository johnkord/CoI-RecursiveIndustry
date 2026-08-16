using System;
using Mafi;
using Mafi.Base;
using Mafi.Core.Buildings.VehicleDepots;
using Mafi.Core.Economy;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Vehicles.Excavators;
using Mafi.Core.Vehicles.TreeHarvesters;
using Mafi.Core.Vehicles.TreePlanters;
using Mafi.Core.Vehicles.Trucks;

namespace RecursiveIndustry;

internal sealed class AutonomousIndustrialVehiclesData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        ProductProto electronics = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(Ids.Products.Electronics4);
        ProductProto controlPackages = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(
                RecursiveIndustryIds.Products.ValidatedControlPackage);
        VehicleDepotProto depot = registrator.PrototypesDb
            .GetOrThrow<VehicleDepotProto>(Ids.Buildings.VehiclesDepotT3);

        depot.AddBuildableEntity(RegisterAmphibiousHauler(
            registrator,
            electronics,
            controlPackages));
        depot.AddBuildableEntity(RegisterExcavator(
            registrator,
            Ids.Vehicles.ExcavatorAmphibiousH,
            RecursiveIndustryIds.Vehicles.AutonomousAmphibiousExcavator,
            "Autonomous Amphibious Excavator",
            "A zero-worker hydrogen amphibious excavator that retains native land, water, and underwater mining behavior.",
            RecursiveIndustryIcons.AutonomousAmphibiousExcavator,
            electronics,
            60,
            controlPackages,
            12,
            IsAmphibiousTruckSupported));
        depot.AddBuildableEntity(RegisterExcavator(
            registrator,
            Ids.Vehicles.ExcavatorT3H,
            RecursiveIndustryIds.Vehicles.AutonomousMegaExcavator,
            "Autonomous Mega Excavator",
            "A zero-worker hydrogen Mega Excavator with embodied control hardware and native mining behavior.",
            RecursiveIndustryIcons.AutonomousMegaExcavator,
            electronics,
            80,
            controlPackages,
            16,
            IsMegaTruckSupported));
        depot.AddBuildableEntity(RegisterLargeTreeHarvester(
            registrator,
            electronics,
            controlPackages));
        depot.AddBuildableEntity(RegisterTreePlanter(
            registrator,
            electronics,
            controlPackages));
    }

    private static TruckProto RegisterAmphibiousHauler(
        ProtoRegistrator registrator,
        ProductProto electronics,
        ProductProto controlPackages)
    {
        TruckProto source = registrator.PrototypesDb
            .GetOrThrow<TruckProto>(Ids.Vehicles.TruckAmphibiousH);

        return registrator.PrototypesDb.Add(new TruckProto(
            RecursiveIndustryIds.Vehicles.AutonomousAmphibiousHauler,
            Proto.CreateStr(
                RecursiveIndustryIds.Vehicles.AutonomousAmphibiousHauler,
                "Autonomous Amphibious Hauler",
                "A zero-worker hydrogen truck for unit, loose, and fluid logistics across both land and water."),
            source.EntitySize,
            WithAutonomousHardware(
                source.Costs,
                electronics,
                60,
                controlPackages,
                12),
            source.VehicleQuotaCost,
            FasterDriving(source.DrivingData),
            source.CapacityBase,
            SupportsTruckProduct,
            source.DumpedThicknessByDistance,
            source.Attachments,
            source.AttachmentWhenEmpty,
            source.CargoPickupDuration,
            source.MinDumpingDistance,
            source.MaxDumpingDistance,
            CloneFuelTank(
                source.FuelTankProto,
                RecursiveIndustryIds.Vehicles.AutonomousAmphibiousHauler),
            source.PathFindingParams,
            source.DisruptionByDistance,
            source.BuildDurationTotal,
            source.VehicleGroup,
            source.NextTier,
            WithCustomIcon(
                source.Graphics,
                RecursiveIndustryIcons.AutonomousAmphibiousHauler),
            source.ProductType));
    }

    private static ExcavatorProto RegisterExcavator(
        ProtoRegistrator registrator,
        Proto.ID sourceId,
        ExcavatorProto.ID id,
        string name,
        string description,
        string iconPath,
        ProductProto electronics,
        int electronicsQuantity,
        ProductProto controlPackages,
        int controlPackageQuantity,
        Func<TruckProto, bool> isTruckSupported)
    {
        ExcavatorProto source = registrator.PrototypesDb
            .GetOrThrow<ExcavatorProto>(sourceId);

        return registrator.PrototypesDb.Add(new ExcavatorProto(
                id,
                Proto.CreateStr(id, name, description),
                source.EntitySize,
                WithAutonomousHardware(
                    source.Costs,
                    electronics,
                    electronicsQuantity,
                    controlPackages,
                    controlPackageQuantity),
                FasterDriving(source.DrivingData),
                source.RotatingCabinDriverProto,
                source.Capacity,
                source.MinMiningDistance,
                source.MaxMiningDistance,
                source.MinedThicknessByDistance,
                source.MineAnimations,
                source.DumpAnimations,
                CloneFuelTank(source.FuelTankProto, id),
                source.PathFindingParams,
                source.DisruptionByDistance,
                source.BuildDurationTotal,
                isTruckSupported,
                WithCustomIcon(source.Graphics, iconPath),
                source.NextTier,
                source.VehicleQuotaCost,
                source.MaxMiningOceanDepth));
    }

    private static TreeHarvesterProto RegisterLargeTreeHarvester(
        ProtoRegistrator registrator,
        ProductProto electronics,
        ProductProto controlPackages)
    {
        TreeHarvesterProto source = registrator.PrototypesDb
            .GetOrThrow<TreeHarvesterProto>(Ids.Vehicles.TreeHarvesterT2H);

        return registrator.PrototypesDb.Add(new TreeHarvesterProto(
            RecursiveIndustryIds.Vehicles.AutonomousLargeTreeHarvester,
            Proto.CreateStr(
                RecursiveIndustryIds.Vehicles.AutonomousLargeTreeHarvester,
                "Autonomous Large Tree Harvester",
                "A zero-worker hydrogen large harvester that retains native forestry designations and service-truck loading."),
            source.EntitySize,
            WithAutonomousHardware(
                source.Costs,
                electronics,
                60,
                controlPackages,
                12),
            source.VehicleQuotaCost,
            FasterDriving(source.DrivingData),
            source.RotatingCabinDriverProto,
            CloneFuelTank(
                source.FuelTankProto,
                RecursiveIndustryIds.Vehicles.AutonomousLargeTreeHarvester),
            source.TreeHarvestDistance,
            source.HarvestTimings,
            source.PathFindingParams,
            source.DisruptionByDistance,
            source.BuildDurationTotal,
            IsTreeTruckSupported,
            source.NextTier,
            WithCustomIcon(
                source.Graphics,
                RecursiveIndustryIcons.AutonomousLargeTreeHarvester)));
    }

    private static TreePlanterProto RegisterTreePlanter(
        ProtoRegistrator registrator,
        ProductProto electronics,
        ProductProto controlPackages)
    {
        TreePlanterProto source = registrator.PrototypesDb
            .GetOrThrow<TreePlanterProto>(Ids.Vehicles.TreePlanterT1H);

        return registrator.PrototypesDb.Add(new TreePlanterProto(
            RecursiveIndustryIds.Vehicles.AutonomousTreePlanter,
            Proto.CreateStr(
                RecursiveIndustryIds.Vehicles.AutonomousTreePlanter,
                "Autonomous Tree Planter",
                "A zero-worker hydrogen tree planter that retains native sapling logistics and planting designations."),
            source.EntitySize,
            WithAutonomousHardware(
                source.Costs,
                electronics,
                40,
                controlPackages,
                8),
            source.VehicleQuotaCost,
            FasterDriving(source.DrivingData),
            source.RotatingCabinDriverProto,
            CloneFuelTank(
                source.FuelTankProto,
                RecursiveIndustryIds.Vehicles.AutonomousTreePlanter),
            source.TreePlantDistance,
            source.PlantingTimings,
            source.CargoPickupDuration,
            source.ProductProto,
            source.Capacity,
            source.PathFindingParams,
            source.DisruptionByDistance,
            source.BuildDurationTotal,
            source.NextTier,
            WithCustomIcon(
                source.Graphics,
                RecursiveIndustryIcons.AutonomousTreePlanter)));
    }

    private static bool SupportsTruckProduct(ProductProto product) =>
        product is CountableProductProto
        || product is LooseProductProto
        || product is FluidProductProto;

    private static bool IsAmphibiousTruckSupported(TruckProto truck) =>
        (!truck.ProductType.HasValue
            || truck.ProductType.Value.Matches(LooseProductProto.ProductType))
        && truck.IsAmphibious;

    private static bool IsMegaTruckSupported(TruckProto truck) =>
        (!truck.ProductType.HasValue
            || truck.ProductType.Value.Matches(LooseProductProto.ProductType))
        && truck.Id != Ids.Vehicles.TruckT1;

    private static bool IsTreeTruckSupported(TruckProto truck) =>
        !truck.IsAmphibious
        && (!truck.ProductType.HasValue
            || truck.ProductType.Value.Matches(
                CountableProductProto.ProductType));

    private static DrivingData FasterDriving(DrivingData source) =>
        new(
            source.MaxForwardsSpeed.ScaledBy(125.Percent()),
            source.MaxBackwardsSpeed.ScaledBy(125.Percent()),
            source.SteeringSpeedMult,
            source.Acceleration.ScaledBy(125.Percent()),
            source.Braking.ScaledBy(125.Percent()),
            source.MaxSteeringAngle,
            source.MaxSteeringSpeed,
            source.BrakingConservativness,
            source.SteeringAxleOffset,
            source.NonSteeringAxleOffset,
            source.IndependentVelocityInertia,
            source.MaxSidewaysSpeed);

    private static EntityCosts WithAutonomousHardware(
        EntityCosts source,
        ProductProto electronics,
        int electronicsQuantity,
        ProductProto controlPackages,
        int controlPackageQuantity)
    {
        AssetValue construction = source.BaseConstructionCost
            + new AssetValue(electronics, electronicsQuantity.Quantity())
            + new AssetValue(
                controlPackages,
                controlPackageQuantity.Quantity());
        return new EntityCosts(
            construction,
            source.DefaultPriority,
            workers: 0,
            source.Maintenance);
    }

    private static Option<FuelTankProto> CloneFuelTank(
        Option<FuelTankProto> source,
        Proto.ID vehicleId)
    {
        if (!source.HasValue)
        {
            return Option<FuelTankProto>.None;
        }

        FuelTankProto tank = source.Value;
        return new FuelTankProto(
            new Proto.ID(
                "FuelTank_" + vehicleId.Value + "_" + tank.Product.Id.Value),
            tank.Product,
            tank.WasteProduct,
            tank.PollutionPercent,
            tank.Capacity,
            tank.Duration,
            tank.ReserveDuration,
            tank.IdleFuelConsumption,
            tank.QuickRefuelCostPerQuantity,
            tank.QuickRefuelHandlingCost);
    }

    private static ExcavatorProto.Gfx WithCustomIcon(
        ExcavatorProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            source.FrontContactPtsOffset,
            source.RearContactPtsOffset,
            source.DustParticles,
            source.ExhaustParticlesSpec,
            source.EngineSoundPath,
            source.MovementSoundPath,
            source.CabinModelName,
            source.LeftTrackModelName,
            source.RightTrackModelName,
            source.SpacingBetweenTracks,
            source.TrackTextureLength,
            source.IdleStateName,
            source.PileParentPath,
            source.PileModelName,
            source.DigSounds,
            source.DumpSounds,
            Option<string>.Some(iconPath),
            source.CabinAngleCompensation);

    private static TruckProto.Gfx WithCustomIcon(
        TruckProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            Option<string>.Some(iconPath),
            source.FrontContactPtsOffset,
            source.RearContactPtsOffset,
            source.DustParticles,
            source.ExhaustParticlesSpec,
            source.EngineSoundPath,
            source.MovementSoundPath,
            source.SteeringWheelsSubmodelPaths,
            source.WheelDiameter,
            source.StaticWheelsSubmodelPaths,
            source.LeftTrackModelName,
            source.RightTrackModelName,
            source.SpacingBetweenTracks,
            source.TrackTextureLength);

    private static TreeHarvesterProto.Gfx WithCustomIcon(
        TreeHarvesterProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            Option<string>.Some(iconPath),
            source.FrontContactPtsOffset,
            source.RearContactPtsOffset,
            source.DustParticles,
            source.ExhaustParticlesSpec,
            source.EngineSoundPath,
            source.MovementSoundPath,
            source.CabinObjectPath,
            source.LeftTrackObjectPath,
            source.RightTrackObjectPath,
            source.SpacingBetweenTracks,
            source.TrackTextureLength,
            source.TreeHolderOffset,
            source.GripperWidth,
            source.IdleAnimStateName,
            source.PreparedForHarvestAnimStateName,
            source.TreeLayingDownAnimStateName,
            source.TreeAboveTruckAnimStateName,
            source.TreeOnTruckAnimStateName,
            source.TreeFromTruckAnimStateName,
            source.FoldedAnimStateName,
            source.HarvestedTreeParentObjectPath,
            source.RotatingHandObjectPath);

    private static TreePlanterProto.Gfx WithCustomIcon(
        TreePlanterProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            Option<string>.Some(iconPath),
            source.FrontContactPtsOffset,
            source.RearContactPtsOffset,
            source.DustParticles,
            source.ExhaustParticlesSpec,
            source.EngineSoundPath,
            source.MovementSoundPath,
            source.CabinObjectPath,
            source.LeftTrackObjectPath,
            source.RightTrackObjectPath,
            source.TreesBaseObjectPath,
            source.NumTrees,
            source.SpacingBetweenTracks,
            source.TrackTextureLength,
            source.IdleAnimStateName,
            source.PlantingAnimStateName);
}