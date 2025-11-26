using System;

namespace QuanLyPhongTro.DTO
{
    public class ContractDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Nullable nếu chưa xác định ngày đi
        public decimal Deposit { get; set; }   // Tiền cọc
        public bool IsActive { get; set; }     // Còn hiệu lực không

        // Trường hiển thị (Display Properties)
        public string RoomName { get; set; }
        public string CustomerName { get; set; }
    }
}