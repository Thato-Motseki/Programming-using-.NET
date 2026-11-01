using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SEIMS.Core;
using SEIMS.Data;
using SEIMS.Services;

namespace SEIMS.WinForms;

/// <summary>Section D dashboard. Controls are built in code to keep this form self-contained.</summary>
public sealed partial class MainForm : Form
{
    private readonly SEIMSDbContext _db;
    private readonly StockMonitor _monitor;
    private readonly FileLogger _logger;
    private readonly IConfiguration _config;
    private readonly MenuStrip _menu = new();
    private readonly ToolStrip _tools = new();
    private readonly StatusStrip _status = new();
    private readonly ToolStripStatusLabel _statusLabel = new();
    private readonly DataGridView _grid = new();
    private readonly TreeView _tree = new();
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly TextBox _search = new() { Width = 250, MaxLength = 120, PlaceholderText = "Search SKU, item or category" };
    private readonly BindingSource _binding = new();
    private readonly BindingList<ItemRow> _rows = new();
    private List<InventoryItem> _items = new();
    private List<Supplier> _suppliers = new();
    private List<StockMovement> _movements = new();
    private readonly System.Drawing.Printing.PrintDocument _printDocument = new();
    private int _printIndex;
    private bool _busy;

    public MainForm(SEIMSDbContext db, StockMonitor monitor, FileLogger logger, IConfiguration config)
    {
        InitializeComponent();

        _db = db ?? throw new ArgumentNullException(nameof(db));
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        Text = "SEIMS | Secure Enterprise Inventory & Audit Management";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 650);
        Size = new Size(1250, 800);
        Font = new Font("Segoe UI", 9F);
        BuildUi();
        WireEvents();
        _printDocument.PrintPage += PrintDocument_PrintPage;
        Shown += async (_, _) => await RefreshDataAsync();
    }

    private void BuildUi()
    {
        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.Add("Refresh", null, async (_, _) => await RefreshDataAsync());
        file.DropDownItems.Add("Print preview", null, (_, _) => PrintReport());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("E&xit", null, (_, _) => Close());
        var inv = new ToolStripMenuItem("&Inventory");
        inv.DropDownItems.Add("Add item", null, (_, _) => AddItem());
        inv.DropDownItems.Add("Update selected", null, (_, _) => UpdateItem());
        inv.DropDownItems.Add("Delete selected", null, async (_, _) => await DeleteItemAsync());
        inv.DropDownItems.Add("Receive stock", null, async (_, _) => await MoveStockAsync(StockMovementType.Receipt));
        inv.DropDownItems.Add("Issue stock", null, async (_, _) => await MoveStockAsync(StockMovementType.Issue));
        var reports = new ToolStripMenuItem("&Reports");
        reports.DropDownItems.Add("Selected-date movements", null, (_, _) => ShowMovements());
        var security = new ToolStripMenuItem("&Security");
        security.DropDownItems.Add("Current access", null, (_, _) => ShowAccess());
        var help = new ToolStripMenuItem("&Help");
        help.DropDownItems.Add("About SEIMS", null, (_, _) => MessageBox.Show(this, "Secure Enterprise Inventory & Audit Management System", "About", MessageBoxButtons.OK, MessageBoxIcon.Information));
        _menu.Items.AddRange(new ToolStripItem[] { file, inv, reports, security, help });

        AddTool("Refresh", async () => await RefreshDataAsync());
        AddTool("Add", AddItem);
        AddTool("Update", UpdateItem);
        AddTool("Delete", async () => await DeleteItemAsync());
        _tools.Items.Add(new ToolStripSeparator());
        AddTool("Receive", async () => await MoveStockAsync(StockMovementType.Receipt));
        AddTool("Issue", async () => await MoveStockAsync(StockMovementType.Issue));
        _tools.Items.Add(new ToolStripSeparator());
        AddTool("Print preview", PrintReport);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _binding.DataSource = _rows;
        _grid.DataSource = _binding;
        AddColumn(nameof(ItemRow.SKU), "SKU", "");
        AddColumn(nameof(ItemRow.Name), "Item", "");
        AddColumn(nameof(ItemRow.Category), "Category", "");
        AddColumn(nameof(ItemRow.Quantity), "Stock", "N0");
        AddColumn(nameof(ItemRow.UnitPrice), "Unit price", "C2");
        AddColumn(nameof(ItemRow.TotalValue), "Stock value", "C2");
        AddColumn(nameof(ItemRow.Supplier), "Supplier", "");
        AddColumn(nameof(ItemRow.Expiry), "Expiry date", "d");

        _tree.Dock = DockStyle.Fill;
        _tree.HideSelection = false;
        var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        left.Controls.Add(_tree);
        left.Controls.Add(new Label { Text = "CATEGORIES", Dock = DockStyle.Top, Height = 28, Font = new Font(Font, FontStyle.Bold) });
        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        right.Controls.Add(_grid);
        right.Controls.Add(new Label { Text = "INVENTORY REGISTER", Dock = DockStyle.Top, Height = 28, Font = new Font(Font, FontStyle.Bold) });
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Size = new Size(1050, 650)
        };

        split.Panel1MinSize = 180;
        split.Panel2MinSize = 500;
        split.SplitterDistance = 235;
        split.Panel1.Controls.Add(left); split.Panel2.Controls.Add(right);
        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(5), WrapContents = false };
        filters.Controls.Add(new Label { Text = "Search:", AutoSize = true, Margin = new Padding(3, 7, 3, 0) });
        filters.Controls.Add(_search);
        filters.Controls.Add(new Label { Text = "Movement date:", AutoSize = true, Margin = new Padding(12, 7, 3, 0) });
        filters.Controls.Add(_date);
        _status.Items.Add(_statusLabel); _statusLabel.Spring = true; _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(5) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(_menu, 0, 0); root.Controls.Add(_tools, 0, 1); root.Controls.Add(filters, 0, 2); root.Controls.Add(split, 0, 3);
        Controls.Add(root); Controls.Add(_status); MainMenuStrip = _menu;
        _menu.Dock = DockStyle.Top; _tools.Dock = DockStyle.Top; _status.Dock = DockStyle.Bottom;
    }

    private void AddTool(string label, Action action)
    {
        var button = new ToolStripButton(label) { DisplayStyle = ToolStripItemDisplayStyle.Text };
        button.Click += (_, _) => action(); _tools.Items.Add(button);
    }

    private void AddColumn(string property, string title, string format)
    {
        var column = new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = title, SortMode = DataGridViewColumnSortMode.Automatic, MinimumWidth = 75 };
        if (format.Length > 0) column.DefaultCellStyle.Format = format;
        _grid.Columns.Add(column);
    }

    private void WireEvents()
    {
        _search.TextChanged += (_, _) => ApplyFilters();
        _tree.AfterSelect += (_, _) => ApplyFilters();
        _grid.SelectionChanged += (_, _) => UpdateStatus();
        _date.ValueChanged += (_, _) => UpdateStatus();
    }

    private async Task RefreshDataAsync()
    {
        if (_busy) return;
        _busy = true; _statusLabel.Text = "Loading from SEIMS_DB...";
        try
        {
            if (!await _db.Database.CanConnectAsync()) throw new InvalidOperationException("SQL Server is unavailable or the connection string is incorrect.");
            _items = await _db.InventoryItems.AsNoTracking().Include(x => x.Supplier).OrderBy(x => x.Name).ToListAsync();
            _suppliers = await _db.Suppliers.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
            _movements = await _db.StockMovements.AsNoTracking().OrderByDescending(x => x.MovementDate).ToListAsync();
            RebuildTree(); ApplyFilters();
            _statusLabel.Text = $"Connected to SEIMS_DB | User: {Environment.UserName} | Items: {_items.Count}";
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            MessageBox.Show(this, "Could not load inventory from SEIMS_DB. Check SQL Server Express and the connection string.\n\n" + SingleLine(ex.Message), "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _statusLabel.Text = "Database load failed.";
        }
        finally { _busy = false; }
    }

    private void RebuildTree()
    {
        _tree.BeginUpdate();
        try
        {
            _tree.Nodes.Clear(); var all = _tree.Nodes.Add("ALL", "All categories");
            foreach (var c in _items.Select(x => Clean(x.Category)).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x)) all.Nodes.Add(c, c);
            all.Expand(); _tree.SelectedNode = all;
        }
        finally { _tree.EndUpdate(); }
    }

    private void ApplyFilters()
    {
        string q = Clean(_search.Text); string? category = _tree.SelectedNode?.Name;
        IEnumerable<InventoryItem> filtered = _items;
        if (q.Length > 0) filtered = filtered.Where(x => x.SKU.Contains(q, StringComparison.OrdinalIgnoreCase) || x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || x.Category.Contains(q, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(category) && category != "ALL") filtered = filtered.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
        _rows.Clear();
        foreach (var x in filtered.OrderBy(x => x.Name))
            _rows.Add(new ItemRow { Id = x.Id, SKU = Clean(x.SKU), Name = Clean(x.Name), Category = Clean(x.Category), Quantity = x.Quantity, UnitPrice = x.UnitPrice, TotalValue = x.TotalValue, Supplier = Clean(x.Supplier?.Name ?? _suppliers.FirstOrDefault(s => s.Id == x.SupplierId)?.Name ?? ""), Expiry = x is PerishableItem p ? p.ExpiryDate : null });
        UpdateStatus();
    }

    private InventoryItem? SelectedItem()
    {
        if (_grid.CurrentRow?.DataBoundItem is ItemRow row) return _items.FirstOrDefault(x => x.Id == row.Id);
        MessageBox.Show(this, "Select an inventory item first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return null;
    }

    private void AddItem()
    {
        using var dialog = new ItemDialog(_suppliers);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null) return;
        _ = RunAsync(async () =>
        {
            if (await _db.InventoryItems.AnyAsync(x => x.SKU == dialog.Result.SKU)) throw new InvalidOperationException("That SKU already exists.");
            _db.InventoryItems.Add(dialog.Result); await _db.SaveChangesAsync();
            _monitor.CheckStock(dialog.Result);
            _logger.WriteLog($"Item registered by {Environment.UserName}: {dialog.Result.SKU}, ID={dialog.Result.Id}");
            await RefreshDataAsync();
        });
    }

    private void UpdateItem()
    {
        var selected = SelectedItem(); if (selected is null) return;
        var tracked = _db.InventoryItems.FirstOrDefault(x => x.Id == selected.Id);
        if (tracked is null) { MessageBox.Show(this, "Item no longer exists. Refresh and try again."); return; }
        using var dialog = new ItemDialog(_suppliers, tracked);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Result is null) return;
        _ = RunAsync(async () =>
        {
            tracked.SKU = dialog.Result.SKU; tracked.Name = dialog.Result.Name; tracked.Category = dialog.Result.Category;
            tracked.Quantity = dialog.Result.Quantity; tracked.UnitPrice = dialog.Result.UnitPrice; tracked.SupplierId = dialog.Result.SupplierId;
            await _db.SaveChangesAsync(); _monitor.CheckStock(tracked);
            _logger.WriteLog($"Item updated by {Environment.UserName}: {tracked.SKU}, ID={tracked.Id}"); await RefreshDataAsync();
        });
    }

    private async Task DeleteItemAsync()
    {
        if (!Authorized("AuthorizedDeleteUsers")) { MessageBox.Show(this, "Your Windows account is not authorized to delete items.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var selected = SelectedItem(); if (selected is null) return;
        if (await _db.StockMovements.AnyAsync(x => x.InventoryItemId == selected.Id))
        { MessageBox.Show(this, "This item has stock-movement history. Deletion is blocked to preserve the audit trail.", "Deletion blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (MessageBox.Show(this, $"Permanently delete {Clean(selected.Name)} ({Clean(selected.SKU)})?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        await RunAsync(async () =>
        {
            var entity = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == selected.Id);
            if (entity is null) throw new InvalidOperationException("Item no longer exists.");
            if (await _db.StockMovements.AnyAsync(x => x.InventoryItemId == entity.Id)) throw new InvalidOperationException("Deletion blocked because movement history exists.");
            _db.InventoryItems.Remove(entity); await _db.SaveChangesAsync();
            _logger.WriteLog($"Item deleted by {Environment.UserName}: {entity.SKU}, ID={entity.Id}"); await RefreshDataAsync();
        });
    }

    private async Task MoveStockAsync(StockMovementType type)
    {
        var selected = SelectedItem(); if (selected is null) return;
        using var dialog = new MovementDialog(type, selected);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        await RunAsync(async () =>
        {
            var entity = await _db.InventoryItems.FirstOrDefaultAsync(x => x.Id == selected.Id);
            if (entity is null) throw new InvalidOperationException("Item no longer exists.");
            int qty = dialog.Quantity;
            if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be positive.");
            if (type == StockMovementType.Issue && qty > entity.Quantity) throw new InvalidOperationException("Cannot issue more than available stock.");
            entity.Quantity = type == StockMovementType.Receipt ? checked(entity.Quantity + qty) : entity.Quantity - qty;
            _db.StockMovements.Add(new StockMovement { InventoryItemId = entity.Id, SKU = entity.SKU, MovementType = type, Quantity = qty, MovementDate = DateTime.Now, PerformedBy = Environment.UserName, Notes = dialog.Notes });
            await _db.SaveChangesAsync(); _monitor.CheckStock(entity);
            _logger.WriteLog($"Stock {type} by {Environment.UserName}: SKU={entity.SKU}; qty={qty}; balance={entity.Quantity}; notes={SingleLine(dialog.Notes)}"); await RefreshDataAsync();
        });
    }

    private void ShowMovements()
    {
        var day = _date.Value.Date;
        var lines = _movements.Where(x => x.MovementDate.Date == day).OrderByDescending(x => x.MovementDate)
            .Select(x => $"{x.MovementDate:yyyy-MM-dd HH:mm} | {x.MovementType} | {Clean(x.SKU)} | Qty {x.Quantity} | By {Clean(x.PerformedBy)}").ToArray();
        MessageBox.Show(this, lines.Length == 0 ? "No movements for this date." : string.Join(Environment.NewLine, lines), $"Movements: {day:d}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private bool Authorized(string key)
    {
        var names = _config.GetSection("Security:" + key).Get<string[]>() ?? Array.Empty<string>();
        return names.Any(x => string.Equals(x?.Trim(), Environment.UserName, StringComparison.OrdinalIgnoreCase) || string.Equals(x?.Trim(), $"{Environment.UserDomainName}\\{Environment.UserName}", StringComparison.OrdinalIgnoreCase));
    }

    private void ShowAccess() => MessageBox.Show(this,
        $"Account: {Environment.UserDomainName}\\{Environment.UserName}\nDelete: {(Authorized("AuthorizedDeleteUsers") ? "Allowed" : "Denied")}\nPrint: {(Authorized("AuthorizedPrintUsers") ? "Allowed" : "Denied")}\n\nConfigured allow-lists are application-level checks.",
        "Current access", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void PrintReport()
    {
        if (!Authorized("AuthorizedPrintUsers")) { MessageBox.Show(this, "Your Windows account is not authorized to print.", "Printing restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        _printIndex = 0;
        using var preview = new PrintPreviewDialog { Document = _printDocument, Width = 1000, Height = 750, StartPosition = FormStartPosition.CenterParent };
        preview.ShowDialog(this);
    }

    private void PrintDocument_PrintPage(object? sender, System.Drawing.Printing.PrintPageEventArgs e)
    {
        if (e.Graphics is null) return;
        using var title = new Font("Segoe UI", 14, FontStyle.Bold); using var body = new Font("Consolas", 8.5f);
        float y = e.MarginBounds.Top;
        e.Graphics.DrawString("SEIMS — Inventory Report", title, Brushes.Black, e.MarginBounds.Left, y); y += title.GetHeight(e.Graphics) + 8;
        e.Graphics.DrawString($"Generated {DateTime.Now:g} | User {Environment.UserName}", body, Brushes.Black, e.MarginBounds.Left, y); y += body.GetHeight(e.Graphics) + 10;
        var lines = _rows.Select(x => $"{x.SKU,-15} {Clip(x.Name, 22),-22} {Clip(x.Category, 12),-12} Qty {x.Quantity,6} Value {x.TotalValue,12:C}").ToArray();
        while (_printIndex < lines.Length)
        {
            if (y + body.GetHeight(e.Graphics) > e.MarginBounds.Bottom) { e.HasMorePages = true; return; }
            e.Graphics.DrawString(lines[_printIndex++], body, Brushes.Black, e.MarginBounds.Left, y); y += body.GetHeight(e.Graphics) + 3;
        }
        e.HasMorePages = false;
    }

    private async Task RunAsync(Func<Task> action)
    {
        try { UseWaitCursor = true; await action(); }
        catch (DbUpdateException ex) { MessageBox.Show(this, "Database rejected the operation. Check required fields, SKU uniqueness and supplier relationship.\n\n" + SingleLine(ex.GetBaseException().Message), "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        catch (Exception ex) when (ex is not OutOfMemoryException) { MessageBox.Show(this, SingleLine(ex.Message), "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { UseWaitCursor = false; }
    }

    private void UpdateStatus() => _statusLabel.Text = $"User: {Environment.UserName} | Items shown: {_rows.Count} | Movement date: {_date.Value:d}";
    private static string Clean(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    private static string SingleLine(string? value) => Clean(value?.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' '));
    private static string Clip(string? value, int max) { var s = SingleLine(value); return s.Length <= max ? s : s[..(max - 1)] + "…"; }
    protected override void OnFormClosed(FormClosedEventArgs e) { _printDocument.Dispose(); base.OnFormClosed(e); }

    private sealed class ItemRow
    {
        public int Id { get; set; }
        public string SKU { get; set; } = ""; public string Name { get; set; } = ""; public string Category { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Supplier { get; set; } = ""; public DateTime? Expiry { get; set; }
    }
}

internal sealed class ItemDialog : Form
{
    private readonly List<Supplier> _suppliers;
    private readonly InventoryItem? _original;
    private readonly TextBox _sku = new() { MaxLength = 40 };
    private readonly TextBox _name = new() { MaxLength = 120 };
    private readonly TextBox _category = new() { MaxLength = 80 };
    private readonly NumericUpDown _qty = new() { Minimum = 0, Maximum = 100000000 };
    private readonly NumericUpDown _price = new() { Minimum = 0, Maximum = 100000000, DecimalPlaces = 2 };
    private readonly ComboBox _supplier = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    public InventoryItem? Result { get; private set; }

    public ItemDialog(List<Supplier> suppliers, InventoryItem? original = null)
    {
        _suppliers = suppliers; _original = original; Text = original is null ? "Register item" : "Update item";
        FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false; ClientSize = new Size(440, 300);
        _supplier.DropDownStyle = ComboBoxStyle.DropDown;
        _supplier.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _supplier.AutoCompleteSource = AutoCompleteSource.ListItems;
        _supplier.DataSource = suppliers.ToList(); _supplier.DisplayMember = nameof(Supplier.Name); _supplier.ValueMember = nameof(Supplier.Id);
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 6 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(table, 0, "SKU", _sku); AddRow(table, 1, "Name", _name); AddRow(table, 2, "Category", _category); AddRow(table, 3, "Quantity", _qty); AddRow(table, 4, "Unit price", _price); AddRow(table, 5, "Supplier", _supplier);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
        var save = new Button { Text = "Save", Width = 85 }; var cancel = new Button { Text = "Cancel", Width = 85, DialogResult = DialogResult.Cancel };
        save.Click += (_, _) => SaveValues(); buttons.Controls.Add(save); buttons.Controls.Add(cancel); Controls.Add(table); Controls.Add(buttons); AcceptButton = save; CancelButton = cancel;
        if (original is not null) { _sku.Text = original.SKU; _name.Text = original.Name; _category.Text = original.Category; _qty.Value = original.Quantity; _price.Value = Math.Min(_price.Maximum, original.UnitPrice); if (suppliers.Any(x => x.Id == original.SupplierId)) _supplier.SelectedValue = original.SupplierId; }
    }

    private static void AddRow(TableLayoutPanel table, int row, string label, Control control)
    {
        table.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 0, row);
        control.Dock = DockStyle.Fill; control.Margin = new Padding(3, 4, 3, 4); table.Controls.Add(control, 1, row);
    }

    private void SaveValues()
    {
        string sku = (_sku.Text ?? "").Trim().ToUpperInvariant(); string name = Clean(_name.Text); string category = Clean(_category.Text);
        if (sku.Length == 0 || name.Length == 0 || category.Length == 0) { MessageBox.Show(this, "SKU, name and category are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (sku.Any(char.IsControl) || name.Length > 120 || category.Length > 80) { MessageBox.Show(this, "One or more fields contain unsupported characters or exceed the allowed length."); return; }
        string supplierName = Clean(_supplier.Text);
        if (supplierName.Length == 0)
        {
            MessageBox.Show(this, "Supplier is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            Supplier? supplier = _suppliers.FirstOrDefault(existing =>
                string.Equals(existing.Name.Trim(), supplierName, StringComparison.OrdinalIgnoreCase));

            if (supplier is null)
            {
                supplier = new Supplier { Name = supplierName };
                _suppliers.Add(supplier);
            }

            Result = new InventoryItem(sku, name, category, decimal.ToInt32(_qty.Value), _price.Value)
            {
                Id = _original?.Id ?? 0,
                SupplierId = supplier.Id,
                Supplier = supplier
            };
            DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException or OverflowException) { MessageBox.Show(this, ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
    private static string Clean(string? s) => string.Join(" ", (s ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
}

internal sealed class MovementDialog : Form
{
    private readonly NumericUpDown _qty = new() { Minimum = 1, Maximum = 100000000, Value = 1 };
    private readonly TextBox _notes = new() { Multiline = true, MaxLength = 300, ScrollBars = ScrollBars.Vertical };
    public int Quantity => decimal.ToInt32(_qty.Value);
    public string Notes => string.Join(" ", (_notes.Text ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Trim();
    public MovementDialog(StockMovementType type, InventoryItem item)
    {
        Text = type == StockMovementType.Receipt ? "Receive stock" : "Issue stock"; FormBorderStyle = FormBorderStyle.FixedDialog; StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false; ClientSize = new Size(410, 190);
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 3 }; table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var itemLabel = new Label { Text = $"{item.Name} ({item.SKU}) — current stock: {item.Quantity}", AutoSize = true }; table.Controls.Add(itemLabel, 0, 0); table.SetColumnSpan(itemLabel, 2);
        table.Controls.Add(new Label { Text = "Quantity", AutoSize = true }, 0, 1); table.Controls.Add(_qty, 1, 1); table.Controls.Add(new Label { Text = "Notes", AutoSize = true }, 0, 2); table.Controls.Add(_notes, 1, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) }; var ok = new Button { Text = "Confirm", Width = 85 }; var cancel = new Button { Text = "Cancel", Width = 85, DialogResult = DialogResult.Cancel };
        ok.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); }; buttons.Controls.Add(ok); buttons.Controls.Add(cancel); Controls.Add(table); Controls.Add(buttons); AcceptButton = ok; CancelButton = cancel;
    }
}
