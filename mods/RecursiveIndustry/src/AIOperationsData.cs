using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class AIOperationsData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        ProductProto controlPackage = registrator.PrototypesDb.GetOrThrow<ProductProto>(
            RecursiveIndustryIds.Products.ValidatedControlPackage);
        ProductProto recyclables = registrator.PrototypesDb.GetOrThrow<ProductProto>(
            Ids.Products.Recyclables);

        OfficeBuildingProto operationsI = OfficeBuildingFactory.Register(
            registrator,
            RecursiveIndustryIds.Offices.OperationsI,
            "AI Operations I",
            "Assisted operations convert validated control and 16 Computing into focused island coordination.",
            Ids.Buildings.OfficeBuildingT1,
            RecursiveIndustryIcons.AiOperationsI,
            controlPackage,
            recyclables,
            Costs.Build.CP4(80)
                .Workers(100)
                .MaintenanceT3(1)
                .Product(4, RecursiveIndustryIds.Products.AcceleratorModule)
                .Product(8, Ids.Products.Server)
                .Product(4, Ids.Products.Electronics3),
            250,
            16,
            1,
            150);
        OfficeBuildingProto operationsII = OfficeBuildingFactory.Register(
            registrator,
            RecursiveIndustryIds.Offices.OperationsII,
            "AI Operations II",
            "Agentic coordination supports deep sector transformation at high Computing and Package demand.",
            Ids.Buildings.OfficeBuildingT2,
            RecursiveIndustryIcons.AiOperationsII,
            controlPackage,
            recyclables,
            Costs.Build.CP4(160)
                .Workers(100)
                .MaintenanceT3(1)
                .Product(8, RecursiveIndustryIds.Products.AcceleratorModule)
                .Product(8, Ids.Products.Server)
                .Product(8, Ids.Products.Electronics4),
            400,
            64,
            4,
            2400);
        OfficeBuildingProto operationsIII = OfficeBuildingFactory.Register(
            registrator,
            RecursiveIndustryIds.Offices.OperationsIII,
            "AI Operations III",
            "Recursive operations create broad abundance while retaining physical deployment costs.",
            Ids.Buildings.OfficeBuildingT3,
            RecursiveIndustryIcons.AiOperationsIII,
            controlPackage,
            recyclables,
            Costs.Build.CP4(320)
                .Workers(100)
                .MaintenanceT3(1)
                .Product(16, RecursiveIndustryIds.Products.AcceleratorModule)
                .Product(16, Ids.Products.Server)
                .Product(16, Ids.Products.Electronics4),
            600,
            192,
            16,
            24900);

        operationsI.SetNextTier(operationsII);
        operationsII.SetNextTier(operationsIII);
    }
}