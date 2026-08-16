using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class RackGenerationData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RegisterRack(
            registrator,
            RecursiveIndustryIds.ServerRacks.RackI,
            "Accelerator Rack I",
            "Dense accelerator hardware for assisted AI workloads and AI Operations I.",
            RecursiveIndustryIds.Products.AcceleratorRackI,
            RecursiveIndustryIcons.AcceleratorRackI,
            16,
            240,
            1.5,
            2);
        RegisterRack(
            registrator,
            RecursiveIndustryIds.ServerRacks.RackII,
            "Frontier Rack II",
            "Agentic-generation hardware for concurrent services and AI Operations II.",
            RecursiveIndustryIds.Products.FrontierRackII,
            RecursiveIndustryIcons.FrontierRackII,
            64,
            600,
            4,
            5);
        RegisterRack(
            registrator,
            RecursiveIndustryIds.ServerRacks.RackIII,
            "Recursive Rack III",
            "Post-frontier hardware for recursive operations and hardware-software co-design.",
            RecursiveIndustryIds.Products.RecursiveRackIII,
            RecursiveIndustryIcons.RecursiveRackIII,
            256,
            1500,
            10,
            12);
    }

    private static void RegisterRack(
        ProtoRegistrator registrator,
        Proto.ID id,
        string name,
        string description,
        ProductProto.ID productId,
        string iconPath,
        int computing,
        int powerKw,
        double coolant,
        int maintenance)
    {
        ProductProto rackProduct = registrator.PrototypesDb.GetOrThrow<ProductProto>(productId);
        ProductProto spentProduct = registrator.PrototypesDb.GetOrThrow<ProductProto>(
            RecursiveIndustryIds.Products.SpentAccelerator);

        registrator.PrototypesDb.Add(new ServerRackProto(
            id,
            Proto.CreateStr(
                id,
                name,
                description,
                "title and description of a Recursive Industry server rack"),
            powerKw.Kw(),
            Computing.FromTFlops(computing),
            new ProductQuantity(rackProduct, new Quantity(1)),
            new ProductQuantity(spentProduct, new Quantity(1)),
            new PartialQuantity(coolant.ToFix32()),
            new PartialQuantity(coolant.ToFix32()),
            new PartialQuantity(maintenance.ToFix32()),
            new ServerRackProto.Gfx(
                iconPath,
                Assets.Base.Buildings.DataCenter.Rack_prefab,
                ImmutableArray.Create(
                    "DataCenter_Rack1",
                    "DataCenter_Rack2",
                    "DataCenter_Rack3",
                    "DataCenter_Rack4"))));
    }
}