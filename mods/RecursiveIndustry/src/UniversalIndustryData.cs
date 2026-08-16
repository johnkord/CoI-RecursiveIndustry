using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace RecursiveIndustry;

internal sealed class UniversalIndustryData : IModData
{
    public void RegisterData(ProtoRegistrator registrator)
    {
        var directByRecipe = ResolveDirectBindings(registrator.PrototypesDb);
        var integrated = UniversalIndustryCatalog.IntegratedRecipes
            .Select(spec => new ResolvedCustomRecipe(spec, Compose(spec, directByRecipe)))
            .ToArray();
        var precision = UniversalIndustryCatalog.PrecisionRecipes
            .Select(spec => new ResolvedCustomRecipe(spec, BuildPrecision(spec, directByRecipe)))
            .ToArray();

        int directCount = 0;
        foreach (UniversalFacilitySpec facility in UniversalIndustryCatalog.Facilities)
        {
            ResolvedDirectBinding[] direct = facility.DirectBindings
                .Select(spec => directByRecipe[spec.RecipeId])
                .ToArray();
            ResolvedCustomRecipe[] custom = integrated
                .Concat(precision)
                .Where(recipe => recipe.MachineKey == facility.Key)
                .ToArray();
            UniversalPortPlan ports = UniversalPortPlan.Create(
                direct.Select(binding => RecipeVector.FromRecipe(binding.Recipe))
                    .Concat(custom.Select(recipe => recipe.Vector)));
            MachineProto machine = BuildMachine(registrator, facility, direct, ports);

            foreach (ResolvedDirectBinding binding in direct)
            {
                Bind(
                    binding.Recipe,
                    machine,
                    binding.SourceBinding.Duration,
                    multiplier: checked(binding.SourceBinding.Multiplier * 4),
                    binding.SourceBinding.MinPartialUtilization,
                    RecipeVector.FromRecipe(binding.Recipe),
                    ports);
                directCount++;
            }

            foreach (ResolvedCustomRecipe recipe in custom)
            {
                RegisterCustomRecipe(registrator, machine, recipe, ports, directByRecipe);
            }
        }

        if (directCount != 234)
        {
            throw new InvalidOperationException(
                $"Universal Industry expected 234 direct bindings, registered {directCount}.");
        }
        Log.Info(
            "RecursiveIndustry: Universal Industry registered "
            + UniversalIndustryCatalog.Facilities.Length
            + " facilities, " + directCount + " direct bindings, "
            + integrated.Length + " Integrated recipes, and "
            + precision.Length + " Precision recipes");
    }

    private static Dictionary<string, ResolvedDirectBinding> ResolveDirectBindings(ProtosDb db)
    {
        var result = new Dictionary<string, ResolvedDirectBinding>(StringComparer.Ordinal);
        foreach (UniversalFacilitySpec facility in UniversalIndustryCatalog.Facilities)
        {
            foreach (UniversalDirectBindingSpec spec in facility.DirectBindings)
            {
                if (result.ContainsKey(spec.RecipeId))
                {
                    throw new InvalidOperationException(
                        $"Universal Industry direct recipe '{spec.RecipeId}' has multiple owners.");
                }
                var recipeId = new RecipeProto.ID(spec.RecipeId);
                var sourceMachineId = new MachineProto.ID(spec.SourceMachineId);
                RecipeProto recipe = db.GetOrThrow<RecipeProto>(recipeId);
                MachineProto sourceMachine = db.GetOrThrow<MachineProto>(sourceMachineId);
                MachineRecipeBinding sourceBinding = FindBinding(sourceMachine, recipe);
                if (sourceBinding.Multiplier <= 0)
                {
                    throw new InvalidOperationException(
                        $"Universal Industry source '{spec.RecipeId}' uses non-positive multiplier "
                        + sourceBinding.Multiplier + ".");
                }
                result.Add(
                    spec.RecipeId,
                    new ResolvedDirectBinding(recipe, sourceMachine, sourceBinding));
            }
        }
        return result;
    }

