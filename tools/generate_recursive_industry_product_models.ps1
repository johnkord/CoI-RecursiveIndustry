[CmdletBinding()]
param(
    [string]$OutputRoot = '',
    [string]$IconRoot = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $PSScriptRoot '..\art\RecursiveIndustry\ProductModels\production\v1-cartridge-family'
}
if ([string]::IsNullOrWhiteSpace($IconRoot)) {
    $IconRoot = Join-Path $PSScriptRoot '..\art\RecursiveIndustry\ProductIcons\exports'
}

$root = [System.IO.Path]::GetFullPath($OutputRoot)
$iconRootPath = [System.IO.Path]::GetFullPath($IconRoot)
$assetDir = Join-Path $root 'assets'
$proofDir = Join-Path $root 'proofs'
[System.IO.Directory]::CreateDirectory($assetDir) | Out-Null
[System.IO.Directory]::CreateDirectory($proofDir) | Out-Null

$products = @(
    [pscustomobject]@{ Slug = 'accelerator_module'; Accent = '#21C4D4'; Shell = '#3B4B55'; Markers = 1 },
    [pscustomobject]@{ Slug = 'accelerator_rack_i'; Accent = '#21C4D4'; Shell = '#374A54'; Markers = 2 },
    [pscustomobject]@{ Slug = 'frontier_rack_ii'; Accent = '#2ABFD5'; Shell = '#344852'; Markers = 3 },
    [pscustomobject]@{ Slug = 'recursive_rack_iii'; Accent = '#38B8DC'; Shell = '#314650'; Markers = 4 },
    [pscustomobject]@{ Slug = 'dataset_archive'; Accent = '#3B9FD0'; Shell = '#394B56'; Markers = 3 },
    [pscustomobject]@{ Slug = 'model_archive'; Accent = '#E0B84C'; Shell = '#444A4E'; Markers = 1 },
    [pscustomobject]@{ Slug = 'validated_control_package'; Accent = '#59C979'; Shell = '#3B4C48'; Markers = 2 },
    [pscustomobject]@{ Slug = 'spent_accelerator'; Accent = '#F06A38'; Shell = '#4A4746'; Markers = 1 }
)

function Format-Float([double]$value) {
    return $value.ToString('0.######', [System.Globalization.CultureInfo]::InvariantCulture)
}

