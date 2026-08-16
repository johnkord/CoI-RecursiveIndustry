namespace RecursiveIndustry;

internal static class SystemsIntegrationLayout
{
    public const string PrefabPath = "Assets/Base/Machines/Assembly/AssemblyT5.prefab";
    public const string SoundPath =
        "Assets/Base/Machines/Assembly/AssemblyT4/AssemblerSound.prefab";

    public static string[] Create()
    {
        return new[]
        {
            "A#>[4][4][4][4][4][4]   ",
            "B#>[4][4][4][4][4][4]   ",
            "C#>[4][4][4][4][4][4]>#X",
            "D#>[5][5][4][4][4][4]   ",
            "   [5][5][4][4][4][4]   ",
        };
    }
}