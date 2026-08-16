using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Mods;

namespace RecursiveIndustry;

public sealed class RecursiveIndustry : DataOnlyMod
{
    public RecursiveIndustry(ModManifest manifest) : base(manifest)
    {
        Log.Info("RecursiveIndustry: constructed");
    }

    public override void RegisterPrototypes(ProtoRegistrator registrator)
    {
        Log.Info("RecursiveIndustry: registering Foundation, Applied Science, and Recursive Epochs");

        registrator.RegisterData<RecursiveIndustryProductData>();
        registrator.RegisterData<AppliedScienceProductData>();
        registrator.RegisterData<EpochProductData>();
        registrator.RegisterData<WorldExchangeData>();
        registrator.RegisterData<AcceleratorWorksData>();
        registrator.RegisterData<RackGenerationData>();
        registrator.RegisterData<CurationOfficeData>();
        registrator.RegisterData<ModelDevelopmentCenterData>();
        registrator.RegisterData<AIOperationsData>();
        registrator.RegisterData<AIElectronicsCellData>();
        registrator.RegisterData<ElectronicsReclaimerData>();
        registrator.RegisterData<AppliedScienceData>();
        registrator.RegisterData<SystemsIntegrationData>();
        registrator.RegisterData<AutonomousNetworksData>();
        registrator.RegisterData<AutonomousIndustrialVehiclesData>();
        registrator.RegisterData<AutonomousTrainsData>();
        registrator.RegisterData<AutonomousMicrochipData>();
        registrator.RegisterData<AutonomousElectronicsIntegrationData>();
        registrator.RegisterData<AutonomousCapitalFabricationData>();
        registrator.RegisterData<PlanetaryCoordinationData>();
        registrator.RegisterData<OrbitalIndustryData>();
        registrator.RegisterData<OrbitalPowerArrayData>();
        registrator.RegisterData<RecursiveFrontierData>();
        registrator.RegisterData<UniversalIndustryData>();
        registrator.RegisterDataWithInterface<IResearchNodesData>();
    }

    public override void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues)
    {
    }
}