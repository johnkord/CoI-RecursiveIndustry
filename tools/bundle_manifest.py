"""Read MaFi's dependency-before-root bundle manifest format."""

from __future__ import annotations

import re


def parse_bundle_manifest(text: str) -> dict[str, tuple[str, ...]]:
    roots: dict[str, tuple[str, ...]] = {}
    pending: list[str] = []
    for raw in text.splitlines():
        line = raw.strip()
        if not line:
            continue
        name = line[1:] if line.startswith("+") else line
        if not re.fullmatch(r"[a-z0-9_]+", name):
            raise ValueError(f"Invalid bundle name: {line}")
        if line.startswith("+"):
            if name in pending:
                raise ValueError(f"Duplicate bundle dependency: {name}")
            pending.append(name)
        else:
            if name in roots or name in pending:
                raise ValueError(f"Duplicate or self-dependent bundle root: {name}")
            roots[name] = tuple(pending)
            pending = []
    if pending or not roots:
        raise ValueError("Missing bundle root")

    def visit(name: str, stack: set[str]) -> None:
        if name in stack:
            raise ValueError(f"Cyclic bundle dependency: {name}")
        for dependency in roots.get(name, ()):
            visit(dependency, stack | {name})

    for name in roots:
        visit(name, set())
    return roots


def bundle_files(text: str) -> set[str]:
    roots = parse_bundle_manifest(text)
    return set(roots) | {dependency for dependencies in roots.values() for dependency in dependencies}