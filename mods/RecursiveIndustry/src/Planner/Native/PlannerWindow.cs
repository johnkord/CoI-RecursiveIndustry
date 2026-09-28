using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mafi;
using Mafi.Core;
using Mafi.Core.GameLoop;
using Mafi.Localization;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.UiStatic.Toolbar;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace RecursiveIndustry.Planner.Native;

public sealed class PlannerWindow : Window
{
    private const string Plus = "Assets/Unity/UserInterface/General/Plus.svg";
    private const string Minus = "Assets/Unity/UserInterface/General/Minus128.png";
    private const string Previous = "Assets/Unity/UserInterface/General/ArrowLeft128.png";
    private const string Next = "Assets/Unity/UserInterface/General/ArrowRight.svg";
    private readonly Column _content = new Column(2.pt()).Padding(2.pt());
    private readonly Column _status = new(1.pt());
    private readonly Column _page = new(2.pt());
    private Column _catalogRows;
    private Column _results;
    private Controller _owner;

    public PlannerWindow() : base(new LocStrFormatted("Reconstruction Planner"))
    {
        WindowSize(1080.px(), 800.px()).MakeMovableAndEnablePositionSaving();
        Body.Add(_content);
    }

    private static Label Text(string value)
    {
        return new Label(new LocStrFormatted(value ?? string.Empty)).TextOverflow(TextOverflow.Wrap).MinWidth(0.px());
    }
    private static ButtonText Command(string label, Action action) => new(Button.General, new LocStrFormatted(label), action);
    private static string Show(Exact value) => value.Display();
    private string Products(IReadOnlyDictionary<string, Exact> values) => string.Join(" + ", values.Select(item => Show(item.Value) + " " + _owner.Catalog.ProductName(item.Key)));
    private string Recipe(PlannerProcess process) => Products(process.Inputs) + " -> " + Products(process.Outputs) + " / " + Show(process.Duration) + " Time";

    private static Dropdown<string> Choices(IEnumerable<string> values, string selected, Func<string, string> label, Action<string> changed, string empty = null)
    {
        var dropdown = new Dropdown<string>((value, _, _) => Text(value == null ? empty ?? "None" : label(value)))
            .SetOptions(values.ToArray()).OnValueChanged((value, _) => changed(value));
        if (empty != null) dropdown.IncludeClearOption(new LocStrFormatted(empty), null);
        if (selected != null) dropdown.SetValue(selected);
        return dropdown;
    }

    private Row Number(string label, string value, Action<string> changed, bool integer = false, string tooltip = null)
    {
        var field = new TextField().Text(value).CharLimit(64).ForFieldSetMaxWidth(140.px());
        field.OnValueChanged(text =>
        {
            bool valid = Exact.TryParse(text, out Exact amount) && amount >= 0 && amount <= 1000000000 && (!integer || amount.Denominator.IsOne);
            field.MarkAsError(!valid, new LocStrFormatted(integer ? "A nonnegative whole number is required." : "A nonnegative number or fraction is required."));
            changed(text);
            _owner.Changed();
        });
        return new Row(2.pt())
        {
            Text(label).Width(230.px()).Tooltip(new LocStrFormatted(tooltip ?? label)),
            field.Width(150.px()),
        };
    }

    private void Render(Controller owner)
    {
        _owner = owner;
        _content.Clear();
        _page.Clear();
        _results = null;
        _catalogRows = null;
        _content.Add(new Row(2.pt())
        {
            Command("Inspect current", () => owner.Refresh(true)),
            Command("Refresh", () => owner.Refresh(false)),
            Text("Read-only").TinyFontSize().Tooltip(new LocStrFormatted("No construction, demolition, assignment, or recipe commands.")),
        });
        var navigation = new Row(1.pt());
        foreach (string tab in new[] { "Catalog", "Process", "Added support", "Conversion", "References" })
        {
            string target = tab;
            navigation.Add(new ButtonText(owner.Tab == tab ? Button.Primary : Button.General,
                new LocStrFormatted(tab), () => { owner.Tab = target; Render(owner); }).FlexGrow(1f));
        }
        _content.Add(navigation, _status, _page);
        RenderStatus();
        if (owner.Catalog == null) return;
        switch (owner.Tab)
        {
            case "Catalog": Catalog(); break;
            case "Process": Process(); break;
            case "Added support": Support(); break;
            case "Conversion": Conversion(); break;
            default: References(); break;
        }
    }

