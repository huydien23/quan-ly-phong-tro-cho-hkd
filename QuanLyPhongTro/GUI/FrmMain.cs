using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;

namespace QuanLyPhongTro.GUI
{
    public class FrmMain : AntdUI.Window
    {
        private AntdUI.Panel panelContent;
        private System.Windows.Forms.Panel panelSidebar;
        private List<System.Windows.Forms.Panel> menuButtons = new List<System.Windows.Forms.Panel>();
        private System.Windows.Forms.Panel activeButton;

        public FrmMain()
        {
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Phần Mềm Quản Lý Phòng Trọ Cho Hộ Kinh Doanh v1.0";
            this.ControlBox = false;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            InitLayout();
        }

        private new void InitLayout()
        {
            // 0. Header Top - Nút điều khiển
            var panelHeader = new AntdUI.Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 35;
            panelHeader.BackColor = AppColors.Blue950;

            // Dùng Label để hiển thị - set BackColor = màu nền header
            var btnClose = new System.Windows.Forms.Label();
            btnClose.Text = "✕";
            btnClose.Size = new Size(45, 35);
            btnClose.Dock = DockStyle.Right;
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = AppColors.Blue950;
            btnClose.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnClose.TextAlign = ContentAlignment.MiddleCenter;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => Application.Exit();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.Red;
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = AppColors.Blue950;

            var btnMaximize = new System.Windows.Forms.Label();
            btnMaximize.Text = "□";
            btnMaximize.Size = new Size(45, 35);
            btnMaximize.Dock = DockStyle.Right;
            btnMaximize.ForeColor = Color.White;
            btnMaximize.BackColor = AppColors.Blue950;
            btnMaximize.Font = new Font("Segoe UI", 14);
            btnMaximize.TextAlign = ContentAlignment.MiddleCenter;
            btnMaximize.Cursor = Cursors.Hand;
            btnMaximize.Click += (s, e) => {
                if (this.WindowState == FormWindowState.Maximized)
                    this.WindowState = FormWindowState.Normal;
                else
                    this.WindowState = FormWindowState.Maximized;
            };
            btnMaximize.MouseEnter += (s, e) => btnMaximize.BackColor = AppColors.Blue600;
            btnMaximize.MouseLeave += (s, e) => btnMaximize.BackColor = AppColors.Blue950;

            var btnMinimize = new System.Windows.Forms.Label();
            btnMinimize.Text = "─";
            btnMinimize.Size = new Size(45, 35);
            btnMinimize.Dock = DockStyle.Right;
            btnMinimize.ForeColor = Color.White;
            btnMinimize.BackColor = AppColors.Blue950;
            btnMinimize.Font = new Font("Segoe UI", 14);
            btnMinimize.TextAlign = ContentAlignment.MiddleCenter;
            btnMinimize.Cursor = Cursors.Hand;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = AppColors.Blue600;
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = AppColors.Blue950;

            panelHeader.Controls.Add(btnClose);
            panelHeader.Controls.Add(btnMaximize);
            panelHeader.Controls.Add(btnMinimize);
            this.Controls.Add(panelHeader);

            // 1. Content Right - PHẢI ADD TRƯỚC để Fill đúng
            panelContent = new AntdUI.Panel();
            panelContent.Dock = DockStyle.Fill;
            panelContent.BackColor = AppColors.Blue50;
            panelContent.Padding = new Padding(20);
            this.Controls.Add(panelContent);

            // 2. Sidebar Left - BLUE WHITE THEME
            panelSidebar = new System.Windows.Forms.Panel();
            panelSidebar.Width = 250;
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.BackColor = Color.White;
            this.Controls.Add(panelSidebar);

            // Sidebar Layout
            var sidebarLayout = new FlowLayoutPanel();
            sidebarLayout.Dock = DockStyle.Fill;
            sidebarLayout.FlowDirection = FlowDirection.TopDown;
            sidebarLayout.WrapContents = false;
            sidebarLayout.AutoScroll = true;
            sidebarLayout.BackColor = Color.White;
            sidebarLayout.Padding = new Padding(0);

            // === LOGO SECTION - With rounded bottom ===
            var panelLogo = new System.Windows.Forms.Panel();
            panelLogo.Size = new Size(250, 60);
            panelLogo.BackColor = Color.White;
            panelLogo.Margin = new Padding(0);

            var logoInner = new System.Windows.Forms.Panel();
            logoInner.Size = new Size(230, 50);
            logoInner.Location = new Point(10, 8);
            logoInner.BackColor = AppColors.Blue600;
            logoInner.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRectangle(0, 0, 229, 49, 8))
                {
                    logoInner.Region = new Region(path);
                }
            };

