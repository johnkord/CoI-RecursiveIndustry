using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RecursiveIndustry.Planner;

internal sealed class PlannerProcess
{
    internal string Key => HostId + "|" + RecipeId;
    internal string HostId { get; }
    internal string RecipeId { get; }
    internal string Name { get; }
    internal string Section { get; }
    internal Exact Duration { get; }
    internal Exact PowerKw { get; }
    internal int Computing { get; }
    internal int Workers { get; }
    internal int Area { get; }
    internal bool Available { get; }
    internal string Unlocks { get; }
    internal IReadOnlyDictionary<string, Exact> Inputs { get; }
    internal IReadOnlyDictionary<string, Exact> Outputs { get; }
    internal IReadOnlyDictionary<string, Exact> Capital { get; }
    internal IReadOnlyDictionary<string, Exact> Maintenance { get; }
    internal IReadOnlyList<string> StartOutputs { get; }
    internal IReadOnlyList<PlannerPort> Ports { get; }

    internal PlannerProcess(string hostId, string recipeId, string name, string section,
        Exact duration, Exact powerKw, int computing, int workers, int area, bool available,
        string unlocks, IDictionary<string, Exact> inputs, IDictionary<string, Exact> outputs,
        IDictionary<string, Exact> capital, IDictionary<string, Exact> maintenance,
        IEnumerable<string> startOutputs = null, IEnumerable<PlannerPort> ports = null)
    {
        if (duration <= 0 || powerKw < 0 || computing < 0 || workers < 0 || area < 0
            || outputs.Count == 0 || inputs.Values.Any(value => value <= 0) || outputs.Values.Any(value => value <= 0))
            throw new ArgumentException("Invalid process declaration.");
        HostId = hostId;
        RecipeId = recipeId;
        Name = name;
        Section = section;
        Duration = duration;
        PowerKw = powerKw;
        Computing = computing;
        Workers = workers;
        Area = area;
        Available = available;
        Unlocks = unlocks;
        Inputs = Freeze(inputs);
        Outputs = Freeze(outputs);
        Capital = Freeze(capital);
        Maintenance = Freeze(maintenance);
        StartOutputs = Array.AsReadOnly((startOutputs ?? Array.Empty<string>()).ToArray());
        Ports = Array.AsReadOnly((ports ?? Array.Empty<PlannerPort>()).ToArray());
    }

    internal static IReadOnlyDictionary<string, Exact> Freeze(IDictionary<string, Exact> values) =>
        new ReadOnlyDictionary<string, Exact>(new Dictionary<string, Exact>(values, StringComparer.Ordinal));

    internal Exact MaximumBatches(IReadOnlyDictionary<string, Exact> capacities, Action checkBudget)
    {
        Exact maximum = 60 / Duration;
        foreach (bool input in new[] { true, false })
        {
            IReadOnlyDictionary<string, Exact> amounts = input ? Inputs : Outputs;
            PlannerPort[] ports = Ports.Where(port => port.Input == input).ToArray();
            string[] products = amounts.Keys.Where(product => ports.Any(port => port.Products.Contains(product))).ToArray();
            var remaining = new HashSet<string>(products, StringComparer.Ordinal);
            while (remaining.Count > 0)
            {
                var component = new HashSet<string>(StringComparer.Ordinal) { remaining.First() };
                bool changed;
                do
                {
                    changed = false;
                    foreach (PlannerPort port in ports.Where(port => port.Products.Any(component.Contains)))
                        foreach (string product in port.Products.Where(amounts.ContainsKey))
                            changed |= component.Add(product);
                } while (changed);
                remaining.ExceptWith(component);
                if (component.Count > 12) throw new PlannerLimitException("Too large: shared-port component exceeds 12 products.");
                string[] members = component.OrderBy(product => product, StringComparer.Ordinal).ToArray();
                for (int subset = 1; subset < (1 << members.Length); subset++)
                {
                    checkBudget();
                    Exact demand = 0;
                    var connected = new HashSet<PlannerPort>();
                    for (int index = 0; index < members.Length; index++)
                    {
                        if ((subset & (1 << index)) == 0) continue;
                        demand += amounts[members[index]];
                        foreach (PlannerPort port in ports.Where(port => port.Products.Contains(members[index]))) connected.Add(port);
                    }
                    Exact capacity = 0;
                    foreach (PlannerPort port in connected)
                        if (capacities.TryGetValue(port.Kind, out Exact selected)) capacity += selected;
                    maximum = Exact.Min(maximum, capacity / demand);
                }
            }
        }
        return maximum;
    }

    internal ProcessQuote Quote(string target, Exact requested, Exact maximumBatchesPer60)
    {
        if (!Outputs.TryGetValue(target, out Exact quantity) || requested <= 0 || requested > 1000000000)
            throw new ArgumentException("Invalid target output.");
        Exact batchesPer60 = Exact.Min(60 / Duration, maximumBatchesPer60);
        if (batchesPer60 <= 0) throw new ArgumentException("No compatible transport capacity.");
        Exact outputPerHost = quantity * batchesPer60;
        int hosts = (requested / outputPerHost).Ceiling();
        Exact requiredBatches = requested / quantity;
        return new ProcessQuote(this, target, requested, hosts, outputPerHost * hosts,
            requiredBatches * Duration / 60,
            Inputs.ToDictionary(row => row.Key, row => row.Value * requiredBatches),
            Outputs.ToDictionary(row => row.Key, row => row.Value * requiredBatches));
    }
}

internal sealed class ProcessQuote
{
    internal PlannerProcess Process { get; }
    internal string Target { get; }
    internal Exact Requested { get; }
    internal int Hosts { get; }
    internal Exact Attainable { get; }
    internal Exact ActiveHosts { get; }
    internal Exact PeakPower => Process.PowerKw * Hosts;
    internal Exact IdealAveragePower => Process.PowerKw * ActiveHosts;
    internal Exact EnergyPerOutput => IdealAveragePower * 60 / Requested;
    internal IReadOnlyDictionary<string, Exact> Inputs { get; }
    internal IReadOnlyDictionary<string, Exact> Outputs { get; }

    internal ProcessQuote(PlannerProcess process, string target, Exact requested, int hosts, Exact attainable,
        Exact activeHosts, IDictionary<string, Exact> inputs, IDictionary<string, Exact> outputs)
    {
        Process = process;
        Target = target;
        Requested = requested;
        Hosts = hosts;
        Attainable = attainable;
        ActiveHosts = activeHosts;
        Inputs = PlannerProcess.Freeze(inputs);
        Outputs = PlannerProcess.Freeze(outputs);
    }
}

internal sealed class PlannerPort
{
    internal string Name { get; }
    internal string Kind { get; }
    internal bool Input { get; }
    internal IReadOnlyList<string> Products { get; }

    internal PlannerPort(string name, string kind, bool input, IEnumerable<string> products)
    {
        Name = name;
        Kind = kind;
        Input = input;
        Products = Array.AsReadOnly(products.Distinct(StringComparer.Ordinal).ToArray());
    }
}