using Mafi.Base;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Factory.Recipes;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;

namespace RecursiveIndustry;

public static partial class RecursiveIndustryIds
{
    public static partial class Products
    {
        public static readonly ProductProto.ID CivicKnowledgeStream =
            Ids.Products.CreateId("RecursiveIndustry_CivicKnowledgeStream");
    }

    public static partial class Machines
    {
        public static readonly MachineProto.ID CivicModelCenter =
            Ids.Machines.CreateId("RecursiveIndustry_CivicModelCenter");
    }

    public static partial class Recipes
    {
        public static readonly RecipeProto.ID ProvideCivicKnowledge =
            Ids.Recipes.CreateId("RecursiveIndustry_ProvideCivicKnowledge");
    }

    public static partial class Research
    {
        public static readonly ResearchNodeProto.ID CivicKnowledgeSystems =
            Ids.Research.CreateId("RecursiveIndustry_CivicKnowledgeSystems");
    }

    public static partial class Settlements
    {
        public static readonly StaticEntityProto.ID KnowledgeCommons = new("RecursiveIndustry_KnowledgeCommons");
        public static readonly Proto.ID CivicKnowledgeNeed = new("RecursiveIndustry_CivicKnowledgeNeed");
    }
}