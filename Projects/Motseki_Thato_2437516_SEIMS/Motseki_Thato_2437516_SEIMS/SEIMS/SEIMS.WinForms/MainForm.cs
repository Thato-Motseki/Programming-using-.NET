using Microsoft.EntityFrameworkCore;
using SEIMS.Core;
using SEIMS.Data;
using SEIMS.Services;
using System.Drawing.Printing;

namespace SEIMS.WinForms;

public partial class MainForm : Form
{
    private readonly SEIMSDbContext _db;
    private readonly InventoryDataService _data;
    private readonly InventoryReports _reports;
    private readonly FileLogger _audit = new(Path.Combine(AppContext.BaseDirectory, "Logs", "desktop-audit.txt"));
    private readonly BindingSource _binding = new();
    private readonly DataGridView _grid = new();
    private readonly TreeView _categories = new();
    private readonly TextBox _search = new();
    private readonly TextBox _sku = new();
    private readonly TextBox _name = new();
    private readonly TextBox _category = new();
    private readonly TextBox _quantity = new();
    private readonly TextBox _price = new();
    private readonly ComboBox _supplier = new();
    private readonly DateTimePicker _date = new();
    private readonly Label _stockCount = new();
    private readonly Label _lowCount = new();
    private readonly Label _valueCount = new();
    private readonly ToolStripStatusLabel _status = new("Ready");
    private List<InventoryItem> _items = new();
    private List<Supplier> _suppliers = new();
    private int? _selectedId;
    private string _role = "Operator";
    private ToolStripButton? _roleButton;
    private string _activeCategory = "All items";
    private readonly PrintDocument _printDocument = new();
    private int _printRow;

    public MainForm(SEIMSDbContext db, InventoryDataService data, InventoryReports reports)
    {
        _db = db;
        _data = data;
        _reports = reports;
        InitializeComponent();
        BuildWorkspace();
        _printDocument.PrintPage += PrintInventoryPage;
        Shown += async (_, _) => await LoadWorkspaceAsync();
    }

    private void BuildWorkspace()
    {
        Text = "SEIMS | Warehouse Operations";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 700);
        Size = new Size(1420, 850);
        BackColor = Color.FromArgb(244, 246, 248);
        Font = new Font("Segoe UI", 9F);

