namespace RecursiveIndustry;

internal static class ConstructionNexusLayout
{
    public const string PrefabPath = SystemsIntegrationLayout.PrefabPath;
    public const string SoundPath = SystemsIntegrationLayout.SoundPath;

    public static string[] Create()
    {
        return new[]
        {
            "A#>[4][4][4][4][4][4]   ",
            "B#>[4][4][4][4][4][4]   ",
            "   [4][4][4][4][4][4]   ",
            "D#>[5][5][4][4][4][4]>#X",
            "E~>[5][5][4][4][4][4]   ",
        };
    }
}