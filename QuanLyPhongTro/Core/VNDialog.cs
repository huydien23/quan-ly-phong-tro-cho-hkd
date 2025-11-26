using System.Windows.Forms;

namespace QuanLyPhongTro.Core
{
    /// <summary>
    /// Dialog tiếng Việt thay thế AntdUI.Modal (mặc định tiếng Trung)
    /// </summary>
    public static class VNDialog
    {
        public static bool Confirm(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public static bool Confirm(string message)
        {
            return Confirm("Xác nhận", message);
        }

        public static void Info(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warn(string message)
        {
            MessageBox.Show(message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string message)
        {
            MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
