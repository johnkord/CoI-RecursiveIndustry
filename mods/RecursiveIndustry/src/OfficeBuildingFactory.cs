using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal static class OfficeBuildingFactory
{
    public static OfficeBuildingProto Register(
        ProtoRegistrator registrator,
        StaticEntityProto.ID id,
        string name,
        string description,
        StaticEntityProto.ID vanillaOfficeId,
        string customIconPath,
        ProductProto controlPackage,
        ProductProto recyclables,
        EntityCostsTpl.Builder costs,
        int powerKw,
        int computing,
        int packageQuantity,
        int bonusPercent,
        int inputBuffer = 64,
        int outputBuffer = 64)
    {
        OfficeBuildingProto vanillaOffice = registrator.PrototypesDb
            .GetOrThrow<OfficeBuildingProto>(vanillaOfficeId);
        EntityCostsTpl costsTemplate = costs;
        LayoutEntityProto.Gfx graphics = vanillaOffice.Graphics;
        var customGraphics = new LayoutEntityProto.Gfx(
            graphics.PrefabPath,
            graphics.PrefabOrigin,
            Option<string>.Some(customIconPath),
            graphics.Color,
            graphics.HideBlockedPortsIcon,
            graphics.VisualizedLayers,
            graphics.Categories,
            graphics.UseInstancedRendering,
            graphics.UseSemiInstancedRendering,
            instancedRenderingExcludedObjects:
                graphics.SemiInstancedRenderingExcludedObjects,
            instancedRenderingExcludedObjectsPattern:
                graphics.SemiInstancedRenderingExcludedObjectsRegex?.ToString(),
            maxRenderedLod: graphics.MaxRenderedLod,
            disableEmptyChildrenStripping:
                graphics.DisableEmptyChildrenStripping,
            removeUndergroundVertices: graphics.RemoveUndergroundVertices,
            yawForGeneratedIcon: graphics.YawForGeneratedIcon,
            canBePickedUnderground: graphics.CanBePickedUnderground,
            doNotFlipModel: graphics.DoNotFlipModel);

        return registrator.PrototypesDb.Add(new OfficeBuildingProto(
            id,
            Proto.CreateStr(
                id,
                name,
                description,
                "title and description of a Recursive Industry Office building"),
            vanillaOffice.Layout,
            costsTemplate.MapToEntityCosts(registrator),
            powerKw.Kw(),
            new ProductQuantity(controlPackage, new Quantity(packageQuantity)),
            new ProductQuantity(recyclables, new Quantity(packageQuantity)),
            inputBuffer.Quantity(),
            outputBuffer.Quantity(),
            360.Seconds(),
            1,
            step => step * computing.TFlops(),
            step => step * bonusPercent.Percent(),
            customGraphics));
    }
}