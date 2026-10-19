using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SoChiTieuCaNhan.Data;
using SoChiTieuCaNhan.Models;
namespace SoChiTieuCaNhan
{
    public partial class FormChiTieu : Form
    {
        private int maDangChon = 0;
        private bool dangTai = false;
        public FormChiTieu()
        {
            InitializeComponent();
        }
        private async Task LoadDuLieu()
        {
            dangTai = true;

            try
            {
                using var db = AppConfig.TaoContext();

                string tuKhoa = txtTimKiem.Text.Trim();
                string loai = cboLocLoai.Text;

                DateOnly tuNgay =
                    DateOnly.FromDateTime(dtpTuNgay.Value.Date);

                DateOnly denNgay =
                    DateOnly.FromDateTime(dtpDenNgay.Value.Date);

                var query = db.ChiTieus.AsNoTracking();

                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    query = query.Where(x =>
                        x.NoiDung.Contains(tuKhoa));
                }

                if (loai != "Tất cả" && !string.IsNullOrEmpty(loai))
                {
                    query = query.Where(x => x.LoaiChi == loai);
                }

                query = query.Where(x =>
                    x.NgayChi >= tuNgay &&
                    x.NgayChi <= denNgay);

                var danhSach = await query
                    .OrderBy(x => x.MaCt)
                    .ToListAsync();

                dgvChiTieu.DataSource = null;
                dgvChiTieu.DataSource = danhSach;

                lblTongChi.Text =
                    $"Tổng chi: {danhSach.Sum(x => x.SoTien):N0} đ";

                dgvChiTieu.ClearSelection();
                dgvChiTieu.CurrentCell = null;

                maDangChon = 0;
            }
            finally
            {
                dangTai = false;
            }
        }
        private async void FormChiTieu_Load(object sender, EventArgs e)
        {
            try
            {
                cboLoaiChi.Items.Clear();
                cboLoaiChi.Items.AddRange(new object[]
                {
            "Ăn uống",
            "Đi lại",
            "Học tập",
            "Giải trí",
            "Khác"
                });

                cboLocLoai.Items.Clear();
                cboLocLoai.Items.AddRange(new object[]
                {
            "Tất cả",
            "Ăn uống",
            "Đi lại",
            "Học tập",
            "Giải trí",
            "Khác"
                });

                cboLoaiChi.SelectedIndex = 0;
                cboLocLoai.SelectedIndex = 0;

                dtpNgayChi.MaxDate = DateTime.Today;
                dtpNgayChi.Value = DateTime.Today;

                dtpTuNgay.Value = new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

                dtpDenNgay.Value = DateTime.Today;

                dgvChiTieu.AutoGenerateColumns = false;

                await LoadDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tải dữ liệu: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        private bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(txtNoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung chi tiêu.");
                txtNoiDung.Focus();
                return false;
            }

            if (cboLoaiChi.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn loại chi.");
                return false;
            }

            if (nudSoTien.Value <= 0)
            {
                MessageBox.Show("Số tiền phải lớn hơn 0.");
                nudSoTien.Focus();
                return false;
            }

            if (dtpNgayChi.Value.Date > DateTime.Today)
            {
                MessageBox.Show("Ngày chi không được lớn hơn hôm nay.");
                return false;
            }

            return true;
        }
        private async void btnThem_Click(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu())
                return;

            try
            {
                using var db = AppConfig.TaoContext();

                ChiTieu ct = new ChiTieu
                {
                    NgayChi = DateOnly.FromDateTime(dtpNgayChi.Value),
                    NoiDung = txtNoiDung.Text.Trim(),
                    LoaiChi = cboLoaiChi.Text,
                    SoTien = nudSoTien.Value,
                    GhiChu = txtGhiChu.Text.Trim()
                };

                db.ChiTieus.Add(ct);
                await db.SaveChangesAsync();

                MessageBox.Show("Thêm khoản chi thành công!");

                await LoadDuLieu();
                LamMoiThongTin();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm khoản chi: " + ex.Message);
            }
        }
        private async void btnSua_Click(object sender, EventArgs e)
        {
            if (maDangChon == 0)
            {
                MessageBox.Show("Vui lòng chọn khoản chi cần sửa.");
                return;
            }

            if (!KiemTraDuLieu())
                return;

            try
            {
                using var db = AppConfig.TaoContext();

                var ct = await db.ChiTieus.FindAsync(maDangChon);

                if (ct == null)
                {
                    MessageBox.Show("Không tìm thấy khoản chi.");
                    return;
                }

                ct.NgayChi = DateOnly.FromDateTime(dtpNgayChi.Value);
                ct.NoiDung = txtNoiDung.Text.Trim();
                ct.LoaiChi = cboLoaiChi.Text;
                ct.SoTien = nudSoTien.Value;
                ct.GhiChu = txtGhiChu.Text.Trim();

                await db.SaveChangesAsync();

                MessageBox.Show("Cập nhật khoản chi thành công!");

                await LoadDuLieu();
                LamMoiThongTin();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi sửa khoản chi: " + ex.Message);
            }
        }
        private async void btnXoa_Click(object sender, EventArgs e)
        {
            if (maDangChon == 0)
            {
                MessageBox.Show("Vui lòng chọn khoản chi cần xóa.");
                return;
            }

            DialogResult ketQua = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa khoản chi này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (ketQua != DialogResult.Yes)
                return;

            try
            {
                using var db = AppConfig.TaoContext();

                var ct = await db.ChiTieus.FindAsync(maDangChon);

                if (ct == null)
                {
                    MessageBox.Show("Không tìm thấy khoản chi.");
                    return;
                }

                db.ChiTieus.Remove(ct);
                await db.SaveChangesAsync();

                MessageBox.Show("Xóa khoản chi thành công!");

                await LoadDuLieu();
                LamMoiThongTin();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa khoản chi: " + ex.Message);
            }
        }
        private void LamMoiThongTin()
        {
            maDangChon = 0;

            txtNoiDung.Clear();
            txtGhiChu.Clear();

            cboLoaiChi.SelectedIndex = 0;

            nudSoTien.Value = 0;
            dtpNgayChi.Value = DateTime.Today;

            dgvChiTieu.ClearSelection();
            dgvChiTieu.CurrentCell = null;

            txtNoiDung.Focus();
        }
        private async void btnLamMoi_Click(object sender, EventArgs e)
        {
            try
            {
                txtTimKiem.Clear();
                cboLocLoai.SelectedIndex = 0;

                dtpTuNgay.Value = new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

                dtpDenNgay.Value = DateTime.Today;

                await LoadDuLieu();
                LamMoiThongTin();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi làm mới: " + ex.Message);
            }
        }
        private async void btnTim_Click(object sender, EventArgs e)
        {
            if (dtpTuNgay.Value.Date > dtpDenNgay.Value.Date)
            {
                MessageBox.Show("Từ ngày không được lớn hơn đến ngày.");
                return;
            }

            try
            {
                await LoadDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }
        private void dgvChiTieu_CellClick(
        object sender, DataGridViewCellEventArgs e)
        {
            if (dangTai || e.RowIndex < 0)
                return;

            if (dgvChiTieu.Rows[e.RowIndex].DataBoundItem is not ChiTieu ct)
                return;

            maDangChon = ct.MaCt;

            dtpNgayChi.Value = ct.NgayChi.ToDateTime(TimeOnly.MinValue);
            txtNoiDung.Text = ct.NoiDung;
            cboLoaiChi.Text = ct.LoaiChi;
            nudSoTien.Value = ct.SoTien;
            txtGhiChu.Text = ct.GhiChu ?? "";
        }
    }
}