    private static MachineRecipeBinding FindBinding(MachineProto machine, RecipeProto recipe)
    {
        foreach (MachineRecipeBinding binding in machine.RecipeBindings)
        {
            if (binding.Recipe == recipe)
            {
                return binding;
            }
        }
        throw new InvalidOperationException(
            $"Recipe '{recipe.Id}' is not bound to declared source machine '{machine.Id}'.");
    }

    private static RecipeVector Compose(
        UniversalIntegratedRecipeSpec spec,
        IReadOnlyDictionary<string, ResolvedDirectBinding> directByRecipe)
    {
        var quantities = new Dictionary<ProductProto, int>();
        var triggerAtStart = new Dictionary<ProductProto, bool>();
        foreach (UniversalSourceRecipeSpec source in spec.Sources)
        {
            ResolvedDirectBinding binding = directByRecipe[source.RecipeId];
            RecipeProto recipe = binding.Recipe;
            int multiplier = checked(source.Multiplier * binding.SourceBinding.Multiplier);
            foreach (RecipeInput input in recipe.AllInputs)
            {
                AddQuantity(
                    quantities,
                    input.Product,
                    checked(-input.Quantity.Value * multiplier));
            }
            foreach (RecipeOutput output in recipe.AllOutputs)
            {
                AddQuantity(
                    quantities,
                    output.Product,
                    checked(output.Quantity.Value * multiplier));
                if (output.TriggerAtStart)
                {
                    triggerAtStart[output.Product] = true;
                }
            }
        }
        return RecipeVector.FromNet(quantities, triggerAtStart, spec.BatchScale);
    }

    private static RecipeVector BuildPrecision(
        UniversalPrecisionRecipeSpec spec,
        IReadOnlyDictionary<string, ResolvedDirectBinding> directByRecipe)
    {
        ResolvedDirectBinding binding = directByRecipe[spec.SourceRecipeId];
        RecipeProto source = binding.Recipe;
        int sourceMultiplier = binding.SourceBinding.Multiplier;
        return new RecipeVector(
            source.AllInputs
                .Select(input => new ProductAmount(
                    input.Product,
                    checked(input.Quantity.Value * sourceMultiplier * 7),
                    false))
                .ToArray(),
            source.AllOutputs
                .Select(output => new ProductAmount(
                    output.Product,
                    checked(output.Quantity.Value * sourceMultiplier * 8),
                    output.TriggerAtStart))
                .ToArray());
    }

    private static void AddQuantity(
        IDictionary<ProductProto, int> quantities,
        ProductProto product,
        int quantity)
    {
        quantities.TryGetValue(product, out int current);
        quantities[product] = checked(current + quantity);
    }