function Write-CartridgeObj([string]$path) {
    $points = @(
        @(-0.35, -0.50),
        @( 0.35, -0.50),
        @( 0.50, -0.35),
        @( 0.50,  0.35),
        @( 0.35,  0.50),
        @(-0.35,  0.50),
        @(-0.50,  0.35),
        @(-0.50, -0.35)
    )
    $topUvs = @(
        @(0.15, 0.05),
        @(0.85, 0.05),
        @(0.95, 0.15),
        @(0.95, 0.85),
        @(0.85, 0.95),
        @(0.15, 0.95),
        @(0.05, 0.85),
        @(0.05, 0.15)
    )
    $halfHeight = 0.5
    $vertices = New-Object System.Collections.Generic.List[object]
    $uvs = New-Object System.Collections.Generic.List[object]
    $normals = New-Object System.Collections.Generic.List[object]
    $faces = New-Object System.Collections.Generic.List[object]

    function Add-Vertex($x, $y, $z, $u, $v, $nx, $ny, $nz) {
        $vertices.Add(@($x, $y, $z))
        $uvs.Add(@($u, $v))
        $normals.Add(@($nx, $ny, $nz))
        return $vertices.Count
    }

    $topCenter = Add-Vertex 0 $halfHeight 0 0.5 0.5 0 1 0
    $topRing = @()
    for ($index = 0; $index -lt $points.Count; $index++) {
        $topRing += Add-Vertex $points[$index][0] $halfHeight $points[$index][1] $topUvs[$index][0] $topUvs[$index][1] 0 1 0
    }
    for ($index = 0; $index -lt $points.Count; $index++) {
        $next = ($index + 1) % $points.Count
        $faces.Add(@($topCenter, $topRing[$next], $topRing[$index]))
    }

    $bottomCenter = Add-Vertex 0 (-$halfHeight) 0 0.5 0.02 0 -1 0
    $bottomRing = @()
    for ($index = 0; $index -lt $points.Count; $index++) {
        $bottomRing += Add-Vertex $points[$index][0] (-$halfHeight) $points[$index][1] (0.1 + $index * 0.01) 0.02 0 -1 0
    }
    for ($index = 0; $index -lt $points.Count; $index++) {
        $next = ($index + 1) % $points.Count
        $faces.Add(@($bottomCenter, $bottomRing[$index], $bottomRing[$next]))
    }

    for ($index = 0; $index -lt $points.Count; $index++) {
        $next = ($index + 1) % $points.Count
        $midX = ($points[$index][0] + $points[$next][0]) / 2
        $midZ = ($points[$index][1] + $points[$next][1]) / 2
        $length = [Math]::Sqrt($midX * $midX + $midZ * $midZ)
        $normalX = $midX / $length
        $normalZ = $midZ / $length
        $u0 = $index / $points.Count
        $u1 = ($index + 1) / $points.Count
        $bottomCurrent = Add-Vertex $points[$index][0] (-$halfHeight) $points[$index][1] $u0 0 $normalX 0 $normalZ
        $topCurrent = Add-Vertex $points[$index][0] $halfHeight $points[$index][1] $u0 0.07 $normalX 0 $normalZ
        $topNext = Add-Vertex $points[$next][0] $halfHeight $points[$next][1] $u1 0.07 $normalX 0 $normalZ
        $bottomNext = Add-Vertex $points[$next][0] (-$halfHeight) $points[$next][1] $u1 0 $normalX 0 $normalZ
        $faces.Add(@($bottomCurrent, $topCurrent, $topNext))
        $faces.Add(@($bottomCurrent, $topNext, $bottomNext))
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# Recursive Industry procedural product cartridge')
    $lines.Add('o RecursiveIndustry_Cartridge')
    foreach ($vertex in $vertices) {
        $lines.Add("v $(Format-Float $vertex[0]) $(Format-Float $vertex[1]) $(Format-Float $vertex[2])")
    }
    foreach ($uv in $uvs) {
        $lines.Add("vt $(Format-Float $uv[0]) $(Format-Float $uv[1])")
    }
    foreach ($normal in $normals) {
        $lines.Add("vn $(Format-Float $normal[0]) $(Format-Float $normal[1]) $(Format-Float $normal[2])")
    }
    $lines.Add('s off')
    foreach ($face in $faces) {
        $indices = $face | ForEach-Object { "$_/$_/$_" }
        $lines.Add("f $($indices -join ' ')")
    }
    [System.IO.File]::WriteAllLines($path, $lines)
}

function New-Texture([string]$path, [scriptblock]$paint) {
    $bitmap = New-Object System.Drawing.Bitmap(512, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        & $paint $bitmap $graphics
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

$meshPath = Join-Path $assetDir 'cartridge-LOD0.obj'
$normalPath = Join-Path $assetDir 'cartridge-normals.png'
$smoothMetalPath = Join-Path $assetDir 'cartridge-smoothmetal.png'
Write-CartridgeObj $meshPath

New-Texture $normalPath {
    param($bitmap, $graphics)
    $graphics.Clear([System.Drawing.Color]::FromArgb(255, 128, 128, 255))
}

New-Texture $smoothMetalPath {
    param($bitmap, $graphics)
    $graphics.Clear([System.Drawing.Color]::FromArgb(88, 96, 0, 0))
}

foreach ($product in $products) {
    $iconPath = Join-Path $iconRootPath ($product.Slug + '.png')
    if (-not [System.IO.File]::Exists($iconPath)) {
        throw "Missing canonical icon: $iconPath"
    }
    $albedoPath = Join-Path $assetDir ($product.Slug + '-albedo.png')
    New-Texture $albedoPath {
        param($bitmap, $graphics)
        $graphics.Clear([System.Drawing.ColorTranslator]::FromHtml($product.Shell))
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

        $borderPen = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml('#17212A'), 30)
        $accentPen = New-Object System.Drawing.Pen([System.Drawing.ColorTranslator]::FromHtml($product.Accent), 8)
        try {
            $graphics.DrawRectangle($borderPen, 16, 16, 480, 480)
            $graphics.DrawRectangle($accentPen, 34, 34, 444, 444)
        }
        finally {
            $accentPen.Dispose()
            $borderPen.Dispose()
        }

        $icon = [System.Drawing.Image]::FromFile($iconPath)
        try {
            $graphics.DrawImage($icon, 76, 76, 360, 360)
        }
        finally {
            $icon.Dispose()
        }

        $sideBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#17212A'))
        $accentBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml($product.Accent))
        $markerBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#F5BC3E'))
        try {
            $graphics.FillRectangle($sideBrush, 0, 474, 512, 38)
            $graphics.FillRectangle($accentBrush, 0, 474, 512, 10)
            for ($marker = 0; $marker -lt $product.Markers; $marker++) {
                $graphics.FillRectangle($markerBrush, 28 + ($marker * 42), 490, 24, 12)
            }
        }
        finally {
            $markerBrush.Dispose()
            $accentBrush.Dispose()
            $sideBrush.Dispose()
        }
    }
}

$sheetPath = Join-Path $proofDir 'production-cartridge-sheet.png'
$sheet = New-Object System.Drawing.Bitmap(1024, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sheetGraphics = [System.Drawing.Graphics]::FromImage($sheet)
try {
    $sheetGraphics.Clear([System.Drawing.ColorTranslator]::FromHtml('#121A20'))
    $sheetGraphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    for ($index = 0; $index -lt $products.Count; $index++) {
        $row = [Math]::Floor($index / 4)
        $column = $index % 4
        $albedo = [System.Drawing.Image]::FromFile((Join-Path $assetDir ($products[$index].Slug + '-albedo.png')))
        try {
            $sheetGraphics.DrawImage($albedo, $column * 256, $row * 256, 256, 256)
        }
        finally {
            $albedo.Dispose()
        }
    }
    $sheet.Save($sheetPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $sheetGraphics.Dispose()
    $sheet.Dispose()
}

$generatedFiles = @(
    Get-Item $meshPath, $normalPath, $smoothMetalPath, $sheetPath
    Get-ChildItem $assetDir -Filter '*-albedo.png'
)
$generatedFiles |
    Select-Object FullName, Length, @{Name='SHA256';Expression={(Get-FileHash $_.FullName -Algorithm SHA256).Hash}} |
    Sort-Object FullName |
    Format-Table -AutoSize
