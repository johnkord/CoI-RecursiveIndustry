using System;
using Mafi;
using Mafi.Core.Products;

namespace RecursiveIndustry;

internal static class CountableProductGraphics
{
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