    private static MachineProto BuildMachine(
        ProtoRegistrator registrator,
        UniversalFacilitySpec spec,
        ResolvedDirectBinding[] direct,
        UniversalPortPlan ports)
    {
        EntityCostsTpl.Builder costs = Costs.Build
            .CP4(spec.Cp4)
            .Product(spec.Electronics4, Ids.Products.Electronics4)
            .Product(spec.Packages, RecursiveIndustryIds.Products.ValidatedControlPackage)
            .Product(spec.Programs, RecursiveIndustryIds.Products.FrontierProgram);
        if (spec.Dossiers > 0)
        {
            costs = costs.Product(
                spec.Dossiers,
                RecursiveIndustryIds.Products.ValidatedResearchDossier);
        }
        if (spec.Calibration > 0)
        {
            costs = costs.Product(
                spec.Calibration,
                RecursiveIndustryIds.Products.OrbitalPowerCalibration);
        }
        costs = costs.Workers(spec.Workers);
        bool exactMaintenance = TryApplySameTierMaintenance(
            costs,
            direct,
            out EntityCostsTpl.Builder resolvedCosts);
        costs = exactMaintenance
            ? resolvedCosts
            : costs.MaintenanceT3(spec.MaintenanceT3);

        int sourceEquivalentPowerKw = direct.Max(binding => checked(
            (int)(binding.SourceMachine.ElectricityConsumed.Value / Electricity.OneKw.Value)
            * 4));
        int facilityPowerKw = Math.Max(
            spec.PowerMw * 1000,
            checked((sourceEquivalentPowerKw * 5 + 3) / 4));
        bool useChemicalPlantBasis = ports.RequiredRows > 5;
        string prefabPath = useChemicalPlantBasis
            ? "Assets/Base/Machines/Oil/ReformerT2.prefab"
            : SystemsIntegrationLayout.PrefabPath;

        MachineProto machine;
        if (useChemicalPlantBasis)
        {
            machine = registrator.MachineProtoBuilder
                .Start(spec.Name, spec.Id)
                .Description(
                    "A high-power AI megafacility with exact 4x Direct bindings and bounded "
                    + "Integrated or Precision modes. Conventional source plants remain available.")
                .SetCost(costs)
                .SetElectricityConsumption(facilityPowerKw.Kw())
                .SetComputingConsumption(Computing.FromTFlops(spec.Computing))
                .SetCategories(Ids.ToolbarCategories.Production_General)
                .SetLayout(new EntityLayoutParams(), ports.LayoutRows)
                .SetPrefabPath(prefabPath)
                .SetCustomIconPath(spec.IconPath)
                .BuildAndAdd();
        }
        else
        {
            machine = registrator.MachineProtoBuilder
                .Start(spec.Name, spec.Id)
                .Description(
                    "A high-power AI megafacility with exact 4x Direct bindings and bounded "
                    + "Integrated or Precision modes. Conventional source plants remain available.")
                .SetCost(costs)
                .SetElectricityConsumption(facilityPowerKw.Kw())
                .SetComputingConsumption(Computing.FromTFlops(spec.Computing))
                .SetCategories(Ids.ToolbarCategories.Production_General)
                .SetLayout(new EntityLayoutParams(), ports.LayoutRows)
                .SetPrefabPath(prefabPath)
                .SetCustomIconPath(spec.IconPath)
                .SetMachineSound(SystemsIntegrationLayout.SoundPath)
                .BuildAndAdd();
        }
        Log.Info(
            "RecursiveIndustry: Universal facility " + spec.Key
            + " power_kw=" + facilityPowerKw
            + " source_equivalent_power_kw=" + sourceEquivalentPowerKw
            + " maintenance=" + machine.Costs.Maintenance
            + " exact_same_tier_maintenance=" + exactMaintenance
            + " layout_rows=" + ports.RequiredRows
            + " presentation=" + (useChemicalPlantBasis ? "chemical_plant_ii" : "assembly_v"));
        return machine;
    }

    private static bool TryApplySameTierMaintenance(
        EntityCostsTpl.Builder costs,
        ResolvedDirectBinding[] direct,
        out EntityCostsTpl.Builder resolved)
    {
        var sourceCosts = direct
            .Select(binding => binding.SourceMachine.Costs.Maintenance)
            .Where(maintenance => maintenance.Product != null)
            .ToArray();
        if (sourceCosts.Length != direct.Length || sourceCosts.Length == 0)
        {
            resolved = costs;
            return false;
        }
        ProductProto product = sourceCosts[0].Product;
        if (sourceCosts.Any(maintenance => maintenance.Product.Id != product.Id))
        {
            resolved = costs;
            return false;
        }
        Fix32 minimum = sourceCosts[0].MaintenancePerMonth.Value;
        foreach (var source in sourceCosts)
        {
            if (source.MaintenancePerMonth.Value < minimum)
            {
                minimum = source.MaintenancePerMonth.Value;
            }
        }
        resolved = costs.Maintenance(minimum * 3, product.Id);
        return true;
    }

