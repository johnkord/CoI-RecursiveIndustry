using System;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Vehicles.Trucks;

namespace RecursiveIndustry;

internal static class WorldAttachmentGraphics
{
    public static ImmutableArray<AttachmentProto> CloneAmphibious(
        ProtoRegistrator registrator,
        ImmutableArray<AttachmentProto> source)
    {
        var result = new Lyst<AttachmentProto>();
        foreach (AttachmentProto attachment in source)
        {
            string id = RecursiveIndustryIds.Vehicles.AutonomousAmphibiousHauler.Value;
            AttachmentProto clone;
            if (attachment is TankAttachmentProto tank)
            {
                clone = new TankAttachmentProto(
                    new Proto.ID(id + "_AttachmentTank"),
                    tank.EligibleProductsFilter,
                    new TankAttachmentProto.Gfx(
                        WorldModelPaths.Root + "amphibious_tank.prefab",
                        "icons", "tank",
                        tank.Graphics.DefaultColor,
                        tank.Graphics.DefaultAccentColor),
                    tank.KeepOnEvenIfNotNeeded);
            }
            else if (attachment is FlatBedAttachmentProto flat)
            {
                clone = new FlatBedAttachmentProto(
                    new Proto.ID(id + "_AttachmentFlatBed"),
                    flat.EligibleProductsFilter,
                    new FlatBedAttachmentProto.Gfx(
                        flat.Graphics.ProductRenderOffsets,
                        WorldModelPaths.Root + "amphibious_flatbed.prefab",
                        flat.Graphics.ShelfName,
                        flat.Graphics.IconIsCustom ? flat.Graphics.IconPath : null,
                        flat.Graphics.ColorsMap),
                    flat.KeepOnEvenIfNotNeeded);
            }
            else if (attachment is DumpAttachmentProto dump)
            {
                if (dump.Graphics.AnimationStateName.IsNone)
                    throw new InvalidOperationException("Amphibious dump fill must retain native animation control");
                clone = new DumpAttachmentProto(
                    new Proto.ID(id + "_AttachmentDump"),
                    new DumpAttachmentProto.Gfx(
                        WorldModelPaths.Root + "amphibious_dump.prefab",
                        "bed/PileSmooth", "bed/PileSmooth",
                        dump.Graphics.PileTextureParams,
                        dump.Graphics.AnimationStateName.Value));
            }
            else
            {
                throw new InvalidOperationException("Unknown amphibious attachment family: " + attachment.Id);
            }
            registrator.PrototypesDb.Add(clone);
            result.Add(clone);
        }
        return result.ToImmutableArrayAndClear();
    }

    public static Option<AttachmentProto> EmptySelection(
        TruckProto source,
        ImmutableArray<AttachmentProto> replacements)
    {
        if (source.AttachmentWhenEmpty.IsNone)
            return Option<AttachmentProto>.None;
        for (int index = 0; index < source.Attachments.Length; index++)
            if (source.Attachments[index] == source.AttachmentWhenEmpty.Value)
                return replacements[index];
        throw new InvalidOperationException("Native empty attachment is outside its supported inventory");
    }
}