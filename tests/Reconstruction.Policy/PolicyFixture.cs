using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.UnlockingTree;

namespace Mafi
{
    public static class Log { public static void Info(string text) { } }
    public struct Vector2i { public int X; public int Y; public Vector2i(int x, int y) { X=x; Y=y; } }
}
namespace Mafi.Core.Prototypes
{
    public interface IProto { }
    public class Proto : IProto
    {
        public class ID { public readonly string Value; public ID(string value) { Value=value; } public override string ToString() => Value; }
        public ID Id; public object Mod;
        public Proto(ID id) { Id=id; }
    }
    public sealed class ProtosDb
    {
        public readonly List<Proto> Items = new();
        public IEnumerable<T> All<T>() => Items.OfType<T>();
        public T GetOrThrow<T>(Proto.ID id) where T : Proto => Items.OfType<T>().Single(item => item.Id.Value == id.Value);
    }
}
namespace Mafi.Core.Factory.Machines
{
    public sealed class MachineProto : Proto
    {
        public new sealed class ID : Proto.ID { public ID(string value) : base(value) { } }
        public MachineProto(ID id) : base(id) { }
    }
}
namespace Mafi.Core.Factory.Recipes
{
    public sealed class RecipeProto : Proto
    {
        public new sealed class ID : Proto.ID { public ID(string value) : base(value) { } }
        public new ID Id => (ID)base.Id;
        public RecipeProto(ID id) : base(id) { }
    }
}
namespace Mafi.Core.UnlockingTree
{
    public interface IUnlockNodeUnit { }
    public interface IProtoUnlock : IUnlockNodeUnit { IEnumerable<IProto> UnlockedProtos { get; } }
    public sealed class Unlock : IProtoUnlock
    {
        public IEnumerable<IProto> UnlockedProtos { get; }
        public Unlock(params IProto[] protos) { UnlockedProtos=protos; }
    }
}
namespace Mafi.Core.Research
{
    public sealed class ResearchNodeProto : Proto
    {
        public Mafi.Vector2i GridPosition;
        public readonly List<IUnlockNodeUnit> Units = new();
        public readonly List<ResearchNodeProto> Parents = new();
        public ResearchNodeProto(string name) : base(new Proto.ID(name)) { }
        public void AddParent(ResearchNodeProto parent) => Parents.Add(parent);
    }
}
namespace Mafi.Core.Mods
{
    public sealed class ProtoRegistrator
    {
        public readonly ProtosDb PrototypesDb = new();
        public readonly object ActiveMod = new();
    }
}
namespace RecursiveIndustry
{
    internal sealed class UniversalDirectBindingSpec { public string SourceMachineId; public string RecipeId; }
    internal sealed class UniversalSourceRecipeSpec { public string SourceMachineId; public string RecipeId; }
    internal sealed class UniversalFacilitySpec
    {
        public MachineProto.ID Id; public string Key; public UniversalDirectBindingSpec[] DirectBindings;
    }
    internal sealed class UniversalIntegratedRecipeSpec
    {
        public RecipeProto.ID Id; public string MachineKey; public UniversalSourceRecipeSpec[] Sources;
    }
    internal sealed class UniversalPrecisionRecipeSpec { public RecipeProto.ID Id; public string MachineKey; public string SourceRecipeId; }
    internal static class UniversalIndustryCatalog
    {
        public static UniversalFacilitySpec[] Facilities;
        public static UniversalIntegratedRecipeSpec[] IntegratedRecipes;
        public static UniversalPrecisionRecipeSpec[] PrecisionRecipes;
    }

    internal static class PolicyFixture
    {
        private sealed class Scenario
        {
            public readonly ProtoRegistrator Registrator = new();
            public readonly MachineProto.ID FacilityId = new("Facility");
            public readonly MachineProto Machine = new(new MachineProto.ID("NativeMachine"));
            public readonly RecipeProto SourceRecipe = new(new RecipeProto.ID("NativeRecipe"));
            public readonly RecipeProto Composed = new(new RecipeProto.ID("Composed"));
            public readonly ResearchNodeProto Target = new("Target");
            public readonly ResearchNodeProto Native = new("NativeOwner");