    private static void RegisterCustomRecipe(
        ProtoRegistrator registrator,
        MachineProto machine,
        ResolvedCustomRecipe resolved,
        UniversalPortPlan ports,
        IReadOnlyDictionary<string, ResolvedDirectBinding> directByRecipe)
    {
        RecipeProto.ID id;
        string name;
        Duration duration;
        if (resolved.Spec is UniversalIntegratedRecipeSpec integrated)
        {
            id = integrated.Id;
            name = integrated.Name;
            duration = integrated.DurationSeconds.Seconds();
        }
        else if (resolved.Spec is UniversalPrecisionRecipeSpec precision)
        {
            id = precision.Id;
            name = precision.Name;
            duration = directByRecipe[precision.SourceRecipeId].SourceBinding.Duration * 2;
        }
        else
        {
            throw new InvalidOperationException("Unknown universal recipe specification.");
        }

        RecipeProtoBuilder.State builder = registrator.RecipeProtoBuilder
            .Start(id)
            .Description(name + ". High-power validated industrial optimization.");
        foreach (ProductAmount input in resolved.Vector.Inputs)
        {
            builder.AddInput(input.Product, new Quantity(input.Quantity));
        }
        foreach (ProductAmount output in resolved.Vector.Outputs)
        {
            builder.AddOutput(
                output.Product,
                new Quantity(output.Quantity),
                triggerAtStart: output.TriggerAtStart);
        }
        RecipeProto recipe = builder
            .SetPowerMultiplier(200.Percent())
            .BuildAndAdd()
            .Recipe;
        Bind(recipe, machine, duration, 1, null, resolved.Vector, ports);
    }

    private static void Bind(
        RecipeProto recipe,
        MachineProto machine,
        Duration duration,
        int multiplier,
        Percent? minPartialUtilization,
        RecipeVector vector,
        UniversalPortPlan ports)
    {
        Duration transportFloor = GetTransportDurationFloor(vector, multiplier);
        if (duration < transportFloor)
        {
            Log.Info(
                "RecursiveIndustry: Universal recipe " + recipe.Id
                + " duration raised from " + duration
                + " to " + transportFloor
                + " for single-port transport capacity");
            duration = transportFloor;
        }
        recipe
            .WithCommonInputPorts(ports.MapInputs(vector))
            .WithCommonOutputPorts(ports.MapOutputs(vector))
            .BindTo(machine, duration, multiplier, minPartialUtilization);
    }

    private static Duration GetTransportDurationFloor(RecipeVector vector, int multiplier)
    {
        int requiredSeconds = 0;
        foreach (ProductAmount amount in vector.Inputs.Concat(vector.Outputs))
        {
            int quantity = checked(amount.Quantity * multiplier);
            int seconds;
            if (amount.Product is CountableProductProto || amount.Product is LooseProductProto)
            {
                seconds = CeilDiv(checked(quantity * 2), 15);
            }
            else if (amount.Product is MoltenProductProto)
            {
                seconds = CeilDiv(quantity, 2);
            }
            else if (amount.Product is FluidProductProto)
            {
                seconds = CeilDiv(quantity, 15);
            }
            else
            {
                continue;
            }
            requiredSeconds = Math.Max(requiredSeconds, seconds);
        }
        return requiredSeconds.Seconds();
    }

    private static int CeilDiv(int numerator, int denominator)
    {
        return checked((numerator + denominator - 1) / denominator);
    }

    private sealed class ResolvedDirectBinding
    {
        public readonly RecipeProto Recipe;
        public readonly MachineProto SourceMachine;
        public readonly MachineRecipeBinding SourceBinding;

        public ResolvedDirectBinding(
            RecipeProto recipe,
            MachineProto sourceMachine,
            MachineRecipeBinding sourceBinding)
        {
            Recipe = recipe;
            SourceMachine = sourceMachine;
            SourceBinding = sourceBinding;
        }
    }

    private sealed class ResolvedCustomRecipe
    {
        public readonly object Spec;
        public readonly RecipeVector Vector;

        public string MachineKey => Spec is UniversalIntegratedRecipeSpec integrated
            ? integrated.MachineKey
            : ((UniversalPrecisionRecipeSpec)Spec).MachineKey;

