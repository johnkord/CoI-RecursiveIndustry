[CmdletBinding()]
param(
    [string]$OutputRoot = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $PSScriptRoot '..\art\RecursiveIndustry\ProductIcons\iterations\v2-flat-symbols'
}

$root = [System.IO.Path]::GetFullPath($OutputRoot)
$masterDir = Join-Path $root 'masters'
$exportDir = Join-Path $root 'exports'
$proofDir = Join-Path $root 'proofs'
[System.IO.Directory]::CreateDirectory($masterDir) | Out-Null
[System.IO.Directory]::CreateDirectory($exportDir) | Out-Null
[System.IO.Directory]::CreateDirectory($proofDir) | Out-Null

Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class RecursiveIndustryFlatIcons {
    private const int MasterSize = 1024;
    private const int ExportSize = 512;

    private static readonly Color Outline = ColorTranslator.FromHtml("#17212A");
    private static readonly Color Body = ColorTranslator.FromHtml("#3B4B55");
    private static readonly Color Recessed = ColorTranslator.FromHtml("#24313A");
    private static readonly Color Cyan = ColorTranslator.FromHtml("#21C4D4");
    private static readonly Color Gold = ColorTranslator.FromHtml("#F0B83F");
    private static readonly Color Green = ColorTranslator.FromHtml("#5BCB78");
    private static readonly Color White = ColorTranslator.FromHtml("#EDF8F8");
    private static readonly Color Orange = ColorTranslator.FromHtml("#EF6A3A");
    private static readonly Color Spent = ColorTranslator.FromHtml("#687780");

    public static string[] GenerateAll(string masterDir, string exportDir) {
        Directory.CreateDirectory(masterDir);
        Directory.CreateDirectory(exportDir);
        var names = new[] {
            "accelerator_module",
            "accelerator_rack_i",
            "frontier_rack_ii",
            "recursive_rack_iii",
            "dataset_archive",
            "model_archive",
            "validated_control_package",
            "spent_accelerator",
        };
        foreach (var name in names) {
            using (var master = CreateMaster()) {
                using (var graphics = Graphics.FromImage(master)) {
                    Configure(graphics);
                    switch (name) {
                        case "accelerator_module": DrawAcceleratorModule(graphics); break;
                        case "accelerator_rack_i": DrawRackI(graphics); break;
                        case "frontier_rack_ii": DrawRackII(graphics); break;
                        case "recursive_rack_iii": DrawRackIII(graphics); break;
                        case "dataset_archive": DrawDatasetArchive(graphics); break;
                        case "model_archive": DrawModelArchive(graphics); break;
                        case "validated_control_package": DrawControlPackage(graphics); break;
                        case "spent_accelerator": DrawSpentAccelerator(graphics); break;
                        default: throw new InvalidOperationException(name);
                    }
                }
                master.Save(Path.Combine(masterDir, name + "-master.png"), ImageFormat.Png);
                using (var export = Resize(master, ExportSize)) {
                    export.Save(Path.Combine(exportDir, name + ".png"), ImageFormat.Png);
                }
            }
        }
        return names;
    }

    public static void CreateProof(string exportDir, string proofPath, bool grayscale) {
        var names = new[] {
            "accelerator_module", "accelerator_rack_i", "frontier_rack_ii", "recursive_rack_iii",
            "dataset_archive", "model_archive", "validated_control_package", "spent_accelerator",
        };
        int rowHeight = 106;
        using (var proof = new Bitmap(1040, names.Length * rowHeight, PixelFormat.Format32bppArgb))
        using (var graphics = Graphics.FromImage(proof))
        using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
        using (var dark = new SolidBrush(Color.FromArgb(45, 48, 53)))
        using (var gray = grayscale ? GrayscaleAttributes() : null) {
            graphics.Clear(Color.White);
            graphics.FillRectangle(dark, 520, 0, 520, proof.Height);
            for (int row = 0; row < names.Length; row++) {
                int y = row * rowHeight;
                string path = Path.Combine(exportDir, names[row] + ".png");
                using (var image = Image.FromFile(path)) {
                    DrawProofRow(graphics, image, gray, 0, y);
                    DrawProofRow(graphics, image, gray, 520, y);
                }
                string title = ToTitle(names[row]);
                graphics.DrawString(title, font, Brushes.DimGray, 8, y + 76);
                graphics.DrawString(title, font, Brushes.White, 528, y + 76);
            }
            proof.Save(proofPath, ImageFormat.Png);
        }
    }

    private static Bitmap CreateMaster() {
        var bitmap = new Bitmap(MasterSize, MasterSize, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap)) {
            graphics.Clear(Color.Transparent);
        }
        return bitmap;
    }

    private static void Configure(Graphics graphics) {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
    }

    private static Bitmap Resize(Image source, int size) {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap)) {
            graphics.Clear(Color.Transparent);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(source, 0, 0, size, size);
        }
        return bitmap;
    }

    private static GraphicsPath RoundedRect(Rectangle rectangle, int radius) {
        int diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static void FillOutlinedPolygon(Graphics graphics, Point[] points, Color fill, float width = 56f) {
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, width) { LineJoin = LineJoin.Round }) {
            graphics.FillPolygon(brush, points);
            graphics.DrawPolygon(pen, points);
        }
    }

    private static void FillOutlinedRect(Graphics graphics, Rectangle rectangle, Color fill, int radius = 24, float width = 52f) {
        using (var path = RoundedRect(rectangle, radius))
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, width) { LineJoin = LineJoin.Round }) {
            graphics.FillPath(brush, path);
            graphics.DrawPath(pen, path);
        }
    }

    private static void FillOutlinedEllipse(Graphics graphics, Rectangle rectangle, Color fill, float width = 52f) {
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, width)) {
            graphics.FillEllipse(brush, rectangle);
            graphics.DrawEllipse(pen, rectangle);
        }
    }

    private static void FillOutlinedSquareRect(Graphics graphics, Rectangle rectangle, Color fill, float width) {
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, width)) {
            graphics.FillRectangle(brush, rectangle);
            graphics.DrawRectangle(pen, rectangle);
        }
    }

    private static Point[] BoardPoints(bool damaged) {
        if (!damaged) {
            return new[] {
                new Point(116, 236), new Point(236, 116), new Point(788, 116), new Point(908, 236),
                new Point(908, 728), new Point(788, 848), new Point(236, 848), new Point(116, 728),
            };
        }
        return new[] {
            new Point(116, 236), new Point(236, 116), new Point(720, 116), new Point(908, 304),
                new Point(908, 430), new Point(786, 500), new Point(908, 594), new Point(908, 728),
                new Point(788, 848), new Point(236, 848), new Point(116, 728),
        };
    }

    private static void DrawAcceleratorModule(Graphics graphics) {
        FillOutlinedPolygon(graphics, BoardPoints(false), Body, 52f);
        FillOutlinedSquareRect(graphics, new Rectangle(172, 314, 112, 356), Recessed, 32f);
        FillOutlinedSquareRect(graphics, new Rectangle(340, 280, 400, 400), Cyan, 44f);
        for (int index = 0; index < 3; index++) {
            FillOutlinedSquareRect(graphics, new Rectangle(350 + index * 134, 756, 98, 92), Gold, 24f);
        }
    }

    private static void DrawRackI(Graphics graphics) {
        FillOutlinedRect(graphics, new Rectangle(256, 80, 512, 864), Body, 54, 60f);
        FillOutlinedRect(graphics, new Rectangle(350, 176, 324, 590), Recessed, 32, 42f);
        FillOutlinedRect(graphics, new Rectangle(422, 226, 180, 470), Cyan, 20, 34f);
        FillOutlinedRect(graphics, new Rectangle(380, 792, 264, 104), Gold, 16, 28f);
    }

    private static void DrawRackII(Graphics graphics) {
        FillOutlinedRect(graphics, new Rectangle(104, 132, 354, 760), Body, 48, 56f);
        FillOutlinedRect(graphics, new Rectangle(566, 132, 354, 760), Body, 48, 56f);
        FillOutlinedRect(graphics, new Rectangle(202, 236, 158, 456), Cyan, 20, 34f);
        FillOutlinedRect(graphics, new Rectangle(664, 236, 158, 456), Cyan, 20, 34f);
        FillOutlinedRect(graphics, new Rectangle(372, 430, 280, 136), Gold, 18, 34f);
    }

    private static void DrawRackIII(Graphics graphics) {
        var shell = new[] {
            new Point(180, 90), new Point(844, 90), new Point(934, 180), new Point(934, 844),
            new Point(844, 934), new Point(180, 934), new Point(90, 844), new Point(90, 180),
        };
        FillOutlinedPolygon(graphics, shell, Body, 62f);
        FillOutlinedRect(graphics, new Rectangle(192, 192, 276, 276), Cyan, 20, 36f);
        FillOutlinedRect(graphics, new Rectangle(556, 192, 276, 276), Cyan, 20, 36f);
        FillOutlinedRect(graphics, new Rectangle(192, 556, 276, 276), Cyan, 20, 36f);
        FillOutlinedRect(graphics, new Rectangle(556, 556, 276, 276), Cyan, 20, 36f);
        FillOutlinedRect(graphics, new Rectangle(446, 446, 132, 132), Gold, 16, 30f);
    }

    private static void DrawDatasetArchive(Graphics graphics) {
        DrawCartridge(graphics, new Rectangle(118, 538, 788, 246), 330);
        DrawCartridge(graphics, new Rectangle(166, 354, 716, 226), 354);
        DrawCartridge(graphics, new Rectangle(214, 178, 644, 210), 378);
    }

    private static void DrawCartridge(Graphics graphics, Rectangle rectangle, int bandX) {
        FillOutlinedRect(graphics, rectangle, Body, 34, 46f);
        FillOutlinedRect(graphics, new Rectangle(bandX, rectangle.Y + 34, 126, rectangle.Height - 68), Cyan, 16, 28f);
        FillOutlinedRect(graphics, new Rectangle(rectangle.Right - 150, rectangle.Y + rectangle.Height / 2 - 38, 88, 76), Gold, 12, 24f);
    }

    private static void DrawModelArchive(Graphics graphics) {
        var shell = new[] {
            new Point(512, 74), new Point(830, 196), new Point(950, 512), new Point(830, 828),
            new Point(512, 950), new Point(194, 828), new Point(74, 512), new Point(194, 196),
        };
        FillOutlinedPolygon(graphics, shell, Body, 62f);
        FillOutlinedEllipse(graphics, new Rectangle(242, 242, 540, 540), Cyan, 52f);
        FillOutlinedEllipse(graphics, new Rectangle(342, 342, 340, 340), Recessed, 42f);
        FillOutlinedEllipse(graphics, new Rectangle(430, 430, 164, 164), Gold, 30f);
    }

    private static void DrawControlPackage(Graphics graphics) {
        FillOutlinedRect(graphics, new Rectangle(126, 248, 772, 612), Body, 58, 62f);
        FillOutlinedRect(graphics, new Rectangle(340, 140, 344, 170), Recessed, 38, 46f);
        FillOutlinedRect(graphics, new Rectangle(294, 326, 436, 398), Green, 48, 48f);
        using (var pen = new Pen(White, 88f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round }) {
            graphics.DrawLines(pen, new[] {
                new Point(382, 526), new Point(470, 620), new Point(650, 416),
            });
        }
    }

    private static void DrawSpentAccelerator(Graphics graphics) {
        FillOutlinedPolygon(graphics, BoardPoints(true), Spent, 52f);
        FillOutlinedRect(graphics, new Rectangle(172, 314, 112, 356), Recessed, 12, 32f);
        FillOutlinedRect(graphics, new Rectangle(340, 280, 400, 400), Recessed, 18, 44f);
        using (var pen = new Pen(Orange, 72f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round }) {
            graphics.DrawLines(pen, new[] {
                new Point(560, 314), new Point(474, 456), new Point(590, 520), new Point(498, 654),
            });
        }
        FillOutlinedRect(graphics, new Rectangle(374, 756, 112, 92), Gold, 10, 24f);
        FillOutlinedRect(graphics, new Rectangle(548, 756, 112, 92), Recessed, 10, 24f);
    }

    private static ImageAttributes GrayscaleAttributes() {
        var attributes = new ImageAttributes();
        var matrix = new ColorMatrix(new[] {
            new[] { .299f, .299f, .299f, 0f, 0f },
            new[] { .587f, .587f, .587f, 0f, 0f },
            new[] { .114f, .114f, .114f, 0f, 0f },
            new[] { 0f, 0f, 0f, 1f, 0f },
            new[] { 0f, 0f, 0f, 0f, 1f },
        });
        attributes.SetColorMatrix(matrix);
        return attributes;
    }

    private static void DrawProofRow(Graphics graphics, Image image, ImageAttributes attributes, int offsetX, int offsetY) {
        int[] sizes = { 24, 32, 48 };
        for (int index = 0; index < sizes.Length; index++) {
            int size = sizes[index];
            int x = offsetX + 232 + index * 88;
            int y = offsetY + 18;
            var destination = new Rectangle(x, y, size, size);
            if (attributes == null) {
                graphics.DrawImage(image, destination);
            } else {
                graphics.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }
    }

    private static string ToTitle(string name) {
        var text = name.Replace('_', ' ');
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text);
    }
}
'@

$names = [RecursiveIndustryFlatIcons]::GenerateAll($masterDir, $exportDir)
[RecursiveIndustryFlatIcons]::CreateProof(
    $exportDir,
    (Join-Path $proofDir 'all-icons-size-proof.png'),
    $false
)
[RecursiveIndustryFlatIcons]::CreateProof(
    $exportDir,
    (Join-Path $proofDir 'all-icons-grayscale-proof.png'),
    $true
)

Get-ChildItem $masterDir, $exportDir, $proofDir -File |
    Sort-Object DirectoryName, Name |
    Select-Object FullName, Length, @{Name='SHA256';Expression={(Get-FileHash $_.FullName -Algorithm SHA256).Hash}} |
    Format-Table -AutoSize
