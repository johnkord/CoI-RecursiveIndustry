namespace RecursiveIndustry;

internal static class OrbitalPowerRelayLayout
{
    public const string PrefabPath = SystemsIntegrationLayout.PrefabPath;
    public const string SoundPath = SystemsIntegrationLayout.SoundPath;

    public static string[] Create()
    {
        return new[]
        {
            "   [4][4][4][4][4][4]   ",
            "A#>[4][4][4][4][4][4]   ",
            "   [4][4][4][4][4][4]   ",
            "   [5][5][4][4][4][4]   ",
            "   [5][5][4][4][4][4]   ",
        };
    }
}