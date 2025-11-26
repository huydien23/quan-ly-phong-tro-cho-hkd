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
                var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Rooms ORDER BY RoomName");
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

                    var card = CreateRoomCard(roomId, name, status, price);
                    flowPanel.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private AntdUI.Panel CreateRoomCard(int roomId, string name, string status, decimal price)
        {
            bool isRented = status == "DangThue";
            
            var card = new AntdUI.Panel();
            card.Size = new Size(180, 140);
            card.Radius = 12;
            card.BackColor = Color.White;
            card.Shadow = 8;
            card.Margin = new Padding(10);
            card.Cursor = Cursors.Hand;

            // Tên phòng
            var lblName = new AntdUI.Label();
            lblName.Text = name;
            lblName.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblName.ForeColor = AppColors.Blue900;
            lblName.Location = new Point(15, 15);
            lblName.Size = new Size(150, 30);

            // Giá phòng
            var lblPrice = new AntdUI.Label();
            lblPrice.Text = price.ToString("N0") + " đ/tháng";
            lblPrice.Font = new Font("Segoe UI", 9);
            lblPrice.ForeColor = Color.Gray;
            lblPrice.Location = new Point(15, 48);
            lblPrice.Size = new Size(150, 20);

            // Trạng thái
            var lblStatus = new AntdUI.Label();
            lblStatus.Text = isRented ? "● Đang thuê" : "● Trống";
            lblStatus.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblStatus.ForeColor = isRented ? AppColors.Red : AppColors.Green;
            lblStatus.Location = new Point(15, 100);
            lblStatus.Size = new Size(150, 25);

            card.Controls.Add(lblName);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblStatus);

            // Click để xem chi tiết
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
            
            // Query thông tin khách thuê nếu phòng đang thuê
            DataRow customerInfo = null;
            if (isRented)
            {
                var dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT cu.FullName, cu.Phone, cu.CCCD, cu.Address, cu.Email,
                           c.StartDate, c.Deposit, c.MonthlyRent
                    FROM Contracts c
                    INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                    WHERE c.RoomId = @roomId AND c.IsActive = 1",
                    new System.Data.SqlClient.SqlParameter("@roomId", roomId));
                if (dt.Rows.Count > 0) customerInfo = dt.Rows[0];
            }

            var form = new Form();
            form.Text = "Chi tiết " + name;
            form.Size = new Size(500, isRented && customerInfo != null ? 550 : 300);
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
            AddInfoRow(mainPanel, "Trạng thái:", isRented ? "Đang có khách thuê" : "Phòng trống", ref y, boldFont, normalFont, isRented ? AppColors.Red : AppColors.Green);

            // === THÔNG TIN NGƯỜI THUÊ ===
            if (isRented && customerInfo != null)
            {
                y += 15;
                var separator = new System.Windows.Forms.Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, y), Size = new Size(440, 2) };
                mainPanel.Controls.Add(separator); y += 15;

                var lblCustomerHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN NGƯỜI THUÊ", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Green, Location = new Point(10, y), Size = new Size(450, 25) };
                mainPanel.Controls.Add(lblCustomerHeader); y += 30;

                AddInfoRow(mainPanel, "Họ và tên:", customerInfo["FullName"]?.ToString() ?? "", ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Số điện thoại:", customerInfo["Phone"]?.ToString() ?? "", ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Số CCCD:", customerInfo["CCCD"]?.ToString() ?? "", ref y, boldFont, normalFont, AppColors.Blue600);
                AddInfoRow(mainPanel, "Email:", customerInfo["Email"]?.ToString() ?? "Chưa có", ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Địa chỉ:", customerInfo["Address"]?.ToString() ?? "", ref y, boldFont, normalFont);

                y += 15;
                var separator2 = new System.Windows.Forms.Label { BorderStyle = BorderStyle.Fixed3D, Location = new Point(10, y), Size = new Size(440, 2) };
                mainPanel.Controls.Add(separator2); y += 15;

                var lblContractHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN HỢP ĐỒNG", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Orange, Location = new Point(10, y), Size = new Size(450, 25) };
                mainPanel.Controls.Add(lblContractHeader); y += 30;

                DateTime startDate = customerInfo["StartDate"] != DBNull.Value ? Convert.ToDateTime(customerInfo["StartDate"]) : DateTime.MinValue;
                decimal deposit = customerInfo["Deposit"] != DBNull.Value ? Convert.ToDecimal(customerInfo["Deposit"]) : 0;
                decimal rent = customerInfo["MonthlyRent"] != DBNull.Value ? Convert.ToDecimal(customerInfo["MonthlyRent"]) : 0;

                AddInfoRow(mainPanel, "Ngày vào ở:", startDate.ToString("dd/MM/yyyy"), ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Tiền cọc:", deposit.ToString("N0") + " VNĐ", ref y, boldFont, normalFont);
                AddInfoRow(mainPanel, "Tiền thuê/tháng:", rent.ToString("N0") + " VNĐ", ref y, boldFont, normalFont);
            }

            // === BUTTONS ===
            y += 20;
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(440, 45), FlowDirection = FlowDirection.LeftToRight };
            
            // NẾU PHÒNG TRỐNG - Hiện nút CHO THUÊ
            if (!isRented)
            {
                var btnRent = new AntdUI.Button { Text = "Cho thuê phòng này", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(160, 35) };
                btnRent.Click += (s, e) => {
                    form.Close();
                    ShowRentRoomForm(roomId, name, price);
                };
                pnlButtons.Controls.Add(btnRent);
            }

            // Nút Sửa phòng
            var btnEdit = new AntdUI.Button { Text = "Sửa", Size = new Size(60, 35) };
            btnEdit.Click += (s, e) => {
                form.Close();
                ShowEditRoomForm(roomId);
            };
            pnlButtons.Controls.Add(btnEdit);

            if (!isRented)
            {
                var btnDelete = new AntdUI.Button { Text = "Xóa", ForeColor = AppColors.Red, Size = new Size(60, 35) };
                btnDelete.Click += (s, e) => {
                    if (VNDialog.Confirm($"Bạn có chắc muốn xóa {name}?"))
                    {
                        try
                        {
                            DatabaseHelper.ExecuteNonQuery("DELETE FROM Rooms WHERE RoomId = @id",
                                new System.Data.SqlClient.SqlParameter("@id", roomId));
                            AntdUI.Message.success(form, "Đã xóa phòng!");
                            form.Close();
                            LoadRooms();
                        }
                        catch (Exception ex)
                        {
                            AntdUI.Message.error(form, "Lỗi: " + ex.Message);
                        }
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

        private void ShowRentRoomForm(int roomId, string roomName, decimal roomPrice)
        {
            var form = new Form();
            form.Text = $"Cho thuê {roomName}";
            form.Size = new Size(550, 580);
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.BackColor = Color.White;

            var mainPanel = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };
            int y = 10;

            // === THÔNG TIN PHÒNG (Read-only) ===
            var lblRoomHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN PHÒNG", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(500, 25) };
            mainPanel.Controls.Add(lblRoomHeader); y += 28;

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = $"Phòng: {roomName}  |  Giá: {roomPrice:N0} VNĐ/tháng", Font = new Font("Segoe UI", 10), ForeColor = AppColors.Green, Location = new Point(10, y), Size = new Size(500, 22) });
            y += 35;

            // === THÔNG TIN KHÁCH THUÊ ===
            var lblCustomerHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN KHÁCH THUÊ", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(500, 25) };
            mainPanel.Controls.Add(lblCustomerHeader); y += 30;

            int lblW = 110, inputW = 180, col2X = 300;

            // Row 1: Họ tên + SĐT
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Họ tên *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtName = new AntdUI.Input { Location = new Point(10 + lblW, y), Size = new Size(inputW, 35), PlaceholderText = "Nguyễn Văn A" };
            mainPanel.Controls.Add(txtName);

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "SĐT *:", Location = new Point(col2X, y + 5), Size = new Size(60, 22) });
            var txtPhone = new AntdUI.Input { Location = new Point(col2X + 60, y), Size = new Size(150, 35), PlaceholderText = "0912345678" };
            mainPanel.Controls.Add(txtPhone);
            y += 42;

            // Row 2: CCCD + Email
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "CCCD *:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtCCCD = new AntdUI.Input { Location = new Point(10 + lblW, y), Size = new Size(inputW, 35), PlaceholderText = "12 số" };
            mainPanel.Controls.Add(txtCCCD);

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Email:", Location = new Point(col2X, y + 5), Size = new Size(60, 22) });
            var txtEmail = new AntdUI.Input { Location = new Point(col2X + 60, y), Size = new Size(150, 35), PlaceholderText = "email@gmail.com" };
            mainPanel.Controls.Add(txtEmail);
            y += 42;

            // Row 3: Địa chỉ
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Địa chỉ:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var txtAddress = new AntdUI.Input { Location = new Point(10 + lblW, y), Size = new Size(380, 35), PlaceholderText = "Địa chỉ thường trú" };
            mainPanel.Controls.Add(txtAddress);
            y += 50;

            // === THÔNG TIN HỢP ĐỒNG ===
            var lblContractHeader = new System.Windows.Forms.Label { Text = "THÔNG TIN HỢP ĐỒNG", Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = AppColors.Blue900, Location = new Point(10, y), Size = new Size(500, 25) };
            mainPanel.Controls.Add(lblContractHeader); y += 30;

            // Row 4: Ngày vào + Tiền thuê
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Ngày vào:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var dateStart = new AntdUI.DatePicker { Location = new Point(10 + lblW, y), Size = new Size(inputW, 35), Value = DateTime.Now };
            mainPanel.Controls.Add(dateStart);

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền thuê:", Location = new Point(col2X, y + 5), Size = new Size(70, 22) });
            var numRent = new AntdUI.InputNumber { Location = new Point(col2X + 70, y), Size = new Size(140, 35), Value = roomPrice };
            mainPanel.Controls.Add(numRent);
            y += 42;

            // Row 5: Tiền cọc + Ghi chú
            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Tiền cọc:", Location = new Point(10, y + 5), Size = new Size(lblW, 22) });
            var numDeposit = new AntdUI.InputNumber { Location = new Point(10 + lblW, y), Size = new Size(inputW, 35), Value = roomPrice };
            mainPanel.Controls.Add(numDeposit);

            mainPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Ghi chú:", Location = new Point(col2X, y + 5), Size = new Size(70, 22) });
            var txtNote = new AntdUI.Input { Location = new Point(col2X + 70, y), Size = new Size(140, 35) };
            mainPanel.Controls.Add(txtNote);
            y += 55;

            // === BUTTONS ===
            var pnlButtons = new FlowLayoutPanel { Location = new Point(10, y), Size = new Size(500, 50), FlowDirection = FlowDirection.RightToLeft };

            var btnCancel = new AntdUI.Button { Text = "Hủy", Size = new Size(80, 40) };
            btnCancel.Click += (s, e) => form.Close();

            var btnSave = new AntdUI.Button { Text = "Tạo hợp đồng", Type = TTypeMini.Primary, BackColor = AppColors.Green, Size = new Size(130, 40) };
            btnSave.Click += (s, e) => {
                // Validation
                if (string.IsNullOrWhiteSpace(txtName.Text)) { AntdUI.Message.warn(form, "Nhập họ tên khách!"); return; }
                if (string.IsNullOrWhiteSpace(txtPhone.Text) || txtPhone.Text.Length != 10) { AntdUI.Message.warn(form, "SĐT phải 10 số!"); return; }
                if (string.IsNullOrWhiteSpace(txtCCCD.Text) || txtCCCD.Text.Length != 12) { AntdUI.Message.warn(form, "CCCD phải 12 số!"); return; }

                try
                {
                    // 1. Tạo khách hàng mới
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Customers (FullName, Phone, CCCD, Email, Address) VALUES (@n, @p, @c, @e, @a)",
                        new System.Data.SqlClient.SqlParameter("@n", txtName.Text),
                        new System.Data.SqlClient.SqlParameter("@p", txtPhone.Text),
                        new System.Data.SqlClient.SqlParameter("@c", txtCCCD.Text),
                        new System.Data.SqlClient.SqlParameter("@e", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text),
                        new System.Data.SqlClient.SqlParameter("@a", string.IsNullOrWhiteSpace(txtAddress.Text) ? DBNull.Value : (object)txtAddress.Text));

                    // 2. Lấy CustomerId vừa tạo
                    var customerIdObj = DatabaseHelper.ExecuteScalar("SELECT MAX(CustomerId) FROM Customers");
                    int customerId = Convert.ToInt32(customerIdObj);

                    // 3. Tạo hợp đồng
                    DatabaseHelper.ExecuteNonQuery(
                        "INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, MonthlyRent, IsActive) VALUES (@rid, @cid, @sd, @dep, @rent, 1)",
                        new System.Data.SqlClient.SqlParameter("@rid", roomId),
                        new System.Data.SqlClient.SqlParameter("@cid", customerId),
                        new System.Data.SqlClient.SqlParameter("@sd", dateStart.Value ?? DateTime.Now),
                        new System.Data.SqlClient.SqlParameter("@dep", numDeposit.Value),
                        new System.Data.SqlClient.SqlParameter("@rent", numRent.Value));

                    // 4. Cập nhật trạng thái phòng
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