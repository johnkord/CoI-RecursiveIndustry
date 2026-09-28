using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.GameLoop;
using Mafi.Core.Prototypes;
using Mafi.Core.Simulation;
using Mafi.Unity.Ui;

namespace RecursiveIndustry.Planner.Native;

[GlobalDependency(RegistrationMode.AsSelf)]
public sealed class PlannerSnapshots : IDisposable
{
    private sealed class Request
    {
        internal readonly int Generation;
        internal readonly EntityId Entity;
        internal readonly bool Inspect;
        internal Request(int generation, EntityId entity, bool inspect) { Generation = generation; Entity = entity; Inspect = inspect; }
    }

    private sealed class Response
    {
        internal readonly int Generation;
        internal readonly PlannerCatalog Catalog;
        internal readonly PlannerSnapshot Selection;
        internal readonly string Status;
        internal Response(int generation, PlannerCatalog catalog, PlannerSnapshot selection, string status)
        { Generation = generation; Catalog = catalog; Selection = selection; Status = status; }
    }

    private readonly object _gate = new();
    private readonly ProtosDb _prototypes;
    private readonly UnlockedProtosDb _unlocked;
    private readonly EntitiesManager _entities;
    private readonly InspectorsManager _inspectors;
    private readonly ISimLoopEvents _simulation;
    private readonly IGameLoopEvents _game;
    private Request _requested;
    private Request _simulationRequest;
    private Response _simulationResponse;
    private Response _visible;
    private PlannerCatalog _catalog;
    private int _generation;
    private int _processed;
    private int _epoch;
    private bool _disposed;
    internal int Epoch => Volatile.Read(ref _epoch);

    public PlannerSnapshots(ProtosDb prototypes, UnlockedProtosDb unlocked, EntitiesManager entities,
        InspectorsManager inspectors, ISimLoopEvents simulation, IGameLoopEvents game)
    {
        _prototypes = prototypes;
        _unlocked = unlocked;
        _entities = entities;
        _inspectors = inspectors;
        _simulation = simulation;
        _game = game;
        _simulation.Sync.AddNonSaveable(this, Sync);
        _simulation.UpdateEndForUi.AddNonSaveable(this, Capture);
        _simulation.IdleUpdate.AddNonSaveable(this, Capture);
        _simulation.BeforeSave.AddNonSaveable(this, Invalidate);
        _game.OnProjectChanged.AddNonSaveable(this, Invalidate);
        _unlocked.OnUnlockedSetChanged.AddNonSaveable(this, Invalidate);
    }

    internal int RequestCatalog(bool inspect)
    {
        EntityId entity = inspect ? _inspectors.GetFirstActiveEntityOrNull()?.Id ?? default : default;
        lock (_gate)
        {
            if (_disposed) return 0;
            _requested = new Request(++_generation, entity, inspect);
            _visible = null;
            return _generation;
        }
    }

    internal bool TryRead(int generation, out PlannerCatalog catalog, out PlannerSnapshot selection, out string status)
    {
        lock (_gate)
        {
            Response response = _visible;
            catalog = response?.Catalog;
            selection = response?.Selection;
            status = response?.Status ?? "Snapshot pending";
            return response != null && response.Generation == generation && generation == _generation;
        }
    }

    internal void Cancel()
    {
        lock (_gate)
        {
            _generation++;
            _requested = null;
            _simulationRequest = null;
            _simulationResponse = null;
            _visible = null;
        }
    }

    private void Invalidate()
    {
        lock (_gate)
        {
            Cancel();
            _catalog = null;
            Interlocked.Increment(ref _epoch);
        }
    }

    private void Sync()
    {
        lock (_gate)
        {
            _simulationRequest = _requested;
            if (_simulationResponse != null && _requested != null && _simulationResponse.Generation == _requested.Generation)
            {
                bool missing = _requested.Entity.IsValid && (!_entities.TryGetEntity(_requested.Entity, out IEntity entity) || entity.IsDestroyed);
                _visible = missing ? new Response(_requested.Generation, _simulationResponse.Catalog, null, "Selected entity is unavailable") : _simulationResponse;
            }
        }
    }

