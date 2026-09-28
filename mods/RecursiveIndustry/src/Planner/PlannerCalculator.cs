using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RecursiveIndustry.Planner;

internal sealed class PlannerLimitException : Exception
{
    internal PlannerLimitException(string message) : base(message) { }
}

internal sealed class PlannerProject
{
    internal string Process { get; }
    internal string Product { get; }
    internal Exact Rate { get; }
    internal string Key => Process + "|" + Product;
    internal PlannerProject(string process, string product, Exact rate)
    {
        Process = process;
        Product = product;
        Rate = rate;
    }
}

internal sealed class PlannerRack
{
    internal string Id { get; }
    internal string Name { get; }
    internal bool Available { get; }
    internal int Computing { get; }
    internal Exact Power { get; }
    internal Exact Coolant { get; }
    internal string CoolantInput { get; }
    internal string CoolantOutput { get; }
    internal Exact CoolantReturn { get; }
    internal IReadOnlyDictionary<string, Exact> Capital { get; }
    internal IReadOnlyDictionary<string, Exact> Maintenance { get; }

    internal PlannerRack(string id, string name, bool available, int computing, Exact power,
        Exact coolant, string coolantInput, string coolantOutput, Exact coolantReturn,
        IDictionary<string, Exact> capital, IDictionary<string, Exact> maintenance)
    {
        Id = id;
        Name = name;
        Available = available;
        Computing = computing;
        Power = power;
        Coolant = coolant;
        CoolantInput = coolantInput;
        CoolantOutput = coolantOutput;
        CoolantReturn = coolantReturn;
        Capital = PlannerProcess.Freeze(capital);
        Maintenance = PlannerProcess.Freeze(maintenance);
    }
}

internal sealed class PlannerShell
{
    internal string Id { get; }
    internal string Name { get; }
    internal bool Available { get; }
    internal int Slots { get; }
    internal int Workers { get; }
    internal int Area { get; }
    internal IReadOnlyDictionary<string, Exact> Capital { get; }
    internal IReadOnlyDictionary<string, Exact> Maintenance { get; }

    internal PlannerShell(string id, string name, bool available, int slots, int workers, int area,
        IDictionary<string, Exact> capital, IDictionary<string, Exact> maintenance)
    {
        Id = id;
        Name = name;
        Available = available;
        Slots = slots;
        Workers = workers;
        Area = area;
        Capital = PlannerProcess.Freeze(capital);
        Maintenance = PlannerProcess.Freeze(maintenance);
    }
}

internal sealed class PlannerRequest
{
    internal IReadOnlyList<PlannerProject> Projects { get; }
    internal IReadOnlyDictionary<string, string> Sources { get; }
    internal IReadOnlyDictionary<string, Exact> Spare { get; }
    internal IReadOnlyDictionary<string, Exact> Transport { get; }
    internal IReadOnlyDictionary<string, Exact> Stock { get; }
    internal IReadOnlyDictionary<string, Exact> CapitalRates { get; }
    internal int InstalledComputing { get; }
    internal int CommittedComputing { get; }
    internal int Reserve { get; }
    internal int RetiredComputing { get; }
    internal int TemporaryComputing { get; }
    internal int SpareRackSlots { get; }
    internal PlannerRack Rack { get; }
    internal PlannerShell Shell { get; }

    internal PlannerRequest(IEnumerable<PlannerProject> projects, IDictionary<string, string> sources,
        IDictionary<string, Exact> spare, IDictionary<string, Exact> transport,
        int installedComputing = 0, int committedComputing = 0, int reserve = 0,
        int retiredComputing = 0, int temporaryComputing = 0, int spareRackSlots = 0,
        PlannerRack rack = null, PlannerShell shell = null,
        IDictionary<string, Exact> stock = null, IDictionary<string, Exact> capitalRates = null)
    {
        Projects = Array.AsReadOnly(projects.ToArray());
        Sources = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(sources, StringComparer.Ordinal));
        Spare = PlannerProcess.Freeze(spare);
        Transport = PlannerProcess.Freeze(transport);
        Stock = PlannerProcess.Freeze(stock ?? new Dictionary<string, Exact>());
        CapitalRates = PlannerProcess.Freeze(capitalRates ?? new Dictionary<string, Exact>());
        InstalledComputing = installedComputing;
        CommittedComputing = committedComputing;
        Reserve = reserve;
        RetiredComputing = retiredComputing;
        TemporaryComputing = temporaryComputing;
        SpareRackSlots = spareRackSlots;
        Rack = rack;
        Shell = shell;
    }
}

