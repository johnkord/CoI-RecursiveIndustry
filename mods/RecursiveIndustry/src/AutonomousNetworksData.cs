using System;
using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Buildings.VehicleDepots;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Factory;
using Mafi.Core.Mods;
using Mafi.Core.PathFinding;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Roads;
using Mafi.Core.Vehicles;
using Mafi.Core.Vehicles.Trucks;
using Mafi.Localization;

namespace RecursiveIndustry;

internal sealed class AutonomousNetworksData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RegisterFleetOptimization(registrator);
        RegisterPredictiveMaintenance(registrator);
        RegisterAutonomousHauler(registrator);
        RegisterAutonomousSpecializedHaulers(registrator);
    }

    private static void RegisterFleetOptimization(ProtoRegistrator registrator)
    {
        Percent fuelPerStep = -1.Percent();
        Percent throughputPerStep = 2.5.Percent();
        LocStr2 description = Loc.Str2(
            RecursiveIndustryIds.Focuses.FleetOptimization + "__desc",
            "{0} vehicle/train/ship fuel and +{1} truck/train capacity",
            "Post-native fleet optimization Focus; values are signed percentages.");

        LocStrFormatted Describe(int step) => description.Format(
            (step * fuelPerStep).ToStringRounded(0),
            (step * throughputPerStep).ToStringRounded(0));

        registrator.PrototypesDb.Add(new OfficeFocusWithPropertiesProto(
            RecursiveIndustryIds.Focuses.FleetOptimization,
            maxStep: 20,
            ImmutableArray.Create(
                Make.Kvp(
                    IdsCore.PropertyIds.VehiclesFuelConsumptionMultiplier,
                    fuelPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.TrainsFuelConsumptionMultiplier,
                    fuelPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.ShipsFuelConsumptionMultiplier,
                    fuelPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.TrucksCapacityMultiplier,
                    throughputPerStep),
                Make.Kvp(
                    IdsCore.PropertyIds.TrainsCapacityMultiplier,
                    throughputPerStep)),
            Describe,
            baseCost: 1000,
            costIncrement: 400,
            new OfficeFocusProto.Gfx(
                RecursiveIndustryIcons.FleetOptimization)));
    }

    private static void RegisterPredictiveMaintenance(ProtoRegistrator registrator)
    {
        Percent effectPerStep = -5.Percent();
        LocStr1 description = Loc.Str1(
            RecursiveIndustryIds.Focuses.PredictiveMaintenance + "__desc",
            "{0} to maintenance consumption",
            "Post-native predictive maintenance Focus; {0} is a signed percentage.");

        LocStrFormatted Describe(int step) =>
            description.Format((step * effectPerStep).ToStringRounded(0));

        registrator.PrototypesDb.Add(new OfficeFocusWithPropertiesProto(
            RecursiveIndustryIds.Focuses.PredictiveMaintenance,
            maxStep: 10,
            ImmutableArray.Create(Make.Kvp(
                IdsCore.PropertyIds.MaintenanceConsumptionMultiplier,
                effectPerStep)),
            Describe,
            baseCost: 2000,
            costIncrement: 1000,
            new OfficeFocusProto.Gfx(
                RecursiveIndustryIcons.PredictiveMaintenance)));
    }

    private static void RegisterAutonomousHauler(ProtoRegistrator registrator)
    {
        DynamicEntityProto.ID vehicleId =
            RecursiveIndustryIds.Vehicles.AutonomousHauler;
        VehicleGroupProto vehicleGroup = registrator.PrototypesDb
            .GetOrThrow<VehicleGroupProto>(Ids.VehicleGroups.TrucksT2);
        EntityCostsTpl costs = Costs.Build
            .Product(90, Ids.Products.VehicleParts3)
            .Product(30, Ids.Products.Rubber)
            .Product(40, Ids.Products.Electronics4)
            .Product(8, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .Workers(0)
            .Priority(0)
            .MaintenanceT1(18.0 / 5.0);
        Func<FuelTankProtoBuilder, FuelTankProto> fuelTank = tankBuilder =>
            tankBuilder.Start(vehicleId)
                .SetReserve(2.Minutes())
                .SetProduct(
                    Ids.Products.Hydrogen,
                    new Quantity(15).ScaledBy(125.Percent()),
                    14.Minutes())
                .BuildTank();

        TruckProto hauler = registrator.TruckProtoBuilder
            .Start("Autonomous Hauler", vehicleId)
            .Description("A zero-worker general cargo truck with embodied control hardware, native logistics behavior, and separately funded fleet optimization.")
            .SetCosts(costs)
            .SetDurationToBuild(120.Seconds())
            .SetCapacity(60)
            .SetTransferDurationScale(Fix32.Half)
            .SetDumpingDistance(3.Tiles(), 8.Tiles())
            .SetSizeInMeters(10.0, 2.5, 2.0)
            .SetWheelDiameter(0.78.Meters())
            .SetDrivingData(new DrivingData(
                1.25.Tiles(),
                0.75.Tiles(),
                50.Percent(),
                0.075.Tiles(),
                0.125.Tiles(),
                60.Degrees(),
                25.Degrees(),
                2.5.ToFix32(),
                2.3.Tiles(),
                0.5.Tiles()))
            .SetPathFindingParams(new VehiclePathFindingParams(
                3.Tiles(),
                SteepnessPathability.SlightSlopeAllowed,
                HeightClearancePathability.Require2TilesClearance,
                100.Percent(),
                RoadLaneType.MaskTwoTileLane))
            .SetDisruptionByDistance(22, 10)
            .SetPrefabPath(
                "Assets/Base/Vehicles/ModularTruck/TruckBaseHydrogen.prefab")
            .SetCustomIconPath(RecursiveIndustryIcons.AutonomousHauler)
            .SetTerrainContactPointsOffsets(
                new RelTile2f(4.6.Meters(), 1.1.Meters()),
                new RelTile2f(1.0.Meters(), 1.1.Meters()))
            .SetSteeringWheelsSubmodelPaths(
                "wheel_front_left",
                "wheel_front_right")
            .SetStaticWheelsSubmodelPaths(
                "wheel_middle_left",
                "wheel_middle_right",
                "wheel_back1_left",
                "wheel_back1_right",
                "wheel_back2_left",
                "wheel_back2_right")
            .SetDumpedThicknessByDistanceMeters(1.5f, 1.2f, 0.6f, 0.4f)
            .SetFuelTank(fuelTank)
            .AddDustSource(new DynamicEntityDustParticlesSpec(
                "Assets/Base/Vehicles/Dust/VehicleDustParticleSystem.prefab",
                1.8f,
                new RelTile3f(0, 0, 0),
                50f,
                0.3.Tiles()))
            .AddExhaustSources(Option<VehicleExhaustParticlesSpec>.None)
            .SetEngineSound(
                "Assets/Base/Vehicles/ModularTruck/Audio/Engine.prefab")
            .AddAttachment(new TankAttachmentProto(
                new Proto.ID(vehicleId + "_AttachmentTank"),
                product => product is FluidProductProto,
                new TankAttachmentProto.Gfx(
                    "Assets/Base/Vehicles/ModularTruck/T2-tank.prefab",
                    "icons",
                    "T2-tank",
                    ColorRgba.Gray,
                    ColorRgba.DarkGray),
                keepOnEvenIfNotNeeded: false))
            .AddAttachment(new FlatBedAttachmentProto(
                new Proto.ID(vehicleId + "_AttachmentFlatBed"),
                product => product is CountableProductProto,
                new FlatBedAttachmentProto.Gfx(
                    FlatBedAttachmentProto.Gfx.ProductOffsetsTruckT2(),
                    "Assets/Base/Vehicles/ModularTruck/Truck_Flat.prefab"),
                keepOnEvenIfNotNeeded: true))
            .AddAttachment(new DumpAttachmentProto(
                new Proto.ID(vehicleId + "_AttachmentDump"),
                new DumpAttachmentProto.Gfx(
                    "Assets/Base/Vehicles/ModularTruck/Truck_Dump.prefab",
                    "Object010/PileSmooth",
                    "Object010/PileRough",
                    LoosePileTextureParams.Default,
                    new Vector3f(2.6.ToFix32(), 0.2.ToFix32(), 0),
                    new Vector3f(2.6.ToFix32(), 1.9.ToFix32(), 0))))
            .SetVehicleGroup(vehicleGroup)
            .BuildAndAdd();

        registrator.PrototypesDb
            .GetOrThrow<VehicleDepotProto>(Ids.Buildings.VehiclesDepotT3)
            .AddBuildableEntity(hauler);
    }

    private static void RegisterAutonomousSpecializedHaulers(
        ProtoRegistrator registrator)
    {
        DynamicEntityProto.ID dumpId =
            RecursiveIndustryIds.Vehicles.AutonomousDumpHauler;
        TruckProto dumpHauler = BuildAutonomousT3Hauler(
            registrator,
            dumpId,
            "Autonomous Dump Hauler",
            "A zero-worker loose-cargo haul truck with onboard control hardware and native logistics behavior.",
            RecursiveIndustryIcons.AutonomousDumpHauler,
            LooseProductProto.ProductType,
            new DumpAttachmentProto(
                new Proto.ID(dumpId + "_AttachmentDump"),
                new DumpAttachmentProto.Gfx(
                    "Assets/Base/Vehicles/ModularTruckT3/TruckT3DumpHydrogen.prefab",
                    "dump/PileSmooth",
                    "dump/PileSmooth",
                    LoosePileTextureParams.Default,
                    "Main")));

        DynamicEntityProto.ID tankId =
            RecursiveIndustryIds.Vehicles.AutonomousTankHauler;
        TruckProto tankHauler = BuildAutonomousT3Hauler(
            registrator,
            tankId,
            "Autonomous Tank Hauler",
            "A zero-worker fluid-cargo haul truck with onboard control hardware and native logistics behavior.",
            RecursiveIndustryIcons.AutonomousTankHauler,
            FluidProductProto.ProductType,
            new TankAttachmentProto(
                new Proto.ID(tankId + "_AttachmentTank"),
                product => product is FluidProductProto,
                new TankAttachmentProto.Gfx(
                    "Assets/Base/Vehicles/ModularTruckT3/T3-tank.prefab",
                    "icons",
                    "T3-tank",
                    new ColorRgba(0.63f, 0.51f, 0.24f),
                    ColorRgba.DarkDarkGray),
                keepOnEvenIfNotNeeded: true));

        VehicleDepotProto depot = registrator.PrototypesDb
            .GetOrThrow<VehicleDepotProto>(Ids.Buildings.VehiclesDepotT3);
        depot.AddBuildableEntity(dumpHauler);
        depot.AddBuildableEntity(tankHauler);
    }

    private static TruckProto BuildAutonomousT3Hauler(
        ProtoRegistrator registrator,
        DynamicEntityProto.ID vehicleId,
        string name,
        string description,
        string customIconPath,
        ProductType productType,
        AttachmentProto attachment)
    {
        VehicleGroupProto vehicleGroup = registrator.PrototypesDb
            .GetOrThrow<VehicleGroupProto>(Ids.VehicleGroups.TrucksT3);
        EntityCostsTpl costs = Costs.Build
            .Product(140, Ids.Products.VehicleParts3)
            .Product(90, Ids.Products.Rubber)
            .Product(60, Ids.Products.Electronics4)
            .Product(12, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .Workers(0)
            .Priority(0)
            .MaintenanceT2(18.0 / 5.0);
        Func<FuelTankProtoBuilder, FuelTankProto> fuelTank = tankBuilder =>
            tankBuilder.Start(vehicleId)
                .SetReserve(2.Minutes())
                .SetProduct(
                    Ids.Products.Hydrogen,
                    new Quantity(36).ScaledBy(125.Percent()),
                    16.Minutes())
                .BuildTank();

        return registrator.TruckProtoBuilder
            .Start(name, vehicleId)
            .Description(description)
            .SetCosts(costs)
            .SetDurationToBuild(240.Seconds())
            .SetCapacity(180)
            .SetTransferDurationScale(Fix32.Quarter)
            .SetDumpingDistance(4.Tiles(), 9.Tiles())
            .SetSizeInMeters(12.0, 7.0, 6.0)
            .SetWheelDiameter(3.8.Meters())
            .SetDrivingData(new DrivingData(
                0.875.Tiles(),
                0.625.Tiles(),
                50.Percent(),
                0.0225.Tiles(),
                0.075.Tiles(),
                60.Degrees(),
                15.Degrees(),
                3.0.ToFix32(),
                1.5.Tiles(),
                1.5.Tiles()))
            .SetPathFindingParams(new VehiclePathFindingParams(
                5.Tiles(),
                SteepnessPathability.SlightSlopeAllowed,
                HeightClearancePathability.Require4TilesClearance,
                100.Percent(),
                RoadLaneType.MaskFourTileLane))
            .SetDisruptionByDistance(0, 32, 32)
            .SetPrefabPath(
                "Assets/Base/Vehicles/ModularTruckT3/TruckT3BaseHydrogen.prefab")
            .SetCustomIconPath(customIconPath)
            .SetTerrainContactPointsOffsets(
                new RelTile2f(3.0.Meters(), 2.5.Meters()),
                new RelTile2f(3.0.Meters(), 2.5.Meters()))
            .SetSteeringWheelsSubmodelPaths(
                "wheel_front_left",
                "wheel_front_right")
            .SetStaticWheelsSubmodelPaths("wheel_back")
            .SetDumpedThicknessByDistanceMeters(1.5f, 1.5f, 1.2f, 0.8f)
            .SetFuelTank(fuelTank)
            .AddDustSource(new DynamicEntityDustParticlesSpec(
                "Assets/Base/Vehicles/Dust/VehicleDustParticleSystem.prefab",
                2.5f,
                new RelTile3f(0, -1.5.ToFix32(), 0),
                50f,
                0.2.Tiles()))
            .AddDustSource(new DynamicEntityDustParticlesSpec(
                "Assets/Base/Vehicles/Dust/VehicleDustParticleSystem.prefab",
                2.5f,
                new RelTile3f(0, 1.5.ToFix32(), 0),
                50f,
                0.2.Tiles()))
            .AddExhaustSources(Option<VehicleExhaustParticlesSpec>.None)
            .SetEngineSound(
                "Assets/Base/Vehicles/ModularTruckT3/Audio/Engine.prefab")
            .SetFixedProductType(productType)
            .AddAttachment(attachment)
            .SetVehicleGroup(vehicleGroup)
            .BuildAndAdd();
    }
}