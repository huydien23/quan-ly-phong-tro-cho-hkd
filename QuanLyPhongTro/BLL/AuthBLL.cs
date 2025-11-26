using System.Security.Cryptography;
using System.Text;
using System.Data;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.BLL
{
    public class AuthBLL
    {
        // Hàm kiểm tra đăng nhập
        public bool Login(string username, string password, out string role, out string fullName)
        {
            role = "";
            fullName = "";

            // 1. Mã hóa password người dùng nhập (SHA256)
            string passHash = ComputeSha256Hash(password);

            // 2. Gọi DAL kiểm tra
            string sql = $"SELECT * FROM Users WHERE Username = '{username}' AND PasswordHash = '{passHash}'";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);

            if (dt.Rows.Count > 0)
            {
                role = dt.Rows[0]["Role"].ToString();
                fullName = dt.Rows[0]["FullName"].ToString();
                return true;
            }
            return false;
        }

        // Hàm băm mật khẩu (Chỉ dùng nội bộ class này)
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}