internal sealed class PlannerResult
{
    internal string Status { get; set; } = "Known scope";
    internal bool Complete => Status == "Known scope";
    internal readonly List<ProcessQuote> Processes = new();
    internal readonly List<ProcessQuote> Support = new();
    internal readonly Dictionary<string, Exact> External = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, Exact> Residuals = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, Exact> SpareUsed = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, Exact> Capital = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, Exact> CutoverExtraCapital = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, Exact> CutoverExternal = new(StringComparer.Ordinal);
    internal readonly List<ProcessQuote> CutoverSupport = new();
    internal readonly Dictionary<string, Exact> Maintenance = new(StringComparer.Ordinal);
    internal readonly List<string> Boundaries = new();
    internal readonly List<string> CapitalBlockers = new();
    internal int PeakComputing { get; set; }
    internal int SteadyDemand { get; set; }
    internal int CutoverDemand { get; set; }
    internal int Racks { get; set; }
    internal int CutoverRacks { get; set; }
    internal int Shells { get; set; }
    internal int CutoverShells { get; set; }
    internal int Workers { get; set; }
    internal int Area { get; set; }
    internal Exact PeakPower { get; set; }
    internal Exact IdealAveragePower { get; set; }
    internal Exact RackPower { get; set; }
    internal Exact CutoverRackPower { get; set; }
    internal Exact CutoverKnownPower { get; set; }
    internal Exact CapitalWait { get; set; }
    internal double Milliseconds { get; set; }
    internal int ExpandedNodes { get; set; }
}

internal static class PlannerCalculator
{
    private sealed class Node
    {
        internal readonly PlannerProject Selection;
        internal readonly PlannerProcess Process;
        internal readonly Dictionary<string, Node> Children = new(StringComparer.Ordinal);
        internal readonly HashSet<string> Cycles = new(StringComparer.Ordinal);
        internal Exact Demand;
        internal int Parents;
        internal bool Terminal;
        internal Node(PlannerProject selection, PlannerProcess process) { Selection = selection; Process = process; }
    }

    internal static PlannerResult Calculate(IReadOnlyDictionary<string, PlannerProcess> catalog, PlannerRequest request,
        CancellationToken cancellation = default, int milliseconds = 25)
    {
        var clock = Stopwatch.StartNew();
        PlannerResult result = CalculatePhase(catalog, request, cancellation, milliseconds);
        if (!result.Complete) return result;
        PlannerResult cutover = result;
        if (request.RetiredComputing != 0 || request.TemporaryComputing != 0)
        {
            var phase = new PlannerRequest(request.Projects, request.Sources.ToDictionary(pair => pair.Key, pair => pair.Value),
                request.Spare.ToDictionary(pair => pair.Key, pair => pair.Value), request.Transport.ToDictionary(pair => pair.Key, pair => pair.Value),
                request.InstalledComputing, checked(request.CommittedComputing + request.TemporaryComputing), request.Reserve,
                spareRackSlots: request.SpareRackSlots, rack: request.Rack, shell: request.Shell,
                stock: request.Stock.ToDictionary(pair => pair.Key, pair => pair.Value), capitalRates: request.CapitalRates.ToDictionary(pair => pair.Key, pair => pair.Value));
            cutover = CalculatePhase(catalog, phase, cancellation, Math.Max(0, milliseconds - (int)clock.Elapsed.TotalMilliseconds));
            if (!cutover.Complete)
            {
                result.Status = "Incomplete cutover: " + cutover.Status;
                result.Boundaries.AddRange(cutover.Boundaries);
                return result;
            }
            result.ExpandedNodes += cutover.ExpandedNodes;
        }
        result.CutoverDemand = cutover.SteadyDemand;
        result.CutoverRacks = cutover.Racks;
        result.CutoverShells = cutover.Shells;
        result.CutoverRackPower = cutover.RackPower;
        result.CutoverKnownPower = cutover.PeakPower + cutover.RackPower;
        result.CutoverSupport.AddRange(cutover.Support);
        foreach (var item in cutover.External) result.CutoverExternal[item.Key] = item.Value;
        result.CutoverExtraCapital.Clear();
        foreach (var item in cutover.Capital)
            Add(result.CutoverExtraCapital, item.Key, Exact.Max(0, item.Value - Get(result.Capital, item.Key)));
        Funding(result, request);
        result.Milliseconds = clock.Elapsed.TotalMilliseconds;
        return result;
    }

