namespace SoChiTieuCaNhan
{
    partial class FormChiTieu
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            grpThongTin = new GroupBox();
            txtGhiChu = new TextBox();
            lblGhiChu = new Label();
            nudSoTien = new NumericUpDown();
            lblSoTien = new Label();
            cboLoaiChi = new ComboBox();
            lblLoaiChi = new Label();
            txtNoiDung = new TextBox();
            lblNoiDung = new Label();
            dtpNgayChi = new DateTimePicker();
            lblNgayChi = new Label();
            grpTimKiem = new GroupBox();
            btnTim = new Button();
            dtpDenNgay = new DateTimePicker();
            lblDenNgay = new Label();
            dtpTuNgay = new DateTimePicker();
            lblTuNgay = new Label();
            cboLocLoai = new ComboBox();
            lblLocLoai = new Label();
            txtTimKiem = new TextBox();
            lblTimKiem = new Label();
            btnThem = new Button();
            btnSua = new Button();
            btnXoa = new Button();
            btnLamMoi = new Button();
            dgvChiTieu = new DataGridView();
            colMaCT = new DataGridViewTextBoxColumn();
            colNgayChi = new DataGridViewTextBoxColumn();
            colNoiDung = new DataGridViewTextBoxColumn();
            colLoaiChi = new DataGridViewTextBoxColumn();
            colSoTien = new DataGridViewTextBoxColumn();
            colGhiChu = new DataGridViewTextBoxColumn();
            lblTongChi = new Label();
            grpThongTin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSoTien).BeginInit();
            grpTimKiem.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChiTieu).BeginInit();
            SuspendLayout();
            // 
            // grpThongTin
            // 
            grpThongTin.Controls.Add(txtGhiChu);
            grpThongTin.Controls.Add(lblGhiChu);
            grpThongTin.Controls.Add(nudSoTien);
            grpThongTin.Controls.Add(lblSoTien);
            grpThongTin.Controls.Add(cboLoaiChi);
            grpThongTin.Controls.Add(lblLoaiChi);
            grpThongTin.Controls.Add(txtNoiDung);
            grpThongTin.Controls.Add(lblNoiDung);
            grpThongTin.Controls.Add(dtpNgayChi);
            grpThongTin.Controls.Add(lblNgayChi);
            grpThongTin.Location = new Point(15, 15);
            grpThongTin.Name = "grpThongTin";
            grpThongTin.Size = new Size(545, 195);
            grpThongTin.TabIndex = 0;
            grpThongTin.TabStop = false;
            grpThongTin.Text = "Thông tin khoản chi";
            // 
            // txtGhiChu
            // 
            txtGhiChu.Location = new Point(100, 145);
            txtGhiChu.Multiline = true;
            txtGhiChu.Name = "txtGhiChu";
            txtGhiChu.Size = new Size(425, 35);
            txtGhiChu.TabIndex = 9;
            // 
            // lblGhiChu
            // 
            lblGhiChu.AutoSize = true;
            lblGhiChu.Location = new Point(15, 145);
            lblGhiChu.Name = "lblGhiChu";
            lblGhiChu.Size = new Size(58, 20);
            lblGhiChu.TabIndex = 8;
            lblGhiChu.Text = "Ghi chú";
            // 
            // nudSoTien
            // 
            nudSoTien.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
            nudSoTien.Location = new Point(350, 68);
            nudSoTien.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            nudSoTien.Name = "nudSoTien";
            nudSoTien.Size = new Size(175, 27);
            nudSoTien.TabIndex = 7;
            nudSoTien.ThousandsSeparator = true;
            // 
            // lblSoTien
            // 
            lblSoTien.AutoSize = true;
            lblSoTien.Location = new Point(280, 73);
            lblSoTien.Name = "lblSoTien";
            lblSoTien.Size = new Size(55, 20);
            lblSoTien.TabIndex = 6;
            lblSoTien.Text = "Số tiền";
            // 
            // cboLoaiChi
            // 
            cboLoaiChi.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoaiChi.FormattingEnabled = true;
            cboLoaiChi.Location = new Point(100, 108);
            cboLoaiChi.Name = "cboLoaiChi";
            cboLoaiChi.Size = new Size(150, 28);
            cboLoaiChi.TabIndex = 5;
            // 
            // lblLoaiChi
            // 
            lblLoaiChi.AutoSize = true;
            lblLoaiChi.Location = new Point(15, 108);
            lblLoaiChi.Name = "lblLoaiChi";
            lblLoaiChi.Size = new Size(60, 20);
            lblLoaiChi.TabIndex = 4;
            lblLoaiChi.Text = "Loại chi";
            // 
            // txtNoiDung
            // 
            txtNoiDung.Location = new Point(100, 70);
            txtNoiDung.Name = "txtNoiDung";
            txtNoiDung.Size = new Size(150, 27);
            txtNoiDung.TabIndex = 3;
            // 
            // lblNoiDung
            // 
            lblNoiDung.AutoSize = true;
            lblNoiDung.Location = new Point(15, 70);
            lblNoiDung.Name = "lblNoiDung";
            lblNoiDung.Size = new Size(71, 20);
            lblNoiDung.TabIndex = 2;
            lblNoiDung.Text = "Nội dung";
            // 
            // dtpNgayChi
            // 
            dtpNgayChi.Format = DateTimePickerFormat.Short;
            dtpNgayChi.Location = new Point(100, 32);
            dtpNgayChi.Name = "dtpNgayChi";
            dtpNgayChi.Size = new Size(150, 27);
            dtpNgayChi.TabIndex = 1;
            // 
            // lblNgayChi
            // 
            lblNgayChi.AutoSize = true;
            lblNgayChi.Location = new Point(15, 32);
            lblNgayChi.Name = "lblNgayChi";
            lblNgayChi.Size = new Size(67, 20);
            lblNgayChi.TabIndex = 0;
            lblNgayChi.Text = "Ngày chi";
            // 
            // grpTimKiem
            // 
            grpTimKiem.Controls.Add(btnTim);
            grpTimKiem.Controls.Add(dtpDenNgay);
            grpTimKiem.Controls.Add(lblDenNgay);
            grpTimKiem.Controls.Add(dtpTuNgay);
            grpTimKiem.Controls.Add(lblTuNgay);
            grpTimKiem.Controls.Add(cboLocLoai);
            grpTimKiem.Controls.Add(lblLocLoai);
            grpTimKiem.Controls.Add(txtTimKiem);
            grpTimKiem.Controls.Add(lblTimKiem);
            grpTimKiem.Location = new Point(575, 15);
            grpTimKiem.Name = "grpTimKiem";
            grpTimKiem.Size = new Size(360, 195);
            grpTimKiem.TabIndex = 1;
            grpTimKiem.TabStop = false;
            grpTimKiem.Text = "Tìm kiếm";
            // 
            // btnTim
            // 
            btnTim.Location = new Point(275, 145);
            btnTim.Name = "btnTim";
            btnTim.Size = new Size(70, 28);
            btnTim.TabIndex = 17;
            btnTim.Text = "Tìm";
            btnTim.UseVisualStyleBackColor = true;
            btnTim.Click += btnTim_Click;
            // 
            // dtpDenNgay
            // 
            dtpDenNgay.Format = DateTimePickerFormat.Short;
            dtpDenNgay.Location = new Point(100, 145);
            dtpDenNgay.Name = "dtpDenNgay";
            dtpDenNgay.Size = new Size(160, 27);
            dtpDenNgay.TabIndex = 16;
            // 
            // lblDenNgay
            // 
            lblDenNgay.AutoSize = true;
            lblDenNgay.Location = new Point(15, 145);
            lblDenNgay.Name = "lblDenNgay";
            lblDenNgay.Size = new Size(72, 20);
            lblDenNgay.TabIndex = 15;
            lblDenNgay.Text = "Đến ngày";
            // 
            // dtpTuNgay
            // 
            dtpTuNgay.Format = DateTimePickerFormat.Short;
            dtpTuNgay.Location = new Point(100, 106);
            dtpTuNgay.Name = "dtpTuNgay";
            dtpTuNgay.Size = new Size(160, 27);
            dtpTuNgay.TabIndex = 14;
            // 
            // lblTuNgay
            // 
            lblTuNgay.AutoSize = true;
            lblTuNgay.Location = new Point(15, 106);
            lblTuNgay.Name = "lblTuNgay";
            lblTuNgay.Size = new Size(62, 20);
            lblTuNgay.TabIndex = 13;
            lblTuNgay.Text = "Từ ngày";
            // 
            // cboLocLoai
            // 
            cboLocLoai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocLoai.FormattingEnabled = true;
            cboLocLoai.Location = new Point(100, 68);
            cboLocLoai.Name = "cboLocLoai";
            cboLocLoai.Size = new Size(245, 28);
            cboLocLoai.TabIndex = 12;
            // 
            // lblLocLoai
            // 
            lblLocLoai.AutoSize = true;
            lblLocLoai.Location = new Point(15, 68);
            lblLocLoai.Name = "lblLocLoai";
            lblLocLoai.Size = new Size(60, 20);
            lblLocLoai.TabIndex = 11;
            lblLocLoai.Text = "Loại chi";
            // 
            // txtTimKiem
            // 
            txtTimKiem.Location = new Point(100, 30);
            txtTimKiem.Name = "txtTimKiem";
            txtTimKiem.PlaceholderText = "Nhập nội dung...";
            txtTimKiem.Size = new Size(245, 27);
            txtTimKiem.TabIndex = 10;
            // 
            // lblTimKiem
            // 
            lblTimKiem.AutoSize = true;
            lblTimKiem.Location = new Point(15, 30);
            lblTimKiem.Name = "lblTimKiem";
            lblTimKiem.Size = new Size(71, 20);
            lblTimKiem.TabIndex = 9;
            lblTimKiem.Text = "Nội dung";
            // 
            // btnThem
            // 
            btnThem.Location = new Point(110, 220);
            btnThem.Name = "btnThem";
            btnThem.Size = new Size(95, 32);
            btnThem.TabIndex = 2;
            btnThem.Text = "Thêm";
            btnThem.UseVisualStyleBackColor = true;
            btnThem.Click += btnThem_Click;
            // 
            // btnSua
            // 
            btnSua.Location = new Point(215, 220);
            btnSua.Name = "btnSua";
            btnSua.Size = new Size(95, 32);
            btnSua.TabIndex = 3;
            btnSua.Text = "Sửa";
            btnSua.UseVisualStyleBackColor = true;
            btnSua.Click += btnSua_Click;
            // 
            // btnXoa
            // 
            btnXoa.Location = new Point(320, 220);
            btnXoa.Name = "btnXoa";
            btnXoa.Size = new Size(95, 32);
            btnXoa.TabIndex = 4;
            btnXoa.Text = "Xóa";
            btnXoa.UseVisualStyleBackColor = true;
            btnXoa.Click += btnXoa_Click;
            // 
            // btnLamMoi
            // 
            btnLamMoi.Location = new Point(425, 220);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(110, 32);
            btnLamMoi.TabIndex = 5;
            btnLamMoi.Text = "Làm mới";
            btnLamMoi.UseVisualStyleBackColor = true;
            btnLamMoi.Click += btnLamMoi_Click;
            // 
            // dgvChiTieu
            // 
            dgvChiTieu.AllowUserToAddRows = false;
            dgvChiTieu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvChiTieu.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvChiTieu.Columns.AddRange(new DataGridViewColumn[] { colMaCT, colNgayChi, colNoiDung, colLoaiChi, colSoTien, colGhiChu });
            dgvChiTieu.Location = new Point(15, 265);
            dgvChiTieu.MultiSelect = false;
            dgvChiTieu.Name = "dgvChiTieu";
            dgvChiTieu.ReadOnly = true;
            dgvChiTieu.RowHeadersWidth = 51;
            dgvChiTieu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvChiTieu.Size = new Size(920, 210);
            dgvChiTieu.TabIndex = 6;
            dgvChiTieu.CellClick += this.dgvChiTieu_CellClick;
            // 
            // colMaCT
            // 
            colMaCT.DataPropertyName = "MaCt";
            colMaCT.HeaderText = "Mã";
            colMaCT.MinimumWidth = 6;
            colMaCT.Name = "colMaCT";
            colMaCT.ReadOnly = true;
            // 
            // colNgayChi
            // 
            colNgayChi.DataPropertyName = "NgayChi";
            colNgayChi.HeaderText = "Ngày chi";
            colNgayChi.MinimumWidth = 6;
            colNgayChi.Name = "colNgayChi";
            colNgayChi.ReadOnly = true;
            // 
            // colNoiDung
            // 
            colNoiDung.DataPropertyName = "NoiDung";
            colNoiDung.HeaderText = "Nội dung";
            colNoiDung.MinimumWidth = 6;
            colNoiDung.Name = "colNoiDung";
            colNoiDung.ReadOnly = true;
            // 
            // colLoaiChi
            // 
            colLoaiChi.DataPropertyName = "LoaiChi";
            colLoaiChi.HeaderText = "Loại chi";
            colLoaiChi.MinimumWidth = 6;
            colLoaiChi.Name = "colLoaiChi";
            colLoaiChi.ReadOnly = true;
            // 
            // colSoTien
            // 
            colSoTien.DataPropertyName = "SoTien";
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle1.Format = "N0";
            colSoTien.DefaultCellStyle = dataGridViewCellStyle1;
            colSoTien.HeaderText = "Số tiền";
            colSoTien.MinimumWidth = 6;
            colSoTien.Name = "colSoTien";
            colSoTien.ReadOnly = true;
            // 
            // colGhiChu
            // 
            colGhiChu.DataPropertyName = "GhiChu";
            colGhiChu.HeaderText = "Ghi chú";
            colGhiChu.MinimumWidth = 6;
            colGhiChu.Name = "colGhiChu";
            colGhiChu.ReadOnly = true;
            // 
            // lblTongChi
            // 
            lblTongChi.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTongChi.ForeColor = Color.Firebrick;
            lblTongChi.Location = new Point(635, 485);
            lblTongChi.Name = "lblTongChi";
            lblTongChi.Size = new Size(300, 35);
            lblTongChi.TabIndex = 7;
            lblTongChi.Text = "Tổng chi: 0 đ";
            lblTongChi.TextAlign = ContentAlignment.MiddleRight;
            // 
            // FormChiTieu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(958, 523);
            Controls.Add(lblTongChi);
            Controls.Add(dgvChiTieu);
            Controls.Add(btnLamMoi);
            Controls.Add(btnXoa);
            Controls.Add(btnSua);
            Controls.Add(btnThem);
            Controls.Add(grpTimKiem);
            Controls.Add(grpThongTin);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormChiTieu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SỔ CHI TIÊU CÁ NHÂN";
            Load += FormChiTieu_Load;
            grpThongTin.ResumeLayout(false);
            grpThongTin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSoTien).EndInit();
            grpTimKiem.ResumeLayout(false);
            grpTimKiem.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvChiTieu).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpThongTin;
        private Label lblNoiDung;
        private DateTimePicker dtpNgayChi;
        private Label lblNgayChi;
        private GroupBox grpTimKiem;
        private TextBox txtGhiChu;
        private Label lblGhiChu;
        private NumericUpDown nudSoTien;
        private Label lblSoTien;
        private ComboBox cboLoaiChi;
        private Label lblLoaiChi;
        private TextBox txtNoiDung;
        private Label lblTimKiem;
        private Button btnTim;
        private DateTimePicker dtpDenNgay;
        private Label lblDenNgay;
        private DateTimePicker dtpTuNgay;
        private Label lblTuNgay;
        private ComboBox cboLocLoai;
        private Label lblLocLoai;
        private TextBox txtTimKiem;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLamMoi;
        private DataGridView dgvChiTieu;
        private DataGridViewTextBoxColumn colMaCT;
        private DataGridViewTextBoxColumn colNgayChi;
        private DataGridViewTextBoxColumn colNoiDung;
        private DataGridViewTextBoxColumn colLoaiChi;
        private DataGridViewTextBoxColumn colSoTien;
        private DataGridViewTextBoxColumn colGhiChu;
        private Label lblTongChi;
    }
}
