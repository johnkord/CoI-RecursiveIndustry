using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RecursiveIndustry.Planner;

internal sealed class PlannerTransport
{
    internal string Id { get; }
    internal string Name { get; }
    internal string Kind { get; }
    internal Exact Capacity { get; }
    internal bool Available { get; }
    internal PlannerTransport(string id, string name, string kind, Exact capacity, bool available)
    {
        Id = id;
        Name = name;
        Kind = kind;
        Capacity = capacity;
        Available = available;
    }
}

internal sealed class PlannerReference
{
    internal string Id { get; }
    internal string Name { get; }
    internal string Family { get; }
    internal string Section { get; }
    internal string Description { get; }
    internal string Unlocks { get; }
    internal bool Available { get; }
    internal int Workers { get; }
    internal int Area { get; }
    internal IReadOnlyDictionary<string, Exact> Capital { get; }
    internal IReadOnlyDictionary<string, Exact> Maintenance { get; }

    internal PlannerReference(string id, string name, string family, string description, string unlocks,
        bool available, int workers, int area, IDictionary<string, Exact> capital, IDictionary<string, Exact> maintenance, string section = "Civic Services")
    {
        Id = id;
        Name = name;
        Family = family;
        Section = section;
        Description = description;
        Unlocks = unlocks;
        Available = available;
        Workers = workers;
        Area = area;
        Capital = PlannerProcess.Freeze(capital);
        Maintenance = PlannerProcess.Freeze(maintenance);
    }
}

internal sealed class PlannerCatalog
{
    internal IReadOnlyDictionary<string, PlannerProcess> Processes { get; }
    internal IReadOnlyDictionary<string, string> Products { get; }
    internal IReadOnlyList<PlannerReference> References { get; }
    internal IReadOnlyList<PlannerRack> Racks { get; }
    internal IReadOnlyList<PlannerShell> Shells { get; }
    internal IReadOnlyList<PlannerTransport> Transports { get; }

    internal PlannerCatalog(IEnumerable<PlannerProcess> processes, IDictionary<string, string> products,
        IEnumerable<PlannerReference> references, IEnumerable<PlannerRack> racks,
        IEnumerable<PlannerShell> shells, IEnumerable<PlannerTransport> transports)
    {
        Processes = new ReadOnlyDictionary<string, PlannerProcess>(processes.ToDictionary(row => row.Key, StringComparer.Ordinal));
        Products = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(products, StringComparer.Ordinal));
        References = Array.AsReadOnly(references.ToArray());
        Racks = Array.AsReadOnly(racks.ToArray());
        Shells = Array.AsReadOnly(shells.ToArray());
        Transports = Array.AsReadOnly(transports.ToArray());
    }

    internal string ProductName(string id) => Products.TryGetValue(id, out string name) ? name : id;
}