        public ResolvedCustomRecipe(object spec, RecipeVector vector)
        {
            Spec = spec;
            Vector = vector;
        }
    }

    private sealed class RecipeVector
    {
        public readonly ProductAmount[] Inputs;
        public readonly ProductAmount[] Outputs;

        public RecipeVector(ProductAmount[] inputs, ProductAmount[] outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }

        public static RecipeVector FromRecipe(RecipeProto recipe)
        {
            return new RecipeVector(
                recipe.AllInputs
                    .Select(input => new ProductAmount(
                        input.Product,
                        input.Quantity.Value,
                        false))
                    .ToArray(),
                recipe.AllOutputs
                    .Select(output => new ProductAmount(
                        output.Product,
                        output.Quantity.Value,
                        output.TriggerAtStart))
                    .ToArray());
        }

        public static RecipeVector FromNet(
            IReadOnlyDictionary<ProductProto, int> quantities,
            IReadOnlyDictionary<ProductProto, bool> triggerAtStart,
            int scale)
        {
            ProductAmount[] inputs = quantities
                .Where(item => item.Value < 0)
                .OrderBy(item => item.Key.Id.Value, StringComparer.Ordinal)
                .Select(item => new ProductAmount(
                    item.Key,
                    checked(-item.Value * scale),
                    false))
                .ToArray();
            ProductAmount[] outputs = quantities
                .Where(item => item.Value > 0)
                .OrderBy(item => item.Key.Id.Value, StringComparer.Ordinal)
                .Select(item => new ProductAmount(
                    item.Key,
                    checked(item.Value * scale),
                    triggerAtStart.TryGetValue(item.Key, out bool trigger) && trigger))
                .ToArray();
            return new RecipeVector(inputs, outputs);
        }
    }

    private sealed class ProductAmount
    {
        public readonly ProductProto Product;
        public readonly int Quantity;
        public readonly bool TriggerAtStart;

        public ProductAmount(ProductProto product, int quantity, bool triggerAtStart)
        {
            Product = product;
            Quantity = quantity;
            TriggerAtStart = triggerAtStart;
        }
    }

    private sealed class UniversalPortPlan
    {
        private const string Names = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly char[] KindOrder = { '#', '~', '\'', '@' };

        private readonly Dictionary<char, string[]> m_inputPorts;
        private readonly Dictionary<char, string[]> m_outputPorts;

        public readonly string[] LayoutRows;

        public readonly int RequiredRows;

        private UniversalPortPlan(
            Dictionary<char, string[]> inputPorts,
            Dictionary<char, string[]> outputPorts,
            string[] layoutRows)
        {
            m_inputPorts = inputPorts;
            m_outputPorts = outputPorts;
            LayoutRows = layoutRows;
            RequiredRows = Math.Max(
                inputPorts.Values.Sum(names => names.Length),
                outputPorts.Values.Sum(names => names.Length));
        }

        public static UniversalPortPlan Create(IEnumerable<RecipeVector> vectors)
        {
            RecipeVector[] all = vectors.ToArray();
            Dictionary<char, int> inputCounts = MaxCounts(all.Select(vector => vector.Inputs));
            Dictionary<char, int> outputCounts = MaxCounts(all.Select(vector => vector.Outputs));
            int nextName = 0;
            Dictionary<char, string[]> inputs = Allocate(inputCounts, ref nextName);
            Dictionary<char, string[]> outputs = Allocate(outputCounts, ref nextName);
            if (nextName > Names.Length)
            {
                throw new InvalidOperationException(
                    $"Universal Industry layout needs {nextName} ports; maximum is {Names.Length}.");
            }

            var flatInputs = Flatten(inputs);
            var flatOutputs = Flatten(outputs);
            int rows = Math.Max(flatInputs.Count, flatOutputs.Count);
            if (rows > 7)
            {
                throw new InvalidOperationException(
                    $"Universal Industry layout needs {rows} port rows; Chemical Plant II supports 7.");
            }
            bool useChemicalPlantBasis = rows > 5;
            int layoutRows = useChemicalPlantBasis ? 7 : Math.Max(rows, 5);
            string body = useChemicalPlantBasis
                ? "[7][7][7][6][5][5][5]"
                : "[4][4][4][4][4][4]";
            var layout = new string[layoutRows];
            for (int index = 0; index < layoutRows; index++)
            {
                string input = index < flatInputs.Count
                    ? flatInputs[index].name + flatInputs[index].kind + ">"
                    : "   ";
                string output = index < flatOutputs.Count
                    ? ">" + flatOutputs[index].kind + flatOutputs[index].name
                    : "   ";
                layout[index] = input + body + output;
            }
            return new UniversalPortPlan(inputs, outputs, layout);
        }

