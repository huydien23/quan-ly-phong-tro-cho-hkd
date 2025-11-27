using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WinChart = System.Windows.Forms.DataVisualization.Charting;
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
        private WinChart.Chart revenueChart;

        private System.Windows.Forms.Button btnAlertDebt, btnAlertResidence, btnAlertOther, btnCreateInvoiceMonth, btnRemindDebt;
        private AlertCategory currentAlertCategory = AlertCategory.Debt;
        private readonly List<AlertInfo> debtAlerts = new List<AlertInfo>();
        private readonly List<AlertInfo> residenceAlerts = new List<AlertInfo>();
        private readonly List<AlertInfo> otherAlerts = new List<AlertInfo>();

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

            // Buttons (dùng FlowLayout để căn đều)
            var actionPanel = new FlowLayoutPanel
            {
                Location = new Point(520, 30),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            actionPanel.Controls.Add(CreateActionButton("Hợp đồng mới", Color.FromArgb(16, 185, 129), (s, e) => NavigateTo(new UC_Contracts())));
            actionPanel.Controls.Add(CreateActionButton("Dịch vụ / Hóa đơn", Color.FromArgb(59, 130, 246), (s, e) => NavigateTo(new UC_Services())));

            btnCreateInvoiceMonth = CreateActionButton("Tạo hóa đơn tháng này", Color.FromArgb(14, 165, 233), (s, e) => GenerateCurrentMonthInvoices());
            btnCreateInvoiceMonth.Size = new Size(150, 34);
            actionPanel.Controls.Add(btnCreateInvoiceMonth);
            pnlHeader.Controls.Add(actionPanel);

            // Refresh Btn - Styled better
            var btnRefresh = new System.Windows.Forms.Button
            {
                Text = "⟳",
                Font = new Font("Segoe UI", 16),
                Size = new Size(45, 45),
                Location = new Point(actionPanel.Right + 10, 30),
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
                Size = new Size(650, 380),
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

            // Revenue Chart
            revenueChart = new WinChart.Chart
            {
                Location = new Point(20, 200),
                Size = new Size(610, 160),
                BackColor = Color.Transparent,
                BorderlineWidth = 0
            };
            var chartArea = new WinChart.ChartArea("RevenueArea")
            {
                BackColor = Color.White,
                AxisX = { Interval = 1, LabelStyle = { Font = new Font("Segoe UI", 8), ForeColor = Color.Gray }, LineColor = Color.FromArgb(226, 232, 240) },
                AxisY = { LabelStyle = { Font = new Font("Segoe UI", 8), ForeColor = Color.Gray }, MajorGrid = { LineColor = Color.FromArgb(229, 231, 235) }, LineColor = Color.FromArgb(226, 232, 240), Minimum = 0 }
            };
            revenueChart.ChartAreas.Add(chartArea);
            revenueChart.Series.Add(new WinChart.Series("RevenueSeries")
            {
                ChartType = WinChart.SeriesChartType.Column,
                Color = Color.FromArgb(59, 130, 246),
                BorderWidth = 0,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            });
            pnlLeft.Controls.Add(revenueChart);

            splitContainer.Controls.Add(pnlLeft);

            // RIGHT
            var pnlRight = new AntdUI.Panel
            {
                Size = new Size(410, 300),
                Back = Color.White,
                Radius = 12,
                Shadow = 10
            };
            var lblAlertHeader = new System.Windows.Forms.Label { Text = "Cảnh báo cần xử lý", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), Location = new Point(20, 15), AutoSize = true };
            pnlRight.Controls.Add(lblAlertHeader);

            // Tab buttons
            btnAlertDebt = CreateAlertTabButton("Nợ hóa đơn", AlertCategory.Debt, new Point(20, 50));
            btnAlertResidence = CreateAlertTabButton("Hết tạm trú", AlertCategory.Residence, new Point(140, 50));
            btnAlertOther = CreateAlertTabButton("Khác", AlertCategory.Other, new Point(270, 50));
            pnlRight.Controls.AddRange(new Control[] { btnAlertDebt, btnAlertResidence, btnAlertOther });
            HighlightAlertTabs();

            btnRemindDebt = new System.Windows.Forms.Button
            {
                Text = "Nhắc nợ nhanh",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(220, 38, 38),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(265, 20),
                Size = new Size(110, 25)
            };
            btnRemindDebt.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnRemindDebt.FlatAppearance.BorderSize = 1;
            btnRemindDebt.Click += (s, e) => ShowRemindDebtDialog();
            pnlRight.Controls.Add(btnRemindDebt);

            var div = new System.Windows.Forms.Panel { Size = new Size(370, 1), BackColor = Color.FromArgb(240, 240, 240), Location = new Point(20, 80) };
            pnlRight.Controls.Add(div);

            pnlAlerts = new FlowLayoutPanel
            {
                Location = new Point(10, 90),
                Size = new Size(390, 200),
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

        private System.Windows.Forms.Button CreateActionButton(string text, Color bg, EventHandler click)
        {
            var btn = new System.Windows.Forms.Button();
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Size = new Size(130, 34);
            btn.Margin = new Padding(5, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.Click += click;
            
            // Paint for Radius
            btn.Paint += (s, e) => {
                 using (GraphicsPath path = GetRoundedPath(btn.ClientRectangle, 8))
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
                LoadRevenueTrend();
            }
            catch { }
        }

        private void LoadAlerts()
        {
            pnlAlerts.Controls.Clear();
            debtAlerts.Clear();
            residenceAlerts.Clear();
            otherAlerts.Clear();
            try
            {
                var overdue = DatabaseHelper.ExecuteQuery($@"SELECT r.RoomName, cu.FullName, i.TotalAmount FROM Invoices i JOIN Contracts c ON i.ContractId=c.ContractId JOIN Rooms r ON c.RoomId=r.RoomId JOIN Customers cu ON c.CustomerId=cu.CustomerId WHERE i.Status='ChuaThu' AND (i.Year < {DateTime.Now.Year} OR (i.Year={DateTime.Now.Year} AND i.Month < {DateTime.Now.Month}))");
                foreach (DataRow r in overdue.Rows)
                {
                    debtAlerts.Add(new AlertInfo
                    {
                        Title = $"Nợ: {r["RoomName"]}",
                        Subtitle = $"{r["FullName"]} - {Convert.ToDecimal(r["TotalAmount"]):N0}đ",
                        Color = Color.FromArgb(239, 68, 68),
                        Urgent = true,
                        Category = AlertCategory.Debt
                    });
                }

                var expiring = DatabaseHelper.ExecuteQuery("SELECT cu.FullName, r.RoomName, tr.ExpiryDate FROM TemporaryResidence tr JOIN Customers cu ON tr.CustomerId=cu.CustomerId JOIN Rooms r ON tr.RoomId=r.RoomId WHERE tr.ExpiryDate <= DATEADD(DAY, 30, GETDATE()) AND tr.Status='DaDangKy'");
                foreach (DataRow r in expiring.Rows)
                {
                    int days = (Convert.ToDateTime(r["ExpiryDate"]) - DateTime.Now).Days;
                    residenceAlerts.Add(new AlertInfo
                    {
                        Title = days < 0 ? $"Hết hạn: {r["RoomName"]}" : $"Sắp hết: {r["RoomName"]}",
                        Subtitle = $"{r["FullName"]} ({days} ngày)",
                        Color = days < 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(249, 115, 22),
                        Urgent = days < 0,
                        Category = AlertCategory.Residence
                    });
                }

                var emptyRooms = DatabaseHelper.ExecuteQuery("SELECT RoomName FROM Rooms WHERE Status = 'Trong'");
                if (emptyRooms.Rows.Count > 5)
                {
                    otherAlerts.Add(new AlertInfo
                    {
                        Title = "Phòng trống nhiều",
                        Subtitle = $"Hiện có {emptyRooms.Rows.Count} phòng trống",
                        Color = Color.FromArgb(20, 184, 166),
                        Urgent = false,
                        Category = AlertCategory.Other
                    });
                }

                RenderAlerts();
            }
            catch { }
        }

        private void RenderAlerts()
        {
            pnlAlerts.Controls.Clear();
            List<AlertInfo> selectedList;
            if (currentAlertCategory == AlertCategory.Residence)
            {
                selectedList = residenceAlerts;
            }
            else if (currentAlertCategory == AlertCategory.Other)
            {
                selectedList = otherAlerts;
            }
            else
            {
                selectedList = debtAlerts;
            }

            if (selectedList.Count == 0)
            {
                pnlAlerts.Controls.Add(new System.Windows.Forms.Label { Text = "Không có cảnh báo nào!", ForeColor = Color.Gray, AutoSize = true, Padding = new Padding(10) });
                return;
            }

            foreach (var alert in selectedList)
            {
                AddAlertItem(alert.Title, alert.Subtitle, alert.Urgent, alert.Color);
            }
        }

        private System.Windows.Forms.Button CreateAlertTabButton(string text, AlertCategory category, Point location)
        {
            var btn = new System.Windows.Forms.Button
            {
                Text = text,
                Location = location,
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += (s, e) =>
            {
                currentAlertCategory = category;
                HighlightAlertTabs();
                RenderAlerts();
            };
            return btn;
        }

        private void HighlightAlertTabs()
        {
            void Apply(System.Windows.Forms.Button btn, bool active)
            {
                btn.BackColor = active ? Color.FromArgb(59, 130, 246) : Color.White;
                btn.ForeColor = active ? Color.White : Color.FromArgb(71, 85, 105);
            }

            Apply(btnAlertDebt, currentAlertCategory == AlertCategory.Debt);
            Apply(btnAlertResidence, currentAlertCategory == AlertCategory.Residence);
            Apply(btnAlertOther, currentAlertCategory == AlertCategory.Other);
        }

        private void AddAlertItem(string title, string sub, bool urgent, Color? overrideColor = null)
        {
            var item = new System.Windows.Forms.Panel
            {
                Size = new Size(360, 60),
                Margin = new Padding(5, 0, 0, 10),
                BackColor = Color.FromArgb(249, 250, 251)
            };

            var stripe = new System.Windows.Forms.Panel { Dock = DockStyle.Left, Width = 4, BackColor = overrideColor ?? (urgent ? Color.Red : Color.Orange) };
            item.Controls.Add(stripe);

            var lblT = new System.Windows.Forms.Label { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 81), Location = new Point(15, 10), AutoSize = true };
            var lblS = new System.Windows.Forms.Label { Text = sub, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, Location = new Point(15, 30), AutoSize = true };

            item.Controls.Add(lblT);
            item.Controls.Add(lblS);

            pnlAlerts.Controls.Add(item);
        }

        private void GenerateCurrentMonthInvoices()
        {
            try
            {
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                DatabaseHelper.ExecuteNonQuery(@"EXEC usp_GenerateInvoicesForMonth @m, @y",
                    new System.Data.SqlClient.SqlParameter("@m", month),
                    new System.Data.SqlClient.SqlParameter("@y", year));

                AntdUI.Message.success(this.FindForm(), "Đã tạo hóa đơn cho tháng hiện tại.");
                LoadStats();
                LoadAlerts();
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this.FindForm(), "Không tạo được hóa đơn: " + ex.Message);
            }
        }

        private void ShowRemindDebtDialog()
        {
            if (debtAlerts.Count == 0)
            {
                AntdUI.Message.info(this.FindForm(), "Không có phòng nợ cần nhắc.");
                return;
            }

            var form = new Form
            {
                Text = "Danh sách cần nhắc nợ",
                Size = new Size(420, 320),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            foreach (var alert in debtAlerts)
            {
                list.Items.Add($"{alert.Title} - {alert.Subtitle}");
            }

            form.Controls.Add(list);
            form.ShowDialog(this.FindForm());
        }

        private void LoadRevenueTrend()
        {
            try
            {
                var series = revenueChart.Series["RevenueSeries"];
                series.Points.Clear();

                for (int i = 5; i >= 0; i--)
                {
                    var date = DateTime.Now.AddMonths(-i);
                    var valObj = DatabaseHelper.ExecuteScalar("SELECT ISNULL(SUM(TotalAmount),0) FROM Invoices WHERE Month = @m AND Year = @y AND Status='DaThu'",
                        new System.Data.SqlClient.SqlParameter("@m", date.Month),
                        new System.Data.SqlClient.SqlParameter("@y", date.Year));
                    decimal val = Convert.ToDecimal(valObj ?? 0);
                    series.Points.AddXY($"{date.Month}/{date.Year % 100}", val);
                }
            }
            catch { }
        }

        private enum AlertCategory
        {
            Debt,
            Residence,
            Other
        }

        private class AlertInfo
        {
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public Color Color { get; set; }
            public bool Urgent { get; set; }
            public AlertCategory Category { get; set; }
        }
    }
}
