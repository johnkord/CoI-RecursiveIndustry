using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Products;
using RecursiveIndustry;

internal static class Program
{
    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, request) =>
        {
            string path = Path.Combine(Environment.GetEnvironmentVariable("COI_ROOT"), "Captain of Industry_Data", "Managed", new AssemblyName(request.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        return Fixture.Run();
    }
}

internal static class Fixture
{
    private static int count;
    private static void Expect(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        count++;
    }

    private static T WithGraphics<T>(object graphics) where T : class
    {
        var proto = (T)FormatterServices.GetUninitializedObject(typeof(T));
        typeof(T).GetField("Graphics", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SetValue(proto, graphics);
        return proto;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Run()
    {
        var originalLods = ImmutableArray.Create(
            new TransportProto.Gfx.TransportCrossSectionLod(32, TransportCrossSection.Empty, 5),
            new TransportProto.Gfx.TransportCrossSectionLod(16, TransportCrossSection.Empty, 4),
            new TransportProto.Gfx.TransportCrossSectionLod(4, TransportCrossSection.Empty, 2),
            new TransportProto.Gfx.TransportCrossSectionLod(1, TransportCrossSection.Empty, 1));
        var graphics = new TransportProto.Gfx(originalLods, false, "source/material.mat", 1.0.Tiles(), false,
            "source/build.prefab", Option<TransportProto.Gfx.FlowIndicatorSpec>.None, Option<string>.None,
            new Dict<TransportPillarAttachmentType, string>(), 0, new TransportProto.Gfx.TransportInstancedRenderingData());
        TransportProto transport = WithGraphics<TransportProto>(graphics);
        var access = FiberGraphics.CrossSections(transport, false);
        var backbone = FiberGraphics.CrossSections(transport, true);
        Expect(access.Length == originalLods.Length && backbone.Length == originalLods.Length, "Native LOD inventory");
        for (int index = 0; index < originalLods.Length; index++)
        {
            Expect(access[index].PixelsPerMeter == originalLods[index].PixelsPerMeter
                && access[index].SamplesPerCurvedSegment == originalLods[index].SamplesPerCurvedSegment, "Native visual cadence " + index);
            var vertices = access[index].CrossSection.StaticCrossSectionParts[0];
            Expect(vertices.Length == (index < 2 ? 9 : 5), "Original reducing cross-section " + index);
            Expect(vertices[0].Coord == vertices[vertices.Length - 1].Coord, "Closed cross-section " + index);
            Expect(backbone[index].CrossSection.StaticCrossSectionParts[0][0].Coord.X > vertices[0].Coord.X, "Backbone visible width " + index);
        }
        Expect(FiberGraphics.FlowIndicator(transport, false).IsNone, "Absent native flow remains absent");
        Expect(FiberGraphics.PillarAttachments(transport).Count == 0, "Absent native mounts remain absent");

        var meshGraphics = new CountableProductProto.Gfx("source/icon.png", new ProductMeshFamilyProto.ID("FixtureMeshFamily"),
            new ProductTextures("source/albedo.png", "source/normal.png", "source/smooth.png"),
            storageRackYawDegrees: 37);
        CountableProductProto product = WithGraphics<CountableProductProto>(meshGraphics);
        var cargo = CountableProductGraphics.WithCustomModel(product, "custom/icon.png", "frontier_program");
        Expect(cargo.CustomLodMeshes.Length == 5, "All five cargo LODs");
        Expect(cargo.MeshFamily == meshGraphics.MeshFamily && cargo.Size.Equals(meshGraphics.Size)
            && cargo.PackingModeOverride == meshGraphics.PackingModeOverride && cargo.StorageRackYawDegrees == 37, "Native cargo packing and dimensions");
        var prefabGraphics = new CountableProductProto.Gfx("source/prefab.prefab", "source/icon.png", allowPackingNoise: true, shadowMinPpm: 7);
        var prefabCargo = CountableProductGraphics.WithCustomModel(WithGraphics<CountableProductProto>(prefabGraphics), "custom/icon.png", "companion_provisions");
        Expect(prefabCargo.PrefabsPath.Value == "Assets/RecursiveIndustry/World/companion_provisions.prefab"
            && prefabCargo.AllowPackingNoise && prefabCargo.ShadowMinPpm == 7 && prefabCargo.PackingMode == prefabGraphics.PackingMode, "Native prefab cargo representation");
        Expect(WorldModelPaths.ForIcon("Assets/RecursiveIndustry/UiIcons/autonomous_hauler.png") == "Assets/RecursiveIndustry/World/autonomous_hauler.prefab", "Exact icon-to-model path");
        bool rejected = false;
        try { WorldModelPaths.ForIcon("source/foreign.png"); } catch (InvalidOperationException) { rejected = true; }
        Expect(rejected, "Foreign model identity rejected");
        Console.WriteLine("PASS: " + count + " compiled world-art constructor and policy checks against installed MaFi types (not game runtime).");
        return 0;
    }
}