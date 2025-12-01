using System.Security.Cryptography;
using System.Text;
using QuanLyPhongTro.DAL;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.BLL
{
    public class AuthBLL
    {
        private readonly AuthDAL _authDAL;

        public AuthBLL()
        {
            _authDAL = new AuthDAL();
        }

        /// <summary>
        /// Đăng nhập và trả về UserDTO nếu thành công
        /// </summary>
        public UserDTO Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            string passwordHash = ComputeSha256Hash(password);
            return _authDAL.GetUserByCredentials(username, passwordHash);
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public OperationResult ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return OperationResult.Fail("Mật khẩu mới không được để trống");

            if (newPassword.Length < 6)
                return OperationResult.Fail("Mật khẩu mới phải có ít nhất 6 ký tự");

            // Verify old password by checking if user exists with this password
            var user = _authDAL.GetUserByCredentials(
                CurrentUser.Username, 
                ComputeSha256Hash(oldPassword));

            if (user == null)
                return OperationResult.Fail("Mật khẩu cũ không đúng");

            string newHash = ComputeSha256Hash(newPassword);
            bool success = _authDAL.UpdatePassword(userId, newHash);

            return success 
                ? OperationResult.Success("Đổi mật khẩu thành công") 
                : OperationResult.Fail("Có lỗi xảy ra khi đổi mật khẩu");
        }

        /// <summary>
        /// Hash password với SHA256
        /// </summary>
        public static string ComputeSha256Hash(string rawData)
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

    /// <summary>
    /// Kết quả thao tác nghiệp vụ
    /// </summary>
    public class OperationResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }

        private OperationResult(bool success, string message)
        {
            IsSuccess = success;
            Message = message;
        }

        public static OperationResult Success(string message = "Thành công") 
            => new OperationResult(true, message);

        public static OperationResult Fail(string message) 
            => new OperationResult(false, message);
    }

    /// <summary>
    /// Static class lưu thông tin user đang đăng nhập
    /// </summary>
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Username { get; set; }
        public static string FullName { get; set; }
        public static string Role { get; set; }

        public static bool IsLoggedIn => Id > 0;

        public static void SetUser(UserDTO user)
        {
            if (user != null)
            {
                Id = user.Id;
                Username = user.Username;
                FullName = user.FullName;
                Role = user.Role;
            }
        }

        public static void Clear()
        {
            Id = 0;
            Username = null;
            FullName = null;
            Role = null;
        }
    }
}