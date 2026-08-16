namespace RecursiveIndustry;

internal static class FrontierProjectLayout
{
    public const string PrefabPath = "Assets/Base/Machines/Oil/ReformerT2.prefab";
    public const string SoundPath =
        "Assets/Base/Machines/Assembly/AssemblyT4/AssemblerSound.prefab";

    public static string[] Create()
    {
        return new[]
        {
            "A#>[7][7][7][6][5][5][5]   ",
            "B#>[7][7][7][6][5][5][5]   ",
            "C#>[6][6][6][6][5][5][5]   ",
            "D#>[5][5][5][5][5][5][5]>#X",
            "E#>[5][5][5][5][5][5][5]   ",
            "F#>[5][5][5][5][5][5][5]   ",
            "   [5][5][5][5][5][5][5]   ",
        };
    }
}