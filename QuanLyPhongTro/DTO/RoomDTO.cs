namespace QuanLyPhongTro.DTO
{
    public class RoomDTO
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public int RoomTypeId { get; set; }
        public string Status { get; set; } // 'Trong', 'DangThue'

        // Các trường mở rộng (lấy từ bảng RoomTypes khi JOIN) để hiển thị lên Grid cho tiện
        public string TypeName { get; set; }
        public decimal Price { get; set; }
        public double Area { get; set; }
    }
}