    private static PlannerResult CalculatePhase(IReadOnlyDictionary<string, PlannerProcess> catalog, PlannerRequest request,
        CancellationToken cancellation, int milliseconds)
    {
        var clock = Stopwatch.StartNew();
        var result = new PlannerResult();
        void Check()
        {
            cancellation.ThrowIfCancellationRequested();
            if (clock.Elapsed.TotalMilliseconds > milliseconds) throw new PlannerLimitException("Incomplete: calculation time limit.");
        }
        try
        {
            if (request.Projects.Count == 0) { result.Status = "Empty selection"; return result; }
            if (request.Projects.Count > 8) throw new PlannerLimitException("Too large: at most eight projects.");
            if (new[] { request.InstalledComputing, request.CommittedComputing, request.Reserve, request.RetiredComputing,
                    request.TemporaryComputing, request.SpareRackSlots }.Any(value => value < 0 || value > 1000000000)
                || request.RetiredComputing > request.CommittedComputing
                || request.Spare.Values.Concat(request.Stock.Values).Concat(request.CapitalRates.Values)
                    .Concat(request.Transport.Values).Any(value => value < 0 || value > 1000000000))
                throw new ArgumentException("Invalid numeric input or retirement exceeds existing commitment.");
            int coolingRacks = 0;
            bool settled = false;
            for (int iteration = 0; iteration < 16; iteration++)
            {
                Check();
                result = Expand(catalog, request, coolingRacks, Check);
                if (!result.Complete) break;
                foreach (ProcessQuote quote in result.Processes.Concat(result.Support))
                {
                    Check();
                    result.PeakComputing = checked(result.PeakComputing + quote.Process.Computing * quote.Hosts);
                    result.Workers = checked(result.Workers + quote.Process.Workers * quote.Hosts);
                    result.Area = checked(result.Area + quote.Process.Area * quote.Hosts);
                    result.PeakPower += quote.PeakPower;
                    result.IdealAveragePower += quote.IdealAveragePower;
                    Add(result.Capital, quote.Process.Capital, quote.Hosts);
                    Add(result.Maintenance, quote.Process.Maintenance, quote.Hosts);
                }
                result.SteadyDemand = checked(request.CommittedComputing - request.RetiredComputing + result.PeakComputing + request.Reserve);
                result.CutoverDemand = checked(request.CommittedComputing + result.PeakComputing + request.Reserve + request.TemporaryComputing);
                int shortfall = Math.Max(0, result.SteadyDemand - request.InstalledComputing);
                int cutoverShortfall = Math.Max(0, result.CutoverDemand - request.InstalledComputing);
                if (cutoverShortfall > 0 && (request.Rack == null || !request.Rack.Available || request.Rack.Computing <= 0))
                {
                    result.Status = "Locked or unselected rack";
                    result.Boundaries.Add("Computing shortfall: " + cutoverShortfall);
                    break;
                }
                result.Racks = shortfall == 0 ? 0 : (new Exact(shortfall) / request.Rack.Computing).Ceiling();
                result.CutoverRacks = cutoverShortfall == 0 ? 0 : (new Exact(cutoverShortfall) / request.Rack.Computing).Ceiling();
                if (result.Racks != coolingRacks)
                {
                    coolingRacks = result.Racks;
                    continue;
                }
                settled = true;
                if (result.CutoverRacks > 0) AddHardware(result, request);
                break;
            }
            if (!settled && result.Complete) result.Status = "Too large: cooling/Computing closure did not converge.";
        }
        catch (OperationCanceledException) { result = new PlannerResult { Status = "Cancelled" }; }
        catch (PlannerLimitException error) { result.Status = error.Message; result.Boundaries.Add("Unexpanded work; known costs are not a complete total."); }
        catch (ArgumentException error) { result.Status = "Invalid input: " + error.Message; }
        catch (OverflowException) { result.Status = "Too large: numeric capacity exceeded."; }
        finally { result.Milliseconds = clock.Elapsed.TotalMilliseconds; }
        return result;
    }

