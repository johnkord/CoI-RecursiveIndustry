using System;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Gfx;

namespace RecursiveIndustry;

internal static class FiberGraphics
{
    public const string Root = "Assets/RecursiveIndustry/World/";
    public const string Port = Root + "fiber_port.prefab";
    public const string PortFar = Root + "fiber_port_far.prefab";
    public const string PortClosed = Root + "fiber_port_closed.prefab";
    public const string PortClosedFar = Root + "fiber_port_closed_far.prefab";
    public const string Material = Root + "fiber_line.mat";
    public const string Junction = Root + "fiber_junction.prefab";
    public const string Vertical = Root + "fiber_vertical.prefab";
    public const string Mount = Root + "fiber_mount.prefab";

    public static ImmutableArray<TransportProto.Gfx.TransportCrossSectionLod> CrossSections(
        TransportProto source,
        bool backbone)
    {
        var levels = new Lyst<TransportProto.Gfx.TransportCrossSectionLod>();
        for (int index = 0; index < source.Graphics.CrossSectionLods.Length; index++)
        {
            TransportProto.Gfx.TransportCrossSectionLod original = source.Graphics.CrossSectionLods[index];
            double halfWidth = backbone ? 0.32 : 0.21;
            double halfHeight = backbone ? 0.13 : 0.10;
            var part = new Lyst<CrossSectionVertex>();
            int sides = index < 2 ? 8 : 4;
            for (int corner = 0; corner <= sides; corner++)
            {
                double angle = (corner % sides) * Math.PI * 2 / sides + Math.PI / 4;
                double horizontal = Math.Cos(angle) * halfWidth;
                double vertical = Math.Sin(angle) * halfHeight;
                part.Add(new CrossSectionVertex(
                    new RelTile2f(horizontal.Meters(), vertical.Meters()),
                    new Vector2f(Math.Cos(angle).ToFix32(), Math.Sin(angle).ToFix32()),
                    (float)corner / sides));
            }
            var section = new TransportCrossSection(
                ImmutableArray.Create(part.ToImmutableArrayAndClear()),
                ImmutableArray<ImmutableArray<CrossSectionVertex>>.Empty);
            levels.Add(new TransportProto.Gfx.TransportCrossSectionLod(
                original.PixelsPerMeter,
                section,
                original.SamplesPerCurvedSegment));
        }
        return levels.ToImmutableArrayAndClear();
    }

    public static Dict<TransportPillarAttachmentType, string> PillarAttachments(TransportProto source)
    {
        var result = new Dict<TransportPillarAttachmentType, string>();
        foreach (var entry in source.Graphics.PillarAttachments)
            result.Add(entry.Key, Mount);
        return result;
    }

    public static Option<TransportProto.Gfx.FlowIndicatorSpec> FlowIndicator(TransportProto source, bool backbone)
    {
        if (source.Graphics.FlowIndicator.IsNone)
            return Option<TransportProto.Gfx.FlowIndicatorSpec>.None;
        TransportProto.Gfx.FlowIndicatorSpec original = source.Graphics.FlowIndicator.Value;
        string suffix = backbone ? "backbone" : "access";
        return new TransportProto.Gfx.FlowIndicatorSpec(
            Root + "fiber_flow_frame_" + suffix + ".prefab",
            Root + "fiber_flow_" + suffix + ".prefab",
            Root + "fiber_flow_glass_" + suffix + ".prefab",
            original.SkipTransportLength,
            original.PlacementGap,
            original.LengthScale,
            original.CrossSectionScale,
            original.Parameters);
    }
}