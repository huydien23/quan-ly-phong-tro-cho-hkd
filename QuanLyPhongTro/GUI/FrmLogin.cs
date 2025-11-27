using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using AntdUI;
using QuanLyPhongTro.Core;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.GUI
{
    public class FrmLogin : AntdUI.Window
    {
        private AntdUI.Input txtUser;
        private AntdUI.Input txtPass;

        public FrmLogin()
        {
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập - Quản Lý Phòng Trọ";

            InitUI();
        }

        private void InitUI()
        {
            // === LEFT PANEL - Branding ===
            var leftPanel = new System.Windows.Forms.Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 400;
            leftPanel.BackColor = AppColors.Blue600;
            leftPanel.Paint += (s, e) => {
                // Gradient background
                using (var brush = new LinearGradientBrush(leftPanel.ClientRectangle, 
                    AppColors.Blue600, AppColors.Blue900, LinearGradientMode.ForwardDiagonal))
                {
                    e.Graphics.FillRectangle(brush, leftPanel.ClientRectangle);
                }
            };

            // Brand content
            var lblBrandTitle = new System.Windows.Forms.Label();
            lblBrandTitle.Text = "Quản Lý\nPhòng Trọ";
            lblBrandTitle.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            lblBrandTitle.ForeColor = Color.White;
            lblBrandTitle.Location = new Point(40, 150);
            lblBrandTitle.Size = new Size(320, 120);
            lblBrandTitle.BackColor = Color.Transparent;
            leftPanel.Controls.Add(lblBrandTitle);

            var lblBrandSub = new System.Windows.Forms.Label();
            lblBrandSub.Text = "Cho Hộ Kinh Doanh v1.0";
            lblBrandSub.Font = new Font("Segoe UI", 12);
            lblBrandSub.ForeColor = Color.FromArgb(180, 200, 255);
            lblBrandSub.Location = new Point(40, 280);
            lblBrandSub.Size = new Size(320, 25);
            lblBrandSub.BackColor = Color.Transparent;
            leftPanel.Controls.Add(lblBrandSub);

            var lblFeatures = new System.Windows.Forms.Label();
            lblFeatures.Text = "✓ Quản lý phòng, khách thuê\n✓ Hợp đồng & hóa đơn\n✓ Dịch vụ điện nước\n✓ Báo cáo doanh thu";
            lblFeatures.Font = new Font("Segoe UI", 10);
            lblFeatures.ForeColor = Color.FromArgb(200, 220, 255);
            lblFeatures.Location = new Point(40, 330);
            lblFeatures.Size = new Size(320, 100);
            lblFeatures.BackColor = Color.Transparent;
            leftPanel.Controls.Add(lblFeatures);

            // === RIGHT PANEL - Login Form (add FIRST for proper docking) ===
            var rightPanel = new System.Windows.Forms.Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.FromArgb(248, 250, 252);
            this.Controls.Add(rightPanel);

            // Add left panel AFTER right panel
            this.Controls.Add(leftPanel);

            // Form container (white card) - centered
            var formCard = new System.Windows.Forms.Panel();
            formCard.Size = new Size(380, 400);
            formCard.BackColor = Color.White;
            
            // Center the form card when panel resizes
            rightPanel.Resize += (s, e) => {
                formCard.Location = new Point(
                    (rightPanel.Width - formCard.Width) / 2,
                    (rightPanel.Height - formCard.Height) / 2
                );
            };
            formCard.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Shadow effect
                using (var shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadowBrush, 3, 3, formCard.Width - 3, formCard.Height - 3);
                }
            };

            // Welcome text
            var lblWelcome = new System.Windows.Forms.Label();
            lblWelcome.Text = "Đăng nhập hệ thống";
            lblWelcome.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblWelcome.ForeColor = AppColors.Blue900;
            lblWelcome.BackColor = Color.Transparent;
            lblWelcome.Location = new Point(30, 25);
            lblWelcome.Size = new Size(320, 32);
            formCard.Controls.Add(lblWelcome);

            var lblSubWelcome = new System.Windows.Forms.Label();
            lblSubWelcome.Text = "Vui lòng nhập thông tin đăng nhập";
            lblSubWelcome.Font = new Font("Segoe UI", 9);
            lblSubWelcome.ForeColor = Color.FromArgb(120, 130, 140);
            lblSubWelcome.BackColor = Color.Transparent;
            lblSubWelcome.Location = new Point(30, 60);
            lblSubWelcome.Size = new Size(320, 20);
            formCard.Controls.Add(lblSubWelcome);

            // Username label
            var lblUser = new System.Windows.Forms.Label();
            lblUser.Text = "Tên đăng nhập";
            lblUser.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblUser.ForeColor = Color.FromArgb(70, 80, 90);
            lblUser.BackColor = Color.Transparent;
            lblUser.Location = new Point(30, 115);
            lblUser.Size = new Size(320, 20);
            formCard.Controls.Add(lblUser);

            // Username input
            txtUser = new AntdUI.Input();
            txtUser.PlaceholderText = "Nhập tên đăng nhập";
            txtUser.Location = new Point(30, 138);
            txtUser.Size = new Size(320, 42);
            txtUser.Text = "admin";
            formCard.Controls.Add(txtUser);

            // Password label
            var lblPass = new System.Windows.Forms.Label();
            lblPass.Text = "Mật khẩu";
            lblPass.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblPass.ForeColor = Color.FromArgb(70, 80, 90);
            lblPass.BackColor = Color.Transparent;
            lblPass.Location = new Point(30, 195);
            lblPass.Size = new Size(320, 18);
            formCard.Controls.Add(lblPass);

            // Password input
            txtPass = new AntdUI.Input();
            txtPass.PlaceholderText = "Nhập mật khẩu";
            txtPass.UseSystemPasswordChar = true;
            txtPass.Location = new Point(30, 218);
            txtPass.Size = new Size(320, 42);
            txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };
            formCard.Controls.Add(txtPass);

            // Login button
            var btnLogin = new AntdUI.Button();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Type = TTypeMini.Primary;
            btnLogin.BackColor = AppColors.Blue600;
            btnLogin.Location = new Point(30, 285);
            btnLogin.Size = new Size(320, 45);
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.Click += (s, e) => DoLogin();
            formCard.Controls.Add(btnLogin);

            // Hint
            var lblHint = new System.Windows.Forms.Label();
            lblHint.Text = "Mật khẩu mặc định: 123456";
            lblHint.Font = new Font("Segoe UI", 9);
            lblHint.ForeColor = Color.FromArgb(150, 150, 150);
            lblHint.BackColor = Color.Transparent;
            lblHint.Location = new Point(30, 345);
            lblHint.Size = new Size(320, 20);
            lblHint.TextAlign = ContentAlignment.MiddleCenter;
            formCard.Controls.Add(lblHint);

            rightPanel.Controls.Add(formCard);

            // Set initial position after form loads
            this.Load += (s, e) => {
                formCard.Location = new Point(
                    (rightPanel.Width - formCard.Width) / 2,
                    (rightPanel.Height - formCard.Height) / 2
                );
            };
        }

        private void DoLogin()
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                AntdUI.Message.warn(this, "Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                // Hash password với SHA256
                string hashedPassword = ComputeSha256Hash(password);

                // Kiểm tra trong database
                var result = DatabaseHelper.ExecuteScalar(
                    "SELECT FullName FROM Users WHERE Username = @user AND PasswordHash = @pass",
                    new System.Data.SqlClient.SqlParameter("@user", username),
                    new System.Data.SqlClient.SqlParameter("@pass", hashedPassword));

                if (result != null)
                {
                    string fullName = result.ToString();
                    AntdUI.Message.success(this, $"Xin chào, {fullName}!");
                    
                    // Lưu thông tin user hiện tại
                    CurrentUser.Username = username;
                    CurrentUser.FullName = fullName;

                    new FrmMain().Show();
                    this.Hide();
                }
                else
                {
                    AntdUI.Message.error(this, "Sai tài khoản hoặc mật khẩu!");
                    txtPass.Focus();
                }
            }
            catch (Exception ex)
            {
                AntdUI.Message.error(this, "Lỗi kết nối: " + ex.Message);
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    // Static class lưu thông tin user hiện tại
    public static class CurrentUser
    {
        public static string Username { get; set; }
        public static string FullName { get; set; }
    }
}