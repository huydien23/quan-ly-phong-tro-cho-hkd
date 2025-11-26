using System;
using System.Drawing;
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
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập hệ thống";
            this.BackColor = AppColors.Blue50;

            InitUI();
        }

        private void InitUI()
        {
            // Main container
            var mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ColumnCount = 1;
            mainPanel.RowCount = 4;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100)); // Logo
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Username
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));  // Password
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));  // Button
            mainPanel.Padding = new Padding(50, 40, 50, 20);

            // Logo
            var lblLogo = new AntdUI.Label();
            lblLogo.Text = "QUẢN LÝ NHÀ TRỌ";
            lblLogo.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            lblLogo.ForeColor = AppColors.Blue900;
            lblLogo.Dock = DockStyle.Fill;
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            mainPanel.Controls.Add(lblLogo, 0, 0);

            // Username
            txtUser = new AntdUI.Input();
            txtUser.PlaceholderText = "Tên đăng nhập";
            txtUser.Dock = DockStyle.Fill;
            txtUser.Text = "admin"; // Default
            mainPanel.Controls.Add(txtUser, 0, 1);

            // Password
            txtPass = new AntdUI.Input();
            txtPass.PlaceholderText = "Mật khẩu";
            txtPass.UseSystemPasswordChar = true;
            txtPass.Dock = DockStyle.Fill;
            txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };
            mainPanel.Controls.Add(txtPass, 0, 2);

            // Button Login
            var btnLogin = new AntdUI.Button();
            btnLogin.Text = "ĐĂNG NHẬP";
            btnLogin.Type = TTypeMini.Primary;
            btnLogin.BackColor = AppColors.Blue600;
            btnLogin.Size = new Size(200, 42);
            btnLogin.Anchor = AnchorStyles.None;
            btnLogin.Click += (s, e) => DoLogin();
            mainPanel.Controls.Add(btnLogin, 0, 3);

            // Hint
            var lblHint = new AntdUI.Label();
            lblHint.Text = "Mật khẩu mặc định: 123456";
            lblHint.Font = new Font("Segoe UI", 9);
            lblHint.ForeColor = Color.Gray;
            lblHint.Location = new Point(50, 380);
            lblHint.Size = new Size(300, 20);
            this.Controls.Add(lblHint);

            this.Controls.Add(mainPanel);
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