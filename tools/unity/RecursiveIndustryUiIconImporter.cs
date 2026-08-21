using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class RecursiveIndustryUiIconImporter
{
    private const string IconRoot = "Assets/RecursiveIndustry/UiIcons";
    private const string ProvisionalBundleName = "recursiveindustry_ui_icons";

    private static bool s_isScheduled;

    static RecursiveIndustryUiIconImporter()
    {
        ScheduleConfiguration();
    }

    [MenuItem("Recursive Industry/Configure UI Icons")]
    private static void ConfigureFromMenu()
    {
        ConfigureIcons();
    }

    internal static void ConfigureIcons()
    {
        var configuredCount = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { IconRoot }))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
            {
                continue;
            }

            var changed = false;
            changed |= SetIfDifferent(importer.textureType, TextureImporterType.Sprite,
                value => importer.textureType = value);
            changed |= SetIfDifferent(importer.spriteImportMode, SpriteImportMode.Single,
                value => importer.spriteImportMode = value);
            changed |= SetIfDifferent(importer.alphaSource, TextureImporterAlphaSource.FromInput,
                value => importer.alphaSource = value);
            changed |= SetIfDifferent(importer.alphaIsTransparency, true,
                value => importer.alphaIsTransparency = value);
            changed |= SetIfDifferent(importer.maxTextureSize, 512,
                value => importer.maxTextureSize = value);
            changed |= SetIfDifferent(importer.mipmapEnabled, false,
                value => importer.mipmapEnabled = value);
            changed |= SetIfDifferent(importer.wrapMode, TextureWrapMode.Clamp,
                value => importer.wrapMode = value);
            changed |= SetIfDifferent(importer.filterMode, FilterMode.Bilinear,
                value => importer.filterMode = value);
            changed |= SetIfDifferent(importer.textureCompression,
                TextureImporterCompression.Uncompressed,
                value => importer.textureCompression = value);
            changed |= SetIfDifferent(importer.anisoLevel, 0,
                value => importer.anisoLevel = value);

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

        Debug.Log($"Recursive Industry: configured {configuredCount} UI icon importer(s).");
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

        ConfigureIcons();
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