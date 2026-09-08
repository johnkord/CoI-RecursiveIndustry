using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.UnlockingTree;

namespace RecursiveIndustry;

internal static class ReconstructionResearchPrerequisites
{
    public static void AddSources(
        ProtoRegistrator registrator,
        ResearchNodeProto target,
        params MachineProto.ID[] facilityIds)
    {
        var required = new HashSet<Proto>();
        var selectedKeys = new HashSet<string>();
        var sourceRecipes = new HashSet<string>();
        foreach (UniversalFacilitySpec facility in UniversalIndustryCatalog.Facilities)
        {
            if (!facilityIds.Contains(facility.Id))
                continue;
            selectedKeys.Add(facility.Key);
            foreach (UniversalDirectBindingSpec binding in facility.DirectBindings)
            {
                required.Add(registrator.PrototypesDb.GetOrThrow<MachineProto>(
                    new MachineProto.ID(binding.SourceMachineId)));
            }
        }
        if (selectedKeys.Count != facilityIds.Length)
            throw new InvalidOperationException("Reconstruction prerequisite facility inventory mismatch.");
        var unlockedRecipes = new HashSet<RecipeProto.ID>();
        foreach (IUnlockNodeUnit unit in target.Units)
        {
            if (!(unit is IProtoUnlock unlock))
                continue;
            foreach (IProto proto in unlock.UnlockedProtos)
                if (proto is RecipeProto recipe)
                    unlockedRecipes.Add(recipe.Id);
        }
        foreach (UniversalIntegratedRecipeSpec recipe in UniversalIndustryCatalog.IntegratedRecipes)
        {
            if (!unlockedRecipes.Contains(recipe.Id))
                continue;
            foreach (UniversalSourceRecipeSpec source in recipe.Sources)
            {
                sourceRecipes.Add(source.RecipeId);
                string machineId = source.SourceMachineId;
                if (string.IsNullOrEmpty(machineId))
                {
                    machineId = UniversalIndustryCatalog.Facilities
                        .SelectMany(facility => facility.DirectBindings)
                        .Single(binding => binding.RecipeId == source.RecipeId).SourceMachineId;
                }
                required.Add(registrator.PrototypesDb.GetOrThrow<MachineProto>(new MachineProto.ID(machineId)));
            }
        }
        foreach (UniversalPrecisionRecipeSpec recipe in UniversalIndustryCatalog.PrecisionRecipes)
        {
            if (unlockedRecipes.Contains(recipe.Id))
                sourceRecipes.Add(recipe.SourceRecipeId);
        }
        foreach (string recipeId in sourceRecipes)
            required.Add(registrator.PrototypesDb.GetOrThrow<RecipeProto>(new RecipeProto.ID(recipeId)));

        var owners = new Dictionary<Proto, List<ResearchNodeProto>>();
        foreach (ResearchNodeProto node in registrator.PrototypesDb.All<ResearchNodeProto>())
        {
            if (node.Mod == registrator.ActiveMod)
                continue;
            foreach (IUnlockNodeUnit unit in node.Units)
            {
                if (!(unit is IProtoUnlock unlock))
                    continue;
                foreach (IProto unlocked in unlock.UnlockedProtos)
                {
                    if (!(unlocked is Proto proto) || !required.Contains(proto))
                        continue;
                    if (!owners.TryGetValue(proto, out List<ResearchNodeProto> nodes))
                    {
                        nodes = new List<ResearchNodeProto>();
                        owners.Add(proto, nodes);
                    }
                    nodes.Add(node);
                }
            }
        }

        var parents = new HashSet<ResearchNodeProto>();
        foreach (Proto proto in required)
        {
            if (!owners.TryGetValue(proto, out List<ResearchNodeProto> nodes))
            {
                if (proto is MachineProto)
                    throw new InvalidOperationException("No native equipment research owner for " + proto.Id);
                continue;
            }
            parents.Add(nodes.OrderBy(node => node.GridPosition.X)
                .ThenBy(node => node.GridPosition.Y)
                .ThenBy(node => node.Id.Value, StringComparer.Ordinal).First());
        }
        foreach (ResearchNodeProto parent in parents.OrderBy(node => node.Id.Value, StringComparer.Ordinal))
            target.AddParent(parent);
        Log.Info("RecursiveIndustry: RECONSTRUCTION_NATIVE_PARENTS research=" + target.Id
            + " required=" + required.Count + " parents=" + parents.Count);
    }
}