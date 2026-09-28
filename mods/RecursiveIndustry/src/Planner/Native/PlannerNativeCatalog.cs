using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.Trains;
using Mafi.Core.UnlockingTree;

namespace RecursiveIndustry.Planner.Native;

internal static class PlannerNativeCatalog
{
    internal static PlannerCatalog Build(ProtosDb database)
    {
        bool Available(Proto proto) => proto.IsUnlockedAndAvailable;
        var owners = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (ResearchNodeProto research in database.All<ResearchNodeProto>())
            foreach (IUnlockNodeUnit unit in research.Units)
                if (unit is IProtoUnlock unlock)
                    foreach (IProto proto in unlock.UnlockedProtos)
                    {
                        if (!owners.TryGetValue(proto.Id.Value, out List<string> names)) owners[proto.Id.Value] = names = new List<string>();
                        names.Add(research.Strings.Name.TranslatedString);
                    }
        string Unlocks(params Proto[] protos) => string.Join(", ", protos.SelectMany(proto =>
            owners.TryGetValue(proto.Id.Value, out List<string> names) ? names : Enumerable.Empty<string>()).Distinct().OrderBy(name => name, StringComparer.Ordinal));
        var nativeHosts = new HashSet<string>(UniversalIndustryCatalog.Facilities.SelectMany(facility => facility.DirectBindings)
            .Select(binding => binding.SourceMachineId), StringComparer.Ordinal);
        foreach (UniversalIntegratedRecipeSpec recipe in UniversalIndustryCatalog.IntegratedRecipes)
            foreach (UniversalSourceRecipeSpec source in recipe.Sources)
                if (!string.IsNullOrEmpty(source.SourceMachineId)) nativeHosts.Add(source.SourceMachineId);
        var processes = new List<PlannerProcess>();
        foreach (MachineProto machine in database.All<MachineProto>())
        {
            if (machine.Mod is not RecursiveIndustry && !nativeHosts.Contains(machine.Id.Value)) continue;
            if (machine.EntityType != typeof(Machine)) continue;
            foreach (MachineRecipeBinding binding in machine.RecipeBindings)
                processes.Add(Copy(machine, binding, Available(machine) && Available(binding.Recipe), Unlocks(machine, binding.Recipe)));
        }
        var products = database.All<ProductProto>().ToDictionary(product => product.Id.Value,
            product => product.Strings.Name.TranslatedString, StringComparer.Ordinal);
        var references = new List<PlannerReference>();
        foreach (EntityProto proto in database.All<EntityProto>())
            if (proto.Mod is RecursiveIndustry && proto is not MachineProto)
                references.Add(new PlannerReference(proto.Id.Value, proto.Strings.Name.TranslatedString,
                    proto.GetType().Name.Replace("Proto", ""), proto.Strings.DescShort.TranslatedString, Unlocks(proto),
                    Available(proto), proto.Costs.Workers, proto is LayoutEntityProto layout ? layout.Layout.TilesCount : -1,
                    Capital(proto.Costs), Maintenance(proto.Costs), proto is LayoutEntityProto ? Section(proto.Id.Value) : "Mobility"));
        foreach (TrainCarBaseProto proto in database.All<TrainCarBaseProto>())
            if (proto.Mod is RecursiveIndustry && !references.Any(row => row.Id == proto.Id.Value))
                references.Add(new PlannerReference(proto.Id.Value, proto.Strings.Name.TranslatedString, "Train",
                    proto.Strings.DescShort.TranslatedString, Unlocks(proto), Available(proto), proto.Costs.Workers, -1,
                    Capital(proto.Costs), Maintenance(proto.Costs), "Mobility"));
        var shells = new List<PlannerShell>();
        DataCenterProto coolantBasis = null;
        foreach (DataCenterProto shell in database.All<DataCenterProto>())
        {
            if (shell.Mod is not RecursiveIndustry && shell.Mod is not Mafi.Base.BaseMod) continue;
            coolantBasis ??= shell;
            shells.Add(new PlannerShell(shell.Id.Value, shell.Strings.Name.TranslatedString, Available(shell), shell.RacksCapacity,
                shell.Costs.Workers, shell.Layout.TilesCount, Capital(shell.Costs), Maintenance(shell.Costs)));
        }
        var racks = new List<PlannerRack>();
        foreach (ServerRackProto rack in database.All<ServerRackProto>())
        {
            if (rack.Mod is not RecursiveIndustry && rack.Mod is not Mafi.Base.BaseMod) continue;
            if (coolantBasis == null) throw new InvalidOperationException("No Data Center coolant contract.");
            var capital = new Dictionary<string, Exact> { [rack.ProductToAddThis.Product.Id.Value] = rack.ProductToAddThis.Quantity.Value };
            var maintenance = new Dictionary<string, Exact>();
            if (coolantBasis.Costs.Maintenance.Product != null)
                maintenance[coolantBasis.Costs.Maintenance.Product.Id.Value] = Partial(rack.Maintenance);
            racks.Add(new PlannerRack(rack.Id.Value, rack.Strings.Name.TranslatedString, Available(rack), rack.CreatedComputingPerTick.Value,
                new Exact(rack.ConsumedPowerPerTick.Value, Electricity.OneKw.Value), Partial(rack.CoolantInPerMonth),
                coolantBasis.CoolantIn.Id.Value, coolantBasis.CoolantOut.Id.Value, Partial(rack.CoolantOutPerMonth), capital, maintenance));
        }
        var transports = database.All<TransportProto>().Where(proto => proto.IsBuildable && proto.ThroughputPer60.Value > 0)
            .Select(proto => new PlannerTransport(proto.Id.Value, proto.Strings.Name.TranslatedString, proto.PortsShape.Id.Value,
                proto.ThroughputPer60.Value, Available(proto))).ToArray();
        return new PlannerCatalog(processes.OrderBy(process => process.Name, StringComparer.Ordinal).ThenBy(process => process.Key, StringComparer.Ordinal),
            products, references, racks.OrderBy(rack => rack.Computing), shells.OrderBy(shell => shell.Slots), transports);
    }

