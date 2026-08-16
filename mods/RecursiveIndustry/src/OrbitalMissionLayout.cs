namespace RecursiveIndustry;

internal static class OrbitalMissionLayout
{
    public const string PrefabPath =
        "Assets/Base/Machines/Electronics/MicrochipMachineT2.prefab";

    public static string[] Create()
    {
        return new[]
        {
            "      D#vF#vB#vC#vE@v      ",
            "   [2][2][2][2][2][2][2]   ",
            "A#>[2][2][3][3][3][2][2]>#X",
            "   [2][2][2][2][2][2][2]>#Y",
            "      [2][2][2][2][2]      ",
        };
    }
}