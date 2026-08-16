using Mafi.Core.Entities.Dynamic;

namespace RecursiveIndustry;

public static partial class RecursiveIndustryIds
{
    public static partial class Vehicles
    {
        public static readonly DynamicEntityProto.ID AutonomousHauler =
            new("RecursiveIndustry_AutonomousHauler");

        public static readonly DynamicEntityProto.ID AutonomousDumpHauler =
            new("RecursiveIndustry_AutonomousDumpHauler");

        public static readonly DynamicEntityProto.ID AutonomousTankHauler =
            new("RecursiveIndustry_AutonomousTankHauler");

        public static readonly DynamicEntityProto.ID AutonomousAmphibiousHauler =
            new("RecursiveIndustry_AutonomousAmphibiousHauler");

        public static readonly DynamicEntityProto.ID AutonomousAmphibiousExcavator =
            new("RecursiveIndustry_AutonomousAmphibiousExcavator");

        public static readonly DynamicEntityProto.ID AutonomousMegaExcavator =
            new("RecursiveIndustry_AutonomousMegaExcavator");

        public static readonly DynamicEntityProto.ID AutonomousLargeTreeHarvester =
            new("RecursiveIndustry_AutonomousLargeTreeHarvester");

        public static readonly DynamicEntityProto.ID AutonomousTreePlanter =
            new("RecursiveIndustry_AutonomousTreePlanter");
    }
}