using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Contracts;
using Mafi.Core.World.Entities;

namespace RecursiveIndustry;

internal sealed class WorldExchangeData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        RegisterContract(
            registrator,
            RecursiveIndustryIds.Contracts.VerifiedProcessLicensing,
            Ids.World.Settlement5,
            RecursiveIndustryIds.Products.ValidatedResearchDossier,
            1,
            Ids.Products.Sulfur,
            20);
        RegisterContract(
            registrator,
            RecursiveIndustryIds.Contracts.AcceleratorTakeBack,
            Ids.World.Settlement3,
            RecursiveIndustryIds.Products.SpentAccelerator,
            1,
            Ids.Products.Recyclables,
            12);
    }

    private static void RegisterContract(
        ProtoRegistrator registrator,
        Proto.ID contractId,
        Proto.ID villageId,
        ProductProto.ID productToPayWithId,
        int quantityToPayWith,
        ProductProto.ID productToBuyId,
        int quantityToBuy)
    {
        WorldMapVillageProto village = registrator.PrototypesDb
            .GetOrThrow<WorldMapVillageProto>(villageId);
        if (village.MaxReputation < 3)
        {
            throw new System.InvalidOperationException(
                $"Village '{village.Id}' cannot expose reputation-3 contract '{contractId}'.");
        }
        foreach (ContractProto existing in village.Contracts)
        {
            if (existing.Id.Value == contractId.Value)
            {
                throw new System.InvalidOperationException(
                    $"Contract '{contractId}' is already registered on village '{village.Id}'.");
            }
        }

        ProductProto productToPayWith = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(productToPayWithId);
        ProductProto productToBuy = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(productToBuyId);
        var contract = registrator.PrototypesDb.Add(new ContractProto(
            contractId,
            new ProductQuantity(productToBuy, quantityToBuy.Quantity()),
            new ProductQuantity(productToPayWith, quantityToPayWith.Quantity()),
            0.2.Upoints(),
            0.12.Upoints(),
            minReputationRequired: 3));
        village.Contracts = village.Contracts.Add(contract);
        Log.Info(
            "RecursiveIndustry: World Exchange registered: "
            + contract.Id.Value
            + ", village=" + village.Id.Value
            + ", pay=" + quantityToPayWith + " " + productToPayWith.Id.Value
            + ", buy=" + quantityToBuy + " " + productToBuy.Id.Value
            + ", reputation=3");
    }
}