        var menu = new MenuStrip { BackColor = Color.White, Padding = new Padding(8, 5, 8, 5) };
        var file = new ToolStripMenuItem("File");
        file.DropDownItems.Add("Refresh workspace", null, async (_, _) => await LoadWorkspaceAsync());
        file.DropDownItems.Add("Print stock report…", null, (_, _) => PrintReport());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Exit", null, (_, _) => Close());
        var records = new ToolStripMenuItem("Inventory");
        records.DropDownItems.Add("New item", null, (_, _) => ClearEditor());
        records.DropDownItems.Add("Save item", null, async (_, _) => await SaveItemAsync());
        records.DropDownItems.Add("Delete selected item", null, async (_, _) => await DeleteItemAsync());
        var help = new ToolStripMenuItem("Help");
        help.DropDownItems.Add("About SEIMS", null, (_, _) => MessageBox.Show("Secure Enterprise Inventory & Audit Management System\nWarehouse operations workspace", "About SEIMS", MessageBoxButtons.OK, MessageBoxIcon.Information));
        menu.Items.AddRange(new ToolStripItem[] { file, records, help });
        MainMenuStrip = menu;

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.White, Padding = new Padding(8, 5, 8, 5), ImageScalingSize = new Size(16, 16) };
        toolbar.Items.Add(new ToolStripButton("Refresh", null, async (_, _) => await LoadWorkspaceAsync()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(new ToolStripButton("New item", null, (_, _) => ClearEditor()));
        toolbar.Items.Add(new ToolStripButton("Save changes", null, async (_, _) => await SaveItemAsync()));
        toolbar.Items.Add(new ToolStripButton("Delete", null, async (_, _) => await DeleteItemAsync()));
        toolbar.Items.Add(new ToolStripSeparator());
        toolbar.Items.Add(new ToolStripButton("Stock movement", null, (_, _) => OpenMovementDialog()));
        toolbar.Items.Add(new ToolStripButton("Print report", null, (_, _) => PrintReport()));
        _roleButton = new ToolStripButton("Role: Operator ▾", null, (_, _) => ChangeRole());
        toolbar.Items.Add(new ToolStripSeparator()); toolbar.Items.Add(_roleButton);

        var statusStrip = new StatusStrip { BackColor = Color.White, SizingGrip = false };
        statusStrip.Items.Add(_status);
        statusStrip.Items.Add(new ToolStripStatusLabel("SEIMS  •  Local warehouse workspace") { Spring = true, TextAlign = ContentAlignment.MiddleRight });

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(18, 10, 18, 10) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        var header = new Panel { Dock = DockStyle.Fill };
        var title = new Label { Text = "Warehouse overview", Font = new Font("Segoe UI Semibold", 21F), ForeColor = Color.FromArgb(31, 48, 61), AutoSize = true, Location = new Point(0, 3) };
        var subtitle = new Label { Text = "Inventory records, stock movements and operational controls", ForeColor = Color.FromArgb(102, 116, 127), AutoSize = true, Location = new Point(2, 42) };
        header.Controls.Add(title); header.Controls.Add(subtitle);
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 5, 0, 12) };
        for (int i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
        cards.Controls.Add(MakeMetricCard("ITEMS IN CATALOG", _stockCount, "Active inventory records", Color.FromArgb(37, 99, 115)), 0, 0);
        cards.Controls.Add(MakeMetricCard("LOW STOCK · BELOW 5", _lowCount, "Requires replenishment", Color.FromArgb(176, 91, 55)), 1, 0);
        cards.Controls.Add(MakeMetricCard("TOTAL STOCK VALUE", _valueCount, "Quantity × unit price", Color.FromArgb(75, 103, 78)), 2, 0);

        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 4, 0, 4) };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var nav = PanelCard(); nav.Padding = new Padding(10);
        nav.Controls.Add(new Label { Text = "BROWSE", Font = new Font("Segoe UI Semibold", 9F), ForeColor = Color.FromArgb(102, 116, 127), Dock = DockStyle.Top, Height = 28 });
        _categories.Dock = DockStyle.Fill; _categories.BorderStyle = BorderStyle.None; _categories.BackColor = Color.White; _categories.Font = new Font("Segoe UI", 9.5F);
        _categories.AfterSelect += (_, e) => { _activeCategory = e.Node?.Text ?? "All items"; ApplyFilters(); };
        nav.Controls.Add(_categories);

        var listPanel = PanelCard(); listPanel.Padding = new Padding(12);
        var listLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var searchRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        _search.PlaceholderText = "Search SKU, item or supplier…"; _search.Dock = DockStyle.Fill; _search.Margin = new Padding(0, 3, 8, 8); _search.TextChanged += (_, _) => ApplyFilters();
        var searchLabel = new Label { Text = "INVENTORY REGISTER", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(102, 116, 127), Font = new Font("Segoe UI Semibold", 8.5F) };
        searchRow.Controls.Add(_search, 0, 0); searchRow.Controls.Add(searchLabel, 1, 0);
        ConfigureGrid(); listLayout.Controls.Add(searchRow, 0, 0); listLayout.Controls.Add(_grid, 0, 1); listPanel.Controls.Add(listLayout);

        var editor = PanelCard(); editor.Padding = new Padding(14);
        var editorLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); editorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        editorLayout.Controls.Add(new Label { Text = "ITEM DETAILS", Font = new Font("Segoe UI Semibold", 10F), ForeColor = Color.FromArgb(31, 48, 61), Dock = DockStyle.Fill }, 0, 0);
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoScroll = true };
        AddField(fields, "SKU / ITEM CODE", _sku); AddField(fields, "ITEM NAME", _name); AddField(fields, "CATEGORY", _category); AddField(fields, "QUANTITY ON HAND", _quantity); AddField(fields, "UNIT PRICE (LSL)", _price);
        _supplier.DropDownStyle = ComboBoxStyle.DropDownList; _supplier.Dock = DockStyle.Top; AddField(fields, "SUPPLIER", _supplier);
        _date.Format = DateTimePickerFormat.Short; _date.Value = DateTime.Today; _date.Dock = DockStyle.Top; AddField(fields, "REVIEW DATE", _date);
        editorLayout.Controls.Add(fields, 0, 1);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        var save = MakeButton("Save item", Color.FromArgb(37, 99, 115)); save.Click += async (_, _) => await SaveItemAsync();
        var clear = MakeButton("Clear form", Color.FromArgb(232, 237, 240), Color.FromArgb(49, 65, 75)); clear.Click += (_, _) => ClearEditor();
        buttons.Controls.Add(save); buttons.Controls.Add(clear); editorLayout.Controls.Add(buttons, 0, 2); editor.Controls.Add(editorLayout);

        workspace.Controls.Add(nav, 0, 0); workspace.Controls.Add(listPanel, 1, 0); workspace.Controls.Add(editor, 2, 0);
        root.Controls.Add(header, 0, 0); root.Controls.Add(cards, 0, 1); root.Controls.Add(workspace, 0, 2); root.Controls.Add(statusStrip, 0, 3);
        Controls.Add(root); Controls.Add(toolbar); Controls.Add(menu);
        toolbar.Dock = DockStyle.Top; menu.Dock = DockStyle.Top;
        root.Dock = DockStyle.Fill;
    }

    private static Panel MakeMetricCard(string caption, Label value, string note, Color accent)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(14) };
        panel.Paint += (_, e) => using (var brush = new SolidBrush(accent)) e.Graphics.FillRectangle(brush, 0, 0, 4, panel.Height);
        panel.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 20, Font = new Font("Segoe UI Semibold", 8F), ForeColor = Color.FromArgb(102, 116, 127) });
        value.Text = "—"; value.Dock = DockStyle.Top; value.Height = 37; value.Font = new Font("Segoe UI Semibold", 20F); value.ForeColor = accent;
        panel.Controls.Add(value); value.BringToFront();
        panel.Controls.Add(new Label { Text = note, Dock = DockStyle.Bottom, Height = 20, ForeColor = Color.FromArgb(120, 132, 140) });
        return panel;
    }
    private static Panel PanelCard() => new() { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 12, 0) };
    private static Button MakeButton(string text, Color back, Color? fore = null) => new() { Text = text, Width = 125, Height = 32, BackColor = back, ForeColor = fore ?? Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Semibold", 9F), Margin = new Padding(0, 0, 8, 6), Cursor = Cursors.Hand };
    private static void AddField(TableLayoutPanel panel, string caption, Control input)
    {
        panel.RowCount += 1; panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        var group = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 2, 0, 4) };
        group.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 21, Font = new Font("Segoe UI Semibold", 8F), ForeColor = Color.FromArgb(102, 116, 127) });
        input.Dock = DockStyle.Top; input.Height = 28; input.Margin = new Padding(0, 0, 0, 0); group.Controls.Add(input); input.BringToFront(); panel.Controls.Add(group, 0, panel.RowCount - 1);
    }
    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false; _grid.AllowUserToDeleteRows = false; _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.AutoGenerateColumns = false; _grid.RowHeadersVisible = false; _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None; _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal; _grid.GridColor = Color.FromArgb(232, 237, 240);
        _grid.EnableHeadersVisualStyles = false; _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 248, 249); _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(65, 82, 92); _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5F); _grid.ColumnHeadersHeight = 36;
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 236, 239); _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(31, 48, 61); _grid.RowTemplate.Height = 32;
        AddColumn("SKU", "SKU", 85); AddColumn("Name", "Item name", 150); AddColumn("Category", "Category", 100); AddColumn("Quantity", "Qty", 50); AddColumn("UnitPrice", "Unit price", 80, "N2"); AddColumn("SupplierName", "Supplier", 120); AddColumn("TotalValue", "Stock value", 90, "N2");
        _grid.DataSource = _binding;
        _grid.SelectionChanged += (_, _) => { if (_grid.CurrentRow?.DataBoundItem is InventoryRow row) FillEditor(row.Source); };
        _grid.CellFormatting += (_, e) => { if (_grid.Columns[e.ColumnIndex].DataPropertyName == "Quantity" && e.Value is int qty && qty < 5) { e.CellStyle.ForeColor = Color.FromArgb(176, 70, 45); e.CellStyle.Font = new Font("Segoe UI Semibold", 9F); } };
    }
    private void AddColumn(string property, string header, int width, string? format = null)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = header, Width = width, SortMode = DataGridViewColumnSortMode.Automatic, DefaultCellStyle = { Format = format ?? "" } });
    }

    private async Task LoadWorkspaceAsync()
    {
        try
        {
            _items = await _data.GetInventoryWithSuppliersAsync();
            _suppliers = await _data.GetSuppliersAsync();
            _supplier.DataSource = _suppliers; _supplier.DisplayMember = nameof(Supplier.Name); _supplier.ValueMember = nameof(Supplier.Id);
            BuildCategoryTree(); ApplyFilters(); UpdateMetrics();
            _status.Text = $"Loaded {_items.Count} inventory records · {DateTime.Now:HH:mm}";
        }
        catch (Exception ex) { ShowError("Could not load inventory. Check the SQL Server connection and database migrations.", ex); }
    }
    private void BuildCategoryTree()
    {
        _categories.BeginUpdate(); _categories.Nodes.Clear(); _categories.Nodes.Add("All items");
        foreach (var category in _items.Select(i => i.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c)) _categories.Nodes.Add(category);
        _categories.ExpandAll(); if (_categories.Nodes.Count > 0) _categories.SelectedNode = _categories.Nodes[0]; _categories.EndUpdate();
    }
    private void ApplyFilters()
    {
        string term = _search.Text.Trim();
        var filtered = _items.Where(i => (_activeCategory == "All items" || i.Category == _activeCategory) &&
            (term.Length == 0 || i.SKU.Contains(term, StringComparison.OrdinalIgnoreCase) || i.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || i.Category.Contains(term, StringComparison.OrdinalIgnoreCase) || (i.Supplier?.Name ?? "").Contains(term, StringComparison.OrdinalIgnoreCase)))
            .Select(i => new InventoryRow { Id = i.Id, SKU = i.SKU, Name = i.Name, Category = i.Category, Quantity = i.Quantity, UnitPrice = i.UnitPrice, SupplierName = i.Supplier?.Name ?? "Unassigned", TotalValue = i.TotalValue, Source = i }).ToList();
        _binding.DataSource = filtered;
    }
    private void UpdateMetrics()
    {
        _stockCount.Text = _items.Count.ToString("N0"); _lowCount.Text = _items.Count(i => i.Quantity < 5).ToString("N0"); _valueCount.Text = _items.Sum(i => i.TotalValue).ToString("N2") + " LSL";
    }
    private void FillEditor(InventoryItem item)
    {
        _selectedId = item.Id; _sku.Text = item.SKU; _name.Text = item.Name; _category.Text = item.Category; _quantity.Text = item.Quantity.ToString(); _price.Text = item.UnitPrice.ToString("0.00"); _date.Value = DateTime.Today;
        if (_supplier.DataSource != null && _suppliers.Any(s => s.Id == item.SupplierId)) _supplier.SelectedValue = item.SupplierId;
    }
    private void ClearEditor()
    {
        _selectedId = null; _sku.Clear(); _name.Clear(); _category.Clear(); _quantity.Text = "0"; _price.Text = "0.00"; _grid.ClearSelection(); _sku.Focus();
    }
    private async Task SaveItemAsync()
    {
        if (!int.TryParse(_quantity.Text.Trim(), out int qty) || qty < 0) { MessageBox.Show("Enter a whole-number quantity of zero or more.", "Check quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning); _quantity.Focus(); return; }
        if (!decimal.TryParse(_price.Text.Trim(), out decimal price) || price < 0) { MessageBox.Show("Enter a valid non-negative unit price.", "Check unit price", MessageBoxButtons.OK, MessageBoxIcon.Warning); _price.Focus(); return; }
        if (string.IsNullOrWhiteSpace(_sku.Text) || string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_category.Text)) { MessageBox.Show("SKU, item name and category are required.", "Missing details", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        try
        {
            bool isNew = !_selectedId.HasValue;
            InventoryItem? item = _selectedId.HasValue ? await _db.InventoryItems.FindAsync(_selectedId.Value) : null;
            if (item == null) { item = new InventoryItem(); _db.InventoryItems.Add(item); }
            item.SKU = _sku.Text.Trim().ToUpperInvariant(); item.Name = _name.Text.Trim(); item.Category = _category.Text.Trim(); item.Quantity = qty; item.UnitPrice = price;
            if (_supplier.SelectedValue is int sid) item.SupplierId = sid;
            await _db.SaveChangesAsync();
            _audit.WriteLog($"{_role} {(isNew ? "created" : "updated")} inventory item SKU={item.SKU}");
            _status.Text = $"Saved {item.SKU} · {_role} · {DateTime.Now:HH:mm}";
            await LoadWorkspaceAsync();
        }
        catch (DbUpdateException ex) { ShowError("The item could not be saved. Confirm that the SKU is unique and the selected supplier exists.", ex); }
        catch (Exception ex) { ShowError("The item could not be saved.", ex); }
    }
    private async Task DeleteItemAsync()
    {
        if (_role != "Administrator") { MessageBox.Show("Deleting inventory is restricted. Switch to an authorised Administrator account first.", "Permission required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!_selectedId.HasValue) { MessageBox.Show("Select an inventory item first."); return; }
        if (MessageBox.Show("Delete this inventory record? This action cannot be undone.", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { var item = await _db.InventoryItems.FindAsync(_selectedId.Value); if (item != null) { _db.InventoryItems.Remove(item); await _db.SaveChangesAsync(); _audit.WriteLog($"{_role} deleted inventory item SKU={item.SKU}"); } _status.Text = $"Record deleted by {_role} at {DateTime.Now:HH:mm}"; ClearEditor(); await LoadWorkspaceAsync(); }
        catch (Exception ex) { ShowError("Delete failed. Records referenced by stock movements cannot be removed.", ex); }
    }
    private void ChangeRole()
    {
        using var form = new Form { Text = "Authorised access", Width = 340, Height = 205, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
        var label = new Label { Text = "Select your working role", Left = 18, Top = 18, Width = 280 };
        var role = new ComboBox { Left = 18, Top = 45, Width = 280, DropDownStyle = ComboBoxStyle.DropDownList }; role.Items.AddRange(new object[] { "Operator", "Administrator" }); role.SelectedItem = _role;
        var pinLabel = new Label { Text = "Administrator access code", Left = 18, Top = 78, Width = 280 };
        var pin = new TextBox { Left = 18, Top = 101, Width = 280, UseSystemPasswordChar = true };
        var ok = new Button { Text = "Apply role", Left = 18, Top = 132, Width = 100, DialogResult = DialogResult.OK };
        form.Controls.AddRange(new Control[] { label, role, pinLabel, pin, ok }); form.AcceptButton = ok;
        if (form.ShowDialog(this) != DialogResult.OK) return;
        if (Equals(role.SelectedItem, "Administrator"))
        {
            string expected = Environment.GetEnvironmentVariable("SEIMS_ADMIN_PIN") ?? "ChangeThisPin";
            if (!string.Equals(pin.Text, expected, StringComparison.Ordinal)) { MessageBox.Show("Access code was not accepted.", "Access denied", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            _role = "Administrator";
        }
        else _role = "Operator";
        if (_roleButton != null) _roleButton.Text = $"Role: {_role} ▾";
        _audit.WriteLog($"Working role changed to {_role}");
        _status.Text = $"Role set to {_role} · {DateTime.Now:HH:mm}";
        MessageBox.Show($"Working role: {_role}.", "Access updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void OpenMovementDialog()
    {
        if (_grid.CurrentRow?.DataBoundItem is not InventoryRow row) { MessageBox.Show("Select an inventory item first."); return; }
        using var form = new Form { Text = "Record stock movement", Width = 390, Height = 330, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
        var type = new ComboBox { Left = 20, Top = 45, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList }; type.Items.AddRange(new object[] { "Receipt — stock in", "Issue — stock out" }); type.SelectedIndex = 0;
        var quantity = new NumericUpDown { Left = 20, Top = 105, Width = 320, Minimum = 1, Maximum = 1000000, Value = 1 };
        var performer = new TextBox { Left = 20, Top = 165, Width = 320, Text = Environment.UserName };
        var notes = new TextBox { Left = 20, Top = 220, Width = 320 };
        form.Controls.AddRange(new Control[] { new Label { Text = $"Item: {row.SKU} — {row.Name}", Left = 20, Top = 18, Width = 320 }, type, new Label { Text = "Movement type", Left = 20, Top = 25, Width = 320 }, new Label { Text = "Quantity", Left = 20, Top = 85, Width = 320 }, quantity, new Label { Text = "Performed by", Left = 20, Top = 145, Width = 320 }, performer, new Label { Text = "Notes (optional)", Left = 20, Top = 200, Width = 320 }, notes });
        var save = new Button { Text = "Record movement", Left = 20, Top = 255, Width = 145, DialogResult = DialogResult.OK }; form.Controls.Add(save); form.AcceptButton = save;
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _ = SaveMovementAsync(row.Id, type.SelectedIndex == 0 ? StockMovementType.Receipt : StockMovementType.Issue, (int)quantity.Value, performer.Text, notes.Text);
    }
    private async Task SaveMovementAsync(int itemId, StockMovementType type, int quantity, string performer, string notes)
    {
        try { await _data.RecordStockMovementAsync(itemId, type, quantity, performer, notes); _audit.WriteLog($"{performer} recorded {type} quantity={quantity} for inventory item ID={itemId}"); _status.Text = $"Stock {type.ToString().ToLowerInvariant()} recorded by {performer} · {DateTime.Now:HH:mm}"; await LoadWorkspaceAsync(); }
        catch (Exception ex) { ShowError("Stock movement was not recorded. Check the quantity and available stock.", ex); }
    }
    private void PrintReport()
    {
        if (_role != "Administrator") { MessageBox.Show("Printing inventory reports is restricted to an authorised Administrator.", "Permission required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        _audit.WriteLog($"{_role} opened inventory print preview");
        using var preview = new PrintPreviewDialog { Document = _printDocument, Width = 1000, Height = 700 };
        _printRow = 0; preview.ShowDialog(this);
    }
    private void PrintInventoryPage(object? sender, PrintPageEventArgs e)
    {
        Graphics g = e.Graphics; float y = 55; using var titleFont = new Font("Segoe UI", 18, FontStyle.Bold); using var headFont = new Font("Segoe UI", 9, FontStyle.Bold); using var bodyFont = new Font("Segoe UI", 9); using var brush = new SolidBrush(Color.Black);
        g.DrawString("SEIMS  |  Inventory report", titleFont, brush, 50, y); y += 38; g.DrawString($"Generated {DateTime.Now:g}   •   Printed by {_role}", bodyFont, brush, 50, y); y += 30;
        g.DrawString("SKU                  ITEM                         CATEGORY             QTY          UNIT PRICE          STOCK VALUE", headFont, brush, 50, y); y += 24;
        while (_printRow < _items.Count && y < e.MarginBounds.Bottom - 35)
        {
            var i = _items[_printRow++]; string line = $"{i.SKU,-20} {Clip(i.Name, 26),-27} {Clip(i.Category, 18),-20} {i.Quantity,4}          {i.UnitPrice,10:N2}          {i.TotalValue,11:N2}";
            g.DrawString(line, bodyFont, brush, 50, y); y += 19;
        }
        if (_printRow < _items.Count) e.HasMorePages = true; else { g.DrawString($"Total inventory value: {_items.Sum(i => i.TotalValue):N2} LSL", headFont, brush, 50, y + 12); e.HasMorePages = false; _printRow = 0; }
    }
    private static string Clip(string value, int max) => value.Length <= max ? value : value[..(max - 1)] + "…";
    private static void ShowError(string message, Exception ex) => MessageBox.Show($"{message}\n\nDetails: {ex.GetBaseException().Message}", "SEIMS — Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private sealed class InventoryRow
    {
        public int Id { get; set; } public string SKU { get; set; } = ""; public string Name { get; set; } = ""; public string Category { get; set; } = "";
        public int Quantity { get; set; } public decimal UnitPrice { get; set; } public string SupplierName { get; set; } = ""; public decimal TotalValue { get; set; }
        public InventoryItem Source { get; set; } = new();
    }
}
