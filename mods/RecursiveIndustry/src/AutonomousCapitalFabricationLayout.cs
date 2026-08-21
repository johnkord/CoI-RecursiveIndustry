namespace RecursiveIndustry;

internal static class AutonomousCapitalFabricationLayout
{
    public const string PrefabPath = ConstructionNexusLayout.PrefabPath;
    public const string SoundPath = ConstructionNexusLayout.SoundPath;

    public static string[] Create()
    {
        return new[]
        {
            "A#>[4][4][4][4][4][4]   ",
            "B#>[4][4][4][4][4][4]   ",
            "C#>[4][4][4][4][4][4]   ",
            "D#>[5][5][4][4][4][4]>#X",
            "E:>[5][5][4][4][4][4]   ",
        };
    }
}