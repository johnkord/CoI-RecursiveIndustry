using System;
using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Factory.Zippers;
using Mafi.Core.Mods;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class IndustrialControlTransportData : IModData
{
    private const int AccessCapacityPer60 = 200;
    private const int BackboneCapacityPer60 = 450;

    public void RegisterData(ProtoRegistrator registrator)
    {
        var dataShape = new IoPortShapeProto(
            RecursiveIndustryIds.Infrastructure.Data,
            Proto.Str.Empty,
            ':',
            DataProductProto.ProductType,
            new IoPortShapeProto.Gfx(
                FiberGraphics.Port,
                FiberGraphics.PortFar,
                showWhenTwoTransportsConnect: true,
                disconnectedPrefabPath: FiberGraphics.PortClosed,
                disconnectedPrefabPathLod3: FiberGraphics.PortClosedFar));
        registrator.PrototypesDb.Add(dataShape);

        TransportProto accessFiber = CloneNativeTransport(
            RecursiveIndustryIds.Infrastructure.AccessFiber,
            "Access Fiber",
            "A Data-only access link sized for three continuously optimized facilities.",
            registrator.PrototypesDb.GetOrThrow<TransportProto>(Ids.Transports.PipeT2),
            dataShape,
            RecursiveIndustryIcons.AccessFiber);
        TransportProto backboneFiber = CloneNativeTransport(
            RecursiveIndustryIds.Infrastructure.BackboneFiber,
            "Backbone Fiber",
            "A high-capacity Data-only trunk sized for seven continuously optimized facilities.",
            registrator.PrototypesDb.GetOrThrow<TransportProto>(Ids.Transports.PipeT3),
            dataShape,
            RecursiveIndustryIcons.BackboneFiber);

        registrator.PrototypesDb.Add(accessFiber);
        registrator.PrototypesDb.Add(backboneFiber);
        accessFiber.SetNextTier(backboneFiber);
        RegisterFiberJunction(registrator, accessFiber);

        DataProductProto data = registrator.PrototypesDb.GetOrThrow<DataProductProto>(
            RecursiveIndustryIds.Products.IndustrialControlStream);
        Quantity accessCapacity = accessFiber.GetMaxThroughputPer60For(data);
        Quantity backboneCapacity = backboneFiber.GetMaxThroughputPer60For(data);
        if (accessCapacity != AccessCapacityPer60.Quantity()
            || backboneCapacity != BackboneCapacityPer60.Quantity())
        {
            throw new InvalidOperationException(
                "Industrial Control Fiber capacity drift: access=" + accessCapacity
                + ", backbone=" + backboneCapacity + ".");
        }
        Log.Info(
            "RecursiveIndustry: INDUSTRIAL_CONTROL_FIBER_REGISTERED"
            + " data_type=" + data.Type
            + " shape=:"
            + " access_per_60=" + accessCapacity
            + " backbone_per_60=" + backboneCapacity);
    }

    private static void RegisterFiberJunction(
        ProtoRegistrator registrator,
        TransportProto accessFiber)
    {
        EntityLayout layout = registrator.LayoutParser.ParseLayoutOrThrow(
            new EntityLayoutParams(portsCanOnlyConnectToTransports: false),
            "   B:+   ",
            "+:C{1}A:+",
            "   D:+   ");
        var graphics = new LayoutEntityProto.Gfx(
            FiberGraphics.Junction,
            customIconPath: RecursiveIndustryIcons.FiberJunction,
            color: ColorRgba.White,
            hideBlockedPortsIcon: true,
            categories: accessFiber.Graphics.Categories,
            useInstancedRendering: true,
            instancedRenderingExcludedObjects: ImmutableArray<string>.Empty,
            maxRenderedLod: 3);
        registrator.PrototypesDb.Add(new MiniZipperProto(
            RecursiveIndustryIds.Infrastructure.FiberJunction,
            Proto.CreateStr(
                RecursiveIndustryIds.Infrastructure.FiberJunction,
                "Fiber Junction",
                "Connects and distributes Data-only Fiber links."),
            layout,
            graphics));
    }

    private static TransportProto CloneNativeTransport(
        StaticEntityProto.ID id,
        string name,
        string description,
        TransportProto source,
        IoPortShapeProto dataShape,
        string iconPath)
    {
        var graphics = new TransportProto.Gfx(
            FiberGraphics.CrossSections(source, id == RecursiveIndustryIds.Infrastructure.BackboneFiber),
            renderProducts: false,
            FiberGraphics.Material,
            source.Graphics.TransportUvLength,
            renderTransportedProducts: false,
            source.Graphics.SoundOnBuildPrefabPath,
            FiberGraphics.FlowIndicator(source, id == RecursiveIndustryIds.Infrastructure.BackboneFiber),
            source.Graphics.VerticalConnectorPrefabPath.HasValue ? Option<string>.Some(FiberGraphics.Vertical) : Option<string>.None,
            FiberGraphics.PillarAttachments(source),
            source.Graphics.UvShiftY,
            new TransportProto.Gfx.TransportInstancedRenderingData(),
            source.Graphics.CrossSectionRadius,
            source.Graphics.CrossSectionScale,
            usePerProductColoring: true,
            customIconPath: iconPath,
            source.Graphics.UseInstancedRendering,
            source.Graphics.MaxRenderedLod,
            source.Graphics.Categories,
            source.Graphics.CanBePickedUnderground);

        return new TransportProto(
            id,
            Proto.CreateStr(id, name, description),
            source.SurfaceRelativeHeight,
            source.MaxQuantityPerTransportedProduct,
            source.TransportedProductsSpacing,
            source.SpeedPerTick,
            source.ZStepLength,
            source.NeedsPillarsAtGround,
            source.CanBeBuried,
            source.TileSurfaceWhenOnGround,
            source.MaxPillarSupportRadius,
            dataShape,
            source.BaseElectricityCost,
            source.CornersSharpnessPercent,
            allowMixedProducts: false,
            isBuildable: true,
            source.Costs,
            source.ConstructionDurationPerProduct,
            graphics);
    }
}