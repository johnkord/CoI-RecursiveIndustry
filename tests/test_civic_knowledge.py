"""Civic service arithmetic and source contract checks."""

from fractions import Fraction
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from model_civic_knowledge import audit, civic_model, load_contract  # noqa: E402


class CivicKnowledgeTests(unittest.TestCase):
    def test_source_matches_contract(self) -> None:
        self.assertEqual(audit(), [])

    def test_thousand_people_use_three_quarters_not_full_inputs(self) -> None:
        result = civic_model(1000)
        self.assertEqual(result["demand_per_60_time"], 200)
        self.assertEqual(result["center_output_per_60_time"], Fraction(800,3))
        self.assertEqual(result["center_utilization"], Fraction(3,4))
        self.assertEqual(result["direct_civic_inputs_per_3600_time"], {
            "ModelArchive": Fraction(15,2), "DatasetArchive": 60,
            "ValidatedResearchDossier": Fraction(15,2), "OfficeSupplies": 60,
        })
        self.assertEqual(result["module_buffer_months_at_full_demand"], Fraction(6,5))

    def test_shared_support_combines_demand_before_rounding(self) -> None:
        support = civic_model(1000)["shared_support"]
        self.assertEqual(support["total_models_per_3600_time"], Fraction(75,8))
        self.assertEqual(support["total_datasets_per_3600_time"], 225)
        self.assertEqual(support["model_development_centers"], 1)
        self.assertEqual(support["curation_offices"], 1)
        self.assertEqual(support["peak_computing"], 224)
        self.assertEqual(support["staff_including_center_and_one_commons"], 268)

    def test_access_limit_is_not_a_source_upgrade(self) -> None:
        self.assertTrue(civic_model(1000,200)["link_sufficient"])
        self.assertFalse(civic_model(1001,200)["link_sufficient"])
        self.assertEqual(civic_model(1334)["required_centers"], 2)
        self.assertTrue(civic_model(2250)["link_sufficient"])
        self.assertFalse(civic_model(2251)["link_sufficient"])

    def test_no_health_productivity_or_mixed_service(self) -> None:
        data = load_contract()
        self.assertIsNone(data["commons"]["health"])
        self.assertIsNone(data["commons"]["productivity"])
        self.assertFalse(data["stream"]["storable"])
        self.assertFalse(data["stream"]["interchangeable_with_industrial_control"])
        self.assertFalse(data["stream"]["mixed_product_links"])

    def test_invalid_model_demand_fails(self) -> None:
        with self.assertRaises(ValueError): civic_model(0)
        with self.assertRaises(ValueError): civic_model(1000,0)


if __name__ == "__main__":
    unittest.main()