        public (ProductProto.ID product, string port)[] MapInputs(RecipeVector vector)
        {
            return Map(vector.Inputs, m_inputPorts);
        }

        public (ProductProto.ID product, string port)[] MapOutputs(RecipeVector vector)
        {
            return Map(vector.Outputs, m_outputPorts);
        }

        private static (ProductProto.ID product, string port)[] Map(
            ProductAmount[] products,
            IReadOnlyDictionary<char, string[]> ports)
        {
            var counters = new Dictionary<char, int>();
            var result = new List<(ProductProto.ID product, string port)>();
            foreach (ProductAmount product in products)
            {
                if (!TryGetKind(product.Product, out char kind))
                {
                    continue;
                }
                counters.TryGetValue(kind, out int index);
                result.Add((product.Product.Id, ports[kind][index]));
                counters[kind] = index + 1;
            }
            return result.ToArray();
        }

        private static Dictionary<char, int> MaxCounts(
            IEnumerable<ProductAmount[]> vectors)
        {
            var maximum = new Dictionary<char, int>();
            foreach (ProductAmount[] vector in vectors)
            {
                var current = new Dictionary<char, int>();
                foreach (ProductAmount product in vector)
                {
                    if (!TryGetKind(product.Product, out char kind))
                    {
                        continue;
                    }
                    current.TryGetValue(kind, out int count);
                    current[kind] = count + 1;
                }
                foreach (KeyValuePair<char, int> item in current)
                {
                    maximum.TryGetValue(item.Key, out int previous);
                    maximum[item.Key] = Math.Max(previous, item.Value);
                }
            }
            return maximum;
        }

        private static Dictionary<char, string[]> Allocate(
            IReadOnlyDictionary<char, int> counts,
            ref int nextName)
        {
            var result = new Dictionary<char, string[]>();
            foreach (char kind in KindOrder)
            {
                counts.TryGetValue(kind, out int count);
                var names = new string[count];
                for (int index = 0; index < count; index++)
                {
                    if (nextName >= Names.Length)
                    {
                        nextName++;
                        continue;
                    }
                    names[index] = Names[nextName++].ToString();
                }
                result[kind] = names;
            }
            return result;
        }

        private static List<(char kind, string name)> Flatten(
            IReadOnlyDictionary<char, string[]> ports)
        {
            var result = new List<(char kind, string name)>();
            foreach (char kind in KindOrder)
            {
                result.AddRange(ports[kind].Select(name => (kind, name)));
            }
            return result;
        }

        private static bool TryGetKind(ProductProto product, out char kind)
        {
            if (product is VirtualProductProto)
            {
                kind = default;
                return false;
            }
            if (product is CountableProductProto)
            {
                kind = '#';
                return true;
            }
            if (product is LooseProductProto)
            {
                kind = '~';
                return true;
            }
            if (product is MoltenProductProto)
            {
                kind = '\'';
                return true;
            }
            if (product is FluidProductProto)
            {
                kind = '@';
                return true;
            }
            throw new InvalidOperationException(
                $"Unsupported Universal Industry product type '{product.GetType().FullName}' "
                + $"for product '{product.Id}'.");
        }
    }
}