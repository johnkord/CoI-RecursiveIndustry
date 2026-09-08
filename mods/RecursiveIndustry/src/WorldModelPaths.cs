using System;
using System.IO;

namespace RecursiveIndustry;

internal static class WorldModelPaths
{
    public const string Root = "Assets/RecursiveIndustry/World/";

    public static string ForIcon(string iconPath)
    {
        string name = Path.GetFileNameWithoutExtension(iconPath);
        if (string.IsNullOrEmpty(name) || !name.StartsWith("autonomous_", StringComparison.Ordinal))
            throw new InvalidOperationException("Unmapped autonomous model icon: " + iconPath);
        return Root + name + ".prefab";
    }
}