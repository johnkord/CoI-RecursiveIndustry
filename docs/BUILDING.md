# Building and Packaging

## Prerequisites

- Windows.
- A lawfully installed Captain of Industry 0.8.7.
- The Trains expansion installed.
- A .NET SDK capable of targeting .NET Framework 4.8.
- Python 3.11 or newer for repository tools.

Set `COI_ROOT` to the game directory:

```powershell
$env:COI_ROOT = "C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry"
```

The project resolves assemblies from:

```text
%COI_ROOT%/Captain of Industry_Data/Managed
%COI_ROOT%/DLCs/Mafi.TrainsDlc
```

Never copy those assemblies into this repository.

## Offline validation

```powershell
python tools/validate_public_repo.py
python tools/generate_recursive_industry_universal_source.py
python -m unittest discover -s tests -p "test_*.py"
```

This check requires no game installation and runs in public CI.

## Compile

```powershell
dotnet build mods/RecursiveIndustry/RecursiveIndustry.csproj -c Release
```

The build deploys player files to
`%APPDATA%/Captain of Industry/Mods/RecursiveIndustry` by default. To compile
without deployment:

```powershell
dotnet build mods/RecursiveIndustry/RecursiveIndustry.csproj -c Release `
  /p:DeployToModsFolder=false
```

All game references use `Private=false` so proprietary assemblies are not copied
to the output.

## Package

```powershell
python tools/package_mod.py mods/RecursiveIndustry
python tools/audit_release_zip.py
```

The deterministic package is written under `dist/` and contains one
`RecursiveIndustry/` root. Release packages contain:

- `manifest.json`;
- `config.json`;
- `readme.txt`;
- `changelog.txt`;
- `RecursiveIndustry.dll`; and
- the three runtime bundles plus `mafi_bundles.manifest`.

PDB files are excluded unless packaging explicitly requests symbols. Game and
engine DLL names are rejected.

## Clean-install check

Before publishing, extract the candidate ZIP into an empty temporary Mods folder
and confirm there is exactly one `RecursiveIndustry/` directory. Test the exact
archive on a fresh world, exit normally, and inspect the complete game log.

A source, manifest, readme, DLL, config, or bundle change produces a new archive
identity and requires a new version. Never overwrite an archive already used for
testing or distribution.
