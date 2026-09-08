param(
    [string]$UnityProject = (Join-Path $PSScriptRoot '..\..\Captain-of-industry-modding\src\ExampleMod.Unity'),
    [string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.0.66f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = (Resolve-Path $UnityProject).Path
$art = Join-Path $root 'art\RecursiveIndustry\Reconstruction'
$target = Join-Path $project 'Assets\RecursiveIndustry\Reconstruction'
if (-not (Test-Path -LiteralPath $UnityEditor)) { throw 'Unity 6000.0.66f1 is required.' }
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close the Unity editor before this batch build.' }
$env:RI_PUBLIC_ROOT = $root
[IO.Directory]::CreateDirectory($target) | Out-Null
Get-ChildItem -LiteralPath (Join-Path $art 'UnitySource') -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $target
}
Copy-Item -LiteralPath (Join-Path $art 'IndustrialSurface.shader') -Destination $target
Copy-Item -LiteralPath (Join-Path $art 'Editor\ReconstructionAssets.cs') -Destination (Join-Path $project 'Assets\Editor\RecursiveIndustry\ReconstructionAssets.cs')
$log = Join-Path $project 'Logs\reconstruction-build.log'
& $UnityEditor -batchmode -quit -projectPath $project -executeMethod ReconstructionAssets.Build -logFile $log | Out-Host
if ($LASTEXITCODE -ne 0) {
    Get-Content -LiteralPath $log -Tail 80
    throw 'Reconstruction asset build failed.'
}
if (-not (Select-String -LiteralPath $log -SimpleMatch 'RI_RECONSTRUCTION_ASSETS_COMPLETE')) {
    throw 'The asset build did not report completion.'
}
Get-ChildItem -LiteralPath $target -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $art 'UnitySource')
}
Write-Output 'PASS: four original models, three icons, rendered previews, and five reconstruction bundles. No game deployment.'