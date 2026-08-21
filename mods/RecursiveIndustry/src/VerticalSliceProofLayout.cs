namespace RecursiveIndustry;

internal static class VerticalSliceProofLayout
{
    public const string PrefabPath = "Assets/Base/Machines/Assembly/AssemblyT5.prefab";
    public const string SoundPath =
        "Assets/Base/Machines/Assembly/AssemblyT4/AssemblerSound.prefab";

    public static string[] Create(bool includeThirdInput = true)
    {
        return new[]
        {
            "   [4][4][4][4][4][4]   ",
            "A#>[4][4][4][4][4][4]>#X",
            "B#>[4][4][4][4][4][4]>#Y",
            (includeThirdInput ? "C#>" : "   ") + "[5][5][4][4][4][4]>~Z",
            "   [5][5][4][4][4][4]   ",
        };
    }
}