    private static PlannerResult Expand(IReadOnlyDictionary<string, PlannerProcess> catalog, PlannerRequest request,
        int coolingRacks, Action check)
    {
        var result = new PlannerResult();
        var nodes = new Dictionary<string, Node>(StringComparer.Ordinal);
        var active = new HashSet<string>(StringComparer.Ordinal);
        var spare = request.Spare.ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

        Node Visit(PlannerProject selection, int depth)
        {
            check();
            if (depth > 12 || nodes.Count >= 128 && !nodes.ContainsKey(selection.Key))
                throw new PlannerLimitException("Too large: graph exceeds 128 nodes or depth 12.");
            if (nodes.TryGetValue(selection.Key, out Node previous)) return previous;
            if (!catalog.TryGetValue(selection.Process, out PlannerProcess process) || !process.Outputs.ContainsKey(selection.Product))
                throw new ArgumentException("Unknown binding or target: " + selection.Key);
            var node = new Node(selection, process);
            nodes.Add(selection.Key, node);
            active.Add(selection.Key);
            foreach (string input in process.Inputs.Keys.OrderBy(product => product, StringComparer.Ordinal))
            {
                if (!request.Sources.TryGetValue(input, out string source)) continue;
                var next = new PlannerProject(source, input, 0);
                if (active.Contains(next.Key)) { node.Cycles.Add(input); continue; }
                Node child = Visit(next, depth + 1);
                node.Children.Add(input, child);
                child.Parents++;
            }
            active.Remove(selection.Key);
            return node;
        }

        foreach (PlannerProject selection in request.Projects)
        {
            if (selection.Rate <= 0 || selection.Rate > 1000000000) throw new ArgumentException("Output rate must be positive and bounded.");
            Node node = Visit(selection, 0);
            node.Terminal = true;
            node.Demand += selection.Rate;
        }
        Exact Allocate(string product, Exact amount)
        {
            Exact used = Exact.Min(Get(spare, product), amount);
            spare[product] = Get(spare, product) - used;
            if (used > 0) Add(result.SpareUsed, product, used);
            return amount - used;
        }
        if (coolingRacks > 0 && request.Rack.Coolant > 0)
        {
            string product = request.Rack.CoolantInput;
            Exact demand = Allocate(product, coolingRacks * request.Rack.Coolant);
            if (request.Sources.TryGetValue(product, out string source)) Visit(new PlannerProject(source, product, 0), 0).Demand += demand;
            else Add(result.External, product, demand);
            Add(result.Residuals, request.Rack.CoolantOutput, coolingRacks * request.Rack.CoolantReturn);
        }
        var ready = new SortedSet<string>(nodes.Values.Where(node => node.Parents == 0).Select(node => node.Selection.Key), StringComparer.Ordinal);
        while (ready.Count > 0)
        {
            check();
            string key = ready.Min;
            ready.Remove(key);
            Node node = nodes[key];
            if (node.Demand > 0)
            {
                if (!node.Process.Available)
                {
                    result.Status = "Locked research";
                    result.Boundaries.Add(node.Process.Name + ": " + node.Process.Unlocks);
                }
                else
                {
                    Exact maximum = node.Process.MaximumBatches(request.Transport, check);
                    if (maximum <= 0)
                    {
                        result.Status = "Unavailable transport";
                        result.Boundaries.Add(node.Process.Name);
                    }
                    else
                    {
                        ProcessQuote quote = node.Process.Quote(node.Selection.Product, node.Demand, maximum);
                        (node.Terminal ? result.Processes : result.Support).Add(quote);
                        foreach (var item in quote.Inputs)
                        {
                            Exact demand = Allocate(item.Key, item.Value);
                            if (node.Children.TryGetValue(item.Key, out Node child)) child.Demand += demand;
                            else
                            {
                                Add(result.External, item.Key, demand);
                                if (demand > 0 && node.Cycles.Contains(item.Key))
                                {
                                    result.Status = "Incomplete: cyclic support";
                                    result.Boundaries.Add(node.Process.Name + " -> " + item.Key);
                                }
                            }
                        }
                        foreach (var item in quote.Outputs.Where(item => item.Key != node.Selection.Product)) Add(result.Residuals, item.Key, item.Value);
                    }
                }
            }
            foreach (Node child in node.Children.Values)
                if (--child.Parents == 0) ready.Add(child.Selection.Key);
        }
        result.ExpandedNodes = nodes.Count;
        return result;
    }

