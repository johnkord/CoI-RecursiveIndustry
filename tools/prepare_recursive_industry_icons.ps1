[CmdletBinding()]
param(
    [string]$SourceDir = '',
    [string]$OutputDir = '',
    [int]$Size = 512,
    [int]$Margin = 24,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;

public static class RecursiveIndustryIconAlphaBounds {
    public static Rectangle Find(Bitmap bitmap, int minimumAlpha) {
        int minX = bitmap.Width;
        int minY = bitmap.Height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < bitmap.Height; y++) {
            for (int x = 0; x < bitmap.Width; x++) {
                if (bitmap.GetPixel(x, y).A < minimumAlpha) {
                    continue;
                }
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        return maxX < minX || maxY < minY
            ? Rectangle.Empty
            : Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
    }
}
'@

if ([string]::IsNullOrWhiteSpace($SourceDir)) {
    $SourceDir = Join-Path $PSScriptRoot '..\art\RecursiveIndustry\ProductIcons\masters'
}
if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $PSScriptRoot '..\art\RecursiveIndustry\ProductIcons\exports'
}
if ($Margin -lt 0 -or $Margin * 2 -ge $Size) {
    throw "Margin must be non-negative and less than half of Size."
}

$iconFiles = [ordered]@{
    'accelerator_module-master.png' = 'accelerator_module.png'
    'accelerator_rack_i-master.png' = 'accelerator_rack_i.png'
    'frontier_rack_ii-master.png' = 'frontier_rack_ii.png'
    'recursive_rack_iii-master.png' = 'recursive_rack_iii.png'
    'dataset_archive-master.png' = 'dataset_archive.png'
    'model_archive-master.png' = 'model_archive.png'
    'validated_control_package-master.png' = 'validated_control_package.png'
    'spent_accelerator-master.png' = 'spent_accelerator.png'
}

function Assert-TransparentCorners {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [string]$Path
    )

    $lastX = $Bitmap.Width - 1
    $lastY = $Bitmap.Height - 1
    $cornerAlpha = @(
        $Bitmap.GetPixel(0, 0).A
        $Bitmap.GetPixel($lastX, 0).A
        $Bitmap.GetPixel(0, $lastY).A
        $Bitmap.GetPixel($lastX, $lastY).A
    )

    if ($cornerAlpha | Where-Object { $_ -gt 8 }) {
        throw "$Path does not have transparent corners (alpha: $($cornerAlpha -join ', '))."
    }
}

function Assert-Export {
    param(
        [string]$Path,
        [int]$ExpectedSize
    )

    $bitmap = [System.Drawing.Bitmap]::FromFile($Path)
    try {
        if ($bitmap.Width -ne $ExpectedSize -or $bitmap.Height -ne $ExpectedSize) {
            throw "$Path is $($bitmap.Width)x$($bitmap.Height), expected ${ExpectedSize}x${ExpectedSize}."
        }
        Assert-TransparentCorners -Bitmap $bitmap -Path $Path
    }
    finally {
        $bitmap.Dispose()
    }
}

$sourceRoot = [System.IO.Path]::GetFullPath($SourceDir)
$outputRoot = [System.IO.Path]::GetFullPath($OutputDir)
[System.IO.Directory]::CreateDirectory($outputRoot) | Out-Null

$results = foreach ($entry in $iconFiles.GetEnumerator()) {
    $sourcePath = Join-Path $sourceRoot $entry.Key
    $outputPath = Join-Path $outputRoot $entry.Value

    if (-not (Test-Path -LiteralPath $sourcePath)) {
        continue
    }

    if ((Test-Path -LiteralPath $outputPath) -and -not $Force) {
        Assert-Export -Path $outputPath -ExpectedSize $Size
        [pscustomobject]@{
            Icon = $entry.Value
            Status = 'existing'
            Size = "${Size}x${Size}"
            SHA256 = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash
        }
        continue
    }

    $sourceBitmap = [System.Drawing.Bitmap]::FromFile($sourcePath)
    try {
        if ($sourceBitmap.Width -lt $Size -or $sourceBitmap.Height -lt $Size) {
            throw "$sourcePath is smaller than $Size pixels on one or more axes."
        }
        Assert-TransparentCorners -Bitmap $sourceBitmap -Path $sourcePath

        $visibleBounds = [RecursiveIndustryIconAlphaBounds]::Find($sourceBitmap, 8)
        if ($visibleBounds.IsEmpty) {
            throw "$sourcePath contains no visible pixels."
        }

        $sourcePadding = [Math]::Max(2, [Math]::Ceiling(
            [Math]::Max($visibleBounds.Width, $visibleBounds.Height) * 0.01
        ))
        $left = [Math]::Max(0, $visibleBounds.Left - $sourcePadding)
        $top = [Math]::Max(0, $visibleBounds.Top - $sourcePadding)
        $right = [Math]::Min($sourceBitmap.Width, $visibleBounds.Right + $sourcePadding)
        $bottom = [Math]::Min($sourceBitmap.Height, $visibleBounds.Bottom + $sourcePadding)
        $sourceBounds = [System.Drawing.Rectangle]::FromLTRB($left, $top, $right, $bottom)

        $availableSize = $Size - (2 * $Margin)
        $scale = [Math]::Min(
            $availableSize / $sourceBounds.Width,
            $availableSize / $sourceBounds.Height
        )
        $drawWidth = [Math]::Max(1, [Math]::Round($sourceBounds.Width * $scale))
        $drawHeight = [Math]::Max(1, [Math]::Round($sourceBounds.Height * $scale))
        $drawX = [Math]::Floor(($Size - $drawWidth) / 2)
        $drawY = [Math]::Floor(($Size - $drawHeight) / 2)

        $outputBitmap = New-Object System.Drawing.Bitmap(
            $Size,
            $Size,
            [System.Drawing.Imaging.PixelFormat]::Format32bppArgb
        )
        try {
            $graphics = [System.Drawing.Graphics]::FromImage($outputBitmap)
            try {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $graphics.DrawImage(
                    $sourceBitmap,
                    [System.Drawing.Rectangle]::new($drawX, $drawY, $drawWidth, $drawHeight),
                    $sourceBounds.X,
                    $sourceBounds.Y,
                    $sourceBounds.Width,
                    $sourceBounds.Height,
                    [System.Drawing.GraphicsUnit]::Pixel
                )
            }
            finally {
                $graphics.Dispose()
            }

            $outputBitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        }
        finally {
            $outputBitmap.Dispose()
        }
    }
    finally {
        $sourceBitmap.Dispose()
    }

    Assert-Export -Path $outputPath -ExpectedSize $Size
    [pscustomobject]@{
        Icon = $entry.Value
        Status = 'generated'
        Size = "${Size}x${Size}"
        SHA256 = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash
    }
}

if (-not $results) {
    throw "No recognized icon masters were found under $sourceRoot."
}

$results | Format-Table -AutoSize
