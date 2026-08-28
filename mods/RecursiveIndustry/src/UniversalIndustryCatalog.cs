using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Products;

namespace RecursiveIndustry;

internal enum UniversalMaintenanceTier
{
    I,
    II,
    III,
}

internal sealed class UniversalFacilitySpec
{
    public readonly string Key;
    public readonly string Name;
    public readonly MachineProto.ID Id;
    public readonly string IconPath;
    public readonly int PowerKw;
    public readonly int Computing;
    public readonly int Workers;
    public readonly UniversalMaintenanceTier MaintenanceTier;
    public readonly int MaintenancePerMonth;
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
        int powerKw,
        int computing,
        int workers,
        UniversalMaintenanceTier maintenanceTier,
        int maintenancePerMonth,
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
        PowerKw = powerKw;
        Computing = computing;
        Workers = workers;
        MaintenanceTier = maintenanceTier;
        MaintenancePerMonth = maintenancePerMonth;
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
    public readonly string SourceMachineId;

    public UniversalSourceRecipeSpec(
        string recipeId,
        int multiplier,
        string sourceMachineId = null)
    {
        RecipeId = recipeId;
        Multiplier = multiplier;
        SourceMachineId = sourceMachineId;
    }
}

internal sealed class UniversalIntegratedRecipeSpec
{
    public readonly RecipeProto.ID Id;
    public readonly string Name;
    public readonly string MachineKey;
    public readonly int BatchScale;
    public readonly int DurationSeconds;
    public readonly int PowerMultiplierPercent;
    public readonly UniversalSourceRecipeSpec[] Sources;

    public UniversalIntegratedRecipeSpec(
        RecipeProto.ID id,
        string name,
        string machineKey,
        int batchScale,
        int durationSeconds,
        int powerMultiplierPercent,
        UniversalSourceRecipeSpec[] sources)
    {
        Id = id;
        Name = name;
        MachineKey = machineKey;
        BatchScale = batchScale;
        DurationSeconds = durationSeconds;
        PowerMultiplierPercent = powerMultiplierPercent;
        Sources = sources;
    }
}

internal sealed class UniversalProductAmountSpec
{
    public readonly ProductProto.ID ProductId;
    public readonly int Quantity;
    public readonly bool TriggerAtStart;

    public UniversalProductAmountSpec(
        ProductProto.ID productId,
        int quantity,
        bool triggerAtStart = false)
    {
        ProductId = productId;
        Quantity = quantity;
        TriggerAtStart = triggerAtStart;
    }
}

internal sealed class UniversalAuthoredRecipeSpec
{
    public readonly RecipeProto.ID Id;
    public readonly string Name;
    public readonly string MachineKey;
    public readonly int DurationSeconds;
    public readonly int PowerMultiplierPercent;
    public readonly UniversalProductAmountSpec[] Inputs;
    public readonly UniversalProductAmountSpec[] Outputs;

    public UniversalAuthoredRecipeSpec(
        RecipeProto.ID id,
        string name,
        string machineKey,
        int durationSeconds,
        int powerMultiplierPercent,
        UniversalProductAmountSpec[] inputs,
        UniversalProductAmountSpec[] outputs)
    {
        Id = id;
        Name = name;
        MachineKey = machineKey;
        DurationSeconds = durationSeconds;
        PowerMultiplierPercent = powerMultiplierPercent;
        Inputs = inputs;
        Outputs = outputs;
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