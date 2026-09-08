using System;
using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Products;

namespace RecursiveIndustry;

internal static class CountableProductGraphics
{
    public static CountableProductProto.Gfx WithCustomModel(
        CountableProductProto source,
        string iconPath,
        string modelKey)
    {
        const string root = "Assets/RecursiveIndustry/World/";
        if (source.Graphics.PrefabsPath.HasValue)
        {
            return new CountableProductProto.Gfx(
                root + modelKey + ".prefab",
                Option<string>.Some(iconPath),
                source.Graphics.PackingMode,
                source.Graphics.AllowPackingNoise,
                source.Graphics.MeshFamily,
                source.Graphics.Size,
                source.Graphics.ShadowMinPpm);
        }
        if (!source.Graphics.MeshFamily.HasValue)
            throw new InvalidOperationException("Missing native cargo mesh family: " + source.Id);
        return new CountableProductProto.Gfx(
            iconPath,
            source.Graphics.MeshFamily.Value,
            new ProductTextures(
                root + modelKey + "-albedo.png",
                root + "cargo-normals.png",
                root + "cargo-smoothmetal.png"),
            ImmutableArray.Create(
                new ProductLodMesh(0, root + modelKey + "-LOD0.obj"),
                new ProductLodMesh(1, root + modelKey + "-LOD1.obj"),
                new ProductLodMesh(2, root + modelKey + "-LOD2.obj"),
                new ProductLodMesh(3, root + modelKey + "-LOD3.obj"),
                new ProductLodMesh(4, root + modelKey + "-LOD4.obj")),
            source.Graphics.PackingModeOverride,
            source.Graphics.Size,
            source.Graphics.StorageRackYawDegrees);
    }

    public static CountableProductProto.Gfx WithCustomIcon(
        CountableProductProto source,
        string iconPath)
    {
        if (source.Graphics.PrefabsPath.HasValue)
        {
            return new CountableProductProto.Gfx(
                source.Graphics.PrefabsPath.Value,
                Option<string>.Some(iconPath),
                source.Graphics.PackingMode,
                source.Graphics.AllowPackingNoise,
                source.Graphics.MeshFamily,
                source.Graphics.Size,
                source.Graphics.ShadowMinPpm);
        }

        if (!source.Graphics.MeshFamily.HasValue)
        {
            throw new InvalidOperationException(
                $"Source product '{source.Id}' has neither prefab nor mesh-family graphics.");
        }

        return new CountableProductProto.Gfx(
            iconPath,
            source.Graphics.MeshFamily.Value,
            source.Graphics.Textures,
            source.Graphics.CustomLodMeshes,
            source.Graphics.PackingModeOverride,
            source.Graphics.Size,
            source.Graphics.StorageRackYawDegrees);
    }
}