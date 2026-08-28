#!/usr/bin/env python3
"""Regression tests for Autonomous Hauler cargo and Fuel Station compatibility."""

from __future__ import annotations

from pathlib import Path
import unittest


ROOT = Path(__file__).resolve().parents[1]
SOURCE_PATH = (
    ROOT
    / "mods"
    / "RecursiveIndustry"
    / "src"
    / "AutonomousNetworksData.cs"
)


def method_block(source: str, start: str, end: str) -> str:
    return source[source.index(start) : source.index(end)]


class AutonomousHaulerCompatibilityTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        source = SOURCE_PATH.read_text(encoding="utf-8")
        cls.general = method_block(
            source,
            "private static void RegisterAutonomousHauler",
            "private static void RegisterAutonomousSpecializedHaulers",
        )
        cls.specialized = source[source.index(
            "private static void RegisterAutonomousSpecializedHaulers"
        ) :]

    def test_general_hauler_has_all_tier_two_attachments(self) -> None:
        self.assertNotIn("SetFixedProductType", self.general)
        self.assertIn("new TankAttachmentProto(", self.general)
        self.assertIn("new FlatBedAttachmentProto(", self.general)
        self.assertIn("new DumpAttachmentProto(", self.general)
        self.assertIn(
            '"Assets/Base/Vehicles/ModularTruck/T2-tank.prefab"',
            self.general,
        )
        self.assertIn(
            '"Assets/Base/Vehicles/ModularTruck/Truck_Flat.prefab"',
            self.general,
        )
        self.assertIn(
            '"Assets/Base/Vehicles/ModularTruck/Truck_Dump.prefab"',
            self.general,
        )

    def test_tier_three_specialists_remain_fixed_roles(self) -> None:
        self.assertIn("LooseProductProto.ProductType", self.specialized)
        self.assertIn("FluidProductProto.ProductType", self.specialized)
        self.assertIn(".SetFixedProductType(productType)", self.specialized)
        self.assertIn(".SetCapacity(180)", self.specialized)


if __name__ == "__main__":
    unittest.main()