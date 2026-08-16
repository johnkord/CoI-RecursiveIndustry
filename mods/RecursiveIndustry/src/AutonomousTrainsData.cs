using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Economy;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Trains;
using Mafi.TrainsDlc;

namespace RecursiveIndustry;

internal sealed class AutonomousTrainsData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        if (!registrator.PrototypesDb.TryGetProto<LocomotiveProto>(
                Ids.Trains.LocomotiveT1Steam,
                out _))
        {
            Log.Info("RecursiveIndustry: Trains DLC prototypes absent; autonomous trains skipped");
            return;
        }

        ProductProto electronics = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(Ids.Products.Electronics4);
        ProductProto controlPackages = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(
                RecursiveIndustryIds.Products.ValidatedControlPackage);

        RegisterLocomotive(
            registrator,
            Ids.Trains.LocomotiveT1Steam,
            RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveI,
            "Autonomous Steam Locomotive I",
            "A zero-worker steam locomotive with embodied control hardware and native train scheduling.",
            RecursiveIndustryIcons.AutonomousSteamLocomotiveI,
            electronics,
            80,
            controlPackages,
            16);
        RegisterLocomotive(
            registrator,
            Ids.Trains.LocomotiveT2Steam,
            RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveII,
            "Autonomous Steam Locomotive II",
            "A zero-worker heavy steam locomotive with embodied control hardware and native train scheduling.",
            RecursiveIndustryIcons.AutonomousSteamLocomotiveII,
            electronics,
            160,
            controlPackages,
            32);
        RegisterLocomotive(
            registrator,
            Ids.Trains.LocomotiveT1Diesel,
            RecursiveIndustryIds.Trains.AutonomousDieselLocomotiveI,
            "Autonomous Diesel Locomotive I",
            "A zero-worker diesel locomotive with embodied control hardware and native train scheduling.",
            RecursiveIndustryIcons.AutonomousDieselLocomotiveI,
            electronics,
            80,
            controlPackages,
            16);
        RegisterLocomotive(
            registrator,
            Ids.Trains.LocomotiveT2Diesel,
            RecursiveIndustryIds.Trains.AutonomousDieselLocomotiveII,
            "Autonomous Diesel Locomotive II",
            "A zero-worker heavy diesel locomotive with embodied control hardware and native train scheduling.",
            RecursiveIndustryIcons.AutonomousDieselLocomotiveII,
            electronics,
            160,
            controlPackages,
            32);
        RegisterLocomotive(
            registrator,
            Ids.Trains.LocomotiveT1Hydrogen,
            RecursiveIndustryIds.Trains.AutonomousHydrogenLocomotiveI,
            "Autonomous Hydrogen Locomotive I",
            "A zero-worker hydrogen locomotive with embodied control hardware and native train scheduling.",
            RecursiveIndustryIcons.AutonomousHydrogenLocomotiveI,
            electronics,
            80,
            controlPackages,
            16);
        RegisterLocomotive(
            registrator,
            Ids.Trains.LocomotiveT2Hydrogen,
            RecursiveIndustryIds.Trains.AutonomousHydrogenLocomotiveII,
            "Autonomous Hydrogen Locomotive II",
            "A zero-worker heavy hydrogen locomotive with embodied control hardware and native train scheduling.",
            RecursiveIndustryIcons.AutonomousHydrogenLocomotiveII,
            electronics,
            160,
            controlPackages,
            32);
        RegisterElectricLocomotive(
            registrator,
            IdsTrainsDlc.LocomotiveT1Electric,
            RecursiveIndustryIds.Trains.AutonomousElectricLocomotiveI,
            "Autonomous Electric Locomotive I",
            "A zero-worker electric locomotive that retains native electrified-track power consumption.",
            RecursiveIndustryIcons.AutonomousElectricLocomotiveI,
            electronics,
            80,
            controlPackages,
            16);
        RegisterElectricLocomotive(
            registrator,
            IdsTrainsDlc.LocomotiveT2Electric,
            RecursiveIndustryIds.Trains.AutonomousElectricLocomotiveII,
            "Autonomous Electric Locomotive II",
            "A zero-worker heavy electric locomotive that retains native electrified-track power consumption.",
            RecursiveIndustryIcons.AutonomousElectricLocomotiveII,
            electronics,
            160,
            controlPackages,
            32);
        RegisterLocomotive(
            registrator,
            IdsTrainsDlc.LocomotiveT1FirelessSteam,
            RecursiveIndustryIds.Trains.AutonomousFirelessSteamLocomotive,
            "Autonomous Fireless Steam Locomotive",
            "A zero-worker fireless locomotive that retains native high-pressure steam fueling.",
            RecursiveIndustryIcons.AutonomousFirelessSteamLocomotive,
            electronics,
            60,
            controlPackages,
            12);
        RegisterLocomotive(
            registrator,
            IdsTrainsDlc.LocomotiveT2Turbine,
            RecursiveIndustryIds.Trains.AutonomousTurbineLocomotive,
            "Autonomous Turbine Locomotive",
            "A zero-worker gas-turbine locomotive that retains native Fuel Gas operation.",
            RecursiveIndustryIcons.AutonomousTurbineLocomotive,
            electronics,
            160,
            controlPackages,
            32);
        RegisterNuclearConsist(
            registrator,
            electronics,
            controlPackages);

        if (registrator.PrototypesDb.TryGetProto<LocomotiveProto>(
                IdsTrainsDlc.LocomotiveT1Captains,
                out _))
        {
            RegisterLocomotive(
                registrator,
                IdsTrainsDlc.LocomotiveT1Captains,
                RecursiveIndustryIds.Trains.AutonomousCaptainsLocomotive,
                "Autonomous Captain's Locomotive",
                "A zero-worker Captain's locomotive available when the Supporter edition is active.",
                RecursiveIndustryIcons.AutonomousCaptainsLocomotive,
                electronics,
                40,
                controlPackages,
                8);
        }

        ImmutableArray<TrainCarBaseProto.ID> steamFamily = ImmutableArray.Create(
            RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveI,
            RecursiveIndustryIds.Trains.AutonomousSteamLocomotiveII,
            RecursiveIndustryIds.Trains.AutonomousSteamTenderI,
            RecursiveIndustryIds.Trains.AutonomousSteamTenderII);
        RegisterTender(
            registrator,
            Ids.Trains.LocomotiveT1Tender,
            RecursiveIndustryIds.Trains.AutonomousSteamTenderI,
            "Autonomous Steam Tender I",
            "A steam tender compatible with autonomous steam locomotives.",
            RecursiveIndustryIcons.AutonomousSteamTenderI,
            steamFamily);
        RegisterTender(
            registrator,
            Ids.Trains.LocomotiveT2Tender,
            RecursiveIndustryIds.Trains.AutonomousSteamTenderII,
            "Autonomous Steam Tender II",
            "A steam tender compatible with autonomous steam locomotives.",
            RecursiveIndustryIcons.AutonomousSteamTenderII,
            steamFamily);

        ImmutableArray<TrainCarBaseProto.ID> turbineFamily = ImmutableArray.Create(
            RecursiveIndustryIds.Trains.AutonomousTurbineLocomotive,
            RecursiveIndustryIds.Trains.AutonomousTurbineTender);
        RegisterTender(
            registrator,
            IdsTrainsDlc.LocomotiveTurbineTender,
            RecursiveIndustryIds.Trains.AutonomousTurbineTender,
            "Autonomous Turbine Tender",
            "A Fuel Gas tender compatible with the autonomous Turbine Locomotive.",
            RecursiveIndustryIcons.AutonomousTurbineTender,
            turbineFamily);
    }

    private static LocomotiveProto RegisterLocomotive(
        ProtoRegistrator registrator,
        Proto.ID sourceId,
        LocomotiveProto.ID id,
        string name,
        string description,
        string iconPath,
        ProductProto electronics,
        int electronicsQuantity,
        ProductProto controlPackages,
        int controlPackageQuantity)
    {
        LocomotiveProto source = registrator.PrototypesDb
            .GetOrThrow<LocomotiveProto>(sourceId);

        return CloneLocomotive(
            registrator,
            source,
            id,
            name,
            description,
            iconPath,
            WithAutonomousHardware(
                source.Costs,
                electronics,
                electronicsQuantity,
                controlPackages,
                controlPackageQuantity),
            source.OnlyAllowedAtFront,
            source.OnlyAllowedAtRear);
    }

    private static ElectricLocomotiveProto RegisterElectricLocomotive(
        ProtoRegistrator registrator,
        Proto.ID sourceId,
        ElectricLocomotiveProto.ID id,
        string name,
        string description,
        string iconPath,
        ProductProto electronics,
        int electronicsQuantity,
        ProductProto controlPackages,
        int controlPackageQuantity)
    {
        ElectricLocomotiveProto source = registrator.PrototypesDb
            .GetOrThrow<ElectricLocomotiveProto>(sourceId);

        return registrator.PrototypesDb.Add(new ElectricLocomotiveProto(
            id,
            Proto.CreateStr(id, name, description),
            WithAutonomousHardware(
                source.Costs,
                electronics,
                electronicsQuantity,
                controlPackages,
                controlPackageQuantity),
            source.CarLength,
            source.BogiePivotsOffset,
            source.BogieWheelBase,
            source.MaxSpeed.ScaledBy(125.Percent()),
            source.MassTonsWhenEmpty,
            source.MassTonsWhenFull,
            source.BrakingForceKn,
            source.RollingResistanceCoefficientTimesThousand,
            source.FrontalAreaM2,
            source.LengthDragAsExtraFrontalArea,
            source.DragCoefficientStandalone,
            source.DragCoefficientInline,
            source.EnginePowerKw,
            source.StartingTractiveEffort,
            source.BuildDurationTotal,
            source.PowerRequired,
            WithCustomIcon(source.Graphics, iconPath),
            source.SubCarCount,
            source.RequiresAlignment,
            source.IgnoreFuelCostDuringConstruction,
            source.LocoTypeDigit,
            source.OnlyAllowedAtFront,
            source.OnlyAllowedAtRear));
    }

    private static void RegisterNuclearConsist(
        ProtoRegistrator registrator,
        ProductProto electronics,
        ProductProto controlPackages)
    {
        LocomotiveProto cab = registrator.PrototypesDb
            .GetOrThrow<LocomotiveProto>(IdsTrainsDlc.LocomotiveT2NuclearA);
        LocomotiveProto reactor = registrator.PrototypesDb
            .GetOrThrow<LocomotiveProto>(IdsTrainsDlc.LocomotiveT2NuclearB);
        LocomotiveProto condenser = registrator.PrototypesDb
            .GetOrThrow<LocomotiveProto>(IdsTrainsDlc.LocomotiveT2NuclearC);

        ImmutableArray<TrainCarBaseProto.ID> cabRear = ImmutableArray.Create(
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveReactor);
        ImmutableArray<TrainCarBaseProto.ID> reactorFront = ImmutableArray.Create(
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCab,
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCondenser);
        ImmutableArray<TrainCarBaseProto.ID> reactorRear = ImmutableArray.Create(
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCondenser);
        ImmutableArray<TrainCarBaseProto.ID> condenserFront = ImmutableArray.Create(
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveReactor);

        CloneLocomotive(
            registrator,
            cab,
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCab,
            "Autonomous Nuclear Locomotive (Cab)",
            "The zero-worker control cab for an autonomous nuclear locomotive consist.",
            RecursiveIndustryIcons.AutonomousNuclearLocomotiveCab,
            WithAutonomousHardware(
                cab.Costs,
                electronics,
                200,
                controlPackages,
                40),
            cab.OnlyAllowedAtFront,
            cabRear);
        CloneLocomotive(
            registrator,
            reactor,
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveReactor,
            "Autonomous Nuclear Locomotive (Reactor)",
            "The fueled reactor and traction module for an autonomous nuclear locomotive consist.",
            RecursiveIndustryIcons.AutonomousNuclearLocomotiveReactor,
            reactor.Costs,
            reactorFront,
            reactorRear);
        CloneLocomotive(
            registrator,
            condenser,
            RecursiveIndustryIds.Trains.AutonomousNuclearLocomotiveCondenser,
            "Autonomous Nuclear Locomotive (Condenser)",
            "The condenser module required behind an autonomous nuclear locomotive reactor.",
            RecursiveIndustryIcons.AutonomousNuclearLocomotiveCondenser,
            condenser.Costs,
            condenserFront,
            condenser.OnlyAllowedAtRear);
    }

    private static LocomotiveProto CloneLocomotive(
        ProtoRegistrator registrator,
        LocomotiveProto source,
        LocomotiveProto.ID id,
        string name,
        string description,
        string iconPath,
        EntityCosts costs,
        ImmutableArray<TrainCarBaseProto.ID> onlyAllowedAtFront,
        ImmutableArray<TrainCarBaseProto.ID> onlyAllowedAtRear) =>
        registrator.PrototypesDb.Add(new LocomotiveProto(
            id,
            Proto.CreateStr(id, name, description),
            costs,
            CloneFuelTank(source.LocomotiveFuelTankProto, id),
            source.CarLength,
            source.BogiePivotsOffset,
            source.BogieWheelBase,
            source.MaxSpeed.ScaledBy(125.Percent()),
            source.MassTonsWhenEmpty,
            source.MassTonsWhenFull,
            source.BrakingForceKn,
            source.RollingResistanceCoefficientTimesThousand,
            source.FrontalAreaM2,
            source.LengthDragAsExtraFrontalArea,
            source.DragCoefficientStandalone,
            source.DragCoefficientInline,
            source.EnginePowerKw,
            source.StartingTractiveEffort,
            source.BuildDurationTotal,
            source.OnlyRefuelIfUnder,
            WithCustomIcon(source.Graphics, iconPath),
            source.SubCarCount,
            source.RequiresAlignment,
            source.IgnoreFuelCostDuringConstruction,
            source.LocoTypeDigit,
            onlyAllowedAtFront,
            onlyAllowedAtRear));

    private static TenderWagonProto RegisterTender(
        ProtoRegistrator registrator,
        Proto.ID sourceId,
        TenderWagonProto.ID id,
        string name,
        string description,
        string iconPath,
        ImmutableArray<TrainCarBaseProto.ID> allowedAtFront)
    {
        TenderWagonProto source = registrator.PrototypesDb
            .GetOrThrow<TenderWagonProto>(sourceId);

        return registrator.PrototypesDb.Add(new TenderWagonProto(
            id,
            Proto.CreateStr(id, name, description),
            source.Costs,
            CloneFuelTank(source.LocomotiveFuelTankProto, id),
            source.CarLength,
            source.BogiePivotsOffset,
            source.BogieWheelBase,
            source.MaxSpeed,
            source.MassTonsWhenEmpty,
            source.MassTonsWhenFull,
            source.BrakingForceKn,
            source.RollingResistanceCoefficientTimesThousand,
            source.FrontalAreaM2,
            source.LengthDragAsExtraFrontalArea,
            source.DragCoefficientStandalone,
            source.DragCoefficientInline,
            source.EnginePowerKw,
            source.StartingTractiveEffort,
            source.BuildDurationTotal,
            WithCustomIcon(source.Graphics, iconPath),
            source.SubCarCount,
            source.RequiresAlignment,
            source.IgnoreFuelCostDuringConstruction,
            allowedAtFront,
            source.OnlyAllowedAtRear));
    }

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

    private static Option<LocomotiveFuelTankProto> CloneFuelTank(
        Option<LocomotiveFuelTankProto> source,
        TrainCarBaseProto.ID ownerId)
    {
        if (!source.HasValue)
        {
            return Option<LocomotiveFuelTankProto>.None;
        }

        LocomotiveFuelTankProto tank = source.Value;
        return new LocomotiveFuelTankProto(
            Ids.Trains.GetFuelTankId(ownerId, tank.Product.Id),
            tank.Product,
            tank.WasteProduct,
            tank.PollutionPercent,
            tank.Capacity,
            tank.Duration,
            tank.ReserveDuration,
            tank.PrimaryProductAmount,
            tank.QuickRefuelCostPerQuantity,
            tank.QuickRefuelHandlingCost,
            tank.SecondaryProduct,
            tank.SecondaryProductAmount);
    }

    private static LocomotiveProto.Gfx WithCustomIcon(
        LocomotiveProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            source.CarWidthMeters,
            source.CarHeightMeters,
            source.WheelRadiusMeters,
            source.DefaultColor,
            source.OrderInBuildMenu + 0.25f,
            iconPath,
            source.FrontBogieModelName,
            source.RearBogieModelName,
            source.FrontCouplerName,
            source.RearCouplerName,
            source.FrontCarConnectorName,
            source.RearCarConnectorName,
            source.FrontCarConnectorSize,
            source.RearCarConnectorSize,
            source.WheelModelPrefix,
            source.ExhaustParticlesSpec,
            source.UseAnimationForWheelMovement,
            source.UseAnimationForEngineThrottle,
            source.ThrottleAnimationSpeedIdle,
            source.ThrottleAnimationSpeedFullPower,
            source.AlterLocoAtAnimPercent,
            source.SignMeshPrefix,
            new LocoSoundSpecs(
                source.MotionSoundSpec,
                source.BrakingSoundSpec,
                source.StoppedSoundSpec,
                source.EngineIdleSoundSpec,
                source.EngineMovingSoundSpec,
                source.HornSoundSpec));

    private static TenderWagonProto.Gfx WithCustomIcon(
        TenderWagonProto.Gfx source,
        string iconPath) =>
        new(
            source.PrefabPath,
            source.CarWidthMeters,
            source.CarHeightMeters,
            source.WheelRadiusMeters,
            source.DefaultColor,
            source.OrderInBuildMenu + 0.25f,
            iconPath,
            source.FrontBogieModelName,
            source.RearBogieModelName,
            source.FrontCouplerName,
            source.RearCouplerName,
            source.FrontCarConnectorName,
            source.RearCarConnectorName,
            source.FrontCarConnectorSize,
            source.RearCarConnectorSize,
            source.WheelModelPrefix,
            source.ExhaustParticlesSpec,
            new TrainCarSoundSpecs(
                source.MotionSoundSpec,
                source.BrakingSoundSpec,
                source.StoppedSoundSpec),
            source.PileObjectPath,
            source.AnimationStateName,
            source.PileTextureParams);
}