using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Mods;
using Mafi.Core.Products;

namespace RecursiveIndustry;

internal sealed class RecursiveIndustryProductData : IModData
{
    private const string AcceleratorModuleIcon =
        "Assets/RecursiveIndustry/ProductIcons/accelerator_module.png";
    private const string AcceleratorRackIIcon =
        "Assets/RecursiveIndustry/ProductIcons/accelerator_rack_i.png";
    private const string FrontierRackIIIcon =
        "Assets/RecursiveIndustry/ProductIcons/frontier_rack_ii.png";
    private const string RecursiveRackIIIIcon =
        "Assets/RecursiveIndustry/ProductIcons/recursive_rack_iii.png";
    private const string SpentAcceleratorIcon =
        "Assets/RecursiveIndustry/ProductIcons/spent_accelerator.png";
    private const string ModelArchiveIcon =
        "Assets/RecursiveIndustry/ProductIcons/model_archive.png";
    private const string ValidatedControlPackageIcon =
        "Assets/RecursiveIndustry/ProductIcons/validated_control_package.png";
    private const string DatasetArchiveIcon =
        "Assets/RecursiveIndustry/ProductIcons/dataset_archive.png";
    private const string CartridgeMesh =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/cartridge-LOD0.obj";
    private const string CartridgeNormals =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/cartridge-normals.png";
    private const string CartridgeSmoothMetal =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/cartridge-smoothmetal.png";
    private const string AcceleratorModuleAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/accelerator_module-albedo.png";
    private const string AcceleratorRackIAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/accelerator_rack_i-albedo.png";
    private const string FrontierRackIIAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/frontier_rack_ii-albedo.png";
    private const string RecursiveRackIIIAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/recursive_rack_iii-albedo.png";
    private const string SpentAcceleratorAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/spent_accelerator-albedo.png";
    private const string ModelArchiveAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/model_archive-albedo.png";
    private const string ValidatedControlPackageAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/validated_control_package-albedo.png";
    private const string DatasetArchiveAlbedo =
        "Assets/RecursiveIndustry/ProductModels/Cartridge/dataset_archive-albedo.png";

    public void RegisterData(ProtoRegistrator registrator)
    {
        CountableProductProto microchips =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.Microchips);
        CountableProductProto server =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.Server);
        CountableProductProto electronics4 =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.Electronics4);
        CountableProductProto electronics3 =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.Electronics3);
        CountableProductProto electronics2 =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.Electronics2);
        CountableProductProto labEquipment4 =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.LabEquipment4);
        CountableProductProto constructionParts4 =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.ConstructionParts4);
        CountableProductProto officeSupplies =
            registrator.PrototypesDb.GetOrThrow<CountableProductProto>(Ids.Products.OfficeSupplies);

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.AcceleratorModule,
            "Accelerator Module",
            WithCustomTransport(microchips, AcceleratorModuleIcon, AcceleratorModuleAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.AcceleratorRackI,
            "Accelerator Rack I",
            WithCustomTransport(server, AcceleratorRackIIcon, AcceleratorRackIAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.FrontierRackII,
            "Frontier Rack II",
            WithCustomTransport(electronics4, FrontierRackIIIcon, FrontierRackIIAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.RecursiveRackIII,
            "Recursive Rack III",
            WithCustomTransport(constructionParts4, RecursiveRackIIIIcon, RecursiveRackIIIAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.SpentAccelerator,
            "Spent Accelerator",
            WithCustomTransport(electronics2, SpentAcceleratorIcon, SpentAcceleratorAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.ModelArchive,
            "Model Archive",
            WithCustomTransport(labEquipment4, ModelArchiveIcon, ModelArchiveAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.ValidatedControlPackage,
            "Validated Control Package",
            WithCustomTransport(
                electronics3,
                ValidatedControlPackageIcon,
                ValidatedControlPackageAlbedo)));

        registrator.PrototypesDb.Add(new CountableProductProto(
            RecursiveIndustryIds.Products.DatasetArchive,
            "Dataset Archive",
            WithCustomTransport(officeSupplies, DatasetArchiveIcon, DatasetArchiveAlbedo)));
    }

    private static CountableProductProto.Gfx WithCustomTransport(
        CountableProductProto source,
        string iconPath,
        string albedoPath)
    {
        if (!source.Graphics.MeshFamily.HasValue)
        {
            throw new System.InvalidOperationException(
                $"Source product '{source.Id}' has no mesh family for custom transport graphics.");
        }

        ImmutableArray<ProductLodMesh> customLods = ImmutableArray.Create(
            new ProductLodMesh(0, CartridgeMesh),
            new ProductLodMesh(1, CartridgeMesh),
            new ProductLodMesh(2, CartridgeMesh),
            new ProductLodMesh(3, CartridgeMesh),
            new ProductLodMesh(4, CartridgeMesh));

        return new CountableProductProto.Gfx(
            iconPath,
            source.Graphics.MeshFamily.Value,
            new ProductTextures(albedoPath, CartridgeNormals, CartridgeSmoothMetal),
            customLods,
            source.Graphics.PackingModeOverride,
            source.Graphics.Size,
            source.Graphics.StorageRackYawDegrees);
    }
}