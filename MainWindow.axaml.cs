using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Data.Sqlite;
using System.Globalization;
using System.Text;

namespace StockFlow;

public partial class MainWindow : Window
{
    private readonly string _cs;
    private readonly ListBox _list = new();
    private readonly TextBlock _stats = new(), _alerts = new(), _status = new();
    private readonly TextBox _name = new(), _sku = new(), _category = new(), _qty = new(), _minimum = new(), _price = new(), _search = new(), _moveQty = new(), _note = new();
    private readonly ComboBox _moveProduct = new(), _language = new();
    private List<Product> _products = new();
    private bool _en;
    private int? _editingId;

    public MainWindow()
    {
        InitializeComponent();
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StockFlow");
        Directory.CreateDirectory(dir);
        _cs = new SqliteConnectionStringBuilder { DataSource = Path.Combine(dir, "stockflow.db") }.ToString();
        using (var db = new SqliteConnection(_cs))
        {
            db.Open();
            using var cmd = db.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Products(Id INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Sku TEXT NOT NULL UNIQUE,Category TEXT NOT NULL DEFAULT '',Quantity INTEGER NOT NULL DEFAULT 0,MinimumQuantity INTEGER NOT NULL DEFAULT 0,UnitPrice REAL NOT NULL DEFAULT 0,CreatedAt TEXT NOT NULL);
                CREATE TABLE IF NOT EXISTS Movements(Id INTEGER PRIMARY KEY AUTOINCREMENT,ProductId INTEGER NOT NULL,Type TEXT NOT NULL,Quantity INTEGER NOT NULL,Note TEXT NOT NULL DEFAULT '',CreatedAt TEXT NOT NULL,FOREIGN KEY(ProductId) REFERENCES Products(Id));
                """;
            cmd.ExecuteNonQuery();
        }
        BuildUi();
        Reload();
    }

    private string T(string fr, string en) => _en ? en : fr;
    private static SolidColorBrush B(string hex) => new(Color.Parse(hex));
    private StackPanel Page() => new() { Margin = new Thickness(26), Spacing = 14 };
    private TextBlock Title(string s) => new() { Text = s, FontSize = 25, FontWeight = FontWeight.Bold, Foreground = B("#17243A") };
    private Border Card(Control c) => new() { Child = c, Background = Brushes.White, CornerRadius = new CornerRadius(12), Padding = new Thickness(18) };
    private Button Btn(string text, Action action)
    {
        var b = new Button { Content = text, Padding = new Thickness(14, 9), Background = B("#2563EB"), Foreground = Brushes.White, CornerRadius = new CornerRadius(7) };
        b.Click += (_, _) => action(); return b;
    }
    private Control Field(string label, TextBox box)
    {
        box.Width = 320; box.Watermark = label;
        return new StackPanel { Spacing = 5, Children = { new TextBlock { Text = label, FontSize = 12, Foreground = B("#64748B") }, box } };
    }

    private void BuildUi()
    {
        Background = B("#F3F6FA");
        var root = new Grid { ColumnDefinitions = new ColumnDefinitions("225,*") };
        var side = new StackPanel { Background = B("#17243A"), Spacing = 16 };
        side.Children.Add(new TextBlock { Text = "STOCKFLOW", FontSize = 23, FontWeight = FontWeight.Bold, Foreground = Brushes.White, Margin = new Thickness(22, 28, 10, 4) });
        side.Children.Add(new TextBlock { Text = T("GESTION DE STOCK", "INVENTORY MANAGEMENT"), FontSize = 11, Foreground = B("#A9B8CE"), Margin = new Thickness(22, 0, 10, 20) });
        var tabs = new TabControl { Margin = new Thickness(0) };
        tabs.Items.Add(new TabItem { Header = T("Tableau de bord", "Dashboard"), Content = Dashboard() });
        tabs.Items.Add(new TabItem { Header = T("Produits", "Products"), Content = ProductsPage() });
        tabs.Items.Add(new TabItem { Header = T("Mouvements", "Movements"), Content = MovementsPage() });
        tabs.Items.Add(new TabItem { Header = T("Rapports", "Reports"), Content = ReportsPage() });
        var header = new DockPanel { Margin = new Thickness(24, 18, 24, 12) };
        _language.Items.Add("Français"); _language.Items.Add("English"); _language.SelectedIndex = _en ? 1 : 0; _language.Width = 125;
        _language.SelectionChanged += (_, _) => { var next = _language.SelectedIndex == 1; if (next != _en) { _en = next; BuildUi(); Reload(); } };
        DockPanel.SetDock(_language, Dock.Right); header.Children.Add(_language);
        header.Children.Add(new TextBlock { Text = "Inventory workspace", FontSize = 19, FontWeight = FontWeight.SemiBold, Foreground = B("#17243A"), VerticalAlignment = VerticalAlignment.Center });
        var body = new DockPanel();
        DockPanel.SetDock(header, Dock.Top); body.Children.Add(header);
        DockPanel.SetDock(_status, Dock.Bottom); _status.Margin = new Thickness(24, 4, 24, 12); body.Children.Add(_status); body.Children.Add(tabs);
        root.Children.Add(side); Grid.SetColumn(body, 1); root.Children.Add(body); Content = root;
    }

    private Control Dashboard()
    {
        var p = Page(); p.Children.Add(Title(T("Vue d'ensemble", "Overview")));
        _stats.FontSize = 17; _stats.TextWrapping = TextWrapping.Wrap; p.Children.Add(Card(_stats));
        p.Children.Add(Title(T("Alertes de stock", "Stock alerts")));
        _alerts.TextWrapping = TextWrapping.Wrap; _alerts.Foreground = B("#B45309"); p.Children.Add(Card(_alerts));
        p.Children.Add(Btn(T("Actualiser", "Refresh"), Reload)); return p;
    }

    private Control ProductsPage()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("2*,3*"), ColumnSpacing = 18 };
        var left = Page(); left.Children.Add(Title(T("Catalogue", "Catalogue")));
        _search.Watermark = T("Rechercher un produit ou un SKU…", "Search product or SKU…");
        _search.TextChanged += (_, _) => LoadProducts(); left.Children.Add(_search);
        _list.Height = 440; _list.SelectionChanged += (_, _) => { if (_list.SelectedItem is Product p) Edit(p); }; left.Children.Add(_list);
        var right = Page(); right.Children.Add(Title(T("Fiche produit", "Product details")));
        right.Children.Add(Field(T("Nom", "Name"), _name)); right.Children.Add(Field(T("Référence / SKU", "SKU"), _sku));
        right.Children.Add(Field(T("Catégorie", "Category"), _category)); right.Children.Add(Field(T("Quantité", "Quantity"), _qty));
        right.Children.Add(Field(T("Seuil minimum", "Minimum stock"), _minimum)); right.Children.Add(Field(T("Prix unitaire", "Unit price"), _price));
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        actions.Children.Add(Btn(T("Ajouter", "Add"), () => Save(false))); actions.Children.Add(Btn(T("Modifier", "Update"), () => Save(true)));
        actions.Children.Add(Btn(T("Supprimer", "Delete"), Delete)); actions.Children.Add(Btn(T("Vider", "Clear"), Clear));
        right.Children.Add(actions); grid.Children.Add(left); Grid.SetColumn(right, 1); grid.Children.Add(right);
        return new ScrollViewer { Content = grid, Margin = new Thickness(8) };
    }

    private Control MovementsPage()
    {
        var p = Page(); p.Children.Add(Title(T("Entrées et sorties", "Stock in and out")));
        p.Children.Add(new TextBlock { Text = T("Enregistrez les réceptions, ventes ou utilisations de produits.", "Record incoming goods, sales or product usage."), Foreground = B("#64748B") });
        p.Children.Add(new TextBlock { Text = T("Produit", "Product") }); _moveProduct.Width = 360; p.Children.Add(_moveProduct);
        p.Children.Add(Field(T("Quantité", "Quantity"), _moveQty)); p.Children.Add(Field(T("Note (facultatif)", "Note (optional)"), _note));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(Btn(T("+ Entrée de stock", "+ Stock in"), () => Move("IN")));
        row.Children.Add(Btn(T("− Sortie de stock", "− Stock out"), () => Move("OUT"))); p.Children.Add(row); return p;
    }

    private Control ReportsPage()
    {
        var p = Page(); p.Children.Add(Title(T("Rapports", "Reports")));
        p.Children.Add(new TextBlock { Text = T("Exportez votre catalogue et les niveaux de stock au format CSV, compatible avec Excel.", "Export your catalogue and stock levels to CSV, compatible with Excel."), TextWrapping = TextWrapping.Wrap, Foreground = B("#64748B") });
        p.Children.Add(Btn(T("Exporter les produits en CSV", "Export products to CSV"), ExportCsv));
        p.Children.Add(Btn(T("Actualiser les statistiques", "Refresh statistics"), Reload)); return p;
    }

    private void LoadProducts()
    {
        _products.Clear();
        using var db = new SqliteConnection(_cs); db.Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Id,Name,Sku,Category,Quantity,MinimumQuantity,UnitPrice FROM Products WHERE Name LIKE $q OR Sku LIKE $q ORDER BY Name";
        cmd.Parameters.AddWithValue("$q", "%" + (_search.Text ?? "") + "%");
        using var r = cmd.ExecuteReader();
        while (r.Read()) _products.Add(new Product(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetInt32(4), r.GetInt32(5), r.GetDouble(6)));
        _list.ItemsSource = null; _list.ItemsSource = _products;
        _moveProduct.ItemsSource = _products.Select(x => $"{x.Name} ({x.Sku})").ToList();
        if (_products.Count > 0 && _moveProduct.SelectedIndex < 0) _moveProduct.SelectedIndex = 0;
    }

    private void Reload()
    {
        LoadProducts();
        using var db = new SqliteConnection(_cs); db.Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*),COALESCE(SUM(Quantity),0),COALESCE(SUM(Quantity*UnitPrice),0),COALESCE(SUM(CASE WHEN Quantity<=MinimumQuantity THEN 1 ELSE 0 END),0) FROM Products";
        using var r = cmd.ExecuteReader(); r.Read();
        _stats.Text = $"{T("Produits", "Products")}: {r.GetInt32(0)}     •     {T("Unités en stock", "Units in stock")}: {r.GetInt32(1)}     •     {T("Valeur estimée", "Estimated value")}: {r.GetDouble(2):N2}     •     {T("Alertes", "Alerts")}: {r.GetInt32(3)}";
        using var a = db.CreateCommand(); a.CommandText = "SELECT Name,Quantity,MinimumQuantity FROM Products WHERE Quantity<=MinimumQuantity ORDER BY Quantity";
        using var ar = a.ExecuteReader(); var lines = new List<string>();
        while (ar.Read()) lines.Add($"• {ar.GetString(0)} — {ar.GetInt32(1)} / {ar.GetInt32(2)}");
        _alerts.Text = lines.Count == 0 ? T("Aucune rupture ou alerte de stock.", "No low-stock alerts.") : string.Join(Environment.NewLine, lines);
    }

    private void Edit(Product p)
    {
        _editingId = p.Id; _name.Text = p.Name; _sku.Text = p.Sku; _category.Text = p.Category;
        _qty.Text = p.Quantity.ToString(); _minimum.Text = p.MinimumQuantity.ToString(); _price.Text = p.UnitPrice.ToString(CultureInfo.InvariantCulture);
    }

    private void Save(bool update)
    {
        if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_sku.Text) ||
            !int.TryParse(_qty.Text, out var q) || q < 0 || !int.TryParse(_minimum.Text, out var min) || min < 0 ||
            !double.TryParse(_price.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) || price < 0)
        { Status(T("Vérifiez les champs obligatoires et les valeurs numériques.", "Check required fields and numeric values.")); return; }
        if (update && !_editingId.HasValue) { Status(T("Sélectionnez un produit à modifier.", "Select a product to update.")); return; }
        using var db = new SqliteConnection(_cs); db.Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = update ? "UPDATE Products SET Name=$n,Sku=$s,Category=$c,Quantity=$q,MinimumQuantity=$m,UnitPrice=$p WHERE Id=$id" :
            "INSERT INTO Products(Name,Sku,Category,Quantity,MinimumQuantity,UnitPrice,CreatedAt) VALUES($n,$s,$c,$q,$m,$p,$d)";
        cmd.Parameters.AddWithValue("$n", _name.Text.Trim()); cmd.Parameters.AddWithValue("$s", _sku.Text.Trim());
        cmd.Parameters.AddWithValue("$c", _category.Text ?? ""); cmd.Parameters.AddWithValue("$q", q); cmd.Parameters.AddWithValue("$m", min); cmd.Parameters.AddWithValue("$p", price);
        if (update) cmd.Parameters.AddWithValue("$id", _editingId!.Value); else cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("O"));
        try { cmd.ExecuteNonQuery(); Clear(); Reload(); Status(T("Produit enregistré.", "Product saved.")); }
        catch (SqliteException) { Status(T("Ce SKU existe déjà.", "This SKU already exists.")); }
    }

    private void Delete()
    {
        if (!_editingId.HasValue) { Status(T("Sélectionnez un produit.", "Select a product first.")); return; }
        using var db = new SqliteConnection(_cs); db.Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "DELETE FROM Movements WHERE ProductId=$id; DELETE FROM Products WHERE Id=$id"; cmd.Parameters.AddWithValue("$id", _editingId.Value);
        cmd.ExecuteNonQuery(); Clear(); Reload(); Status(T("Produit supprimé.", "Product deleted."));
    }

    private void Clear() { _editingId = null; _name.Text = _sku.Text = _category.Text = _qty.Text = _minimum.Text = _price.Text = ""; }

    private void Move(string type)
    {
        if (_moveProduct.SelectedIndex < 0 || _moveProduct.SelectedIndex >= _products.Count || !int.TryParse(_moveQty.Text, out var amount) || amount <= 0)
        { Status(T("Choisissez un produit et une quantité positive.", "Choose a product and a positive quantity.")); return; }
        var p = _products[_moveProduct.SelectedIndex];
        using var db = new SqliteConnection(_cs); db.Open(); using var tx = db.BeginTransaction();
        using var check = db.CreateCommand(); check.Transaction = tx; check.CommandText = "SELECT Quantity FROM Products WHERE Id=$id"; check.Parameters.AddWithValue("$id", p.Id);
        var current = Convert.ToInt32(check.ExecuteScalar());
        if (type == "OUT" && amount > current) { Status(T("Stock insuffisant pour cette sortie.", "Not enough stock for this operation.")); return; }
        using var up = db.CreateCommand(); up.Transaction = tx; up.CommandText = "UPDATE Products SET Quantity=Quantity+$delta WHERE Id=$id";
        up.Parameters.AddWithValue("$delta", type == "IN" ? amount : -amount); up.Parameters.AddWithValue("$id", p.Id); up.ExecuteNonQuery();
        using var log = db.CreateCommand(); log.Transaction = tx; log.CommandText = "INSERT INTO Movements(ProductId,Type,Quantity,Note,CreatedAt) VALUES($id,$type,$q,$note,$date)";
        log.Parameters.AddWithValue("$id", p.Id); log.Parameters.AddWithValue("$type", type); log.Parameters.AddWithValue("$q", amount);
        log.Parameters.AddWithValue("$note", _note.Text ?? ""); log.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("O"));
        log.ExecuteNonQuery(); tx.Commit(); _moveQty.Text = _note.Text = ""; Reload(); Status(T("Mouvement enregistré.", "Movement recorded."));
    }

    private async void ExportCsv()
    {
        var dialog = new SaveFileDialog { Title = T("Exporter le catalogue", "Export catalogue"), InitialFileName = "stockflow-products.csv", DefaultExtension = "csv" };
        var path = await dialog.ShowAsync(this); if (string.IsNullOrWhiteSpace(path)) return;
        using var db = new SqliteConnection(_cs); db.Open(); using var cmd = db.CreateCommand();
        cmd.CommandText = "SELECT Name,Sku,Category,Quantity,MinimumQuantity,UnitPrice FROM Products ORDER BY Name";
        using var r = cmd.ExecuteReader(); var sb = new StringBuilder("Name;SKU;Category;Quantity;Minimum quantity;Unit price\n");
        while (r.Read()) sb.AppendLine(string.Join(";", Enumerable.Range(0, 6).Select(i => "\"" + r.GetValue(i).ToString()!.Replace("\"", "\"\"") + "\"")));
        await File.WriteAllTextAsync(path, sb.ToString(), new UTF8Encoding(true)); Status(T("Export CSV terminé.", "CSV export completed."));
    }

    private void Status(string s) => _status.Text = s;
    private record Product(int Id, string Name, string Sku, string Category, int Quantity, int MinimumQuantity, double UnitPrice)
    { public override string ToString() => $"{Name}  ·  {Sku}  ·  {Quantity}"; }
}