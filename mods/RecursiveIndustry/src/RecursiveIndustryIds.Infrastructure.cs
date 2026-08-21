using Mafi.Core.Entities.Static;
using Mafi.Core.Ports.Io;

namespace RecursiveIndustry;

public static partial class RecursiveIndustryIds
{
    public static partial class Infrastructure
    {
        public static readonly IoPortShapeProto.ID Data =
            new IoPortShapeProto.ID("IoPortShape_RecursiveIndustry_Data");

        public static readonly StaticEntityProto.ID AccessFiber =
            new StaticEntityProto.ID("Transport_RecursiveIndustry_AccessFiber");

        public static readonly StaticEntityProto.ID BackboneFiber =
            new StaticEntityProto.ID("Transport_RecursiveIndustry_BackboneFiber");

        public static readonly StaticEntityProto.ID FiberJunction =
            new StaticEntityProto.ID("LayoutEntity_RecursiveIndustry_FiberJunction");
    }
}