    private void RenderStatus()
    {
        _status.Clear();
        _status.Add(Text(_owner.Status).TinyFontSize());
        PlannerSnapshot snapshot = _owner.Selection;
        if (snapshot == null) return;
        _status.Add(Text(snapshot.Name + " | " + snapshot.State + " | " + (snapshot.Constructed ? "Constructed" : "Not constructed")
            + " | " + (snapshot.Paused ? "Paused" : "Unpaused") + " | " + snapshot.Assignments.Count + " assigned recipes").TinyFontSize()
            .Tooltip(new LocStrFormatted("Entity " + snapshot.Entity + ", step " + snapshot.Step + ". Snapshot " + snapshot.Milliseconds.ToString("0.###")
                + " ms. Observed required/charged: " + snapshot.ComputingRequired + "/" + snapshot.ComputingCharged + " Computing, "
                + Show(snapshot.PowerRequired) + "/" + Show(snapshot.PowerCharged) + " kW. Transient values are not guaranteed spare capacity.")));
    }

    private void Catalog()
    {
        var search = new SearchField().Text(_owner.Query).CharLimit(120).OnValueChanged(value =>
        { _owner.Query = value; _owner.Page = 0; _owner.SearchChanged(); });
        _page.Add(new Row(2.pt())
        {
            search.FlexGrow(1f),
            Choices(new[] { "All sections" }.Concat(_owner.Catalog.Processes.Values.Select(row => row.Section).Distinct()),
                _owner.Section, value => value, value => { _owner.Section = value; _owner.Page = 0; RenderCatalogRows(); }).Width(240.px()),
            new Toggle().Label(new LocStrFormatted("Available only")).Value(_owner.AvailableOnly)
                .OnValueChanged(value => { _owner.AvailableOnly = value; _owner.Page = 0; RenderCatalogRows(); }),
            new Toggle().Label(new LocStrFormatted("Inspected host")).Value(_owner.InspectedOnly)
                .OnValueChanged(value => { _owner.InspectedOnly = value; _owner.Page = 0; RenderCatalogRows(); }).Enabled(_owner.Selection != null),
        });
        _catalogRows = new Column(1.pt());
        _page.Add(_catalogRows);
        RenderCatalogRows();
    }

