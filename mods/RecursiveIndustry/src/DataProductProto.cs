using Mafi;
using Mafi.Core.Localization.Quantity;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

public sealed class DataProductProto : ProductProto
{
    public static readonly ProductType ProductType =
        new ProductType(typeof(DataProductProto));

    public DataProductProto(ID id, Proto.Str strings, ProductProto.Gfx graphics)
        : base(
            id,
            strings,
            3.Quantity(),
            isStorable: false,
            canBeDiscarded: false,
            isWaste: false,
            graphics,
            doNotNormalize: false,
            isExcludedFromStats: false,
            radioactivity: 0,
            pinToHomeScreenByDefault: false,
            isRecyclable: false,
            doNotTrackSourceProducts: true,
            quantityFormatter: NoUnitsQuantityFormatter.Instance)
    {
    }
}