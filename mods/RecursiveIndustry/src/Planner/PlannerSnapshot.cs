using System;
using System.Collections.Generic;
using System.Linq;

namespace RecursiveIndustry.Planner;

internal sealed class PlannerSnapshot
{
    internal string Entity { get; }
    internal string Host { get; }
    internal string Name { get; }
    internal string State { get; }
    internal bool Constructed { get; }
    internal bool Paused { get; }
    internal IReadOnlyList<string> Assignments { get; }
    internal Exact PowerRequired { get; }
    internal Exact PowerCharged { get; }
    internal int ComputingRequired { get; }
    internal int ComputingCharged { get; }
    internal string Step { get; }
    internal double Milliseconds { get; }

    internal PlannerSnapshot(string entity, string host, string name, string state, bool constructed, bool paused,
        IEnumerable<string> assignments, Exact powerRequired, Exact powerCharged, int computingRequired,
        int computingCharged, string step, double milliseconds)
    {
        Entity = entity;
        Host = host;
        Name = name;
        State = state;
        Constructed = constructed;
        Paused = paused;
        Assignments = Array.AsReadOnly(assignments.ToArray());
        PowerRequired = powerRequired;
        PowerCharged = powerCharged;
        ComputingRequired = computingRequired;
        ComputingCharged = computingCharged;
        Step = step;
        Milliseconds = milliseconds;
    }
}