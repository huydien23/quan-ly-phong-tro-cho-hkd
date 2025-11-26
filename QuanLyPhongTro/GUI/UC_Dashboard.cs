using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class UC_Dashboard : UserControl
    {
        private System.Windows.Forms.Label lblTotalRooms, lblOccupied, lblEmpty, lblRevenue, lblUnpaid;
        private AntdUI.Progress progressTax;
        private System.Windows.Forms.Label lblTaxAmount, lblTaxStatus;
        private FlowLayoutPanel pnlAlerts;

        public UC_Dashboard()
        {
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.DoubleBuffered = true;
            InitUI();
            LoadStats();
            LoadAlerts();
        }

        private void InitUI()
        {
            // Main scroll panel
            var mainPanel = new FlowLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.FlowDirection = FlowDirection.TopDown;
            mainPanel.WrapContents = false;
            mainPanel.AutoScroll = true;
            mainPanel.Padding = new Padding(25);
            mainPanel.BackColor = Color.Transparent;

            // === HEADER: Welcome + Quick Actions ===
            var pnlHeader = CreateModernCard(1080, 80);
            pnlHeader.Margin = new Padding(0, 0, 0, 20);

            string greeting = DateTime.Now.Hour < 12 ? "Chào buổi sáng" : (DateTime.Now.Hour < 18 ? "Chào buổi chiều" : "Chào buổi tối");
            var lblGreeting = new System.Windows.Forms.Label { 
                Text = greeting + ",", 
                Font = new Font("Segoe UI", 11, FontStyle.Regular), 
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(25, 15), 
                AutoSize = true,
                BackColor = Color.White
            };
            var lblUsername = new System.Windows.Forms.Label { 
                Text = CurrentUser.FullName ?? "Admin", 
                Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(25, 38), 
                AutoSize = true,
                BackColor = Color.White
            };
            pnlHeader.Controls.Add(lblGreeting);
            pnlHeader.Controls.Add(lblUsername);

            // Quick Action Buttons - Modern Style
            var btnNewContract = CreateActionButton("+ Hợp đồng mới", Color.FromArgb(34, 197, 94), 680, 20);
            btnNewContract.Click += (s, e) => NavigateTo(new UC_Contracts());

            var btnNewInvoice = CreateActionButton("📄 Tạo hóa đơn", Color.FromArgb(59, 130, 246), 830, 20);
            btnNewInvoice.Click += (s, e) => NavigateTo(new UC_Services());

            var btnRefresh = CreateActionButton("⟳", Color.FromArgb(100, 116, 139), 980, 20, 40);
            btnRefresh.Click += (s, e) => { 
                btnRefresh.Enabled = false;
                btnRefresh.Text = "...";
                LoadStats(); 
                LoadAlerts();
                btnRefresh.Text = "✓";
                var timer = new Timer { Interval = 1000 };
                timer.Tick += (ts, te) => { btnRefresh.Text = "⟳"; btnRefresh.Enabled = true; timer.Stop(); timer.Dispose(); };
                timer.Start();
            };

            pnlHeader.Controls.Add(btnNewContract);
            pnlHeader.Controls.Add(btnNewInvoice);
            pnlHeader.Controls.Add(btnRefresh);
            mainPanel.Controls.Add(pnlHeader);

            // === STAT CARDS - Modern Gradient Style ===
            var pnlStats = new FlowLayoutPanel();
            pnlStats.Size = new Size(1080, 130);
            pnlStats.FlowDirection = FlowDirection.LeftToRight;
            pnlStats.Margin = new Padding(0, 0, 0, 20);
            pnlStats.BackColor = Color.Transparent;

            pnlStats.Controls.Add(CreateModernStatCard("🏠", "Tổng phòng", "0", Color.FromArgb(59, 130, 246), Color.FromArgb(37, 99, 235), out lblTotalRooms));
            pnlStats.Controls.Add(CreateModernStatCard("✓", "Đang thuê", "0", Color.FromArgb(34, 197, 94), Color.FromArgb(22, 163, 74), out lblOccupied));
            pnlStats.Controls.Add(CreateModernStatCard("○", "Phòng trống", "0", Color.FromArgb(251, 146, 60), Color.FromArgb(249, 115, 22), out lblEmpty));
            pnlStats.Controls.Add(CreateModernStatCard("💰", "Thu tháng này", "0đ", Color.FromArgb(168, 85, 247), Color.FromArgb(147, 51, 234), out lblRevenue));
            pnlStats.Controls.Add(CreateModernStatCard("⏳", "Chưa thu", "0đ", Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38), out lblUnpaid));

            mainPanel.Controls.Add(pnlStats);

            // === ALERTS SECTION - Modern Style ===
            var pnlAlertContainer = CreateModernCard(1080, 200);
            pnlAlertContainer.Margin = new Padding(0, 0, 0, 20);

            var lblAlertTitle = new System.Windows.Forms.Label { 
                Text = "⚠ Cảnh báo cần xử lý", 
                Font = new Font("Segoe UI", 13, FontStyle.Bold), 
                ForeColor = Color.FromArgb(234, 88, 12),
                Location = new Point(20, 15), 
                AutoSize = true,
                BackColor = Color.White
            };
            pnlAlertContainer.Controls.Add(lblAlertTitle);

            // Divider line
            var divider = new System.Windows.Forms.Panel();
            divider.Size = new Size(1040, 1);
            divider.Location = new Point(20, 55);
            divider.BackColor = Color.FromArgb(226, 232, 240);
            pnlAlertContainer.Controls.Add(divider);

            pnlAlerts = new FlowLayoutPanel { 
                Location = new Point(20, 65), 
                Size = new Size(1040, 125), 
                FlowDirection = FlowDirection.TopDown, 
                AutoScroll = true, 
                WrapContents = false,
                BackColor = Color.White
            };
            pnlAlertContainer.Controls.Add(pnlAlerts);
            mainPanel.Controls.Add(pnlAlertContainer);

            // === TAX PROGRESS SECTION ===
            var pnlTax = CreateModernCard(1080, 140);
            pnlTax.Margin = new Padding(0, 0, 0, 20);

            var lblTaxIcon = new System.Windows.Forms.Label { 
                Text = "📊", 
                Font = new Font("Segoe UI", 16), 
                Location = new Point(20, 15), 
                AutoSize = true,
                BackColor = Color.White
            };
            var lblTaxTitle = new System.Windows.Forms.Label { 
                Text = $"Tiến độ doanh thu năm {DateTime.Now.Year}", 
                Font = new Font("Segoe UI", 13, FontStyle.Bold), 
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(55, 18), 
                AutoSize = true,
                BackColor = Color.White
            };
            pnlTax.Controls.Add(lblTaxIcon);
            pnlTax.Controls.Add(lblTaxTitle);

            progressTax = new AntdUI.Progress { 
                Value = 0, 
                Location = new Point(20, 55), 
                Size = new Size(1040, 28), 
                Fill = Color.FromArgb(34, 197, 94),
                Radius = 14
            };
            pnlTax.Controls.Add(progressTax);

            lblTaxAmount = new System.Windows.Forms.Label { 
                Text = "0 / 100,000,000 VNĐ (0%)", 
                Font = new Font("Segoe UI", 10), 
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(20, 95), 
                AutoSize = true,
                BackColor = Color.White
            };
            lblTaxStatus = new System.Windows.Forms.Label { 
                Text = "✓ An toàn - Chưa phải đóng thuế", 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                ForeColor = Color.FromArgb(34, 197, 94),
                Location = new Point(20, 115), 
                AutoSize = true,
                BackColor = Color.White
            };
            pnlTax.Controls.Add(lblTaxAmount);
            pnlTax.Controls.Add(lblTaxStatus);
            mainPanel.Controls.Add(pnlTax);

            this.Controls.Add(mainPanel);
        }

        private void NavigateTo(UserControl uc)
        {
            var parent = this.Parent as AntdUI.Panel;
            if (parent != null)
            {
                parent.Controls.Clear();
                uc.Dock = DockStyle.Fill;
                parent.Controls.Add(uc);
            }
        }

        private System.Windows.Forms.Panel CreateModernCard(int width, int height)
        {
            var card = new System.Windows.Forms.Panel();
            card.Size = new Size(width, height);
            card.BackColor = Color.White;
            card.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRectangle(0, 0, width - 1, height - 1, 12))
                {
                    e.Graphics.FillPath(Brushes.White, path);
                    using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1))
                        e.Graphics.DrawPath(pen, path);
                }
            };
            return card;
        }

        private System.Windows.Forms.Panel CreateModernStatCard(string icon, string title, string value, Color color1, Color color2, out System.Windows.Forms.Label lblValue)
        {
            var card = new System.Windows.Forms.Panel();
            card.Size = new Size(200, 120);
            card.Margin = new Padding(0, 0, 15, 0);

            // Store colors for paint event
            card.Tag = new Color[] { color1, color2 };
            
            card.Paint += (s, e) => {
                var colors = (Color[])card.Tag;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRectangle(0, 0, card.Width - 1, card.Height - 1, 16))
                using (var brush = new LinearGradientBrush(card.ClientRectangle, colors[0], colors[1], 45f))
                {
                    e.Graphics.FillPath(brush, path);
                }
            };

            var lblIcon = new System.Windows.Forms.Label { 
                Text = icon, 
                Font = new Font("Segoe UI", 22), 
                ForeColor = Color.White,
                Location = new Point(18, 15), 
                AutoSize = true,
                BackColor = Color.Transparent
            };

            var lblTitle = new System.Windows.Forms.Label { 
                Text = title.ToUpper(), 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                ForeColor = Color.FromArgb(200, 255, 255, 255),
                Location = new Point(18, 55), 
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblValue = new System.Windows.Forms.Label { 
                Text = value, 
                Font = new Font("Segoe UI", 22, FontStyle.Bold), 
                ForeColor = Color.White,
                Location = new Point(18, 75), 
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);

            return card;
        }

        private System.Windows.Forms.Button CreateActionButton(string text, Color bgColor, int x, int y, int width = 140)
        {
            var btn = new System.Windows.Forms.Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, 38);
            btn.BackColor = bgColor;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void LoadAlerts()
        {
            pnlAlerts.Controls.Clear();
            try
            {
                // 1. Hóa đơn quá hạn (chưa thu từ tháng trước)
                int prevMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
                int prevYear = DateTime.Now.Month == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
                var overdueInvoices = DatabaseHelper.ExecuteQuery($@"
                    SELECT i.InvoiceId, r.RoomName, cu.FullName, i.TotalAmount 
                    FROM Invoices i 
                    INNER JOIN Contracts c ON i.ContractId = c.ContractId
                    INNER JOIN Rooms r ON c.RoomId = r.RoomId
                    INNER JOIN Customers cu ON c.CustomerId = cu.CustomerId
                    WHERE i.Status = 'ChuaThu' AND (i.Year < {DateTime.Now.Year} OR (i.Year = {DateTime.Now.Year} AND i.Month < {DateTime.Now.Month}))");

                foreach (DataRow row in overdueInvoices.Rows)
                {
                    AddAlert($"🔴 HÓA ĐƠN QUÁ HẠN: {row["RoomName"]} - {row["FullName"]} - {Convert.ToDecimal(row["TotalAmount"]):N0}đ", AppColors.Red);
                }

                // 2. Tạm trú sắp hết hạn (30 ngày)
                var expiringResidence = DatabaseHelper.ExecuteQuery(@"
                    SELECT cu.FullName, r.RoomName, tr.ExpiryDate 
                    FROM TemporaryResidence tr
                    INNER JOIN Customers cu ON tr.CustomerId = cu.CustomerId
                    INNER JOIN Rooms r ON tr.RoomId = r.RoomId
                    WHERE tr.ExpiryDate <= DATEADD(DAY, 30, GETDATE()) AND tr.Status = 'DaDangKy'");

                foreach (DataRow row in expiringResidence.Rows)
                {
                    DateTime exp = Convert.ToDateTime(row["ExpiryDate"]);
                    int days = (exp - DateTime.Now).Days;
                    string msg = days < 0 ? $"🔴 TẠM TRÚ HẾT HẠN: {row["RoomName"]} - {row["FullName"]}" 
                                          : $"🟡 Tạm trú sắp hết ({days} ngày): {row["RoomName"]} - {row["FullName"]}";
                    AddAlert(msg, days < 0 ? AppColors.Red : Color.Orange);
                }

                // 3. Phòng trống lâu (>30 ngày)
                var emptyRooms = DatabaseHelper.ExecuteQuery(@"
                    SELECT RoomName FROM Rooms 
                    WHERE Status = 'Trong' AND RoomId NOT IN (
                        SELECT RoomId FROM Contracts WHERE IsActive = 1
                    )");
                if (emptyRooms.Rows.Count > 2)
                {
                    AddAlert($"🟡 Có {emptyRooms.Rows.Count} phòng trống - Cân nhắc giảm giá hoặc quảng cáo", Color.Orange);
                }

                if (pnlAlerts.Controls.Count == 0)
                {
                    AddAlert("✅ Không có cảnh báo nào. Mọi thứ đang ổn!", AppColors.Green);
                }
            }
            catch { }
        }

        private void AddAlert(string text, Color color)
        {
            var lbl = new System.Windows.Forms.Label { Text = text, ForeColor = color, Font = new Font("Segoe UI", 10), AutoSize = false, Size = new Size(980, 24), Padding = new Padding(5, 3, 0, 3) };
            pnlAlerts.Controls.Add(lbl);
        }

        private AntdUI.Panel CreateStatCard(string title, string value, Color color, out AntdUI.Label lblValue)
        {
            var card = new AntdUI.Panel();
            card.Size = new Size(170, 100);
            card.BackColor = Color.White;
            card.Radius = 10;
            card.Shadow = 5;
            card.Margin = new Padding(0, 0, 10, 0);

            var lblTitle = new AntdUI.Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 9);
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Location = new Point(15, 15);
            lblTitle.Size = new Size(140, 20);

            lblValue = new AntdUI.Label();
            lblValue.Text = value;
            lblValue.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblValue.ForeColor = color;
            lblValue.Location = new Point(15, 45);
            lblValue.Size = new Size(140, 40);

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
        }

        private void LoadStats()
        {
            try
            {
                // Thống kê phòng
                var totalRooms = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Rooms");
                var occupied = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Rooms WHERE Status = 'DangThue'");
                var empty = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Rooms WHERE Status = 'Trong'");

                lblTotalRooms.Text = totalRooms?.ToString() ?? "0";
                lblOccupied.Text = occupied?.ToString() ?? "0";
                lblEmpty.Text = empty?.ToString() ?? "0";

                // Doanh thu tháng này
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                var revenue = DatabaseHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Month = @m AND Year = @y AND Status = 'DaThu'",
                    new System.Data.SqlClient.SqlParameter("@m", month),
                    new System.Data.SqlClient.SqlParameter("@y", year));
                var unpaid = DatabaseHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Month = @m AND Year = @y AND Status = 'ChuaThu'",
                    new System.Data.SqlClient.SqlParameter("@m", month),
                    new System.Data.SqlClient.SqlParameter("@y", year));

                decimal revenueVal = revenue != null ? Convert.ToDecimal(revenue) : 0;
                decimal unpaidVal = unpaid != null ? Convert.ToDecimal(unpaid) : 0;

                lblRevenue.Text = revenueVal.ToString("N0") + " đ";
                lblUnpaid.Text = unpaidVal.ToString("N0") + " đ";

                // Tính thuế năm
                var yearRevenue = DatabaseHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Year = @y AND Status = 'DaThu'",
                    new System.Data.SqlClient.SqlParameter("@y", year));
                var taxThreshold = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'NguongMienThue'");

                decimal yearRev = yearRevenue != null ? Convert.ToDecimal(yearRevenue) : 0;
                decimal threshold = taxThreshold != null ? Convert.ToDecimal(taxThreshold) : 100000000;

                float percent = threshold > 0 ? (float)(yearRev / threshold) : 0;
                if (percent > 1) percent = 1;

                progressTax.Value = percent;
                progressTax.Fill = percent >= 1 ? AppColors.Red : AppColors.Green;

                lblTaxAmount.Text = $"{yearRev:N0} / {threshold:N0} VNĐ ({percent * 100:F0}%)";

                if (yearRev >= threshold)
                {
                    lblTaxStatus.Text = "PHẢI ĐÓNG THUẾ! Liên hệ kế toán.";
                    lblTaxStatus.ForeColor = AppColors.Red;
                }
                else
                {
                    lblTaxStatus.Text = "Trạng thái: Chưa phải đóng thuế (An toàn)";
                    lblTaxStatus.ForeColor = AppColors.Green;
                }
            }
            catch (Exception ex)
            {
                // Silent fail for dashboard
                System.Diagnostics.Debug.WriteLine("Dashboard error: " + ex.Message);
            }
        }
    }
}