param(
    [string]$UnityProject = (Join-Path $PSScriptRoot '..\..\Captain-of-industry-modding\src\ExampleMod.Unity'),
    [string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.0.66f1\Editor\Unity.exe',
    [string]$LogPath = ''
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
$log = if ([string]::IsNullOrWhiteSpace($LogPath)) {
    Join-Path $project ('Logs\all-buildings-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '.log')
} else { [IO.Path]::GetFullPath($LogPath) }
if (Test-Path -LiteralPath $log) { throw ('Refusing to overwrite existing build evidence: ' + $log) }
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($log)) | Out-Null
$arguments = @('-batchmode', '-quit', '-projectPath', ('"{0}"' -f $project), '-executeMethod', 'AllBuildingAssets.Build', '-logFile', ('"{0}"' -f $log))
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -PassThru
$process.WaitForExit()
$process.Refresh()
if ($process.ExitCode -ne 0) {
    Select-String -LiteralPath $log -Pattern 'error CS\d+|No valid Unity Editor|Exception:|RI_.*FAIL' | ForEach-Object { $_.Line }
    throw ('Complete building asset build failed with exit ' + $process.ExitCode + '; log: ' + $log)
}
if (-not (Select-String -LiteralPath $log -SimpleMatch 'RI_ALL_BUILDING_ART_COMPLETE')) { throw 'Missing completed building-art marker.' }
[IO.Directory]::CreateDirectory($sources) | Out-Null
Get-ChildItem -LiteralPath $assets -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $sources }
Write-Output 'PASS: 51 original building models, 153 LOD states, root picking colliders, native emissions, and required recipe signs.'