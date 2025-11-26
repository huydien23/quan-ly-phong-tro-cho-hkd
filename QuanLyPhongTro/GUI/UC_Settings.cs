using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Settings : UserControl
    {
        private AntdUI.InputNumber txtPriceElec;
        private AntdUI.InputNumber txtPriceWater;
        private AntdUI.InputNumber txtInternet;
        private AntdUI.InputNumber txtTrash;
        private AntdUI.InputNumber txtTaxThreshold;

        public UC_Settings()
        {
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadSettings();
        }

        private void InitUI()
        {
            // Panel chứa form cài đặt
            var panelContent = new AntdUI.Panel();
            panelContent.Size = new Size(550, 520);
            panelContent.Location = new Point(50, 20);
            panelContent.Radius = 12;
            panelContent.BackColor = Color.White;
            panelContent.Shadow = 10;

            // Tiêu đề
            var lblTitle = new AntdUI.Label();
            lblTitle.Text = "CẤU HÌNH ĐƠN GIÁ & THUẾ";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = AppColors.Blue900;
            lblTitle.Location = new Point(30, 20);
            lblTitle.Size = new Size(440, 35);
            panelContent.Controls.Add(lblTitle);

            int y = 70;
            int col1 = 30, col2 = 280;
            int inputW = 220, inputH = 38;

            // Row 1: Giá Điện + Giá Nước
            AddLabel(panelContent, "Giá Điện (VNĐ/kWh):", col1, y);
            txtPriceElec = new AntdUI.InputNumber { Location = new Point(col1, y + 25), Size = new Size(inputW, inputH) };
            panelContent.Controls.Add(txtPriceElec);

            AddLabel(panelContent, "Giá Nước (VNĐ/khối):", col2, y);
            txtPriceWater = new AntdUI.InputNumber { Location = new Point(col2, y + 25), Size = new Size(inputW, inputH) };
            panelContent.Controls.Add(txtPriceWater);

            y += 85;

            // Row 2: Internet + Rác
            AddLabel(panelContent, "Phí Internet (VNĐ/tháng):", col1, y);
            txtInternet = new AntdUI.InputNumber { Location = new Point(col1, y + 25), Size = new Size(inputW, inputH) };
            panelContent.Controls.Add(txtInternet);

            AddLabel(panelContent, "Phí Rác (VNĐ/tháng):", col2, y);
            txtTrash = new AntdUI.InputNumber { Location = new Point(col2, y + 25), Size = new Size(inputW, inputH) };
            panelContent.Controls.Add(txtTrash);

            y += 85;

            // Row 3: Ngưỡng thuế (full width)
            AddLabel(panelContent, "Ngưỡng miễn thuế HKD (VNĐ/năm):", col1, y);
            txtTaxThreshold = new AntdUI.InputNumber { Location = new Point(col1, y + 25), Size = new Size(470, inputH) };
            panelContent.Controls.Add(txtTaxThreshold);

            y += 85;

            // Ghi chú
            var lblNote = new AntdUI.Label();
            lblNote.Text = "💡 Lưu ý: Thay đổi sẽ áp dụng cho hóa đơn tạo mới. Hóa đơn cũ giữ nguyên giá cũ.";
            lblNote.ForeColor = Color.Gray;
            lblNote.Font = new Font("Segoe UI", 9);
            lblNote.Location = new Point(col1, y);
            lblNote.Size = new Size(480, 40);
            panelContent.Controls.Add(lblNote);

            y += 50;

            // Nút Lưu
            var btnSave = new AntdUI.Button();
            btnSave.Text = "LƯU CẤU HÌNH";
            btnSave.Type = TTypeMini.Primary;
            btnSave.BackColor = AppColors.Blue600;
            btnSave.Location = new Point(col1, y);
            btnSave.Size = new Size(220, 45);
            btnSave.Click += (s, e) => SaveSettings();
            panelContent.Controls.Add(btnSave);

            var btnReset = new AntdUI.Button();
            btnReset.Text = "TẢI LẠI";
            btnReset.Location = new Point(col2, y);
            btnReset.Size = new Size(120, 45);
            btnReset.Click += (s, e) => LoadSettings();
            panelContent.Controls.Add(btnReset);

            this.Controls.Add(panelContent);
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            var lbl = new AntdUI.Label { Text = text, Location = new Point(x, y), Size = new Size(220, 22), Font = new Font("Segoe UI", 10) };
            parent.Controls.Add(lbl);
        }

        private void LoadSettings()
        {
            try
            {
                var elec = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'GiaDien'");
                var water = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'GiaNuoc'");
                var internet = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'PhiInternet'");
                var trash = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'PhiRac'");
                var tax = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'NguongMienThue'");

                txtPriceElec.Value = elec != null ? Convert.ToDecimal(elec) : 4000;
                txtPriceWater.Value = water != null ? Convert.ToDecimal(water) : 10000;
                txtInternet.Value = internet != null ? Convert.ToDecimal(internet) : 100000;
                txtTrash.Value = trash != null ? Convert.ToDecimal(trash) : 20000;
                txtTaxThreshold.Value = tax != null ? Convert.ToDecimal(tax) : 100000000;
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi tải cài đặt: " + ex.Message);
            }
        }

        private void SaveSettings()
        {
            try
            {
                UpdateSetting("GiaDien", txtPriceElec.Value);
                UpdateSetting("GiaNuoc", txtPriceWater.Value);
                UpdateSetting("PhiInternet", txtInternet.Value);
                UpdateSetting("PhiRac", txtTrash.Value);
                UpdateSetting("NguongMienThue", txtTaxThreshold.Value);

                AntdUI.Message.success(this.FindForm(), "Đã lưu cài đặt!");
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void UpdateSetting(string key, decimal value)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Settings SET SettingValue = @val WHERE SettingKey = @key",
                new System.Data.SqlClient.SqlParameter("@val", value),
                new System.Data.SqlClient.SqlParameter("@key", key));
        }
    }
}