using Mafi;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class IndustrialControlProductData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        ProductProto.ID id = RecursiveIndustryIds.Products.IndustrialControlStream;
        registrator.PrototypesDb.Add(new DataProductProto(
            id,
            Proto.CreateStr(
                id,
                "Industrial Control Stream",
                "Authenticated telemetry, optimization, model health, and supervisory set points reserved over Data-only Fiber."),
            new ProductProto.Gfx(
                Option<string>.None,
                RecursiveIndustryIcons.IndustrialControlStream,
                color: new ColorRgba(0.20f, 0.80f, 1.00f),
                transportColor: new ColorRgba(0.12f, 0.62f, 0.86f),
                transportAccentColor: new ColorRgba(0.72f, 0.96f, 1.00f))));
    }
}