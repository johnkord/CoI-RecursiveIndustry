using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.Entities;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.GameLoop;
using Mafi.Core.Prototypes;
using Mafi.Core.Simulation;
using Mafi.Unity.Ui;
using RecursiveIndustry.Planner;
using RecursiveIndustry.Planner.Native;

namespace Mafi
{
    public readonly struct Electricity
    {
        public int Value { get; }
        public static Electricity OneKw => new(10);
        public Electricity(int value) { Value = value; }
    }
    public readonly struct TestOption<T> where T : class
    {
        public bool HasValue => Value != null;
        public T Value { get; }
        public TestOption(T value) { Value = value; }
    }
    public readonly struct TestComputing
    {
        public int Value { get; }
        public TestComputing(int value) { Value = value; }
    }
    public sealed class TestEvent
    {
        private readonly List<Action> _handlers = new();
        public int Count => _handlers.Count;
        public void AddNonSaveable(object owner, Action handler) => _handlers.Add(handler);
        public void RemoveNonSaveable(object owner, Action handler) => _handlers.Remove(handler);
        public void Invoke() { foreach (Action handler in _handlers.ToArray()) handler(); }
    }
}
namespace Mafi.Core
{
    public enum RegistrationMode { AsSelf }
    public sealed class GlobalDependencyAttribute : Attribute
    {
        public GlobalDependencyAttribute(RegistrationMode mode) { }
    }
}
namespace Mafi.Core.Entities
{
    public readonly struct EntityId
    {
        public int Value { get; }
        public bool IsValid => Value > 0;
        public EntityId(int value) { Value = value; }
        public override string ToString() => Value.ToString();
    }
    public interface IEntity
    {
        EntityId Id { get; }
        bool IsDestroyed { get; }
        bool IsPaused { get; }
        Proto Prototype { get; }
    }
    public sealed class EntitiesManager
    {
        public readonly Dictionary<int, IEntity> Entities = new();
        public bool TryGetEntity(EntityId id, out IEntity entity) => Entities.TryGetValue(id.Value, out entity);
    }
}
namespace Mafi.Core.Entities.Static
{
    public interface IStaticEntity : IEntity { bool IsConstructed { get; } }
}
namespace Mafi.Core.Factory.Machines
{
    public sealed class TestComputingConsumer
    {
        public Mafi.TestComputing ComputingRequired = new(64);
        public Mafi.TestComputing ComputingCharged = new(32);
    }
    public sealed class TestElectricityConsumer
    {
        public Mafi.Electricity PowerRequired = new(25000);
        public Mafi.Electricity PowerCharged = new(10000);
    }
    public sealed class Machine : Mafi.Core.Entities.Static.IStaticEntity
    {
        public EntityId Id { get; set; }
        public bool IsDestroyed { get; set; }
        public bool IsPaused { get; set; }
        public bool IsConstructed { get; set; } = true;
        public MachineProto Prototype { get; set; }
        Proto IEntity.Prototype => Prototype;
        public string CurrentState = "Working";
        public readonly List<RecipeProto> RecipesAssigned = new();
        public Mafi.TestOption<TestComputingConsumer> ComputingConsumer = new(new TestComputingConsumer());
        public Mafi.TestOption<TestElectricityConsumer> ElectricityConsumer = new(new TestElectricityConsumer());
    }
}
namespace Mafi.Core.Prototypes
{
    public sealed class UnlockedProtosDb { public Mafi.TestEvent OnUnlockedSetChanged = new(); }
}
namespace Mafi.Core.Simulation
{
    public interface ISimLoopEvents
    {
        Mafi.TestEvent Sync { get; }
        Mafi.TestEvent UpdateEndForUi { get; }
        Mafi.TestEvent IdleUpdate { get; }
        Mafi.TestEvent BeforeSave { get; }
        int CurrentStep { get; }
    }
    public sealed class TestSimulation : ISimLoopEvents
    {
        public Mafi.TestEvent Sync { get; } = new();
        public Mafi.TestEvent UpdateEndForUi { get; } = new();
        public Mafi.TestEvent IdleUpdate { get; } = new();
        public Mafi.TestEvent BeforeSave { get; } = new();
        public int CurrentStep => 123;
    }
}
namespace Mafi.Core.GameLoop
{
    public interface IGameLoopEvents { Mafi.TestEvent OnProjectChanged { get; } }
    public sealed class TestGameLoop : IGameLoopEvents { public Mafi.TestEvent OnProjectChanged { get; } = new(); }
}
namespace Mafi.Unity.Ui
{
    public sealed class InspectorsManager
    {
        public IEntity Selected;
        public IEntity GetFirstActiveEntityOrNull() => Selected;
    }
}
namespace RecursiveIndustry.Planner.Native
{
    internal static class PlannerNativeCatalog
    {
        internal static int Builds;
        internal static PlannerCatalog Build(ProtosDb database)
        {
            Builds++;
            return new PlannerCatalog(new[] { PlannerFixture.Process("Host", "Recipe", new(), new() { ["Part"] = 1 }) },
                new Dictionary<string, string>(), Array.Empty<PlannerReference>(), Array.Empty<PlannerRack>(),
                Array.Empty<PlannerShell>(), Array.Empty<PlannerTransport>());
        }
    }
}
namespace RecursiveIndustry
{
    internal static class PlannerSnapshotFixture
    {
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        internal static int Run()
        {
            var simulation = new TestSimulation();
            var game = new TestGameLoop();
            var unlocked = new UnlockedProtosDb();
            var inspectors = new InspectorsManager();
            var entities = new EntitiesManager();
            var entity = new Machine { Id = new EntityId(7), Prototype = new MachineProto(new MachineProto.ID("Host")), IsPaused = true };
            entity.RecipesAssigned.Add(new RecipeProto(new RecipeProto.ID("Recipe")));
            entities.Entities.Add(7, entity);
            inspectors.Selected = entity;
            PlannerNativeCatalog.Builds = 0;
            var snapshots = new PlannerSnapshots(new ProtosDb(), unlocked, entities, inspectors, simulation, game);
            int generation = snapshots.RequestCatalog(true);
            Require(!snapshots.TryRead(generation, out _, out _, out _), "No render-thread entity traversal before synchronization.");
            simulation.Sync.Invoke();
            simulation.IdleUpdate.Invoke();
            Require(!snapshots.TryRead(generation, out _, out _, out _), "Simulation snapshots must wait for publication synchronization.");
            simulation.Sync.Invoke();
            Require(snapshots.TryRead(generation, out _, out PlannerSnapshot result, out string status) && status == "Inspected"
                && result.Paused && result.Assignments.SequenceEqual(new[] { "Recipe" }), "Paused IdleUpdate captures real assignments.");
            entity.RecipesAssigned.Clear();
            Require(result.Assignments.Count == 1 && result.PowerRequired == 2500 && result.ComputingCharged == 32,
                "Snapshots own copied assignments and diagnostic utility values.");

            generation = snapshots.RequestCatalog(true);
            simulation.Sync.Invoke();
            simulation.UpdateEndForUi.Invoke();
            entities.Entities.Remove(7);
            simulation.Sync.Invoke();
            Require(snapshots.TryRead(generation, out _, out result, out status) && result == null && status == "Selected entity is unavailable",
                "Destruction between capture and publication must not expose the stale entity.");
            Require(PlannerNativeCatalog.Builds == 1, "Ordinary selected snapshots reuse the detached catalog.");

            generation = snapshots.RequestCatalog(false);
            simulation.Sync.Invoke();
            simulation.IdleUpdate.Invoke();
            snapshots.Cancel();
            simulation.Sync.Invoke();
            Require(!snapshots.TryRead(generation, out _, out _, out _), "Closing invalidates pending generations.");

            foreach (Mafi.TestEvent boundary in new[] { simulation.BeforeSave, game.OnProjectChanged, unlocked.OnUnlockedSetChanged })
            {
                int epoch = snapshots.Epoch;
                generation = snapshots.RequestCatalog(false);
                simulation.Sync.Invoke();
                simulation.IdleUpdate.Invoke();
                boundary.Invoke();
                simulation.Sync.Invoke();
                Require(snapshots.Epoch == epoch + 1 && !snapshots.TryRead(generation, out _, out _, out _), "Save/world/research changes invalidate the cache and result.");
            }
            inspectors.Selected = null;
            generation = snapshots.RequestCatalog(true);
            simulation.Sync.Invoke();
            simulation.IdleUpdate.Invoke();
            simulation.Sync.Invoke();
            Require(snapshots.TryRead(generation, out _, out result, out status) && result == null && status == "No building selected", "Empty inspection is distinct from the global catalog.");

            snapshots.Dispose();
            Require(new[] { simulation.Sync, simulation.UpdateEndForUi, simulation.IdleUpdate, simulation.BeforeSave, game.OnProjectChanged, unlocked.OnUnlockedSetChanged }
                .All(signal => signal.Count == 0), "Disposal removes every non-saveable callback.");
            Require(snapshots.RequestCatalog(true) == 0, "Disposed coordinators reject new work.");
            Console.WriteLine("PASS: 13 production snapshot coordinator checks with controlled native event stand-ins.");
            return 13;
        }
    }
}