    private static PlannerProcess Copy(MachineProto machine, MachineRecipeBinding binding, bool available, string unlocks)
    {
        var inputs = new Dictionary<string, Exact>(StringComparer.Ordinal);
        var outputs = new Dictionary<string, Exact>(StringComparer.Ordinal);
        var ports = new Dictionary<string, (IoPortTemplate Template, bool Input, List<string> Products)>(StringComparer.Ordinal);
        var starts = new List<string>();
        void Port(IoPortTemplate template, bool input, string product)
        {
            string key = template.Name + ":" + template.RelativePosition + ":" + input;
            if (!ports.TryGetValue(key, out var entry))
                ports.Add(key, entry = (template, input, new List<string>()));
            entry.Products.Add(product);
        }
        for (int index = 0; index < binding.Recipe.AllInputs.Length; index++)
        {
            RecipeInput input = binding.Recipe.AllInputs[index];
            inputs[input.Product.Id.Value] = PlannerCalculator.Get(inputs, input.Product.Id.Value) + checked(input.Quantity.Value * binding.Multiplier);
            foreach (IoPortTemplate port in binding.InputPorts[index]) Port(port, true, input.Product.Id.Value);
        }
        for (int index = 0; index < binding.Recipe.AllOutputs.Length; index++)
        {
            RecipeOutput output = binding.Recipe.AllOutputs[index];
            outputs[output.Product.Id.Value] = PlannerCalculator.Get(outputs, output.Product.Id.Value) + checked(output.Quantity.Value * binding.Multiplier);
            if (output.TriggerAtStart) starts.Add(output.Product.Id.Value);
            foreach (IoPortTemplate port in binding.OutputPorts[index]) Port(port, false, output.Product.Id.Value);
        }
        Electricity activePower = machine.ConsumedPowerPerTick.ScaledBy(binding.Recipe.PowerMultiplier);
        return new PlannerProcess(machine.Id.Value, binding.Recipe.Id.Value, machine.Strings.Name.TranslatedString,
            machine.Mod is RecursiveIndustry ? Section(machine.Id.Value) : "Native comparators",
            new Exact(binding.Duration.Ticks, 1.Seconds().Ticks), new Exact(activePower.Value, Electricity.OneKw.Value),
            machine.ComputingConsumed.Value, machine.Costs.Workers, machine.Layout.TilesCount, available, unlocks,
            inputs, outputs, Capital(machine.Costs), Maintenance(machine.Costs), starts,
            ports.Values.Select(port => new PlannerPort(port.Template.Name.ToString(), port.Template.Shape.Id.Value, port.Input, port.Products)));
    }

    private static string Section(string id)
    {
        UniversalFacilitySpec facility = UniversalIndustryCatalog.Facilities.FirstOrDefault(row => row.Id.Value == id);
        if (facility != null)
        {
            if (facility.Key == "orbital_fabrication_fab") return "Frontier Projects";
            if (new[] { "refinery_complex", "gas_fertilizer_complex", "materials_chemistry_complex", "medical_chemistry_complex",
                "nuclear_fuel_complex", "nuclear_reprocessing_center", "nuclear_fuel_fabrication_cell" }.Contains(facility.Key)) return "Process Industries";
            if (new[] { "food_processing_campus", "food_pack_campus", "crop_soil_bioprocessing", "bioenergy_center", "water_utility",
                "process_water_chiller", "thermal_desalination_works", "thermal_emissions_utility", "materials_recovery_center" }.Contains(facility.Key)) return "Food and Recovery";
            return "Materials";
        }
        if (new[] { "Civic", "Knowledge", "Companion", "Operations", "PlanetaryCoordination" }.Any(id.Contains)) return "Civic Services";
        if (new[] { "Greenhouse", "Poultry", "Farm", "Reclaimer" }.Any(id.Contains)) return "Food and Recovery";
        if (new[] { "Deployment", "Controller" }.Any(id.Contains)) return "Control Infrastructure";
        if (new[] { "Orbital", "Frontier", "IntegrationArray", "ConstructionNexus", "SystemsIntegration" }.Any(id.Contains)) return "Frontier Projects";
        if (new[] { "Accelerator", "Curation", "ModelDevelopment", "Science" }.Any(id.Contains)) return "Computing and Models";
        return "Materials";
    }

    private static Dictionary<string, Exact> Capital(EntityCosts costs)
    {
        var result = new Dictionary<string, Exact>(StringComparer.Ordinal);
        foreach (ProductQuantity item in costs.BaseConstructionCost.Products)
            result[item.Product.Id.Value] = PlannerCalculator.Get(result, item.Product.Id.Value) + item.Quantity.Value;
        return result;
    }

    private static Dictionary<string, Exact> Maintenance(EntityCosts costs)
    {
        var result = new Dictionary<string, Exact>(StringComparer.Ordinal);
        if (costs.Maintenance.Product != null) result[costs.Maintenance.Product.Id.Value] = Partial(costs.Maintenance.MaintenancePerMonth);
        return result;
    }

    private static Exact Partial(PartialQuantity amount) => new(amount.Value.RawValue, Fix32.One.RawValue);
}