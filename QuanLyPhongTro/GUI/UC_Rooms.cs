using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Rooms : UserControl
    {
        private FlowLayoutPanel flowPanel;

        public UC_Rooms()
        {
            this.BackColor = AppColors.Blue50;
            InitUI();
            LoadRooms();
        }

        private void InitUI()
        {
            // 1. Flow Grid (thêm TRƯỚC)
            flowPanel = new FlowLayoutPanel();
            flowPanel.Dock = DockStyle.Fill;
            flowPanel.AutoScroll = true;
            flowPanel.Padding = new Padding(15);
            flowPanel.BackColor = AppColors.Blue50;
            this.Controls.Add(flowPanel);

            // 2. Toolbar (thêm SAU)
            var toolBar = new AntdUI.Panel();
            toolBar.Height = 60;
            toolBar.Dock = DockStyle.Top;
            toolBar.BackColor = Color.White;
            toolBar.Padding = new Padding(15);

            var btnAdd = new AntdUI.Button();
            btnAdd.Text = "+ Thêm Phòng";
            btnAdd.Type = TTypeMini.Primary;
            btnAdd.BackColor = AppColors.Blue600;
            btnAdd.Size = new Size(130, 36);
            btnAdd.Location = new Point(15, 12);
            btnAdd.Click += (s, e) => ShowAddRoomForm();

            var lblInfo = new AntdUI.Label();
            lblInfo.Text = "Click vào phòng để xem chi tiết";
            lblInfo.ForeColor = Color.Gray;
            lblInfo.Location = new Point(160, 18);
            lblInfo.Size = new Size(250, 25);

            toolBar.Controls.Add(btnAdd);
            toolBar.Controls.Add(lblInfo);
            this.Controls.Add(toolBar);
        }

        private void LoadRooms()
        {
            try
            {
                // Query mới: Lấy thêm tên khách thuê (nếu có)
                var sql = @"
                    SELECT r.*, cu.FullName AS TenantName
                    FROM Rooms r
                    LEFT JOIN Contracts c ON r.RoomId = c.RoomId AND c.IsActive = 1
                    LEFT JOIN Customers cu ON c.CustomerId = cu.CustomerId
                    ORDER BY r.RoomName";

                var dt = DatabaseHelper.ExecuteQuery(sql);
                flowPanel.Controls.Clear();
                
                if (dt.Rows.Count == 0)
                {
                    var lblEmpty = new System.Windows.Forms.Label();
                    lblEmpty.Text = "Chưa có phòng nào. Bấm '+ Thêm Phòng' để tạo mới.";
                    lblEmpty.ForeColor = Color.Gray;
                    lblEmpty.Font = new Font("Segoe UI", 11);
                    lblEmpty.AutoSize = true;
                    flowPanel.Controls.Add(lblEmpty);
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    int roomId = Convert.ToInt32(row["RoomId"]);
                    string name = row["RoomName"].ToString();
                    string status = row["Status"]?.ToString() ?? "Trong";
                    decimal price = row["Price"] != DBNull.Value ? Convert.ToDecimal(row["Price"]) : 0;
                    string tenantName = row["TenantName"]?.ToString();

                    var card = CreateRoomCard(roomId, name, status, price, tenantName);
                    flowPanel.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private AntdUI.Panel CreateRoomCard(int roomId, string name, string status, decimal price, string tenantName)
        {
            bool isRented = status == "DangThue";
            bool isDeposited = status == "DaCoc";
            
            // Màu dịu hơn (Muted Colors)
            Color statusColor = isRented ? Color.FromArgb(244, 63, 94) : // Rose-500
                               (isDeposited ? Color.FromArgb(245, 158, 11) : // Amber-500
                                              Color.FromArgb(16, 185, 129)); // Emerald-500
            
            string statusText = isRented ? "Đang thuê" : (isDeposited ? "Đã cọc" : "Phòng trống");

            var card = new AntdUI.Panel();
            card.Size = new Size(200, 130);
            card.Radius = 10;
            card.Back = Color.White;
            card.Shadow = 5;
            card.Margin = new Padding(10);
            card.Cursor = Cursors.Hand;

            // 1. Dải màu trạng thái bên trái
            var sideStrip = new System.Windows.Forms.Panel();
            sideStrip.Size = new Size(6, 130);
            sideStrip.BackColor = statusColor;
            sideStrip.Dock = DockStyle.Left;

            // 2. Tên phòng
            var lblName = new System.Windows.Forms.Label();
            lblName.Text = name;
            lblName.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(30, 41, 59);
            lblName.Location = new Point(18, 10);
            lblName.Size = new Size(170, 30);

            // 3. Giá phòng
            var lblPrice = new System.Windows.Forms.Label();
            lblPrice.Text = $"{price:N0}";
            lblPrice.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPrice.ForeColor = Color.FromArgb(100, 116, 139);
            lblPrice.Location = new Point(18, 40);
            lblPrice.Size = new Size(170, 25);

            // 4. Đường kẻ mờ
            var sepLine = new System.Windows.Forms.Panel();
            sepLine.Size = new Size(160, 1);
            sepLine.BackColor = Color.FromArgb(241, 245, 249);
            sepLine.Location = new Point(18, 75);

            // 5. Thông tin trạng thái / Khách thuê
            var lblInfo = new System.Windows.Forms.Label();
            lblInfo.Location = new Point(18, 85);
            lblInfo.Size = new Size(170, 25);
            lblInfo.Font = new Font("Segoe UI", 9);

            if (isRented && !string.IsNullOrEmpty(tenantName))
            {
                // Hiển thị tên khách nếu đang thuê
                lblInfo.Text = $"👤 {tenantName}";
                lblInfo.ForeColor = Color.FromArgb(51, 65, 85); // Màu chữ đậm hơn chút
                lblInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
            else
            {
                // Hiển thị trạng thái nếu trống/cọc
                lblInfo.Text = statusText;
                lblInfo.ForeColor = statusColor;
                lblInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            card.Controls.Add(lblName);
            card.Controls.Add(lblPrice);
            card.Controls.Add(sepLine);
            card.Controls.Add(lblInfo);
            card.Controls.Add(sideStrip);

            // Sự kiện Click
            card.Click += (s, e) => ShowRoomDetail(roomId, name, status, price);
            foreach (Control c in card.Controls)
            {
                c.Click += (s, e) => ShowRoomDetail(roomId, name, status, price);
            }

            return card;
        }

        private void ShowRoomDetail(int roomId, string name, string status, decimal price)
        {
            bool isRented = status == "DangThue";
            bool isDeposited = status == "DaCoc";
            bool isEmpty = status == "Trong";
            
            // Query thông tin khách thuê/đặt cọc
            DataRow contractInfo = null;
            if (isRented || isDeposited)
            {
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT cu.CustomerId, cu.FullName, cu.Phone, cu.CCCD, cu.Address, cu.Email,
                           c.ContractId, c.StartDate, c.Deposit, c.MonthlyRent, c.IsActive
                    FROM Contracts c
                    INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                    WHERE c.RoomId = @roomId ORDER BY c.ContractId DESC",
                    new System.Data.SqlClient.SqlParameter("@roomId", roomId));
                if (dt.Rows.Count > 0) contractInfo = dt.Rows[0];
            }

            var form = new Form();
            form.Text = "Chi tiết " + name;
            form.Size = new Size(500, (isRented || isDeposited) && contractInfo != null ? 550 : 320);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.BackColor = Color.White;

            var mainPanel = new System.Windows.Forms.Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            mainPanel.Padding = new Padding(20);

            int y = 10;
            Font boldFont = new Font("Segoe UI", 10, FontStyle.Bold);
            Font normalFont = new Font("Segoe UI", 10);

            // === THÔNG TIN PHÒNG ===
            var lblRoomHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN PHÒNG", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(450, 25) };
            mainPanel.Controls.Add(lblRoomHeader); y += 30;

            AddInfoRow(mainPanel, "Tên phòng:", name, ref y, boldFont, normalFont);
            AddInfoRow(mainPanel, "Giá thuê:", price.ToString("N0") + " VNĐ/tháng", ref y, boldFont, normalFont);
            
            string statusText = isEmpty ? "Phòng trống" : (isDeposited ? "Đã đặt cọc (chờ vào ở)" : "Đang có khách thuê");
            Color statusColor = isEmpty ? AppColors.Green : (isDeposited ? Color.Orange : AppColors.Red);
            AddInfoRow(mainPanel, "Trạng thái:", statusText, ref y, boldFont, normalFont, statusColor);

            // === THÔNG TIN NGƯỜI THUÊ/ĐẶT CỌC ===
            if ((isRented || isDeposited) && contractInfo != null)
            {
                y += 15;
                var separator = new System.Windows.Forms.Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, y), Size = new Size(440, 2) };
                mainPanel.Controls.Add(separator); y += 15;

                string headerText = isDeposited ? "KHÁCH ĐẶT CỌC" : "NGƯỜI ĐANG THUÊ";
                var lblCustomerHeader = new System.Windows.Forms.Label { Text = headerText, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = isDeposited ? Color.Orange : AppColors.Green, Location = new Point(10, y), Size = new Size(450, 25) };
                mainPanel.Controls.Add(lblCustomerHeader); y += 30;

                AddInfoRow(mainPanel, "Họ và tên:", contractInfo["FullName"]?.ToString() ?? "", ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Số điện thoại:", contractInfo["Phone"]?.ToString() ?? "", ref y, boldFont, normalFont);
                
                string cccd = contractInfo["CCCD"]?.ToString();
                AddInfoRow(mainPanel, "Số CCCD:", string.IsNullOrEmpty(cccd) ? "(Chưa bổ sung)" : cccd, ref y, boldFont, normalFont, string.IsNullOrEmpty(cccd) ? Color.Gray : AppColors.Blue600);
                
                AddInfoRow(mainPanel, "Địa chỉ:", contractInfo["Address"]?.ToString() ?? "", ref y, boldFont, normalFont);

                y += 15;
                var separator2 = new System.Windows.Forms.Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, y), Size = new Size(440, 2) };
                mainPanel.Controls.Add(separator2); y += 15;

                var lblContractHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN HỢP ĐỒNG", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Blue600, Location = new Point(10, y), Size = new Size(450, 25) };
                mainPanel.Controls.Add(lblContractHeader); y += 30;

                DateTime startDate = contractInfo["StartDate"] != DBNull.Value ? Convert.ToDateTime(contractInfo["StartDate"]) : DateTime.MinValue;
                decimal deposit = contractInfo["Deposit"] != DBNull.Value ? Convert.ToDecimal(contractInfo["Deposit"]) : 0;
                decimal rent = contractInfo["MonthlyRent"] != DBNull.Value ? Convert.ToDecimal(contractInfo["MonthlyRent"]) : 0;

                AddInfoRow(mainPanel, isDeposited ? "Ngày dự kiến:" : "Ngày vào ở:", startDate.ToString("dd/MM/yyyy"), ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Tiền cọc:", deposit.ToString("N0") + " VNĐ", ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Tiền thuê/tháng:", rent.ToString("N0") + " VNĐ", ref y, boldFont, normalFont);
            }

            // === BUTTONS ===
            y += 20;
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(440, 45), FlowDirection = FlowDirection.LeftToRight };
            
            // NẾU PHÒNG TRỐNG - Nút ĐẶT CỌC + CHO THUÊ NGAY
            if (isEmpty)
            {
                var btnRentNow = new AntdUI.Button { Text = "Cho thuê ngay", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Size = new Size(120, 35) };
                btnRentNow.Click += (s, e) => {
                    form.Close();
                    ShowRentNowForm(roomId, name, price);
                };
                pnlButtons.Controls.Add(btnRentNow);

                var btnDeposit = new AntdUI.Button { Text = "Đặt cọc", Type = TTypeMini.Warn, Size = new Size(80, 35) };
                btnDeposit.Click += (s, e) => {
                    form.Close();
                    ShowRentRoomForm(roomId, name, price);
                };
                pnlButtons.Controls.Add(btnDeposit);
            }
            
            // NẾU ĐÃ CỌC - Nút XÁC NHẬN VÀO Ở
            if (isDeposited && contractInfo != null)
            {
                var btnConfirm = new AntdUI.Button { Text = "Xác nhận vào ở", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(140, 35) };
                btnConfirm.Click += (s, e) => {
                    form.Close();
                    ShowConfirmMoveInForm(roomId, name, contractInfo);
                };
                pnlButtons.Controls.Add(btnConfirm);
                
                var btnCancelDeposit = new AntdUI.Button { Text = "Hủy cọc", ForeColor = AppColors.Red, Size = new Size(80, 35) };
                btnCancelDeposit.Click += (s, e) => {
                    if (VNDialog.Confirm("Hủy đặt cọc phòng này?"))
                    {
                        try
                        {
                            int contractId = Convert.ToInt32(contractInfo["ContractId"]);
                            DatabaseHelper.ExecuteNonQuery("DELETE FROM Contracts WHERE ContractId = @id", new System.Data.SqlClient.SqlParameter("@id", contractId));
                            DatabaseHelper.ExecuteNonQuery("UPDATE Rooms SET Status = 'Trong' WHERE RoomId = @id", new System.Data.SqlClient.SqlParameter("@id", roomId));
                            AntdUI.Message.success(form, "Đã hủy cọc!");
                            form.Close();
                            LoadRooms();
                        }
                        catch (Exception ex) { AntdUI.Message.error(form, "Lỗi: " + ex.Message); }
                    }
                };
                pnlButtons.Controls.Add(btnCancelDeposit);
            }

            // Nút Sửa phòng (chỉ khi trống)
            if (isEmpty)
            {
                var btnEdit = new AntdUI.Button { Text = "Sửa", Size = new Size(60, 35) };
                btnEdit.Click += (s, e) => { form.Close(); ShowEditRoomForm(roomId); };
                pnlButtons.Controls.Add(btnEdit);

                var btnDelete = new AntdUI.Button { Text = "Xóa", ForeColor = AppColors.Red, Size = new Size(60, 35) };
                btnDelete.Click += (s, e) => {
                    if (VNDialog.Confirm($"Xóa {name}?"))
                    {
                        try
                        {
                            DatabaseHelper.ExecuteNonQuery("DELETE FROM Rooms WHERE RoomId = @id", new System.Data.SqlClient.SqlParameter("@id", roomId));
                            AntdUI.Message.success(form, "Đã xóa!");
                            form.Close();
                            LoadRooms();
                        }
                        catch (Exception ex) { AntdUI.Message.error(form, "Lỗi: " + ex.Message); }
                    }
                };
                pnlButtons.Controls.Add(btnDelete);
            }

            var btnClose = new AntdUI.Button { Text = "Đóng", Size = new Size(60, 35) };
            btnClose.Click += (s, e) => form.Close();
            pnlButtons.Controls.Add(btnClose);
            mainPanel.Controls.Add(pnlButtons);

            form.Controls.Add(mainPanel);
            form.ShowDialog(this.FindForm());
        }

        // Form xác nhận vào ở - bổ sung CCCD và thông tin còn thiếu
        private void ShowConfirmMoveInForm(int roomId, string roomName, DataRow contractInfo)
        {
            int contractId = Convert.ToInt32(contractInfo["ContractId"]);
            int customerId = Convert.ToInt32(contractInfo["CustomerId"]);
            
            var form = new Form();
            form.Text = $"Xác nhận vào ở - {roomName}";
            form.Size = new Size(500, 450);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var mainPanel = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            int y = 10;
            int lblW = 110;

            // Header
            var lblHeader = new System.Windows.Forms.Label { 
                Text = $"Khách: {contractInfo["FullName"]} - {contractInfo["Phone"]}", 
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                ForeColor = AppColors.Blue900, 
                Location = new Point(10, y), Size = new Size(460, 25) 
            };
            mainPanel.Controls.Add(lblHeader); y += 35;

            // Bổ sung thông tin
            var lblInfo = new System.Windows.Forms.Label { Text = "BỔ SUNG THÔNG TIN (bắt buộc khi vào ở)", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Orange, Location = new Point(10, y), Size = new Size(460, 22) };
            mainPanel.Controls.Add(lblInfo); y += 28;

            // CCCD
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "CCCD *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtCCCD = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(200, 35), PlaceholderText = "12 số", Text = contractInfo["CCCD"]?.ToString() ?? "" };
            mainPanel.Controls.Add(txtCCCD);
            y += 42;

            // Email
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Email:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtEmail = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(300, 35), PlaceholderText = "email@gmail.com", Text = contractInfo["Email"]?.ToString() ?? "" };
            mainPanel.Controls.Add(txtEmail);
            y += 42;

            // Ngày vào ở thực tế
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày vào ở:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var dateActual = new AntdUI.DatePicker { Location = new Point(lblW + 10, y), Size = new Size(150, 35), Value = DateTime.Now };
            mainPanel.Controls.Add(dateActual);
            y += 42;

            // Tiền thuê
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền thuê:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            decimal rent = contractInfo["MonthlyRent"] != DBNull.Value ? Convert.ToDecimal(contractInfo["MonthlyRent"]) : 0;
            var numRent = new AntdUI.InputNumber { Location = new Point(lblW + 10, y), Size = new Size(150, 35), Value = rent };
            mainPanel.Controls.Add(numRent);
            y += 55;

            // Buttons
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(460, 50), FlowDirection = FlowDirection.RightToLeft };

            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 40) };
            btnCancel.Click += (s, e) => form.Close();

            var btnConfirm = new AntdUI.Button { Text = "Xác nhận vào ở", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(150, 40) };
            btnConfirm.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtCCCD.Text) || txtCCCD.Text.Length != 12)
                {
                    AntdUI.Message.warn(form, "CCCD phải 12 số!");
                    return;
                }

                try
                {
                    // 1. Cập nhật thông tin khách
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Customers SET CCCD = @cccd, Email = @email WHERE CustomerId = @id",
                        new System.Data.SqlClient.SqlParameter("@cccd", txtCCCD.Text),
                        new System.Data.SqlClient.SqlParameter("@email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text),
                        new System.Data.SqlClient.SqlParameter("@id", customerId));

                    // 2. Cập nhật hợp đồng = Active
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Contracts SET IsActive = 1, StartDate = @sd, MonthlyRent = @rent WHERE ContractId = @id",
                        new System.Data.SqlClient.SqlParameter("@sd", dateActual.Value ?? DateTime.Now),
                        new System.Data.SqlClient.SqlParameter("@rent", numRent.Value),
                        new System.Data.SqlClient.SqlParameter("@id", contractId));

                    // 3. Cập nhật phòng = Đang thuê
                    DatabaseHelper.ExecuteNonQuery("UPDATE Rooms SET Status = 'DangThue' WHERE RoomId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", roomId));

                    AntdUI.Message.success(form, $"Khách đã vào ở {roomName}!");
                    form.Close();
                    LoadRooms();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                }
            };

            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnConfirm);
            mainPanel.Controls.Add(pnlButtons);

            form.Controls.Add(mainPanel);
            form.ShowDialog(this.FindForm());
        }

        private void AddInfoRow(System.Windows.Forms.Panel panel, string label, string value, ref int y, Font boldFont, Font normalFont, Color? valueColor = null)
        {
            var lblLabel = new System.Windows.Forms.Label { Text = label, Font = boldFont, Location = new Point(10, y), Size = new Size(130, 22) };
            var lblValue = new System.Windows.Forms.Label { Text = value, Font = normalFont, ForeColor = valueColor ?? Color.Black, Location = new Point(145, y), Size = new Size(300, 22) };
            panel.Controls.Add(lblLabel);
            panel.Controls.Add(lblValue);
            y += 26;
        }

        private void ShowEditRoomForm(int roomId)
        {
            var room = DatabaseHelper.ExecuteQuery("SELECT * FROM Rooms WHERE RoomId = @id",
                new System.Data.SqlClient.SqlParameter("@id", roomId));
            if (room.Rows.Count == 0) return;
            var r = room.Rows[0];

            var form = new Form();
            form.Text = "Sửa phòng " + r["RoomName"];
            form.Size = new Size(400, 300);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.ColumnCount = 2;
            layout.RowCount = 3;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 2; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Tên phòng:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var txtName = new AntdUI.Input { Text = r["RoomName"].ToString(), Dock = DockStyle.Fill };
            layout.Controls.Add(txtName, 1, 0);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Giá thuê:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var txtPrice = new AntdUI.InputNumber { Dock = DockStyle.Fill, Value = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]) : 0 };
            layout.Controls.Add(txtPrice, 1, 1);

            var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 35) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Size = new Size(80, 35) };
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    AntdUI.Message.warn(form, "Vui lòng nhập tên phòng!");
                    return;
                }
                try
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE Rooms SET RoomName=@name, Price=@price WHERE RoomId=@id",
                        new System.Data.SqlClient.SqlParameter("@name", txtName.Text),
                        new System.Data.SqlClient.SqlParameter("@price", txtPrice.Value),
                        new System.Data.SqlClient.SqlParameter("@id", roomId));
                    AntdUI.Message.success(form, "Cập nhật thành công!");
                    form.Close();
                    LoadRooms();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                }
            };

            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSave);
            layout.Controls.Add(pnlButtons, 1, 2);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }

        private void ShowRentNowForm(int roomId, string roomName, decimal roomPrice)
        {
            var form = new Form();
            form.Text = $"Cho thuê ngay - {roomName}";
            form.Size = new Size(520, 520);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var mainPanel = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
            int y = 10;
            int lblW = 100;

            // === THÔNG TIN PHÒNG ===
            var lblRoomInfo = new System.Windows.Forms.Label { 
                Text = $"{roomName}  •  {roomPrice:N0} VNĐ/tháng", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                ForeColor = AppColors.Blue600, 
                Location = new Point(10, y), Size = new Size(460, 28) 
            };
            mainPanel.Controls.Add(lblRoomInfo); y += 35;

            // === THÔNG TIN KHÁCH ===
            var lblCustomerHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN KHÁCH THUÊ", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(460, 22) };
            mainPanel.Controls.Add(lblCustomerHeader); y += 26;

            // Họ tên + SĐT
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Họ tên *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtName = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(170, 35), PlaceholderText = "Nguyễn Văn A" };
            mainPanel.Controls.Add(txtName);
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "SĐT *:", Location = new Point(300, y + 5), Size = new Size(50, 22) });
            var txtPhone = new AntdUI.Input { Location = new Point(355, y), Size = new Size(120, 35), PlaceholderText = "0912345678" };
            mainPanel.Controls.Add(txtPhone);
            y += 40;

            // CCCD + Email
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "CCCD *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtCCCD = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(170, 35), PlaceholderText = "12 số" };
            mainPanel.Controls.Add(txtCCCD);
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Email:", Location = new Point(300, y + 5), Size = new Size(50, 22) });
            var txtEmail = new AntdUI.Input { Location = new Point(355, y), Size = new Size(120, 35) };
            mainPanel.Controls.Add(txtEmail);
            y += 40;

            // Địa chỉ
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Địa chỉ:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtAddress = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(365, 35), PlaceholderText = "Địa chỉ thường trú" };
            mainPanel.Controls.Add(txtAddress);
            y += 48;

            // === THÔNG TIN HỢP ĐỒNG ===
            var lblContractHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN HỢP ĐỒNG", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(460, 22) };
            mainPanel.Controls.Add(lblContractHeader); y += 26;

            // Ngày vào + Tiền thuê
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày vào ở:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var dateStart = new AntdUI.DatePicker { Location = new Point(lblW + 10, y), Size = new Size(140, 35), Value = DateTime.Now };
            mainPanel.Controls.Add(dateStart);
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền thuê:", Location = new Point(270, y + 5), Size = new Size(80, 22) });
            var numRent = new AntdUI.InputNumber { Location = new Point(350, y), Size = new Size(125, 35), Value = roomPrice };
            mainPanel.Controls.Add(numRent);
            y += 40;

            // Tiền cọc
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền cọc:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var numDeposit = new AntdUI.InputNumber { Location = new Point(lblW + 10, y), Size = new Size(140, 35), Value = roomPrice };
            mainPanel.Controls.Add(numDeposit);
            y += 55;

            // === BUTTONS ===
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(460, 50), FlowDirection = FlowDirection.RightToLeft };

            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 40) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Cho thuê ngay", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Size = new Size(140, 40) };
            btnSave.Click += (s, e) => {
                // Validation
                if (string.IsNullOrWhiteSpace(txtName.Text)) { AntdUI.Message.warn(form, "Nhập họ tên!"); return; }
                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 10) { AntdUI.Message.warn(form, "SĐT phải 10 số!"); return; }
                if (string.IsNullOrWhiteSpace(txtCCCD.Text) || txtCCCD.Text.Length != 12) { AntdUI.Message.warn(form, "CCCD phải 12 số!"); return; }

                try
                {
                    // 1. Tạo khách hàng đầy đủ
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Customers (FullName, Phone, CCCD, Email, Address) VALUES (@n, @p, @c, @e, @a)",
                        new System.Data.SqlClient.SqlParameter("@n", txtName.Text),
                        new System.Data.SqlClient.SqlParameter("@p", txtPhone.Text),
                        new System.Data.SqlClient.SqlParameter("@c", txtCCCD.Text),
                        new System.Data.SqlClient.SqlParameter("@e", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text),
                        new System.Data.SqlClient.SqlParameter("@a", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text));

                    // 2. Lấy CustomerId
                    var customerIdObj = DatabaseHelper.ExecuteScalar("SELECT MAX(CustomerId) FROM Customers");
                    int customerId = Convert.ToInt32(customerIdObj);

                    // 3. Tạo hợp đồng Active luôn
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, MonthlyRent, IsActive) VALUES (@rid, @cid, @sd, @dep, @rent, 1)",
                        new System.Data.SqlClient.SqlParameter("@rid", roomId),
                        new System.Data.SqlClient.SqlParameter("@cid", customerId),
                        new System.Data.SqlClient.SqlParameter("@sd", dateStart.Value ?? DateTime.Now),
                        new System.Data.SqlClient.SqlParameter("@dep", numDeposit.Value),
                        new System.Data.SqlClient.SqlParameter("@rent", numRent.Value));

                    // 4. Phòng = Đang thuê
                    DatabaseHelper.ExecuteNonQuery("UPDATE Rooms SET Status = 'DangThue' WHERE RoomId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", roomId));

                    AntdUI.Message.success(form, $"Đã cho thuê {roomName} thành công!");
                    form.Close();
                    LoadRooms();
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

        private void ShowRentRoomForm(int roomId, string roomName, decimal roomPrice)
        {
            var form = new Form();
            form.Text = $"Đặt cọc {roomName}";
            form.Size = new Size(480, 480);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var mainPanel = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
            int y = 10;

            // === THÔNG TIN PHÒNG ===
            var lblRoomInfo = new System.Windows.Forms.Label { 
                Text = $" {roomName}  •  {roomPrice:N0} VNĐ/tháng", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                ForeColor = AppColors.Green, 
                Location = new Point(10, y), 
                Size = new Size(440, 30) 
            };
            mainPanel.Controls.Add(lblRoomInfo); y += 40;

            // === THÔNG TIN KHÁCH (CƠ BẢN) ===
            var lblCustomerHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN KHÁCH ĐẶT CỌC", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(440, 22) };
            mainPanel.Controls.Add(lblCustomerHeader); y += 28;

            int lblW = 100;

            // Họ tên
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Họ tên *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtName = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(320, 35), PlaceholderText = "Nguyễn Văn A" };
            mainPanel.Controls.Add(txtName);
            y += 42;

            // SĐT
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "SĐT *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtPhone = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(180, 35), PlaceholderText = "0912345678" };
            mainPanel.Controls.Add(txtPhone);
            y += 42;

            // Địa chỉ
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Địa chỉ:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtAddress = new AntdUI.Input { Location = new Point(lblW + 10, y), Size = new Size(320, 35), PlaceholderText = "Địa chỉ liên hệ" };
            mainPanel.Controls.Add(txtAddress);
            y += 50;

            // === THÔNG TIN ĐẶT CỌC ===
            var lblDepositHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN ĐẶT CỌC", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(440, 22) };
            mainPanel.Controls.Add(lblDepositHeader); y += 28;

            // Ngày cọc + Tiền cọc
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày cọc:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var dateDeposit = new AntdUI.DatePicker { Location = new Point(lblW + 10, y), Size = new Size(140, 35), Value = DateTime.Now };
            mainPanel.Controls.Add(dateDeposit);

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền cọc:", Location = new Point(270, y + 5), Size = new Size(70, 22) });
            var numDeposit = new AntdUI.InputNumber { Location = new Point(340, y), Size = new Size(100, 35), Value = roomPrice };
            mainPanel.Controls.Add(numDeposit);
            y += 42;

            // Ngày dự kiến vào ở
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày vào ở:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var dateMoveIn = new AntdUI.DatePicker { Location = new Point(lblW + 10, y), Size = new Size(140, 35), Value = DateTime.Now.AddDays(7) };
            mainPanel.Controls.Add(dateMoveIn);

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "(dự kiến)", ForeColor = Color.Gray, Location = new Point(260, y + 8), Size = new Size(80, 22) });
            y += 55;

            // === BUTTONS ===
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(440, 50), FlowDirection = FlowDirection.RightToLeft };

            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 40) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Xác nhận đặt cọc", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(150, 40) };
            btnSave.Click += (s, e) => {
                // Validation
                if (string.IsNullOrWhiteSpace(txtName.Text)) { AntdUI.Message.warn(form, "Nhập họ tên khách!"); return; }
                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 10) { AntdUI.Message.warn(form, "SĐT phải 10 số!"); return; }

                try
                {
                    // 1. Tạo khách hàng (thông tin cơ bản, CCCD để trống)
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Customers (FullName, Phone, Address) VALUES (@n, @p, @a)",
                        new System.Data.SqlClient.SqlParameter("@n", txtName.Text),
                        new System.Data.SqlClient.SqlParameter("@p", txtPhone.Text),
                        new System.Data.SqlClient.SqlParameter("@a", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text));

                    // 2. Lấy CustomerId vừa tạo
                    var customerIdObj = DatabaseHelper.ExecuteScalar("SELECT MAX(CustomerId) FROM Customers");
                    int customerId = Convert.ToInt32(customerIdObj);

                    // 3. Tạo hợp đồng (IsActive = 0 = Đang chờ vào ở)
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, MonthlyRent, IsActive) VALUES (@rid, @cid, @sd, @dep, @rent, 0)",
                        new System.Data.SqlClient.SqlParameter("@rid", roomId),
                        new System.Data.SqlClient.SqlParameter("@cid", customerId),
                        new System.Data.SqlClient.SqlParameter("@sd", dateMoveIn.Value ?? DateTime.Now),
                        new System.Data.SqlClient.SqlParameter("@dep", numDeposit.Value),
                        new System.Data.SqlClient.SqlParameter("@rent", roomPrice));

                    // 4. Cập nhật phòng = Đã cọc
                    DatabaseHelper.ExecuteNonQuery("UPDATE Rooms SET Status = 'DaCoc' WHERE RoomId = @id",
                        new System.Data.SqlClient.SqlParameter("@id", roomId));

                    AntdUI.Message.success(form, $"Đã đặt cọc {roomName}!\nKhi khách vào ở, vào tab Hợp Đồng để xác nhận.");
                    form.Close();
                    LoadRooms();
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

        private void ShowAddRoomForm()
        {
            var form = new Form();
            form.Text = "Thêm phòng mới";
            form.Size = new Size(400, 300);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;

            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(20);
            layout.ColumnCount = 2;
            layout.RowCount = 4;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Tên phòng:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
            var txtName = new AntdUI.Input { PlaceholderText = "VD: P.101", Dock = DockStyle.Fill };
            layout.Controls.Add(txtName, 1, 0);

            layout.Controls.Add(new System.Windows.Forms.Label { Text = "Giá thuê:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
            var txtPrice = new AntdUI.InputNumber { PlaceholderText = "VNĐ/tháng", Dock = DockStyle.Fill, Value = 2000000 };
            layout.Controls.Add(txtPrice, 1, 1);

            var pnlButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 35) };
            btnCancel.Click += (s, e) => form.Close();
            
            var btnSave = new AntdUI.Button { Text = "Lưu", Type = TTypeMini.Primary, BackColor = AppColors.Blue600, Size = new Size(80, 35) };
            btnSave.Click += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    AntdUI.Message.warn(form, "Vui lòng nhập tên phòng!");
                    return;
                }
                try
                {
                    DatabaseHelper.ExecuteNonQuery("INSERT INTO Rooms (RoomName, Price, Status) VALUES (@name, @price, 'Trong')",
                        new System.Data.SqlClient.SqlParameter("@name", txtName.Text),
                        new System.Data.SqlClient.SqlParameter("@price", txtPrice.Value));
                    AntdUI.Message.success(form, "Thêm phòng thành công!");
                    form.Close();
                    LoadRooms();
                }
                catch (Exception ex)
                {
                    AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                }
            };

            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSave);
            layout.Controls.Add(pnlButtons, 1, 3);

            form.Controls.Add(layout);
            form.ShowDialog(this.FindForm());
        }
    }
}