    private static void AddHardware(PlannerResult result, PlannerRequest request)
    {
        PlannerRack rack = request.Rack;
        result.RackPower = result.Racks * rack.Power;
        result.CutoverRackPower = result.CutoverRacks * rack.Power;
        Add(result.Capital, rack.Capital, result.Racks);
        Add(result.CutoverExtraCapital, rack.Capital, result.CutoverRacks - result.Racks);
        Add(result.Maintenance, rack.Maintenance, result.Racks);
        int slotsNeeded = Math.Max(0, result.Racks - request.SpareRackSlots);
        int cutoverSlots = Math.Max(0, result.CutoverRacks - request.SpareRackSlots);
        if (cutoverSlots == 0) return;
        if (request.Shell == null || !request.Shell.Available || request.Shell.Slots <= 0)
        {
            result.Status = "Locked or unselected Data Center";
            result.Boundaries.Add("Rack slots required: " + cutoverSlots);
            return;
        }
        PlannerShell shell = request.Shell;
        result.Shells = (new Exact(slotsNeeded) / shell.Slots).Ceiling();
        result.CutoverShells = (new Exact(cutoverSlots) / shell.Slots).Ceiling();
        result.Workers = checked(result.Workers + shell.Workers * result.Shells);
        result.Area = checked(result.Area + shell.Area * result.Shells);
        Add(result.Capital, shell.Capital, result.Shells);
        Add(result.CutoverExtraCapital, shell.Capital, result.CutoverShells - result.Shells);
        Add(result.Maintenance, shell.Maintenance, result.Shells);
    }

    internal static Exact Get(IReadOnlyDictionary<string, Exact> values, string key) => values.TryGetValue(key, out Exact value) ? value : 0;
    private static void Funding(PlannerResult result, PlannerRequest request)
    {
        foreach (string product in result.Capital.Keys.Union(result.CutoverExtraCapital.Keys))
        {
            Exact missing = Exact.Max(0, Get(result.Capital, product) + Get(result.CutoverExtraCapital, product) - Get(request.Stock, product));
            if (missing == 0) continue;
            Exact rate = Get(request.CapitalRates, product);
            if (rate == 0) result.CapitalBlockers.Add(product);
            else result.CapitalWait = Exact.Max(result.CapitalWait, missing * 60 / rate);
        }
    }
    private static void Add(Dictionary<string, Exact> values, string product, Exact amount)
    {
        if (amount > 0) values[product] = Get(values, product) + amount;
    }
    private static void Add(Dictionary<string, Exact> values, IReadOnlyDictionary<string, Exact> source, Exact multiplier)
    {
        foreach (var item in source) Add(values, item.Key, item.Value * multiplier);
    }
}