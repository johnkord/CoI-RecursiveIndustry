param(
    [string]$UnityProject = (Join-Path $PSScriptRoot '..\..\Captain-of-industry-modding\src\ExampleMod.Unity'),
    [string]$UnityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.0.66f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = (Resolve-Path $UnityProject).Path
if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close Unity before the native graphics policy check.' }
dotnet build (Join-Path $root 'tests\WorldArt.Policy\WorldArt.Policy.csproj') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Native graphics policy compilation failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'unity\WorldNativePolicyRunner.cs') -Destination (Join-Path $project 'Assets\Editor\RecursiveIndustry\WorldNativePolicyRunner.cs')
$env:RI_WORLD_POLICY_ASSEMBLY = Join-Path $root 'tests\WorldArt.Policy\bin\Release\WorldArt.Policy.exe'
$log = Join-Path $project 'Logs\world-native-policy.log'
& $UnityEditor -batchmode -quit -projectPath $project -executeMethod WorldNativePolicyRunner.Run -logFile $log | Out-Host
if ($LASTEXITCODE -ne 0) { Get-Content -LiteralPath $log -Tail 70; throw 'Native graphics policy check failed.' }
$result = Select-String -LiteralPath $log -SimpleMatch 'RI_WORLD_NATIVE_POLICY_COMPLETE checks='
if (-not $result) { throw 'Native graphics policy completion marker is missing.' }
$result | ForEach-Object { $_.Line }