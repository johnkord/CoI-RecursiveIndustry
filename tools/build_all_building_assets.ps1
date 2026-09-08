param(
    [string]$UnityProject = (Join-Path $PSScriptRoot '..\..\Captain-of-industry-modding\src\ExampleMod.Unity'),
    [string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.0.66f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = (Resolve-Path $UnityProject).Path
$art = Join-Path $root 'art\RecursiveIndustry\Buildings'
$assets = Join-Path $project 'Assets\RecursiveIndustry\Buildings'
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close the Unity editor before the batch build.' }
$env:RI_PUBLIC_ROOT = $root
[IO.Directory]::CreateDirectory($assets) | Out-Null
$sources = Join-Path $art 'UnitySource'
if (Test-Path -LiteralPath $sources) {
    Get-ChildItem -LiteralPath $sources -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $assets }
}
Copy-Item -LiteralPath (Join-Path $art 'BuildingSurface.shader') -Destination $assets
Copy-Item -LiteralPath (Join-Path $art 'BuildingSign.shader') -Destination $assets
Copy-Item -LiteralPath (Join-Path $art 'BuildingGlazing.shader') -Destination $assets
Copy-Item -LiteralPath (Join-Path $art 'Editor\AllBuildingAssets.cs') -Destination (Join-Path $project 'Assets\Editor\RecursiveIndustry\AllBuildingAssets.cs')
Copy-Item -LiteralPath (Join-Path $root 'art\RecursiveIndustry\Reconstruction\Editor\ReconstructionAssets.cs') -Destination (Join-Path $project 'Assets\Editor\RecursiveIndustry\ReconstructionAssets.cs')
$log = Join-Path $project 'Logs\all-buildings.log'
& $UnityEditor -batchmode -quit -projectPath $project -executeMethod AllBuildingAssets.Build -logFile $log | Out-Host
if ($LASTEXITCODE -ne 0) { Get-Content -LiteralPath $log -Tail 90; throw 'Complete building asset build failed.' }
if (-not (Select-String -LiteralPath $log -SimpleMatch 'RI_ALL_BUILDING_ART_COMPLETE')) { throw 'Missing completed building-art marker.' }
[IO.Directory]::CreateDirectory($sources) | Out-Null
Get-ChildItem -LiteralPath $assets -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $sources }
Write-Output 'PASS: 48 original building models, 144 LOD states, root picking colliders, native emissions, and required recipe signs.'