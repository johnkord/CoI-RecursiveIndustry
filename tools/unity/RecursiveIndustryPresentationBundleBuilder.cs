using System;
using System.Linq;
using Mafi.UnityEditor;
using UnityEditor;
using UnityEngine;

public static class RecursiveIndustryPresentationBundleBuilder
{
    public static void BuildPresentationBundles()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        RecursiveIndustryUiIconImporter.ConfigureIcons();

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
                "Recursive Industry presentation bundle build failed: " + error);
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
                "Recursive Industry presentation bundle deployment failed: " + error);
        }

        string iconBundle = manifest.GetAllAssetBundles().Single(
            name => name.StartsWith("producticons_", StringComparison.Ordinal));
        string modelBundle = manifest.GetAllAssetBundles().Single(
            name => name.StartsWith("cartridge_", StringComparison.Ordinal));
        string uiIconBundle = manifest.GetAllAssetBundles().Single(
            name => name.StartsWith("uiicons_", StringComparison.Ordinal));
        if (!AssetBundlesHelpers.TryGenerateMafiBundlesManifest(
                manifest,
                new[] { iconBundle, modelBundle, uiIconBundle },
                isDlc: false,
                validateBundle: _ => true,
                "AssetBundles/mafi_bundles.manifest",
                out error))
        {
            throw new InvalidOperationException(
                "Recursive Industry presentation bundle manifest filtering failed: "
                + error);
        }

        Debug.Log(
            $"Recursive Industry: presentation bundle build completed "
            + $"({uiIconBundle}; {bundlesCount} project bundles).");
    }
}