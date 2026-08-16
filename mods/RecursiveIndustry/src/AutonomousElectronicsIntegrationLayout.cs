namespace RecursiveIndustry;

internal static class AutonomousElectronicsIntegrationLayout
{
    public const string PrefabPath = AutonomousMicrochipLayout.PrefabPath;

    public static string[] Create()
    {
        return new[]
        {
            "      D#vF#vB#vC#vE#v      ",
            "   [2][2][2][2][2][2][2]   ",
            "A#>[2][2][3][3][3][2][2]>#X",
            "   [2][2][2][2][2][2][2]   ",
            "      [2][2][2][2][2]      ",
        };
    }
}