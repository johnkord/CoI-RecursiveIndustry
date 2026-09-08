param(
    [string]$UnityProject = (Join-Path $PSScriptRoot '..\..\Captain-of-industry-modding\src\ExampleMod.Unity'),
    [string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.0.66f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = (Resolve-Path $UnityProject).Path
$art = Join-Path $root 'art\RecursiveIndustry\World'
$assets = Join-Path $project 'Assets\RecursiveIndustry\World'
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close the Unity editor before the batch build.' }
$env:RI_PUBLIC_ROOT = $root
[IO.Directory]::CreateDirectory($assets) | Out-Null
$sources = Join-Path $art 'UnitySource'
if (Test-Path -LiteralPath $sources) {
    Get-ChildItem -LiteralPath $sources -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $assets }
}
Get-ChildItem -LiteralPath $art -File -Filter '*.shader' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $assets }
Get-ChildItem -LiteralPath (Join-Path $art 'Editor') -File -Filter '*.cs' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $project 'Assets\Editor\RecursiveIndustry') }
Copy-Item -LiteralPath (Join-Path $root 'art\RecursiveIndustry\Reconstruction\Editor\ReconstructionAssets.cs') -Destination (Join-Path $project 'Assets\Editor\RecursiveIndustry\ReconstructionAssets.cs')
$log = Join-Path $project 'Logs\world-art.log'
& $UnityEditor -batchmode -quit -projectPath $project -executeMethod WorldAssets.Build -logFile $log | Out-Host
if ($LASTEXITCODE -ne 0) { Get-Content -LiteralPath $log -Tail 100; throw 'World asset build failed.' }
$completion = Select-String -LiteralPath $log -SimpleMatch 'RI_WORLD_ART_COMPLETE models='
if (-not $completion) { throw 'Missing completed world-art marker.' }
[IO.Directory]::CreateDirectory($sources) | Out-Null
Get-ChildItem -LiteralPath $assets -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $sources }
$completion | ForEach-Object { $_.Line }
Write-Output 'PASS: original world assets built and final bundle models validated; no deployment.'