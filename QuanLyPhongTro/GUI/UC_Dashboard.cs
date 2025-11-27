using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI; // Chỉ dùng Panel và Progress
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
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.DoubleBuffered = true;
            this.Dock = DockStyle.Fill;

            InitUI();
            LoadStats();
            LoadAlerts();
        }

        private void InitUI()
        {
            // Container
            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };

            // ================= HEADER =================
            var pnlHeader = new AntdUI.Panel
            {
                Size = new Size(1080, 100), // Tăng chiều cao
                Back = Color.White,
                Radius = 12,
                Shadow = 6, // Giảm shadow cho nhẹ nhàng
                Margin = new Padding(0, 0, 0, 25)
            };

            // Greeting Text - Căn giữa theo chiều dọc tốt hơn
            string greeting = DateTime.Now.Hour < 12 ? "Chào buổi sáng" : (DateTime.Now.Hour < 18 ? "Chào buổi chiều" : "Chào buổi tối");
            var lblGreeting = new System.Windows.Forms.Label
            {
                Text = $"{greeting},",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(25, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var lblName = new System.Windows.Forms.Label
            {
                Text = CurrentUser.FullName ?? "Admin",
                Font = new Font("Segoe UI", 20, FontStyle.Bold), // To hơn xíu
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(22, 45),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblGreeting);
            pnlHeader.Controls.Add(lblName);

            // Buttons 
            int btnY = 30; // Căn giữa header
            pnlHeader.Controls.Add(CreateFlatButton("Hợp đồng mới", Color.FromArgb(16, 185, 129), 660, btnY, (s, e) => NavigateTo(new UC_Contracts())));
            pnlHeader.Controls.Add(CreateFlatButton("Tạo hóa đơn", Color.FromArgb(59, 130, 246), 820, btnY, (s, e) => NavigateTo(new UC_Services())));
            
            // Refresh Btn - Styled better
            var btnRefresh = new System.Windows.Forms.Button
            {
                Text = "⟳",
                Font = new Font("Segoe UI", 16),
                Size = new Size(45, 45),
                Location = new Point(980, btnY),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 245, 249), // Nền xám nhạt
                ForeColor = Color.FromArgb(100, 116, 139),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Paint += (s, e) => { using (GraphicsPath path = GetRoundedPath(btnRefresh.ClientRectangle, 8)) btnRefresh.Region = new Region(path); }; // Bo góc nhẹ
            btnRefresh.Click += (s, e) => { LoadStats(); LoadAlerts(); };
            pnlHeader.Controls.Add(btnRefresh);

            mainPanel.Controls.Add(pnlHeader);

            // ================= STATS CARDS (Pastel No Purple) =================
            var pnlStats = new FlowLayoutPanel
            {
                Size = new Size(1080, 140),
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0, 0, 0, 15),
                BackColor = Color.Transparent
            };

            // Blue
            pnlStats.Controls.Add(CreatePastelCard("TỔNG PHÒNG", "0", Color.FromArgb(239, 246, 255), Color.FromArgb(59, 130, 246), out lblTotalRooms));
            // Green
            pnlStats.Controls.Add(CreatePastelCard("ĐANG THUÊ", "0", Color.FromArgb(240, 253, 244), Color.FromArgb(34, 197, 94), out lblOccupied));
            // Orange
            pnlStats.Controls.Add(CreatePastelCard("PHÒNG TRỐNG", "0", Color.FromArgb(255, 247, 237), Color.FromArgb(249, 115, 22), out lblEmpty));
            // Teal (Thay cho Tím) - Doanh thu
            pnlStats.Controls.Add(CreatePastelCard("DOANH THU", "0", Color.FromArgb(204, 251, 241), Color.FromArgb(20, 184, 166), out lblRevenue));
            // Red
            pnlStats.Controls.Add(CreatePastelCard("CHƯA THU", "0", Color.FromArgb(254, 242, 242), Color.FromArgb(239, 68, 68), out lblUnpaid));

            mainPanel.Controls.Add(pnlStats);

            // ================= SPLIT =================
            var splitContainer = new FlowLayoutPanel
            {
                Size = new Size(1080, 400),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            // LEFT
            var pnlLeft = new AntdUI.Panel
            {
                Size = new Size(650, 300),
                Back = Color.White,
                Radius = 12,
                Shadow = 10,
                Margin = new Padding(0, 0, 20, 0)
            };
             var lblTaxHeader = new System.Windows.Forms.Label { Text = $"Tiến độ doanh thu {DateTime.Now.Year}", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Black, Location = new Point(20, 20), AutoSize = true };
            pnlLeft.Controls.Add(lblTaxHeader);

            progressTax = new AntdUI.Progress
            {
                Location = new Point(20, 60),
                Size = new Size(610, 30),
                Value = 0,
                Radius = 12,
                Fill = Color.FromArgb(34, 197, 94)
            };
            pnlLeft.Controls.Add(progressTax);

            lblTaxAmount = new System.Windows.Forms.Label { Text = "0 / 100tr", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(20, 100), AutoSize = true };
            lblTaxStatus = new System.Windows.Forms.Label { Text = "An toàn", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Green, Location = new Point(20, 125), AutoSize = true };
            
            pnlLeft.Controls.Add(lblTaxAmount);
            pnlLeft.Controls.Add(lblTaxStatus);
            
            var msg = new System.Windows.Forms.Label { Text = "Mẹo: Hãy theo dõi sát doanh thu để tránh bị phạt thuế.", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(20, 160), AutoSize = true };
            pnlLeft.Controls.Add(msg);

            splitContainer.Controls.Add(pnlLeft);

            // RIGHT
            var pnlRight = new AntdUI.Panel
            {
                Size = new Size(410, 300),
                Back = Color.White,
                Radius = 12,
                Shadow = 10
            };
            var lblAlertHeader = new System.Windows.Forms.Label { Text = "Cảnh báo cần xử lý", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), Location = new Point(20, 20), AutoSize = true };
            pnlRight.Controls.Add(lblAlertHeader);

            var div = new System.Windows.Forms.Panel { Size = new Size(370, 1), BackColor = Color.FromArgb(240, 240, 240), Location = new Point(20, 55) };
            pnlRight.Controls.Add(div);

            pnlAlerts = new FlowLayoutPanel
            {
                Location = new Point(10, 60),
                Size = new Size(390, 230),
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                WrapContents = false,
                BackColor = Color.White
            };
            pnlRight.Controls.Add(pnlAlerts);

            splitContainer.Controls.Add(pnlRight);
            mainPanel.Controls.Add(splitContainer);

            this.Controls.Add(mainPanel);
        }

        private System.Windows.Forms.Button CreateFlatButton(string text, Color bg, int x, int y, EventHandler click)
        {
            var btn = new System.Windows.Forms.Button();
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Location = new Point(x, y);
            btn.Size = new Size(150, 40);
            btn.Cursor = Cursors.Hand;
            btn.Click += click;
            
            // Paint for Radius
            btn.Paint += (s, e) => {
                 using (GraphicsPath path = GetRoundedPath(btn.ClientRectangle, 10))
                 {
                     btn.Region = new Region(path);
                 }
            };
            
            return btn;
        }

        private AntdUI.Panel CreatePastelCard(string title, string value, Color bgColor, Color accentColor, out System.Windows.Forms.Label lblVal)
        {
            var card = new AntdUI.Panel();
            card.Size = new Size(200, 120);
            card.Margin = new Padding(0, 0, 15, 0);
            card.Back = bgColor;
            card.Radius = 12; // Bo góc mềm mại
            card.Shadow = 6;  // Đổ bóng nhẹ
            
            // Dot Accent
            var pnlDot = new AntdUI.Panel 
            { 
                Size = new Size(8, 8), 
                Location = new Point(20, 26), 
                Back = accentColor,
                Radius = 4 // Bo tròn thành chấm
            };
            
            var lblTitle = new System.Windows.Forms.Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(35, 22),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblVal = new System.Windows.Forms.Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(15, 55),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            card.Controls.Add(pnlDot);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblVal);
            
            return card;
        }

        private void AddAlertItem(string title, string sub, bool urgent)
        {
            var item = new System.Windows.Forms.Panel
            {
                Size = new Size(360, 60),
                Margin = new Padding(5, 0, 0, 10),
                BackColor = Color.FromArgb(249, 250, 251),
                Cursor = Cursors.Hand
            };
            
            var stripe = new System.Windows.Forms.Panel { Dock = DockStyle.Left, Width = 4, BackColor = urgent ? Color.Red : Color.Orange };
            item.Controls.Add(stripe);

            var lblT = new System.Windows.Forms.Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 81), Location = new Point(15, 10), AutoSize = true };
            var lblS = new System.Windows.Forms.Label { Text = sub, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(15, 30), AutoSize = true };
            
            item.Controls.Add(lblT);
            item.Controls.Add(lblS);

            pnlAlerts.Controls.Add(item);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2.0F;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void NavigateTo(UserControl uc)
        {
            var parent = this.Parent;
            while (parent != null && !(parent is AntdUI.Panel) && !(parent is Form)) parent = parent.Parent;
            if (parent != null)
            {
                parent.Controls.Clear();
                uc.Dock = DockStyle.Fill;
                parent.Controls.Add(uc);
            }
        }

        private void LoadStats()
        {
            try
            {
                var totalRooms = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Rooms");
                var occupied = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Rooms WHERE Status = 'DangThue'");
                var empty = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Rooms WHERE Status = 'Trong'");

                lblTotalRooms.Text = totalRooms?.ToString() ?? "0";
                lblOccupied.Text = occupied?.ToString() ?? "0";
                lblEmpty.Text = empty?.ToString() ?? "0";

                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                var revenue = DatabaseHelper.ExecuteScalar("SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Month = @m AND Year = @y AND Status = 'DaThu'", new System.Data.SqlClient.SqlParameter("@m", month), new System.Data.SqlClient.SqlParameter("@y", year));
                var unpaid = DatabaseHelper.ExecuteScalar("SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Month = @m AND Year = @y AND Status = 'ChuaThu'", new System.Data.SqlClient.SqlParameter("@m", month), new System.Data.SqlClient.SqlParameter("@y", year));

                lblRevenue.Text = Convert.ToDecimal(revenue ?? 0).ToString("N0");
                lblUnpaid.Text = Convert.ToDecimal(unpaid ?? 0).ToString("N0");

                var yearRevObj = DatabaseHelper.ExecuteScalar("SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Year = @y AND Status = 'DaThu'", new System.Data.SqlClient.SqlParameter("@y", year));
                var taxObj = DatabaseHelper.ExecuteScalar("SELECT SettingValue FROM Settings WHERE SettingKey = 'NguongMienThue'");

                decimal yearRev = Convert.ToDecimal(yearRevObj ?? 0);
                decimal threshold = taxObj != null ? Convert.ToDecimal(taxObj) : 100000000;
                
                float percent = threshold > 0 ? (float)yearRev / (float)threshold : 0;
                if (percent > 1) percent = 1;

                progressTax.Value = percent;
                progressTax.Fill = yearRev >= threshold ? Color.Red : Color.FromArgb(34, 197, 94);
                
                lblTaxAmount.Text = $"{yearRev:N0} / {threshold:N0}";
                lblTaxStatus.Text = yearRev >= threshold ? "CẢNH BÁO: VƯỢT NGƯỠNG!" : "An toàn";
                lblTaxStatus.ForeColor = yearRev >= threshold ? Color.Red : Color.Green;
            }
            catch { }
        }

        private void LoadAlerts()
        {
            pnlAlerts.Controls.Clear();
            try
            {
                var overdue = DatabaseHelper.ExecuteQuery($@"SELECT r.RoomName, cu.FullName, i.TotalAmount FROM Invoices i JOIN Contracts c ON i.ContractId=c.ContractId JOIN Rooms r ON c.RoomId=r.RoomId JOIN Customers cu ON c.CustomerId=cu.CustomerId WHERE i.Status='ChuaThu' AND (i.Year < {DateTime.Now.Year} OR (i.Year={DateTime.Now.Year} AND i.Month < {DateTime.Now.Month}))");
                foreach (DataRow r in overdue.Rows) AddAlertItem($"Nợ: {r["RoomName"]}", $"{r["FullName"]} - {Convert.ToDecimal(r["TotalAmount"]):N0}đ", true);

                var expiring = DatabaseHelper.ExecuteQuery("SELECT cu.FullName, r.RoomName, tr.ExpiryDate FROM TemporaryResidence tr JOIN Customers cu ON tr.CustomerId=cu.CustomerId JOIN Rooms r ON tr.RoomId=r.RoomId WHERE tr.ExpiryDate <= DATEADD(DAY, 30, GETDATE()) AND tr.Status='DaDangKy'");
                foreach (DataRow r in expiring.Rows)
                {
                    int days = (Convert.ToDateTime(r["ExpiryDate"]) - DateTime.Now).Days;
                    AddAlertItem(days < 0 ? $"Hết hạn: {r["RoomName"]}" : $"Sắp hết: {r["RoomName"]}", $"{r["FullName"]} ({days} ngày)", days < 0);
                }
                
                if (pnlAlerts.Controls.Count == 0)
                {
                   var lbl = new System.Windows.Forms.Label { Text = "Không có cảnh báo nào!", ForeColor = Color.Green, AutoSize = true, Padding = new Padding(10) };
                   pnlAlerts.Controls.Add(lbl);
                }
            }
            catch { }
        }
    }
}
