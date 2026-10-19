using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace BanHangQuanTraSua;

public sealed class FrmMain : Form
{
    private readonly DbContextOptions<TraSuaContext> _dbOptions; private readonly PaymentOptions _payment; private readonly HttpClient _http;
    private readonly BindingList<CartLine> _cart = [];
    private readonly ComboBox _saleCategory = new(), _menuCategory = new(); private readonly ListBox _products = new(); private readonly NumericUpDown _qty = new();
    private readonly DataGridView _cartGrid = Grid(), _menuGrid = Grid(), _invoiceGrid = Grid(), _detailGrid = Grid();
    private readonly Label _total = new(), _payAmount = new(), _payInfo = new(), _revenue = new(); private readonly PictureBox _qr = new();
    private readonly TextBox _menuName = new(), _searchMenu = new(); private readonly NumericUpDown _menuPrice = new(); private readonly CheckBox _menuActive = new();
    private readonly DateTimePicker _invoiceDate = new(); private readonly Button _received = new();
    private int? _pendingId; private string _pendingInfo = "";
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public FrmMain(DbContextOptions<TraSuaContext> options, PaymentOptions payment, HttpClient http)
    {
        _dbOptions = options; _payment = payment; _http = http;
        Text = "Bán hàng - Quán trà sữa"; StartPosition = FormStartPosition.CenterScreen; MinimumSize = new Size(1050, 650); Size = new Size(1240, 760); Font = new Font("Segoe UI", 9F);
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var sale = new TabPage("Bán hàng"); var menu = new TabPage("Thực đơn"); var invoices = new TabPage("Hóa đơn");
        tabs.TabPages.AddRange([sale, menu, invoices]); Controls.Add(tabs);
        BuildSale(sale); BuildMenu(menu); BuildInvoices(invoices);
        Shown += async (_, _) =>
        {
            while (true)
            {
                try { await LoadAllAsync(); break; }
                catch (Exception ex)
                {
                    var detail = ex.GetBaseException().Message;
                    Log.Error(ex, "Không tải được dữ liệu lúc mở ứng dụng");
                    var choice = MessageBox.Show(
                        $"Chưa kết nối được cơ sở dữ liệu QL_TraSua.\n\nHãy kiểm tra SQL Server LocalDB đang cài và chạy Database\\QL_TraSua.sql trong SSMS. Nếu LocalDB vừa khởi động, chọn Retry.\n\nChi tiết: {detail}",
                        "Không kết nối được cơ sở dữ liệu", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                    if (choice != DialogResult.Retry) break;
                }
            }
        };
    }

    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersWidth = 28, BackgroundColor = Color.DarkGray };
    private static void AddColumn(DataGridView g, string name, string header, string? format = null) { g.Columns.Add(new DataGridViewTextBoxColumn { Name = name, HeaderText = header, DataPropertyName = name, DefaultCellStyle = { Format = format ?? "" } }); }
    private static Label L(string text) => new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left };
    private static Button B(string text) => new() { Text = text, AutoSize = true, MinimumSize = new Size(88, 32), Margin = new Padding(4) };
    private static TableLayoutPanel Table(int columns, int rows) => new() { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rows, Padding = new Padding(8) };
    private static void AddPercentColumns(TableLayoutPanel t, params float[] widths) { foreach (var w in widths) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, w)); }

    private void BuildSale(TabPage page)
    {
        var layout = Table(3, 1); AddPercentColumns(layout, 26, 43, 31); page.Controls.Add(layout);
        var choose = new GroupBox { Text = "Chọn món", Dock = DockStyle.Fill, Padding = new Padding(10) };
        var chooseTable = Table(1, 6); chooseTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); chooseTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); chooseTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); chooseTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); chooseTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); chooseTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        _saleCategory.DropDownStyle = ComboBoxStyle.DropDownList; _saleCategory.Dock = DockStyle.Fill; _products.Dock = DockStyle.Fill;
        _qty.Minimum = 1; _qty.Maximum = 99; _qty.Value = 2; _qty.Width = 90;
        var qtyRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false }; qtyRow.Controls.Add(L("Số ly:")); qtyRow.Controls.Add(_qty);
        var add = B("Thêm vào đơn"); add.Dock = DockStyle.Fill; add.Click += (_, _) => AddProduct();
        chooseTable.Controls.Add(L("Danh mục"), 0, 0); chooseTable.Controls.Add(_saleCategory, 0, 1); chooseTable.Controls.Add(_products, 0, 2); chooseTable.Controls.Add(qtyRow, 0, 3); chooseTable.Controls.Add(add, 0, 5); choose.Controls.Add(chooseTable);

        var order = new GroupBox { Text = "Đơn hiện tại", Dock = DockStyle.Fill, Padding = new Padding(8) };
        AddColumn(_cartGrid, "TenMon", "Tên món"); AddColumn(_cartGrid, "SoLy", "SL", "N0"); AddColumn(_cartGrid, "DonGia", "Đơn giá", "N0"); AddColumn(_cartGrid, "ThanhTien", "Thành tiền", "N0"); _cartGrid.DataSource = _cart;
        var orderLayout = Table(1, 3); orderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); orderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); orderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        var remove = B("Xóa dòng"); remove.Dock = DockStyle.Fill; remove.Click += (_, _) => { if (_cartGrid.CurrentRow?.DataBoundItem is CartLine line) _cart.Remove(line); UpdateTotal(); };
        _total.AutoSize = false; _total.Dock = DockStyle.Fill; _total.Margin = new Padding(2, 4, 2, 2); _total.TextAlign = ContentAlignment.MiddleCenter; _total.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        orderLayout.Controls.Add(_cartGrid, 0, 0); orderLayout.Controls.Add(remove, 0, 1); orderLayout.Controls.Add(_total, 0, 2); order.Controls.Add(orderLayout);

        var payment = new GroupBox { Text = "Thanh toán", Dock = DockStyle.Fill, Padding = new Padding(8) };
        _qr.Dock = DockStyle.Fill; _qr.SizeMode = PictureBoxSizeMode.Zoom; _qr.BackColor = Color.White; _qr.BorderStyle = BorderStyle.FixedSingle;
        _qr.Paint += (_, e) =>
        {
            if (_qr.Image is not null) return;
            var message = _pendingId is null ? "Mã QR sẽ hiện sau khi thêm món\nvà bấm Thanh toán chuyển khoản" : "Chưa tải được QR.\nDùng thông tin chuyển khoản bên dưới.";
            TextRenderer.DrawText(e.Graphics, message, Font, _qr.ClientRectangle, Color.DimGray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        };
        _payAmount.AutoSize = false; _payAmount.Dock = DockStyle.Fill; _payAmount.TextAlign = ContentAlignment.MiddleLeft;
        _payInfo.AutoSize = false; _payInfo.Dock = DockStyle.Fill; _payInfo.TextAlign = ContentAlignment.MiddleLeft; _payInfo.AutoEllipsis = true; _payInfo.Text = "Thêm món vào đơn để bắt đầu thanh toán.";
        var payButtons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 }; for (int i = 0; i < 3; i++) payButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        var transfer = B("Thanh toán chuyển khoản"); var cash = B("Tiền mặt"); _received.Text = "Đã nhận tiền"; _received.Dock = DockStyle.Fill; _received.BackColor = Color.ForestGreen; _received.ForeColor = Color.White; _received.Enabled = false;
        transfer.Dock = cash.Dock = DockStyle.Fill; transfer.Click += async (_, _) => await BeginTransferAsync(); cash.Click += async (_, _) => await PayCashAsync(); _received.Click += async (_, _) => await ConfirmReceivedAsync();
        payButtons.Controls.Add(transfer, 0, 0); payButtons.Controls.Add(cash, 0, 1); payButtons.Controls.Add(_received, 0, 2);
        var payLayout = Table(1, 5); payLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); payLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); payLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); payLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); payLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        payLayout.Controls.Add(L("Quét mã VietQR để chuyển khoản"), 0, 0); payLayout.Controls.Add(_qr, 0, 1); payLayout.Controls.Add(_payAmount, 0, 2); payLayout.Controls.Add(_payInfo, 0, 3); payLayout.Controls.Add(payButtons, 0, 4); payment.Controls.Add(payLayout);
        layout.Controls.Add(choose, 0, 0); layout.Controls.Add(order, 1, 0); layout.Controls.Add(payment, 2, 0); UpdateTotal();
    }

    private void BuildMenu(TabPage page)
    {
        var root = Table(1, 3); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 115)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); page.Controls.Add(root);
        var editor = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(4) };
        _menuName.Width = 220; _menuPrice.Minimum = 1000; _menuPrice.Maximum = 10000000; _menuPrice.Increment = 1000; _menuPrice.ThousandsSeparator = true; _menuPrice.Width = 130; _menuActive.Text = "Đang bán"; _menuActive.Checked = true; _menuCategory.DropDownStyle = ComboBoxStyle.DropDownList; _menuCategory.Width = 180;
        editor.Controls.Add(L("Tên món")); editor.Controls.Add(_menuName); editor.Controls.Add(L("Giá bán")); editor.Controls.Add(_menuPrice); editor.Controls.Add(L("Danh mục")); editor.Controls.Add(_menuCategory); editor.Controls.Add(_menuActive);
        var add = B("Thêm"); var edit = B("Sửa"); var delete = B("Xóa"); var clear = B("Làm mới");
        add.Click += async (_, _) => await SaveMenuAsync(false); edit.Click += async (_, _) => await SaveMenuAsync(true); delete.Click += async (_, _) => await DeleteMenuAsync(); clear.Click += (_, _) => ClearMenu();
        editor.Controls.AddRange([add, edit, delete, clear]); root.Controls.Add(editor, 0, 0);
        var search = new FlowLayoutPanel { Dock = DockStyle.Fill }; _searchMenu.Width = 260; var find = B("Tìm"); find.Click += async (_, _) => await LoadMenuAsync(); search.Controls.Add(_searchMenu); search.Controls.Add(find); root.Controls.Add(search, 0, 1);
        AddColumn(_menuGrid, "MaMon", "Mã"); AddColumn(_menuGrid, "TenMon", "Tên món"); AddColumn(_menuGrid, "GiaBan", "Đơn giá", "N0"); AddColumn(_menuGrid, "TenDM", "Danh mục"); AddColumn(_menuGrid, "DangBanText", "Trạng thái");
        root.Controls.Add(_menuGrid, 0, 2); _menuGrid.SelectionChanged += (_, _) => FillMenuEditor();
    }

    private void BuildInvoices(TabPage page)
    {
        var root = Table(1, 3); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); page.Controls.Add(root);
        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill }; _invoiceDate.Format = DateTimePickerFormat.Short; var refresh = B("Lọc ngày"); refresh.Click += async (_, _) => await LoadInvoicesAsync(); _revenue.AutoSize = true; filters.Controls.Add(L("Ngày lập:")); filters.Controls.Add(_invoiceDate); filters.Controls.Add(refresh); filters.Controls.Add(_revenue); root.Controls.Add(filters, 0, 0);
        AddColumn(_invoiceGrid, "MaHD", "Mã hóa đơn"); AddColumn(_invoiceGrid, "NgayLap", "Ngày lập", "dd/MM/yyyy HH:mm"); AddColumn(_invoiceGrid, "TongTien", "Tổng tiền", "N0"); AddColumn(_invoiceGrid, "HinhThucTT", "Hình thức"); AddColumn(_invoiceGrid, "DaThanhToanText", "Trạng thái");
        AddColumn(_detailGrid, "TenMon", "Tên món"); AddColumn(_detailGrid, "SoLy", "Số ly", "N0"); AddColumn(_detailGrid, "DonGia", "Đơn giá", "N0"); AddColumn(_detailGrid, "ThanhTien", "Thành tiền", "N0");
        _invoiceGrid.SelectionChanged += async (_, _) => await LoadInvoiceDetailsAsync(); root.Controls.Add(_invoiceGrid, 0, 1); root.Controls.Add(_detailGrid, 0, 2);
    }

    private async Task LoadAllAsync()
    {
        await using var db = new TraSuaContext(_dbOptions);
        // First run: create the SQL schema when the LocalDB is reachable, then seed a small menu.
        // The accompanying SQL script remains available for Database First coursework setup.
        await db.Database.EnsureCreatedAsync();
        var starterCategories = new[] { "Trà sữa", "Trà trái cây", "Topping", "Đồ ăn vặt" };
        var existingCategories = await db.DanhMucMons.Select(x => x.TenDM).ToListAsync();
        foreach (var name in starterCategories.Where(name => !existingCategories.Contains(name))) db.DanhMucMons.Add(new DanhMucMon { TenDM = name });
        if (db.ChangeTracker.HasChanges()) await db.SaveChangesAsync();
        if (!await db.MonUongs.AnyAsync())
        {
            var categories = await db.DanhMucMons.ToDictionaryAsync(x => x.TenDM, x => x.MaDM);
            db.MonUongs.AddRange(
                new MonUong { TenMon = "Trà sữa trân châu", GiaBan = 35000, DangBan = true, MaDM = categories["Trà sữa"] },
                new MonUong { TenMon = "Trà sữa matcha", GiaBan = 40000, DangBan = true, MaDM = categories["Trà sữa"] },
                new MonUong { TenMon = "Hồng trà sữa", GiaBan = 32000, DangBan = true, MaDM = categories["Trà sữa"] },
                new MonUong { TenMon = "Trà sữa khoai môn", GiaBan = 38000, DangBan = true, MaDM = categories["Trà sữa"] },
                new MonUong { TenMon = "Trà đào", GiaBan = 35000, DangBan = true, MaDM = categories["Trà trái cây"] },
                new MonUong { TenMon = "Trà vải", GiaBan = 35000, DangBan = true, MaDM = categories["Trà trái cây"] },
                new MonUong { TenMon = "Trà chanh dây", GiaBan = 30000, DangBan = true, MaDM = categories["Trà trái cây"] },
                new MonUong { TenMon = "Trân châu đen", GiaBan = 5000, DangBan = true, MaDM = categories["Topping"] },
                new MonUong { TenMon = "Trân châu trắng", GiaBan = 6000, DangBan = true, MaDM = categories["Topping"] },
                new MonUong { TenMon = "Pudding trứng", GiaBan = 7000, DangBan = true, MaDM = categories["Topping"] },
                new MonUong { TenMon = "Bánh flan", GiaBan = 10000, DangBan = true, MaDM = categories["Đồ ăn vặt"] },
                new MonUong { TenMon = "Khoai tây chiên", GiaBan = 25000, DangBan = true, MaDM = categories["Đồ ăn vặt"] });
            await db.SaveChangesAsync();
        }
        var cats = await db.DanhMucMons.AsNoTracking().OrderBy(x => x.TenDM).ToListAsync();
        _saleCategory.DataSource = cats; _saleCategory.DisplayMember = "TenDM"; _saleCategory.ValueMember = "MaDM";
        _menuCategory.DataSource = cats.ToList(); _menuCategory.DisplayMember = "TenDM"; _menuCategory.ValueMember = "MaDM";
        _saleCategory.SelectedIndexChanged += async (_, _) => await LoadProductsAsync();
        await LoadProductsAsync(); await LoadMenuAsync(); await LoadInvoicesAsync();
    }
    private async Task LoadProductsAsync()
    {
        if (_saleCategory.SelectedValue is not int id) return;
        try { await using var db = new TraSuaContext(_dbOptions); var data = await db.MonUongs.AsNoTracking().Where(x => x.DangBan && x.MaDM == id).OrderBy(x => x.TenMon).Select(x => new ProductChoice(x.MaMon, x.TenMon, x.GiaBan)).ToListAsync(); _products.DataSource = data; _products.DisplayMember = "Display"; }
        catch (Exception ex) { ShowError(ex, "Không tải được danh sách món."); }
    }
    private sealed record ProductChoice(int Id, string Name, decimal Price) { public string Display => $"{Name} - {Price:N0} đ"; }
    private void AddProduct()
    {
        if (_products.SelectedItem is not ProductChoice p) { MessageBox.Show("Hãy chọn món cần thêm."); return; }
        var line = _cart.FirstOrDefault(x => x.MaMon == p.Id);
        if (line is null) _cart.Add(new CartLine { MaMon = p.Id, TenMon = p.Name, DonGia = p.Price, SoLy = (int)_qty.Value }); else line.SoLy += (int)_qty.Value;
        _cart.ResetBindings(); UpdateTotal();
    }
    private void UpdateTotal() { _total.Text = $"Tổng tiền: {_cart.Sum(x => x.ThanhTien):N0} đ"; _payAmount.Text = $"Số tiền: {_cart.Sum(x => x.ThanhTien):N0} đ"; }

    private async Task BeginTransferAsync()
    {
        if (!ValidateCart()) return;
        try
        {
            await using var db = new TraSuaContext(_dbOptions); var invoice = MakeInvoice(db, "Chuyển khoản", false); db.HoaDons.Add(invoice); await db.SaveChangesAsync(); _pendingId = invoice.MaHD; _pendingInfo = $"TRASUA HD{invoice.MaHD}"; _received.Enabled = true;
            var url = $"https://img.vietqr.io/image/{Uri.EscapeDataString(_payment.BankId)}-{Uri.EscapeDataString(_payment.AccountNo)}-{Uri.EscapeDataString(_payment.Template)}.png?amount={invoice.TongTien.ToString("0", CultureInfo.InvariantCulture)}&addInfo={Uri.EscapeDataString(_pendingInfo)}&accountName={Uri.EscapeDataString(_payment.AccountName)}";
            using var response = await _http.GetAsync(url); response.EnsureSuccessStatusCode(); await using var stream = await response.Content.ReadAsStreamAsync(); using var temp = new MemoryStream(); await stream.CopyToAsync(temp); temp.Position = 0; var img = Image.FromStream(temp); var copy = new Bitmap(img); img.Dispose(); var old = _qr.Image; _qr.Image = copy; old?.Dispose();
            _payAmount.Text = $"Số tiền: {invoice.TongTien:N0} đ"; _payInfo.Text = $"Nội dung: {_pendingInfo}"; Log.Information("Đã tạo VietQR cho hóa đơn {InvoiceId}, số tiền {Amount}", invoice.MaHD, invoice.TongTien);
            await LoadInvoicesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Không tải được VietQR"); _qr.Image?.Dispose(); _qr.Image = null;
            _payInfo.Text = $"Nội dung: {_pendingInfo}\nTK {_payment.AccountNo} - {_payment.AccountName}";
            MessageBox.Show($"Không tải được mã QR. Hóa đơn {_pendingId} đã được lưu ở trạng thái chưa thanh toán. Bạn có thể chuyển khoản thủ công theo thông tin bên phải hoặc xác nhận Tiền mặt.\n\n{ex.Message}", "Không tải được VietQR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            await LoadInvoicesAsync();
        }
    }
    private HoaDon MakeInvoice(TraSuaContext db, string method, bool paid) => new() { NgayLap = DateTime.Now, TongTien = _cart.Sum(x => x.ThanhTien), HinhThucTT = method, DaThanhToan = paid, ChiTiets = _cart.Select(x => new ChiTietHoaDon { MaMon = x.MaMon, SoLy = x.SoLy, DonGia = x.DonGia }).ToList() };
    private bool ValidateCart() { if (_cart.Count == 0) { MessageBox.Show("Thêm ít nhất một món vào đơn trước."); return false; } if (string.IsNullOrWhiteSpace(_payment.BankId) || string.IsNullOrWhiteSpace(_payment.AccountNo)) { MessageBox.Show("Hãy điền BankId và AccountNo trong appsettings.json."); return false; } return true; }
    private async Task PayCashAsync()
    {
        if (!ValidateCart()) return;
        try { await using var db = new TraSuaContext(_dbOptions); db.HoaDons.Add(MakeInvoice(db, "Tiền mặt", true)); await db.SaveChangesAsync(); Log.Information("Đã lưu hóa đơn tiền mặt tổng {Amount}", _cart.Sum(x => x.ThanhTien)); ClearCart(); await LoadInvoicesAsync(); MessageBox.Show("Đã lưu hóa đơn tiền mặt.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { ShowError(ex, "Không lưu được hóa đơn tiền mặt."); }
    }
    private async Task ConfirmReceivedAsync()
    {
        if (_pendingId is not int id) return;
        try { await using var db = new TraSuaContext(_dbOptions); var invoice = await db.HoaDons.FindAsync(id); if (invoice is null) throw new InvalidOperationException("Không tìm thấy hóa đơn chờ thanh toán."); invoice.DaThanhToan = true; await db.SaveChangesAsync(); Log.Information("Đã xác nhận thanh toán hóa đơn {InvoiceId}", id); ClearCart(); await LoadInvoicesAsync(); MessageBox.Show("Đã cập nhật hóa đơn: thanh toán thành công.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { ShowError(ex, "Không cập nhật được trạng thái thanh toán."); }
    }
    private void ClearCart() { _cart.Clear(); _cart.ResetBindings(); _pendingId = null; _pendingInfo = ""; _received.Enabled = false; _qr.Image?.Dispose(); _qr.Image = null; _payInfo.Text = ""; UpdateTotal(); }

    private async Task LoadMenuAsync()
    {
        try { await using var db = new TraSuaContext(_dbOptions); var text = _searchMenu.Text.Trim(); _menuGrid.DataSource = await db.MonUongs.AsNoTracking().Include(x => x.DanhMuc).Where(x => text == "" || x.TenMon.Contains(text)).OrderBy(x => x.MaMon).Select(x => new { x.MaMon, x.TenMon, x.GiaBan, TenDM = x.DanhMuc!.TenDM, DangBanText = x.DangBan ? "Đang bán" : "Ngừng bán", x.MaDM, x.DangBan }).ToListAsync(); }
        catch (Exception ex) { ShowError(ex, "Không tải được thực đơn."); }
    }
    private void FillMenuEditor()
    {
        if (_menuGrid.CurrentRow?.DataBoundItem is not null) { dynamic row = _menuGrid.CurrentRow.DataBoundItem; try { _menuName.Text = row.TenMon; _menuPrice.Value = Math.Clamp((decimal)row.GiaBan, _menuPrice.Minimum, _menuPrice.Maximum); _menuActive.Checked = row.DangBan; _menuCategory.SelectedValue = (int)row.MaDM; } catch { } }
    }
    private async Task SaveMenuAsync(bool edit)
    {
        if (string.IsNullOrWhiteSpace(_menuName.Text) || _menuCategory.SelectedValue is not int category) { MessageBox.Show("Nhập tên món và chọn danh mục."); return; }
        try
        {
            await using var db = new TraSuaContext(_dbOptions); MonUong item;
            if (edit) { if (_menuGrid.CurrentRow?.Cells["MaMon"].Value is not int id) { MessageBox.Show("Chọn món cần sửa."); return; } item = await db.MonUongs.FindAsync(id) ?? throw new InvalidOperationException("Không tìm thấy món."); }
            else { item = new MonUong(); db.MonUongs.Add(item); }
            item.TenMon = _menuName.Text.Trim(); item.GiaBan = _menuPrice.Value; item.DangBan = _menuActive.Checked; item.MaDM = category; await db.SaveChangesAsync(); await LoadMenuAsync(); await LoadProductsAsync(); ClearMenu();
        }
        catch (Exception ex) { ShowError(ex, "Không lưu được món."); }
    }
    private async Task DeleteMenuAsync()
    {
        if (_menuGrid.CurrentRow?.Cells["MaMon"].Value is not int id) { MessageBox.Show("Chọn món cần xóa."); return; }
        if (MessageBox.Show("Xóa món đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try { await using var db = new TraSuaContext(_dbOptions); var item = await db.MonUongs.FindAsync(id); if (item is not null) { db.MonUongs.Remove(item); await db.SaveChangesAsync(); } await LoadMenuAsync(); await LoadProductsAsync(); ClearMenu(); }
        catch (Exception ex) { ShowError(ex, "Món đã nằm trong hóa đơn nên không thể xóa. Hãy bỏ chọn Đang bán để ẩn món."); }
    }
    private void ClearMenu() { _menuName.Clear(); _menuPrice.Value = 1000; _menuActive.Checked = true; if (_menuCategory.Items.Count > 0) _menuCategory.SelectedIndex = 0; }
    private async Task LoadInvoicesAsync()
    {
        try
        {
            var day = _invoiceDate.Value.Date; await using var db = new TraSuaContext(_dbOptions);
            var invoices = await db.HoaDons.AsNoTracking().Where(x => x.NgayLap >= day && x.NgayLap < day.AddDays(1)).OrderByDescending(x => x.NgayLap).Select(x => new { x.MaHD, x.NgayLap, x.TongTien, x.HinhThucTT, DaThanhToanText = x.DaThanhToan ? "Đã thanh toán" : "Chưa thanh toán" }).ToListAsync();
            _invoiceGrid.DataSource = invoices; _revenue.Text = $"Tổng thu ngày: {invoices.Where(x => x.DaThanhToanText == "Đã thanh toán").Sum(x => x.TongTien):N0} đ";
        }
        catch (Exception ex) { ShowError(ex, "Không tải được danh sách hóa đơn."); }
    }
    private async Task LoadInvoiceDetailsAsync()
    {
        if (_invoiceGrid.CurrentRow?.Cells["MaHD"].Value is not int id) { _detailGrid.DataSource = null; return; }
        try { await using var db = new TraSuaContext(_dbOptions); _detailGrid.DataSource = await db.ChiTietHoaDons.AsNoTracking().Where(x => x.MaHD == id).Include(x => x.MonUong).Select(x => new { TenMon = x.MonUong!.TenMon, x.SoLy, x.DonGia, ThanhTien = x.SoLy * x.DonGia }).ToListAsync(); }
        catch (Exception ex) { ShowError(ex, "Không tải được chi tiết hóa đơn."); }
    }
    private static void ShowError(Exception ex, string message) { Log.Error(ex, "{Message}", message); MessageBox.Show($"{message}\n\nChi tiết: {ex.GetBaseException().Message}", "Có lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
}
