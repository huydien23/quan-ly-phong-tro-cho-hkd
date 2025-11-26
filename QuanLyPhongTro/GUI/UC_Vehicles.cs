using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Vehicles : UserControl
    {
        private AntdUI.Table table;

        public UC_Vehicles()
        {
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            // 1. Table
            table = new AntdUI.Table();
            table.Dock = DockStyle.Fill;
            table.EmptyText = "Chưa có xe nào được đăng ký";
            table.Columns.Add(new Column("VehicleId", "ID", ColumnAlign.Center) { Width = "50" });
            table.Columns.Add(new Column("CustomerName", "Chủ xe", ColumnAlign.Left) { Width = "150" });
            table.Columns.Add(new Column("RoomName", "Phòng", ColumnAlign.Center) { Width = "70" });
            table.Columns.Add(new Column("VehicleTypeName", "Loại xe", ColumnAlign.Center) { Width = "80" });
            table.Columns.Add(new Column("LicensePlate", "Biển số", ColumnAlign.Center) { Width = "100" });
            table.Columns.Add(new Column("Brand", "Hãng xe", ColumnAlign.Left) { Width = "120" });
            table.Columns.Add(new Column("Color", "Màu", ColumnAlign.Center) { Width = "80" });
            table.Columns.Add(new Column("MonthlyFee", "Phí/tháng", ColumnAlign.Right) { Width = "100" });
            this.Controls.Add(table);

            // 2. Toolbar
            var panelTop = new AntdUI.Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.White;

            var btnAdd = new AntdUI.Button { Text = "+ Thêm Xe", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Location = new Point(15, 12), Size = new Size(100, 36) };
            btnAdd.Click += (s, e) => ShowVehicleForm(null);

            var btnEdit = new AntdUI.Button { Text = "Sửa", Location = new Point(125, 12), Size = new Size(70, 36) };
            btnEdit.Click += (s, e) => EditSelected();

            var btnDelete = new AntdUI.Button { Text = "Xóa", ForeColor = AppColors.Red, Location = new Point(205, 12), Size = new Size(70, 36) };
            btnDelete.Click += (s, e) => DeleteSelected();

            panelTop.Controls.Add(btnAdd);
            panelTop.Controls.Add(btnEdit);
            panelTop.Controls.Add(btnDelete);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            try
            {
                string sql = @"
                    SELECT v.VehicleId, cu.FullName as CustomerName, r.RoomName,
                           CASE v.VehicleType WHEN 'XeMay' THEN N'Xe máy' WHEN 'XeDap' THEN N'Xe đạp' WHEN 'OTo' THEN N'Ô tô' ELSE v.VehicleType END as VehicleTypeName,
                           v.LicensePlate, v.Brand, v.Color, v.MonthlyFee, v.VehicleType, v.CustomerId
                    FROM Vehicles v
                    INNER JOIN Customers cu ON v.CustomerId = cu.CustomerId
                    LEFT JOIN Contracts c ON cu.CustomerId = c.CustomerId AND c.IsActive = 1
                    LEFT JOIN Rooms r ON c.RoomId = r.RoomId
                    ORDER BY v.VehicleId DESC";
                table.DataSource = DatabaseHelper.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void ShowVehicleForm(int? vehicleId)
        {
            DataRow existing = null;
            if (vehicleId.HasValue)
            {
                var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Vehicles WHERE VehicleId = @id",
                    new System.Data.SqlClient.SqlParameter("@id", vehicleId.Value));
                if (dt.Rows.Count > 0) existing = dt.Rows[0];
            }

            // Load danh sách khách
            var dtCustomers = DatabaseHelper.ExecuteQuery(@"
                SELECT cu.CustomerId, cu.FullName, r.RoomName
                FROM Customers cu
                LEFT JOIN Contracts c ON cu.CustomerId = c.CustomerId AND c.IsActive = 1
                LEFT JOIN Rooms r ON c.RoomId = r.RoomId
                ORDER BY cu.FullName");

            var form = new Form();
            form.Text = vehicleId.HasValue ? "Sửa thông tin xe" : "Thêm xe mới";
            form.Size = new Size(450, 420);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 8 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Chủ xe
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Chủ xe:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var cboCustomer = new AntdUI.Select { Dock = DockStyle.Fill };
            foreach (DataRow row in dtCustomers.Rows)
            {
                string display = row["FullName"].ToString() + (row["RoomName"] != DBNull.Value ? $" ({row["RoomName"]})" : "");
                cboCustomer.Items.Add(new SelectItem(display, row["CustomerId"]));
            }
            layout.Controls.Add(cboCustomer, 1, 0);

            // Loại xe
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Loại xe:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var cboType = new AntdUI.Select { Dock = DockStyle.Fill };
            cboType.Items.Add(new SelectItem("Xe máy", "XeMay"));
            cboType.Items.Add(new SelectItem("Xe đạp", "XeDap"));
            cboType.Items.Add(new SelectItem("Ô tô", "OTo"));
            layout.Controls.Add(cboType, 1, 1);

            // Biển số
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Biển số:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            var txtPlate = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "65B1-12345" };
            layout.Controls.Add(txtPlate, 1, 2);

            // Hãng xe
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Hãng xe:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
            var txtBrand = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "Honda, Yamaha..." };
            layout.Controls.Add(txtBrand, 1, 3);

            // Màu xe
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Màu xe:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 4);
            var txtColor = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "Đen, Trắng, Đỏ..." };
            layout.Controls.Add(txtColor, 1, 4);

            // Phí gửi xe
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Phí/tháng:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 5);
            var numFee = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = 100000 };
            layout.Controls.Add(numFee, 1, 5);

            // Auto fill phí theo loại xe
            cboType.SelectedIndexChanged += (s, e) => {
                string type = ((SelectItem)cboType.SelectedValue)?.Tag?.ToString();
                var fee = DatabaseHelper.ExecuteScalar($"SELECT SettingValue FROM Settings WHERE SettingKey = 'Phi{type}'");
                if (fee != null) numFee.Value = Convert.ToDecimal(fee);
            };

            // Fill data nếu edit
            if (existing != null)
            {
                for (int i = 0; i < cboCustomer.Items.Count; i++)
                    if (((SelectItem)cboCustomer.Items[i]).Tag.ToString() == existing["CustomerId"].ToString())
                    { cboCustomer.SelectedIndex = i; break; }
                
                string vtype = existing["VehicleType"]?.ToString();
                if (vtype == "XeMay") cboType.SelectedIndex = 0;
                else if (vtype == "XeDap") cboType.SelectedIndex = 1;
                else if (vtype == "OTo") cboType.SelectedIndex = 2;

                txtPlate.Text = existing["LicensePlate"]?.ToString();
                txtBrand.Text = existing["Brand"]?.ToString();
                txtColor.Text = existing["Color"]?.ToString();
                numFee.Value = existing["MonthlyFee"] != DBNull.Value ? Convert.ToDecimal(existing["MonthlyFee"]) : 0;
            }

            // Buttons
            var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 35) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Size = new Size(80, 35) };
            btnSave.Click += (s, e) => {
                if (cboCustomer.SelectedValue == null || cboType.SelectedValue == null)
                {
                    AntdUI.Message.warn(form, "Vui lòng chọn chủ xe và loại xe!");
                    return;
                }

                try
                {
                    int customerId = Convert.ToInt32(((SelectItem)cboCustomer.SelectedValue).Tag);
                    string vehicleType = ((SelectItem)cboType.SelectedValue).Tag.ToString();

                    if (vehicleId.HasValue)
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            @"UPDATE Vehicles SET CustomerId=@cid, VehicleType=@type, LicensePlate=@plate, Brand=@brand, Color=@color, MonthlyFee=@fee WHERE VehicleId=@id",
                            new System.Data.SqlClient.SqlParameter("@cid", customerId),
                            new System.Data.SqlClient.SqlParameter("@type", vehicleType),
                            new System.Data.SqlClient.SqlParameter("@plate", (object)txtPlate.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@brand", (object)txtBrand.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@color", (object)txtColor.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@fee", numFee.Value),
                            new System.Data.SqlClient.SqlParameter("@id", vehicleId.Value));
                        AntdUI.Message.success(form, "Cập nhật thành công!");
                    }
                    else
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            @"INSERT INTO Vehicles (CustomerId, VehicleType, LicensePlate, Brand, Color, MonthlyFee) VALUES (@cid, @type, @plate, @brand, @color, @fee)",
                            new System.Data.SqlClient.SqlParameter("@cid", customerId),
                            new System.Data.SqlClient.SqlParameter("@type", vehicleType),
                            new System.Data.SqlClient.SqlParameter("@plate", (object)txtPlate.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@brand", (object)txtBrand.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@color", (object)txtColor.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@fee", numFee.Value));
                        AntdUI.Message.success(form, "Thêm xe thành công!");
                    }
                    form.Close();
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                }
            };

            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSave);
            layout.Controls.Add(pnlButtons, 1, 7);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }

        private void EditSelected()
        {
            if (table.SelectedIndex < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn xe cần sửa!"); return; }
            var dt = table.DataSource as DataTable;
            int id = Convert.ToInt32(dt.Rows[table.SelectedIndex]["VehicleId"]);
            ShowVehicleForm(id);
        }

        private void DeleteSelected()
        {
            if (table.SelectedIndex < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn xe cần xóa!"); return; }
            var dt = table.DataSource as DataTable;
            int id = Convert.ToInt32(dt.Rows[table.SelectedIndex]["VehicleId"]);
            string plate = dt.Rows[table.SelectedIndex]["LicensePlate"]?.ToString() ?? "N/A";

            if (VNDialog.Confirm($"Xóa xe biển số '{plate}'?"))
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Vehicles WHERE VehicleId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", id));
                    AntdUI.Message.success(this.FindForm(), "Đã xóa!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
                }
            }
        }
    }
}
