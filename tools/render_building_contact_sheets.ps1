$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$manifest = Get-Content -LiteralPath (Join-Path $root 'art\RecursiveIndustry\Buildings\asset-manifest.json') -Raw | ConvertFrom-Json
$font = [Drawing.Font]::new('Segoe UI', 12, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
$brush = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(225, 231, 230))
try {
    $pageCount = [int][Math]::Ceiling($manifest.models.Count / 16.0)
    for ($page = 0; $page -lt $pageCount; $page++) {
        $bitmap = [Drawing.Bitmap]::new(1680, 1336)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([Drawing.Color]::FromArgb(47, 54, 56))
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            for ($slot = 0; $slot -lt 16; $slot++) {
                if ($page * 16 + $slot -ge $manifest.models.Count) { break }
                $model = $manifest.models[$page * 16 + $slot]
                $image = [Drawing.Image]::FromFile((Join-Path $root $model.preview_path))
                $left = ($slot % 4) * 420
                $top = [int][Math]::Floor($slot / 4) * 334
                try { $graphics.DrawImage($image, [Drawing.Rectangle]::new($left, $top, 420, 300)) }
                finally { $image.Dispose() }
                $label = $model.key.Replace('_', ' ') + '  [' + ($model.triangles_per_lod -join '/') + ' tris]'
                $graphics.DrawString($label, $font, $brush, [Drawing.RectangleF]::new($left + 12, $top + 300, 396, 33))
            }
            $path = Join-Path $root ('art\RecursiveIndustry\Buildings\previews\contact-sheet-{0:00}.png' -f ($page + 1))
            $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
}
finally { $brush.Dispose(); $font.Dispose() }
Write-Output ('PASS: ' + $pageCount + ' contact sheets include all ' + $manifest.models.Count + ' final-bundle model previews and their three LOD triangle counts.')