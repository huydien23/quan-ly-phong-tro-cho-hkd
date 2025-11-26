using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyPhongTro.GUI
{
    public class UC_Contracts : UserControl
    {
        private AntdUI.Table table;
        private DataTable dtRooms;
        private DataTable dtCustomers;
        private int selectedContractId = -1;

        public UC_Contracts()
        {
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            // 1. Table (thêm TRƯỚC)
            table = new AntdUI.Table();
            table.Dock = DockStyle.Fill;
            table.EmptyText = "Chưa có hợp đồng nào";
            table.Columns.Add(new Column("ContractId", "Mã HĐ", ColumnAlign.Center) { Width = "70" });
            table.Columns.Add(new Column("RoomName", "Phòng", ColumnAlign.Center) { Width = "80" });
            table.Columns.Add(new Column("CustomerName", "Khách thuê", ColumnAlign.Left) { Width = "180" });
            table.Columns.Add(new Column("StartDate", "Ngày vào", ColumnAlign.Center) { Width = "100" });
            table.Columns.Add(new Column("Deposit", "Tiền cọc", ColumnAlign.Right) { Width = "120" });
            table.Columns.Add(new Column("MonthlyRent", "Tiền thuê/tháng", ColumnAlign.Right) { Width = "130" });
            table.Columns.Add(new Column("Status", "Trạng thái", ColumnAlign.Center) { Width = "100" });
            table.CellClick += (s, e) => {
                if (e.Record is DataRowView drv)
                    selectedContractId = Convert.ToInt32(drv["ContractId"]);
                else if (e.Record is DataRow dr)
                    selectedContractId = Convert.ToInt32(dr["ContractId"]);
            };
            this.Controls.Add(table);

            // 2. Toolbar (thêm SAU)
            var panelTop = new AntdUI.Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.White;
            panelTop.Padding = new Padding(15);

            var btnAdd = new AntdUI.Button();
            btnAdd.Text = "+ Tạo Hợp Đồng";
            btnAdd.Type = TTypeMini.Primary;
            btnAdd.BackColor = AppColors.Green;
            btnAdd.Location = new Point(15, 12);
            btnAdd.Size = new Size(140, 36);
            btnAdd.Click += (s, e) => ShowContractForm();

            var btnRenew = new AntdUI.Button();
            btnRenew.Text = "Gia hạn HĐ";
            btnRenew.Type = TTypeMini.Primary;
            btnRenew.BackColor = AppColors.Blue600;
            btnRenew.Location = new Point(170, 12);
            btnRenew.Size = new Size(110, 36);
            btnRenew.Click += (s, e) => RenewContract();

            var btnEnd = new AntdUI.Button();
            btnEnd.Text = "Kết thúc HĐ";
            btnEnd.Location = new Point(290, 12);
            btnEnd.Size = new Size(110, 36);
            btnEnd.Click += (s, e) => EndSelectedContract();

            var btnRefund = new AntdUI.Button();
            btnRefund.Text = "Hoàn cọc";
            btnRefund.ForeColor = Color.Orange;
            btnRefund.Location = new Point(410, 12);
            btnRefund.Size = new Size(100, 36);
            btnRefund.Click += (s, e) => RefundDeposit();

            var btnEdit = new AntdUI.Button();
            btnEdit.Text = "Sửa";
            btnEdit.Location = new Point(520, 12);
            btnEdit.Size = new Size(70, 36);
            btnEdit.Click += (s, e) => EditContract();

            var btnDelete = new AntdUI.Button();
            btnDelete.Text = "Xóa";
            btnDelete.ForeColor = AppColors.Red;
            btnDelete.Location = new Point(600, 12);
            btnDelete.Size = new Size(70, 36);
            btnDelete.Click += (s, e) => DeleteContract();

            var btnDetail = new AntdUI.Button();
            btnDetail.Text = "Chi tiết";
            btnDetail.Location = new Point(680, 12);
            btnDetail.Size = new Size(80, 36);
            btnDetail.Click += (s, e) => ShowContractDetail();

            panelTop.Controls.Add(btnAdd);
            panelTop.Controls.Add(btnRenew);
            panelTop.Controls.Add(btnEnd);
            panelTop.Controls.Add(btnRefund);
            panelTop.Controls.Add(btnEdit);
            panelTop.Controls.Add(btnDelete);
            panelTop.Controls.Add(btnDetail);
            this.Controls.Add(panelTop);
        }

        private void LoadData()
        {
            try
            {
                string sql = @"SELECT c.ContractId, r.RoomName, cu.FullName as CustomerName, 
                               c.StartDate, c.Deposit, c.MonthlyRent,
                               CASE WHEN c.IsActive = 1 THEN N'Đang thuê' ELSE N'Đã kết thúc' END as Status,
                               c.RoomId, c.CustomerId, c.IsActive
                               FROM Contracts c
                               INNER JOIN Rooms r ON c.RoomId = r.RoomId
                               INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                               ORDER BY c.IsActive DESC, c.StartDate DESC";
                table.DataSource = DatabaseHelper.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
            }
        }

        private void LoadComboData()
        {
            // Load phòng trống
            dtRooms = DatabaseHelper.ExecuteQuery("SELECT RoomId, RoomName, Price FROM Rooms WHERE Status = 'Trong' ORDER BY RoomName");
            // Load khách chưa có hợp đồng hoặc tất cả khách
            dtCustomers = DatabaseHelper.ExecuteQuery("SELECT CustomerId, FullName, Phone FROM Customers ORDER BY FullName");
        }

        private void ShowContractForm(int? contractId = null)
        {
            LoadComboData();

            // Kiểm tra điều kiện
            if (dtRooms.Rows.Count == 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Không có phòng trống! Vui lòng thêm phòng trước.");
                return;
            }

            if (dtCustomers.Rows.Count == 0)
            {
                if (VNDialog.Confirm("Chưa có khách hàng nào! Bạn có muốn thêm khách mới ngay?"))
                {
                    ShowQuickAddCustomerForm(() => ShowContractForm(null));
                }
                return;
            }

            var form = new Form();
            form.Text = "Tạo Hợp Đồng Mới";
            form.Size = new Size(500, 500);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.ColumnCount = 3;
            layout.RowCount = 7;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            for (int i = 0; i < 7; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Chọn phòng
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Phòng:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var cboRoom = new AntdUI.Select { Dock = DockStyle.Fill };
            foreach (DataRow row in dtRooms.Rows)
                cboRoom.Items.Add(new SelectItem(row["RoomName"].ToString() + " - " + Convert.ToDecimal(row["Price"]).ToString("N0") + "đ", row["RoomId"]));
            if (cboRoom.Items.Count > 0) cboRoom.SelectedIndex = 0;
            layout.Controls.Add(cboRoom, 1, 0);
            layout.SetColumnSpan(cboRoom, 2);

            // Chọn khách + nút thêm mới
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Khách thuê:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var cboCustomer = new AntdUI.Select { Dock = DockStyle.Fill };
            foreach (DataRow row in dtCustomers.Rows)
                cboCustomer.Items.Add(new SelectItem(row["FullName"].ToString() + " - " + row["Phone"].ToString(), row["CustomerId"]));
            if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;
            layout.Controls.Add(cboCustomer, 1, 1);

            var btnAddCustomer = new AntdUI.Button { Text = "+ Mới", Size = new Size(70, 35), Dock = DockStyle.Fill };
            btnAddCustomer.Click += (s, e) => {
                ShowQuickAddCustomerForm(() => {
                    // Reload customer list
                    dtCustomers = DatabaseHelper.ExecuteQuery("SELECT CustomerId, FullName, Phone FROM Customers ORDER BY CustomerId DESC");
                    cboCustomer.Items.Clear();
                    foreach (DataRow row in dtCustomers.Rows)
                        cboCustomer.Items.Add(new SelectItem(row["FullName"].ToString() + " - " + row["Phone"].ToString(), row["CustomerId"]));
                    if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;
                });
            };
            layout.Controls.Add(btnAddCustomer, 2, 1);

            // Ngày bắt đầu
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày vào:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            var dateStart = new AntdUI.DatePicker { Dock = DockStyle.Fill, Value = DateTime.Now };
            layout.Controls.Add(dateStart, 1, 2);
            layout.SetColumnSpan(dateStart, 2);

            // Tiền thuê
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền thuê:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
            var numRent = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = 0 };
            // Auto fill giá phòng khi chọn
            cboRoom.SelectedIndexChanged += (s, e) => {
                if (cboRoom.SelectedIndex >= 0 && cboRoom.SelectedIndex < dtRooms.Rows.Count)
                {
                    numRent.Value = Convert.ToDecimal(dtRooms.Rows[cboRoom.SelectedIndex]["Price"]);
                }
            };
            if (dtRooms.Rows.Count > 0) numRent.Value = Convert.ToDecimal(dtRooms.Rows[0]["Price"]);
            layout.Controls.Add(numRent, 1, 3);
            layout.SetColumnSpan(numRent, 2);

            // Tiền cọc
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền cọc:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 4);
            var numDeposit = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = 0 };
            layout.Controls.Add(numDeposit, 1, 4);
            layout.SetColumnSpan(numDeposit, 2);

            // Ghi chú
            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Ghi chú:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 5);
            var txtNote = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "Ghi chú thêm (nếu có)" };
            layout.Controls.Add(txtNote, 1, 5);
            layout.SetColumnSpan(txtNote, 2);

            // Buttons
            var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 35) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Tạo HĐ", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(100, 35) };
            btnSave.Click += (s, e) => {
                try
                {
                    // Lấy ID từ index thay vì cast SelectItem
                    if (cboRoom.SelectedIndex < 0 || cboRoom.SelectedIndex >= dtRooms.Rows.Count)
                    {
                        AntdUI.Message.warn(form, "Vui lòng chọn phòng!");
                        return;
                    }
                    if (cboCustomer.SelectedIndex < 0 || cboCustomer.SelectedIndex >= dtCustomers.Rows.Count)
                    {
                        AntdUI.Message.warn(form, "Vui lòng chọn khách!");
                        return;
                    }

                    int roomId = Convert.ToInt32(dtRooms.Rows[cboRoom.SelectedIndex]["RoomId"]);
                    int customerId = Convert.ToInt32(dtCustomers.Rows[cboCustomer.SelectedIndex]["CustomerId"]);

                    // Insert hợp đồng
                    string sql = @"INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, MonthlyRent, IsActive) 
                                   VALUES (@roomId, @customerId, @startDate, @deposit, @rent, 1)";
                    DatabaseHelper.ExecuteNonQuery(sql,
                        new System.Data.SqlClient.SqlParameter("@roomId", roomId),
                        new System.Data.SqlClient.SqlParameter("@customerId", customerId),
                        new System.Data.SqlClient.SqlParameter("@startDate", dateStart.Value ?? DateTime.Now),
                        new System.Data.SqlClient.SqlParameter("@deposit", numDeposit.Value),
                        new System.Data.SqlClient.SqlParameter("@rent", numRent.Value));

                    // Update trạng thái phòng
                    DatabaseHelper.ExecuteNonQuery("UPDATE Rooms SET Status = 'DangThue' WHERE RoomId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", roomId));

                    AntdUI.Message.success(form, "Tạo hợp đồng thành công!");
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
            layout.SetColumnSpan(pnlButtons, 2);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }

        private void ShowQuickAddCustomerForm(Action onSuccess)
        {
            var form = new Form();
            form.Text = "Thêm khách nhanh";
            form.Size = new Size(400, 280);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 2, RowCount = 5 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Họ tên *:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var txtName = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "Nguyễn Văn A" };
            layout.Controls.Add(txtName, 1, 0);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "SĐT *:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var txtPhone = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "0912345678" };
            layout.Controls.Add(txtPhone, 1, 1);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "CCCD *:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
            var txtCCCD = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "12 số" };
            layout.Controls.Add(txtCCCD, 1, 2);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Địa chỉ:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
            var txtAddress = new AntdUI.Input { Dock = DockStyle.Fill, PlaceholderText = "Địa chỉ thường trú" };
            layout.Controls.Add(txtAddress, 1, 3);

            var pnlBtn = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(70, 35) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(70, 35) };
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text)) { AntdUI.Message.warn(form, "Nhập họ tên!"); return; }
                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 10) { AntdUI.Message.warn(form, "SĐT phải 10 số!"); return; }
                if (string.IsNullOrWhiteSpace(txtCCCD.Text) || txtCCCD.Text.Length != 12) { AntdUI.Message.warn(form, "CCCD phải 12 số!"); return; }

                try
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Customers (FullName, Phone, CCCD, Address) VALUES (@n, @p, @c, @a)",
                        new System.Data.SqlClient.SqlParameter("@n", txtName.Text),
                        new System.Data.SqlClient.SqlParameter("@p", txtPhone.Text),
                        new System.Data.SqlClient.SqlParameter("@c", txtCCCD.Text),
                        new System.Data.SqlClient.SqlParameter("@a", (object)txtAddress.Text ?? DBNull.Value));
                    AntdUI.Message.success(form, "Đã thêm khách!");
                    form.Close();
                    onSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                }
            };

            pnlBtn.Controls.Add(btnCancel);
            pnlBtn.Controls.Add(btnSave);
            layout.Controls.Add(pnlBtn, 1, 4);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }

        private void EndSelectedContract()
        {
            if (selectedContractId < 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Vui lòng chọn hợp đồng cần kết thúc!");
                return;
            }

            var data = DatabaseHelper.ExecuteQuery("SELECT c.*, r.RoomName FROM Contracts c INNER JOIN Rooms r ON c.RoomId = r.RoomId WHERE c.ContractId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
            if (data.Rows.Count == 0) return;

            var row = data.Rows[0];
            if (Convert.ToInt32(row["IsActive"]) == 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Hợp đồng này đã kết thúc!");
                return;
            }

            int contractId = Convert.ToInt32(row["ContractId"]);
            int roomId = Convert.ToInt32(row["RoomId"]);
            string roomName = row["RoomName"].ToString();

            if (VNDialog.Confirm($"Kết thúc hợp đồng phòng {roomName}?"))
            {
                try
                {
                    // Update hợp đồng
                    DatabaseHelper.ExecuteNonQuery("UPDATE Contracts SET IsActive = 0, EndDate = GETDATE() WHERE ContractId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", contractId));
                    // Update phòng về trống
                    DatabaseHelper.ExecuteNonQuery("UPDATE Rooms SET Status = 'Trong' WHERE RoomId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", roomId));

                    AntdUI.Message.success(this.FindForm(), "Đã kết thúc hợp đồng!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
                }
            }
        }

        private void RenewContract()
        {
            if (selectedContractId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn hợp đồng cần gia hạn!"); return; }

            var data = DatabaseHelper.ExecuteQuery("SELECT c.*, r.RoomName, cu.FullName as CustomerName FROM Contracts c INNER JOIN Rooms r ON c.RoomId = r.RoomId INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId WHERE c.ContractId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
            if (data.Rows.Count == 0) return;
            var row = data.Rows[0];
            if (Convert.ToInt32(row["IsActive"]) == 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Không thể gia hạn hợp đồng đã kết thúc!");
                return;
            }

            int contractId = Convert.ToInt32(row["ContractId"]);
            string roomName = row["RoomName"].ToString();
            string customerName = row["CustomerName"].ToString();
            decimal currentRent = Convert.ToDecimal(row["MonthlyRent"]);

            var form = new Form { Text = "Gia hạn hợp đồng", Size = new Size(400, 280), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };

            var pnl = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblInfo = new System.Windows.Forms.Label { Text = $"Phòng: {roomName}\nKhách: {customerName}", Location = new Point(20, 10), Size = new Size(340, 40), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pnl.Controls.Add(lblInfo);

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Giá thuê mới:", Location = new Point(20, 60), Size = new Size(100, 25) });
            var numNewRent = new AntdUI.InputNumber { Location = new Point(130, 55), Size = new Size(200, 35), Value = currentRent };
            pnl.Controls.Add(numNewRent);

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Ghi chú:", Location = new Point(20, 105), Size = new Size(100, 25) });
            var txtNote = new AntdUI.Input { Location = new Point(130, 100), Size = new Size(200, 35), PlaceholderText = "Lý do tăng/giữ giá" };
            pnl.Controls.Add(txtNote);

            var btnSave = new AntdUI.Button { Text = "Gia hạn", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Location = new Point(130, 155), Size = new Size(100, 38) };
            btnSave.Click += (s, e) => {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("UPDATE Contracts SET MonthlyRent = @rent, StartDate = GETDATE() WHERE ContractId = @id",
                        new System.Data.SqlClient.SqlParameter("@rent", numNewRent.Value),
                        new System.Data.SqlClient.SqlParameter("@id", contractId));
                    AntdUI.Message.success(form, $"Đã gia hạn! Giá mới: {numNewRent.Value:N0}đ");
                    form.Close();
                    LoadData();
                }
                catch (Exception ex) { AntdUI.Message.error(form, "Lỗi: " + ex.Message); }
            };
            pnl.Controls.Add(btnSave);

            var btnCancel = new AntdUI.Button { Text = "Hủy", Location = new Point(240, 155), Size = new Size(80, 38) };
            btnCancel.Click += (s, e) => form.Close();
            pnl.Controls.Add(btnCancel);

            form.Controls.Add(pnl);
            form.ShowDialog(this.FindForm());
        }

        private void RefundDeposit()
        {
            if (selectedContractId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn hợp đồng cần hoàn cọc!"); return; }

            var data = DatabaseHelper.ExecuteQuery("SELECT c.*, r.RoomName, cu.FullName as CustomerName FROM Contracts c INNER JOIN Rooms r ON c.RoomId = r.RoomId INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId WHERE c.ContractId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
            if (data.Rows.Count == 0) return;
            var row = data.Rows[0];

            if (Convert.ToInt32(row["IsActive"]) == 1)
            {
                AntdUI.Message.warn(this.FindForm(), "Phải kết thúc hợp đồng trước khi hoàn cọc!");
                return;
            }

            string roomName = row["RoomName"].ToString();
            string customerName = row["CustomerName"].ToString();
            decimal deposit = Convert.ToDecimal(row["Deposit"]);

            if (deposit <= 0)
            {
                AntdUI.Message.info(this.FindForm(), "Hợp đồng này không có tiền cọc!");
                return;
            }

            var form = new Form { Text = "Hoàn tiền cọc", Size = new Size(420, 320), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };

            var pnl = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblInfo = new System.Windows.Forms.Label { Text = $"Phòng: {roomName}\nKhách: {customerName}\nTiền cọc: {deposit:N0} VNĐ", Location = new Point(20, 10), Size = new Size(360, 55), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pnl.Controls.Add(lblInfo);

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Khấu trừ:", Location = new Point(20, 75), Size = new Size(80, 25) });
            var numDeduct = new AntdUI.InputNumber { Location = new Point(110, 70), Size = new Size(150, 35), Value = 0, Minimum = 0, Maximum = deposit };
            pnl.Controls.Add(numDeduct);
            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "(Hư hỏng, nợ...)", Location = new Point(270, 75), Size = new Size(100, 25), ForeColor = Color.Gray });

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Thực hoàn:", Location = new Point(20, 115), Size = new Size(80, 25) });
            var lblRefund = new System.Windows.Forms.Label { Text = $"{deposit:N0} VNĐ", Location = new Point(110, 115), Size = new Size(200, 25), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Green };
            pnl.Controls.Add(lblRefund);

            numDeduct.ValueChanged += (s, val) => {
                decimal refund = deposit - numDeduct.Value;
                lblRefund.Text = $"{refund:N0} VNĐ";
                lblRefund.ForeColor = refund > 0 ? AppColors.Green : AppColors.Red;
            };

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Ghi chú:", Location = new Point(20, 155), Size = new Size(80, 25) });
            var txtNote = new AntdUI.Input { Location = new Point(110, 150), Size = new Size(260, 35), PlaceholderText = "Lý do khấu trừ (nếu có)" };
            pnl.Controls.Add(txtNote);

            var btnConfirm = new AntdUI.Button { Text = "Xác nhận hoàn cọc", Type = TTypeMini.Primary, BackColor = AppColors.Green, Location = new Point(110, 205), Size = new Size(150, 40) };
            btnConfirm.Click += (s, e) => {
                decimal refundAmount = deposit - numDeduct.Value;
                string msg = $"BIÊN NHẬN HOÀN CỌC\n" +
                             $"══════════════════════\n" +
                             $"Phòng: {roomName}\n" +
                             $"Khách: {customerName}\n" +
                             $"Tiền cọc: {deposit:N0} VNĐ\n" +
                             $"Khấu trừ: {numDeduct.Value:N0} VNĐ\n" +
                             $"Thực hoàn: {refundAmount:N0} VNĐ\n" +
                             $"Ghi chú: {txtNote.Text}\n" +
                             $"══════════════════════\n" +
                             $"Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}";

                MessageBox.Show(msg, "Biên nhận hoàn cọc", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AntdUI.Message.success(form, "Đã hoàn cọc thành công!");
                form.Close();
            };
            pnl.Controls.Add(btnConfirm);

            form.Controls.Add(pnl);
            form.ShowDialog(this.FindForm());
        }

        private void EditContract()
        {
            if (selectedContractId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn hợp đồng cần sửa!"); return; }

            var data = DatabaseHelper.ExecuteQuery("SELECT * FROM Contracts WHERE ContractId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
            if (data.Rows.Count == 0) return;
            var contract = data.Rows[0];

            if (Convert.ToInt32(contract["IsActive"]) == 0)
            {
                AntdUI.Message.warn(this.FindForm(), "Không thể sửa hợp đồng đã kết thúc!");
                return;
            }

            var form = new Form { Text = "Sửa hợp đồng", Size = new Size(400, 280), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            var pnl = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền thuê:", Location = new Point(20, 20), Size = new Size(100, 25) });
            var numRent = new AntdUI.InputNumber { Location = new Point(130, 15), Size = new Size(200, 35), Value = Convert.ToDecimal(contract["MonthlyRent"]) };
            pnl.Controls.Add(numRent);

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền cọc:", Location = new Point(20, 65), Size = new Size(100, 25) });
            var numDeposit = new AntdUI.InputNumber { Location = new Point(130, 60), Size = new Size(200, 35), Value = Convert.ToDecimal(contract["Deposit"]) };
            pnl.Controls.Add(numDeposit);

            pnl.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày vào:", Location = new Point(20, 110), Size = new Size(100, 25) });
            var dateStart = new AntdUI.DatePicker { Location = new Point(130, 105), Size = new Size(200, 35), Value = Convert.ToDateTime(contract["StartDate"]) };
            pnl.Controls.Add(dateStart);

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Location = new Point(130, 160), Size = new Size(100, 38) };
            btnSave.Click += (s, e) => {
                try
                {
                    DatabaseHelper.ExecuteNonQuery("UPDATE Contracts SET MonthlyRent = @rent, Deposit = @dep, StartDate = @date WHERE ContractId = @id",
                        new System.Data.SqlClient.SqlParameter("@rent", numRent.Value),
                        new System.Data.SqlClient.SqlParameter("@dep", numDeposit.Value),
                        new System.Data.SqlClient.SqlParameter("@date", dateStart.Value ?? DateTime.Now),
                        new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
                    AntdUI.Message.success(form, "Đã cập nhật!");
                    form.Close();
                    LoadData();
                }
                catch (Exception ex) { AntdUI.Message.error(form, "Lỗi: " + ex.Message); }
            };
            pnl.Controls.Add(btnSave);

            var btnCancel = new AntdUI.Button { Text = "Hủy", Location = new Point(240, 160), Size = new Size(80, 38) };
            btnCancel.Click += (s, e) => form.Close();
            pnl.Controls.Add(btnCancel);

            form.Controls.Add(pnl);
            form.ShowDialog(this.FindForm());
        }

        private void DeleteContract()
        {
            if (selectedContractId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn hợp đồng cần xóa!"); return; }

            var data = DatabaseHelper.ExecuteQuery("SELECT c.ContractId, r.RoomName, c.IsActive FROM Contracts c INNER JOIN Rooms r ON c.RoomId = r.RoomId WHERE c.ContractId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
            if (data.Rows.Count == 0) return;
            var contract = data.Rows[0];

            if (Convert.ToInt32(contract["IsActive"]) == 1)
            {
                AntdUI.Message.warn(this.FindForm(), "Phải kết thúc hợp đồng trước khi xóa!");
                return;
            }

            string roomName = contract["RoomName"].ToString();
            if (VNDialog.Confirm($"Xóa hợp đồng phòng {roomName}?"))
            {
                try
                {
                    // Kiểm tra có hóa đơn liên quan không
                    var invoiceCount = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Invoices WHERE ContractId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
                    if (Convert.ToInt32(invoiceCount) > 0)
                    {
                        AntdUI.Message.warn(this.FindForm(), "Không thể xóa! Hợp đồng có hóa đơn liên quan.");
                        return;
                    }

                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Contracts WHERE ContractId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", selectedContractId));
                    AntdUI.Message.success(this.FindForm(), "Đã xóa hợp đồng!");
                    selectedContractId = -1;
                    LoadData();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(this.FindForm(), "Lỗi: " + ex.Message);
                }
            }
        }

        private void ShowContractDetail()
        {
            if (selectedContractId < 0) { AntdUI.Message.warn(this.FindForm(), "Chọn hợp đồng cần xem!"); return; }

            // Lấy thêm thông tin chi tiết
            var detail = DatabaseHelper.ExecuteQuery(@"
                SELECT c.*, r.RoomName, r.Price as RoomPrice, cu.FullName, cu.Phone, cu.CCCD, cu.Address,
                       (SELECT COUNT(*) FROM Invoices WHERE ContractId = c.ContractId) as TotalInvoices,
                       (SELECT ISNULL(SUM(TotalAmount),0) FROM Invoices WHERE ContractId = c.ContractId AND Status = 'DaThu') as TotalPaid
                FROM Contracts c
                INNER JOIN Rooms r ON c.RoomId = r.RoomId
                INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                WHERE c.ContractId = @id",
                new System.Data.SqlClient.SqlParameter("@id", selectedContractId));

            if (detail.Rows.Count == 0) return;
            var d = detail.Rows[0];

            DateTime start = Convert.ToDateTime(d["StartDate"]);
            int months = ((DateTime.Now.Year - start.Year) * 12) + DateTime.Now.Month - start.Month;

            string info = $@"
══════════════════════════════════════════
           CHI TIẾT HỢP ĐỒNG #{selectedContractId}
══════════════════════════════════════════
THÔNG TIN PHÒNG
  Phòng:         {d["RoomName"]}
  Tiền thuê:     {Convert.ToDecimal(d["MonthlyRent"]):N0} VNĐ/tháng
  Tiền cọc:      {Convert.ToDecimal(d["Deposit"]):N0} VNĐ
──────────────────────────────────────────
THÔNG TIN KHÁCH
  Họ tên:        {d["FullName"]}
  SĐT:           {d["Phone"]}
  CCCD:          {d["CCCD"]}
  Địa chỉ:       {d["Address"]}
──────────────────────────────────────────
THỜI GIAN
  Ngày vào:      {start:dd/MM/yyyy}
  Số tháng thuê: {months} tháng
  Trạng thái:    {(Convert.ToInt32(d["IsActive"]) == 1 ? "Đang thuê" : "Đã kết thúc")}
──────────────────────────────────────────
THANH TOÁN
  Số hóa đơn:    {d["TotalInvoices"]}
  Đã thu:        {Convert.ToDecimal(d["TotalPaid"]):N0} VNĐ
══════════════════════════════════════════";

            MessageBox.Show(info, "Chi tiết hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}