#!/usr/bin/env python3
"""Regression tests for the public repository boundary."""

from __future__ import annotations

from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools"))

from validate_public_repo import (  # noqa: E402
    forbidden_path_reason,
    validate,
)
from generate_recursive_industry_universal_source import check as check_generated  # noqa: E402


class PublicRepositoryTests(unittest.TestCase):
    def test_repository_passes(self) -> None:
        self.assertEqual(validate(), [])

    def test_generated_source_matches_public_catalog(self) -> None:
        self.assertEqual(check_generated(), [])

    def test_private_evidence_path_is_forbidden(self) -> None:
        reason = forbidden_path_reason(
            Path("data/verification_evidence/session.json")
        )
        self.assertIsNotNone(reason)

    def test_validation_mod_is_forbidden(self) -> None:
        reason = forbidden_path_reason(Path("mods/ValidationMod/proof.cs"))
        self.assertIsNotNone(reason)

    def test_player_zip_is_not_source(self) -> None:
        reason = forbidden_path_reason(Path("dist/RecursiveIndustry-1.0.0.zip"))
        self.assertIsNotNone(reason)

    def test_normal_mod_source_is_allowed(self) -> None:
        reason = forbidden_path_reason(
            Path("mods/RecursiveIndustry/src/RecursiveIndustry.cs")
        )
        self.assertIsNone(reason)


if __name__ == "__main__":
    unittest.main()
