param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$media = Join-Path $Root "media"
$productIcons = Join-Path $Root "art\RecursiveIndustry\ProductIcons\exports"
$uiIcons = Join-Path $Root "art\RecursiveIndustry\UiIcons\exports"
New-Item -ItemType Directory -Force -Path $media | Out-Null

function New-Canvas([int]$width, [int]$height) {
    $bitmap = New-Object System.Drawing.Bitmap($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bitmap.SetResolution(96, 96)
    return $bitmap
}

function Draw-ContainedImage($graphics, [string]$path, $bounds) {
    $image = [System.Drawing.Image]::FromFile($path)
    try {
        $scale = [Math]::Min($bounds.Width / $image.Width, $bounds.Height / $image.Height)
        $width = [int]($image.Width * $scale)
        $height = [int]($image.Height * $scale)
        $x = [int]($bounds.X + ($bounds.Width - $width) / 2)
        $y = [int]($bounds.Y + ($bounds.Height - $height) / 2)
        $graphics.DrawImage($image, $x, $y, $width, $height)
    }
    finally {
        $image.Dispose()
    }
}

function Add-IndustrialBackground($graphics, [int]$width, [int]$height) {
    $graphics.Clear([System.Drawing.Color]::FromArgb(255, 13, 21, 28))
    $gridPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(30, 94, 197, 208), 1)
    try {
        for ($x = 0; $x -lt $width; $x += 40) { $graphics.DrawLine($gridPen, $x, 0, $x, $height) }
        for ($y = 0; $y -lt $height; $y += 40) { $graphics.DrawLine($gridPen, 0, $y, $width, $y) }
    }
    finally {
        $gridPen.Dispose()
    }
    $cyan = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 79, 213, 222))
    $gold = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 224, 174, 78))
    try {
        $graphics.FillRectangle($cyan, 0, 0, $width, 10)
        $graphics.FillRectangle($gold, 0, $height - 10, $width, 10)
    }
    finally {
        $cyan.Dispose()
        $gold.Dispose()
    }
}

function Save-Png($bitmap, [string]$path) {
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "Wrote $path"
}

$social = New-Canvas 1280 640
$g = [System.Drawing.Graphics]::FromImage($social)
try {
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    Add-IndustrialBackground $g 1280 640

    $framePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(180, 79, 213, 222), 3)
    $panelBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(220, 18, 31, 40))
    $titleBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 244, 245))
    $cyanBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 112, 222, 229))
    $mutedBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 170, 187, 194))
    $titleFont = New-Object System.Drawing.Font("Segoe UI", 58, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $subFont = New-Object System.Drawing.Font("Segoe UI", 25, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
    $labelFont = New-Object System.Drawing.Font("Segoe UI", 18, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    try {
        $g.FillRectangle($panelBrush, 58, 70, 460, 500)
        $g.DrawRectangle($framePen, 58, 70, 460, 500)
        Draw-ContainedImage $g (Join-Path $productIcons "recursive_rack_iii.png") ([System.Drawing.Rectangle]::new(92, 102, 392, 392))
        $g.DrawString("PHYSICAL COMPUTE", $labelFont, $cyanBrush, 183, 522)

        $g.DrawString("RECURSIVE", $titleFont, $titleBrush, 575, 133)
        $g.DrawString("INDUSTRY", $titleFont, $titleBrush, 575, 202)
        $g.DrawString("A physical AI economy for the endgame", $subFont, $cyanBrush, 581, 300)
        $g.DrawString("COMPUTE  |  VALIDATE  |  AUTOMATE  |  EXPAND", $labelFont, $mutedBrush, 581, 352)

        $iconBounds = @(
            [System.Drawing.Rectangle]::new(590, 425, 90, 90),
            [System.Drawing.Rectangle]::new(716, 425, 90, 90),
            [System.Drawing.Rectangle]::new(842, 425, 90, 90),
            [System.Drawing.Rectangle]::new(968, 425, 90, 90)
        )
        $iconPaths = @(
            (Join-Path $productIcons "validated_control_package.png"),
            (Join-Path $uiIcons "autonomous_amphibious_hauler.png"),
            (Join-Path $uiIcons "planetary_coordination_center.png"),
            (Join-Path $uiIcons "orbital_power_relay.png")
        )
        for ($i = 0; $i -lt $iconBounds.Count; $i++) {
            Draw-ContainedImage $g $iconPaths[$i] $iconBounds[$i]
        }
    }
    finally {
        $framePen.Dispose(); $panelBrush.Dispose(); $titleBrush.Dispose(); $cyanBrush.Dispose(); $mutedBrush.Dispose()
        $titleFont.Dispose(); $subFont.Dispose(); $labelFont.Dispose()
    }
}
finally {
    $g.Dispose()
}
Save-Png $social (Join-Path $media "social-preview.png")
$social.Dispose()

$thumb = New-Canvas 512 512
$g = [System.Drawing.Graphics]::FromImage($thumb)
try {
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    Add-IndustrialBackground $g 512 512

    $panelBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(225, 18, 31, 40))
    $framePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(200, 224, 174, 78), 3)
    $titleBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 239, 244, 245))
    $font = New-Object System.Drawing.Font("Segoe UI", 34, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    try {
        $g.FillRectangle($panelBrush, 48, 42, 416, 428)
        $g.DrawRectangle($framePen, 48, 42, 416, 428)
        Draw-ContainedImage $g (Join-Path $productIcons "recursive_rack_iii.png") ([System.Drawing.Rectangle]::new(104, 68, 304, 304))
        $g.DrawString("RECURSIVE INDUSTRY", $font, $titleBrush, ([System.Drawing.RectangleF]::new(54, 395, 404, 52)), $format)
    }
    finally {
        $panelBrush.Dispose(); $framePen.Dispose(); $titleBrush.Dispose(); $font.Dispose(); $format.Dispose()
    }
}
finally {
    $g.Dispose()
}
Save-Png $thumb (Join-Path $media "hub-thumbnail.png")
$thumb.Dispose()
