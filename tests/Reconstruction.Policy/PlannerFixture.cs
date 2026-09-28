using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RecursiveIndustry.Planner;
using Amounts = System.Collections.Generic.Dictionary<string, RecursiveIndustry.Planner.Exact>;
using Sources = System.Collections.Generic.Dictionary<string, string>;

namespace RecursiveIndustry;

internal static class PlannerFixture
{
    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    internal static PlannerProcess Process(string host, string recipe, Dictionary<string, Exact> inputs,
        Dictionary<string, Exact> outputs, bool available = true) =>
        new(host, recipe, host, "Fixture", 60, 2500, 64, 4, 30, available, "Research",
            inputs, outputs, new Dictionary<string, Exact> { ["Parts"] = 750 },
            new Dictionary<string, Exact> { ["Maintenance"] = 28 });

    internal static int Run()
    {
        Require(Exact.TryParse("0.1", out Exact fraction) && fraction == new Exact(1, 10), "Decimal parsing must be exact.");
        Require(default(Exact) + new Exact(2, 3) == new Exact(2, 3), "Default exact value must be zero.");
        Require(new Exact(5, 2).Ceiling() == 3 && new Exact(-5, 2).Ceiling() == -2, "Whole hosts use mathematical ceiling.");
        Require(!Exact.TryParse("NaN", out _) && !Exact.TryParse("1/0", out _), "Invalid numbers must fail.");
        var process = Process("Water", "Direct", new() { ["WasteWater"] = 400 }, new() { ["Water"] = 480, ["Sludge"] = 40 });
        ProcessQuote quote = process.Quote("Water", 240, 1);
        Require(quote.Hosts == 1 && quote.Inputs["WasteWater"] == 200 && quote.Outputs["Sludge"] == 20,
            "Rates must follow requested target, not installed nameplate; co-products remain outputs.");
        Require(quote.PeakPower == 2500 && quote.IdealAveragePower == 1250 && quote.EnergyPerOutput == new Exact(625, 2),
            "Peak power, ideal average, and process energy have distinct units.");
        ProcessQuote restricted = process.Quote("Water", 480, new Exact(1, 2));
        Require(restricted.Hosts == 2 && restricted.Attainable == 480 && restricted.ActiveHosts == 1,
            "Transport bounds increase dedicated hosts, not physical material ratios.");
        var sameRecipe = Process("OtherHost", "Direct", new(), new() { ["Water"] = 480 });
        Require(process.Key != sameRecipe.Key, "Binding identity includes host, not recipe alone.");
        var other = Process("Other", "Consumer", new() { ["Package"] = 3 }, new() { ["Part"] = 1 });
        var terminal = Process("Terminal", "Consumer", new() { ["Package"] = 3 }, new() { ["Part"] = 1 });
        var validator = Process("Validator", "Validate", new() { ["Model"] = 1 }, new() { ["Package"] = 8 });
        var catalog = new[] { other, terminal, validator }.ToDictionary(item => item.Key);
        var selections = new[] { new PlannerProject(other.Key, "Part", 1), new PlannerProject(terminal.Key, "Part", 1) };
        var request = new PlannerRequest(selections, new Sources { ["Package"] = validator.Key }, new Amounts(), new Amounts(), installedComputing: 1000);
        PlannerResult shared = PlannerCalculator.Calculate(catalog, request, milliseconds: 10000);
        Require(shared.Complete && shared.Support.Single().Hosts == 1 && shared.Support.Single().Requested == 6,
            "Shared support demand must aggregate before rounding hosts.");
        request = new PlannerRequest(selections, new Sources { ["Package"] = validator.Key }, new Amounts { ["Package"] = 4 }, new Amounts(), installedComputing: 1000);
        PlannerResult spare = PlannerCalculator.Calculate(catalog, request, milliseconds: 10000);
        Require(spare.Support.Single().Requested == 2 && spare.SpareUsed["Package"] == 4,
            "A shared spare pool cannot be allocated twice.");
        var rack = new PlannerRack("Rack", "Rack III", true, 256, 1500, 0, "Cold", "Water", 0, new Amounts { ["Rack"] = 1 }, new Amounts());
        var shell = new PlannerShell("Shell", "Data Center", true, 8, 10, 20, new Amounts { ["Parts"] = 100 }, new Amounts());
        request = new PlannerRequest(selections, new Sources(), new Amounts(), new Amounts(), installedComputing: 256, committedComputing: 128,
            retiredComputing: 128, temporaryComputing: 256, rack: rack, shell: shell);
        PlannerResult cutover = PlannerCalculator.Calculate(catalog, request, milliseconds: 10000);
        Require(cutover.Complete && cutover.Racks == 0 && cutover.CutoverRacks == 1 && cutover.SteadyDemand == 128 && cutover.CutoverDemand == 512,
            "Existing commitments include the old line; retirement affects only steady state.");
        Require(cutover.CutoverShells == 1 && cutover.CutoverExtraCapital["Rack"] == 1 && cutover.CutoverExtraCapital["Parts"] == 100,
            "Temporary rack and shell capital must remain visible.");
        Require(cutover.CapitalBlockers.Contains("Rack"), "Capital funding includes temporary cutover hardware.");
        request = new PlannerRequest(selections, new Sources(), new Amounts(), new Amounts());
        Require(PlannerCalculator.Calculate(catalog, request, milliseconds: 10000).Status == "Locked or unselected rack",
            "Unselected or locked hardware must not become free Computing.");
        var cycle = Process("Loop", "Loop", new() { ["Package"] = 1 }, new() { ["Model"] = 1 });
        catalog.Add(cycle.Key, cycle);
        request = new PlannerRequest(selections, new Sources { ["Package"] = validator.Key, ["Model"] = cycle.Key }, new Amounts(), new Amounts(), installedComputing: 1000);
        PlannerResult cyclic = PlannerCalculator.Calculate(catalog, request, milliseconds: 10000);
        Require(!cyclic.Complete && cyclic.Status.Contains("cyclic") && cyclic.External["Package"] > 0,
            "A support cycle is a labeled boundary, not a free co-product credit.");
        var portProcess = new PlannerProcess("Ports", "Recipe", "Ports", "Fixture", 60, 1, 0, 0, 4, true, "",
            new Amounts { ["First"] = 300, ["Second"] = 300 }, new Amounts { ["Output"] = 60 }, new Amounts(), new Amounts(), ports: new[] {
                new PlannerPort("A", "Countable", true, new[] { "First", "Second" }),
                new PlannerPort("X", "Fluid", false, new[] { "Output" }) });
        Require(portProcess.MaximumBatches(new Dictionary<string, Exact> { ["Countable"] = 450, ["Fluid"] = 900 }, () => { }) == new Exact(3, 4),
            "Products sharing one physical port must share its typed throughput.");
        Require(portProcess.MaximumBatches(new Dictionary<string, Exact> { ["Fluid"] = 900 }, () => { }) == 0,
            "Missing unlocked countable transport cannot borrow fluid capacity.");
        Require(PlannerCalculator.Calculate(catalog, request, new CancellationToken(true), 10000).Status == "Cancelled", "Cancellation must discard results.");
        Require(PlannerCalculator.Calculate(catalog, request, milliseconds: 0).Status.StartsWith("Incomplete"), "A work limit must not look like a cheap complete result.");
        var detached = new PlannerCatalog(catalog.Values, new Dictionary<string, string>(), Array.Empty<PlannerReference>(),
            new[] { rack }, new[] { shell }, Array.Empty<PlannerTransport>());
        request = new PlannerRequest(selections, new Sources { ["Package"] = validator.Key }, new Amounts { ["Package"] = 4 },
            new Amounts(), installedComputing: 1000);
        using (var work = new PlannerWork())
        {
            int generation = work.Start(detached, request, true, 10000);
            work.Pending.GetAwaiter().GetResult();
            Require(work.TryRead(generation, out var combined) && combined.Single().Result.Support.Single().Requested == 2,
                "Combined projects share one support pool.");
            generation = work.Start(detached, request, false, 10000);
            work.Pending.GetAwaiter().GetResult();
            Require(work.TryRead(generation, out var alternatives) && alternatives.Count == 2 && alternatives.All(row => row.Result.Support.Count == 0),
                "Mutually exclusive alternatives compare against the same capacity, without pretending both can use it simultaneously.");
            work.Cancel();
            Require(!work.TryRead(generation, out _), "A closed or edited view cannot publish an older worker generation.");
        }
        var cooler = Process("Cooler", "Chill", new() { ["Water"] = 10 }, new() { ["Cold"] = 10 });
        catalog.Add(cooler.Key, cooler);
        var cooledRack = new PlannerRack("Rack", "Rack III", true, 256, 1500, 10, "Cold", "Water", 10,
            new Amounts { ["Rack"] = 1 }, new Amounts());
        request = new PlannerRequest(new[] { selections[0] }, new Sources { ["Cold"] = cooler.Key }, new Amounts(), new Amounts(),
            temporaryComputing: 512, rack: cooledRack, shell: shell);
        PlannerResult cooling = PlannerCalculator.Calculate(catalog, request, milliseconds: 10000);
        Require(cooling.Complete && cooling.Racks == 1 && cooling.CutoverRacks == 3, "Cutover must close its own cooling/Computing feedback.");
        Require(cooling.External["Water"] == 10 && cooling.CutoverExternal["Water"] == 30 && cooling.CutoverSupport.Single().Hosts == 3,
            "Temporary cooling demand and dedicated support are not steady-state quantities.");
        Require(cooling.CutoverExtraCapital["Parts"] == 1500 && cooling.CutoverExtraCapital["Rack"] == 2,
            "Extra cutover cooling capital must be counted alongside racks.");
        Console.WriteLine("PASS: 25 exact planner process, support, and worker checks.");
        return 25;
    }
}