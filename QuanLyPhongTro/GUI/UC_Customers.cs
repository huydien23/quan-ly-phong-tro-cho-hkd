using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Customers : UserControl
    {
        private AntdUI.Table table;
        private AntdUI.Input txtSearch;
        private int selectedCustomerId = -1;

        public UC_Customers()
        {
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            // 1. Bảng dữ liệu (thêm TRƯỚC)
            table = new AntdUI.Table();
            table.Dock = DockStyle.Fill;
            table.EmptyText = "Chưa có khách thuê nào";
            
            table.Columns.Add(new Column("CustomerId", "ID", ColumnAlign.Center) { Width = "50" });
            table.Columns.Add(new Column("FullName", "Họ và Tên", ColumnAlign.Left) { Width = "150" });
            table.Columns.Add(new Column("Phone", "SĐT", ColumnAlign.Center) { Width = "100" });
            table.Columns.Add(new Column("CCCD", "CCCD", ColumnAlign.Center) { Width = "120" });
            table.Columns.Add(new Column("Gender", "Giới tính", ColumnAlign.Center) { Width = "70" });
            table.Columns.Add(new Column("Job", "Nghề nghiệp", ColumnAlign.Left) { Width = "120" });
            table.Columns.Add(new Column("Address", "Địa chỉ", ColumnAlign.Left) { Width = "300" });
            table.CellClick += (s, e) => {
                if (e.Record is DataRowView drv)
                    selectedCustomerId = Convert.ToInt32(drv["CustomerId"]);
                else if (e.Record is DataRow dr)
                    selectedCustomerId = Convert.ToInt32(dr["CustomerId"]);
            };
            this.Controls.Add(table);

            // 2. Toolbar (thêm SAU)
            var panelTop = new AntdUI.Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.White;
            panelTop.Padding = new Padding(15);

            var btnAdd = new AntdUI.Button();
            btnAdd.Text = "+ Thêm Khách";
            btnAdd.Type = TTypeMini.Primary;
            btnAdd.BackColor = AppColors.Blue600;
            btnAdd.Location = new Point(15, 12);
            btnAdd.Size = new Size(120, 36);
            btnAdd.Click += (s, e) => ShowCustomerForm(null);

            var btnEdit = new AntdUI.Button();
            btnEdit.Text = "Sửa";
            btnEdit.Location = new Point(145, 12);
            btnEdit.Size = new Size(70, 36);
            btnEdit.Click += (s, e) => EditSelected();

            var btnDelete = new AntdUI.Button();
            btnDelete.Text = "Xóa";
            btnDelete.ForeColor = AppColors.Red;
            btnDelete.Location = new Point(225, 12);
            btnDelete.Size = new Size(70, 36);
            btnDelete.Click += (s, e) => DeleteSelected();

            var btnDetail = new AntdUI.Button();
            btnDetail.Text = "Chi tiết";
            btnDetail.Location = new Point(305, 12);
            btnDetail.Size = new Size(80, 36);
            btnDetail.Click += (s, e) => ShowDetail();

            txtSearch = new AntdUI.Input();
            txtSearch.PlaceholderText = "Tìm theo tên, SĐT, CCCD...";
            txtSearch.Location = new Point(400, 12);
            txtSearch.Size = new Size(250, 36);
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(txtSearch.Text); };

            panelTop.Controls.Add(btnAdd);
            panelTop.Controls.Add(btnEdit);
            panelTop.Controls.Add(btnDelete);
            panelTop.Controls.Add(btnDetail);
            panelTop.Controls.Add(txtSearch);
            this.Controls.Add(panelTop);
        }

        private void LoadData(string keyword = "")
        {
            try
            {
                string sql;
                DataTable dt;
                if (string.IsNullOrEmpty(keyword))
                {
                    sql = "SELECT * FROM Customers ORDER BY CustomerId DESC";
                    dt = DatabaseHelper.ExecuteQuery(sql);
                }
                else
                {
                    sql = "SELECT * FROM Customers WHERE FullName LIKE @kw OR Phone LIKE @kw OR CCCD LIKE @kw ORDER BY CustomerId DESC";
                    dt = DatabaseHelper.ExecuteQuery(sql, new System.Data.SqlClient.SqlParameter("@kw", "%" + keyword + "%"));
                }
                table.DataSource = dt;
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void ShowCustomerForm(int? customerId)
        {
            DataRow existingData = null;
            if (customerId.HasValue)
            {
                var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers WHERE CustomerId = @id",
                    new System.Data.SqlClient.SqlParameter("@id", customerId.Value));
                if (dt.Rows.Count > 0) existingData = dt.Rows[0];
            }

            var form = new Form();
            form.Text = customerId.HasValue ? "Sửa thông tin khách" : "Thêm khách thuê mới";
            form.Size = new Size(700, 550);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var mainPanel = new System.Windows.Forms.Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            mainPanel.Padding = new Padding(20);

            int y = 10;
            int labelWidth = 120;
            int inputWidth = 220;
            int col2X = 350;

            // === THÔNG TIN CÁ NHÂN ===
            AddSectionHeader(mainPanel, "THÔNG TIN CÁ NHÂN", ref y);

            // Row 1: Họ tên + Giới tính
            var txtName = AddInputRow(mainPanel, "Họ và tên *:", 10, ref y, labelWidth, inputWidth);
            var cboGender = new AntdUI.Select { Location = new Point(col2X + labelWidth, y - 40), Size = new Size(inputWidth, 35) };
            cboGender.Items.Add(new SelectItem("Nam", "Nam"));
            cboGender.Items.Add(new SelectItem("Nữ", "Nữ"));
            AddLabel(mainPanel, "Giới tính:", col2X, y - 40, labelWidth);
            mainPanel.Controls.Add(cboGender);

            // Row 2: CCCD + Ngày sinh
            var txtCCCD = AddInputRow(mainPanel, "Số CCCD *:", 10, ref y, labelWidth, inputWidth);
            var dateBirth = new AntdUI.DatePicker { Location = new Point(col2X + labelWidth, y - 40), Size = new Size(inputWidth, 35), PlaceholderText = "Ngày sinh" };
            AddLabel(mainPanel, "Ngày sinh:", col2X, y - 40, labelWidth);
            mainPanel.Controls.Add(dateBirth);

            // Row 3: SĐT + Email
            var txtPhone = AddInputRow(mainPanel, "Số điện thoại *:", 10, ref y, labelWidth, inputWidth);
            var txtEmail = new AntdUI.Input { Location = new Point(col2X + labelWidth, y - 40), Size = new Size(inputWidth, 35), PlaceholderText = "email@gmail.com" };
            AddLabel(mainPanel, "Email:", col2X, y - 40, labelWidth);
            mainPanel.Controls.Add(txtEmail);

            // Row 4: Nghề nghiệp + Nơi làm việc
            var txtJob = AddInputRow(mainPanel, "Nghề nghiệp:", 10, ref y, labelWidth, inputWidth);
            var txtWorkplace = new AntdUI.Input { Location = new Point(col2X + labelWidth, y - 40), Size = new Size(inputWidth, 35), PlaceholderText = "Công ty / Trường học" };
            AddLabel(mainPanel, "Nơi làm việc:", col2X, y - 40, labelWidth);
            mainPanel.Controls.Add(txtWorkplace);

            // Row 5: Địa chỉ thường trú (full width)
            y += 5;
            AddLabel(mainPanel, "Địa chỉ thường trú:", 10, y, labelWidth);
            var txtAddress = new AntdUI.Input { Location = new Point(10 + labelWidth, y), Size = new Size(520, 35), PlaceholderText = "Phường/Xã, Quận/Huyện, Tỉnh/TP" };
            mainPanel.Controls.Add(txtAddress);
            y += 45;

            // === LIÊN HỆ KHẨN CẤP ===
            y += 10;
            AddSectionHeader(mainPanel, "LIÊN HỆ KHẨN CẤP", ref y);

            var txtEmergencyName = AddInputRow(mainPanel, "Người liên hệ:", 10, ref y, labelWidth, inputWidth);
            var txtEmergencyPhone = new AntdUI.Input { Location = new Point(col2X + labelWidth, y - 40), Size = new Size(inputWidth, 35), PlaceholderText = "SĐT khẩn cấp" };
            AddLabel(mainPanel, "SĐT:", col2X, y - 40, labelWidth);
            mainPanel.Controls.Add(txtEmergencyPhone);

            // Ghi chú
            y += 5;
            AddLabel(mainPanel, "Ghi chú:", 10, y, labelWidth);
            var txtNote = new AntdUI.Input { Location = new Point(10 + labelWidth, y), Size = new Size(520, 35), PlaceholderText = "Ghi chú thêm (nếu có)" };
            mainPanel.Controls.Add(txtNote);
            y += 50;

            // Fill data nếu edit
            if (existingData != null)
            {
                txtName.Text = existingData["FullName"]?.ToString();
                txtCCCD.Text = existingData["CCCD"]?.ToString();
                txtPhone.Text = existingData["Phone"]?.ToString();
                txtEmail.Text = existingData["Email"]?.ToString();
                txtJob.Text = existingData["Job"]?.ToString();
                txtWorkplace.Text = existingData["Workplace"]?.ToString();
                txtAddress.Text = existingData["Address"]?.ToString();
                txtEmergencyName.Text = existingData["EmergencyContact"]?.ToString();
                txtEmergencyPhone.Text = existingData["EmergencyPhone"]?.ToString();
                txtNote.Text = existingData["Note"]?.ToString();
                if (existingData["Gender"]?.ToString() == "Nam") cboGender.SelectedIndex = 0;
                else if (existingData["Gender"]?.ToString() == "Nữ") cboGender.SelectedIndex = 1;
                if (existingData["DateOfBirth"] != DBNull.Value)
                    dateBirth.Value = Convert.ToDateTime(existingData["DateOfBirth"]);
            }

            // === BUTTONS ===
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(640, 45), FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 38) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Size = new Size(100, 38) };
            btnSave.Click += (s, e) => {
                // Validation
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    AntdUI.Message.warn(form, "Vui lòng nhập họ tên!"); return;
                }
                if (string.IsNullOrWhiteSpace(txtCCCD.Text) || txtCCCD.Text.Length != 12 || !long.TryParse(txtCCCD.Text, out _))
                {
                    AntdUI.Message.warn(form, "CCCD phải là 12 chữ số!"); return;
                }
                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 10 || !txtPhone.Text.StartsWith("0"))
                {
                    AntdUI.Message.warn(form, "SĐT phải là 10 số, bắt đầu bằng 0!"); return;
                }
                if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !txtEmail.Text.Contains("@"))
                {
                    AntdUI.Message.warn(form, "Email không hợp lệ!"); return;
                }

                try
                {
                    string gender = cboGender.SelectedIndex >= 0 ? (cboGender.SelectedIndex == 0 ? "Nam" : "Nữ") : null;
                    
                    if (customerId.HasValue)
                    {
                        // Update
                        string sql = @"UPDATE Customers SET FullName=@name, Phone=@phone, CCCD=@cccd, Email=@email,
                                       DateOfBirth=@dob, Gender=@gender, Job=@job, Workplace=@workplace,
                                       Address=@addr, EmergencyContact=@ec, EmergencyPhone=@ep, Note=@note
                                       WHERE CustomerId=@id";
                        DatabaseHelper.ExecuteNonQuery(sql,
                            new System.Data.SqlClient.SqlParameter("@name", txtName.Text),
                            new System.Data.SqlClient.SqlParameter("@phone", txtPhone.Text),
                            new System.Data.SqlClient.SqlParameter("@cccd", txtCCCD.Text),
                            new System.Data.SqlClient.SqlParameter("@email", (object)txtEmail.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@dob", dateBirth.Value.HasValue ? (object)dateBirth.Value.Value : DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@gender", (object)gender ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@job", (object)txtJob.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@workplace", (object)txtWorkplace.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@addr", (object)txtAddress.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@ec", (object)txtEmergencyName.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@ep", (object)txtEmergencyPhone.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@note", (object)txtNote.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@id", customerId.Value));
                        AntdUI.Message.success(form, "Cập nhật thành công!");
                    }
                    else
                    {
                        // Insert
                        string sql = @"INSERT INTO Customers (FullName, Phone, CCCD, Email, DateOfBirth, Gender, Job, Workplace, Address, EmergencyContact, EmergencyPhone, Note)
                                       VALUES (@name, @phone, @cccd, @email, @dob, @gender, @job, @workplace, @addr, @ec, @ep, @note)";
                        DatabaseHelper.ExecuteNonQuery(sql,
                            new System.Data.SqlClient.SqlParameter("@name", txtName.Text),
                            new System.Data.SqlClient.SqlParameter("@phone", txtPhone.Text),
                            new System.Data.SqlClient.SqlParameter("@cccd", txtCCCD.Text),
                            new System.Data.SqlClient.SqlParameter("@email", (object)txtEmail.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@dob", dateBirth.Value.HasValue ? (object)dateBirth.Value.Value : DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@gender", (object)gender ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@job", (object)txtJob.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@workplace", (object)txtWorkplace.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@addr", (object)txtAddress.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@ec", (object)txtEmergencyName.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@ep", (object)txtEmergencyPhone.Text ?? DBNull.Value),
                            new System.Data.SqlClient.SqlParameter("@note", (object)txtNote.Text ?? DBNull.Value));
                        AntdUI.Message.success(form, "Thêm khách thành công!");
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
            mainPanel.Controls.Add(pnlButtons);

            form.Controls.Add(mainPanel);
            form.ShowDialog(this.FindForm());
        }

        private void AddSectionHeader(System.Windows.Forms.Panel panel, string text, ref int y)
        {
            var lbl = new System.Windows.Forms.Label { Text = text, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(400, 25) };
            panel.Controls.Add(lbl);
            y += 30;
        }

        private void AddLabel(System.Windows.Forms.Panel panel, string text, int x, int y, int width)
        {
            var lbl = new System.Windows.Forms.Label { Text = text, TextAlign = ContentAlignment.MiddleRight, Location = new Point(x, y + 5), Size = new Size(width, 25) };
            panel.Controls.Add(lbl);
        }

        private AntdUI.Input AddInputRow(System.Windows.Forms.Panel panel, string label, int x, ref int y, int labelWidth, int inputWidth)
        {
            AddLabel(panel, label, x, y, labelWidth);
            var txt = new AntdUI.Input { Location = new Point(x + labelWidth, y), Size = new Size(inputWidth, 35) };
            panel.Controls.Add(txt);
            y += 45;
            return txt;
        }

        private void EditSelected()
        {
            if (selectedCustomerId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn khách cần sửa!"); return; }
            ShowCustomerForm(selectedCustomerId);
        }

        private void DeleteSelected()
        {
            if (selectedCustomerId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn khách cần xóa!"); return; }
            
            // Lấy tên khách để hiển thị
            var customer = DatabaseHelper.ExecuteQuery("SELECT FullName FROM Customers WHERE CustomerId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedCustomerId));
            if (customer.Rows.Count == 0) return;
            string name = customer.Rows[0]["FullName"].ToString();

            if (VNDialog.Confirm($"Xóa khách '{name}'?"))
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Customers WHERE CustomerId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", selectedCustomerId));
                    AntdUI.Message.success(this.FindForm(), "Đã xóa!");
                    selectedCustomerId = -1;
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(this.FindForm(), "Không thể xóa (có thể khách đang có hợp đồng): " + ex.Message);
                }
            }
        }

        private void ShowDetail()
        {
            if (selectedCustomerId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn khách cần xem!"); return; }
            
            // Query trực tiếp từ database theo ID đã chọn
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers WHERE CustomerId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedCustomerId));
            if (dt.Rows.Count == 0) return;
            var row = dt.Rows[0];

            string info = $@"
═══════════════════════════════════════
           THÔNG TIN KHÁCH THUÊ
═══════════════════════════════════════
Họ và tên:      {row["FullName"]}
Số CCCD:        {row["CCCD"]}
Số điện thoại:  {row["Phone"]}
Email:          {row["Email"]}
Ngày sinh:      {(row["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(row["DateOfBirth"]).ToString("dd/MM/yyyy") : "N/A")}
Giới tính:      {row["Gender"]}
Nghề nghiệp:    {row["Job"]}
Nơi làm việc:   {row["Workplace"]}
Địa chỉ:        {row["Address"]}
───────────────────────────────────────
LIÊN HỆ KHẨN CẤP
Người liên hệ:  {row["EmergencyContact"]}
SĐT:            {row["EmergencyPhone"]}
───────────────────────────────────────
Ghi chú:        {row["Note"]}
";
            MessageBox.Show(info, "Chi tiết khách thuê", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}