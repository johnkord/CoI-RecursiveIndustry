using Mafi.Base;
using Mafi.Core.Mods;
using Mafi.Core.Products;

namespace RecursiveIndustry;

internal sealed class EpochProductData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        CountableProductProto spaceProbeParts =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(
                Ids.Products.SpaceProbeParts);
        CountableProductProto asteroidBoosterParts =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(
                Ids.Products.AsteroidBoosterParts);
        CountableProductProto solarCellMono =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(
                Ids.Products.SolarCellMono);

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.FrontierProgram,
            "Frontier Program",
            CountableProductGraphics.WithCustomIcon(
                spaceProbeParts,
                RecursiveIndustryIcons.FrontierProgram)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.FrontierExpansionProject,
            "Frontier Expansion Project",
            CountableProductGraphics.WithCustomIcon(
                asteroidBoosterParts,
                RecursiveIndustryIcons.FrontierExpansionProject)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.OrbitalPowerCalibration,
            "Orbital Power Calibration",
            CountableProductGraphics.WithCustomIcon(
                solarCellMono,
                RecursiveIndustryIcons.OrbitalPowerCalibration)));
    }
}