            public Scenario()
            {
                Target.Mod = Registrator.ActiveMod;
                Native.Mod = new object();
                Native.GridPosition = new Mafi.Vector2i(20,10);
                Native.Units.Add(new Unlock(Machine, SourceRecipe));
                Target.Units.Add(new Unlock(Composed));
                Registrator.PrototypesDb.Items.AddRange(new Proto[] { Machine, SourceRecipe, Composed, Target, Native });
                UniversalIndustryCatalog.Facilities = new[] {
                    new UniversalFacilitySpec { Id=FacilityId, Key="facility", DirectBindings=new[] {
                        new UniversalDirectBindingSpec { SourceMachineId="NativeMachine", RecipeId="NativeRecipe" } } }
                };
                UniversalIndustryCatalog.IntegratedRecipes = new[] {
                    new UniversalIntegratedRecipeSpec { Id=Composed.Id, MachineKey="facility", Sources=new[] {
                        new UniversalSourceRecipeSpec { RecipeId="NativeRecipe" } } }
                };
                UniversalIndustryCatalog.PrecisionRecipes = Array.Empty<UniversalPrecisionRecipeSpec>();
            }
            public void Run() => ReconstructionResearchPrerequisites.AddSources(Registrator, Target, FacilityId);
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        public static int Main()
        {
            int passed = 0;
            var simple = new Scenario();
            simple.Run();
            Require(simple.Target.Parents.SequenceEqual(new[] { simple.Native }), "Native equipment/recipe must deduplicate to one parent");
            passed++;

            var modOwner = new Scenario();
            var falseOwner = new ResearchNodeProto("ModOwner") { Mod=modOwner.Registrator.ActiveMod, GridPosition=new Mafi.Vector2i(0,0) };
            falseOwner.Units.Add(new Unlock(modOwner.Machine, modOwner.SourceRecipe));
            modOwner.Registrator.PrototypesDb.Items.Add(falseOwner);
            modOwner.Run();
            Require(modOwner.Target.Parents.SequenceEqual(new[] { modOwner.Native }), "A mod owner must not replace native prerequisites");
            passed++;

            var unrelated = new Scenario();
            var spaceMachine = new MachineProto(new MachineProto.ID("SpaceMachine"));
            var spaceRecipe = new RecipeProto(new RecipeProto.ID("SpaceRecipe"));
            var crewMode = new RecipeProto(new RecipeProto.ID("CrewMode"));
            var spaceOwner = new ResearchNodeProto("SpaceOwner") { Mod=new object() };
            spaceOwner.Units.Add(new Unlock(spaceMachine,spaceRecipe));
            unrelated.Registrator.PrototypesDb.Items.AddRange(new Proto[] { spaceMachine, spaceRecipe, crewMode, spaceOwner });
            UniversalIndustryCatalog.IntegratedRecipes = UniversalIndustryCatalog.IntegratedRecipes.Concat(new[] {
                new UniversalIntegratedRecipeSpec { Id=crewMode.Id, MachineKey="facility", Sources=new[] {
                    new UniversalSourceRecipeSpec { RecipeId="SpaceRecipe", SourceMachineId="SpaceMachine" } } }
            }).ToArray();
            unrelated.Run();
            Require(!unrelated.Target.Parents.Contains(spaceOwner), "A recipe not unlocked by the target must not introduce an orbital gate");
            unrelated.Target.Parents.Clear();
            unrelated.Target.Units.Add(new Unlock(crewMode));
            unrelated.Run();
            Require(unrelated.Target.Parents.Contains(spaceOwner), "An explicitly unlocked composition must require its external source machine");
            passed++;

            var missing = new Scenario();
            missing.Native.Units.Clear();
            bool failed = false;
            try { missing.Run(); } catch (InvalidOperationException) { failed=true; }
            Require(failed, "Unowned native equipment must fail closed");
            passed++;

            var unownedRecipe = new Scenario();
            unownedRecipe.Native.Units.Clear();
            unownedRecipe.Native.Units.Add(new Unlock(unownedRecipe.Machine));
            unownedRecipe.Run();
            Require(unownedRecipe.Target.Parents.Contains(unownedRecipe.Native), "An ownerless recipe retains the machine gate");
            passed++;

            var alternate = new Scenario();
            var early = new ResearchNodeProto("EarlierOwner") { Mod=new object(), GridPosition=new Mafi.Vector2i(10,10) };
            early.Units.Add(new Unlock(alternate.Machine,alternate.SourceRecipe));
            alternate.Registrator.PrototypesDb.Items.Add(early);
            alternate.Run();
            Require(alternate.Target.Parents.SequenceEqual(new[] { early }), "Alternate native owner selection must be deterministic");
            passed++;

            var precision = new Scenario();
            var precisionRecipe = new RecipeProto(new RecipeProto.ID("PrecisionMode"));
            precision.Target.Units.Clear();
            precision.Target.Units.Add(new Unlock(precisionRecipe));
            UniversalIndustryCatalog.PrecisionRecipes = new[] {
                new UniversalPrecisionRecipeSpec { Id=precisionRecipe.Id, MachineKey="facility", SourceRecipeId="NativeRecipe" }
            };
            precision.Run();
            Require(precision.Target.Parents.SequenceEqual(new[] { precision.Native }), "Precision must retain its native source technology");
            passed++;

            var specialized = new Scenario();
            var diesel = new Proto(new Proto.ID("DieselLocomotive"));
            var nuclear = new Proto(new Proto.ID("NuclearLocomotive"));
            var dieselOwner = new ResearchNodeProto("DieselResearch") { Mod=new object() };
            var nuclearOwner = new ResearchNodeProto("NuclearResearch") { Mod=new object() };
            dieselOwner.Units.Add(new Unlock(diesel));
            nuclearOwner.Units.Add(new Unlock(nuclear));
            specialized.Registrator.PrototypesDb.Items.AddRange(new Proto[] { diesel, nuclear, dieselOwner, nuclearOwner });
            ReconstructionResearchPrerequisites.AddNativeOwners(specialized.Registrator, specialized.Target, diesel);
            Require(specialized.Target.Parents.SequenceEqual(new[] { dieselOwner }), "A chosen native application must not inherit another application's technology");
            passed++;

            var unknown = new Proto(new Proto.ID("UnownedVehicle"));
            bool missingOwnerFailed = false;
            try { ReconstructionResearchPrerequisites.AddNativeOwners(specialized.Registrator, specialized.Target, unknown); }
            catch (InvalidOperationException) { missingOwnerFailed=true; }
            Require(missingOwnerFailed, "Unowned native vehicles or buildings must fail closed");
            passed++;

            var duplicate = new Scenario();
            ReconstructionResearchPrerequisites.AddNativeOwners(duplicate.Registrator, duplicate.Target, duplicate.Machine, duplicate.Machine);
            Require(duplicate.Target.Parents.SequenceEqual(new[] { duplicate.Native }), "Repeated source prototypes must not duplicate research parents");
            passed++;

            Console.WriteLine("PASS: " + passed + " compiled reconstruction policy scenarios (native data stand-ins, not game runtime).");
            return 0;
        }
    }
}