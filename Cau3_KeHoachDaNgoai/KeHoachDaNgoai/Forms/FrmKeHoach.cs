using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using KeHoachDaNgoai.Data;
using KeHoachDaNgoai.Models;
using KeHoachDaNgoai.Services;

namespace KeHoachDaNgoai.Forms;

public sealed class FrmKeHoach : Form
{
    private readonly DbContextOptions<QLDaNgoaiContext> _dbOptions;
    private readonly WeatherService _weather;
    private readonly ILogger _log = Log.ForContext<FrmKeHoach>();
    private readonly UserSettings _settings = UserSettings.Load();
    private readonly TextBox txtTen = new(), txtDiaDiem = new(), txtGhiChu = new();
    private readonly DateTimePicker dtpNgayDi = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
    private readonly NumericUpDown nudSoNguoi = new() { Minimum = 1, Maximum = 100, Value = 6 };
    private readonly NumericUpDown nudNganSach = new() { Minimum = 0, Maximum = 1000000000, Increment = 100000, ThousandsSeparator = true, Value = 3500000 };
    private readonly ComboBox cboPhuongTien = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox chkHoanThanh = new();
    private readonly ComboBox cboNgonNgu = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtTim = new();
    private readonly CheckBox chkChuaHoanThanh = new();
    private readonly DateTimePicker dtpTuNgay = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
    private readonly DateTimePicker dtpDenNgay = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = true };
    private readonly DataGridView grid = new();
    private readonly Button btnThoiTiet = new(), btnTim = new();
    private readonly ProgressBar progress = new() { Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 25, Visible = false };
    private readonly Label lblWeatherTitle = new(), lblWeatherIcon = new(), lblWeatherText = new(), lblRainWarning = new();
    private int? _selectedId;
    private bool IsEnglish => _settings.Language == "en";

    public FrmKeHoach(DbContextOptions<QLDaNgoaiContext> options, HttpClient http)
    {
        _dbOptions = options; _weather = new WeatherService(http);
        // The optional date filters must start inactive so the initial grid shows all trips.
        dtpTuNgay.Checked = false;
        dtpDenNgay.Checked = false;
        Text = "Kế hoạch dã ngoại cuối tuần"; StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 700); MinimumSize = new Size(980, 620); Font = new Font("Segoe UI", 9F);
        BuildLayout(); ApplyLanguage(); Load += async (_, _) => await LoadGridAsync();
        cboNgonNgu.SelectedIndexChanged += (_, _) => { _settings.Language = cboNgonNgu.SelectedIndex == 1 ? "en" : "vi"; _settings.Save(); ApplyLanguage(); _ = LoadGridAsync(); };
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 310)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        header.Controls.Add(new Label { Name = "lblTitle", Text = "Kế hoạch dã ngoại cuối tuần", Font = new Font("Segoe UI", 16, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        var languagePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(2, 5, 0, 0) };
        languagePanel.Controls.Add(new Label { Name = "lblLanguage", Text = "Ngôn ngữ", AutoSize = true, Padding = new Padding(0, 5, 0, 0) });
        cboNgonNgu.Items.AddRange(["Tiếng Việt", "English"]); cboNgonNgu.Width = 125;
        languagePanel.Controls.Add(cboNgonNgu); header.Controls.Add(languagePanel, 1, 0); root.Controls.Add(header, 0, 0);

        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58)); top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.Controls.Add(top, 0, 1);
        var group = new GroupBox { Name = "grpTrip", Text = "Thông tin chuyến đi", Dock = DockStyle.Fill, Padding = new Padding(10) };
        top.Controls.Add(group, 0, 0);
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9, Padding = new Padding(5) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 7; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        group.Controls.Add(fields);
        AddField(fields, 0, "Tên chuyến", txtTen); AddField(fields, 1, "Địa điểm", txtDiaDiem); AddField(fields, 2, "Ngày đi", dtpNgayDi);
        AddField(fields, 3, "Số người", nudSoNguoi); AddField(fields, 4, "Ngân sách", nudNganSach);
        cboPhuongTien.Items.AddRange(["Xe máy", "Ô tô", "Xe khách", "Tàu hỏa"]); cboPhuongTien.SelectedIndex = 1;
        AddField(fields, 5, "Phương tiện", cboPhuongTien);
        chkHoanThanh.Text = "Đã hoàn thành"; chkHoanThanh.AutoSize = true;
        fields.Controls.Add(chkHoanThanh, 0, 6); fields.SetColumnSpan(chkHoanThanh, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        buttons.Controls.Add(Button("Thêm", async () => await SaveAsync(false)));
        buttons.Controls.Add(Button("Sửa", async () => await SaveAsync(true)));
        buttons.Controls.Add(Button("Xóa", async () => await DeleteAsync()));
        buttons.Controls.Add(Button("Làm mới", () => { ClearForm(); return Task.CompletedTask; }));
        fields.Controls.Add(buttons, 0, 7); fields.SetColumnSpan(buttons, 2);

        var weather = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8, 0, 0, 0) };
        weather.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); weather.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); weather.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        top.Controls.Add(weather, 1, 0);
        var actionRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        btnThoiTiet.Text = "Xem thời tiết"; btnThoiTiet.Width = 145; btnThoiTiet.Height = 28; btnThoiTiet.Click += async (_, _) => await ShowWeatherAsync();
        actionRow.Controls.Add(btnThoiTiet); progress.Width = 190; progress.Height = 18; actionRow.Controls.Add(progress); weather.Controls.Add(actionRow, 0, 0);
        lblWeatherTitle.Text = "Dự báo thời tiết"; lblWeatherTitle.Dock = DockStyle.Fill; weather.Controls.Add(lblWeatherTitle, 0, 1);
        var weatherCard = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.WhiteSmoke };
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(12) };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130)); content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); content.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        lblWeatherIcon.Text = "🌤️"; lblWeatherIcon.Font = new Font("Segoe UI Emoji", 46); lblWeatherIcon.Dock = DockStyle.Fill; lblWeatherIcon.TextAlign = ContentAlignment.MiddleCenter;
        content.Controls.Add(lblWeatherIcon, 0, 0); content.SetRowSpan(lblWeatherIcon, 2);
        lblWeatherText.Text = "Chọn chuyến đi rồi bấm Xem thời tiết."; lblWeatherText.Dock = DockStyle.Fill; lblWeatherText.TextAlign = ContentAlignment.MiddleLeft;
        content.Controls.Add(lblWeatherText, 1, 0);
        lblRainWarning.Text = ""; lblRainWarning.Dock = DockStyle.Fill; lblRainWarning.TextAlign = ContentAlignment.MiddleCenter; lblRainWarning.BackColor = Color.Transparent;
        content.Controls.Add(lblRainWarning, 0, 2); content.SetColumnSpan(lblRainWarning, 2);
        weatherCard.Controls.Add(content); weather.Controls.Add(weatherCard, 0, 2);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        bottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.Controls.Add(bottom, 0, 2);
        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = false, Padding = new Padding(0, 4, 0, 0) };
        txtTim.Width = 240; txtTim.PlaceholderText = "Tìm địa điểm...";
        chkChuaHoanThanh.AutoSize = true; chkChuaHoanThanh.Padding = new Padding(2, 6, 2, 0);
        dtpTuNgay.Width = 120; dtpDenNgay.Width = 120;
        btnTim.Text = "Tìm"; btnTim.Width = 64; btnTim.Height = 28; btnTim.Click += async (_, _) => await LoadGridAsync();
        filters.Controls.Add(txtTim); filters.Controls.Add(chkChuaHoanThanh);
        filters.Controls.Add(new Label { Name = "lblFrom", Text = "Từ:", AutoSize = true, Padding = new Padding(4, 5, 0, 0) }); filters.Controls.Add(dtpTuNgay);
        filters.Controls.Add(new Label { Name = "lblTo", Text = "Đến:", AutoSize = true, Padding = new Padding(4, 5, 0, 0) }); filters.Controls.Add(dtpDenNgay); filters.Controls.Add(btnTim);
        bottom.Controls.Add(filters, 0, 0);
        grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false; grid.AutoGenerateColumns = false; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.RowHeadersWidth = 25;
        AddColumn("MaChuyen", "Mã", .45f); AddColumn("TenChuyen", "Tên chuyến", 1.2f); AddColumn("DiaDiem", "Địa điểm", .9f);
        AddColumn("NgayDi", "Ngày đi", .8f, "d"); AddColumn("SoNguoi", "Số người", .65f); AddColumn("NganSach", "Ngân sách", .9f, "N0");
        AddColumn("PhuongTien", "Phương tiện", .8f); grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "colDone", DataPropertyName = "DaHoanThanh", HeaderText = "Hoàn thành", FillWeight = .75f });
        grid.CellClick += (_, _) => SelectTrip(); bottom.Controls.Add(grid, 0, 1);

        var testError = Button("Gây lỗi thử", () => Task.FromException(new InvalidOperationException("Lỗi thử nghiệm theo đề.")));
        testError.Name = "btnGayLoiThu"; testError.Visible = false; Controls.Add(testError);
    }

    private static void AddField(TableLayoutPanel p, int row, string text, Control c)
    {
        p.Controls.Add(new Label { Name = "lbl" + row, Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        c.Dock = DockStyle.Fill; c.Margin = new Padding(3, 2, 3, 2); p.Controls.Add(c, 1, row);
    }

    private Button Button(string text, Func<Task> handler)
    {
        var b = new Button { Text = text, Width = 82, Height = 30, Margin = new Padding(3) };
        b.Click += async (_, _) => { try { await handler(); } catch (Exception ex) { _log.Error(ex, "Action failed"); ShowError(ex); } };
        return b;
    }

    private void AddColumn(string name, string header, float weight, string format = "") =>
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "col" + name, DataPropertyName = name, HeaderText = header, FillWeight = weight, DefaultCellStyle = { Format = format } });

    private async Task LoadGridAsync()
    {
        try
        {
            await using var db = new QLDaNgoaiContext(_dbOptions);
            var q = db.ChuyenDis.AsNoTracking().AsQueryable();
            var location = txtTim.Text.Trim(); if (location.Length > 0) q = q.Where(x => x.DiaDiem.Contains(location));
            if (chkChuaHoanThanh.Checked) q = q.Where(x => !x.DaHoanThanh);
            if (dtpTuNgay.Checked) q = q.Where(x => x.NgayDi >= dtpTuNgay.Value.Date);
            if (dtpDenNgay.Checked) q = q.Where(x => x.NgayDi <= dtpDenNgay.Value.Date);
            var trips = await q.OrderBy(x => x.NgayDi).ToListAsync();
            grid.DataSource = trips;
            ApplyLanguage();
            if (trips.Count > 0)
            {
                var selectedIndex = trips.FindIndex(t => t.MaChuyen == _selectedId);
                if (selectedIndex < 0) selectedIndex = 0;
                grid.CurrentCell = grid.Rows[selectedIndex].Cells[0];
                SelectTrip();
            }
        }
        catch (Exception ex) { _log.Error(ex, "Could not load trips from database"); ShowError(ex); }
    }

    private async Task SaveAsync(bool edit)
    {
        if (string.IsNullOrWhiteSpace(txtTen.Text) || string.IsNullOrWhiteSpace(txtDiaDiem.Text))
        { MessageBox.Show(this, IsEnglish ? "Enter a trip name and destination." : "Vui lòng nhập tên chuyến và địa điểm."); return; }
        if (dtpNgayDi.Value.Date < DateTime.Today && !edit)
        { MessageBox.Show(this, IsEnglish ? "A new trip date cannot be in the past." : "Ngày đi của chuyến mới không được nhỏ hơn hôm nay."); return; }
        if (nudSoNguoi.Value < 1 || nudNganSach.Value < 0) return;
        if (edit && _selectedId is null) { MessageBox.Show(this, IsEnglish ? "Select a trip in the table first." : "Hãy chọn chuyến đi trong bảng trước."); return; }
        try
        {
            await using var db = new QLDaNgoaiContext(_dbOptions);
            var trip = edit ? await db.ChuyenDis.FindAsync(_selectedId) : new ChuyenDi();
            if (trip is null) return;
            trip.TenChuyen = txtTen.Text.Trim(); trip.DiaDiem = txtDiaDiem.Text.Trim(); trip.NgayDi = dtpNgayDi.Value.Date;
            trip.SoNguoi = (int)nudSoNguoi.Value; trip.NganSach = nudNganSach.Value; trip.PhuongTien = cboPhuongTien.Text; trip.DaHoanThanh = chkHoanThanh.Checked;
            if (!edit) db.ChuyenDis.Add(trip);
            await db.SaveChangesAsync();
            _log.Information("{Action} trip MaChuyen={MaChuyen}", edit ? "Updated" : "Created", trip.MaChuyen);
            ClearListFilters();
            await LoadGridAsync(); ClearForm();
            MessageBox.Show(this, IsEnglish ? "Trip saved successfully." : "Đã lưu chuyến đi thành công.",
                IsEnglish ? "Success" : "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { _log.Error(ex, "Database error saving trip"); ShowError(ex); }
    }

    private async Task DeleteAsync()
    {
        if (_selectedId is null) { MessageBox.Show(this, IsEnglish ? "Select a trip first." : "Hãy chọn chuyến đi cần xóa."); return; }
        var answer = MessageBox.Show(this, IsEnglish ? "Delete the selected trip?" : "Bạn có chắc muốn xóa chuyến đi đang chọn?",
            IsEnglish ? "Confirm delete" : "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;
        try
        {
            await using var db = new QLDaNgoaiContext(_dbOptions);
            var trip = await db.ChuyenDis.FindAsync(_selectedId.Value); if (trip is null) return;
            db.ChuyenDis.Remove(trip); await db.SaveChangesAsync(); _log.Information("Deleted trip MaChuyen={MaChuyen}", trip.MaChuyen);
            await LoadGridAsync(); ClearForm();
        }
        catch (Exception ex) { _log.Error(ex, "Database error deleting trip"); ShowError(ex); }
    }

    private void SelectTrip()
    {
        if (grid.CurrentRow?.DataBoundItem is not ChuyenDi t) return;
        _selectedId = t.MaChuyen; txtTen.Text = t.TenChuyen; txtDiaDiem.Text = t.DiaDiem;
        dtpNgayDi.Value = t.NgayDi; nudSoNguoi.Value = Math.Clamp(t.SoNguoi, (int)nudSoNguoi.Minimum, (int)nudSoNguoi.Maximum);
        nudNganSach.Value = Math.Clamp(t.NganSach, nudNganSach.Minimum, nudNganSach.Maximum);
        cboPhuongTien.SelectedItem = t.PhuongTien; chkHoanThanh.Checked = t.DaHoanThanh;
    }

    private async Task ShowWeatherAsync()
    {
        if (string.IsNullOrWhiteSpace(txtDiaDiem.Text)) { MessageBox.Show(this, IsEnglish ? "Enter a destination first." : "Hãy nhập địa điểm trước."); return; }
        btnThoiTiet.Enabled = false; progress.Visible = true;
        try
        {
            var weather = await _weather.GetForecastAsync(txtDiaDiem.Text.Trim(), dtpNgayDi.Value.Date);
            if (weather is null)
            {
                lblWeatherText.Text = IsEnglish ? "No forecast available (trip is more than 16 days away)." : "Chưa có dữ liệu dự báo (ngày đi quá 16 ngày).";
                lblWeatherTitle.Text = IsEnglish ? "Forecast unavailable" : "Chưa có dữ liệu dự báo"; lblWeatherIcon.Text = "📅"; lblRainWarning.Text = ""; return;
            }
            var description = Describe(weather.WeatherCode, IsEnglish);
            lblWeatherTitle.Text = IsEnglish
                ? $"Forecast for {dtpNgayDi.Value.ToString("d", CultureInfo.GetCultureInfo("en-US"))} - {weather.Place}"
                : $"Dự báo thời tiết ngày {dtpNgayDi.Value:dd/MM/yyyy} - {weather.Place}";
            lblWeatherIcon.Text = IconFor(weather.WeatherCode);
            lblWeatherText.Text = IsEnglish
                ? $"{description}\nTemperature: {weather.MinC:0}°C - {weather.MaxC:0}°C\nRain probability: {weather.RainProbability}%"
                : $"{description}\nNhiệt độ: {weather.MinC:0}°C - {weather.MaxC:0}°C\nKhả năng mưa: {weather.RainProbability}%";
            if (weather.RainProbability >= 60)
            {
                lblRainWarning.Text = "⚠ " + (IsEnglish ? "Bring a raincoat or consider changing the date" : "Nên mang áo mưa hoặc cân nhắc dời lịch");
                lblRainWarning.BackColor = Color.PeachPuff;
            }
            else { lblRainWarning.Text = ""; lblRainWarning.BackColor = Color.Transparent; }
        }
        catch (KeyNotFoundException)
        {
            lblWeatherText.Text = IsEnglish ? "Location not found. Try an unaccented name, e.g. Da Lat." : "Không tìm thấy địa điểm. Hãy thử tên không dấu, ví dụ: Da Lat.";
            lblWeatherIcon.Text = "🔎"; lblWeatherTitle.Text = IsEnglish ? "Location not found" : "Không tìm thấy địa điểm"; lblRainWarning.Text = "";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _log.Error(ex, "Weather API request failed for {Location}", txtDiaDiem.Text);
            lblWeatherTitle.Text = IsEnglish ? "Weather service unavailable" : "Không lấy được dự báo";
            lblWeatherText.Text = IsEnglish ? "Check your connection and try again." : "Mất kết nối hoặc dịch vụ thời tiết không phản hồi. Hãy kiểm tra mạng và thử lại.";
            lblWeatherIcon.Text = "🌐"; lblRainWarning.Text = "";
        }
        catch (Exception ex) { _log.Error(ex, "Unexpected weather error"); ShowError(ex); }
        finally { progress.Visible = false; btnThoiTiet.Enabled = true; }
    }

    private static string IconFor(int code) => code switch
    {
        0 => "☀️", 1 or 2 or 3 => "☁️", 45 or 48 => "🌫️",
        >= 51 and <= 67 or >= 80 and <= 82 => "🌧️", >= 95 and <= 99 => "⛈️", _ => "🌤️"
    };

    private static string Describe(int code, bool english) => code switch
    {
        0 => english ? "Clear sky" : "Trời quang",
        1 or 2 or 3 => english ? "Partly cloudy" : "Có mây",
        45 or 48 => english ? "Fog" : "Sương mù",
        >= 51 and <= 67 or >= 80 and <= 82 => english ? "Light rain" : "Mưa rào nhẹ",
        >= 95 and <= 99 => english ? "Thunderstorm" : "Dông",
        _ => english ? "Variable weather" : "Thời tiết thay đổi"
    };

    private void ClearForm()
    {
        _selectedId = null; txtTen.Clear(); txtDiaDiem.Clear(); dtpNgayDi.Value = DateTime.Today.AddDays(6);
        nudSoNguoi.Value = 6; nudNganSach.Value = 3500000; cboPhuongTien.SelectedItem = "Ô tô"; chkHoanThanh.Checked = false; grid.ClearSelection();
    }

    private void ClearListFilters()
    {
        txtTim.Clear();
        chkChuaHoanThanh.Checked = false;
        dtpTuNgay.Checked = false;
        dtpDenNgay.Checked = false;
    }

    private void ShowError(Exception ex)
    {
        _log.Error(ex, "User-facing operation error");
        MessageBox.Show(this, IsEnglish ? "An error occurred. Details were logged." : "Đã xảy ra lỗi, thông tin đã được ghi lại",
            IsEnglish ? "Error" : "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ApplyLanguage()
    {
        if (cboNgonNgu.Items.Count > 0 && cboNgonNgu.SelectedIndex != (IsEnglish ? 1 : 0)) cboNgonNgu.SelectedIndex = IsEnglish ? 1 : 0;
        Text = IsEnglish ? "Weekend Trip Planner" : "Kế hoạch dã ngoại cuối tuần";
        if (Controls.Find("lblTitle", true).FirstOrDefault() is Label title) title.Text = IsEnglish ? "Weekend Trip Planner" : "Kế hoạch dã ngoại cuối tuần";
        if (Controls.Find("lblLanguage", true).FirstOrDefault() is Label lang) lang.Text = IsEnglish ? "Language" : "Ngôn ngữ";
        if (Controls.Find("grpTrip", true).FirstOrDefault() is GroupBox group) group.Text = IsEnglish ? "Trip information" : "Thông tin chuyến đi";
        var captionsVi = new[] { "Tên chuyến", "Địa điểm", "Ngày đi", "Số người", "Ngân sách", "Phương tiện" };
        var captionsEn = new[] { "Trip name", "Destination", "Travel date", "People", "Budget", "Transport" };
        for (var i = 0; i < 6; i++) if (Controls.Find("lbl" + i, true).FirstOrDefault() is Label label) label.Text = IsEnglish ? captionsEn[i] : captionsVi[i];
        chkHoanThanh.Text = IsEnglish ? "Completed" : "Đã hoàn thành";
        btnThoiTiet.Text = IsEnglish ? "Show weather" : "Xem thời tiết"; btnTim.Text = IsEnglish ? "Search" : "Tìm";
        if (Controls.Find("lblFrom", true).FirstOrDefault() is Label from) from.Text = IsEnglish ? "From:" : "Từ:";
        if (Controls.Find("lblTo", true).FirstOrDefault() is Label to) to.Text = IsEnglish ? "To:" : "Đến:";
        chkChuaHoanThanh.Text = IsEnglish ? "Show incomplete trips only" : "Chỉ hiện chuyến chưa hoàn thành";
        txtTim.PlaceholderText = IsEnglish ? "Search destination..." : "Tìm địa điểm...";
        lblWeatherTitle.Text = IsEnglish ? "Weather forecast" : "Dự báo thời tiết";
        lblWeatherText.Text = IsEnglish ? "Select a trip and click Show weather." : "Chọn chuyến đi rồi bấm Xem thời tiết.";
        var selectedTransport = cboPhuongTien.SelectedItem?.ToString();
        cboPhuongTien.Items.Clear();
        cboPhuongTien.Items.AddRange(IsEnglish
            ? ["Motorbike", "Car", "Coach", "Train"]
            : ["Xe máy", "Ô tô", "Xe khách", "Tàu hỏa"]);
        cboPhuongTien.SelectedIndex = selectedTransport switch
        {
            "Xe máy" or "Motorbike" => 0,
            "Ô tô" or "Car" => 1,
            "Xe khách" or "Coach" => 2,
            "Tàu hỏa" or "Train" => 3,
            _ => 1
        };
        Thread.CurrentThread.CurrentCulture = IsEnglish ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("vi-VN");
        var headersVi = new[] { "Mã", "Tên chuyến", "Địa điểm", "Ngày đi", "Số người", "Ngân sách", "Phương tiện", "Hoàn thành" };
        var headersEn = new[] { "ID", "Trip name", "Destination", "Date", "People", "Budget", "Transport", "Completed" };
        for (var i = 0; i < grid.Columns.Count; i++) grid.Columns[i].HeaderText = IsEnglish ? headersEn[i] : headersVi[i];
        if (grid.Columns.Count > 3) grid.Columns[3].DefaultCellStyle.Format = IsEnglish ? "MM/dd/yyyy" : "dd/MM/yyyy";
        if (grid.DataSource is List<ChuyenDi> trips)
            grid.DataSource = trips;
        foreach (Control c in ControlsRecursive(this))
        {
            if (c is Button b && b.Name != "btnGayLoiThu")
            {
                b.Text = (b.Text, IsEnglish) switch
                {
                    ("Thêm", true) => "Add", ("Sửa", true) => "Edit", ("Xóa", true) => "Delete", ("Làm mới", true) => "Clear",
                    ("Add", false) => "Thêm", ("Edit", false) => "Sửa", ("Delete", false) => "Xóa", ("Clear", false) => "Làm mới",
                    _ => b.Text
                };
            }
        }
    }

    private static IEnumerable<Control> ControlsRecursive(Control parent)
    {
        foreach (Control child in parent.Controls) { yield return child; foreach (var nested in ControlsRecursive(child)) yield return nested; }
    }
}
