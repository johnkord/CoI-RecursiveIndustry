using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Entities.Animations;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class CompanionAnimalCareData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        CountableProductProto foodPack = registrator.PrototypesDb
            .GetOrThrow<CountableProductProto>(Ids.Products.FoodPack);
        CountableProductProto provisions = registrator.PrototypesDb.Add(
            new CountableProductProto(
                RecursiveIndustryIds.Products.CompanionProvisions,
                "Companion Provisions",
                CountableProductGraphics.WithCustomIcon(
                    foodPack,
                    RecursiveIndustryIcons.CompanionProvisions)));
        ProductProto waste = registrator.PrototypesDb
            .GetOrThrow<ProductProto>(Ids.Products.Waste);
        UpointsStatsCategoryProto services = registrator.PrototypesDb
            .GetOrThrow<UpointsStatsCategoryProto>(
                new Proto.ID("UpointsStatsCat_Services"));
        UpointsCategoryProto category = registrator.PrototypesDb.Add(
            new UpointsCategoryProto(
                RecursiveIndustryIds.Settlements.CompanionCareNeed,
                RecursiveIndustryIcons.CompanionAnimalCenter,
                services,
                hideCount: true));
        PopNeedProto need = registrator.PrototypesDb.Add(new PopNeedProto(
            RecursiveIndustryIds.Settlements.CompanionCareNeed,
            Proto.CreateStr(
                RecursiveIndustryIds.Settlements.CompanionCareNeed,
                "Companion animal care",
                "Optional staffed care, veterinary oversight, exercise, and safe access to companion animals. Supply grants modest Unity but no Health or worker-productivity bonus."),
            0.6.Upoints(),
            category,
            healthGiven: null,
            consumptionMultiplierProperty: null,
            unityMultiplierProperty: null,
            graphics: new PopNeedProto.Gfx(
                RecursiveIndustryIcons.CompanionAnimalCenter)));

        registrator.SettlementModuleProtoBuilder
            .Start(
                "Companion Animal Center",
                RecursiveIndustryIds.Settlements.CompanionAnimalCenter)
            .Description(
                "Provides optional companion-animal care to the attached settlement. It consumes packaged provisions, employs accountable care workers, and returns ordinary Waste.")
            .SetNeed(need)
            .SetCost(Costs.Build
                .CP4(240)
                .Product(32, Ids.Products.Electronics4)
                .Product(
                    8,
                    RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Workers(8)
                .MaintenanceT2(4))
            .SetElectricityConsumed(250.Kw())
            .SetCategories(Ids.ToolbarCategories.Housing)
            .SetLayout(
                new EntityLayoutParams(
                    null,
                    null,
                    portsCanOnlyConnectToTransports: false,
                    Ids.TerrainTileSurfaces.SettlementPaths),
                "[4][4][4][4][5][5][5][5][5][5][4]   ",
                "[4][4][4][4][5][5][5][5][5][5][4]   ",
                "[4][4][4][4][5][5][5][5][5][5][4]<A#",
                "[4][4][4][4][5][5][5][5][5][5][4]   ",
                "[4][4][4][4][5][5][5][5][5][5][4]<B#",
                "[4][4][4][4][5][5][5][5][5][5][4]>X~",
                "[4][4][4][4][5][5][5][5][5][5][4]   ")
            .SetInput(provisions, 0.02.ToFix64(), 160)
            .SetOutput(waste, 0.004.ToFix64(), 64)
            .SetPrefabPath("Assets/Base/Settlements/HouseholdGoodsModule.prefab")
            .SetCustomIconPath(RecursiveIndustryIcons.CompanionAnimalCenter)
            .SetAnimationParams(AnimationParams.Loop(60.Percent()))
            .SetStayConnectedToLogisticsByDefault()
            .AnimateOnlyWhenServicingPops()
            .SetEmissionIntensity(2)
            .BuildAndAdd();

        Log.Info(
            "RecursiveIndustry: COMPANION_ANIMAL_CARE_REGISTERED "
            + "unity=0.6 provisions_per_pop_month=0.02 "
            + "waste_per_pop_month=0.004 workers=8 power_kw=250");
    }
}