[CmdletBinding()]
param(
    [string]$OutputRoot = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $PSScriptRoot '..\art\RecursiveIndustry\UiIcons'
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

public static class RecursiveIndustryUiIconsV3 {
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
    private static readonly Color Blue = ColorTranslator.FromHtml("#4D9DE0");

    public static readonly string[] Names = new[] {
        "experiment_program",
        "validated_research_dossier",
        "frontier_program",
        "frontier_expansion_project",
        "orbital_power_calibration",
        "accelerator_works",
        "curation_office",
        "model_development_center",
        "ai_electronics_cell",
        "electronics_reclaimer",
        "ai_science_institute",
        "pilot_science_complex",
        "systems_integration_complex",
        "autonomous_microchip_complex",
        "autonomous_electronics_integration_complex",
        "autonomous_capital_fabrication_matrix",
        "orbital_mission_complex",
        "frontier_project_complex",
        "recursive_integration_array",
        "autonomous_construction_nexus",
        "ai_operations_i",
        "ai_operations_ii",
        "ai_operations_iii",
        "planetary_coordination_center",
        "orbital_power_relay",
        "autonomous_hauler",
        "autonomous_dump_hauler",
        "autonomous_tank_hauler",
        "autonomous_amphibious_hauler",
        "autonomous_amphibious_excavator",
        "autonomous_mega_excavator",
        "autonomous_large_tree_harvester",
        "autonomous_tree_planter",
        "autonomous_steam_locomotive_i",
        "autonomous_steam_locomotive_ii",
        "autonomous_diesel_locomotive_i",
        "autonomous_diesel_locomotive_ii",
        "autonomous_hydrogen_locomotive_i",
        "autonomous_hydrogen_locomotive_ii",
        "autonomous_steam_tender_i",
        "autonomous_steam_tender_ii",
        "autonomous_electric_locomotive_i",
        "autonomous_electric_locomotive_ii",
        "autonomous_fireless_steam_locomotive",
        "autonomous_turbine_locomotive",
        "autonomous_turbine_tender",
        "autonomous_nuclear_locomotive_cab",
        "autonomous_nuclear_locomotive_reactor",
        "autonomous_nuclear_locomotive_condenser",
        "autonomous_captains_locomotive",
        "fleet_optimization",
        "predictive_maintenance",
        "planetary_extraction",
        "contract_coordination",
        "orbital_lift_coordination",
        "comminution_hub",
        "mineral_products_works",
        "primary_smelter",
        "precision_metals_works",
        "refinery_complex",
        "gas_fertilizer_complex",
        "materials_chemistry_complex",
        "medical_chemistry_complex",
        "food_processing_campus",
        "food_pack_campus",
        "crop_soil_bioprocessing",
        "bioenergy_center",
        "water_utility",
        "thermal_emissions_utility",
        "materials_recovery_center",
        "nuclear_fuel_complex",
        "precision_components_fab",
        "general_manufacturing_fab",
        "orbital_fabrication_fab",
    };

    public static void GenerateAll(string masterDir, string exportDir) {
        Directory.CreateDirectory(masterDir);
        Directory.CreateDirectory(exportDir);
        foreach (string name in Names) {
            using (var master = CreateMaster()) {
                using (var graphics = Graphics.FromImage(master)) {
                    Configure(graphics);
                    DrawIcon(graphics, name);
                }
                master.Save(Path.Combine(masterDir, name + "-master.png"), ImageFormat.Png);
                using (var export = Resize(master, ExportSize)) {
                    export.Save(Path.Combine(exportDir, name + ".png"), ImageFormat.Png);
                }
            }
        }
    }

    public static void CreateProof(string exportDir, string proofPath, bool grayscale) {
        const int rowHeight = 92;
        using (var proof = new Bitmap(1160, Names.Length * rowHeight, PixelFormat.Format32bppArgb))
        using (var graphics = Graphics.FromImage(proof))
        using (var font = new Font("Segoe UI", 10, FontStyle.Bold))
        using (var dark = new SolidBrush(Color.FromArgb(45, 48, 53)))
        using (var gray = grayscale ? GrayscaleAttributes() : null) {
            graphics.Clear(Color.White);
            graphics.FillRectangle(dark, 580, 0, 580, proof.Height);
            for (int row = 0; row < Names.Length; row++) {
                int y = row * rowHeight;
                using (var image = Image.FromFile(Path.Combine(exportDir, Names[row] + ".png"))) {
                    DrawProofRow(graphics, image, gray, 0, y);
                    DrawProofRow(graphics, image, gray, 580, y);
                }
                string title = ToTitle(Names[row]);
                graphics.DrawString(title, font, Brushes.DimGray, 8, y + 62);
                graphics.DrawString(title, font, Brushes.White, 588, y + 62);
            }
            proof.Save(proofPath, ImageFormat.Png);
        }
    }

    private static void DrawIcon(Graphics g, string name) {
        switch (name) {
            case "experiment_program": DrawExperimentProgram(g); break;
            case "validated_research_dossier": DrawValidatedDossier(g); break;
            case "frontier_program": DrawFrontierProgram(g); break;
            case "frontier_expansion_project": DrawExpansionProject(g); break;
            case "orbital_power_calibration": DrawPowerCalibration(g); break;
            case "accelerator_works": DrawAcceleratorWorks(g); break;
            case "curation_office": DrawCurationOffice(g); break;
            case "model_development_center": DrawModelCenter(g); break;
            case "ai_electronics_cell": DrawElectronicsCell(g); break;
            case "electronics_reclaimer": DrawReclaimer(g); break;
            case "ai_science_institute": DrawScienceInstitute(g); break;
            case "pilot_science_complex": DrawPilotComplex(g); break;
            case "systems_integration_complex": DrawSystemsIntegration(g); break;
            case "autonomous_microchip_complex": DrawAutonomousMicrochips(g); break;
            case "autonomous_electronics_integration_complex": DrawElectronicsIntegration(g); break;
            case "autonomous_capital_fabrication_matrix": DrawCapitalFabrication(g); break;
            case "orbital_mission_complex": DrawOrbitalMission(g); break;
            case "frontier_project_complex": DrawFrontierProject(g); break;
            case "recursive_integration_array": DrawRecursiveArray(g); break;
            case "autonomous_construction_nexus": DrawConstructionNexus(g); break;
            case "ai_operations_i": DrawOperations(g, 1); break;
            case "ai_operations_ii": DrawOperations(g, 2); break;
            case "ai_operations_iii": DrawOperations(g, 3); break;
            case "planetary_coordination_center": DrawPlanetaryCenter(g); break;
            case "orbital_power_relay": DrawPowerRelay(g); break;
            case "autonomous_hauler": DrawTruck(g, 0); break;
            case "autonomous_dump_hauler": DrawTruck(g, 1); break;
            case "autonomous_tank_hauler": DrawTruck(g, 2); break;
            case "autonomous_amphibious_hauler": DrawAmphibiousHauler(g); break;
            case "autonomous_amphibious_excavator": DrawExcavator(g, true); break;
            case "autonomous_mega_excavator": DrawExcavator(g, false); break;
            case "autonomous_large_tree_harvester": DrawTreeHarvester(g); break;
            case "autonomous_tree_planter": DrawTreePlanter(g); break;
            case "autonomous_steam_locomotive_i": DrawLocomotive(g, 0, 1); break;
            case "autonomous_steam_locomotive_ii": DrawLocomotive(g, 0, 2); break;
            case "autonomous_diesel_locomotive_i": DrawLocomotive(g, 1, 1); break;
            case "autonomous_diesel_locomotive_ii": DrawLocomotive(g, 1, 2); break;
            case "autonomous_hydrogen_locomotive_i": DrawLocomotive(g, 2, 1); break;
            case "autonomous_hydrogen_locomotive_ii": DrawLocomotive(g, 2, 2); break;
            case "autonomous_steam_tender_i": DrawSteamTender(g, 1); break;
            case "autonomous_steam_tender_ii": DrawSteamTender(g, 2); break;
            case "autonomous_electric_locomotive_i": DrawLocomotive(g, 3, 1); break;
            case "autonomous_electric_locomotive_ii": DrawLocomotive(g, 3, 2); break;
            case "autonomous_fireless_steam_locomotive": DrawLocomotive(g, 4, 1); break;
            case "autonomous_turbine_locomotive": DrawLocomotive(g, 5, 2); break;
            case "autonomous_turbine_tender": DrawTurbineTender(g); break;
            case "autonomous_nuclear_locomotive_cab": DrawNuclearModule(g, 0); break;
            case "autonomous_nuclear_locomotive_reactor": DrawNuclearModule(g, 1); break;
            case "autonomous_nuclear_locomotive_condenser": DrawNuclearModule(g, 2); break;
            case "autonomous_captains_locomotive": DrawLocomotive(g, 6, 1); break;
            case "fleet_optimization": DrawFleetOptimization(g); break;
            case "predictive_maintenance": DrawPredictiveMaintenance(g); break;
            case "planetary_extraction": DrawPlanetaryExtraction(g); break;
            case "contract_coordination": DrawContractCoordination(g); break;
            case "orbital_lift_coordination": DrawOrbitalLift(g); break;
            case "comminution_hub": DrawMegafacility(g, 0); break;
            case "mineral_products_works": DrawMegafacility(g, 1); break;
            case "primary_smelter": DrawMegafacility(g, 2); break;
            case "precision_metals_works": DrawMegafacility(g, 3); break;
            case "refinery_complex": DrawMegafacility(g, 4); break;
            case "gas_fertilizer_complex": DrawMegafacility(g, 5); break;
            case "materials_chemistry_complex": DrawMegafacility(g, 6); break;
            case "medical_chemistry_complex": DrawMegafacility(g, 7); break;
            case "food_processing_campus": DrawMegafacility(g, 8); break;
            case "food_pack_campus": DrawMegafacility(g, 9); break;
            case "crop_soil_bioprocessing": DrawMegafacility(g, 10); break;
            case "bioenergy_center": DrawMegafacility(g, 11); break;
            case "water_utility": DrawMegafacility(g, 12); break;
            case "thermal_emissions_utility": DrawMegafacility(g, 13); break;
            case "materials_recovery_center": DrawMegafacility(g, 14); break;
            case "nuclear_fuel_complex": DrawMegafacility(g, 15); break;
            case "precision_components_fab": DrawMegafacility(g, 16); break;
            case "general_manufacturing_fab": DrawMegafacility(g, 17); break;
            case "orbital_fabrication_fab": DrawMegafacility(g, 18); break;
            default: throw new InvalidOperationException(name);
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

    private static void Rect(Graphics g, int x, int y, int w, int h, Color fill, int radius = 24, float stroke = 48f) {
        using (var path = RoundedRect(new Rectangle(x, y, w, h), radius))
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, stroke) { LineJoin = LineJoin.Round }) {
            g.FillPath(brush, path);
            g.DrawPath(pen, path);
        }
    }

    private static void Ellipse(Graphics g, int x, int y, int w, int h, Color fill, float stroke = 48f) {
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, stroke)) {
            g.FillEllipse(brush, x, y, w, h);
            g.DrawEllipse(pen, x, y, w, h);
        }
    }

    private static void Polygon(Graphics g, Point[] points, Color fill, float stroke = 48f) {
        using (var brush = new SolidBrush(fill))
        using (var pen = new Pen(Outline, stroke) { LineJoin = LineJoin.Round }) {
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }
    }

    private static void Line(Graphics g, Color color, float width, params Point[] points) {
        using (var pen = new Pen(color, width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round }) {
            g.DrawLines(pen, points);
        }
    }

    private static void Arrow(Graphics g, Point from, Point to, Color color, float width = 54f) {
        Line(g, color, width, from, to);
        double angle = Math.Atan2(to.Y - from.Y, to.X - from.X);
        int size = 96;
        Point left = new Point(
            to.X - (int)(Math.Cos(angle - Math.PI / 5) * size),
            to.Y - (int)(Math.Sin(angle - Math.PI / 5) * size));
        Point right = new Point(
            to.X - (int)(Math.Cos(angle + Math.PI / 5) * size),
            to.Y - (int)(Math.Sin(angle + Math.PI / 5) * size));
        Polygon(g, new[] { to, left, right }, color, 18f);
    }

    private static Point[] Hex(int cx, int cy, int radius) {
        var points = new Point[6];
        for (int index = 0; index < 6; index++) {
            double angle = Math.PI / 3 * index - Math.PI / 2;
            points[index] = new Point(cx + (int)(radius * Math.Cos(angle)), cy + (int)(radius * Math.Sin(angle)));
        }
        return points;
    }

    private static void Check(Graphics g, int x, int y, int scale) {
        Line(g, White, 54f, new Point(x, y), new Point(x + scale / 3, y + scale / 3), new Point(x + scale, y - scale / 2));
    }

    private static void Document(Graphics g, Color accent) {
        Polygon(g, new[] {
            new Point(246, 112), new Point(650, 112), new Point(806, 268),
            new Point(806, 896), new Point(246, 896)
        }, Body, 54f);
        Polygon(g, new[] { new Point(650, 112), new Point(650, 268), new Point(806, 268) }, Recessed, 34f);
        Line(g, accent, 44f, new Point(342, 410), new Point(710, 410));
        Line(g, accent, 44f, new Point(342, 538), new Point(650, 538));
    }

    private static void MachineShell(Graphics g) {
        Rect(g, 126, 196, 772, 650, Body, 54, 60f);
        Rect(g, 220, 284, 584, 390, Recessed, 34, 42f);
        Rect(g, 184, 766, 656, 106, Gold, 18, 30f);
    }

    private static void DrawExperimentProgram(Graphics g) {
        Document(g, Cyan);
        Rect(g, 396, 580, 216, 188, Cyan, 24, 38f);
        Line(g, White, 38f, new Point(452, 628), new Point(512, 694), new Point(576, 614));
        Ellipse(g, 162, 318, 92, 92, Gold, 26f);
        Ellipse(g, 770, 424, 92, 92, Gold, 26f);
    }

    private static void DrawValidatedDossier(Graphics g) {
        Document(g, Cyan);
        Ellipse(g, 390, 564, 244, 244, Green, 42f);
        Check(g, 444, 692, 146);
    }

    private static void DrawFrontierProgram(Graphics g) {
        Polygon(g, Hex(512, 512, 390), Body, 58f);
        Ellipse(g, 324, 324, 376, 376, Recessed, 42f);
        Arrow(g, new Point(402, 624), new Point(650, 374), Cyan, 62f);
        Ellipse(g, 448, 448, 128, 128, Gold, 30f);
    }

    private static void DrawExpansionProject(Graphics g) {
        Polygon(g, Hex(512, 512, 230), Cyan, 54f);
        Rect(g, 410, 410, 204, 204, Gold, 26, 34f);
        Arrow(g, new Point(512, 252), new Point(512, 88), White, 48f);
        Arrow(g, new Point(772, 512), new Point(936, 512), White, 48f);
        Arrow(g, new Point(512, 772), new Point(512, 936), White, 48f);
        Arrow(g, new Point(252, 512), new Point(88, 512), White, 48f);
    }

    private static void DrawPowerCalibration(Graphics g) {
        Ellipse(g, 160, 160, 704, 704, Recessed, 58f);
        Ellipse(g, 310, 310, 404, 404, Cyan, 42f);
        Ellipse(g, 438, 438, 148, 148, Gold, 30f);
        Line(g, White, 44f, new Point(512, 78), new Point(512, 280));
        Line(g, White, 44f, new Point(512, 744), new Point(512, 946));
        Line(g, White, 44f, new Point(78, 512), new Point(280, 512));
        Line(g, White, 44f, new Point(744, 512), new Point(946, 512));
    }

    private static void DrawAcceleratorWorks(Graphics g) {
        MachineShell(g);
        Rect(g, 350, 330, 324, 256, Cyan, 28, 40f);
        Line(g, White, 44f, new Point(404, 458), new Point(620, 458));
        Line(g, Gold, 42f, new Point(370, 690), new Point(254, 574), new Point(322, 506));
    }

    private static void DrawCurationOffice(Graphics g) {
        MachineShell(g);
        Polygon(g, new[] { new Point(330, 326), new Point(694, 326), new Point(592, 510), new Point(592, 610), new Point(432, 610), new Point(432, 510) }, Cyan, 38f);
        Line(g, Gold, 36f, new Point(286, 706), new Point(738, 706));
    }

    private static void DrawModelCenter(Graphics g) {
        MachineShell(g);
        Ellipse(g, 368, 334, 288, 288, Cyan, 42f);
        Ellipse(g, 448, 414, 128, 128, Gold, 28f);
        Line(g, White, 32f, new Point(512, 334), new Point(512, 622));
        Line(g, White, 32f, new Point(368, 478), new Point(656, 478));
    }

    private static void DrawElectronicsCell(Graphics g) {
        MachineShell(g);
        Rect(g, 352, 326, 320, 320, Cyan, 28, 42f);
        for (int i = 0; i < 4; i++) {
            Line(g, Gold, 28f, new Point(304, 370 + i * 76), new Point(352, 370 + i * 76));
            Line(g, Gold, 28f, new Point(672, 370 + i * 76), new Point(720, 370 + i * 76));
        }
        Check(g, 418, 516, 140);
    }

    private static void DrawReclaimer(Graphics g) {
        MachineShell(g);
        Rect(g, 380, 348, 264, 264, Recessed, 24, 38f);
        Line(g, Orange, 48f, new Point(438, 396), new Point(566, 560));
        Arrow(g, new Point(306, 576), new Point(388, 340), Cyan, 44f);
        Arrow(g, new Point(716, 380), new Point(636, 626), Cyan, 44f);
    }

    private static void DrawScienceInstitute(Graphics g) {
        MachineShell(g);
        Ellipse(g, 348, 332, 328, 280, Recessed, 34f);
        using (var pen = new Pen(Cyan, 32f)) {
            g.DrawEllipse(pen, 364, 388, 296, 120);
            g.DrawEllipse(pen, 452, 310, 120, 296);
        }
        Ellipse(g, 468, 426, 88, 88, Gold, 22f);
    }

    private static void DrawPilotComplex(Graphics g) {
        MachineShell(g);
        Polygon(g, new[] { new Point(438, 320), new Point(586, 320), new Point(566, 438), new Point(686, 626), new Point(338, 626), new Point(458, 438) }, Cyan, 42f);
        Line(g, White, 34f, new Point(408, 544), new Point(616, 544));
        Check(g, 632, 350, 118);
    }

    private static void DrawSystemsIntegration(Graphics g) {
        MachineShell(g);
        Ellipse(g, 432, 402, 160, 160, Gold, 34f);
        Arrow(g, new Point(258, 350), new Point(420, 434), Cyan, 38f);
        Arrow(g, new Point(258, 614), new Point(420, 530), Green, 38f);
        Arrow(g, new Point(766, 350), new Point(604, 434), Blue, 38f);
        Arrow(g, new Point(604, 530), new Point(766, 614), White, 38f);
    }

    private static void DrawAutonomousMicrochips(Graphics g) {
        MachineShell(g);
        Rect(g, 360, 344, 304, 256, Cyan, 24, 40f);
        Ellipse(g, 300, 284, 424, 376, Color.FromArgb(0, 0, 0, 0), 38f);
        Arrow(g, new Point(680, 334), new Point(736, 456), Green, 36f);
        for (int i = 0; i < 3; i++) {
            Line(g, Gold, 26f, new Point(408 + i * 104, 600), new Point(408 + i * 104, 654));
        }
    }

    private static void DrawElectronicsIntegration(Graphics g) {
        MachineShell(g);
        Rect(g, 420, 352, 232, 232, Cyan, 26, 38f);
        Rect(g, 470, 402, 132, 132, Recessed, 18, 28f);
        Point[] inputs = { new Point(280, 356), new Point(280, 476), new Point(280, 596) };
        foreach (Point input in inputs) {
            Rect(g, input.X - 38, input.Y - 38, 76, 76, Gold, 12, 22f);
            Arrow(g, new Point(input.X + 44, input.Y), new Point(408, 468), White, 24f);
        }
        Arrow(g, new Point(664, 468), new Point(756, 468), Green, 38f);
        for (int i = 0; i < 3; i++) {
            Line(g, Gold, 22f, new Point(464 + i * 72, 584), new Point(464 + i * 72, 636));
        }
    }

    private static void DrawCapitalFabrication(Graphics g) {
        MachineShell(g);
        Rect(g, 252, 542, 150, 128, Cyan, 18, 30f);
        Rect(g, 436, 438, 150, 232, Gold, 18, 30f);
        Rect(g, 620, 334, 150, 336, Green, 18, 30f);
        Arrow(g, new Point(294, 730), new Point(706, 730), White, 42f);
        Line(g, Orange, 30f, new Point(326, 542), new Point(510, 438), new Point(694, 334));
    }

    private static void DrawOrbitalMission(Graphics g) {
        MachineShell(g);
        Polygon(g, new[] { new Point(512, 274), new Point(610, 466), new Point(570, 634), new Point(454, 634), new Point(414, 466) }, White, 40f);
        Polygon(g, new[] { new Point(414, 500), new Point(330, 622), new Point(448, 594) }, Orange, 28f);
        Polygon(g, new[] { new Point(610, 500), new Point(694, 622), new Point(576, 594) }, Orange, 28f);
        using (var pen = new Pen(Cyan, 34f)) { g.DrawArc(pen, 266, 260, 492, 410, 210, 250); }
    }

    private static void DrawFrontierProject(Graphics g) {
        MachineShell(g);
        Polygon(g, Hex(512, 476, 174), Cyan, 38f);
        Line(g, Gold, 44f, new Point(708, 316), new Point(708, 606), new Point(618, 666));
        Ellipse(g, 584, 640, 70, 70, Orange, 22f);
    }

    private static void DrawRecursiveArray(Graphics g) {
        MachineShell(g);
        Ellipse(g, 434, 404, 156, 156, Gold, 30f);
        int[,] nodes = { { 300, 316 }, { 644, 316 }, { 300, 584 }, { 644, 584 } };
        for (int i = 0; i < 4; i++) Ellipse(g, nodes[i,0], nodes[i,1], 80, 80, Cyan, 22f);
        Arrow(g, new Point(380, 356), new Point(438, 426), White, 28f);
        Arrow(g, new Point(644, 356), new Point(586, 426), White, 28f);
        Arrow(g, new Point(438, 538), new Point(380, 624), White, 28f);
        Arrow(g, new Point(586, 538), new Point(644, 624), White, 28f);
    }

    private static void DrawConstructionNexus(Graphics g) {
        MachineShell(g);
        Rect(g, 286, 420, 150, 150, Cyan, 20, 30f);
        Rect(g, 454, 338, 150, 232, Gold, 20, 30f);
        Rect(g, 622, 394, 150, 176, Green, 20, 30f);
        Arrow(g, new Point(310, 664), new Point(706, 664), White, 40f);
    }

    private static void DrawOperations(Graphics g, int tier) {
        Rect(g, 154, 156, 716, 714, Body, 52, 58f);
        Rect(g, 246, 252, 532, 390, Recessed, 32, 40f);
        int count = tier == 1 ? 1 : (tier == 2 ? 2 : 4);
        Point[] positions = { new Point(512, 446), new Point(392, 446), new Point(632, 446), new Point(392, 554) };
        if (count == 4) positions[0] = new Point(632, 554);
        for (int i = 0; i < count; i++) Ellipse(g, positions[i].X - 54, positions[i].Y - 54, 108, 108, i == 0 ? Gold : Cyan, 24f);
        if (count > 1) Line(g, White, 28f, new Point(392, 446), new Point(632, 446));
        if (count > 2) Line(g, White, 28f, new Point(392, 446), new Point(392, 554), new Point(632, 554), new Point(632, 446));
        Rect(g, 294, 706, 436, 100, tier == 1 ? Cyan : (tier == 2 ? Gold : Green), 16, 28f);
    }

    private static void DrawPlanetaryCenter(Graphics g) {
        Rect(g, 126, 170, 772, 700, Body, 54, 58f);
        Ellipse(g, 332, 282, 360, 360, Blue, 44f);
        Line(g, White, 30f, new Point(512, 300), new Point(512, 624));
        using (var pen = new Pen(White, 30f)) { g.DrawEllipse(pen, 382, 282, 260, 360); g.DrawEllipse(pen, 332, 402, 360, 120); }
        Point[] hubs = { new Point(246, 344), new Point(778, 344), new Point(512, 742) };
        foreach (Point hub in hubs) Ellipse(g, hub.X - 42, hub.Y - 42, 84, 84, Gold, 22f);
        Line(g, Cyan, 26f, hubs[0], new Point(360, 410));
        Line(g, Cyan, 26f, hubs[1], new Point(664, 410));
        Line(g, Cyan, 26f, hubs[2], new Point(512, 642));
    }

    private static void DrawPowerRelay(Graphics g) {
        Rect(g, 156, 650, 712, 208, Body, 36, 52f);
        using (var pen = new Pen(Cyan, 58f)) { g.DrawArc(pen, 252, 274, 520, 420, 198, 144); }
        Line(g, Outline, 58f, new Point(512, 558), new Point(512, 690));
        Arrow(g, new Point(812, 116), new Point(610, 376), Gold, 54f);
        Polygon(g, new[] { new Point(492, 506), new Point(584, 506), new Point(526, 616), new Point(608, 616), new Point(438, 806), new Point(486, 650), new Point(414, 650) }, White, 26f);
    }

    private static void DrawTruck(Graphics g, int kind) {
        Rect(g, 138, 468, 736, 272, Body, 42, 52f);
        Rect(g, 650, 382, 224, 214, Cyan, 30, 42f);
        if (kind == 0) {
            Rect(g, 208, 362, 380, 178, Recessed, 24, 38f);
            Rect(g, 250, 398, 116, 100, Gold, 14, 22f);
            Rect(g, 402, 398, 116, 100, Green, 14, 22f);
        } else if (kind == 1) {
            Polygon(g, new[] { new Point(190, 344), new Point(590, 392), new Point(540, 568), new Point(238, 568) }, Orange, 42f);
            Polygon(g, new[] { new Point(276, 438), new Point(506, 438), new Point(472, 522), new Point(304, 522) }, Gold, 22f);
        } else {
            Rect(g, 190, 334, 414, 236, Blue, 112, 42f);
            Ellipse(g, 228, 378, 76, 112, White, 22f);
        }
        Ellipse(g, 230, 650, 164, 164, Recessed, 40f);
        Ellipse(g, 636, 650, 164, 164, Recessed, 40f);
        Ellipse(g, 286, 706, 52, 52, Cyan, 16f);
        Ellipse(g, 692, 706, 52, 52, Cyan, 16f);
    }

    private static void DrawTrackedBase(Graphics g, int width, Color accent) {
        int x = (1024 - width) / 2;
        Rect(g, x, 586, width, 210, Body, 72, 52f);
        Rect(g, x + 44, 646, width - 88, 106, Recessed, 42, 30f);
        Ellipse(g, x + 88, 660, 82, 82, accent, 22f);
        Ellipse(g, x + width - 170, 660, 82, 82, accent, 22f);
    }

    private static void DrawAmphibiousHauler(Graphics g) {
        Polygon(g, new[] {
            new Point(128, 496), new Point(842, 496), new Point(902, 650),
            new Point(790, 746), new Point(238, 746), new Point(118, 650)
        }, Body, 52f);
        Rect(g, 612, 354, 230, 204, Cyan, 32, 40f);
        Rect(g, 228, 382, 330, 160, Recessed, 24, 38f);
        Rect(g, 278, 416, 104, 86, Gold, 14, 22f);
        Rect(g, 410, 416, 104, 86, Green, 14, 22f);
        Line(g, Blue, 40f, new Point(116, 818), new Point(270, 774), new Point(424, 818), new Point(578, 774), new Point(732, 818), new Point(886, 774));
        Ellipse(g, 658, 404, 74, 74, White, 20f);
    }

    private static void DrawExcavator(Graphics g, bool amphibious) {
        DrawTrackedBase(g, amphibious ? 760 : 850, amphibious ? Blue : Cyan);
        Rect(g, 248, 400, 304, 230, amphibious ? Blue : Cyan, 34, 42f);
        Ellipse(g, 332, 456, 116, 116, Gold, 28f);
        Line(g, Gold, 68f, new Point(498, 446), new Point(650, 292), new Point(796, 456));
        Polygon(g, new[] {
            new Point(760, 430), new Point(900, 476),
            new Point(840, 586), new Point(718, 530)
        }, Orange, 36f);
        Check(g, 288, 344, 122);
        if (amphibious) {
            Line(g, Blue, 36f, new Point(110, 866), new Point(268, 824), new Point(426, 866), new Point(584, 824), new Point(742, 866), new Point(900, 824));
        } else {
            Rect(g, 146, 520, 166, 98, Recessed, 18, 26f);
        }
    }

    private static void DrawTreeHarvester(Graphics g) {
        DrawTrackedBase(g, 820, Cyan);
        Rect(g, 184, 406, 294, 216, Cyan, 32, 40f);
        Line(g, Gold, 58f, new Point(446, 438), new Point(622, 304), new Point(702, 474));
        Line(g, White, 34f, new Point(688, 430), new Point(808, 548));
        Line(g, Gold, 48f, new Point(784, 248), new Point(784, 620));
        Ellipse(g, 690, 130, 188, 188, Green, 36f);
        Check(g, 248, 366, 112);
    }

    private static void DrawTreePlanter(Graphics g) {
        DrawTrackedBase(g, 780, Cyan);
        Rect(g, 214, 424, 596, 196, Body, 34, 42f);
        Rect(g, 266, 464, 180, 118, Cyan, 22, 30f);
        for (int index = 0; index < 3; index++) {
            int x = 534 + index * 96;
            Line(g, Gold, 28f, new Point(x, 566), new Point(x, 400));
            Polygon(g, new[] {
                new Point(x, 270), new Point(x - 66, 430), new Point(x + 66, 430)
            }, Green, 28f);
        }
        Arrow(g, new Point(512, 810), new Point(512, 684), White, 34f);
    }

    private static void DrawLocomotive(Graphics g, int fuelKind, int tier) {
        int bodyX = tier == 1 ? 132 : 92;
        int bodyWidth = tier == 1 ? 760 : 840;
        Color accent = fuelKind == 0 ? Gold
            : fuelKind == 1 ? Orange
            : fuelKind == 2 ? Blue
            : fuelKind == 3 ? White
            : fuelKind == 4 ? Cyan
            : fuelKind == 5 ? Orange
            : Gold;
        Rect(g, bodyX, 420, bodyWidth, 292, Body, 38, 52f);
        Rect(g, bodyX + bodyWidth - 250, 286, 218, 250, Cyan, 28, 38f);
        Rect(g, bodyX + 84, 470, tier == 1 ? 300 : 390, 128, accent, 20, 30f);
        if (tier == 2) {
            Line(g, White, 30f, new Point(bodyX + 106, 634), new Point(bodyX + bodyWidth - 292, 634));
        }
        Ellipse(g, bodyX + 116, 640, 154, 154, Recessed, 38f);
        Ellipse(g, bodyX + bodyWidth - 286, 640, 154, 154, Recessed, 38f);
        Ellipse(g, bodyX + 166, 690, 54, 54, Cyan, 16f);
        Ellipse(g, bodyX + bodyWidth - 236, 690, 54, 54, Cyan, 16f);

        if (fuelKind == 0) {
            Rect(g, bodyX + 156, 282, 100, 210, Recessed, 20, 30f);
            Ellipse(g, bodyX + 126, 222, 160, 94, White, 24f);
            Ellipse(g, bodyX + 236, 158, 126, 92, White, 22f);
        } else if (fuelKind == 1) {
            Polygon(g, new[] {
                new Point(bodyX + 214, 210), new Point(bodyX + 300, 366),
                new Point(bodyX + 214, 438), new Point(bodyX + 128, 366)
            }, Orange, 32f);
        } else if (fuelKind == 2) {
            Ellipse(g, bodyX + 126, 226, 126, 126, Blue, 28f);
            Ellipse(g, bodyX + 286, 178, 126, 126, Cyan, 28f);
            Line(g, White, 28f, new Point(bodyX + 238, 268), new Point(bodyX + 298, 236));
        } else if (fuelKind == 3) {
            Line(g, White, 32f, new Point(bodyX + 92, 232), new Point(bodyX + 430, 232));
            Line(g, Cyan, 28f, new Point(bodyX + 216, 232), new Point(bodyX + 216, 390));
            Polygon(g, new[] {
                new Point(bodyX + 332, 170), new Point(bodyX + 242, 316),
                new Point(bodyX + 318, 316), new Point(bodyX + 244, 450),
                new Point(bodyX + 414, 272), new Point(bodyX + 334, 272)
            }, White, 24f);
        } else if (fuelKind == 4) {
            Ellipse(g, bodyX + 120, 248, 302, 164, Cyan, 32f);
            Ellipse(g, bodyX + 154, 178, 112, 86, White, 20f);
            Ellipse(g, bodyX + 274, 132, 142, 108, White, 22f);
        } else if (fuelKind == 5) {
            Ellipse(g, bodyX + 154, 188, 250, 250, Orange, 34f);
            Ellipse(g, bodyX + 234, 268, 90, 90, Recessed, 22f);
            Line(g, White, 24f, new Point(bodyX + 279, 206), new Point(bodyX + 279, 268));
            Line(g, White, 24f, new Point(bodyX + 384, 313), new Point(bodyX + 324, 313));
            Line(g, White, 24f, new Point(bodyX + 279, 418), new Point(bodyX + 279, 358));
            Line(g, White, 24f, new Point(bodyX + 174, 313), new Point(bodyX + 234, 313));
        } else {
            Polygon(g, new[] {
                new Point(bodyX + 120, 332), new Point(bodyX + 166, 202),
                new Point(bodyX + 238, 286), new Point(bodyX + 310, 182),
                new Point(bodyX + 388, 332)
            }, Gold, 30f);
            Ellipse(g, bodyX + 214, 330, 88, 88, White, 20f);
        }
        Check(g, bodyX + bodyWidth - 208, 426, 112);
    }

    private static void DrawSteamTender(Graphics g, int tier) {
        int x = tier == 1 ? 156 : 112;
        int width = tier == 1 ? 712 : 800;
        Rect(g, x, 410, width, 302, Body, 38, 52f);
        Polygon(g, new[] {
            new Point(x + 72, 430), new Point(x + width - 72, 430),
            new Point(x + width - 142, 590), new Point(x + 142, 590)
        }, Gold, 34f);
        int markerCount = tier == 1 ? 3 : 5;
        for (int index = 0; index < markerCount; index++) {
            Ellipse(g, x + 142 + index * ((width - 284) / markerCount), 466, 74, 74,
                index % 2 == 0 ? Gold : Cyan, 18f);
        }
        Ellipse(g, x + 112, 646, 146, 146, Cyan, 36f);
        Ellipse(g, x + width - 258, 646, 146, 146, Cyan, 36f);
        Line(g, White, 40f, new Point(x - 64, 564), new Point(x + 38, 564));
        Check(g, x + width - 208, 330, 112);
    }

    private static void DrawTurbineTender(Graphics g) {
        Rect(g, 112, 410, 800, 302, Body, 38, 52f);
        Rect(g, 190, 450, 560, 140, Orange, 66, 34f);
        Ellipse(g, 690, 424, 164, 164, Gold, 30f);
        Ellipse(g, 738, 472, 68, 68, Recessed, 18f);
        Ellipse(g, 224, 646, 146, 146, Cyan, 36f);
        Ellipse(g, 654, 646, 146, 146, Cyan, 36f);
        Line(g, White, 40f, new Point(48, 564), new Point(150, 564));
        Check(g, 704, 326, 112);
    }

    private static void DrawNuclearModule(Graphics g, int module) {
        Rect(g, 92, 410, 840, 302, Body, 38, 52f);
        Color accent = module == 0 ? Cyan : module == 1 ? Green : Blue;
        Rect(g, 162, 456, 600, 142, accent, 24, 34f);
        Ellipse(g, 176, 646, 146, 146, Recessed, 36f);
        Ellipse(g, 702, 646, 146, 146, Recessed, 36f);

        if (module == 0) {
            Rect(g, 662, 272, 208, 256, Cyan, 28, 38f);
            Check(g, 708, 408, 112);
            Arrow(g, new Point(228, 300), new Point(448, 300), White, 38f);
        } else if (module == 1) {
            Ellipse(g, 350, 166, 324, 324, Green, 42f);
            Ellipse(g, 448, 264, 128, 128, Gold, 28f);
            Line(g, White, 30f, new Point(512, 184), new Point(512, 264));
            Line(g, White, 30f, new Point(654, 328), new Point(576, 328));
            Line(g, White, 30f, new Point(512, 470), new Point(512, 392));
            Line(g, White, 30f, new Point(370, 328), new Point(448, 328));
        } else {
            for (int index = 0; index < 5; index++) {
                Line(g, Blue, 34f,
                    new Point(306 + index * 102, 194),
                    new Point(306 + index * 102, 454));
            }
            Line(g, White, 30f, new Point(244, 250), new Point(780, 250));
            Line(g, White, 30f, new Point(244, 398), new Point(780, 398));
        }
    }

    private static void DrawFleetOptimization(Graphics g) {
        Ellipse(g, 132, 388, 148, 148, Cyan, 34f);
        Ellipse(g, 438, 184, 148, 148, Gold, 34f);
        Ellipse(g, 744, 388, 148, 148, Green, 34f);
        Line(g, White, 42f, new Point(268, 424), new Point(452, 280), new Point(758, 424));
        Arrow(g, new Point(224, 688), new Point(800, 688), Cyan, 48f);
    }

    private static void DrawPredictiveMaintenance(Graphics g) {
        Polygon(g, new[] { new Point(174, 248), new Point(286, 136), new Point(454, 304), new Point(382, 376), new Point(806, 800), new Point(686, 920), new Point(262, 496), new Point(190, 568), new Point(78, 456), new Point(246, 288) }, Body, 48f);
        Line(g, Cyan, 46f, new Point(422, 620), new Point(502, 620), new Point(546, 518), new Point(604, 726), new Point(656, 620), new Point(826, 620));
        Check(g, 642, 316, 142);
    }

    private static void DrawPlanetaryExtraction(Graphics g) {
        Ellipse(g, 180, 146, 560, 560, Blue, 52f);
        using (var pen = new Pen(White, 28f)) { g.DrawEllipse(pen, 310, 146, 300, 560); g.DrawEllipse(pen, 180, 340, 560, 150); }
        Polygon(g, new[] { new Point(650, 562), new Point(846, 562), new Point(902, 730), new Point(594, 730) }, Body, 42f);
        Arrow(g, new Point(748, 848), new Point(748, 640), Orange, 50f);
    }

    private static void DrawContractCoordination(Graphics g) {
        Document(g, Cyan);
        Line(g, Gold, 44f, new Point(512, 316), new Point(512, 700));
        Line(g, Gold, 44f, new Point(348, 420), new Point(676, 420));
        Line(g, White, 34f, new Point(348, 420), new Point(286, 572));
        Line(g, White, 34f, new Point(676, 420), new Point(738, 572));
        Polygon(g, new[] { new Point(206, 572), new Point(366, 572), new Point(330, 668), new Point(242, 668) }, Green, 26f);
        Polygon(g, new[] { new Point(658, 572), new Point(818, 572), new Point(782, 668), new Point(694, 668) }, Green, 26f);
    }

    private static void DrawOrbitalLift(Graphics g) {
        Polygon(g, new[] { new Point(512, 104), new Point(618, 316), new Point(582, 586), new Point(442, 586), new Point(406, 316) }, White, 44f);
        Polygon(g, new[] { new Point(406, 430), new Point(296, 610), new Point(448, 562) }, Orange, 30f);
        Polygon(g, new[] { new Point(618, 430), new Point(728, 610), new Point(576, 562) }, Orange, 30f);
        Arrow(g, new Point(382, 892), new Point(382, 676), Cyan, 42f);
        Arrow(g, new Point(642, 892), new Point(642, 676), Cyan, 42f);
        using (var pen = new Pen(Gold, 34f)) { g.DrawArc(pen, 154, 130, 716, 716, 200, 220); }
    }

    private static void DrawMegafacility(Graphics g, int kind) {
        Color[] accents = { Cyan, Gold, Orange, Blue, Cyan, Green, Blue, White, Gold, Orange, Green, Cyan, Blue, Orange, Green, Gold, Cyan, White, Blue };
        Color accent = accents[kind];
        Rect(g, 176, 344, 672, 456, Body, 34, 46f);
        Rect(g, 240, 260, 544, 150, Recessed, 28, 40f);
        Line(g, accent, 34f, new Point(240, 742), new Point(784, 742));
        Ellipse(g, 246, 676, 104, 104, accent, 26f);
        Ellipse(g, 674, 676, 104, 104, accent, 26f);

        switch (kind) {
            case 0:
                Polygon(g, new[] { new Point(286, 432), new Point(486, 520), new Point(286, 608) }, Gold, 30f);
                Polygon(g, new[] { new Point(738, 432), new Point(538, 520), new Point(738, 608) }, Cyan, 30f);
                break;
            case 1:
                Ellipse(g, 334, 404, 356, 232, Orange, 34f);
                Rect(g, 430, 464, 164, 112, Gold, 18, 24f);
                break;
            case 2:
                Polygon(g, new[] { new Point(512, 382), new Point(678, 636), new Point(346, 636) }, Orange, 34f);
                Polygon(g, new[] { new Point(512, 464), new Point(584, 600), new Point(440, 600) }, Gold, 24f);
                break;
            case 3:
                Polygon(g, new[] { new Point(512, 384), new Point(680, 502), new Point(612, 650), new Point(412, 650), new Point(344, 502) }, Cyan, 34f);
                Line(g, White, 24f, new Point(344, 502), new Point(680, 502), new Point(512, 650), new Point(344, 502));
                break;
            case 4:
                Rect(g, 300, 414, 112, 238, Blue, 24, 30f);
                Rect(g, 456, 374, 112, 278, Cyan, 24, 30f);
                Rect(g, 612, 446, 112, 206, Gold, 24, 30f);
                break;
            case 5:
                Ellipse(g, 302, 438, 166, 166, Cyan, 30f);
                Ellipse(g, 454, 388, 166, 166, Green, 30f);
                Ellipse(g, 574, 470, 166, 166, Gold, 30f);
                break;
            case 6:
                Polygon(g, new[] { new Point(438, 392), new Point(586, 392), new Point(570, 480), new Point(690, 642), new Point(334, 642), new Point(454, 480) }, Blue, 34f);
                Line(g, Cyan, 30f, new Point(384, 586), new Point(640, 586));
                break;
            case 7:
                Rect(g, 446, 382, 132, 276, White, 20, 24f);
                Rect(g, 374, 454, 276, 132, White, 20, 24f);
                break;
            case 8:
                Ellipse(g, 370, 382, 284, 284, Gold, 34f);
                Line(g, Green, 30f, new Point(512, 430), new Point(512, 620));
                Line(g, Green, 24f, new Point(512, 486), new Point(448, 450));
                Line(g, Green, 24f, new Point(512, 538), new Point(576, 502));
                break;
            case 9:
                Rect(g, 346, 394, 332, 250, Orange, 18, 34f);
                Line(g, White, 28f, new Point(346, 474), new Point(512, 558), new Point(678, 474));
                Line(g, White, 28f, new Point(512, 558), new Point(512, 644));
                break;
            case 10:
                Polygon(g, new[] { new Point(512, 382), new Point(678, 512), new Point(512, 658), new Point(346, 512) }, Green, 34f);
                Line(g, White, 26f, new Point(512, 424), new Point(512, 624));
                break;
            case 11:
                Ellipse(g, 334, 414, 210, 210, Cyan, 32f);
                Polygon(g, new[] { new Point(602, 382), new Point(716, 612), new Point(510, 590) }, Orange, 30f);
                break;
            case 12:
                Polygon(g, new[] { new Point(512, 370), new Point(680, 570), new Point(624, 654), new Point(400, 654), new Point(344, 570) }, Blue, 34f);
                Line(g, White, 26f, new Point(408, 574), new Point(616, 574));
                break;
            case 13:
                Line(g, Orange, 40f, new Point(348, 620), new Point(348, 414));
                Line(g, Gold, 40f, new Point(512, 620), new Point(512, 414));
                Line(g, White, 40f, new Point(676, 620), new Point(676, 414));
                break;
            case 14:
                Polygon(g, new[] { new Point(512, 376), new Point(704, 620), new Point(320, 620) }, Green, 36f);
                Polygon(g, new[] { new Point(512, 446), new Point(608, 576), new Point(416, 576) }, Recessed, 24f);
                break;
            case 15:
                Ellipse(g, 430, 438, 164, 164, Gold, 30f);
                for (int i = 0; i < 3; i++) {
                    double angle = i * Math.PI * 2 / 3 - Math.PI / 2;
                    int x = 512 + (int)(Math.Cos(angle) * 154);
                    int y = 520 + (int)(Math.Sin(angle) * 154);
                    Ellipse(g, x - 62, y - 62, 124, 124, Orange, 24f);
                }
                break;
            case 16:
                Rect(g, 350, 386, 324, 268, Cyan, 24, 34f);
                for (int i = 0; i < 4; i++) Line(g, Gold, 24f, new Point(406 + i * 70, 350), new Point(406 + i * 70, 386));
                break;
            case 17:
                Ellipse(g, 364, 372, 296, 296, White, 34f);
                Ellipse(g, 444, 452, 136, 136, Recessed, 28f);
                break;
            case 18:
                Polygon(g, new[] { new Point(512, 344), new Point(650, 570), new Point(570, 650), new Point(454, 650), new Point(374, 570) }, Blue, 34f);
                Polygon(g, new[] { new Point(512, 578), new Point(586, 694), new Point(438, 694) }, Orange, 24f);
                break;
        }
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
            int x = offsetX + 310 + index * 82;
            int y = offsetY + 12;
            var destination = new Rectangle(x, y, size, size);
            if (attributes == null) graphics.DrawImage(image, destination);
            else graphics.DrawImage(image, destination, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }
    }

    private static string ToTitle(string name) {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.Replace('_', ' '));
    }
}
'@

[RecursiveIndustryUiIconsV3]::GenerateAll($masterDir, $exportDir)
[RecursiveIndustryUiIconsV3]::CreateProof(
    $exportDir,
    (Join-Path $proofDir 'all-ui-icons-size-proof.png'),
    $false
)
[RecursiveIndustryUiIconsV3]::CreateProof(
    $exportDir,
    (Join-Path $proofDir 'all-ui-icons-grayscale-proof.png'),
    $true
)

Get-ChildItem $masterDir, $exportDir, $proofDir -File |
    Sort-Object DirectoryName, Name |
    Select-Object FullName, Length, @{Name='SHA256';Expression={(Get-FileHash $_.FullName -Algorithm SHA256).Hash}} |
    Format-Table -AutoSize