            var lblTitle = new System.Windows.Forms.Label();
            lblTitle.Text = "Quản Lý Phòng Trọ";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.Size = new Size(210, 22);
            lblTitle.Location = new Point(12, 8);
            lblTitle.BackColor = Color.Transparent;

            var lblSubtitle = new System.Windows.Forms.Label();
            lblSubtitle.Text = "Cho Hộ Kinh Doanh v1.0";
            lblSubtitle.ForeColor = Color.FromArgb(180, 200, 255);
            lblSubtitle.Font = new Font("Segoe UI", 8);
            lblSubtitle.Size = new Size(210, 16);
            lblSubtitle.Location = new Point(12, 30);
            lblSubtitle.BackColor = Color.Transparent;

            logoInner.Controls.Add(lblTitle);
            logoInner.Controls.Add(lblSubtitle);
            panelLogo.Controls.Add(logoInner);
            sidebarLayout.Controls.Add(panelLogo);

            // === USER SECTION - Rounded ===
            var panelUserWrap = new System.Windows.Forms.Panel();
            panelUserWrap.Size = new Size(250, 55);
            panelUserWrap.BackColor = Color.White;
            panelUserWrap.Margin = new Padding(0, 0, 0, 8);

            var panelUser = new System.Windows.Forms.Panel();
            panelUser.Size = new Size(230, 45);
            panelUser.Location = new Point(10, 0);
            panelUser.BackColor = Color.FromArgb(240, 245, 255);
            panelUser.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRectangle(0, 0, 229, 44, 8))
                {
                    panelUser.Region = new Region(path);
                }
            };

            var lblUserName = new System.Windows.Forms.Label();
            lblUserName.Text = CurrentUser.FullName ?? "Admin";
            lblUserName.ForeColor = AppColors.Blue900;
            lblUserName.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblUserName.Size = new Size(200, 20);
            lblUserName.Location = new Point(12, 6);
            lblUserName.BackColor = Color.Transparent;

            var lblUserStatus = new System.Windows.Forms.Label();
            lblUserStatus.Text = "● Đang hoạt động";
            lblUserStatus.ForeColor = Color.FromArgb(34, 197, 94);
            lblUserStatus.Font = new Font("Segoe UI", 8);
            lblUserStatus.Size = new Size(200, 16);
            lblUserStatus.Location = new Point(12, 25);
            lblUserStatus.BackColor = Color.Transparent;

            panelUser.Controls.Add(lblUserName);
            panelUser.Controls.Add(lblUserStatus);
            panelUserWrap.Controls.Add(panelUser);
            sidebarLayout.Controls.Add(panelUserWrap);

            // === MENU GROUPED ===

            // Group 1: Tổng quan
            var btnDashboard = CreateMenuButton("Tổng Quan", () => SwitchPage(new UC_Dashboard()));
            sidebarLayout.Controls.Add(btnDashboard);
            menuButtons.Add(btnDashboard);

            // Group 2: Quản lý
            sidebarLayout.Controls.Add(CreateGroupLabel("QUẢN LÝ"));
            var btnRooms = CreateMenuButton("Sơ Đồ Phòng", () => SwitchPage(new UC_Rooms()));
            var btnCustomers = CreateMenuButton("Khách Thuê", () => SwitchPage(new UC_Customers()));
            var btnContracts = CreateMenuButton("Hợp Đồng", () => SwitchPage(new UC_Contracts()));
            sidebarLayout.Controls.Add(btnRooms);
            sidebarLayout.Controls.Add(btnCustomers);
            sidebarLayout.Controls.Add(btnContracts);
            menuButtons.AddRange(new[] { btnRooms, btnCustomers, btnContracts });

            // Group 3: Nghiệp vụ
            sidebarLayout.Controls.Add(CreateGroupLabel("NGHIỆP VỤ"));
            var btnServices = CreateMenuButton("Dịch Vụ", () => SwitchPage(new UC_Services()));
            var btnInvoices = CreateMenuButton("Hóa Đơn", () => SwitchPage(new UC_Invoices()));
            var btnResidence = CreateMenuButton("Tạm Trú", () => SwitchPage(new UC_Residence()));
            sidebarLayout.Controls.Add(btnServices);
            sidebarLayout.Controls.Add(btnInvoices);
            sidebarLayout.Controls.Add(btnResidence);
            menuButtons.AddRange(new[] { btnServices, btnInvoices, btnResidence });

            // Group 4: Hệ thống
            sidebarLayout.Controls.Add(CreateGroupLabel("HỆ THỐNG"));
            var btnSettings = CreateMenuButton("Cài Đặt", () => SwitchPage(new UC_Settings()));
            var btnLogout = CreateMenuButton("Đăng Xuất", () => {
                if (VNDialog.Confirm("Bạn có chắc muốn đăng xuất?"))
                    Application.Exit();
            }, true);
            sidebarLayout.Controls.Add(btnSettings);
            sidebarLayout.Controls.Add(btnLogout);
            menuButtons.Add(btnSettings);

            panelSidebar.Controls.Add(sidebarLayout);

            // Load Default
            SetActiveButton(btnDashboard);
            SwitchPage(new UC_Dashboard());
        }

        private System.Windows.Forms.Panel CreateMenuButton(string text, Action onClick, bool isDanger = false)
        {
            var btn = new System.Windows.Forms.Panel();
            btn.Size = new Size(250, 42);
            btn.BackColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(0, 1, 0, 1);
            btn.Tag = "inactive";

            // Active Indicator (left bar)
            var indicator = new System.Windows.Forms.Panel();
            indicator.Size = new Size(4, 42);
            indicator.Location = new Point(0, 0);
            indicator.BackColor = Color.Transparent;
            indicator.Tag = "indicator";

            // Text only - clean design
            var lblText = new System.Windows.Forms.Label();
            lblText.Text = text;
            lblText.Font = new Font("Segoe UI", 10);
            lblText.ForeColor = isDanger ? AppColors.Red : Color.FromArgb(60, 70, 90);
            lblText.Size = new Size(220, 42);
            lblText.Location = new Point(20, 0);
            lblText.TextAlign = ContentAlignment.MiddleLeft;
            lblText.BackColor = Color.Transparent;

            btn.Controls.Add(indicator);
            btn.Controls.Add(lblText);

            // Events - Blue White Theme
            Action<object, EventArgs> onEnter = (s, e) => {
                if (btn.Tag.ToString() != "active")
                {
                    btn.BackColor = Color.FromArgb(240, 245, 255);
                    foreach (Control c in btn.Controls) 
                        if (c is System.Windows.Forms.Label lbl && c.Tag?.ToString() != "indicator") 
                            lbl.BackColor = Color.FromArgb(240, 245, 255);
                }
            };
            Action<object, EventArgs> onLeave = (s, e) => {
                if (btn.Tag.ToString() != "active")
                {
                    btn.BackColor = Color.White;
                    foreach (Control c in btn.Controls) 
                        if (c is System.Windows.Forms.Label lbl && c.Tag?.ToString() != "indicator") 
                            lbl.BackColor = Color.Transparent;
                }
            };
            Action<object, EventArgs> onClickAction = (s, e) => {
                if (!isDanger) SetActiveButton(btn);
                onClick?.Invoke();
            };

            btn.MouseEnter += (s, e) => onEnter(s, e);
            btn.MouseLeave += (s, e) => onLeave(s, e);
            btn.Click += (s, e) => onClickAction(s, e);

            foreach (Control c in btn.Controls)
            {
                c.MouseEnter += (s, e) => onEnter(s, e);
                c.MouseLeave += (s, e) => onLeave(s, e);
                c.Click += (s, e) => onClickAction(s, e);
            }

            return btn;
        }

        private void SetActiveButton(System.Windows.Forms.Panel btn)
        {
            // Reset all - White background
            foreach (var b in menuButtons)
            {
                b.Tag = "inactive";
                b.BackColor = Color.White;
                foreach (Control c in b.Controls)
                {
                    if (c is System.Windows.Forms.Label lbl && c.Tag?.ToString() != "indicator")
                    {
                        lbl.BackColor = Color.Transparent;
                        lbl.ForeColor = Color.FromArgb(70, 80, 100);
                    }
                    if (c.Tag?.ToString() == "indicator")
                        c.BackColor = Color.Transparent;
                }
            }

            // Set active - Blue background
            activeButton = btn;
            btn.Tag = "active";
            btn.BackColor = AppColors.Blue600;
            foreach (Control c in btn.Controls)
            {
                if (c is System.Windows.Forms.Label lbl && c.Tag?.ToString() != "indicator")
                {
                    lbl.BackColor = AppColors.Blue600;
                    lbl.ForeColor = Color.White;
                }
                if (c.Tag?.ToString() == "indicator")
                    c.BackColor = Color.White;
            }
        }

        private System.Windows.Forms.Label CreateGroupLabel(string text)
        {
            var lbl = new System.Windows.Forms.Label();
            lbl.Text = "  " + text;
            lbl.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(140, 150, 170);
            lbl.Size = new Size(250, 28);
            lbl.TextAlign = ContentAlignment.BottomLeft;
            lbl.Margin = new Padding(0, 8, 0, 0);
            return lbl;
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

        private void SwitchPage(UserControl uc)
        {
            panelContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelContent.Controls.Add(uc);
        }
    }
}