# Contributing

Contributions are welcome within Recursive Industry's gameplay contract: a
physical, expensive, bounded AI economy that extends vanilla systems without
creating free matter or replacing every conventional option.

## Before opening a pull request

1. Open an issue for substantial gameplay, progression, compatibility, or asset
   changes so the intended scope is clear.
2. Keep changes focused. Do not bundle game DLLs, decompiled source, unmodified
   game assets, saves, logs, or credentials.
3. Preserve stable prototype IDs. Renaming IDs can break saves even when display
   text remains unchanged.
4. Preserve explicit product-kind port mappings and conventional fallback paths.
5. Update player documentation and `CHANGELOG.md` when behavior changes.
6. Run:

   ```powershell
   python tools/validate_public_repo.py
   python tools/generate_recursive_industry_universal_source.py
   python -m unittest discover -s tests -p "test_*.py"
   dotnet build mods/RecursiveIndustry/RecursiveIndustry.csproj -c Release
   python tools/package_mod.py mods/RecursiveIndustry
   ```

The C# build requires Windows, Captain of Industry, the Trains expansion, and a
`COI_ROOT` environment variable pointing to the game installation. Public CI
cannot compile against proprietary game assemblies and therefore runs only the
offline repository checks.

## Generated and binary files

The three runtime bundles under `mods/RecursiveIndustry/AssetBundles/` are
intentional player files. Do not add Unity sidecar manifests or workspace output.
Source art belongs under `art/RecursiveIndustry/`.

Files ending in `.g.cs` are generated from
`data/universal-industry-catalog.json`. Edit the compact catalog, run
`python tools/generate_recursive_industry_universal_source.py --write`, and commit
the authority plus generated results together. Do not hand-edit generated files.

## License

Contributions are accepted under [COI-Open Version 1.0](LICENSE). By submitting a
contribution, you confirm that you have the right to provide it under that license
and the Captain of Industry Modding Policy.
