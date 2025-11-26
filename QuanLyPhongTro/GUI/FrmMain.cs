using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;

namespace QuanLyPhongTro.GUI
{
    public class FrmMain : AntdUI.Window
    {
        private AntdUI.Panel panelContent;
        private AntdUI.Panel panelSidebar;
        private List<System.Windows.Forms.Label> menuButtons = new List<System.Windows.Forms.Label>();
        private System.Windows.Forms.Label activeButton;

        public FrmMain()
        {
            this.Size = new Size(1366, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Phần Mềm Quản Lý Phòng Trọ";
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

            // 2. Sidebar Left
            panelSidebar = new AntdUI.Panel();
            panelSidebar.Width = 220;
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.BackColor = AppColors.Blue900;
            this.Controls.Add(panelSidebar);

            // Sidebar dùng FlowLayoutPanel để sắp xếp từ trên xuống
            var sidebarLayout = new FlowLayoutPanel();
            sidebarLayout.Dock = DockStyle.Fill;
            sidebarLayout.FlowDirection = FlowDirection.TopDown;
            sidebarLayout.WrapContents = false;
            sidebarLayout.AutoScroll = true;
            sidebarLayout.BackColor = AppColors.Blue900;
            sidebarLayout.Padding = new Padding(0);

            // Logo/Title Panel
            var panelLogo = new AntdUI.Panel();
            panelLogo.Size = new Size(220, 70);
            panelLogo.BackColor = AppColors.Blue950;

            var lblTitle = new AntdUI.Label();
            lblTitle.Text = "QUẢN LÝ PHÒNG TRỌ";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Size = new Size(220, 25);
            lblTitle.Location = new Point(0, 12);

            var lblSubtitle = new AntdUI.Label();
            lblSubtitle.Text = "Cho Hộ Kinh Doanh v1.0";
            lblSubtitle.ForeColor = Color.LightGray;
            lblSubtitle.Font = new Font("Segoe UI", 9);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.Size = new Size(220, 20);
            lblSubtitle.Location = new Point(0, 38);

            panelLogo.Controls.Add(lblTitle);
            panelLogo.Controls.Add(lblSubtitle);
            sidebarLayout.Controls.Add(panelLogo);

            // User Info Panel
            var panelUser = new AntdUI.Panel();
            panelUser.Size = new Size(220, 80);
            panelUser.BackColor = AppColors.Blue900;

            var avatar = new AntdUI.Avatar();
            avatar.Text = CurrentUser.FullName?.Substring(0, 1).ToUpper() ?? "A";
            avatar.BackColor = AppColors.Blue600;
            avatar.Size = new Size(40, 40);
            avatar.Location = new Point(15, 20);

            var lblName = new AntdUI.Label();
            lblName.Text = CurrentUser.FullName ?? "Admin";
            lblName.ForeColor = Color.White;
            lblName.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblName.Size = new Size(140, 22);
            lblName.Location = new Point(65, 18);

            var lblRole = new AntdUI.Label();
            lblRole.Text = CurrentUser.Username ?? "admin";
            lblRole.ForeColor = Color.LightGray;
            lblRole.Font = new Font("Segoe UI", 9);
            lblRole.Size = new Size(140, 18);
            lblRole.Location = new Point(65, 42);

            panelUser.Controls.Add(avatar);
            panelUser.Controls.Add(lblName);
            panelUser.Controls.Add(lblRole);
            sidebarLayout.Controls.Add(panelUser);

            // Menu Items - thêm theo thứ tự từ trên xuống
            var btnDashboard = CreateMenuButton("Tổng Quan", () => SwitchPage(new UC_Dashboard()));
            var btnRooms = CreateMenuButton("Sơ Đồ Phòng", () => SwitchPage(new UC_Rooms()));
            var btnCustomers = CreateMenuButton("Khách Thuê", () => SwitchPage(new UC_Customers()));
            var btnContracts = CreateMenuButton("Hợp Đồng", () => SwitchPage(new UC_Contracts()));
            var btnServices = CreateMenuButton("Dịch Vụ", () => SwitchPage(new UC_Services()));
            var btnInvoices = CreateMenuButton("Hóa Đơn", () => SwitchPage(new UC_Invoices()));
            var btnResidence = CreateMenuButton("Tạm Trú", () => SwitchPage(new UC_Residence()));
            var btnSettings = CreateMenuButton("Cài Đặt", () => SwitchPage(new UC_Settings()));
            var btnLogout = CreateMenuButton("Đăng Xuất", () => {
                if (VNDialog.Confirm("Bạn có chắc muốn đăng xuất?"))
                    Application.Exit();
            }, true);

            menuButtons.AddRange(new[] { btnDashboard, btnRooms, btnCustomers, btnContracts, btnServices, btnInvoices, btnResidence, btnSettings });
            
            sidebarLayout.Controls.Add(btnDashboard);
            sidebarLayout.Controls.Add(btnRooms);
            sidebarLayout.Controls.Add(btnCustomers);
            sidebarLayout.Controls.Add(btnContracts);
            sidebarLayout.Controls.Add(btnServices);
            sidebarLayout.Controls.Add(btnInvoices);
            sidebarLayout.Controls.Add(btnResidence);
            sidebarLayout.Controls.Add(btnSettings);
            sidebarLayout.Controls.Add(btnLogout);

            panelSidebar.Controls.Add(sidebarLayout);

            // Load Default - set active button
            SetActiveButton(btnDashboard);
            SwitchPage(new UC_Dashboard());
        }

        private System.Windows.Forms.Label CreateMenuButton(string text, Action onClick, bool isDanger = false)
        {
            var btn = new System.Windows.Forms.Label();
            btn.Text = text;
            btn.Size = new Size(220, 42);
            btn.ForeColor = isDanger ? AppColors.Red : Color.White;
            btn.BackColor = AppColors.Blue900;
            btn.Font = new Font("Segoe UI", 10);
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(0, 2, 0, 2);
            btn.Click += (s, e) => {
                if (!isDanger) SetActiveButton(btn);
                onClick?.Invoke();
            };
            btn.MouseEnter += (s, e) => { if (btn != activeButton) btn.BackColor = AppColors.Blue700; };
            btn.MouseLeave += (s, e) => { if (btn != activeButton) btn.BackColor = AppColors.Blue900; };
            return btn;
        }

        private void SetActiveButton(System.Windows.Forms.Label btn)
        {
            // Reset all buttons
            foreach (var b in menuButtons)
            {
                b.BackColor = AppColors.Blue900;
                b.ForeColor = Color.White;
            }
            // Highlight active
            activeButton = btn;
            btn.BackColor = AppColors.Blue600;
            btn.ForeColor = Color.White;
        }

        private void SwitchPage(UserControl uc)
        {
            panelContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelContent.Controls.Add(uc);
        }
    }
}