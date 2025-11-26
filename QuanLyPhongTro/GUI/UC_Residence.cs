using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Residence : UserControl
    {
        private AntdUI.Table table;

        public UC_Residence()
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
            table.EmptyText = "Chưa có đăng ký tạm trú";
            table.Columns.Add(new Column("ResidenceId", "ID", ColumnAlign.Center) { Width = "50" });
            table.Columns.Add(new Column("CustomerName", "Họ tên", ColumnAlign.Left) { Width = "150" });
            table.Columns.Add(new Column("CCCD", "CCCD", ColumnAlign.Center) { Width = "120" });
            table.Columns.Add(new Column("RoomName", "Phòng", ColumnAlign.Center) { Width = "70" });
            table.Columns.Add(new Column("RegisterDate", "Ngày ĐK", ColumnAlign.Center) { Width = "100" });
            table.Columns.Add(new Column("ExpiryDate", "Hết hạn", ColumnAlign.Center) { Width = "100" });
            table.Columns.Add(new Column("PoliceStation", "Công an", ColumnAlign.Left) { Width = "180" });
            table.Columns.Add(new Column("StatusText", "Trạng thái", ColumnAlign.Center) { Width = "100" });
            this.Controls.Add(table);

            // 2. Toolbar
            var panelTop = new AntdUI.Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.White;

            var btnAdd = new AntdUI.Button { Text = "+ Đăng ký", Type = TTypeMini.Primary, BackColor = AppColors.Green, Location = new Point(15, 12), Size = new Size(110, 36) };
            btnAdd.Click += (s, e) => ShowForm(null);

            var btnEdit = new AntdUI.Button { Text = "Sửa", Location = new Point(135, 12), Size = new Size(70, 36) };
            btnEdit.Click += (s, e) => EditSelected();

            var btnDelete = new AntdUI.Button { Text = "Xóa", ForeColor = AppColors.Red, Location = new Point(215, 12), Size = new Size(70, 36) };
            btnDelete.Click += (s, e) => DeleteSelected();

            var btnCheckExpiry = new AntdUI.Button { Text = "Kiểm tra hết hạn", Location = new Point(300, 12), Size = new Size(130, 36) };
            btnCheckExpiry.Click += (s, e) => CheckExpiry();

            panelTop.Controls.Add(btnAdd);
            panelTop.Controls.Add(btnEdit);
            panelTop.Controls.Add(btnDelete);
            panelTop.Controls.Add(btnCheckExpiry);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            try
            {
                string sql = @"
                    SELECT tr.ResidenceId, cu.FullName as CustomerName, cu.CCCD, r.RoomName,
                           tr.RegisterDate, tr.ExpiryDate, tr.PoliceStation, tr.Status,
                           CASE 
                               WHEN tr.Status = 'ChuaDangKy' THEN N'Chưa ĐK'
                               WHEN tr.ExpiryDate < GETDATE() THEN N'Hết hạn'
                               ELSE N'Còn hạn' 
                           END as StatusText,
                           tr.CustomerId, tr.RoomId
                    FROM TemporaryResidence tr
                    INNER JOIN Customers cu ON tr.CustomerId = cu.CustomerId
                    INNER JOIN Rooms r ON tr.RoomId = r.RoomId
                    ORDER BY tr.ExpiryDate ASC";
                table.DataSource = DatabaseHelper.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void ShowForm(int? residenceId)
        {
            DataRow existing = null;
            if (residenceId.HasValue)
            {
                var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM TemporaryResidence WHERE ResidenceId = @id",
                    new System.Data.SqlClient.SqlParameter("@id", residenceId.Value));
                if (dt.Rows.Count > 0) existing = dt.Rows[0];
            }

            // Load khách đang thuê
            var dtCustomers = DatabaseHelper.ExecuteQuery(@"
                SELECT cu.CustomerId, cu.FullName, cu.CCCD, r.RoomId, r.RoomName
                FROM Contracts c
                INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                INNER JOIN Rooms r ON c.RoomId = r.RoomId
                WHERE c.IsActive = 1
                ORDER BY r.RoomName");

            if (dtCustomers.Rows.Count == 0 && !residenceId.HasValue)
            {
                AntdUI.Message.warn(this.FindForm(), "Không có khách đang thuê phòng!");
                return;
            }

            var form = new Form();
            form.Text = residenceId.HasValue ? "Sửa đăng ký tạm trú" : "Đăng ký tạm trú mới";
            form.Size = new Size(500, 400);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 7; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Khách thuê
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Khách thuê:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var cboCustomer = new AntdUI.Select { Dock = DockStyle.Fill };
            foreach (DataRow row in dtCustomers.Rows)
            {
                string display = $"{row["RoomName"]} - {row["FullName"]} ({row["CCCD"]})";
                // Lưu CustomerId|RoomId dạng string để tránh lỗi cast
                cboCustomer.Items.Add(new SelectItem(display, $"{row["CustomerId"]}|{row["RoomId"]}"));
            }
            if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;
            layout.Controls.Add(cboCustomer, 1, 0);

            // Ngày đăng ký
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày đăng ký:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var dateRegister = new AntdUI.DatePicker { Dock = DockStyle.Fill, Value = DateTime.Now };
            layout.Controls.Add(dateRegister, 1, 1);

            // Ngày hết hạn
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày hết hạn:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            var dateExpiry = new AntdUI.DatePicker { Dock = DockStyle.Fill, Value = DateTime.Now.AddYears(1) };
            layout.Controls.Add(dateExpiry, 1, 2);

            // Công an phường
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Công an P/X:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
            var txtPolice = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "Công an Phường An Khánh" };
            layout.Controls.Add(txtPolice, 1, 3);

            // Trạng thái
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Trạng thái:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 4);
            var cboStatus = new AntdUI.Select { Dock = DockStyle.Fill };
            cboStatus.Items.Add(new SelectItem("Đã đăng ký", "DaDangKy"));
            cboStatus.Items.Add(new SelectItem("Chưa đăng ký", "ChuaDangKy"));
            cboStatus.SelectedIndex = 0;
            layout.Controls.Add(cboStatus, 1, 4);

            // Ghi chú
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Ghi chú:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 5);
            var txtNote = new AntdUI.Input { Dock = DockStyle.Fill };
            layout.Controls.Add(txtNote, 1, 5);

            // Fill data nếu edit
            if (existing != null)
            {
                for (int i = 0; i < cboCustomer.Items.Count; i++)
                {
                    var item = cboCustomer.Items[i] as SelectItem;
                    string tag = item?.Tag?.ToString();
                    if (tag != null && tag.Split('|')[0] == existing["CustomerId"].ToString())
                    { cboCustomer.SelectedIndex = i; break; }
                }
                if (existing["RegisterDate"] != DBNull.Value) dateRegister.Value = Convert.ToDateTime(existing["RegisterDate"]);
                if (existing["ExpiryDate"] != DBNull.Value) dateExpiry.Value = Convert.ToDateTime(existing["ExpiryDate"]);
                txtPolice.Text = existing["PoliceStation"]?.ToString();
                if (existing["Status"]?.ToString() == "ChuaDangKy") cboStatus.SelectedIndex = 1;
                txtNote.Text = existing["Note"]?.ToString();
            }

            // Buttons
            var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 35) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(80, 35) };
            btnSave.Click += (s, e) => {
                if (cboCustomer.SelectedValue == null)
                {
                    AntdUI.Message.warn(form, "Vui lòng chọn khách thuê!");
                    return;
                }

                try
                {
                    var customerItem = cboCustomer.SelectedValue as SelectItem;
                    string selectedTag = customerItem?.Tag?.ToString();
                    if (string.IsNullOrEmpty(selectedTag)) { AntdUI.Message.warn(form, "Lỗi chọn khách!"); return; }
                    string[] parts = selectedTag.Split('|');
                    int customerId = Convert.ToInt32(parts[0]);
                    int roomId = Convert.ToInt32(parts[1]);
                    string status = cboStatus.SelectedIndex == 0 ? "DaDangKy" : "ChuaDangKy";

                    if (residenceId.HasValue)
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            @"UPDATE TemporaryResidence SET CustomerId=@cid, RoomId=@rid, RegisterDate=@rd, ExpiryDate=@ed, PoliceStation=@ps, Status=@st, Note=@note WHERE ResidenceId=@id",
                            new System.Data.SqlClient.SqlParameter("@cid", customerId),
                            new System.Data.SqlClient.SqlParameter("@rid", roomId),
                            new System.Data.SqlClient.SqlParameter("@rd", dateRegister.Value ?? DateTime.Now),
                            new System.Data.SqlClient.SqlParameter("@ed", dateExpiry.Value ?? DateTime.Now.AddYears(1)),
                            new System.Data.SqlClient.SqlParameter("@ps", (object)txtPolice.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@st", status),
                            new System.Data.SqlClient.SqlParameter("@note", (object)txtNote.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@id", residenceId.Value));
                        AntdUI.Message.success(form, "Cập nhật thành công!");
                    }
                    else
                    {
                        DatabaseHelper.ExecuteNonQuery(
                            @"INSERT INTO TemporaryResidence (CustomerId, RoomId, RegisterDate, ExpiryDate, PoliceStation, Status, Note) VALUES (@cid, @rid, @rd, @ed, @ps, @st, @note)",
                            new System.Data.SqlClient.SqlParameter("@cid", customerId),
                            new System.Data.SqlClient.SqlParameter("@rid", roomId),
                            new System.Data.SqlClient.SqlParameter("@rd", dateRegister.Value ?? DateTime.Now),
                            new System.Data.SqlClient.SqlParameter("@ed", dateExpiry.Value ?? DateTime.Now.AddYears(1)),
                            new System.Data.SqlClient.SqlParameter("@ps", (object)txtPolice.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@st", status),
                            new System.Data.SqlClient.SqlParameter("@note", (object)txtNote.Text ?? DBNull.Value));
                        AntdUI.Message.success(form, "Đăng ký thành công!");
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
            layout.Controls.Add(pnlButtons, 1, 6);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }

        private void EditSelected()
        {
            if (table.SelectedIndex < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn dòng cần sửa!"); return; }
            var dt = table.DataSource as DataTable;
            int id = Convert.ToInt32(dt.Rows[table.SelectedIndex]["ResidenceId"]);
            ShowForm(id);
        }

        private void DeleteSelected()
        {
            if (table.SelectedIndex < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn dòng cần xóa!"); return; }
            var dt = table.DataSource as DataTable;
            int id = Convert.ToInt32(dt.Rows[table.SelectedIndex]["ResidenceId"]);
            string name = dt.Rows[table.SelectedIndex]["CustomerName"].ToString();

            if (VNDialog.Confirm($"Xóa đăng ký tạm trú của '{name}'?"))
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM TemporaryResidence WHERE ResidenceId = @id",
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

        private void CheckExpiry()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT cu.FullName, r.RoomName, tr.ExpiryDate
                    FROM TemporaryResidence tr
                    INNER JOIN Customers cu ON tr.CustomerId = cu.CustomerId
                    INNER JOIN Rooms r ON tr.RoomId = r.RoomId
                    WHERE tr.ExpiryDate <= DATEADD(MONTH, 1, GETDATE()) AND tr.Status = 'DaDangKy'
                    ORDER BY tr.ExpiryDate");

                if (dt.Rows.Count == 0)
                {
                    AntdUI.Message.success(this.FindForm(), "Không có tạm trú nào sắp hết hạn!");
                    return;
                }

                string msg = "CÁC TẠM TRÚ SẮP HẾT HẠN (trong 30 ngày):\n\n";
                foreach (DataRow row in dt.Rows)
                {
                    DateTime exp = Convert.ToDateTime(row["ExpiryDate"]);
                    int days = (exp - DateTime.Now).Days;
                    msg += $"• {row["RoomName"]} - {row["FullName"]}: {exp:dd/MM/yyyy}";
                    if (days < 0) msg += " (ĐÃ HẾT HẠN!)";
                    else msg += $" (còn {days} ngày)";
                    msg += "\n";
                }

                MessageBox.Show(msg, "Cảnh báo hết hạn tạm trú", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }
    }
}
