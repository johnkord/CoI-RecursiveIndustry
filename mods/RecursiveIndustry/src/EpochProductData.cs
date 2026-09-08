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
            CountableProductGraphics.WithCustomModel(
                spaceProbeParts,
                RecursiveIndustryIcons.FrontierProgram,
                "frontier_program")));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.FrontierExpansionProject,
            "Frontier Expansion Project",
            CountableProductGraphics.WithCustomModel(
                asteroidBoosterParts,
                RecursiveIndustryIcons.FrontierExpansionProject,
                "frontier_expansion_project")));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.OrbitalPowerCalibration,
            "Orbital Power Calibration",
            CountableProductGraphics.WithCustomModel(
                solarCellMono,
                RecursiveIndustryIcons.OrbitalPowerCalibration,
                "orbital_power_calibration")));
    }
}