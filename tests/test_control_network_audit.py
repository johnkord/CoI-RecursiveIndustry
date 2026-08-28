#!/usr/bin/env python3
"""Regression tests for the Industrial Control source auditor."""

from __future__ import annotations

import copy
import json
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
FIXTURES = ROOT / "tests" / "fixtures" / "control_network"
sys.path.insert(0, str(ROOT / "tools"))

from audit_recursive_industry_control_network import (  # noqa: E402
    audit_catalog,
    audit_assurance_source,
    audit_data_product_source,
    audit_forbidden_runtime,
    audit_gateway_source,
    audit_transport_source,
)
from generate_recursive_industry_universal_source import load_catalog  # noqa: E402


CONTROL = json.loads(
    (ROOT / "data" / "industrial-control-network.json").read_text(
        encoding="utf-8"
    )
)
CATALOG = load_catalog()

VALID_DATA_PRODUCT = """
public sealed class DataProductProto : ProductProto
{
    public static readonly ProductType ProductType =
        new ProductType(typeof(DataProductProto));
    public DataProductProto() : base(
        isStorable: false,
        canBeDiscarded: false,
        isWaste: false,
        isRecyclable: false,
        quantityFormatter: NoUnitsQuantityFormatter.Instance) {}
}
"""

VALID_TRANSPORT = """
private const int AccessCapacityPer60 = 200;
private const int BackboneCapacityPer60 = 450;
var shape = new IoPortShapeProto(
    RecursiveIndustryIds.Infrastructure.Data,
    Proto.Str.Empty,
    ':',
    DataProductProto.ProductType);
var access = Ids.Transports.PipeT2;
var backbone = Ids.Transports.PipeT3;
var accessIcon = RecursiveIndustryIcons.AccessFiber;
var backboneIcon = RecursiveIndustryIcons.BackboneFiber;
var junctionIcon = RecursiveIndustryIcons.FiberJunction;
var clone = new TransportProto(allowMixedProducts: false);
accessFiber.SetNextTier(backboneFiber);
var junction = new MiniZipperProto(
    RecursiveIndustryIds.Infrastructure.FiberJunction);
var throughput = accessFiber.GetMaxThroughputPer60For(data);
"""

VALID_GATEWAY = """
var gateway = builder
    .Start("Control Deployment Gateway",
        RecursiveIndustryIds.Machines.ControlDeploymentGateway)
    .SetCost(Costs.Build
        .CP4(640)
        .Product(128, Ids.Products.Electronics4)
        .Product(32, RecursiveIndustryIds.Products.ValidatedControlPackage)
        .Product(4, RecursiveIndustryIds.Products.FrontierProgram)
        .Product(4, RecursiveIndustryIds.Products.ValidatedResearchDossier)
        .Workers(4)
        .MaintenanceT3(8))
    .SetElectricityConsumption(1000.Kw())
    .SetComputingConsumption(Computing.FromTFlops(256))
    .SetLayout("A#>[4][4][4][4][4][4]>:X")
    .SetCustomIconPath(RecursiveIndustryIcons.ControlDeploymentGateway);
recipe
    .AddInput(1, RecursiveIndustryIds.Products.ValidatedControlPackage)
    .AddOutput(210, RecursiveIndustryIds.Products.IndustrialControlStream)
    .WithCommonInputPorts((RecursiveIndustryIds.Products.ValidatedControlPackage, "A"))
    .WithCommonOutputPorts((RecursiveIndustryIds.Products.IndustrialControlStream, "X"))
    .BindTo(gateway, 60.Seconds());
recipe
    .Start(RecursiveIndustryIds.Recipes.DeployBackboneIndustrialControl)
    .AddInput(2, RecursiveIndustryIds.Products.ValidatedControlPackage)
    .AddOutput(420, RecursiveIndustryIds.Products.IndustrialControlStream)
    .SetPowerMultiplier(250.Percent())
    .WithCommonInputPorts((RecursiveIndustryIds.Products.ValidatedControlPackage, "A"))
    .WithCommonOutputPorts((RecursiveIndustryIds.Products.IndustrialControlStream, "X"))
    .BindTo(gateway, 60.Seconds());
"""