    private void RenderCatalogRows()
    {
        if (_catalogRows == null) return;
        _catalogRows.Clear();
        PlannerProcess[] rows = _owner.Catalog.Processes.Values.Where(row =>
            (!_owner.AvailableOnly || row.Available) && (_owner.Section == "All sections" || row.Section == _owner.Section)
            && (!_owner.InspectedOnly || row.HostId == _owner.Selection?.Host)
            && (string.IsNullOrWhiteSpace(_owner.Query) || (row.Name + " " + Recipe(row)).IndexOf(_owner.Query, StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
        int pages = Math.Max(1, (rows.Length + 49) / 50);
        _owner.Page = Math.Min(_owner.Page, pages - 1);
        _catalogRows.Add(new Row(2.pt())
        {
            new ButtonIcon(Button.General, Previous, () => { _owner.Page--; RenderCatalogRows(); }).Compact().Enabled(_owner.Page > 0).Tooltip(new LocStrFormatted("Previous page")),
            Text((rows.Length == 0 ? "No matching recipes" : rows.Length + " bindings") + " | Page " + (_owner.Page + 1) + "/" + pages).FlexGrow(1f),
            Text(_owner.Projects.Count + "/8 selected"),
            new ButtonIcon(Button.General, Next, () => { _owner.Page++; RenderCatalogRows(); }).Compact().Enabled(_owner.Page + 1 < pages).Tooltip(new LocStrFormatted("Next page")),
        });
        var scroll = new ScrollColumn().Gap(2.pt()).MaxHeight(565.px());
        foreach (PlannerProcess process in rows.Skip(_owner.Page * 50).Take(50))
        {
            PlannerProcess chosen = process;
            scroll.Add(new Row(2.pt())
            {
                Text(process.Name + (process.Available ? "" : " [locked]")).Width(215.px()).Tooltip(new LocStrFormatted(process.Unlocks)),
                Text(Recipe(process)).FlexGrow(1f).MaxWidth(580.px()),
                Text(Show(process.PowerKw) + " kW\n" + process.Computing + " Computing").Width(115.px()),
                new ButtonIcon(Button.General, Plus, () => _owner.Add(chosen)).Compact().Enabled(process.Available && _owner.Projects.Count < 8)
                    .Tooltip(new LocStrFormatted("Add recipe comparison")),
            });
        }
        _catalogRows.Add(scroll);
    }

    private void Process()
    {
        _page.Add(Choices(new[] { "Alternatives", "Combined project" }, _owner.Combined ? "Combined project" : "Alternatives",
            value => value, value => { _owner.Combined = value == "Combined project"; _owner.Changed(); }).Width(250.px())
            .Tooltip(new LocStrFormatted("Alternatives are mutually exclusive scenarios. Combined projects share support and spare-capacity pools once.")));
        var projects = new ScrollColumn().Gap(2.pt()).MaxHeight(235.px());
        foreach (Controller.Entry entry in _owner.Projects)
        {
            if (!_owner.Catalog.Processes.TryGetValue(entry.Key, out PlannerProcess process)) continue;
            projects.Add(new Row(2.pt())
            {
                Text(process.Name).Width(240.px()).Tooltip(new LocStrFormatted(Recipe(process))),
                Choices(process.Outputs.Keys, entry.Product, _owner.Catalog.ProductName, product => { entry.Product = product; _owner.Changed(); }).Width(280.px()),
                Number("Target / 60 Time", entry.Rate, value => entry.Rate = value).FlexGrow(1f),
                new ButtonIcon(Button.General, Minus, () => { _owner.Projects.Remove(entry); _owner.Changed(); Render(_owner); }).Compact().Tooltip(new LocStrFormatted("Remove comparison")),
            });
        }
        _page.Add(projects);
        _results = new Column(2.pt());
        _page.Add(_results);
        RenderResults();
    }

    private void Support()
    {
        var form = new ScrollColumn().Gap(2.pt()).MaxHeight(275.px());
        PlannerCatalog catalog = _owner.Catalog;
        string[] products = catalog.Processes.Values.SelectMany(row => row.Inputs.Keys).Concat(catalog.Racks.Select(row => row.CoolantInput))
            .Distinct().OrderBy(catalog.ProductName, StringComparer.Ordinal).ToArray();
        _owner.SupplyProduct ??= products.FirstOrDefault();
        form.Add(new Row(2.pt())
        {
            Text("Supply product").Width(230.px()),
            Choices(products, _owner.SupplyProduct, catalog.ProductName, value => { _owner.SupplyProduct = value; Render(_owner); }).Width(450.px()),
        });
        if (_owner.SupplyProduct != null)
        {
            string product = _owner.SupplyProduct;
            _owner.Sources.TryGetValue(product, out string source);
            form.Add(new Row(2.pt())
            {
                Text("Dedicated source").Width(230.px()),
                Choices(catalog.Processes.Values.Where(row => row.Available && row.Outputs.ContainsKey(product)).Select(row => row.Key), source,
                    key => catalog.Processes[key].Name + ": " + Recipe(catalog.Processes[key]),
                    value => { if (value == null) _owner.Sources.Remove(product); else _owner.Sources[product] = value; _owner.Changed(); }, "External supply").Width(745.px()),
            }, Number("Supplied spare / 60 Time", _owner.Get(_owner.Spare, product), value => _owner.Spare[product] = value,
                tooltip: "Guaranteed supplied capacity, allocated once per combined project. Momentary idle output is not spare supply."));
        }
        form.Add(new Row(2.pt())
        {
            Text("Rack generation").Width(230.px()),
            Choices(catalog.Racks.Where(row => row.Available).Select(row => row.Id), _owner.Rack,
                id => { PlannerRack rack = catalog.Racks.First(row => row.Id == id); return rack.Name + " | " + rack.Computing + " Computing, " + Show(rack.Power) + " kW"; },
                value => { _owner.Rack = value; _owner.Changed(); }, "No rack selected").Width(745.px()),
        }, new Row(2.pt())
        {
            Text("Data Center").Width(230.px()),
            Choices(catalog.Shells.Where(row => row.Available).Select(row => row.Id), _owner.Shell,
                id => { PlannerShell shell = catalog.Shells.First(row => row.Id == id); return shell.Name + " | " + shell.Slots + " slots, " + shell.Workers + " staff"; },
                value => { _owner.Shell = value; _owner.Changed(); }, "No Data Center selected").Width(745.px()),
        });
        foreach (var family in catalog.Transports.GroupBy(row => row.Kind))
        {
            string kind = family.Key;
            _owner.Transports.TryGetValue(kind, out string selected);
            form.Add(new Row(2.pt())
            {
                Text(kind.Replace("IoPortShape_", "")).Width(230.px()),
                Choices(family.Where(row => row.Available).Select(row => row.Id), selected,
                    id => { PlannerTransport transport = catalog.Transports.First(row => row.Id == id); return transport.Name + " | " + Show(transport.Capacity) + "/60 Time"; },
                    value => { if (value == null) _owner.Transports.Remove(kind); else _owner.Transports[kind] = value; _owner.Changed(); }, "Unavailable / unselected").Width(745.px())
                    .Tooltip(new LocStrFormatted("Per-port throughput bound, not a route or connectivity guarantee.")),
            });
        }
        foreach (var chosen in _owner.Sources)
            if (catalog.Processes.TryGetValue(chosen.Value, out PlannerProcess producer)) form.Add(Text(catalog.ProductName(chosen.Key) + " <- " + producer.Name).TinyFontSize());
        _page.Add(form);
        _results = new Column(2.pt());
        _page.Add(_results);
        RenderResults();
    }

    private void Conversion()
    {
        var form = new ScrollColumn().Gap(2.pt()).MaxHeight(280.px());
        foreach (var field in new[] {
            ("Installed Computing", "installed"), ("Existing committed Computing", "committed"), ("Reserved Computing", "reserve"),
            ("Declared retired Computing", "retired"), ("Separate temporary Computing", "temporary"), ("Existing spare rack slots", "slots") })
        {
            string key = field.Item2;
            form.Add(Number(field.Item1, _owner.Get(_owner.Numbers, key), value => _owner.Numbers[key] = value, true,
                key == "retired" ? "Explicit retirement only; shared suppliers remain. Subtraction applies only after cutover."
                    : key == "committed" ? "Includes the old line. The planner does not add it twice during cutover." : null));
        }
        string[] products = _owner.Catalog.Products.Keys.OrderBy(_owner.Catalog.ProductName, StringComparer.Ordinal).ToArray();
        _owner.CapitalProduct ??= products.FirstOrDefault();
        form.Add(new Row(2.pt())
        {
            Text("Capital product").Width(230.px()),
            Choices(products, _owner.CapitalProduct, _owner.Catalog.ProductName, value => { _owner.CapitalProduct = value; Render(_owner); }).Width(450.px()),
        });
        if (_owner.CapitalProduct != null)
        {
            string product = _owner.CapitalProduct;
            form.Add(Number("Available stock", _owner.Get(_owner.Stock, product), value => _owner.Stock[product] = value),
                Number("Spare production / 60 Time", _owner.Get(_owner.CapitalRates, product), value => _owner.CapitalRates[product] = value));
        }
        _page.Add(form);
        _results = new Column(2.pt());
        _page.Add(_results);
        RenderResults();
    }

    private void References()
    {
        var scroll = new ScrollColumn().Gap(3.pt()).MaxHeight(625.px());
        foreach (PlannerReference reference in _owner.Catalog.References.OrderBy(row => row.Id == _owner.Selection?.Host ? 0 : 1).ThenBy(row => row.Section))
        {
            scroll.Add(Text(reference.Name + (reference.Available ? "" : " [locked]")).IncFontSize(),
            Text(reference.Section + " | " + reference.Family + " | " + (reference.Area < 0 ? "Mobile" : reference.Area + " occupied tiles") + " | " + reference.Workers + " staff").TinyFontSize(),
                Text(reference.Description), Text("Capital: " + Products(reference.Capital)).TinyFontSize(),
                Text("Maintenance / month: " + Products(reference.Maintenance)).TinyFontSize(),
                Text("Research: " + reference.Unlocks).TinyFontSize());
        }
        _page.Add(scroll);
    }

    private void RenderResults()
    {
        RenderStatus();
        if (_results == null) return;
        _results.Clear();
        if (_owner.Results == null) { _results.Add(Text(_owner.Status)); return; }
        var scroll = new ScrollColumn().Gap(3.pt()).MaxHeight(330.px());
        foreach (PlannerComparison comparison in _owner.Results)
        {
            PlannerResult result = comparison.Result;
            string heading = _owner.Combined ? "Combined project" : string.Join(" + ", comparison.Projects.Select(row =>
                _owner.Catalog.Processes[row.Process].Name + " | " + Show(row.Rate) + " " + _owner.Catalog.ProductName(row.Product) + "/60"));
            scroll.Add(Text(heading).IncFontSize(), Text(result.Status).TinyFontSize());
            if (!result.Complete)
            {
                foreach (string boundary in result.Boundaries) scroll.Add(Text(boundary).TinyFontSize());
                continue;
            }
            if (_owner.Tab == "Process")
            {
                foreach (ProcessQuote quote in result.Processes)
                    scroll.Add(Text(quote.Hosts + " x " + quote.Process.Name + " | requested " + Show(quote.Requested) + ", capacity bound " + Show(quote.Attainable) + "/60"),
                        Text(Recipe(quote.Process)).TinyFontSize(),
                        Text("Peak " + Show(quote.PeakPower) + " kW | ideal average " + Show(quote.IdealAveragePower) + " kW | " + Show(quote.EnergyPerOutput) + " kW-Time/output | "
                            + (quote.Process.Computing * quote.Hosts) + " Computing | " + quote.Process.Area * quote.Hosts + " occupied tiles").TinyFontSize(),
                        Text("Research: " + quote.Process.Unlocks).TinyFontSize());
                scroll.Add(Text("Known scope: " + result.Workers + " staff | " + result.Area + " occupied tiles | " + result.PeakComputing + " Computing | "
                    + Show(result.PeakPower + result.RackPower) + " peak kW").TinyFontSize());
            }
            else if (_owner.Tab == "Added support")
            {
                foreach (ProcessQuote quote in result.Support)
                    scroll.Add(Text(quote.Hosts + " x " + quote.Process.Name + " -> " + Show(quote.Requested) + " " + _owner.Catalog.ProductName(quote.Target) + "/60"),
                        Text(Recipe(quote.Process)).TinyFontSize());
                scroll.Add(Text(result.Racks + " racks | " + result.Shells + " Data Centers | rack power " + Show(result.RackPower) + " kW"),
                    Text("Supplied spare allocated /60: " + Products(result.SpareUsed)).TinyFontSize(),
                    Text("Maintenance / month: " + Products(result.Maintenance)).TinyFontSize());
            }
            else
            {
                scroll.Add(Text("Steady Computing " + result.SteadyDemand + " | cutover " + result.CutoverDemand + " | installed " + _owner.Get(_owner.Numbers, "installed")),
                    Text("Cutover: " + result.CutoverRacks + " added racks, " + result.CutoverShells + " added Data Centers, " + Show(result.CutoverRackPower) + " rack kW").TinyFontSize(),
                    Text("Known added cutover power: " + Show(result.CutoverKnownPower) + " kW").TinyFontSize(),
                    Text("Cutover external inputs /60: " + Products(result.CutoverExternal)).TinyFontSize(),
                    Text("Capital: " + Products(result.Capital)).TinyFontSize(),
                    Text("Additional cutover capital: " + Products(result.CutoverExtraCapital)).TinyFontSize(),
                    Text(result.CapitalBlockers.Count == 0 ? "Parallel supply lower bound: " + Show(result.CapitalWait) + " Time"
                        : "Capital supply missing: " + string.Join(", ", result.CapitalBlockers.Select(_owner.Catalog.ProductName))).TinyFontSize()
                        .Tooltip(new LocStrFormatted("Excludes delivery, construction, startup batches, temporary storage, and transport construction.")));
                foreach (ProcessQuote quote in result.CutoverSupport)
                    scroll.Add(Text("Cutover support: " + quote.Hosts + " x " + quote.Process.Name + " -> " + Show(quote.Requested) + " " + _owner.Catalog.ProductName(quote.Target) + "/60").TinyFontSize());
            }
            scroll.Add(Text("Remaining external inputs /60: " + Products(result.External)).TinyFontSize(),
                Text("Unallocated co-products /60: " + Products(result.Residuals)).TinyFontSize());
            foreach (string boundary in result.Boundaries) scroll.Add(Text(boundary).TinyFontSize());
        }
        _results.Add(scroll);
    }

    [GlobalDependency(RegistrationMode.AsEverything)]
    public sealed class Controller : WindowController<PlannerWindow>, IToolbarItemController, IDisposable
    {
        internal sealed class Entry
        {
            internal string Key;
            internal string Product;
            internal string Rate = "60";
        }

        private readonly PlannerSnapshots _snapshots;
        private readonly PlannerWork _work = new();
        private int _snapshotGeneration;
        private int _workGeneration;
        private int _epoch;
        private long _changed;
        private long _searchChanged;
        private bool _needsCalculation;
        private bool _disposed;
        private bool _visible = true;
        internal PlannerCatalog Catalog;
        internal PlannerSnapshot Selection;
        internal IReadOnlyList<PlannerComparison> Results;
        internal readonly List<Entry> Projects = new();
        internal readonly Dictionary<string, string> Sources = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> Spare = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> Numbers = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> Stock = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> CapitalRates = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, string> Transports = new(StringComparer.Ordinal);
        internal string Tab = "Catalog";
        internal string Query = "";
        internal string Section = "All sections";
        internal string Status = "Catalog pending";
        internal string Rack;
        internal string Shell;
        internal string SupplyProduct;
        internal string CapitalProduct;
        internal bool Combined;
        internal bool AvailableOnly;
        internal bool InspectedOnly;
        internal int Page;
        public bool DeactivateShortcutsIfNotVisible => false;
        public event Action<IToolbarItemController> VisibilityChanged;
        public bool IsVisible
        {
            get => _visible;
            set { if (_visible == value) return; _visible = value; VisibilityChanged?.Invoke(this); }
        }

        public Controller(ControllerContext context, ToolbarHud toolbar, PlannerSnapshots snapshots) : base(context, null)
        {
            _snapshots = snapshots;
            toolbar.AddMainMenuButton(new LocStrFormatted("Reconstruction Planner"), this, RecursiveIndustryIcons.ControlDeploymentGateway, -109f);
            Context.GameLoop.RenderUpdate.AddNonSaveable(this, RenderUpdate);
            Log.Info("RecursiveIndustry: PLANNER_TOOLBAR_REGISTERED commands=0");
        }

        protected override void OnActivate() { _epoch = _snapshots.Epoch; Refresh(true); }
        protected override void OnDeactivate()
        {
            _snapshots.Cancel();
            _work.Cancel();
            _workGeneration = 0;
            _snapshotGeneration = 0;
            Results = null;
            Selection = null;
            _needsCalculation = false;
            Log.Info("RecursiveIndustry: PLANNER_CLOSED commands=0");
        }

        internal string Get(Dictionary<string, string> values, string key) => values.TryGetValue(key, out string value) ? value : "0";
        internal void Refresh(bool inspect)
        {
            _work.Cancel();
            _workGeneration = 0;
            _needsCalculation = false;
            Catalog = null;
            Results = null;
            Selection = null;
            _snapshotGeneration = _snapshots.RequestCatalog(inspect);
            Status = "Snapshot pending";
            Window.Render(this);
        }

        internal void Add(PlannerProcess process)
        {
            if (Projects.Count >= 8 || !process.Available) return;
            Projects.Add(new Entry { Key = process.Key, Product = process.Outputs.Keys.First() });
            Tab = "Process";
            Changed();
            Window.Render(this);
        }

        internal void Changed()
        {
            _work.Cancel();
            _workGeneration = 0;
            Results = null;
            _changed = Stopwatch.GetTimestamp();
            _needsCalculation = true;
            Status = "Calculation pending";
            Window.RenderResults();
        }

        internal void SearchChanged() => _searchChanged = Stopwatch.GetTimestamp();

        private void RenderUpdate(GameTime time)
        {
            if (_disposed || !IsActive) return;
            if (_epoch != _snapshots.Epoch)
            {
                _epoch = _snapshots.Epoch;
                Catalog = null;
                Refresh(false);
            }
            if (_snapshotGeneration != 0 && _snapshots.TryRead(_snapshotGeneration, out PlannerCatalog catalog, out PlannerSnapshot selection, out string status))
            {
                _snapshotGeneration = 0;
                Catalog = catalog;
                Selection = selection;
                Status = status;
                if (catalog != null)
                {
                    Rack = catalog.Racks.FirstOrDefault(row => row.Id == Rack && row.Available)?.Id ?? catalog.Racks.LastOrDefault(row => row.Available)?.Id;
                    Shell = catalog.Shells.FirstOrDefault(row => row.Id == Shell && row.Available)?.Id ?? catalog.Shells.FirstOrDefault(row => row.Available)?.Id;
                    foreach (var family in catalog.Transports.GroupBy(row => row.Kind))
                        if (!Transports.TryGetValue(family.Key, out string current) || !family.Any(row => row.Id == current && row.Available))
                        {
                            PlannerTransport best = family.Where(row => row.Available).OrderBy(row => row.Capacity).LastOrDefault();
                            if (best != null) Transports[family.Key] = best.Id;
                            else Transports.Remove(family.Key);
                        }
                    if (Projects.Count > 0) { _changed = Stopwatch.GetTimestamp(); _needsCalculation = true; }
                }
                Window.Render(this);
            }
            if (_searchChanged != 0 && (Stopwatch.GetTimestamp() - _searchChanged) * 1000.0 / Stopwatch.Frequency >= 150)
            { _searchChanged = 0; Window.RenderCatalogRows(); }
            if (_needsCalculation && Catalog != null && (Stopwatch.GetTimestamp() - _changed) * 1000.0 / Stopwatch.Frequency >= 150)
            {
                _needsCalculation = false;
                try
                {
                    PlannerRequest request = Request();
                    _workGeneration = _work.Start(Catalog, request, Combined);
                    Status = "Calculating";
                }
                catch (ArgumentException error) { Status = error.Message; }
                Window.RenderResults();
            }
            if (_workGeneration != 0 && _work.TryRead(_workGeneration, out IReadOnlyList<PlannerComparison> results))
            {
                _workGeneration = 0;
                Results = results;
                Status = results.All(row => row.Result.Complete) ? "Known costs calculated; external supply remains" : "Incomplete comparison";
                Log.Info("RecursiveIndustry: PLANNER_QUOTE scenarios=" + results.Count + " nodes=" + results.Sum(row => row.Result.ExpandedNodes)
                    + " milliseconds=" + results.Sum(row => row.Result.Milliseconds) + " complete=" + results.All(row => row.Result.Complete) + " commands=0");
                Window.RenderResults();
            }
        }

        private PlannerRequest Request()
        {
            Exact Amount(string text)
            {
                if (!Exact.TryParse(text, out Exact value) || value < 0 || value > 1000000000) throw new ArgumentException("Invalid numeric input");
                return value;
            }
            int Whole(string name)
            {
                Exact value = Amount(Get(Numbers, name));
                if (!value.Denominator.IsOne) throw new ArgumentException("Computing and slots require whole numbers");
                return value.Ceiling();
            }
            Dictionary<string, Exact> Values(Dictionary<string, string> source) => source.ToDictionary(item => item.Key, item => Amount(item.Value));
            return new PlannerRequest(Projects.Select(entry => new PlannerProject(entry.Key, entry.Product, Amount(entry.Rate))), Sources, Values(Spare),
                Catalog.Transports.Where(row => row.Available && Transports.TryGetValue(row.Kind, out string id) && row.Id == id).ToDictionary(row => row.Kind, row => row.Capacity),
                Whole("installed"), Whole("committed"), Whole("reserve"), Whole("retired"), Whole("temporary"), Whole("slots"),
                Catalog.Racks.FirstOrDefault(row => row.Id == Rack), Catalog.Shells.FirstOrDefault(row => row.Id == Shell), Values(Stock), Values(CapitalRates));
        }

        public void Dispose()
        {
            _disposed = true;
            _work.Dispose();
            _snapshots.Cancel();
            Context.GameLoop.RenderUpdate.RemoveNonSaveable(this, RenderUpdate);
            Projects.Clear();
            Results = null;
            Catalog = null;
            Selection = null;
        }
    }
}