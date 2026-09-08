$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$manifest = Get-Content -LiteralPath (Join-Path $root 'art\RecursiveIndustry\World\asset-manifest.json') -Raw | ConvertFrom-Json
$items = @($manifest.models) + @($manifest.assemblies)
$font = [Drawing.Font]::new('Segoe UI', 12, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
$brush = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(225, 231, 230))
try {
    for ($page = 0; $page -lt [Math]::Ceiling($items.Count / 16); $page++) {
        $bitmap = [Drawing.Bitmap]::new(1680, 1336)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([Drawing.Color]::FromArgb(47, 54, 56))
            $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            for ($slot = 0; $slot -lt 16; $slot++) {
                $index = $page * 16 + $slot
                if ($index -ge $items.Count) { break }
                $model = $items[$index]
                $image = [Drawing.Image]::FromFile((Join-Path $root $model.preview_path))
                $left = ($slot % 4) * 420
                $top = [int][Math]::Floor($slot / 4) * 334
                try { $graphics.DrawImage($image, [Drawing.Rectangle]::new($left, $top, 420, 300)) }
                finally { $image.Dispose() }
                $label = $model.key.Replace('_', ' ')
                if ($model.triangles_per_lod) { $label += '  [' + ($model.triangles_per_lod -join '/') + ']' }
                $graphics.DrawString($label, $font, $brush, [Drawing.RectangleF]::new($left + 12, $top + 300, 396, 33))
            }
            $path = Join-Path $root ('art\RecursiveIndustry\World\previews\contact-sheet-{0:00}.png' -f ($page + 1))
            $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
        }
        finally { $graphics.Dispose(); $bitmap.Dispose() }
    }
}
finally { $brush.Dispose(); $font.Dispose() }
Write-Output ('PASS: {0} final model and composed-load previews on five review sheets.' -f $items.Count)