VALID_ASSURANCE = """
var campus = builder
    .Start("Deployment Assurance Campus",
        RecursiveIndustryIds.Machines.DeploymentAssuranceCampus)
    .SetCost(Costs.Build
        .CP4(1200)
        .Product(256, Ids.Products.Electronics4)
        .Product(64, RecursiveIndustryIds.Products.ValidatedControlPackage)
        .Product(16, RecursiveIndustryIds.Products.FrontierProgram)
        .Product(8, RecursiveIndustryIds.Products.ValidatedResearchDossier)
        .Workers(48)
        .MaintenanceT3(16))
    .SetElectricityConsumption(4000.Kw())
    .SetComputingConsumption(Computing.FromTFlops(256))
    .SetCustomIconPath(RecursiveIndustryIcons.DeploymentAssuranceCampus);
recipe
    .Start(RecursiveIndustryIds.Recipes.BatchDeploymentAssurance)
    .AddInput(16, RecursiveIndustryIds.Products.ModelArchive)
    .AddInput(32, Ids.Products.LabEquipment4)
    .AddInput(32, Ids.Products.Electronics3)
    .AddOutput(128, RecursiveIndustryIds.Products.ValidatedControlPackage)
    .BindTo(campus, 720.Seconds());
"""


def implemented_catalog() -> dict[str, object]:
    return copy.deepcopy(CATALOG)


class ControlNetworkAuditTests(unittest.TestCase):
    def test_valid_data_product_contract_passes(self) -> None:
        self.assertEqual(audit_data_product_source(VALID_DATA_PRODUCT), [])

    def test_fluid_alias_fixture_is_rejected(self) -> None:
        source = (FIXTURES / "data_product_inherits_fluid.cs").read_text(
            encoding="utf-8"
        )
        errors = audit_data_product_source(source)
        self.assertTrue(any("FluidProductProto" in error for error in errors))

    def test_valid_transport_contract_passes(self) -> None:
        self.assertEqual(audit_transport_source(VALID_TRANSPORT), [])

    def test_mixed_transport_fixture_is_rejected(self) -> None:
        source = (FIXTURES / "transport_allows_mixed.cs").read_text(
            encoding="utf-8"
        )
        errors = audit_transport_source(VALID_TRANSPORT + source)
        self.assertIn("Fiber transports must not allow mixed products", errors)

    def test_valid_gateway_vector_passes(self) -> None:
        self.assertEqual(audit_gateway_source(VALID_GATEWAY), [])

    def test_gateway_output_drift_is_rejected(self) -> None:
        errors = audit_gateway_source(VALID_GATEWAY.replace("AddOutput(210", "AddOutput(211"))
        self.assertTrue(any("AddOutput(210" in error for error in errors))

    def test_valid_assurance_vector_passes(self) -> None:
        self.assertEqual(audit_assurance_source(VALID_ASSURANCE), [])

    def test_assurance_yield_drift_is_rejected(self) -> None:
        errors = audit_assurance_source(
            VALID_ASSURANCE.replace("AddOutput(128", "AddOutput(129")
        )
        self.assertTrue(any("AddOutput(128" in error for error in errors))

    def test_implemented_catalog_passes(self) -> None:
        self.assertEqual(audit_catalog(implemented_catalog(), CONTROL), [])

    def test_electronics_three_wrong_owner_is_rejected(self) -> None:
        catalog = implemented_catalog()
        robotic = next(
            item
            for item in catalog["facilities"]
            if item["key"] == "robotic_components_fab"
        )
        binding = next(
            item
            for item in robotic["direct_bindings"]
            if item["recipe_id"] == "Electronics3Assembly"
        )
        robotic["direct_bindings"].remove(binding)
        catalog["facilities"][0]["direct_bindings"].append(binding)
        errors = audit_catalog(catalog, CONTROL)
        self.assertTrue(any("owned only" in error for error in errors))

    def test_right_edge_oversubscription_is_rejected(self) -> None:
        control = copy.deepcopy(CONTROL)
        primary = next(
            owner
            for owner in control["owners"]
            if owner["key"] == "primary_smelter"
        )
        primary["output_ports"] = 7
        errors = audit_catalog(implemented_catalog(), control)
        self.assertTrue(any("right edge is oversubscribed" in error for error in errors))

    def test_direct_stream_and_automatic_fallback_fixtures_are_rejected(self) -> None:
        fixtures = {
            path.name: path.read_text(encoding="utf-8")
            for path in (
                FIXTURES / "direct_stream_input.cs",
                FIXTURES / "automatic_direct_fallback.cs",
            )
        }
        errors = audit_forbidden_runtime(fixtures)
        self.assertTrue(any("Direct Stream input" in error for error in errors))
        self.assertTrue(any("automatic Direct fallback" in error for error in errors))


if __name__ == "__main__":
    unittest.main()