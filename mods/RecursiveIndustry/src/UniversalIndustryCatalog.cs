using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;

namespace RecursiveIndustry;

internal sealed class UniversalFacilitySpec
{
    public readonly string Key;
    public readonly string Name;
    public readonly MachineProto.ID Id;
    public readonly string IconPath;
    public readonly int PowerMw;
    public readonly int Computing;
    public readonly int Workers;
    public readonly int MaintenanceT3;
    public readonly int Cp4;
    public readonly int Electronics4;
    public readonly int Packages;
    public readonly int Programs;
    public readonly int Dossiers;
    public readonly int Calibration;
    public readonly UniversalDirectBindingSpec[] DirectBindings;

    public UniversalFacilitySpec(
        string key,
        string name,
        MachineProto.ID id,
        string iconPath,
        int powerMw,
        int computing,
        int workers,
        int maintenanceT3,
        int cp4,
        int electronics4,
        int packages,
        int programs,
        int dossiers,
        int calibration,
        UniversalDirectBindingSpec[] directBindings)
    {
        Key = key;
        Name = name;
        Id = id;
        IconPath = iconPath;
        PowerMw = powerMw;
        Computing = computing;
        Workers = workers;
        MaintenanceT3 = maintenanceT3;
        Cp4 = cp4;
        Electronics4 = electronics4;
        Packages = packages;
        Programs = programs;
        Dossiers = dossiers;
        Calibration = calibration;
        DirectBindings = directBindings;
    }
}

internal sealed class UniversalDirectBindingSpec
{
    public readonly string RecipeId;
    public readonly string SourceMachineId;

    public UniversalDirectBindingSpec(string recipeId, string sourceMachineId)
    {
        RecipeId = recipeId;
        SourceMachineId = sourceMachineId;
    }
}

internal sealed class UniversalSourceRecipeSpec
{
    public readonly string RecipeId;
    public readonly int Multiplier;

    public UniversalSourceRecipeSpec(string recipeId, int multiplier)
    {
        RecipeId = recipeId;
        Multiplier = multiplier;
    }
}

internal sealed class UniversalIntegratedRecipeSpec
{
    public readonly RecipeProto.ID Id;
    public readonly string Name;
    public readonly string MachineKey;
    public readonly int BatchScale;
    public readonly int DurationSeconds;
    public readonly UniversalSourceRecipeSpec[] Sources;

    public UniversalIntegratedRecipeSpec(
        RecipeProto.ID id,
        string name,
        string machineKey,
        int batchScale,
        int durationSeconds,
        UniversalSourceRecipeSpec[] sources)
    {
        Id = id;
        Name = name;
        MachineKey = machineKey;
        BatchScale = batchScale;
        DurationSeconds = durationSeconds;
        Sources = sources;
    }
}

internal sealed class UniversalPrecisionRecipeSpec
{
    public readonly RecipeProto.ID Id;
    public readonly string Name;
    public readonly string MachineKey;
    public readonly string SourceRecipeId;

    public UniversalPrecisionRecipeSpec(
        RecipeProto.ID id,
        string name,
        string machineKey,
        string sourceRecipeId)
    {
        Id = id;
        Name = name;
        MachineKey = machineKey;
        SourceRecipeId = sourceRecipeId;
    }
}