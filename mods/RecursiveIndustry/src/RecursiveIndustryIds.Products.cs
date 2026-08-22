using Mafi.Base;
using ProductID = Mafi.Core.Products.ProductProto.ID;

namespace RecursiveIndustry;

public static partial class RecursiveIndustryIds
{
    public static partial class Products
    {
        public static readonly ProductID AcceleratorModule =
            Ids.Products.CreateId("RecursiveIndustry_AcceleratorModule");

        public static readonly ProductID AcceleratorRackI =
            Ids.Products.CreateId("RecursiveIndustry_AcceleratorRackI");

        public static readonly ProductID FrontierRackII =
            Ids.Products.CreateId("RecursiveIndustry_FrontierRackII");

        public static readonly ProductID RecursiveRackIII =
            Ids.Products.CreateId("RecursiveIndustry_RecursiveRackIII");

        public static readonly ProductID SpentAccelerator =
            Ids.Products.CreateId("RecursiveIndustry_SpentAccelerator");

        public static readonly ProductID ModelArchive =
            Ids.Products.CreateId("RecursiveIndustry_ModelArchive");

        public static readonly ProductID ValidatedControlPackage =
            Ids.Products.CreateId("RecursiveIndustry_ValidatedControlPackage");

        // Preserve the original virtual-product value for save and protocol stability.
        public static readonly ProductID DatasetArchive =
            Ids.Products.CreateId("RecursiveIndustry_CuratedDataset");

        public static readonly ProductID ExperimentProgram =
            Ids.Products.CreateId("RecursiveIndustry_ExperimentProgram");

        public static readonly ProductID ValidatedResearchDossier =
            Ids.Products.CreateId("RecursiveIndustry_ValidatedResearchDossier");

        public static readonly ProductID FrontierProgram =
            Ids.Products.CreateId("RecursiveIndustry_FrontierProgram");

        public static readonly ProductID FrontierExpansionProject =
            Ids.Products.CreateId("RecursiveIndustry_FrontierExpansionProject");

        public static readonly ProductID OrbitalPowerCalibration =
            Ids.Products.CreateId("RecursiveIndustry_OrbitalPowerCalibration");

        public static readonly ProductID IndustrialControlStream =
            Ids.Products.CreateId("RecursiveIndustry_IndustrialControlStream");

        public static readonly ProductID CompanionProvisions =
            Ids.Products.CreateId("RecursiveIndustry_CompanionProvisions");
    }
}