using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;

namespace RecursiveIndustry;

internal sealed class CivicKnowledgeData : IModData
{
    public const string AssetRoot = "Assets/RecursiveIndustry/Reconstruction/";
    public const string StreamIcon = AssetRoot + "civic_knowledge_stream.png";
    public const string CenterIcon = AssetRoot + "civic_model_center.png";
    public const string CommonsIcon = AssetRoot + "knowledge_commons.png";

    public void RegisterData(ProtoRegistrator registrator)
    {
        ProductProto.ID streamId = RecursiveIndustryIds.Products.CivicKnowledgeStream;
        DataProductProto stream = registrator.PrototypesDb.Add(new DataProductProto(
            streamId,
            Proto.CreateStr(streamId, "Civic Knowledge Stream",
                "Live access to learning, translation, public knowledge, and assisted services. Carries only over dedicated Fiber connections; it is not Industrial Control Stream and cannot be stockpiled in storage."),
            new ProductProto.Gfx(Option<string>.None, StreamIcon,
                color: new ColorRgba(0.2f, 0.8f, 0.5f),
                transportColor: new ColorRgba(0.12f, 0.65f, 0.42f),
                transportAccentColor: new ColorRgba(0.8f, 1.0f, 0.88f))));

        var center = registrator.MachineProtoBuilder
            .Start("Civic Model Center", RecursiveIndustryIds.Machines.CivicModelCenter)
            .Description("Turns Models, Datasets, physical evidence, and Office Supplies into a live civic service. A full center supplies about 1,333 people through Backbone Fiber. Its support remains staffed; it does not replace clinics or choose island policy.")
            .SetCost(Costs.Build.CP4(240)
                .Product(64, Ids.Products.Electronics4)
                .Product(16, RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Product(4, RecursiveIndustryIds.Products.ValidatedResearchDossier)
                .Workers(40).MaintenanceT3(6))
            .SetElectricityConsumption(1000.Kw())
            .SetComputingConsumption(Computing.FromTFlops(128))
            .SetCategories(Ids.ToolbarCategories.Production_General)
            .SetLayout(new EntityLayoutParams(),
                "A#>[3][3][3][3][3][3]   ",
                "B#>[3][3][4][4][3][3]   ",
                "C#>[3][3][4][4][3][3]>:X",
                "D#>[3][3][3][3][3][3]   ",
                "   [3][3][3][3][3][3]   ")
            .SetPrefabPath(AssetRoot + "civic_model_center.prefab")
            .SetCustomIconPath(CenterIcon)
            .BuildAndAdd();

        registrator.RecipeProtoBuilder
            .Start(RecursiveIndustryIds.Recipes.ProvideCivicKnowledge)
            .AddInput(1, RecursiveIndustryIds.Products.ModelArchive)
            .AddInput(8, RecursiveIndustryIds.Products.DatasetArchive)
            .AddInput(1, RecursiveIndustryIds.Products.ValidatedResearchDossier)
            .AddInput(8, Ids.Products.OfficeSupplies)
            .AddOutput(1600, streamId)
            .BuildAndAdd()
            .WithCommonInputPorts(
                (RecursiveIndustryIds.Products.ModelArchive, "A"),
                (RecursiveIndustryIds.Products.DatasetArchive, "B"),
                (RecursiveIndustryIds.Products.ValidatedResearchDossier, "C"),
                (Ids.Products.OfficeSupplies, "D"))
            .WithCommonOutputPorts((streamId, "X"))
            .BindTo(center, 360.Seconds());

        UpointsStatsCategoryProto services = registrator.PrototypesDb
            .GetOrThrow<UpointsStatsCategoryProto>(new Proto.ID("UpointsStatsCat_Services"));
        UpointsCategoryProto category = registrator.PrototypesDb.Add(new UpointsCategoryProto(
            RecursiveIndustryIds.Settlements.CivicKnowledgeNeed, CommonsIcon, services, hideCount: true));
        PopNeedProto need = registrator.PrototypesDb.Add(new PopNeedProto(
            RecursiveIndustryIds.Settlements.CivicKnowledgeNeed,
            Proto.CreateStr(RecursiveIndustryIds.Settlements.CivicKnowledgeNeed,
                "Civic knowledge", "Optional learning, translation, public information, and staffed service access. Supply contributes Unity; it grants no Health or worker-productivity bonus."),
            1.2.Upoints(), category, healthGiven: null,
            consumptionMultiplierProperty: null, unityMultiplierProperty: null,
            graphics: new PopNeedProto.Gfx(CommonsIcon)));

        registrator.SettlementModuleProtoBuilder
            .Start("Knowledge Commons", RecursiveIndustryIds.Settlements.KnowledgeCommons)
            .Description("A staffed learning and public-service space attached to a settlement. Needs its own Civic Knowledge Fiber supply, not Industrial Control. A supply interruption removes only this optional service's Unity contribution.")
            .SetNeed(need)
            .SetCost(Costs.Build.CP4(240)
                .Product(32, Ids.Products.Electronics4)
                .Product(8, RecursiveIndustryIds.Products.ValidatedControlPackage)
                .Workers(12).MaintenanceT2(4))
            .SetElectricityConsumed(250.Kw())
            .SetCategories(Ids.ToolbarCategories.Housing)
            .SetLayout(new EntityLayoutParams(null, null,
                    portsCanOnlyConnectToTransports: false, Ids.TerrainTileSurfaces.SettlementPaths),
                "[3][3][3][3][3][3][3][3][3][3][3]   ",
                "[3][3][4][4][4][4][4][4][4][3][3]   ",
                "[3][3][4][4][4][4][4][4][4][3][3]<A:",
                "[3][3][4][4][4][4][4][4][4][3][3]   ",
                "[3][3][4][4][4][4][4][4][4][3][3]   ",
                "[3][3][3][3][3][3][3][3][3][3][3]   ",
                "[3][3][3][3][3][3][3][3][3][3][3]   ")
            .SetInput(stream, 0.2.ToFix64(), 240)
            .SetPrefabPath(AssetRoot + "knowledge_commons.prefab")
            .SetCustomIconPath(CommonsIcon)
            .BuildAndAdd();

        Log.Info("RecursiveIndustry: CIVIC_KNOWLEDGE_REGISTERED stream=" + stream.Id
            + " output=1600 duration=360 per_pop_month=0.2 buffer=240 unity=1.2 center_workers=40 commons_workers=12");
    }

    public static void RegisterResearch(ProtoRegistrator registrator, ResearchNodeProto industrialControl)
    {
        ResearchNodeProto civic = registrator.ResearchNodeProtoBuilder
            .Start("Civic Knowledge Systems", RecursiveIndustryIds.Research.CivicKnowledgeSystems, costMonths: 480)
            .Description("Invest industrial abundance in learning, translation, and public-service access. A Civic Model Center supplies the staffed Knowledge Commons over dedicated Fiber. The service is optional for housing, health, and all Epochs.")
            .AddProductToUnlock(RecursiveIndustryIds.Products.CivicKnowledgeStream, addIconToNode: true)
            .AddMachineToUnlock(RecursiveIndustryIds.Machines.CivicModelCenter, unlockAllRecipes: false)
            .AddRecipeToUnlock(RecursiveIndustryIds.Recipes.ProvideCivicKnowledge)
            .AddLayoutEntityToUnlock(RecursiveIndustryIds.Settlements.KnowledgeCommons)
            .AddProtoToUnlock<PopNeedProto>(RecursiveIndustryIds.Settlements.CivicKnowledgeNeed)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ValidatedResearchDossier, 4)
            .AddRequirementForLifetimeProduction(RecursiveIndustryIds.Products.ModelArchive, 16)
            .BuildAndAdd();
        civic.GridPosition = new Vector2i(200, 34);
        civic.AddParent(industrialControl);
        civic.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(RecursiveIndustryIds.Research.PhysicalValidation));
        civic.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.IspModule));
    }
}