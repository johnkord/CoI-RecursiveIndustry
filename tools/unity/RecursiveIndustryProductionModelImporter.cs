using System;
using System.Linq;
using Mafi.UnityEditor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class RecursiveIndustryProductionModelImporter
{
    private const string ModelRoot = "Assets/RecursiveIndustry/ProductModels/Cartridge";
    private const string ProvisionalBundleName = "recursiveindustry_product_models";

    private static bool s_isScheduled;

    static RecursiveIndustryProductionModelImporter()
    {
        ScheduleConfiguration();
    }

    [MenuItem("Recursive Industry/Configure Production Cartridges")]
    private static void ConfigureFromMenu()
    {
        ConfigureAssets();
    }

    public static void BuildProductionBundles()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureAssets();

        if (!AssetBundlesHelpers.TryBuildAssetBundles(
                "AssetBundles",
                string.Empty,
                BuildTarget.StandaloneWindows64,
                cleanBuild: false,
                out var manifest,
                out var bundleToDlc,
                out var error))
        {
            throw new InvalidOperationException(
                "Recursive Industry production cartridge bundle build failed: " + error);
        }

        if (!AssetBundlesHelpers.TryDeployAssetBundles(
                "AssetBundles",
                ".",
                manifest,
                bundleToDlc,
                out var bundlesCount,
                out error))
        {
            throw new InvalidOperationException(
                "Recursive Industry production cartridge bundle deployment failed: " + error);
        }

        string iconBundle = manifest.GetAllAssetBundles().Single(
            name => name.StartsWith("producticons_", StringComparison.Ordinal));
        string modelBundle = manifest.GetAllAssetBundles().Single(
            name => name.StartsWith("cartridge_", StringComparison.Ordinal));
        if (!AssetBundlesHelpers.TryGenerateMafiBundlesManifest(
                manifest,
                new[] { iconBundle, modelBundle },
                isDlc: false,
                validateBundle: _ => true,
                "AssetBundles/mafi_bundles.manifest",
                out error))
        {
            throw new InvalidOperationException(
                "Recursive Industry production bundle manifest filtering failed: " + error);
        }

        Debug.Log(
            $"Recursive Industry: production cartridge bundle build completed "
            + $"({modelBundle}; {bundlesCount} project bundles)." );
    }

    private static void ScheduleConfiguration()
    {
        if (s_isScheduled)
        {
            return;
        }

        s_isScheduled = true;
        EditorApplication.delayCall += ConfigureWhenReady;
    }

    private static void ConfigureWhenReady()
    {
        s_isScheduled = false;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            ScheduleConfiguration();
            return;
        }

        ConfigureAssets();
    }

    private static void ConfigureAssets()
    {
        var configuredCount = 0;
        foreach (var guid in AssetDatabase.FindAssets(string.Empty, new[] { ModelRoot }))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer is null)
            {
                continue;
            }

            var changed = importer switch
            {
                ModelImporter modelImporter => ConfigureModel(modelImporter),
                TextureImporter textureImporter => ConfigureTexture(assetPath, textureImporter),
                _ => false,
            };

            if (!string.Equals(importer.assetBundleName, ProvisionalBundleName,
                    StringComparison.Ordinal))
            {
                importer.assetBundleName = ProvisionalBundleName;
                changed = true;
            }

            if (!changed)
            {
                continue;
            }

            importer.SaveAndReimport();
            configuredCount++;
        }

        Debug.Log($"Recursive Industry: configured {configuredCount} production cartridge asset(s).");
    }

    private static bool ConfigureModel(ModelImporter importer)
    {
        var changed = false;
        changed |= SetIfDifferent(importer.globalScale, 1f, value => importer.globalScale = value);
        changed |= SetIfDifferent(importer.isReadable, true, value => importer.isReadable = value);
        changed |= SetIfDifferent(importer.importNormals, ModelImporterNormals.Import,
            value => importer.importNormals = value);
        changed |= SetIfDifferent(importer.importTangents, ModelImporterTangents.CalculateMikk,
            value => importer.importTangents = value);
        changed |= SetIfDifferent(importer.importCameras, false, value => importer.importCameras = value);
        changed |= SetIfDifferent(importer.importLights, false, value => importer.importLights = value);
        changed |= SetIfDifferent(importer.addCollider, false, value => importer.addCollider = value);
        changed |= SetIfDifferent(importer.generateSecondaryUV, false,
            value => importer.generateSecondaryUV = value);
        changed |= SetIfDifferent(importer.materialImportMode, ModelImporterMaterialImportMode.None,
            value => importer.materialImportMode = value);
        return changed;
    }

    private static bool ConfigureTexture(string assetPath, TextureImporter importer)
    {
        var isNormal = assetPath.EndsWith("-normals.png", StringComparison.OrdinalIgnoreCase);
        var isSmoothMetal = assetPath.EndsWith("-smoothmetal.png", StringComparison.OrdinalIgnoreCase);
        var changed = false;

        changed |= SetIfDifferent(importer.textureType,
            isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default,
            value => importer.textureType = value);
        changed |= SetIfDifferent(importer.sRGBTexture, !isNormal && !isSmoothMetal,
            value => importer.sRGBTexture = value);
        changed |= SetIfDifferent(importer.alphaSource, TextureImporterAlphaSource.None,
            value => importer.alphaSource = value);
        changed |= SetIfDifferent(importer.maxTextureSize, 512,
            value => importer.maxTextureSize = value);
        changed |= SetIfDifferent(importer.mipmapEnabled, true,
            value => importer.mipmapEnabled = value);
        changed |= SetIfDifferent(importer.wrapMode, TextureWrapMode.Clamp,
            value => importer.wrapMode = value);
        changed |= SetIfDifferent(importer.filterMode, FilterMode.Bilinear,
            value => importer.filterMode = value);
        changed |= SetIfDifferent(importer.textureCompression, TextureImporterCompression.Uncompressed,
            value => importer.textureCompression = value);
        changed |= SetIfDifferent(importer.anisoLevel, 2, value => importer.anisoLevel = value);
        return changed;
    }

    private static bool SetIfDifferent<T>(T currentValue, T expectedValue, Action<T> setter)
    {
        if (Equals(currentValue, expectedValue))
        {
            return false;
        }

        setter(expectedValue);
        return true;
    }
}
