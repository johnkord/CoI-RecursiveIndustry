using System;

namespace RecursiveIndustry;

internal static class BuildingModelPaths
{
    private const string Root = "Assets/RecursiveIndustry/Buildings/";
    public const string AcceleratorWorks = Root + "accelerator_works.prefab";
    public const string CurationOffice = Root + "curation_office.prefab";
    public const string ModelDevelopmentCenter = Root + "model_development_center.prefab";
    public const string AIElectronicsCell = Root + "ai_electronics_cell.prefab";
    public const string ElectronicsReclaimer = Root + "electronics_reclaimer.prefab";
    public const string AIScienceInstitute = Root + "ai_science_institute.prefab";
    public const string PilotScienceComplex = Root + "pilot_science_complex.prefab";
    public const string SystemsIntegrationComplex = Root + "systems_integration_complex.prefab";
    public const string ControlDeploymentGateway = Root + "control_deployment_gateway.prefab";
    public const string DeploymentAssuranceCampus = Root + "deployment_assurance_campus.prefab";
    public const string AutonomousMicrochipComplex = Root + "autonomous_microchip_complex.prefab";
    public const string AutonomousCapitalFabricationMatrix = Root + "capital_fabrication_matrix.prefab";
    public const string OrbitalMissionComplex = Root + "orbital_mission_complex.prefab";
    public const string FrontierProjectComplex = Root + "frontier_project_complex.prefab";
    public const string RecursiveIntegrationArray = Root + "recursive_integration_array.prefab";
    public const string AutonomousConstructionNexus = Root + "autonomous_construction_nexus.prefab";
    public const string OperationsI = Root + "ai_operations_i.prefab";
    public const string OperationsII = Root + "ai_operations_ii.prefab";
    public const string OperationsIII = Root + "ai_operations_iii.prefab";
    public const string OrbitalPowerArray = Root + "orbital_power_array.prefab";
    public const string CompanionAnimalCenter = Root + "companion_animal_center.prefab";
    public const string SensorGuidedGreenhouse = Root + "sensor_guided_greenhouse.prefab";
    public const string MonitoredPoultryFarm = Root + "monitored_poultry_farm.prefab";
    public const string ComminutionHub = Root + "comminution_hub.prefab";
    public const string MineralProductsWorks = Root + "mineral_products_works.prefab";
    public const string PrimarySmelter = Root + "primary_smelter.prefab";
    public const string FuelSmelter = Root + "fuel_smelter.prefab";
    public const string PrecisionMetalsWorks = Root + "precision_metals_works.prefab";
    public const string CastingFinishingWorks = Root + "casting_finishing_works.prefab";
    public const string RefineryComplex = Root + "refinery_complex.prefab";
    public const string GasFertilizerComplex = Root + "gas_fertilizer_complex.prefab";
    public const string MaterialsChemistryComplex = Root + "materials_chemistry_complex.prefab";
    public const string MedicalChemistryComplex = Root + "medical_chemistry_complex.prefab";
    public const string FoodProcessingCampus = Root + "food_processing_campus.prefab";
    public const string FoodPackCampus = Root + "food_pack_campus.prefab";
    public const string CropSoilBioprocessing = Root + "crop_soil_bioprocessing.prefab";
    public const string BioenergyCenter = Root + "bioenergy_center.prefab";
    public const string WaterUtility = Root + "water_utility.prefab";
    public const string ThermalDesalinationWorks = Root + "thermal_desalination_works.prefab";
    public const string ThermalEmissionsUtility = Root + "thermal_emissions_utility.prefab";
    public const string MaterialsRecoveryCenter = Root + "materials_recovery_center.prefab";
    public const string NuclearFuelComplex = Root + "nuclear_fuel_complex.prefab";
    public const string NuclearReprocessingCenter = Root + "nuclear_reprocessing_center.prefab";
    public const string NuclearFuelFabricationCell = Root + "nuclear_fuel_fabrication_cell.prefab";
    public const string PrecisionComponentsFab = Root + "precision_components_fab.prefab";
    public const string RoboticComponentsFab = Root + "robotic_components_fab.prefab";
    public const string GeneralManufacturingFab = Root + "general_manufacturing_fab.prefab";
    public const string OrbitalFabricationFab = Root + "orbital_fabrication_fab.prefab";

    public static string Universal(string key) => key switch
    {
            "comminution_hub" => ComminutionHub,
            "mineral_products_works" => MineralProductsWorks,
            "primary_smelter" => PrimarySmelter,
            "fuel_smelter" => FuelSmelter,
            "precision_metals_works" => PrecisionMetalsWorks,
            "casting_finishing_works" => CastingFinishingWorks,
            "refinery_complex" => RefineryComplex,
            "gas_fertilizer_complex" => GasFertilizerComplex,
            "materials_chemistry_complex" => MaterialsChemistryComplex,
            "medical_chemistry_complex" => MedicalChemistryComplex,
            "food_processing_campus" => FoodProcessingCampus,
            "food_pack_campus" => FoodPackCampus,
            "crop_soil_bioprocessing" => CropSoilBioprocessing,
            "bioenergy_center" => BioenergyCenter,
            "water_utility" => WaterUtility,
            "thermal_desalination_works" => ThermalDesalinationWorks,
            "thermal_emissions_utility" => ThermalEmissionsUtility,
            "materials_recovery_center" => MaterialsRecoveryCenter,
            "nuclear_fuel_complex" => NuclearFuelComplex,
            "nuclear_reprocessing_center" => NuclearReprocessingCenter,
            "nuclear_fuel_fabrication_cell" => NuclearFuelFabricationCell,
            "precision_components_fab" => PrecisionComponentsFab,
            "robotic_components_fab" => RoboticComponentsFab,
            "general_manufacturing_fab" => GeneralManufacturingFab,
            "orbital_fabrication_fab" => OrbitalFabricationFab,
        _ => throw new InvalidOperationException("Missing building model: " + key),
    };
}
