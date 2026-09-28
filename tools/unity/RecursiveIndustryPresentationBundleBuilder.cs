using System;
using System.IO;
using System.Linq;
using Mafi.UnityEditor;
using UnityEditor;
using UnityEngine;

public static class RecursiveIndustryPresentationBundleBuilder
{
    public static void BuildUiIconBundle()
    {
        string root = Environment.GetEnvironmentVariable("RI_PUBLIC_ROOT");
        if (string.IsNullOrEmpty(root)) throw new InvalidOperationException("RI_PUBLIC_ROOT is required.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        RecursiveIndustryUiIconImporter.ConfigureIcons();
        if (!AssetBundlesHelpers.TryBuildAssetBundles("AssetBundles", string.Empty, BuildTarget.StandaloneWindows64,
            cleanBuild: false, out var manifest, out var bundleToDlc, out var error))
            throw new InvalidOperationException("UI icon bundle build failed: " + error);
        string name = manifest.GetAllAssetBundles().Single(value => value.StartsWith("uiicons_", StringComparison.Ordinal));
        if (name != "uiicons_5287" || manifest.GetAllDependencies(name).Length != 0)
            throw new InvalidOperationException("UI icon bundle identity or dependencies changed.");
        string[] expected = Directory.GetFiles(Path.Combine(root, "art/RecursiveIndustry/UiIcons/exports"), "*.png")
            .Select(path => "assets/recursiveindustry/uiicons/" + Path.GetFileName(path).ToLowerInvariant()).OrderBy(path => path, StringComparer.Ordinal).ToArray();
        if (expected.Length != 94) throw new InvalidOperationException("Expected 94 UI icons.");
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.GetFullPath(Path.Combine("AssetBundles", name)));
        if (bundle == null) throw new InvalidOperationException("Final UI icon bundle failed to load.");
        try
        {
            if (!bundle.GetAllAssetNames().OrderBy(path => path, StringComparer.Ordinal).SequenceEqual(expected))
                throw new InvalidOperationException("Final UI icon asset inventory differs.");
            foreach (string path in expected)
            {
                Sprite sprite = bundle.LoadAsset<Sprite>(path);
                if (sprite == null || sprite.texture.width != 512 || sprite.texture.height != 512)
                    throw new InvalidOperationException("Missing or incorrect final UI sprite: " + path);
            }
        }
        finally { bundle.Unload(true); }
        File.Copy(Path.Combine("AssetBundles", name), Path.Combine(root, "mods/RecursiveIndustry/AssetBundles", name), true);
        string metadata = Path.Combine(root, "art/RecursiveIndustry/UiIcons/unity");
        Directory.CreateDirectory(metadata);
        File.Copy(Path.Combine("AssetBundles", name + ".manifest"), Path.Combine(metadata, name + ".manifest"), true);
        foreach (string path in Directory.GetFiles("Assets/RecursiveIndustry/UiIcons", "*.png.meta"))
            File.Copy(path, Path.Combine(metadata, Path.GetFileName(path)), true);
        Debug.Log("RI_UI_ICON_BUNDLE_COMPLETE icons=94 dependencies=0 final_sprites=94");
    }

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