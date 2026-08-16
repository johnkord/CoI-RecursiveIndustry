using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Machines.PowerGenerators;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Animations;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class OrbitalPowerRelayData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RecursiveIndustry mod = (RecursiveIndustry)registrator.ActiveMod;
        int arraySupportSeconds =
            mod.JsonConfig.GetInt("orbital_power_array_seconds", 360);
        ProductProto dossier = registrator.PrototypesDb.GetOrThrow<ProductProto>(
            RecursiveIndustryIds.Products.ValidatedResearchDossier);
        ProductProto electricity = registrator.PrototypesDb.GetOrThrow<ProductProto>(
            IdsCore.Products.Electricity);
        EntityCostsTpl costsTemplate = Costs.Build
            .CP4(2400)
            .Product(512, Ids.Products.Electronics4)
            .Product(256, Ids.Products.SpaceStationParts2)
            .Product(
                64,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .Product(32, RecursiveIndustryIds.Products.FrontierProgram)
            .Product(
                1,
                RecursiveIndustryIds.Products.OrbitalPowerCalibration)
            .Workers(40)
            .MaintenanceT3(40);
        EntityCosts costs = costsTemplate.MapToEntityCosts(registrator);

        registrator.PrototypesDb.Add(new ElectricityGeneratorFromProductProto(
            RecursiveIndustryIds.Power.OrbitalPowerRelay,
            Proto.CreateStr(
                RecursiveIndustryIds.Power.OrbitalPowerRelay,
                "Orbital Power Relay",
                "Legacy 30 MW orbital receiver retained for existing saves. New campaigns unlock the Orbital Power Array instead.",
                "title and description of the Recursive Industry orbital power reward"),
            registrator.LayoutParser.ParseLayoutOrThrow(
                OrbitalPowerRelayLayout.Create()),
            costs,
            30000.Kw(),
            generationPriority: 10,
            dossier.WithQuantity(1),
            outputProduct: null,
            electricity,
            bufferCapacityMultiplier: 4,
            360.Seconds(),
            DestroyReason.UsedAsFuel,
            ImmutableArray<AnimationParams>.Empty,
            new ElectricityGeneratorFromProductProto.Gfx(
                OrbitalPowerRelayLayout.PrefabPath,
                ImmutableArray<ParticlesParams>.Empty,
                OrbitalPowerRelayLayout.SoundPath,
                registrator.GetCategoriesProtos(
                    Ids.ToolbarCategories.Power_General),
                customIconPath:
                    Option<string>.Some(RecursiveIndustryIcons.OrbitalPowerRelay),
                useSemiInstancedRendering: true)));

        EntityCostsTpl arrayCostsTemplate = Costs.Build
            .CP4(4800)
            .Product(1024, Ids.Products.Electronics4)
            .Product(512, Ids.Products.SpaceStationParts2)
            .Product(
                128,
                RecursiveIndustryIds.Products.ValidatedControlPackage)
            .Product(64, RecursiveIndustryIds.Products.FrontierProgram)
            .Product(
                2,
                RecursiveIndustryIds.Products.OrbitalPowerCalibration)
            .Workers(80)
            .MaintenanceT3(80);
        EntityCosts arrayCosts = arrayCostsTemplate.MapToEntityCosts(registrator);

        registrator.PrototypesDb.Add(new ElectricityGeneratorFromProductProto(
            RecursiveIndustryIds.Power.OrbitalPowerArray,
            Proto.CreateStr(
                RecursiveIndustryIds.Power.OrbitalPowerArray,
                "Orbital Power Array",
                "Receives 240 MW of beamed orbital power while one renewable Dossier per support cycle sustains constellation targeting, calibration, and control.",
                "title and description of the Recursive Industry orbital power array"),
            registrator.LayoutParser.ParseLayoutOrThrow(
                OrbitalPowerRelayLayout.Create()),
            arrayCosts,
            240000.Kw(),
            generationPriority: 10,
            dossier.WithQuantity(1),
            outputProduct: null,
            electricity,
            bufferCapacityMultiplier: 4,
            arraySupportSeconds.Seconds(),
            DestroyReason.UsedAsFuel,
            ImmutableArray<AnimationParams>.Empty,
            new ElectricityGeneratorFromProductProto.Gfx(
                OrbitalPowerRelayLayout.PrefabPath,
                ImmutableArray<ParticlesParams>.Empty,
                OrbitalPowerRelayLayout.SoundPath,
                registrator.GetCategoriesProtos(
                    Ids.ToolbarCategories.Power_General),
                customIconPath:
                    Option<string>.Some(RecursiveIndustryIcons.OrbitalPowerRelay),
                useSemiInstancedRendering: true)));
    }
}