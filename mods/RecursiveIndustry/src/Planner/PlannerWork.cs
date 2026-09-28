using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecursiveIndustry.Planner;

internal sealed class PlannerComparison
{
    internal IReadOnlyList<PlannerProject> Projects { get; }
    internal PlannerResult Result { get; }
    internal PlannerComparison(IEnumerable<PlannerProject> projects, PlannerResult result)
    {
        Projects = Array.AsReadOnly(projects.ToArray());
        Result = result;
    }
}

internal sealed class PlannerWork : IDisposable
{
    private CancellationTokenSource _cancellation;
    private Task<PlannerComparison[]> _pending;
    private int _generation;
    internal Task<PlannerComparison[]> Pending => _pending;

    internal int Start(PlannerCatalog catalog, PlannerRequest request, bool combined, int milliseconds = 25)
    {
        Cancel();
        int generation = _generation;
        _cancellation = new CancellationTokenSource();
        CancellationToken token = _cancellation.Token;
        _pending = Task.Run(() =>
        {
            var clock = Stopwatch.StartNew();
            IEnumerable<IReadOnlyList<PlannerProject>> scenarios = combined
                ? new[] { request.Projects }
                : request.Projects.Select(project => (IReadOnlyList<PlannerProject>)new[] { project });
            if (request.Projects.Count > 8 || request.Projects.Count == 0) scenarios = new[] { request.Projects };
            var results = new List<PlannerComparison>();
            foreach (IReadOnlyList<PlannerProject> projects in scenarios)
            {
                var scenario = new PlannerRequest(projects, request.Sources.ToDictionary(pair => pair.Key, pair => pair.Value),
                    request.Spare.ToDictionary(pair => pair.Key, pair => pair.Value), request.Transport.ToDictionary(pair => pair.Key, pair => pair.Value),
                    request.InstalledComputing, request.CommittedComputing, request.Reserve, request.RetiredComputing,
                    request.TemporaryComputing, request.SpareRackSlots, request.Rack, request.Shell,
                    request.Stock.ToDictionary(pair => pair.Key, pair => pair.Value), request.CapitalRates.ToDictionary(pair => pair.Key, pair => pair.Value));
                PlannerResult quote = PlannerCalculator.Calculate(catalog.Processes, scenario, token,
                    Math.Max(0, milliseconds - (int)clock.Elapsed.TotalMilliseconds));
                results.Add(new PlannerComparison(projects, quote));
                if (token.IsCancellationRequested) break;
            }
            return results.ToArray();
        }, token);
        return generation;
    }

    internal bool TryRead(int generation, out IReadOnlyList<PlannerComparison> results)
    {
        results = null;
        if (_generation != generation || _pending == null || !_pending.IsCompleted) return false;
        if (_pending.Status != TaskStatus.RanToCompletion)
        {
            results = new[] { new PlannerComparison(Array.Empty<PlannerProject>(), new PlannerResult { Status = "Calculation failed or cancelled" }) };
            return true;
        }
        results = Array.AsReadOnly(_pending.Result);
        return true;
    }

    internal void Cancel()
    {
        _generation++;
        _cancellation?.Cancel();
        _cancellation?.Dispose();
        _cancellation = null;
        _pending = null;
    }

    public void Dispose() => Cancel();
}