    private void Capture()
    {
        Request request;
        int epoch;
        lock (_gate)
        {
            request = _simulationRequest;
            epoch = _epoch;
            if (_disposed || request == null || request.Generation == _processed) return;
            _processed = request.Generation;
        }
        Response response;
        try
        {
            PlannerCatalog catalog;
            lock (_gate) catalog = _catalog;
            if (catalog == null)
            {
                var build = Stopwatch.StartNew();
                catalog = PlannerNativeCatalog.Build(_prototypes);
                Log.Info("RecursiveIndustry: PLANNER_CATALOG bindings=" + catalog.Processes.Count + " milliseconds=" + build.Elapsed.TotalMilliseconds);
                lock (_gate) if (_epoch == epoch) _catalog = catalog;
            }
            var clock = Stopwatch.StartNew();
            PlannerSnapshot snapshot = null;
            string status = request.Inspect && !request.Entity.IsValid ? "No building selected" : "Catalog ready";
            if (request.Entity.IsValid)
            {
                if (!_entities.TryGetEntity(request.Entity, out IEntity entity) || entity.IsDestroyed) status = "Selected entity is unavailable";
                else if (entity is Machine machine)
                {
                    var assignments = new List<string>();
                    foreach (RecipeProto recipe in machine.RecipesAssigned) assignments.Add(recipe.Id.Value);
                    int required = machine.ComputingConsumer.HasValue ? machine.ComputingConsumer.Value.ComputingRequired.Value : 0;
                    int charged = machine.ComputingConsumer.HasValue ? machine.ComputingConsumer.Value.ComputingCharged.Value : 0;
                    Exact power = machine.ElectricityConsumer.HasValue ? new Exact(machine.ElectricityConsumer.Value.PowerRequired.Value, Electricity.OneKw.Value) : 0;
                    Exact powerCharged = machine.ElectricityConsumer.HasValue ? new Exact(machine.ElectricityConsumer.Value.PowerCharged.Value, Electricity.OneKw.Value) : 0;
                    snapshot = new PlannerSnapshot(entity.Id.ToString(), machine.Prototype.Id.Value, machine.Prototype.Strings.Name.TranslatedString,
                        machine.CurrentState.ToString(), machine.IsConstructed, machine.IsPaused, assignments, power, powerCharged, required, charged,
                        _simulation.CurrentStep.ToString(), clock.Elapsed.TotalMilliseconds);
                    status = catalog.Processes.Values.Any(row => row.HostId == snapshot.Host) ? "Inspected" : "Unsupported content: external supplier";
                }
                else
                {
                    snapshot = new PlannerSnapshot(entity.Id.ToString(), entity.Prototype.Id.Value, entity.Prototype.Strings.Name.TranslatedString,
                        "Reference only", entity is IStaticEntity building && building.IsConstructed, entity.IsPaused,
                        Array.Empty<string>(), 0, 0, 0, 0, _simulation.CurrentStep.ToString(), clock.Elapsed.TotalMilliseconds);
                    status = catalog.References.Any(row => row.Id == snapshot.Host) ? "Reference selected" : "Unsupported content: external supplier";
                }
            }
            response = new Response(request.Generation, catalog, snapshot, status);
            Log.Info("RecursiveIndustry: PLANNER_SNAPSHOT generation=" + request.Generation + " entity=" + request.Entity
                + " status=" + status + " milliseconds=" + clock.Elapsed.TotalMilliseconds + " commands=0");
        }
        catch (Exception error)
        {
            response = new Response(request.Generation, null, null, "Snapshot failed");
            Log.Error("RecursiveIndustry: PLANNER_SNAPSHOT_FAILED " + error);
        }
        lock (_gate)
            if (!_disposed && epoch == _epoch && request.Generation == _generation) _simulationResponse = response;
    }

    public void Dispose()
    {
        lock (_gate) { _disposed = true; Invalidate(); }
        _simulation.Sync.RemoveNonSaveable(this, Sync);
        _simulation.UpdateEndForUi.RemoveNonSaveable(this, Capture);
        _simulation.IdleUpdate.RemoveNonSaveable(this, Capture);
        _simulation.BeforeSave.RemoveNonSaveable(this, Invalidate);
        _game.OnProjectChanged.RemoveNonSaveable(this, Invalidate);
        _unlocked.OnUnlockedSetChanged.RemoveNonSaveable(this, Invalidate);
    }
}