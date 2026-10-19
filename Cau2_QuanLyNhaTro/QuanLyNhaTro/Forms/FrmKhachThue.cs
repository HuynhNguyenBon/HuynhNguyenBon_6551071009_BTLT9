using Microsoft.EntityFrameworkCore;
using Serilog;
using QuanLyNhaTro.Data;
using QuanLyNhaTro.Models;
using QuanLyNhaTro.Services;

namespace QuanLyNhaTro.Forms;

public sealed class FrmKhachThue : Form
{
    private readonly DbContextOptions<QLNhaTroContext> _dbOptions;
    private readonly ProvinceApiClient _api;
    private readonly ILogger _log = Log.ForContext<FrmKhachThue>();
    private readonly TextBox txtHoTen = new();
    private readonly RadioButton rdbNam = new() { Text = "Nam", Checked = true, AutoSize = true };
    private readonly RadioButton rdbNu = new() { Text = "Nữ", AutoSize = true };
    private readonly DateTimePicker dtpNgaySinh = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Value = new DateTime(2004, 5, 12) };
    private readonly TextBox txtSoDienThoai = new();
    private readonly ComboBox cboPhong = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboTinhThanh = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboPhuongXa = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly CheckBox chkTamTru = new() { Text = "Đã đăng ký tạm trú", Checked = true, AutoSize = true };
    private readonly TextBox txtTimKiem = new() { PlaceholderText = "Tìm theo tên hoặc SĐT" };
    private readonly ComboBox cboLocPhong = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboLocTinh = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView dgv = new();
    private readonly ToolStripStatusLabel status = new("Đang tải tỉnh/thành...");
    private int? _selectedId;
    private bool _loadingProvinces;
    private List<ProvinceDto> _provinces = [];

    public FrmKhachThue(DbContextOptions<QLNhaTroContext> dbOptions, ProvinceApiClient api)
    {
        _dbOptions = dbOptions;
        _api = api;
        Text = "Quản lý nhà trọ - Khách thuê";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 650);
        MinimumSize = new Size(1000, 580);
        Font = new Font("Segoe UI", 9F);
        BuildLayout();
        Load += async (_, _) => await LoadInitialAsync();
        cboTinhThanh.SelectedValueChanged += async (_, _) => await ProvinceChangedAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2, RowCount = 1 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var left = new GroupBox { Text = "Thông tin khách thuê", Dock = DockStyle.Fill, Padding = new Padding(10) };
        root.Controls.Add(left, 0, 0);
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 10, Padding = new Padding(4) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 8; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 1 ? 38 : 42));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        left.Controls.Add(fields);
        AddField(fields, 0, "Họ tên", txtHoTen);
        var gender = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        gender.Controls.AddRange([rdbNam, rdbNu]);
        AddField(fields, 1, "Giới tính", gender);
        AddField(fields, 2, "Ngày sinh", dtpNgaySinh);
        AddField(fields, 3, "Số điện thoại", txtSoDienThoai);
        AddField(fields, 4, "Phòng", cboPhong);
        AddField(fields, 5, "Tỉnh/Thành", cboTinhThanh);
        AddField(fields, 6, "Phường/Xã", cboPhuongXa);
        fields.Controls.Add(chkTamTru, 0, 7); fields.SetColumnSpan(chkTamTru, 2);

        var buttons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty, Padding = Padding.Empty };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var add = MakeButton("Thêm", async () => await SaveAsync(false));
        var edit = MakeButton("Sửa", async () => await SaveAsync(true));
        var delete = MakeButton("Xóa", async () => await DeleteAsync());
        var clear = MakeButton("Làm mới", () => { ClearForm(); return Task.CompletedTask; });
        foreach (var b in new[] { add, edit, delete, clear }) { b.Dock = DockStyle.Fill; b.Margin = new Padding(2); }
        buttons.Controls.Add(add, 0, 0); buttons.Controls.Add(edit, 1, 0); buttons.Controls.Add(delete, 2, 0); buttons.Controls.Add(clear, 3, 0);
        fields.Controls.Add(buttons, 0, 9); fields.SetColumnSpan(buttons, 2);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(12, 0, 0, 0) };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(right, 1, 0);
        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(0, 4, 0, 0),
            Margin = Padding.Empty
        };
        txtTimKiem.Width = 190;
        cboLocPhong.Width = 92;
        cboLocTinh.Width = 112;
        filters.Controls.Add(txtTimKiem);
        filters.Controls.Add(new Label { Text = "Phòng:", AutoSize = true, Padding = new Padding(4, 5, 0, 0) });
        filters.Controls.Add(cboLocPhong);
        filters.Controls.Add(new Label { Text = "Tỉnh:", AutoSize = true, Padding = new Padding(4, 5, 0, 0) });
        filters.Controls.Add(cboLocTinh);
        var searchButton = MakeButton("Tìm", async () => await SearchAsync());
        searchButton.Width = 58;
        filters.Controls.Add(searchButton);
        var clearFilterButton = MakeButton("Bỏ lọc", async () =>
        {
            txtTimKiem.Clear();
            if (cboLocPhong.Items.Count > 0) cboLocPhong.SelectedIndex = 0;
            if (cboLocTinh.Items.Count > 0) cboLocTinh.SelectedIndex = 0;
            await LoadGridAsync();
        });
        clearFilterButton.Width = 68;
        filters.Controls.Add(clearFilterButton);
        right.Controls.Add(filters, 0, 0);

        dgv.Dock = DockStyle.Fill; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.MultiSelect = false;
        dgv.AutoGenerateColumns = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowHeadersWidth = 25;
        AddTextColumn("HoTen", "Họ tên", 1.25f);
        AddTextColumn("SoDienThoai", "SĐT", 0.9f);
        AddTextColumn("TenPhong", "Phòng", 0.65f);
        AddTextColumn("GiaThue", "Giá thuê", 0.95f, "N0 đ");
        AddTextColumn("QueQuan", "Quê quán", 1.3f);
        dgv.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "DaDangKyTamTru", HeaderText = "Tạm trú", FillWeight = 0.65f, Name = "colTamTru" });
        dgv.CellClick += (_, _) => SelectCurrentRow();
        right.Controls.Add(dgv, 0, 1);

        var strip = new StatusStrip(); strip.Items.Add(status); Controls.Add(strip);
        var errorButton = MakeButton("Gây lỗi thử", () => Task.FromException(new InvalidOperationException("Lỗi thử nghiệm theo yêu cầu bài tập.")));
        errorButton.Name = "btnGayLoiThu"; errorButton.Visible = false; Controls.Add(errorButton);
    }

    private static void AddField(TableLayoutPanel panel, int row, string caption, Control control)
    {
        panel.Controls.Add(new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        control.Dock = DockStyle.Fill; control.Margin = new Padding(3, 5, 3, 5);
        panel.Controls.Add(control, 1, row);
    }

    private Button MakeButton(string text, Func<Task> action)
    {
        var b = new Button { Text = text, AutoSize = false, Height = 30, Margin = new Padding(3) };
        b.Click += async (_, _) => { try { await action(); } catch (Exception ex) { await HandleErrorAsync(ex); } };
        return b;
    }

    private void AddTextColumn(string property, string header, float weight, string? format = null)
    {
        dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "col" + property, DataPropertyName = property, HeaderText = header, FillWeight = weight, DefaultCellStyle = { Format = format ?? "" } });
    }

    private async Task LoadInitialAsync()
    {
        while (true)
        {
            try
            {
                await InitializeDatabaseAsync();
                await LoadRoomsAsync();
                await LoadGridAsync();
                break;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Không khởi tạo hoặc tải được dữ liệu khách thuê");
                var detail = ex.GetBaseException().Message;
                var result = MessageBox.Show(this,
                    $"Không tải được dữ liệu nhà trọ. Kiểm tra SQL Server LocalDB và cơ sở dữ liệu QL_NhaTro.\n\nChi tiết: {detail}\n\nChọn Retry để thử lại hoặc Cancel để tiếp tục mở form.",
                    "Lỗi cơ sở dữ liệu", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                if (result != DialogResult.Retry)
                {
                    status.Text = "Chưa kết nối được cơ sở dữ liệu QL_NhaTro";
                    return;
                }
            }
        }
        await LoadProvincesAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        await using var db = new QLNhaTroContext(_dbOptions);
        // Allows the coursework project to start on a fresh LocalDB; the SQL script remains the Database First source.
        await db.Database.EnsureCreatedAsync();
        if (!await db.PhongTros.AnyAsync())
        {
            db.PhongTros.AddRange(
                new PhongTro { TenPhong = "P01", GiaThue = 1500000, TinhTrang = "Trống" },
                new PhongTro { TenPhong = "P02", GiaThue = 1600000, TinhTrang = "Trống" },
                new PhongTro { TenPhong = "P03", GiaThue = 1800000, TinhTrang = "Trống" },
                new PhongTro { TenPhong = "P04", GiaThue = 2000000, TinhTrang = "Trống" },
                new PhongTro { TenPhong = "P05", GiaThue = 1700000, TinhTrang = "Trống" },
                new PhongTro { TenPhong = "P06", GiaThue = 2200000, TinhTrang = "Trống" });
            await db.SaveChangesAsync();
        }
        if (!await db.KhachThues.AnyAsync())
        {
            db.KhachThues.AddRange(
                new KhachThue { HoTen = "Nguyễn Văn An", GioiTinh = "Nam", NgaySinh = new DateTime(2004, 5, 12), SoDienThoai = "0912345678", MaTinh = 1, TenTinh = "Thành phố Hà Nội", MaPhuongXa = 1, TenPhuongXa = "Phường Ba Đình", DaDangKyTamTru = true, MaPhong = 3 },
                new KhachThue { HoTen = "Trần Thị Bình", GioiTinh = "Nữ", NgaySinh = new DateTime(2002, 9, 18), SoDienThoai = "0876543210", MaTinh = 1, TenTinh = "Thành phố Hà Nội", MaPhuongXa = 2, TenPhuongXa = "Phường Ngọc Hà", DaDangKyTamTru = false, MaPhong = 1 },
                new KhachThue { HoTen = "Lê Văn Cường", GioiTinh = "Nam", NgaySinh = new DateTime(2001, 3, 21), SoDienThoai = "0901234567", MaTinh = 38, TenTinh = "Tỉnh Thanh Hóa", TenPhuongXa = "Phường Đông Sơn", DaDangKyTamTru = false, MaPhong = 2 },
                new KhachThue { HoTen = "Phạm Minh Đức", GioiTinh = "Nam", NgaySinh = new DateTime(1999, 11, 2), SoDienThoai = "0987654321", MaTinh = 1, TenTinh = "Thành phố Hà Nội", MaPhuongXa = 3, TenPhuongXa = "Phường Giảng Võ", DaDangKyTamTru = true, MaPhong = 4 });
            await db.SaveChangesAsync();
        }
        await UpdateRoomStatesAsync(db);
        await db.SaveChangesAsync();
    }

    private async Task LoadRoomsAsync()
    {
        await using var db = new QLNhaTroContext(_dbOptions);
        var rooms = await db.PhongTros.OrderBy(x => x.TenPhong).ToListAsync();
        cboPhong.DisplayMember = nameof(PhongTro.TenPhong); cboPhong.ValueMember = nameof(PhongTro.MaPhong);
        cboPhong.DataSource = rooms.Where(r => r.TinhTrang == "Trống").ToList();
        var filters = new List<RoomChoice> { new(null, "Tất cả") };
        filters.AddRange(rooms.Select(r => new RoomChoice(r.MaPhong, r.TenPhong)));
        cboLocPhong.DisplayMember = nameof(RoomChoice.Name); cboLocPhong.ValueMember = nameof(RoomChoice.Id); cboLocPhong.DataSource = filters;
        if (rooms.Count > 0) cboPhong.SelectedIndex = 0;
    }

    private async Task LoadProvincesAsync()
    {
        try
        {
            _loadingProvinces = true; cboTinhThanh.Enabled = false; cboTinhThanh.Items.Clear(); cboTinhThanh.Items.Add("Đang tải…"); cboTinhThanh.SelectedIndex = 0;
            _provinces = await _api.GetProvincesAsync() ?? [];
            cboTinhThanh.DataSource = _provinces; cboTinhThanh.DisplayMember = nameof(ProvinceDto.Name); cboTinhThanh.ValueMember = nameof(ProvinceDto.Code);
            var filter = new List<ProvinceChoice> { new(null, "Tất cả") };
            filter.AddRange(_provinces.Select(p => new ProvinceChoice(p.Code, p.Name)));
            cboLocTinh.DisplayMember = nameof(ProvinceChoice.Name); cboLocTinh.ValueMember = nameof(ProvinceChoice.Id); cboLocTinh.DataSource = filter;
            status.Text = $"Đã tải {_provinces.Count} tỉnh/thành từ provinces.open-api.vn";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _log.Warning(ex, "Không tải được danh sách tỉnh/thành từ API");
            cboTinhThanh.DataSource = null; cboTinhThanh.Items.Clear(); cboTinhThanh.Items.Add("Không tải được tỉnh/thành"); cboTinhThanh.SelectedIndex = 0;
            cboTinhThanh.Enabled = true;
            status.Text = "Không tải được danh sách địa chỉ, vui lòng kiểm tra mạng";
            MessageBox.Show(this, "Không tải được danh sách địa chỉ, vui lòng kiểm tra mạng", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex) { await HandleErrorAsync(ex); }
        finally { _loadingProvinces = false; cboTinhThanh.Enabled = true; }
    }

    private async Task ProvinceChangedAsync()
    {
        if (_loadingProvinces || cboTinhThanh.SelectedItem is not ProvinceDto p) return;
        try
        {
            cboPhuongXa.Enabled = false; cboPhuongXa.Items.Clear(); cboPhuongXa.Items.Add("Đang tải…"); cboPhuongXa.SelectedIndex = 0;
            var wards = await _api.GetWardsAsync(p.Code);
            cboPhuongXa.DataSource = wards; cboPhuongXa.DisplayMember = nameof(WardDto.Name); cboPhuongXa.ValueMember = nameof(WardDto.Code);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _log.Warning(ex, "Không tải được phường/xã cho tỉnh {ProvinceCode}", p.Code);
            cboPhuongXa.DataSource = null; cboPhuongXa.Items.Clear(); cboPhuongXa.Text = "Nhập phường/xã thủ công";
            cboPhuongXa.DropDownStyle = ComboBoxStyle.DropDown;
            MessageBox.Show(this, "Không tải được danh sách địa chỉ, vui lòng kiểm tra mạng", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex) { await HandleErrorAsync(ex); }
        finally { cboPhuongXa.Enabled = true; }
    }

    private async Task LoadGridAsync()
    {
        await using var db = new QLNhaTroContext(_dbOptions);
        var rows = await db.KhachThues.AsNoTracking().Include(k => k.MaPhongNavigation)
            .Select(k => new TenantRow { MaKhach = k.MaKhach, HoTen = k.HoTen, SoDienThoai = k.SoDienThoai,
                TenPhong = k.MaPhongNavigation.TenPhong, GiaThue = k.MaPhongNavigation.GiaThue,
                QueQuan = k.TenPhuongXa + " - " + k.TenTinh, DaDangKyTamTru = k.DaDangKyTamTru,
                GioiTinh = k.GioiTinh, NgaySinh = k.NgaySinh, MaTinh = k.MaTinh, TenTinh = k.TenTinh,
                MaPhuongXa = k.MaPhuongXa, TenPhuongXa = k.TenPhuongXa, MaPhong = k.MaPhong }).ToListAsync();
        dgv.DataSource = rows;
    }

    private async Task SearchAsync()
    {
        try
        {
            await using var db = new QLNhaTroContext(_dbOptions);
            var query = db.KhachThues.AsNoTracking().Include(k => k.MaPhongNavigation).AsQueryable();
            var text = txtTimKiem.Text.Trim();
            if (text.Length > 0) query = query.Where(k => k.HoTen.Contains(text) || k.SoDienThoai.Contains(text));
            if (cboLocPhong.SelectedValue is int roomId) query = query.Where(k => k.MaPhong == roomId);
            if (cboLocTinh.SelectedValue is int provinceId) query = query.Where(k => k.MaTinh == provinceId);
            var rows = await query.Select(k => new TenantRow { MaKhach = k.MaKhach, HoTen = k.HoTen,
                SoDienThoai = k.SoDienThoai, TenPhong = k.MaPhongNavigation.TenPhong,
                GiaThue = k.MaPhongNavigation.GiaThue, QueQuan = k.TenPhuongXa + " - " + k.TenTinh,
                DaDangKyTamTru = k.DaDangKyTamTru, GioiTinh = k.GioiTinh, NgaySinh = k.NgaySinh,
                MaTinh = k.MaTinh, TenTinh = k.TenTinh, MaPhuongXa = k.MaPhuongXa,
                TenPhuongXa = k.TenPhuongXa, MaPhong = k.MaPhong }).ToListAsync();
            dgv.DataSource = rows;
        }
        catch (Exception ex) { await HandleErrorAsync(ex); }
    }

    private void SelectCurrentRow()
    {
        if (dgv.CurrentRow?.DataBoundItem is not TenantRow r) return;
        _selectedId = r.MaKhach; txtHoTen.Text = r.HoTen; txtSoDienThoai.Text = r.SoDienThoai;
        rdbNam.Checked = r.GioiTinh == "Nam"; rdbNu.Checked = r.GioiTinh == "Nữ";
        dtpNgaySinh.Value = r.NgaySinh; chkTamTru.Checked = r.DaDangKyTamTru;
        if (cboPhong.SelectedValue is not int currentRoom || currentRoom != r.MaPhong)
        {
            var choices = (cboPhong.DataSource as List<PhongTro> ?? []).ToList();
            if (!choices.Any(x => x.MaPhong == r.MaPhong))
                choices.Add(new PhongTro { MaPhong = r.MaPhong, TenPhong = r.TenPhong, GiaThue = r.GiaThue, TinhTrang = "Đang thuê" });
            cboPhong.DataSource = choices;
        }
        cboPhong.SelectedValue = r.MaPhong;
        if (r.MaTinh.HasValue) cboTinhThanh.SelectedValue = r.MaTinh.Value;
        cboPhuongXa.Text = r.TenPhuongXa;
    }

    private async Task SaveAsync(bool editing)
    {
        if (string.IsNullOrWhiteSpace(txtHoTen.Text)) { MessageBox.Show(this, "Vui lòng nhập họ tên."); return; }
        if (string.IsNullOrWhiteSpace(txtSoDienThoai.Text)) { MessageBox.Show(this, "Vui lòng nhập số điện thoại."); return; }
        if (dtpNgaySinh.Value.Date > DateTime.Today) { MessageBox.Show(this, "Ngày sinh không được lớn hơn hôm nay."); return; }
        if (cboPhong.SelectedValue is not int roomId) { MessageBox.Show(this, "Vui lòng chọn phòng."); return; }
        var province = cboTinhThanh.SelectedItem as ProvinceDto;
        var ward = cboPhuongXa.SelectedItem as WardDto;
        var wardName = ward?.Name ?? cboPhuongXa.Text.Trim();
        if (string.IsNullOrWhiteSpace(wardName)) { MessageBox.Show(this, "Vui lòng chọn hoặc nhập phường/xã."); return; }
        if (editing && _selectedId is null) { MessageBox.Show(this, "Hãy chọn khách thuê trong bảng trước."); return; }
        try
        {
            await using var db = new QLNhaTroContext(_dbOptions);
            var selectedRoom = await db.PhongTros.FindAsync(roomId);
            if (selectedRoom is null || (!editing && selectedRoom.TinhTrang != "Trống"))
            {
                MessageBox.Show(this, "Khi thêm mới, chỉ chọn phòng đang trống.", "Phòng không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                await LoadRoomsAsync();
                return;
            }
            KhachThue? tenant = editing ? await db.KhachThues.FindAsync(_selectedId) : new KhachThue();
            if (tenant is null) { MessageBox.Show(this, "Không tìm thấy khách thuê."); return; }
            tenant.HoTen = txtHoTen.Text.Trim(); tenant.SoDienThoai = txtSoDienThoai.Text.Trim();
            tenant.GioiTinh = rdbNam.Checked ? "Nam" : "Nữ"; tenant.NgaySinh = dtpNgaySinh.Value.Date;
            tenant.MaPhong = roomId; tenant.MaTinh = province?.Code; tenant.TenTinh = province?.Name ?? "Chưa xác định";
            tenant.MaPhuongXa = ward?.Code; tenant.TenPhuongXa = wardName; tenant.DaDangKyTamTru = chkTamTru.Checked;
            if (!editing) db.KhachThues.Add(tenant);
            await db.SaveChangesAsync();
            await UpdateRoomStatesAsync(db);
            await db.SaveChangesAsync();
            _log.Information("{Action} khách thuê MaKhach={MaKhach}", editing ? "Sửa" : "Thêm", tenant.MaKhach);
            await LoadRoomsAsync(); await LoadGridAsync(); ClearForm();
        }
        catch (Exception ex) { _log.Error(ex, "Lỗi CSDL khi lưu khách thuê"); await HandleErrorAsync(ex); }
    }

    private async Task DeleteAsync()
    {
        if (_selectedId is null) { MessageBox.Show(this, "Hãy chọn khách thuê cần xóa."); return; }
        if (MessageBox.Show(this, "Bạn có chắc muốn xóa khách thuê đang chọn?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            await using var db = new QLNhaTroContext(_dbOptions);
            var tenant = await db.KhachThues.FindAsync(_selectedId.Value);
            if (tenant is null) return;
            db.KhachThues.Remove(tenant);
            await db.SaveChangesAsync();
            await UpdateRoomStatesAsync(db);
            await db.SaveChangesAsync();
            _log.Information("Xóa khách thuê MaKhach={MaKhach}", tenant.MaKhach);
            await LoadRoomsAsync(); await LoadGridAsync(); ClearForm();
        }
        catch (Exception ex) { _log.Error(ex, "Lỗi CSDL khi xóa khách thuê"); await HandleErrorAsync(ex); }
    }

    private static async Task UpdateRoomStatesAsync(QLNhaTroContext db)
    {
        var rooms = await db.PhongTros.ToListAsync();
        foreach (var room in rooms)
        {
            var occupied = await db.KhachThues.AnyAsync(k => k.MaPhong == room.MaPhong);
            room.TinhTrang = occupied ? "Đang thuê" : "Trống";
        }
    }

    private async Task HandleErrorAsync(Exception ex)
    {
        _log.Error(ex, "Thao tác thất bại");
        MessageBox.Show(this, $"Thao tác không thành công. Chi tiết đã được ghi vào Logs.\n\nLỗi: {ex.GetBaseException().Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        await Task.CompletedTask;
    }

    private void ClearForm()
    {
        _selectedId = null; txtHoTen.Clear(); txtSoDienThoai.Clear(); rdbNam.Checked = true;
        dtpNgaySinh.Value = new DateTime(2004, 5, 12); chkTamTru.Checked = true;
        if (cboPhong.DataSource is List<PhongTro> choices)
        {
            cboPhong.DataSource = choices.Where(x => x.TinhTrang == "Trống").ToList();
            if (cboPhong.Items.Count > 0) cboPhong.SelectedIndex = 0;
        }
        cboPhuongXa.Text = ""; dgv.ClearSelection();
    }

    private sealed record RoomChoice(int? Id, string Name);
    private sealed record ProvinceChoice(int? Id, string Name);
    private sealed class TenantRow
    {
        public int MaKhach { get; set; } public string HoTen { get; set; } = "";
        public string SoDienThoai { get; set; } = ""; public string TenPhong { get; set; } = "";
        public decimal GiaThue { get; set; } public string QueQuan { get; set; } = "";
        public bool DaDangKyTamTru { get; set; } public string GioiTinh { get; set; } = "";
        public DateTime NgaySinh { get; set; } public int? MaTinh { get; set; }
        public string TenTinh { get; set; } = ""; public int? MaPhuongXa { get; set; }
        public string TenPhuongXa { get; set; } = ""; public int MaPhong { get; set; }
    }
}
