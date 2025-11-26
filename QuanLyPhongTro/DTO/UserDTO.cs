namespace QuanLyPhongTro.DTO
{
    public class UserDTO
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        // Không lưu PasswordHash vào DTO để tránh lộ khi truyền data lên UI
    }
}